using System.Text;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// The smallest bytes the image upload tells apart: the PNG and JPEG signatures it accepts, and content it refuses.
/// </summary>
public static class TestImages
{
    public static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    public static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    public static readonly byte[] Gif = Encoding.ASCII.GetBytes("GIF89a....");

    public static readonly byte[] Text = Encoding.UTF8.GetBytes("# not an image");
}
