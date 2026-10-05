using Avalonia.Media.Imaging;

namespace MPDCtrlX.Models;

public class AlbumImage
{
    public bool IsDownloading { get; set; }

    public bool IsSuccess { get; set; }

    public string? SongFilePath { get; set; }

    public byte[]? BinaryData { get; set; }// = Array.Empty<byte>();

    public int BinarySize { get; set; }

    public Bitmap? AlbumImageSource { get; set; }
}
