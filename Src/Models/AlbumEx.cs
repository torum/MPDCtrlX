using Avalonia.Media.Imaging;

namespace MPDCtrlX.Models;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public sealed class AlbumEx : Album
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
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
