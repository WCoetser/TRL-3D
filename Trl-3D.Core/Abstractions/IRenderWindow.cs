using Microsoft.Extensions.Logging;

namespace Trl_3D.Core.Abstractions
{
    public interface IRenderWindow
    {
        /// <summary>
        /// Shows the window.
        /// </summary>
        void Run();

        /// <summary>
        /// Initializes the render window with the basic necesities to make it work.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sceneLoader"></param>
        void Initialize(ILogger logger, ISceneLoader sceneLoader);
    }
}
