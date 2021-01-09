using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk.RenderCommands
{
    /// <summary>
    /// Command toggles a flag that executes an extra render pass to do picking
    /// </summary>
    public class RequestPickingInfo : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        public bool SelfDestruct => true;

        public int ScreenX { get; }
        public int ScreenY { get; }

        public RequestPickingInfo(int screenX, int screenY)
        {
            ScreenX = screenX;
            ScreenY = screenY;
        }

        public void Render(RenderInfo renderInfo)
        {
            // Main logic in render loop
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            return null;
        }

        public void SetState(RenderInfo renderInfo)
        {
        }
    }
}
