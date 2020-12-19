using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
                new ClearColor(0.1f, 0.1f, 0.2f),
                new RenderTestTriagle(),
                new GrabScreenshot
                {
                    CaptureCallback = ProcessCapture
                }
            };
        }

        private void ProcessCapture(byte[] buffer, RenderInfo renderInfo)
        {
            var filename = $"capture.png";

            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            using var image = Image.LoadPixelData<Rgb24>(buffer, renderInfo.Width, renderInfo.Height);
            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.SaveAsPng(fileInfo.FullName);

            _logger.LogInformation($"Captured to {fileInfo.FullName}");
        }
    }
}
