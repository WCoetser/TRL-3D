using System;
using System.Threading.Channels;

namespace Trl_3D.Core.Abstractions
{
    public interface IRenderWindow
    {
        public Channel<IRenderCommand> RenderCommandUpdatesChannel { get; }

        /// <summary>
        /// Channel for events going out of the 3D engine, ex. mouse, keyboard, screenshot
        /// </summary>
        Channel<IEvent> EventChannel { get; }

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Run();

        /// <summary>
        /// Initializes the render window with service provider for further dependency injection as needed.
        /// </summary>
        void Initialize(IServiceProvider serviceProvider);
        
        /// <summary>
        /// Closes the render window
        /// </summary>
        void Close();
    }
}
