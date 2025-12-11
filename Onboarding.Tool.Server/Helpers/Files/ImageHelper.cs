using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Model.BriefYourMarket.Images;
using DatabaseImage = Onboarding.Tool.Model.Data.Image;
using Image = SixLabors.ImageSharp.Image;

namespace Onboarding.Tool.Server.Helpers.Files;

public static class ImageHelper
{
    private const int LOGO_MAX_DIMENSION = 200;
    private const int IMAGE_MAX_DIMENSION = 620;

    public static byte[] GetBrandColourBytes(string colour)
    {
        var rgba = System.Drawing.ColorTranslator.FromHtml(colour);
        var imageColor = new Rgba32(rgba.R, rgba.G, rgba.B, rgba.A);

        using var image = new Image<Rgba32>(50, 50);
        image.Mutate(ctx => ctx.Fill(imageColor));

        using var ms = new MemoryStream();
        image.Save(ms, new TiffEncoder());
        return ms.ToArray();
    }

    public static async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int maxDimension)
    {
        await using var inputStream = new MemoryStream(imageBytes);
        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(inputStream);

        var (newWidth, newHeight) = CalculateDimensions(image.Width, image.Height, maxDimension);

        image.Mutate(ctx => ctx.Resize(newWidth, newHeight, KnownResamplers.Bicubic));

        await using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new PngEncoder());

        return outputStream.ToArray();
    }

    public static bool TryValidateAndResizeImage(byte[] imageData, string fileName, int maxWidth, int maxHeight, out byte[]? processedImage)
    {
        processedImage = null;

        try
        {
            using MemoryStream ms = new(imageData);
            using Image image = Image.Load(ms);

            IImageFormat? format = image.Metadata.DecodedImageFormat;
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";

            bool isPng = format == PngFormat.Instance && extension == ".png";
            bool isJpeg = format == JpegFormat.Instance &&
                          (extension == ".jpg" || extension == ".jpeg");

            if (!isPng && !isJpeg || format == null)
                return false;

            int width = image.Width;
            int height = image.Height;

            if (width > maxWidth || height > maxHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            using MemoryStream outputStream = new();
            image.Save(outputStream, format);
            processedImage = outputStream.ToArray();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static DatabaseImage CreatePrintImage(string fileName, ImageType type, byte[] data, int instanceId)
    {
        using var image = Image.Load(data);
        using var outputStream = new MemoryStream();
        image.SaveAsTiff(outputStream);

        return new()
        {
            Name = fileName,
            Data = outputStream.ToArray(),
            Height = 0,
            Width = 0,
            Type = type,
            InstanceId = instanceId
        };
    }

    public static DatabaseImage CreateDatabaseImage(string fileName, byte[] imageData, ImageType imageType, int instanceId)
    {
        return new()
        {
            Name = fileName,
            Data = imageData,
            Height = 0,
            Width = 0,
            Type = imageType,
            InstanceId = instanceId
        };
    }

    public static byte[]? ProcessImage(UploadImage uploadImage, ImageType imageType)
    {
        int maxDimension = imageType == ImageType.Logo ? LOGO_MAX_DIMENSION : IMAGE_MAX_DIMENSION;

        if (TryValidateAndResizeImage(
            uploadImage.Data,
            uploadImage.FileName,
            maxDimension,
            maxDimension,
            out byte[]? processedImageBytes))
        {
            return processedImageBytes;
        }

        return null;
    }

    private static (int width, int height) CalculateDimensions(int originalWidth, int originalHeight, int maxDimension)
    {
        if (originalWidth > originalHeight)
        {
            int newHeight = (int)(originalHeight * ((float)maxDimension / originalWidth));
            return (maxDimension, newHeight);
        }

        int newWidth = (int)(originalWidth * ((float)maxDimension / originalHeight));
        return (newWidth, maxDimension);
    }
}
