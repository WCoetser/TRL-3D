using System;
using System.Threading.Tasks;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Interface for loading images.
    /// This is implemented by the app using this system to supply images
    /// without worrying about where they come from.
    /// </summary>
    public interface IImageLoader
    {
        /// <summary>
        /// Gets an image based on a path or URL
        /// </summary>
        Task<ImageData> LoadImage(Uri uri);
    }
}
