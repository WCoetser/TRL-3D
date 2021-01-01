using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;

namespace Trl_3D.SampleApp
{
    public class ImageLoader : IImageLoader
    {
        private readonly ILogger<ImageLoader> _logger;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public ImageLoader(ILogger<ImageLoader> logger, ICancellationTokenManager cancellationTokenManager)
        {
            _logger = logger;
            _cancellationTokenManager = cancellationTokenManager;
        }

        public async Task<ImageData> LoadImage(Uri uri)
        {
            try
            {
                if (uri.Scheme != Uri.UriSchemeFile)
                {
                    throw new Exception($"Expected image URI schema: {Uri.UriSchemeFile}");
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var imageRaw = await File.ReadAllBytesAsync(uri.LocalPath, _cancellationTokenManager.CancellationToken);                                                
                using var inputImage = Image.Load(imageRaw);
                byte[] outputBufferRgba = new byte[inputImage.Width * inputImage.Height * 4];
               
                for (int y = 0; y < inputImage.Height; y++)
                {
                    Rgba32[] inputRow = inputImage.GetPixelRowSpan(y).ToArray();
                    var y_inverted = (inputImage.Height - 1) - y;
                    for (int x = 0; x < inputImage.Width; x++)
                    {
                        // Buffer is loaded from top to bottom from left to right
                        var bufferAddress = (y_inverted * inputImage.Width + x) * 4;
                        outputBufferRgba[bufferAddress] = inputRow[x].R;
                        outputBufferRgba[bufferAddress + 1] = inputRow[x].G;
                        outputBufferRgba[bufferAddress + 2] = inputRow[x].B;
                        outputBufferRgba[bufferAddress + 3] = inputRow[x].A;
                    }
                }

                stopwatch.Stop();

                _logger.LogInformation($"Loaded {uri} ({inputImage.Width}x{inputImage.Height} pixels) in {stopwatch.ElapsedMilliseconds} ms");

                return new ImageData(outputBufferRgba, inputImage.Width, inputImage.Height);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load {uri.AbsoluteUri}: {ex.Message}");
                throw;
            }
        }
    }
}

