using Microsoft.Extensions.Logging;
using System;

namespace Trl_3D.Core.Abstractions
{
    public interface IRenderWindow
    {
        /// <summary>
        /// Shows the window.
        /// </summary>
        void Run();

        /// <summary>
        /// Initializes the render window with service provider for further dependency injection as needed.
        /// </summary>
        void Initialize(IServiceProvider serviceProvider);
    }
}
