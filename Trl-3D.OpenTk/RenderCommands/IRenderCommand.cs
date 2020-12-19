using System;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk.RenderCommands
{
    public interface IRenderCommand : IDisposable
    {
        /// <summary>
        /// The type of the assersion for this render command.
        /// </summary>
        Type AssociatedAssertionType { get; }

        /// <summary>
        /// When to perform the render step.
        /// </summary>
        RenderProcessPosition ProcessStep { get; }

        /// <summary>
        /// Delete this assersion immediately, ie. it renders and then it disposes.
        /// </summary>
        bool SelfDestruct { get; }

        /// <summary>
        /// Builds buffers etc. based on the parameters from the assertion/
        /// </summary>
        void SetState(IAssertion assertion);

        /// <summary>
        /// Shows state from <see cref="SetState"/>
        /// </summary>
        /// <param name="renderInfo"></param>
        void Render(RenderInfo renderInfo);
    }
}
