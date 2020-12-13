using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;
using System.Linq;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        private readonly ILogger _logger;

        private LinkedList<IAssertion> _assertionListRenderOrder;
        private LinkedListNode<IAssertion> _middleAssertionsInsertPoint;

        public OpenGLSceneProcessor(ILogger logger)
        {
            _logger = logger;
            _assertionListRenderOrder = new LinkedList<IAssertion>();
        }

        internal void ResizeRenderWindow(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _logger.LogInformation($"Window resized to {width}x{height}={width*height} pixels");
        }

        public void SetState(IEnumerable<IAssertion> scene)
        {
            foreach (var assertion in scene)
            {
                _logger.LogInformation($"Generate state: {assertion.GetType().FullName}");
                InsertAssersionInRenderOrder(assertion);
                try
                {
                    assertion.SetState();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate state.");
                }
            }
        }

        private void InsertAssersionInRenderOrder(IAssertion assertion)
        {
            if (!_assertionListRenderOrder.Any())
            {
                _assertionListRenderOrder.AddLast(assertion);
                _middleAssertionsInsertPoint = _assertionListRenderOrder.First;
            }
            else
            {
                if (assertion.ProcessStep == RenderProcessStep.Start)
                {
                    _assertionListRenderOrder.AddFirst(assertion);
                }
                else if (assertion.ProcessStep == RenderProcessStep.End)
                {
                    _assertionListRenderOrder.AddLast(assertion);
                }
                else if (assertion.ProcessStep == RenderProcessStep.Middle)
                {
                    _assertionListRenderOrder.AddAfter(_middleAssertionsInsertPoint, assertion);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Render(double time)
        {
            foreach (var assertion in _assertionListRenderOrder)
            {
                assertion.Render();
            }
        }

        public void ReleaseResources()
        {
            foreach (var assertion in _assertionListRenderOrder)
            {
                assertion.Dispose();
            }
        }
    }
}
