namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Where it should be processed during the rendering step. This is reight before the screen update/swap buffers.
    /// </summary>
    public enum RenderProcessStep
    {
        /// <summary>
        /// Process this assertion before others, ex. set clear colour.
        /// </summary>
        Start,

        /// <summary>
        /// Normal draw operations go here, ex. render triangles.
        /// </summary>
        Middle,

        /// <summary>
        /// Process this assertion afrter others, ex. render to seperate frame buffer.
        /// </summary>
        End
    }
}