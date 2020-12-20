using Microsoft.Extensions.Logging;

using Trl_3D.Core.Assertions;
using Trl_3D.Core.Abstractions;

using System.Threading.Channels;
using System.Threading.Tasks;
using System.IO;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Threading;

namespace Trl_3D.SampleApp
{
    public class SceneLoader : IAssertionLoader
    {
        private readonly ILogger<SceneLoader> _logger;

        public SceneLoader(ILogger<SceneLoader> logger)
        {
            _logger = logger;
            _logger.LogInformation("SceneLoader created");
            AssertionUpdatesChannel = Channel.CreateUnbounded<IAssertion>();
        }

        public Channel<IAssertion> AssertionUpdatesChannel { get; private set; }

        public async Task StartAssertionProducer(CancellationToken cancellationToken)
        {
            await AssertionUpdatesChannel.Writer.WriteAsync(new ClearColor(0.1f, 0.1f, 0.2f), cancellationToken);
            await AssertionUpdatesChannel.Writer.WriteAsync(new RenderTestTriagle(), cancellationToken);
            await AssertionUpdatesChannel.Writer.WriteAsync(new GrabScreenshot { CaptureCallback = ProcessCapture }, cancellationToken);

            AssertionUpdatesChannel.Writer.Complete();

            _logger.LogInformation("Scene assertion producer stopped.");
        }

        private void ProcessCapture(byte[] buffer, int width, int height)
        {
            var filename = $"capture.png";

            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            using var image = Image.LoadPixelData<Rgb24>(buffer, width, height);

            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.SaveAsPng(fileInfo.FullName);

            _logger.LogInformation($"Captured to {fileInfo.FullName}");
        }
    }
}
