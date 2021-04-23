using Microsoft.Extensions.DependencyInjection;
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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        // Dependency injection
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

        // Frame buffer objects
        private int _defaultFrameBufferId;

        // Capture OpenGL log messages to .NET logger
        // Reference: https://gist.github.com/Vassalware/d47ff5e60580caf2cbbf0f31aa20af5d
        private GCHandle _gcHandle;
        private static DebugProc _debugProc;

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

            // Get human readable log messages during debugging
            if (Debugger.IsAttached)
            {
                GL.Enable(EnableCap.DebugOutput);
                _debugProc = DebugMessageCallback;
                _gcHandle = GCHandle.Alloc(_debugProc);
                GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
            }

            GL.Enable(EnableCap.DepthTest);

            // Save default frame buffer id for later blitting
            GL.GetInteger(GetPName.FramebufferBinding, out _defaultFrameBufferId);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _defaultFrameBufferId);
        }

        internal void DebugMessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr messageASCII, IntPtr _)
        {
            var message = Marshal.PtrToStringAnsi(messageASCII, length);
            message = $"{source} - {type} - {severity} - {message}";
            switch (severity)
            {
                case DebugSeverity.DontCare:
                case DebugSeverity.DebugSeverityNotification:
                    _logger.LogTrace(message);
                    break;
                case DebugSeverity.DebugSeverityLow:
                case DebugSeverity.DebugSeverityMedium:
                    _logger.LogWarning(message);
                    break;
                case DebugSeverity.DebugSeverityHigh:
                    _logger.LogError(message);
                    break;
                default:
                    throw new Exception("Unknown severity");
            }

            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            }
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

            if (_gcHandle != default)
            {
                _gcHandle.Free();
                _gcHandle = default;
            }

            _logger.LogInformation("Render commands disposed");
        }
    }
}
