namespace Trl_3D.Core.Abstractions
{
    public interface IRenderCommand
    {
        /// <summary>
        /// When to perform the render step.
        /// </summary>
        RenderProcessPosition ProcessStep { get; }

        /// <summary>
        /// Delete this assersion immediately, ie. it renders and then it disposes.
        /// </summary>
        bool SelfDestruct { get; }

        /// <summary>
        /// Generates buffers, prepare for rendering."/>
        /// </summary>
        void SetState(RenderInfo renderInfo);

        /// <summary>
        /// Shows state from <see cref="SetState"/>
        /// </summary>
        /// <param name="renderInfo"></param>
        void Render(RenderInfo renderInfo);
    }
}
