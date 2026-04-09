using Avalonia.Controls.Chrome;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MPDCtrlX.Core.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MPDCtrlX.Core.Models;


public class Album : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    public bool IsSongsAcquired { get; set; } = false;

    public ObservableCollection<SongInfo> _songs = [];
    public ObservableCollection<SongInfo> Songs
    {
        get => _songs;
        set
        {
            if (_songs == value)
            {
                return;
            }
            _songs = value;
            OnPropertyChanged(nameof(Songs));
        }
    }
}

public class AlbumEx :Album
{
    public string AlbumArtist { get; set; } = string.Empty;

    public string AlbumArtistSort { get; set; } = string.Empty;

    public string? AlbumImagePath { get; set; } = null;

    private Bitmap? _albumImage = null;
    public Bitmap? AlbumImage { 
        get => _albumImage; 
        set
        {
            if (_albumImage == value)
            {
                return;
            }
            _albumImage = value;
            OnPropertyChanged(nameof(AlbumImage));
        }
    }

    public bool IsImageAcquired { get; set; } = false;
    public bool IsImageLoading { get; set; } = false;
}

public class AlbumArtist : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string NameSort { get; set; } = string.Empty;

    public ObservableCollection<Album> Albums { get; private set; } = [];
}
