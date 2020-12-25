using System;
using System.Threading.Channels;
using Trl_3D.Core.Scene;

namespace Trl_3D.Core.Abstractions
{
    public interface IRenderWindow
    {
        /// <summary>
        /// Channel for receiving updates to the current scene graph.
        /// </summary>
        Channel<SceneGraph> SceneGraphUpdatesChannel { get; }

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
