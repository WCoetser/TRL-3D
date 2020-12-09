using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.SampleApp
{
    public class SceneLoader : ISceneLoader
    {
        private readonly ILogger<SceneLoader> _logger;

        public SceneLoader(ILogger<SceneLoader> logger)
        {
            _logger = logger;
            _logger.LogInformation("SceneLoader created");
        }

        public IEnumerable<IAssertion> LoadInitialScene()
        {
            return new IAssertion[]
            {
                new ClearColor(0.1f, 0.1f, 0.2f)
            };
        }
    }
}
