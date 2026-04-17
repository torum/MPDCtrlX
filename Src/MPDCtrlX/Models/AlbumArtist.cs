using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MPDCtrlX.Core.Models;

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
    public bool IsSongsAcquired { get; set; } = false;

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

public class AlbumEx :Album
{
    public string AlbumArtist { get; set; } = string.Empty;

    public string AlbumArtistSort { get; set; } = string.Empty;

    public string? AlbumImagePath { get; set; } = null;

    public Bitmap? AlbumImage { 
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
    } = null;

    public bool IsImageAcquired { get; set; } = false;
    public bool IsImageLoading { get; set; } = false;
}

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
