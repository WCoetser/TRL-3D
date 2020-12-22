using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using Trl_3D.OpenTk.RenderCommands;
using Trl_3D.Core.Scene;
using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRenderWindow _renderWindow;

        // Render lists
        private readonly LinkedList<IRenderCommand> _renderList;
        private LinkedListNode<IRenderCommand> _renderListContentInsertionPoint;
        
        // Screen dimensions, frame rate etc.
        private readonly RenderInfo _renderInfo;

        public OpenGLSceneProcessor(IServiceProvider serviceProvider, IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _renderList = new LinkedList<IRenderCommand>();
            _renderInfo = new RenderInfo();
        }

        internal void ResizeRenderWindow(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _renderInfo.Width = width;
            _renderInfo.Height = height;
            _logger.LogInformation($"Window resized to {width}x{height}={width*height} pixels");
        }

        public void UpdateState(SceneGraph sceneGraph)
        {
            // TODO: Add differential rendering

            _renderList.Clear();
            
            InsertCommandInRenderOrder(new ClearColor(sceneGraph.RgbClearColor));
            InsertCommandInRenderOrder(new RenderTestTriagle(_logger));
            InsertCommandInRenderOrder(new GrabScreenshot(_renderWindow));

            foreach (var command in _renderList)
            {
                command.SetState();
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
                    assertion.Dispose();
                }
                currentNode = next;                
            }
        }

        public void ReleaseResources()
        {
            foreach (var assertion in _renderList)
            {
                assertion.Dispose();
            }
        }
    }
}
