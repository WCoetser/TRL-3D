﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics.OpenGL4;

using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.Textures;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.RenderCommands;
using System.Threading.Tasks;
using Trl_3D.Core.Events;
using Trl_3D.Core.Scene;
using System.Threading;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        private readonly IShaderCompiler _shaderCompiler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRenderWindow _renderWindow;
        private readonly SceneGraph _sceneGraph;
        private readonly ITextureLoader _textureLoader;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenManager;

        // Render lists
        private readonly LinkedList<IRenderCommand> _renderList;
        private LinkedListNode<IRenderCommand> _renderListContentInsertionPoint;
        
        // Screen dimensions, frame rate etc.
        private readonly RenderInfo _renderInfo;
        private bool _windowSizeChanged;
        private RequestPickingInfo _requestPicking;

        public OpenGLSceneProcessor(IServiceProvider serviceProvider, IRenderWindow renderWindow, SceneGraph sceneGraph)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenManager = _serviceProvider.GetRequiredService<CancellationTokenSource>();
            _logger = _serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _shaderCompiler = _serviceProvider.GetRequiredService<IShaderCompiler>();
            _textureLoader = _serviceProvider.GetRequiredService<ITextureLoader>();

            _renderWindow = renderWindow; // This cannot be passed via the service provider otherwise there will be a cycle in the DI graph
            _sceneGraph = sceneGraph;
            _renderList = new LinkedList<IRenderCommand>();
            _renderInfo = new RenderInfo();
            _requestPicking = null;

            GL.Enable(EnableCap.DepthTest);
        }

        internal void ResizeRenderWindow(int width, int height)
        {
            _renderInfo.Width = width;
            _renderInfo.Height = height;
            _windowSizeChanged = true;
        }

        public void UpdateState(IRenderCommand renderCommand)
        {
            if (renderCommand is RequestPickingInfo requestPicking)
            {
                _requestPicking = requestPicking;
            }

            renderCommand.SetState(_renderInfo);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                throw new Exception($"Set state: {err}");
            }

            InsertCommandInRenderOrder(renderCommand);
        }

        private void InsertCommandInRenderOrder(IRenderCommand command)
        {
            if (!_renderList.Any())
            {
                _renderList.AddLast(command);
                _renderListContentInsertionPoint = _renderList.First;
            }
            else
            {
                if (command.ProcessStep == RenderProcessPosition.BeforeContent)
                {
                    _renderList.AddFirst(command);
                }
                else if (command.ProcessStep == RenderProcessPosition.AfterContent)
                {
                    _renderList.AddLast(command);
                }
                else if (command.ProcessStep == RenderProcessPosition.ContentRenderStep)
                {
                    _renderList.AddAfter(_renderListContentInsertionPoint, command);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Render(double timeSinceLastFrameSeconds)
        {
            _renderInfo.TotalRenderTime += timeSinceLastFrameSeconds;
            _renderInfo.FrameRate = 1.0 / timeSinceLastFrameSeconds;
            _renderInfo.CurrentViewMatrix = _sceneGraph.ViewMatrix;
            _renderInfo.CurrentProjectionMatrix = _sceneGraph.ProjectionMatrix;

            if (_windowSizeChanged)
            {
                GL.Viewport(0, 0, _renderInfo.Width, _renderInfo.Height);
                _windowSizeChanged = false;
                _logger.LogInformation($"Window resized to {_renderInfo.Width}x{_renderInfo.Height}={_renderInfo.Width * _renderInfo.Height} pixels");
            }

            if (!_renderList.Any())
            {
                return;
            }

            // Render content for picking
            LinkedListNode<IRenderCommand> currentNode = null;            
            if (_requestPicking != null)
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);

                currentNode = _renderList.First;
                PickingInfo currentPickingInfo = null;
                while (currentNode != null)
                {
                    var command = currentNode.Value;
                    var pickingInfo = command.RenderForPicking(_renderInfo, _requestPicking.ScreenX, _requestPicking.ScreenY);
                    if (pickingInfo != null)
                    {
                        currentPickingInfo = pickingInfo;
                    }
                    var next = currentNode.Next;
                    if (command.SelfDestruct)
                    {
                        _renderList.Remove(currentNode);
                        if (command is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    currentNode = next;
                }
                _requestPicking = null;

                if (currentPickingInfo != null)
                {
                    var writeTask = Task.Run(async () =>
                    {
                        await _renderWindow.EventChannel.Writer.WriteAsync(new PickingFeedback(currentPickingInfo),
                            _cancellationTokenManager.Token);
                    });
                    writeTask.Wait();
                }
            }

            // Render content for display
            GL.Clear(ClearBufferMask.DepthBufferBit);

            currentNode = _renderList.First;
            while (currentNode != null)
            {
                var command = currentNode.Value;
                command.Render(_renderInfo);
                var next = currentNode.Next;
                if (command.SelfDestruct)
                {
                    _renderList.Remove(currentNode);
                    if (command is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }                    
                }
                currentNode = next;                
            }
        }

        public void ReleaseResources()
        {
            foreach (var command in _renderList)
            {
                if (command is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _textureLoader.Dispose();

            _logger.LogInformation("Render commands disposed");
        }
    }
}
