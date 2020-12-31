using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;

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

                var imageRaw = await File.ReadAllBytesAsync(uri.LocalPath, _cancellationTokenManager.CancellationToken);
                using var memIn = new MemoryStream(imageRaw);
                using var inputImage = Image.FromStream(memIn);
                using var bmp = new Bitmap(inputImage);

                // Buffer is loaded from top to bottom from left to right
                var imageDataRgba = new ArrayBufferWriter<byte>();
                for (int y = 0; y < inputImage.Height; y++)                    
                {
                    for (int x = 0; x < inputImage.Width; x++)
                    {
                        var y_inverted = (inputImage.Height - 1) - y;
                        var bufferAddress = (y_inverted * inputImage.Width + x) * 4;
                        var pixel = bmp.GetPixel(x, y_inverted);
                        imageDataRgba.Write(new ReadOnlySpan<byte>(new byte[] {
                            pixel.R,
                            pixel.G,
                            pixel.B,
                            pixel.A
                        }));
                    }
                }

                return new ImageData(imageDataRgba.WrittenMemory.ToArray(), 
                    inputImage.Width, inputImage.Height);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load {uri.AbsoluteUri}: {ex.Message}");
                throw;
            }
        }
    }
}
