namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Where it should be processed during the rendering step. This is reight before the screen update/swap buffers.
    /// </summary>
    public enum RenderProcessPosition
    {
        /// <summary>
        /// Process this assertion before others, ex. set clear colour/depth buffer
        /// </summary>
        BeforeContent,

        /// <summary>
        /// Normal draw operations, ex. render triangles.
        /// </summary>
        ContentRenderStep,

        /// <summary>
        /// Process this assertion after others, ex. capture screenshot
        /// </summary>
        AfterContent
    }
}
