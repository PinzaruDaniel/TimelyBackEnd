using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace TimelyBackEnd.Services;

public class ImageCompressionService
{
    /// <summary>
    /// Compresses an image stream and returns the compressed image as a memory stream.
    /// </summary>
    /// <param name="inputStream">The input image stream</param>
    /// <param name="maxWidth">Maximum width in pixels (default: 1920)</param>
    /// <param name="maxHeight">Maximum height in pixels (default: 1920)</param>
    /// <param name="quality">JPEG quality (1-100, default: 85)</param>
    /// <returns>Compressed image as memory stream and the file extension (.jpg or .png)</returns>
    public async Task<(MemoryStream CompressedStream, string Extension)> CompressImageAsync(
        Stream inputStream, 
        int maxWidth = 1920, 
        int maxHeight = 1920, 
        int quality = 85)
    {
        using var image = await Image.LoadAsync(inputStream);
        
        // Resize if image is larger than max dimensions
        if (image.Width > maxWidth || image.Height > maxHeight)
        {
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            };
            image.Mutate(x => x.Resize(options));
        }

        // Convert to RGB format (JPEG doesn't support transparency)
        // This ensures we can save as JPEG while preserving the image content
        using var rgbImage = image.CloneAs<Rgb24>();
        
        // Always use JPEG for compression (better compression ratio for photos)
        // TODO: If you need to preserve transparency, you can add PNG support here
        var outputStream = new MemoryStream();
        var extension = ".jpg";
        
        var jpegEncoder = new JpegEncoder
        {
            Quality = quality
        };
        await rgbImage.SaveAsync(outputStream, jpegEncoder);

        outputStream.Position = 0;
        return (outputStream, extension);
    }
}

