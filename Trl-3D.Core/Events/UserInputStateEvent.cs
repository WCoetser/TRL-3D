using OpenTK.Windowing.GraphicsLibraryFramework;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Events
{
    public class UserInputStateEvent : IEvent
    {
        public KeyboardState KeyboardState { get; }
        public MouseState MouseState { get; }
        public double TimeSinceLastEvent { get; }

        public UserInputStateEvent(KeyboardState keyboardState, MouseState mouseState, double timeSinceLastEvent)
        {
            KeyboardState = keyboardState;
            MouseState = mouseState;
            TimeSinceLastEvent = timeSinceLastEvent;
        }
    }
}
