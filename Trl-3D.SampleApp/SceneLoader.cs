using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;
using Trl_3D.OpenTk.Assertions;

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
                new RenderTestTriagle(_logger),
                new GrabScreenshot
                {
                    CaptureCallback = (buffer, renderInfo) =>
                    {
                        var filename = $"capture.png";

                        var fileInfo = new FileInfo(filename);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                        }

                        using (Bitmap bmp = new Bitmap(renderInfo.Width, renderInfo.Height))
                        {
                            for (int x = 0; x < renderInfo.Width; x++)
                            {
                                for (int y = 0; y < renderInfo.Height; y++)
                                {
                                    var bufferAddress = (y * renderInfo.Width + x) * 3;
                                    byte red = buffer[bufferAddress];
                                    byte green = buffer[bufferAddress + 1];
                                    byte blue = buffer[bufferAddress + 2];
                                    
                                    var y_inverted = (renderInfo.Height - 1) - y;

                                    bmp.SetPixel(x, y_inverted, Color.FromArgb(red, green, blue));
                                }
                            }

                            bmp.Save(fileInfo.FullName, ImageFormat.Png);
                        }

                        _logger.LogInformation($"Captured to {fileInfo.FullName}");
                    }
                }
            };
        }
    }
}
