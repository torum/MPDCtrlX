using Avalonia.Media.Imaging;

namespace MPDCtrlX.Models;

public sealed class AlbumEx : Album
{
    public string AlbumArtist { get; set; } = string.Empty;

    public string AlbumArtistSort { get; set; } = string.Empty;

    public string? AlbumImagePath { get; set; }

    public Bitmap? AlbumImage
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImageAcquired { get; set; }
    public bool IsImageLoading { get; set; }
}
