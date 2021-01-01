﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using Trl_3D.OpenTk.RenderCommands;
using Trl_3D.Core.Scene;
using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.Textures;
using Trl_3D.Core.Scene.Updates;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        private readonly IShaderCompiler _shaderCompiler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRenderWindow _renderWindow;
        private readonly ITextureLoader _textureLoader;
        private readonly ILogger _logger;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        // Render lists
        private readonly LinkedList<IRenderCommand> _renderList;
        private LinkedListNode<IRenderCommand> _renderListContentInsertionPoint;
        
        // Screen dimensions, frame rate etc.
        private readonly RenderInfo _renderInfo;
        private bool _windowSizeChanged;

        public OpenGLSceneProcessor(IServiceProvider serviceProvider, IRenderWindow renderWindow)
        {
            _serviceProvider = serviceProvider;
            _cancellationTokenManager = _serviceProvider.GetRequiredService<ICancellationTokenManager>();
            _logger = _serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _shaderCompiler = _serviceProvider.GetRequiredService<IShaderCompiler>();
            _textureLoader = _serviceProvider.GetRequiredService<ITextureLoader>();

            _renderWindow = renderWindow; // This cannot be passed via the service provider otherwise there will be a cycle in the DI graph
            _renderList = new LinkedList<IRenderCommand>();
            _renderInfo = new RenderInfo();
        }

        internal void ResizeRenderWindow(int width, int height)
        {
            _renderInfo.Width = width;
            _renderInfo.Height = height;
            _windowSizeChanged = true;
        }

        public void UpdateState(ISceneGraphUpdate sceneGraphUpdate)
        {
            if (sceneGraphUpdate is ViewMatrixUpdate viewMatrixUpdate)
            {
                _renderInfo.ViewMatrix = viewMatrixUpdate.NewViewMatrix;
            }
            else if (sceneGraphUpdate is ContentUpdate contentUpdate)
            {
                // TODO: Add differential rendering for sub-updates

                _renderList.Clear();

                InsertCommandInRenderOrder(new ClearColor(contentUpdate.SceneGraph.RgbClearColor));
                InsertCommandInRenderOrder(new RenderSceneGraph(_logger, _shaderCompiler, _textureLoader, contentUpdate.SceneGraph));
                InsertCommandInRenderOrder(new GrabScreenshot(_renderWindow, _cancellationTokenManager));

                foreach (var command in _renderList)
                {
                    command.SetState();
                }
            }
            else
            {
                throw new Exception($"Unknown scene graph update of type {sceneGraphUpdate.GetType().FullName} given.");
            }
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

            var currentNode = _renderList.First;
            while (currentNode != null)
            {
                var assertion = currentNode.Value;
                assertion.Render(_renderInfo);
                var next = currentNode.Next;
                if (assertion.SelfDestruct)
                {
                    _renderList.Remove(currentNode);
                    if (assertion is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }                    
                }
                currentNode = next;                
            }
        }

        public void ReleaseResources()
        {
            foreach (var assertion in _renderList)
            {
                if (assertion is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _logger.LogInformation("Render commands disposed");
        }
    }
}
