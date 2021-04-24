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
        private int _mrtFrameBufferId;
        private int _mrtFrameBufferTextureId;
        private int _mrtFrameBufferDepthId;

        // Capture OpenGL log messages to .NET logger
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

            InitFrameBuffer(640, 360);
        }

        private void InitFrameBuffer(int width, int height)
        {
            // Generate extra frame buffer for main rendering, to be blitted to default frame buffer
            _mrtFrameBufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _mrtFrameBufferId);

            // Bind main texture that will eventually end up on the screen as first target
            _mrtFrameBufferTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _mrtFrameBufferTextureId);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _mrtFrameBufferTextureId, 0);

            // Bind depth buffer
            _mrtFrameBufferDepthId = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _mrtFrameBufferDepthId);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _mrtFrameBufferDepthId);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("MRT frame buffer is incomplete.");
            }
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
                    // Unknown severity
                    throw new Exception(message);
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
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _defaultFrameBufferId);
                GL.Viewport(0, 0, _renderInfo.Width, _renderInfo.Height);

                // Clean up existing frame buffer
                GL.DeleteFramebuffer(_mrtFrameBufferId);
                GL.DeleteTexture(_mrtFrameBufferTextureId);
                GL.DeleteRenderbuffer(_mrtFrameBufferDepthId);

                // Create new MRT frameobuffer with resized dimensions
                InitFrameBuffer(_renderInfo.Width, _renderInfo.Height);

                _windowSizeChanged = false;
                _logger.LogInformation($"Window resized to {_renderInfo.Width}x{_renderInfo.Height}={_renderInfo.Width * _renderInfo.Height} pixels");
            }

            if (!_renderList.Any())
            {
                return;
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _mrtFrameBufferId);

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

            // Blit MRT source to default frame buffer to display it
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _mrtFrameBufferId);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _defaultFrameBufferId);
            GL.BlitFramebuffer(0, 0, _renderInfo.Width - 1, _renderInfo.Height - 1,  // source
                0, 0, _renderInfo.Width - 1, _renderInfo.Height - 1, // destination
                ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest); 
        }

        public void ReleaseResources()
        {
            // Dispose render commands
            foreach (var command in _renderList)
            {
                if (command is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Dispose all loaded textures
            _textureLoader.Dispose();

            // Handle used by error handler callback
            if (_gcHandle != default)
            {
                _gcHandle.Free();
                _gcHandle = default;
            }

            // Dispose MRT frame buffer
            GL.DeleteFramebuffer(_mrtFrameBufferId);
            GL.DeleteTexture(_mrtFrameBufferTextureId);
            GL.DeleteRenderbuffer(_mrtFrameBufferDepthId);

            _logger.LogInformation("Render commands disposed");
        }
    }
}
