using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MPDCtrlX.Models;
/*
public class Album : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string NameSort { get; set; } = string.Empty;

    public string ReleaseYear
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
    } = string.Empty;

    public bool IsSongsAcquired { get; set; }

    public ObservableCollection<SongInfo> Songs
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
    } = [];
}

public class AlbumEx : Album
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
*/
public class AlbumArtist : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string NameSort { get; set; } = string.Empty;

    public ObservableCollection<Album> Albums
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }
            field = value;
            OnPropertyChanged();
        }
    } = [];
}
