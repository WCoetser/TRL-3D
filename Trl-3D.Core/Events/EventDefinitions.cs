using OpenTK.Windowing.GraphicsLibraryFramework;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Events
{
    public record ScreenCaptureEvent(byte[] RgbBuffer, int Width, int Height) : IEvent;

    public record UserInputStateEvent(KeyboardState KeyboardState, MouseState MouseState, double TimeSinceLastEventSeconds) : IEvent;

    public record PickingFeedback(PickingInfo PickingInfo) : IEvent;
}
