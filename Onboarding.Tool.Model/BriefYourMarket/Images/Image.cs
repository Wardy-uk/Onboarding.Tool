using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using ImageSharp = SixLabors.ImageSharp.Image;

namespace Onboarding.Tool.Model.BriefYourMarket.Images;

public class Image
{
    private const double Dpi = 300.0;
    private const double MmPerInch = 25.4;
    private const double MaxLogoWidthMm = 21.0;
    private const double MaxLogoHeightMm = 14.0;
    private const double PaddingMm = 8.0;
    private const double CanvasWidthMm = 220.0;
    private const double CanvasHeightMm = 158.0;

    private int _width = 0;

    private int _height = 0;

    public required string Name { get; init; }

    public required byte[] Data { get; init; }

    public required ImageType Type { get; init; }

    public int Width
    {
        get
        {
            if (_width == 0 || _height == 0)
                GetImageDimensions();

            return _width;
        }
    }

    public int Height
    {
        get
        {
            if (_width == 0 || _height == 0)
                GetImageDimensions();

            return _height;
        }
    }

    public int? LogoWidthOverride = null;
    public int? LogoHeightOverride = null;
    public int? LogoXOverride = null;
    public int? LogoYOverride = null;

    public double CardLogoWidth
    {
        get
        {
            if (LogoWidthOverride != null)
                return (double)LogoWidthOverride;

            double widthMm = Width * MmPerInch / Dpi;
            double heightMm = Height * MmPerInch / Dpi;

            double widthScale = MaxLogoWidthMm / widthMm;
            double heightScale = MaxLogoHeightMm / heightMm;
            double scale = Math.Min(widthScale, heightScale);

            return widthMm * scale;
        }
    }

    public double CardLogoHeight
    {
        get
        {
            if (LogoHeightOverride != null)
                return (double)LogoHeightOverride;

            double widthMm = Width * MmPerInch / Dpi;
            double heightMm = Height * MmPerInch / Dpi;

            double widthScale = MaxLogoWidthMm / widthMm;
            double heightScale = MaxLogoHeightMm / heightMm;
            double scale = Math.Min(widthScale, heightScale);

            return heightMm * scale;
        }
    }

    public double CardLogoX
    {
        get
        {
            if (LogoXOverride != null)
                return (double)LogoXOverride;

            return CanvasWidthMm - CardLogoWidth - PaddingMm;
        }
    }

    public double CardLogoY
    {
        get
        {
            if (LogoYOverride != null)
                return (double)LogoYOverride;

            return CanvasHeightMm - CardLogoHeight - PaddingMm;
        }
    }

    private void GetImageDimensions()
    {
        if (Data.Length == 0)
            return;

        using MemoryStream ms = new(Data);
        using ImageSharp image = ImageSharp.Load(ms);

        _width = image.Width;
        _height = image.Height;
    }
}
