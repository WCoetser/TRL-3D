using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.RenderCommands;

namespace Trl_3D.OpenTk
{
    public class RenderCommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private Dictionary<string, Type> _assertionsToRenderCommandMappings;

        public RenderCommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _assertionsToRenderCommandMappings = new Dictionary<string, Type>();
            _logger = serviceProvider.GetService<ILogger<RenderCommandFactory>>();
        }

        public void MapRenderCommandsToAssertions()
        {
            foreach (var command in _serviceProvider.GetServices<IRenderCommand>())
            {
                var commandType = command.GetType();
                var assertionTypeName = command.AssociatedAssertionType.AssemblyQualifiedName;

                if (_assertionsToRenderCommandMappings.ContainsKey(assertionTypeName))
                {
                    _logger.LogError($"Render command for {assertionTypeName} registered more than once.");
                }

                // Save mapping
                _assertionsToRenderCommandMappings.Add(assertionTypeName, commandType);
            }
        }

        public IRenderCommand CreateRenderCommandForAssertion(IAssertion assertion)
        {
            // Load assertion mappings on first call
            if (!_assertionsToRenderCommandMappings.Any())
            {
                MapRenderCommandsToAssertions();
            }

            var assertionTypeTarget = assertion.GetType().AssemblyQualifiedName;
            if (!_assertionsToRenderCommandMappings.TryGetValue(assertionTypeTarget, out var renderCommandType))
            {
                throw new Exception($"No render command mapping was made for assertion type {assertionTypeTarget}, use {nameof(MapRenderCommandsToAssertions)}");
            }
            return (IRenderCommand)_serviceProvider.GetRequiredService(renderCommandType);
        }
    }
}
