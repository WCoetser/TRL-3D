using System.Collections.Generic;

namespace Trl_3D.Core.Abstractions
{
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads the initial scene
        /// </summary>
        IEnumerable<IAssertion> LoadInitialScene();
    }
}
