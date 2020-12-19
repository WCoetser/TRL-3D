using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;
using System.Linq;
using Trl_3D.OpenTk.RenderCommands;
using Microsoft.Extensions.DependencyInjection;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        // Render lists
        private readonly LinkedList<IRenderCommand> _renderList;
        private LinkedListNode<IRenderCommand> _renderListContentInsertionPoint;

        // Screen dimensions, frame rate etc.
        private readonly RenderInfo _renderInfo;
        private readonly RenderCommandFactory _renderCommandFactory;

        public OpenGLSceneProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _renderList = new LinkedList<IRenderCommand>();
            _renderInfo = new RenderInfo();
            _renderCommandFactory = _serviceProvider.GetRequiredService<RenderCommandFactory>();
        }

        internal void ResizeRenderWindow(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _renderInfo.Width = width;
            _renderInfo.Height = height;
            _logger.LogInformation($"Window resized to {width}x{height}={width*height} pixels");
        }

        public void SetState(IEnumerable<IAssertion> scene)
        {
            foreach (var command in scene)
            {
                try
                {
                    var renderCommand = _renderCommandFactory.CreateRenderCommandForAssertion(command);
                    _logger.LogInformation($"Generate state: {command.GetType().FullName} mapped to {renderCommand.GetType().FullName}");
                    InsertAssersionInRenderOrder(renderCommand);
                    renderCommand.SetState(command);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate state for {command.GetType().FullName}");
                }
            }
        }

        private void InsertAssersionInRenderOrder(IRenderCommand command)
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
