using System;

namespace Trl_3D.Core.Abstractions
{
    public interface IAssertion : IDisposable    
    {
        /// <summary>
        /// When to perform the render step.
        /// </summary>
        RenderProcessStep ProcessStep { get; }
        
        /// <summary>
        /// Delete this assersion immediately, ie. it renders and then it disposes.
        /// </summary>
        bool SelfDestruct { get; }

        /// <summary>
        /// Builds buffers etc.
        /// </summary>
        void SetState();

        /// <summary>
        /// Shows state from <see cref="SetState"/>
        /// </summary>
        /// <param name="renderInfo"></param>
        void Render(RenderInfo renderInfo);
    }
}
