using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Core;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using MPDCtrlX.Common;
using MPDCtrlX.Models;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.Views;
using MPDCtrlX.Views.Dialogs;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;
//using CommunityToolkit.WinUI.Converters; // this is bad one.

namespace MPDCtrlX.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private string _appVersion = string.Empty;
    public string AppVersion
    {
        get
        {
            if (!string.IsNullOrEmpty(_appVersion)) return _appVersion;
            
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = assembly.Version;
            _appVersion = $"{version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision} - RC9";

            return _appVersion;
        }
    }

    #region == Layout ==

    #region == Window and loading flag ==

    public int WindowTop = 0;
    public int WindowLeft = 0;
    public double WindowHeight = 0;
    public double WindowWidth = 0;
    public WindowState WindowState = WindowState.Normal;

    private bool _isFullyLoaded;
    public bool IsFullyLoaded
    {
        get
        {
            return _isFullyLoaded;
        }
        set
        {
            if (_isFullyLoaded == value)
                return;

            _isFullyLoaded = value;
            OnPropertyChanged(nameof(IsFullyLoaded));
        }
    }

    // TODO: no longer used...
    private double _mainLeftPainActualWidth = 241;
    public double MainLeftPainActualWidth
    {
        get
        {
            return _mainLeftPainActualWidth;
        }
        set
        {
            if (value == _mainLeftPainActualWidth) return;

            _mainLeftPainActualWidth = value;

            OnPropertyChanged(nameof(MainLeftPainActualWidth));
        }
    }

    // TODO: no longer used...
    private double _mainLeftPainWidth = 241;
    public double MainLeftPainWidth
    {
        get
        {
            return _mainLeftPainWidth;
        }
        set
        {
            if (value == _mainLeftPainWidth) return;

            _mainLeftPainWidth = value;

            OnPropertyChanged(nameof(MainLeftPainWidth));
        }
    }

    private bool _isNavigationViewMenuOpen = true;
    public bool IsNavigationViewMenuOpen
    {
        get { return _isNavigationViewMenuOpen; }
        set
        {
            if (_isNavigationViewMenuOpen == value)
                return;

            _isNavigationViewMenuOpen = value;
            OnPropertyChanged(nameof(IsNavigationViewMenuOpen));

            foreach (var hoge in MainMenuItems)
            {
                switch (hoge)
                {
                    case NodeMenuLibrary lib:
                        lib.Expanded = _isNavigationViewMenuOpen;
                        break;
                    case NodeMenuPlaylists plt:
                        plt.Expanded = _isNavigationViewMenuOpen;
                        break;
                }
            }

        }
    }

    #endregion

    #region == Queue column headers ==

    // Posiotion header
    private bool _isQueueColumnHeaderPositionVisible = true;
    public bool IsQueueColumnHeaderPositionVisible
    {
        get
        {
            return _isQueueColumnHeaderPositionVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderPositionVisible)
                return;

            _isQueueColumnHeaderPositionVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderPositionVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderPositionWidth = 60;
    public double QueueColumnHeaderPositionWidth
    {
        get
        {
            return _queueColumnHeaderPositionWidth;
        }
        set
        {
            if (value == _queueColumnHeaderPositionWidth)
                return;

            _queueColumnHeaderPositionWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderPositionWidth));
        }
    }

    // NowPlaying header
    private bool _isQueueColumnHeaderNowPlayingVisible = true;
    public bool IsQueueColumnHeaderNowPlayingVisible
    {
        get
        {
            return _isQueueColumnHeaderNowPlayingVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderNowPlayingVisible)
                return;

            _isQueueColumnHeaderNowPlayingVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderNowPlayingVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderNowPlayingWidth = 32;
    public double QueueColumnHeaderNowPlayingWidth
    {
        get
        {
            return _queueColumnHeaderNowPlayingWidth;
        }
        set
        {
            if (value == _queueColumnHeaderNowPlayingWidth)
                return;

            _queueColumnHeaderNowPlayingWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderNowPlayingWidth));
        }
    }

    // Title header (not user customizable)
    private double _queueColumnHeaderTitleWidth = 180;
    public double QueueColumnHeaderTitleWidth
    {
        get
        {
            return _queueColumnHeaderTitleWidth;
        }
        set
        {
            if (value == _queueColumnHeaderTitleWidth)
                return;

            _queueColumnHeaderTitleWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderTitleWidth));
        }
    }

    private bool _isQueueColumnHeaderTimeVisible = true;
    public bool IsQueueColumnHeaderTimeVisible
    {
        get
        {
            return _isQueueColumnHeaderTimeVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderTimeVisible)
                return;

            _isQueueColumnHeaderTimeVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderTimeVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderTimeWidth = 62;
    public double QueueColumnHeaderTimeWidth
    {
        get
        {
            return _queueColumnHeaderTimeWidth;
        }
        set
        {
            if (value == _queueColumnHeaderTimeWidth)
                return;

            _queueColumnHeaderTimeWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderTimeWidth));
        }
    }

    private bool _isQueueColumnHeaderArtistVisible = true;
    public bool IsQueueColumnHeaderArtistVisible
    {
        get
        {
            return _isQueueColumnHeaderArtistVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderArtistVisible)
                return;

            _isQueueColumnHeaderArtistVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderArtistVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderArtistWidth = 120;
    public double QueueColumnHeaderArtistWidth
    {
        get
        {
            return _queueColumnHeaderArtistWidth;
        }
        set
        {
            if (value == _queueColumnHeaderArtistWidth)
                return;

            _queueColumnHeaderArtistWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderArtistWidth));
        }
    }

    private bool _isQueueColumnHeaderAlbumVisible = true;
    public bool IsQueueColumnHeaderAlbumVisible
    {
        get
        {
            return _isQueueColumnHeaderAlbumVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderAlbumVisible)
                return;

            _isQueueColumnHeaderAlbumVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderAlbumVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderAlbumWidth = 120;
    public double QueueColumnHeaderAlbumWidth
    {
        get
        {
            return _queueColumnHeaderAlbumWidth;
        }
        set
        {
            if (value == _queueColumnHeaderAlbumWidth)
                return;

            _queueColumnHeaderAlbumWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderAlbumWidth));
        }
    }

    private bool _isQueueColumnHeaderDiscVisible = true;
    public bool IsQueueColumnHeaderDiscVisible
    {
        get
        {
            return _isQueueColumnHeaderDiscVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderDiscVisible)
                return;

            _isQueueColumnHeaderDiscVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderDiscVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderDiscWidth = 62;
    public double QueueColumnHeaderDiscWidth
    {
        get
        {
            return _queueColumnHeaderDiscWidth;
        }
        set
        {
            if (value == _queueColumnHeaderDiscWidth)
                return;

            _queueColumnHeaderDiscWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderDiscWidth));
        }
    }

    private bool _isQueueColumnHeaderTrackVisible = true;
    public bool IsQueueColumnHeaderTrackVisible
    {
        get
        {
            return _isQueueColumnHeaderTrackVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderTrackVisible)
                return;

            _isQueueColumnHeaderTrackVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderTrackVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderTrackWidth = 62;
    public double QueueColumnHeaderTrackWidth
    {
        get
        {
            return _queueColumnHeaderTrackWidth;
        }
        set
        {
            if (value == _queueColumnHeaderTrackWidth)
                return;

            _queueColumnHeaderTrackWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderTrackWidth));
        }
    }

    // Genre header
    private bool _isQueueColumnHeaderGenreVisible = true;
    public bool IsQueueColumnHeaderGenreVisible
    {
        get
        {
            return _isQueueColumnHeaderGenreVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderGenreVisible)
                return;

            _isQueueColumnHeaderGenreVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderGenreVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderGenreWidth = 100;
    public double QueueColumnHeaderGenreWidth
    {
        get
        {
            return _queueColumnHeaderGenreWidth;
        }
        set
        {
            if (value == _queueColumnHeaderGenreWidth)
                return;

            _queueColumnHeaderGenreWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderGenreWidth));
        }
    }

    private bool _isQueueColumnHeaderLastModifiedVisible = true;
    public bool IsQueueColumnHeaderLastModifiedVisible
    {
        get
        {
            return _isQueueColumnHeaderLastModifiedVisible;
        }
        set
        {
            if (value == _isQueueColumnHeaderLastModifiedVisible)
                return;

            _isQueueColumnHeaderLastModifiedVisible = value;

            OnPropertyChanged(nameof(IsQueueColumnHeaderLastModifiedVisible));
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _queueColumnHeaderLastModifiedWidth = 180;
    public double QueueColumnHeaderLastModifiedWidth
    {
        get
        {
            return _queueColumnHeaderLastModifiedWidth;
        }
        set
        {
            if (value == _queueColumnHeaderLastModifiedWidth)
                return;

            _queueColumnHeaderLastModifiedWidth = value;

            OnPropertyChanged(nameof(QueueColumnHeaderLastModifiedWidth));
        }
    }


    #endregion

    #region == Files column headers == 

    private double _filesColumnHeaderTitleWidth = 260;
    public double FilesColumnHeaderTitleWidth
    {
        get
        {
            return _filesColumnHeaderTitleWidth;
        }
        set
        {
            if (value == _filesColumnHeaderTitleWidth)
                return;

            if (value > 12)
                _filesColumnHeaderTitleWidth = value;

            OnPropertyChanged(nameof(FilesColumnHeaderTitleWidth));
        }
    }

    private double _filesColumnHeaderFilePathWidth = 250;
    public double FilesColumnHeaderFilePathWidth
    {
        get
        {
            return _filesColumnHeaderFilePathWidth;
        }
        set
        {
            if (value == _filesColumnHeaderFilePathWidth)
                return;

            if (value > 12)
                _filesColumnHeaderFilePathWidth = value;

            OnPropertyChanged(nameof(FilesColumnHeaderFilePathWidth));
        }
    }

    private bool _isFilesColumnHeaderFilePathVisible = true;
    public bool IsFilesColumnHeaderFilePathVisible
    {
        get
        {
            return _isFilesColumnHeaderFilePathVisible;
        }
        set
        {
            if (value == _isFilesColumnHeaderFilePathVisible)
                return;

            _isFilesColumnHeaderFilePathVisible = value;

            OnPropertyChanged(nameof(IsFilesColumnHeaderFilePathVisible));
            // Notify code behind to do some work around ...
            FilesHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region == Search column headers ==

    private bool _isSearchColumnHeaderPositionVisible = true;
    public bool IsSearchColumnHeaderPositionVisible
    {
        get
        {
            return _isSearchColumnHeaderPositionVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderPositionVisible)
                return;

            _isSearchColumnHeaderPositionVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderPositionVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderPositionWidth = 60;
    public double SearchColumnHeaderPositionWidth
    {
        get
        {
            return _searchColumnHeaderPositionWidth;
        }
        set
        {
            if (value == _searchColumnHeaderPositionWidth)
                return;

            _searchColumnHeaderPositionWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderPositionWidth));
        }
    }

    private double _searchColumnHeaderTitleWidth = 180;
    public double SearchColumnHeaderTitleWidth
    {
        get
        {
            return _searchColumnHeaderTitleWidth;
        }
        set
        {
            if (value == _searchColumnHeaderTitleWidth)
                return;

            _searchColumnHeaderTitleWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderTitleWidth));
        }
    }

    private bool _isSearchColumnHeaderTimeVisible = true;
    public bool IsSearchColumnHeaderTimeVisible
    {
        get
        {
            return _isSearchColumnHeaderTimeVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderTimeVisible)
                return;

            _isSearchColumnHeaderTimeVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderTimeVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderTimeWidth = 62;
    public double SearchColumnHeaderTimeWidth
    {
        get
        {
            return _searchColumnHeaderTimeWidth;
        }
        set
        {
            if (value == _searchColumnHeaderTimeWidth)
                return;

            _searchColumnHeaderTimeWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderTimeWidth));
        }
    }

    private bool _isSearchColumnHeaderArtistVisible = true;
    public bool IsSearchColumnHeaderArtistVisible
    {
        get
        {
            return _isSearchColumnHeaderArtistVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderArtistVisible)
                return;

            _isSearchColumnHeaderArtistVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderArtistVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderArtistWidth = 120;
    public double SearchColumnHeaderArtistWidth
    {
        get
        {
            return _searchColumnHeaderArtistWidth;
        }
        set
        {
            if (value == _searchColumnHeaderArtistWidth)
                return;

            _searchColumnHeaderArtistWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderArtistWidth));
        }
    }

    private bool _isSearchColumnHeaderAlbumVisible = true;
    public bool IsSearchColumnHeaderAlbumVisible
    {
        get
        {
            return _isSearchColumnHeaderAlbumVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderAlbumVisible)
                return;

            _isSearchColumnHeaderAlbumVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderAlbumVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderAlbumWidth = 120;
    public double SearchColumnHeaderAlbumWidth
    {
        get
        {
            return _searchColumnHeaderAlbumWidth;
        }
        set
        {
            if (value == _searchColumnHeaderAlbumWidth)
                return;

            _searchColumnHeaderAlbumWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderAlbumWidth));
        }
    }

    private bool _isSearchColumnHeaderDiscVisible = true;
    public bool IsSearchColumnHeaderDiscVisible
    {
        get
        {
            return _isSearchColumnHeaderDiscVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderDiscVisible)
                return;

            _isSearchColumnHeaderDiscVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderDiscVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderDiscWidth = 62;
    public double SearchColumnHeaderDiscWidth
    {
        get
        {
            return _searchColumnHeaderDiscWidth;
        }
        set
        {
            if (value == _searchColumnHeaderDiscWidth)
                return;

            _searchColumnHeaderDiscWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderDiscWidth));
        }
    }

    private bool _isSearchColumnHeaderTrackVisible = true;
    public bool IsSearchColumnHeaderTrackVisible
    {
        get
        {
            return _isSearchColumnHeaderTrackVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderTrackVisible)
                return;

            _isSearchColumnHeaderTrackVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderTrackVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderTrackWidth = 62;
    public double SearchColumnHeaderTrackWidth
    {
        get
        {
            return _searchColumnHeaderTrackWidth;
        }
        set
        {
            if (value == _searchColumnHeaderTrackWidth)
                return;

            _searchColumnHeaderTrackWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderTrackWidth));
        }
    }

    private bool _isSearchColumnHeaderGenreVisible = true;
    public bool IsSearchColumnHeaderGenreVisible
    {
        get
        {
            return _isSearchColumnHeaderGenreVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderGenreVisible)
                return;

            _isSearchColumnHeaderGenreVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderGenreVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderGenreWidth = 100;
    public double SearchColumnHeaderGenreWidth
    {
        get
        {
            return _searchColumnHeaderGenreWidth;
        }
        set
        {
            if (value == _searchColumnHeaderGenreWidth)
                return;

            _searchColumnHeaderGenreWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderGenreWidth));
        }
    }

    private bool _isSearchColumnHeaderLastModifiedVisible = true;
    public bool IsSearchColumnHeaderLastModifiedVisible
    {
        get
        {
            return _isSearchColumnHeaderLastModifiedVisible;
        }
        set
        {
            if (value == _isSearchColumnHeaderLastModifiedVisible)
                return;

            _isSearchColumnHeaderLastModifiedVisible = value;

            OnPropertyChanged(nameof(IsSearchColumnHeaderLastModifiedVisible));
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _searchColumnHeaderLastModifiedWidth = 180;
    public double SearchColumnHeaderLastModifiedWidth
    {
        get
        {
            return _searchColumnHeaderLastModifiedWidth;
        }
        set
        {
            if (value == _searchColumnHeaderLastModifiedWidth)
                return;

            _searchColumnHeaderLastModifiedWidth = value;

            OnPropertyChanged(nameof(SearchColumnHeaderLastModifiedWidth));
        }
    }


    #endregion

    #region == PlaylistItem headers == 

    private bool _isPlaylistColumnHeaderPositionVisible = true;
    public bool IsPlaylistColumnHeaderPositionVisible
    {
        get
        {
            return _isPlaylistColumnHeaderPositionVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderPositionVisible)
                return;

            _isPlaylistColumnHeaderPositionVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderPositionVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderPositionWidth = 60;
    public double PlaylistColumnHeaderPositionWidth
    {
        get
        {
            return _playlistColumnHeaderPositionWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderPositionWidth)
                return;

            _playlistColumnHeaderPositionWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderPositionWidth));
        }
    }

    private double _playlistColumnHeaderTitleWidth = 180;
    public double PlaylistColumnHeaderTitleWidth
    {
        get
        {
            return _playlistColumnHeaderTitleWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderTitleWidth)
                return;

            _playlistColumnHeaderTitleWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderTitleWidth));
        }
    }

    private bool _isPlaylistColumnHeaderTimeVisible = true;
    public bool IsPlaylistColumnHeaderTimeVisible
    {
        get
        {
            return _isPlaylistColumnHeaderTimeVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderTimeVisible)
                return;

            _isPlaylistColumnHeaderTimeVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderTimeVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderTimeWidth = 62;
    public double PlaylistColumnHeaderTimeWidth
    {
        get
        {
            return _playlistColumnHeaderTimeWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderTimeWidth)
                return;

            _playlistColumnHeaderTimeWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderTimeWidth));
        }
    }

    private bool _isPlaylistColumnHeaderArtistVisible = true;
    public bool IsPlaylistColumnHeaderArtistVisible
    {
        get
        {
            return _isPlaylistColumnHeaderArtistVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderArtistVisible)
                return;

            _isPlaylistColumnHeaderArtistVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderArtistVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderArtistWidth = 120;
    public double PlaylistColumnHeaderArtistWidth
    {
        get
        {
            return _playlistColumnHeaderArtistWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderArtistWidth)
                return;

            _playlistColumnHeaderArtistWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderArtistWidth));
        }
    }

    private bool _isPlaylistColumnHeaderAlbumVisible = true;
    public bool IsPlaylistColumnHeaderAlbumVisible
    {
        get
        {
            return _isPlaylistColumnHeaderAlbumVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderAlbumVisible)
                return;

            _isPlaylistColumnHeaderAlbumVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderAlbumVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderAlbumWidth = 120;
    public double PlaylistColumnHeaderAlbumWidth
    {
        get
        {
            return _playlistColumnHeaderAlbumWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderAlbumWidth)
                return;

            _playlistColumnHeaderAlbumWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderAlbumWidth));
        }
    }

    private bool _isPlaylistColumnHeaderDiscVisible = true;
    public bool IsPlaylistColumnHeaderDiscVisible
    {
        get
        {
            return _isPlaylistColumnHeaderDiscVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderDiscVisible)
                return;

            _isPlaylistColumnHeaderDiscVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderDiscVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderDiscWidth = 62;
    public double PlaylistColumnHeaderDiscWidth
    {
        get
        {
            return _playlistColumnHeaderDiscWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderDiscWidth)
                return;

            _playlistColumnHeaderDiscWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderDiscWidth));
        }
    }

    private bool _isPlaylistColumnHeaderTrackVisible = true;
    public bool IsPlaylistColumnHeaderTrackVisible
    {
        get
        {
            return _isPlaylistColumnHeaderTrackVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderTrackVisible)
                return;

            _isPlaylistColumnHeaderTrackVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderTrackVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

    }
    private double _playlistColumnHeaderTrackWidth = 62;
    public double PlaylistColumnHeaderTrackWidth
    {
        get
        {
            return _playlistColumnHeaderTrackWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderTrackWidth)
                return;

            _playlistColumnHeaderTrackWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderTrackWidth));
        }
    }

    private bool _isPlaylistColumnHeaderGenreVisible = true;
    public bool IsPlaylistColumnHeaderGenreVisible
    {
        get
        {
            return _isPlaylistColumnHeaderGenreVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderGenreVisible)
                return;

            _isPlaylistColumnHeaderGenreVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderGenreVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

    }
    private double _playlistColumnHeaderGenreWidth = 100;
    public double PlaylistColumnHeaderGenreWidth
    {
        get
        {
            return _playlistColumnHeaderGenreWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderGenreWidth)
                return;

            _playlistColumnHeaderGenreWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderGenreWidth));
        }
    }

    private bool _isPlaylistColumnHeaderLastModifiedVisible = true;
    public bool IsPlaylistColumnHeaderLastModifiedVisible
    {
        get
        {
            return _isPlaylistColumnHeaderLastModifiedVisible;
        }
        set
        {
            if (value == _isPlaylistColumnHeaderLastModifiedVisible)
                return;

            _isPlaylistColumnHeaderLastModifiedVisible = value;

            OnPropertyChanged(nameof(IsPlaylistColumnHeaderLastModifiedVisible));
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private double _playlistColumnHeaderLastModifiedWidth = 180;
    public double PlaylistColumnHeaderLastModifiedWidth
    {
        get
        {
            return _playlistColumnHeaderLastModifiedWidth;
        }
        set
        {
            if (value == _playlistColumnHeaderLastModifiedWidth)
                return;

            _playlistColumnHeaderLastModifiedWidth = value;

            OnPropertyChanged(nameof(PlaylistColumnHeaderLastModifiedWidth));
        }
    }

    #endregion

    #endregion

    #region == Themes ==

    private ObservableCollection<Theme> _themes =
        [
            new Theme() { Id = 0, Name = "System", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_System, IconData="M7.5,2C5.71,3.15 4.5,5.18 4.5,7.5C4.5,9.82 5.71,11.85 7.53,13C4.46,13 2,10.54 2,7.5A5.5,5.5 0 0,1 7.5,2M19.07,3.5L20.5,4.93L4.93,20.5L3.5,19.07L19.07,3.5M12.89,5.93L11.41,5L9.97,6L10.39,4.3L9,3.24L10.75,3.12L11.33,1.47L12,3.1L13.73,3.13L12.38,4.26L12.89,5.93M9.59,9.54L8.43,8.81L7.31,9.59L7.65,8.27L6.56,7.44L7.92,7.35L8.37,6.06L8.88,7.33L10.24,7.36L9.19,8.23L9.59,9.54M19,13.5A5.5,5.5 0 0,1 13.5,19C12.28,19 11.15,18.6 10.24,17.93L17.93,10.24C18.6,11.15 19,12.28 19,13.5M14.6,20.08L17.37,18.93L17.13,22.28L14.6,20.08M18.93,17.38L20.08,14.61L22.28,17.15L18.93,17.38M20.08,12.42L18.94,9.64L22.28,9.88L20.08,12.42M9.63,18.93L12.4,20.08L9.87,22.27L9.63,18.93Z"},
            new Theme() { Id = 1, Name = "Dark", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_Dark, IconData="M17.75,4.09L15.22,6.03L16.13,9.09L13.5,7.28L10.87,9.09L11.78,6.03L9.25,4.09L12.44,4L13.5,1L14.56,4L17.75,4.09M21.25,11L19.61,12.25L20.2,14.23L18.5,13.06L16.8,14.23L17.39,12.25L15.75,11L17.81,10.95L18.5,9L19.19,10.95L21.25,11M18.97,15.95C19.8,15.87 20.69,17.05 20.16,17.8C19.84,18.25 19.5,18.67 19.08,19.07C15.17,23 8.84,23 4.94,19.07C1.03,15.17 1.03,8.83 4.94,4.93C5.34,4.53 5.76,4.17 6.21,3.85C6.96,3.32 8.14,4.21 8.06,5.04C7.79,7.9 8.75,10.87 10.95,13.06C13.14,15.26 16.1,16.22 18.97,15.95M17.33,17.97C14.5,17.81 11.7,16.64 9.53,14.5C7.36,12.31 6.2,9.5 6.04,6.68C3.23,9.82 3.34,14.64 6.35,17.66C9.37,20.67 14.19,20.78 17.33,17.97Z"},
            new Theme() { Id = 2, Name = "Light", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_Light, IconData="M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,2L14.39,5.42C13.65,5.15 12.84,5 12,5C11.16,5 10.35,5.15 9.61,5.42L12,2M3.34,7L7.5,6.65C6.9,7.16 6.36,7.78 5.94,8.5C5.5,9.24 5.25,10 5.11,10.79L3.34,7M3.36,17L5.12,13.23C5.26,14 5.53,14.78 5.95,15.5C6.37,16.24 6.91,16.86 7.5,17.37L3.36,17M20.65,7L18.88,10.79C18.74,10 18.47,9.23 18.05,8.5C17.63,7.78 17.1,7.15 16.5,6.64L20.65,7M20.64,17L16.5,17.36C17.09,16.85 17.62,16.22 18.04,15.5C18.46,14.77 18.73,14 18.87,13.21L20.64,17M12,22L9.59,18.56C10.33,18.83 11.14,19 12,19C12.82,19 13.63,18.83 14.37,18.56L12,22Z"}
        ];

    public ObservableCollection<Theme> Themes
    {
        get { return _themes; }
        set { _themes = value; }
    }

    private Theme _currentTheme;
    public Theme CurrentTheme
    {
        get
        {
            return _currentTheme;
        }
        set
        {
            if (_currentTheme == value) return;

            _currentTheme = value;
            OnPropertyChanged(nameof(CurrentTheme));

            FluentAvaloniaTheme? faTheme = ((Application.Current as App)!.Styles[0] as FluentAvaloniaTheme);
            if (_currentTheme.Id == 1)
            {
                faTheme!.PreferSystemTheme = false;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else if (_currentTheme.Id == 2)
            {
                faTheme!.PreferSystemTheme = false;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else
            {
                faTheme!.PreferSystemTheme = true;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Default;
            }
        }
    }

    #endregion

    #region == Status and Visibility switch flags ==  

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value)
                return;

            _isConnected = value;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));
            OnPropertyChanged(nameof(IsNotConnecting));

            IsConnecting = !_isConnected;

            if (!_isConnected)
            {
                IsNotConnectingNorConnected = true;
            }
            if (_isConnected)
            {
                IsConnectButtonEnabled = true;
            }
        }
    }

    private bool _isConnecting;
    public bool IsConnecting
    {
        get
        {
            return _isConnecting;
        }
        set
        {
            if (_isConnecting == value)
                return;

            _isConnecting = value;
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsNotConnecting));
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));

            OnPropertyChanged(nameof(IsProfileSwitchOK));
            if (_isConnecting)
            {
                IsConnectButtonEnabled = false;
            }
        }
    }

    private bool _isNotConnectingNorConnected = true;
    public bool IsNotConnectingNorConnected
    {
        get
        {
            return _isNotConnectingNorConnected;
        }
        set
        {
            if (_isNotConnectingNorConnected == value)
                return;

            _isNotConnectingNorConnected = value;
            OnPropertyChanged(nameof(IsNotConnectingNorConnected));
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));

            if (_isNotConnectingNorConnected)
            {
                IsConnectButtonEnabled = true;
            }
        }
    }

    private bool _isConnectButtonEnabled = true;
    public bool IsConnectButtonEnabled
    {
        get { return _isConnectButtonEnabled; }
        set
        {
            if (_isConnectButtonEnabled == value)
                return;

            _isConnectButtonEnabled = value;

            OnPropertyChanged(nameof(IsConnectButtonEnabled));
        }
    }

    public bool IsNotConnecting
    {
        get
        {
            return !_isConnecting;
        }
    }
    /*
    private bool _isSettingsShow;
    public bool IsSettingsShow
    {
        get { return _isSettingsShow; }
        set
        {
            if (_isSettingsShow == value)
                return;

            _isSettingsShow = value;

            if (_isSettingsShow)
            {
                if (SelectedProfile is null)
                {
                    IsConnectionSettingShow = false;
                }
                else
                {
                    IsConnectionSettingShow = false;
                }
            }
            else
            {
                if (SelectedProfile is null)
                {
                    IsConnectionSettingShow = true;
                }
                else
                {
                    if (!IsConnected)
                    {
                        IsConnectionSettingShow = true;
                    }
                }
            }

            OnPropertyChanged(nameof(IsSettingsShow));

        }
    }
    */
    private bool _isConnectionSettingShow;
    public bool IsConnectionSettingShow
    {
        get { return _isConnectionSettingShow; }
        set
        {
            if (_isConnectionSettingShow == value)
                return;

            _isConnectionSettingShow = value;
            OnPropertyChanged(nameof(IsConnectionSettingShow));
        }
    }
    /*
    private bool _isChangePasswordDialogShow;
    public bool IsChangePasswordDialogShow
    {
        get
        {
            return _isChangePasswordDialogShow;
        }
        set
        {
            if (_isChangePasswordDialogShow == value)
                return;

            _isChangePasswordDialogShow = value;
            OnPropertyChanged(nameof(IsChangePasswordDialogShow));
        }
    }
    */
    /*
    public bool IsCurrentProfileSet
    {
        get
        {
            if (Profiles.Count > 0)
                return true;
            else
                return false;
        }
    }
    */

    private bool _isAlbumArtVisible = true;
    public bool IsAlbumArtVisible
    {
        get
        {
            return _isAlbumArtVisible;
        }
        set
        {
            if (_isAlbumArtVisible == value)
                return;

            _isAlbumArtVisible = value;
            OnPropertyChanged(nameof(IsAlbumArtVisible));
        }
    }

    private bool _isAlbumArtPanelIsOpen = false;
    public bool IsAlbumArtPanelIsOpen
    {
        get
        {
            return _isAlbumArtPanelIsOpen;
        }
        set
        {
            if (_isAlbumArtPanelIsOpen == value)
                return;

            _isAlbumArtPanelIsOpen = value;

            OnPropertyChanged(nameof(IsAlbumArtPanelIsOpen));
        }
    }

    /*
    private bool _isAlbumArtsDownLoaded;
    public bool IsAlbumArtsDownLoaded
    {
        get
        {
            return _isAlbumArtsDownLoaded;
        }
        set
        {
            if (_isAlbumArtsDownLoaded == value)
                return;
            _isAlbumArtsDownLoaded = value;
            OnPropertyChanged(nameof(IsAlbumArtsDownLoaded));
        }
    }
    */

    private bool _isBusy;
    public bool IsBusy
    {
        get
        {
            return _isBusy;
        }
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsProfileSwitchOK));

            //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            //Dispatcher.UIThread.Post(async () => { CommandManager.InvalidateRequerySuggested()});
        }
    }

    private bool _isWorking;
    public bool IsWorking
    {
        get
        {
            return _isWorking;
        }
        set
        {
            if (_isWorking == value)
                return;

            _isWorking = value;
            OnPropertyChanged(nameof(IsWorking));
            OnPropertyChanged(nameof(IsProfileSwitchOK));

            //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
        }
    }

    private bool _isShowInfoWindow;
    public bool IsShowInfoWindow

    {
        get { return _isShowInfoWindow; }
        set
        {
            if (_isShowInfoWindow == value)
                return;

            _isShowInfoWindow = value;

            if (!_isShowInfoWindow)
            {
                InfoBarInfoTitle = string.Empty;
                InfoBarInfoMessage = string.Empty;
            }

            OnPropertyChanged(nameof(IsShowInfoWindow));
        }
    }

    private bool _isShowAckWindow;
    public bool IsShowAckWindow

    {
        get { return _isShowAckWindow; }
        set
        {
            if (_isShowAckWindow == value)
                return;

            _isShowAckWindow = value;

            if (!_isShowAckWindow)
            {
                InfoBarAckTitle = string.Empty;
                InfoBarAckMessage = string.Empty;
            }

            OnPropertyChanged(nameof(IsShowAckWindow));
        }
    }

    private bool _isShowErrWindow;
    public bool IsShowErrWindow

    {
        get { return _isShowErrWindow; }
        set
        {
            if (_isShowErrWindow == value)
                return;

            _isShowErrWindow = value;

            if (!_isShowErrWindow)
            {
                InfoBarErrTitle = string.Empty;
                InfoBarErrMessage = string.Empty;
            }

            OnPropertyChanged(nameof(IsShowErrWindow));
        }
    }

    private bool _isShowDebugWindow;
    public bool IsShowDebugWindow

    {
        get { return _isShowDebugWindow; }
        set
        {
            if (_isShowDebugWindow == value)
                return;

            _isShowDebugWindow = value;

            OnPropertyChanged(nameof(IsShowDebugWindow));

            if (_isShowDebugWindow)
            {
                /*
                //Application.Current.Dispatcher.Invoke(() =>
                Dispatcher.UIThread.Post(() =>
                {
                    //DebugWindowShowHide?.Invoke
                    DebugWindowShowHide2?.Invoke(this, true);
                });
                */
            }
            else
            {
                /*
                //Application.Current.Dispatcher.Invoke(() =>
                Dispatcher.UIThread.Post(() =>
                {
                    //DebugWindowShowHide?.Invoke();
                    DebugWindowShowHide2?.Invoke(this, false);
                });
                */
            }
        }
    }

    private bool _isEnableDebugWindow;
    public bool IsEnableDebugWindow

    {
        get { return _isEnableDebugWindow; }
        set
        {
            if (_isEnableDebugWindow == value)
                return;

            _isEnableDebugWindow = value;

            OnPropertyChanged(nameof(IsEnableDebugWindow));
        }
    }

    #endregion

    #region == CurrentSong, Playback controls, AlbumArt ==  

    private SongInfoEx? _currentSong;
    public SongInfoEx? CurrentSong
    {
        get
        {
            return _currentSong;
        }
        set
        {
            if (_currentSong == value)
                return;

            _currentSong = value;
            OnPropertyChanged(nameof(CurrentSong));
            OnPropertyChanged(nameof(CurrentSongTitle));
            OnPropertyChanged(nameof(CurrentSongArtist));
            OnPropertyChanged(nameof(CurrentSongAlbum));
            OnPropertyChanged(nameof(IsCurrentSongArtistNotNull));
            OnPropertyChanged(nameof(IsCurrentSongAlbumNotNull));

            CurrentSongChanged?.Invoke(this, CurrentSongStringForWindowTitle);

            if (value is null)
            {
                _elapsedTimer.Stop();
                IsCurrentSongNotNull = false;
            }
            else
            {
                IsCurrentSongNotNull = true;
            }
        }
    }

    public string CurrentSongTitle
    {
        get
        {
            if (_currentSong is not null)
            {
                return _currentSong.Title;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public string CurrentSongArtist
    {
        get
        {
            if (_currentSong is not null)
            {
                if (!string.IsNullOrEmpty(_currentSong.Artist))
                    return _currentSong.Artist.Trim();
                else
                    return "";
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public string CurrentSongAlbum
    {
        get
        {
            if (_currentSong is not null)
            {
                if (!string.IsNullOrEmpty(_currentSong.Album))
                    return _currentSong.Album.Trim();
                else
                    return "";
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public bool IsCurrentSongArtistNotNull
    {
        get
        {
            if (_currentSong is not null)
            {
                if (!string.IsNullOrEmpty(_currentSong.Artist))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsCurrentSongAlbumNotNull
    {
        get
        {
            if (_currentSong is not null)
            {
                if (!string.IsNullOrEmpty(_currentSong.Album))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }

    private bool _isCurrentSongNotNull;
    public bool IsCurrentSongNotNull
    {
        get
        {
            return _isCurrentSongNotNull;
        }
        set
        {
            if (_isCurrentSongNotNull == value)
                return;

            _isCurrentSongNotNull = value;
            OnPropertyChanged(nameof(IsCurrentSongNotNull));
        }
    }

    public string CurrentSongStringForWindowTitle
    {
        get
        {
            if (_currentSong is not null)
            {
                string s = string.Empty;

                if (!string.IsNullOrEmpty(_currentSong.Title))
                {
                    s = _currentSong.Title.Trim();
                }

                if (!string.IsNullOrEmpty(_currentSong.Artist))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        s += " by ";
                    }
                    s += $"{_currentSong.Artist.Trim()}"; 
                }

                if (!string.IsNullOrEmpty(_currentSong.Album))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        s += " from ";
                    }
                    s += $"{_currentSong.Album.Trim()}";
                }

                return s;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    #region == Playback ==  

    private static readonly string _pathPlayButton = "M10.856 8.155A1.25 1.25 0 0 0 9 9.248v5.504a1.25 1.25 0 0 0 1.856 1.093l5.757-3.189a.75.75 0 0 0 0-1.312l-5.757-3.189ZM12 2C6.477 2 2 6.477 2 12s4.477 10 10 10 10-4.477 10-10S17.523 2 12 2ZM3.5 12a8.5 8.5 0 1 1 17 0 8.5 8.5 0 0 1-17 0Z";//"M2 12C2 6.477 6.477 2 12 2s10 4.477 10 10-4.477 10-10 10S2 17.523 2 12Zm8.856-3.845A1.25 1.25 0 0 0 9 9.248v5.504a1.25 1.25 0 0 0 1.856 1.093l5.757-3.189a.75.75 0 0 0 0-1.312l-5.757-3.189Z";//"M10,16.5V7.5L16,12M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
    private static readonly string _pathPauseButton = "M12 2C6.477 2 2 6.477 2 12s4.477 10 10 10 10-4.477 10-10S17.523 2 12 2Zm-1.5 6.25v7.5a.75.75 0 0 1-1.5 0v-7.5a.75.75 0 0 1 1.5 0Zm4.5 0v7.5a.75.75 0 0 1-1.5 0v-7.5a.75.75 0 0 1 1.5 0Z";
    //private static string _pathStopButton = "M10,16.5V7.5L16,12M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
    private string _playButton = _pathPlayButton;
    public string PlayButton
    {
        get
        {
            return _playButton;
        }
        set
        {
            if (_playButton == value)
                return;

            _playButton = value;
            OnPropertyChanged(nameof(PlayButton));
        }
    }

    private double _volume = 20;
    public double Volume
    {
        get => _volume;
        set
        {
            if (_volume == value) return;
            _volume = value;
            OnPropertyChanged(nameof(Volume));

            //if (Convert.ToDouble(_mpc.MpdStatus.MpdVolume) == _volume) return;
            
            // If we have a timer and we are in this event handler, a user is still interact with the slider
            // we stop the timer
            _volumeDelayTimer?.Stop();

            //System.Diagnostics.Debug.WriteLine("Volume value is still changing. Skipping.");

            // we always create a new instance of DispatcherTimer
            _volumeDelayTimer = new System.Timers.Timer
            {
                AutoReset = false,

                // if one second passes, that means our user has stopped interacting with the slider
                // we do real event
                Interval = (double)1000
            };
            _volumeDelayTimer.Elapsed += new System.Timers.ElapsedEventHandler(DoChangeVolume);

            _volumeDelayTimer.Start();
        }
    }

    private System.Timers.Timer? _volumeDelayTimer;
    private async void DoChangeVolume(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_mpc is not null)
        {
            await _mpc.MpdSetVolume(Convert.ToInt32(_volume));
        }
    }

    private bool _repeat;
    public bool Repeat
    {
        get => _repeat;
        set
        {
            _repeat = value;
            OnPropertyChanged(nameof(Repeat));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdRepeat != value)
                {
                    _ = SetRpeat();
                }
            }
        }
    }

    private bool _random;
    public bool Random
    {
        get { return _random; }
        set
        {
            _random = value;
            OnPropertyChanged(nameof(Random));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdRandom != value)
                {
                    _ = SetRandom();
                }
            }
        }
    }

    private bool _consume;
    public bool Consume
    {
        get { return _consume; }
        set
        {
            _consume = value;
            OnPropertyChanged(nameof(Consume));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdConsume != value)
                {
                    _ = SetConsume();
                }
            }
        }
    }

    private bool _single;
    public bool Single
    {
        get { return _single; }
        set
        {
            _single = value;
            OnPropertyChanged(nameof(Single));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdSingle != value)
                {
                    _ = SetSingle();
                }
            }
        }
    }

    private int _time = 0;
    public int Time
    {
        get
        {
            return _time;
        }
        set
        {
            if (_time == value) 
                return;

            _time = value;
            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(TimeFormatted));
        }
    }

    public string TimeFormatted
    {
        get
        {
            int sec, min, hour, s;

            sec = Time / _elapsedTimeMultiplier;

            min = sec / 60;
            s = sec % 60;
            hour = min / 60;
            min %= 60;
            /*
            if ((hour == 0) && min == 0)
            {
                _timeFormatted = String.Format("{0}", s);
            }
            else if ((hour == 0) && (min != 0))
            {
                _timeFormatted = String.Format("{0}:{1:00}", min, s);
            }
            else if ((hour != 0) && (min != 0))
            {
                _timeFormatted = String.Format("{0}:{1:00}:{2:00}", hour, min, s);
            }
            else if (hour != 0)
            {
                _timeFormatted = String.Format("{0}:{1:00}:{2:00}", hour, min, s);
            }
            */
            return string.Format("{0}:{1:00}:{2:00}", hour, min, s);
        }
    }

    private readonly int _elapsedTimeMultiplier = 1;// or 10
    private int _elapsed = 0;
    public int Elapsed
    {
        get
        {
            return _elapsed;
        }
        set
        {
            if ((value < _time) && _elapsed != value)
            {
                _elapsed = value;
                Dispatcher.UIThread.Post(() =>
                {
                    OnPropertyChanged(nameof(Elapsed));
                    OnPropertyChanged(nameof(ElapsedFormatted));
                });
                // If we have a timer and we are in this event handler, a user is still interact with the slider
                // we stop the timer
                _elapsedDelayTimer?.Stop();

                //System.Diagnostics.Debug.WriteLine("Elapsed value is still changing. Skipping.");

                // we always create a new instance of DispatcherTimer
                _elapsedDelayTimer = new System.Timers.Timer
                {
                    AutoReset = false,

                    // if one second passes, that means our user has stopped interacting with the slider
                    // we do real event
                    Interval = (double)1000
                };
                _elapsedDelayTimer.Elapsed += new System.Timers.ElapsedEventHandler(DoChangeElapsed);

                _elapsedDelayTimer.Start();
            }
        }
    }

    public string ElapsedFormatted
    {
        get
        {
            int sec, min, hour, s;

            sec = _elapsed / _elapsedTimeMultiplier;

            min = sec / 60;
            s = sec % 60;
            hour = min / 60;
            min %= 60;
            /*
            if ((hour == 0) && min == 0)
            {
                _elapsedFormatted = String.Format("{0}", s);
            }
            else if ((hour == 0) && (min != 0))
            {
                _elapsedFormatted = String.Format("{0}:{1:00}", min, s);
            }
            else if ((hour != 0) && (min != 0))
            {
                _elapsedFormatted = String.Format("{0}:{1:00}:{2:00}", hour, min, s);
            }
            else if (hour != 0)
            {
                _elapsedFormatted = String.Format("{0}:{1:00}:{2:00}", hour, min, s);
            }
            */
            return string.Format("{0}:{1:00}:{2:00}", hour, min, s);
        }
    }

    private System.Timers.Timer? _elapsedDelayTimer = null;
    private void DoChangeElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_mpc is not null && (_elapsed < _time))
        {
            _ = SetSeek();
        }
    }

    #endregion

    #region == AlbumArt == 

    private AlbumImage? _albumCover;
    public AlbumImage? AlbumCover
    {
        get
        {
            return _albumCover;
        }
        set
        {
            if (_albumCover == value)
                return;
            _albumCover = value;

            if (_albumCover is null)
            { 
                //IsAlbumArtVisible = false; 
            }
            else
            {
                if (!IsAlbumArtPanelIsOpen)
                {
                    //IsAlbumArtVisible = true;
                }
            }

            OnPropertyChanged(nameof(AlbumCover));
        }
    }

    private readonly Bitmap? _albumArtBitmapSourceDefault = null;
    private Bitmap? _albumArtBitmapSource;
    public Bitmap? AlbumArtBitmapSource
    {
        get
        {
            return _albumArtBitmapSource;
        }
        set
        {
            if (_albumArtBitmapSource == value)
                return;

            _albumArtBitmapSource = value;
            OnPropertyChanged(nameof(AlbumArtBitmapSource));
        }
    }

    #endregion

    #endregion

    #region == NavigationView/TreeView Menu (Queue, Files, Search, Playlists, Playlist) ==

    #region == MenuTree ==

    private readonly MenuTreeBuilder _mainMenuItems = new("");
    public ObservableCollection<NodeTree> MainMenuItems
    {
        get { return _mainMenuItems.Children; }
        set
        {
            _mainMenuItems.Children = value;
            OnPropertyChanged(nameof(MainMenuItems));
        }
    }

    private NodeTree? _selectedNodeMenu = new NodeMenu("root");
    public NodeTree? SelectedNodeMenu
    {
        get { return _selectedNodeMenu; }
        set
        {
            if (_selectedNodeMenu == value)
                return;

            if (value is null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SelectedPlaylistName = string.Empty;
                    RenamedSelectPendingPlaylistName = string.Empty;
                    PlaylistSongs.Clear();
                });

                return;
            }

            if (value is NodeMenuQueue)
            {
                CurrentPage = App.GetService<QueuePage>();
            }
            else if (value is NodeMenuSearch)
            {
                CurrentPage = App.GetService<SearchPage>();
            }
            /*
            else if (value is NodeMenuLibrary)
            {
                // Do nothing.
            }
            */
            else if (value is NodeMenuArtist)
            {
                CurrentPage = App.GetService<ArtistPage>();

                if ((Artists.Count > 0) && (SelectedAlbumArtist is null))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        SelectedAlbumArtist = Artists[0];
                    });
                }
                // Get album pictures.
                /*
                if (!IsAlbumArtsDownLoaded)
                {
                    GetAlbumPictures(_albums);
                }
                */
            }
            else if (value is NodeMenuAlbum nmb)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    //await Task.Yield();
                    await Task.Delay(10);

                    CurrentPage = App.GetService<AlbumPage>();

                    await Task.Delay(20);
                    IsWorking = false;
                });
            }
            else if (value is NodeMenuFiles nml)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();

                    CurrentPage = App.GetService<FilesPage>();

                    if (!nml.IsAcquired || (MusicDirectories.Count <= 1) && (MusicEntries.Count == 0))
                    {
                        GetFiles(nml);
                    }

                    await Task.Yield();
                    IsWorking = false;
                });
            }
            else if (value is NodeMenuPlaylists)
            {
                // Do nothing
                //CurrentPage = App.GetService<PlaylistsPage>();
            }
            else if (value is NodeMenuPlaylistItem nmpli)
            {
                CurrentPage = App.GetService<PlaylistItemPage>();

                SelectedPlaylistSong = null;
                PlaylistSongs = nmpli.PlaylistSongs;
                SelectedPlaylistName = nmpli.Name;

                if ((nmpli.PlaylistSongs.Count == 0) || nmpli.IsUpdateRequied)
                {
                    GetPlaylistSongs(nmpli);
                }
            }
            else if (value is NodeMenu)
            {
                if (value.Name != "root")
                    throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }

            _selectedNodeMenu = value;
            OnPropertyChanged(nameof(SelectedNodeMenu));
        }
    }

    private UserControl? _currentpage;
    public UserControl? CurrentPage
    {
        get { return _currentpage; }
        set
        {
            /*
            if (SetProperty(ref _currentpage, value))
            {
                //
            }
            */

            if (_currentpage == value)
                return;

            _currentpage = value;
            this.OnPropertyChanged(nameof(CurrentPage));
        }
    }

    private string _selectedPlaylistName = "";
    public string SelectedPlaylistName
    {
        get
        {
            return _selectedPlaylistName;
        }
        set
        {
            if (_selectedPlaylistName == value)
                return;

            _selectedPlaylistName = value;
            OnPropertyChanged(nameof(SelectedPlaylistName));
        }
    }

    private string _renamedSelectPendingPlaylistName = "";
    public string RenamedSelectPendingPlaylistName
    {
        get
        {
            return _renamedSelectPendingPlaylistName;
        }
        set
        {
            if (_renamedSelectPendingPlaylistName == value)
                return;

            _renamedSelectPendingPlaylistName = value;
            OnPropertyChanged(nameof(RenamedSelectPendingPlaylistName));
        }
    }

    #endregion

    #region == Queue ==  

    private ObservableCollection<SongInfoEx> _queue = [];
    public ObservableCollection<SongInfoEx> Queue
    {
        get
        {
            if (_mpc is not null)
            {
                return _queue;
                //return _mpc.CurrentQueue;
            }
            else
            {
                return _queue;
            }
        }
        set
        {
            if (_queue == value)
                return;

            _queue = value;
            OnPropertyChanged(nameof(Queue));
            OnPropertyChanged(nameof(QueuePageSubTitleSongCount));
        }
    }

    private SongInfoEx? _selectedQueueSong;
    public SongInfoEx? SelectedQueueSong
    {
        get
        {
            return _selectedQueueSong;
        }
        set
        {
            if (_selectedQueueSong == value)
                return;

            _selectedQueueSong = value;
            OnPropertyChanged(nameof(SelectedQueueSong));
        }
    }

    private bool _isQueueFindVisible;
    public bool IsQueueFindVisible
    {
        get
        {
            return _isQueueFindVisible;
        }
        set
        {
            if (_isQueueFindVisible == value)
                return;

            _isQueueFindVisible = value;

            QueueForFilter.Clear();
            /*
            var collectionView = CollectionViewSource.GetDefaultView(QueueForFilter);
            collectionView.Filter = x =>
            {
                return false;
            };
            */
            FilterQueueQuery = "";

            OnPropertyChanged(nameof(IsQueueFindVisible));
        }
    }

    private bool FilterSongInfoEx(SongInfoEx song)
    {
        return song.Title.Contains(FilterQueueQuery, StringComparison.CurrentCultureIgnoreCase);// InvariantCultureIgnoreCase
    }

    private ObservableCollection<SongInfoEx> _queueForFilter = [];
    public ObservableCollection<SongInfoEx> QueueForFilter
    {
        get
        {
            return _queueForFilter;
        }
        set
        {
            if (_queueForFilter == value)
                return;

            _queueForFilter = value;
            OnPropertyChanged(nameof(QueueForFilter));
        }
    }

    private SearchTags _selectedQueueFilterTags = SearchTags.Title;
    public SearchTags SelectedQueueFilterTags
    {
        get
        {
            return _selectedQueueFilterTags;
        }
        set
        {
            if (_selectedQueueFilterTags == value)
                return;

            _selectedQueueFilterTags = value;
            OnPropertyChanged(nameof(SelectedQueueFilterTags));

            if (_filterQueueQuery == "")
                return;
            /*
            var collectionView = CollectionViewSource.GetDefaultView(_queueForFilter);
            collectionView.Filter = x =>
            {
                var entry = (SongInfoEx)x;

                if (SelectedQueueFilterTags == SearchTags.Title)
                {
                    return entry.Title.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (SelectedQueueFilterTags == SearchTags.Artist)
                {
                    return entry.Artist.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (SelectedQueueFilterTags == SearchTags.Album)
                {
                    return entry.Album.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (SelectedQueueFilterTags == SearchTags.Genre)
                {
                    return entry.Genre.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    return false;
                }
            };
            */
        }
    }

    private string _filterQueueQuery = "";
    public string FilterQueueQuery
    {
        get
        {
            return _filterQueueQuery;
        }
        set
        {
            if (_filterQueueQuery == value)
                return;

            _filterQueueQuery = value;
            OnPropertyChanged(nameof(FilterQueueQuery));

            if (_filterQueueQuery == "")
            {
                return;
            }

            var filtered = Queue.Where(song => FilterSongInfoEx(song));
            QueueForFilter = new ObservableCollection<SongInfoEx>(filtered);
            /*
            var collectionView = CollectionViewSource.GetDefaultView(_queueForFilter);
            collectionView.Filter = x =>
            {
                if (_filterQueueQuery == "")
                {
                    return false;
                }
                else
                {
                    var entry = (SongInfoEx)x;

                    if (SelectedQueueFilterTags == SearchTags.Title)
                    {
                        return entry.Title.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else if (SelectedQueueFilterTags == SearchTags.Artist)
                    {
                        return entry.Artist.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else if (SelectedQueueFilterTags == SearchTags.Album)
                    {
                        return entry.Album.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else if (SelectedQueueFilterTags == SearchTags.Genre)
                    {
                        return entry.Genre.Contains(_filterQueueQuery, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        return false;
                    }
                }
            };
            */
            //collectionView.Refresh();
        }
    }

    private SongInfoEx? _selectedQueueFilterSong;
    public SongInfoEx? SelectedQueueFilterSong
    {
        get
        {
            return _selectedQueueFilterSong;
        }
        set
        {
            if (_selectedQueueFilterSong == value)
                return;

            _selectedQueueFilterSong = value;
            OnPropertyChanged(nameof(SelectedQueueFilterSong));
        }
    }

    private string _queuePageSubTitleSongCount = "";
    public string QueuePageSubTitleSongCount
    {
        get 
        {
            _queuePageSubTitleSongCount = string.Format(MPDCtrlX.Properties.Resources.QueuePage_SubTitle_SongCount, Queue.Count);
            return _queuePageSubTitleSongCount;
        }
    }

    #endregion

    #region == Files ==

    private readonly DirectoryTreeBuilder _musicDirectories = new("");
    public ObservableCollection<NodeTree> MusicDirectories
    {
        get { return _musicDirectories.Children; }
        set
        {
            _musicDirectories.Children = value;
            OnPropertyChanged(nameof(MusicDirectories));
        }
    }

    private NodeDirectory _selectedNodeDirectory = new(".", new Uri(@"file:///./"));
    public NodeDirectory SelectedNodeDirectory
    {
        get { return _selectedNodeDirectory; }
        set
        {
            if (_selectedNodeDirectory == value)
                return;

            _selectedNodeDirectory = value;
            OnPropertyChanged(nameof(SelectedNodeDirectory));

            if (_selectedNodeDirectory is null)
                return;

            if (MusicEntries is null)
                return;
            if (MusicEntries.Count == 0)
                return;

            if (_selectedNodeDirectory.DireUri.LocalPath == "/")
            {
                if (FilterMusicEntriesQuery != "")
                {
                    var filtered = _musicEntries.Where(song => song.Name.Contains(FilterMusicEntriesQuery, StringComparison.InvariantCultureIgnoreCase));
                    _musicEntriesFiltered = new ObservableCollection<NodeFile>(filtered);
                }
                else
                {
                    _musicEntriesFiltered = new ObservableCollection<NodeFile>(_musicEntries);
                }
            }
            else
            {
                FilterFiles();
            }

            OnPropertyChanged(nameof(MusicEntriesFiltered));
            
            /*
            bool filteringMode = true;
            
            var collectionView = CollectionViewSource.GetDefaultView(MusicEntries);
            if (collectionView is null)
                return;

            try
            {
                collectionView.Filter = x =>
                {
                    var entry = (NodeFile)x;

                    if (entry is null)
                        return false;

                    if (entry.FileUri is null)
                        return false;

                    string path = entry.FileUri.LocalPath; //person.FileUri.AbsoluteUri;
                    if (string.IsNullOrEmpty(path))
                        return false;
                    string filename = System.IO.Path.GetFileName(path);//System.IO.Path.GetFileName(uri.LocalPath);
                    if (string.IsNullOrEmpty(filename))
                        return false;

                    if ((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath == "/")
                    {
                        if (filteringMode)
                        {
                            if (!string.IsNullOrEmpty(FilterMusicEntriesQuery))
                            {
                                return (filename.Contains(FilterMusicEntriesQuery, StringComparison.CurrentCultureIgnoreCase));
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // Only the matched(in the folder) items
                            path = path.Replace("/", "");

                            if (!string.IsNullOrEmpty(FilterMusicEntriesQuery))
                            {
                                return ((path == filename) && filename.Contains(FilterMusicEntriesQuery, StringComparison.CurrentCultureIgnoreCase));
                            }
                            else
                            {
                                return (path == filename);
                            }
                        }
                    }
                    else
                    {
                        path = path.Replace(("/" + filename), "");

                        if (filteringMode)
                        {
                            // testing (adding "/")
                            path += "/";

                            if (!string.IsNullOrEmpty(FilterMusicEntriesQuery))
                            {
                                // testing (adding "/")
                                return (path.StartsWith((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath + "/") && filename.Contains(FilterMusicEntriesQuery, StringComparison.CurrentCultureIgnoreCase));
                            }
                            else
                            {
                                // This is not enough. eg. "/Hoge/Hoge" and /Hoge/Hoge2
                                //return (path.StartsWith((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath));

                                // testing (adding "/")
                                return (path.StartsWith((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath + "/"));
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(FilterMusicEntriesQuery))
                            {
                                return (path.StartsWith((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath) && filename.Contains(FilterMusicEntriesQuery, StringComparison.CurrentCultureIgnoreCase));
                            }
                            else
                            {
                                return (path == (_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath);
                            }
                        }
                    }
                };

            }
            catch (Exception e)
            {
                Debug.WriteLine("collectionView.Filter = x => " + e.Message);

                //Application.Current?.Dispatcher.Invoke(() => { (Application.Current as App)?.AppendErrorLog("Exception@SelectedNodeDirectory collectionView.Filter = x =>", e.Message); });
                Dispatcher.UIThread.Post(async () => { (Application.Current as App)?.AppendErrorLog("Exception@SelectedNodeDirectory collectionView.Filter = x =>", e.Message); });
            }
            */
        }
    }

    private ObservableCollection<NodeFile> _musicEntries = [];
    public ObservableCollection<NodeFile> MusicEntries
    {
        get
        {
            return _musicEntries;
        }
        set
        {
            if (value == _musicEntries)
                return;

            _musicEntries = value;
            OnPropertyChanged(nameof(MusicEntries));
            OnPropertyChanged(nameof(FilesPageSubTitleFileCount));
        }
    }

    private ObservableCollection<NodeFile> _musicEntriesFiltered = [];
    public ObservableCollection<NodeFile> MusicEntriesFiltered
    {
        get
        {
            return _musicEntriesFiltered;
        }
        set
        {
            if (_musicEntriesFiltered == value)
                return;

            _musicEntriesFiltered = value;
            OnPropertyChanged(nameof(MusicEntriesFiltered));
        }
    }

    private void FilterFiles()
    {
        _musicEntriesFiltered.Clear();

        foreach (var entry in _musicEntries)
        {
            string path = entry.FileUri.LocalPath; //person.FileUri.AbsoluteUri;
            if (string.IsNullOrEmpty(path))
                continue;
            string filename = System.IO.Path.GetFileName(path);//System.IO.Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrEmpty(filename))
                continue;

            path = path.Replace(("/" + filename), "");

            if (path.StartsWith(_selectedNodeDirectory.DireUri.LocalPath))
            {
                if (FilterMusicEntriesQuery != "")
                {
                    if (entry.Name.Contains(FilterMusicEntriesQuery, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _musicEntriesFiltered.Add(entry);
                    }
                }
                else
                {
                    _musicEntriesFiltered.Add(entry);
                }
            }
        }
    }

    private string _filterMusicEntriesQuery = "";
    public string FilterMusicEntriesQuery
    {
        get
        {
            return _filterMusicEntriesQuery;
        }
        set
        {
            if (_filterMusicEntriesQuery == value)
                return;

            _filterMusicEntriesQuery = value;
            OnPropertyChanged(nameof(FilterMusicEntriesQuery));

            if (_selectedNodeDirectory is null)
                return;

            if ((_selectedNodeDirectory as NodeDirectory).DireUri.LocalPath == "/")
            {
                if (FilterMusicEntriesQuery != "")
                {
                    var filtered = _musicEntries.Where(song => song.Name.Contains(FilterMusicEntriesQuery, StringComparison.InvariantCultureIgnoreCase));
                    MusicEntriesFiltered = new ObservableCollection<NodeFile>(filtered);
                }
                else
                {
                    MusicEntriesFiltered = new ObservableCollection<NodeFile>(_musicEntries);
                }
            }
            else
            {
                FilterFiles();
                OnPropertyChanged(nameof(MusicEntriesFiltered));
            }
        }
    }

    private string _filesPageSubTitleFileCount = "";
    public string FilesPageSubTitleFileCount
    {
        get
        {
            _filesPageSubTitleFileCount = string.Format(MPDCtrlX.Properties.Resources.FilesPage_SubTitle_FileCount, MusicEntries.Count);
            return _filesPageSubTitleFileCount;
        }
    }

    #endregion

    #region == Artists ==

    private ObservableCollection<AlbumArtist> _artists = [];
    public ObservableCollection<AlbumArtist> Artists
    {
        get { return _artists; }
        set
        {
            if (_artists == value)
                return;

            _artists = value;
            OnPropertyChanged(nameof(Artists));
            OnPropertyChanged(nameof(ArtistPageSubTitleArtistCount));
        }
    }

    private string _artistPageSubTitleArtistCount = "";
    public string ArtistPageSubTitleArtistCount
    {
        get
        {
            _artistPageSubTitleArtistCount = string.Format(MPDCtrlX.Properties.Resources.ArtistPage_SubTitle_ArtistCount, Artists.Count);
            return _artistPageSubTitleArtistCount;
        }
    }

    private AlbumArtist? _selectedAlbumArtist;
    public AlbumArtist? SelectedAlbumArtist
    {
        get { return _selectedAlbumArtist; }
        set
        {
            if (_selectedAlbumArtist != value)
            {
                _selectedAlbumArtist = value;

                OnPropertyChanged(nameof(SelectedAlbumArtist));

                SelectedArtistAlbums = _selectedAlbumArtist?.Albums;
               
                Task.Run(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();

                    GetArtistSongs(_selectedAlbumArtist);
                    GetAlbumPictures(SelectedArtistAlbums);

                    IsWorking = false;
                    await Task.Yield();
                });
            }
        }
    }

    private ObservableCollection<Album>? _selectedArtistAlbums = [];
    public ObservableCollection<Album>? SelectedArtistAlbums
    {
        get 
        {
            return _selectedArtistAlbums;
        }
        set
        {
            if (_selectedArtistAlbums == value)
                return;

            _selectedArtistAlbums = value;
            OnPropertyChanged(nameof(SelectedArtistAlbums));
        }
    }

    #endregion

    #region == Albums ==

    private ObservableCollection<AlbumEx> _albums = [];
    public ObservableCollection<AlbumEx> Albums
    {
        get { return _albums; }
        set
        {
            if (_albums == value)
                return;

            _albums = value;
            OnPropertyChanged(nameof(Albums));
            OnPropertyChanged(nameof(AlbumPageSubTitleAlbumCount));
        }
    }

    private bool _isAlbumContentPanelVisible = false;
    public bool IsAlbumContentPanelVisible
    {
        get { return _isAlbumContentPanelVisible; }
        set
        {
            if (_isAlbumContentPanelVisible == value)
                return;

            _isAlbumContentPanelVisible = value;
            OnPropertyChanged(nameof(IsAlbumContentPanelVisible));
        }
    }

    private AlbumEx? _selectedAlbum = new();
    public AlbumEx? SelectedAlbum
    {
        get { return _selectedAlbum; }
        set
        {
            if (_selectedAlbum == value)
                return;

            _selectedAlbum = value;
            OnPropertyChanged(nameof(SelectedAlbum));
            OnPropertyChanged(nameof(SelectedAlbumSongs));
        }
    }

    private ObservableCollection<SongInfo> _selectedAlbumSongs = [];
    public ObservableCollection<SongInfo> SelectedAlbumSongs
    {
        get
        {
            if (_selectedAlbum is not null)
            {
                return _selectedAlbum.Songs;
            }

            return _selectedAlbumSongs;
        }
        set
        {
            if (_selectedAlbumSongs == value)
                return;

            _selectedAlbumSongs = value;
            OnPropertyChanged(nameof(SelectedAlbumSongs));
        }
    }

    private string _albumPageSubTitleAlbumCount = "";
    public string AlbumPageSubTitleAlbumCount
    {
        get
        {
            _albumPageSubTitleAlbumCount = string.Format(MPDCtrlX.Properties.Resources.AlbumPage_SubTitle_AlbumCount, Albums.Count);
            return _albumPageSubTitleAlbumCount;
        }
    }

    private IEnumerable<object>? _visibleViewportItemsAlbumEx;
    public IEnumerable<object>? VisibleViewportItemsAlbumEx
    {
        get => _visibleViewportItemsAlbumEx;
        set
        {
            _visibleViewportItemsAlbumEx = value;

            //OnPropertyChanged(nameof(VisibleViewportItemsAlbumEx));

            if (VisibleViewportItemsAlbumEx is null)
            {
                return;
            }

            _ = Task.Run(() => GetAlbumPictures(VisibleViewportItemsAlbumEx));
            //GetAlbumPictures(VisibleViewportItemsAlbumEx);
        }
    }

    #endregion

    #region == Search ==

    private ObservableCollection<SongInfo>? _searchResult = [];
    public ObservableCollection<SongInfo>? SearchResult
    {
        get
        {
            return _searchResult;
        }
        set
        {
            if (_searchResult == value)
                return;

            _searchResult = value;
            OnPropertyChanged(nameof(SearchResult));
            OnPropertyChanged(nameof(SearchPageSubTitleResultCount));
        }
    }

    // Search Tags
    private readonly ObservableCollection<Models.SearchOption> _searchTagList = 
    [
        new Models.SearchOption(SearchTags.Title, Properties.Resources.ListviewColumnHeader_Title),
        new Models.SearchOption(SearchTags.Artist, Properties.Resources.ListviewColumnHeader_Artist),
        new Models.SearchOption(SearchTags.Album, Properties.Resources.ListviewColumnHeader_Album),
        new Models.SearchOption(SearchTags.Genre, Properties.Resources.ListviewColumnHeader_Genre),
        new Models.SearchOption(SearchTags.Any, Properties.Resources.SearchOption_Any)
    ];

    public ObservableCollection<Models.SearchOption> SearchTagList
    {
        get
        {
            return _searchTagList;
        }
    }

    private Models.SearchOption _selectedSearchTag = new(SearchTags.Title, Properties.Resources.ListviewColumnHeader_Title);
    public Models.SearchOption SelectedSearchTag
    {
        get
        {
            return _selectedSearchTag;
        }
        set
        {
            if (_selectedSearchTag == value)
                return;

            _selectedSearchTag = value;
            OnPropertyChanged(nameof(SelectedSearchTag));
        }
    }

    // Search Shiki (contain/==)
    private readonly ObservableCollection<Models.SearchWith> _searchShikiList =
[
    new Models.SearchWith(SearchShiki.Contains, Properties.Resources.Search_Shiki_Contains),
        new Models.SearchWith(SearchShiki.Equals, Properties.Resources.Search_Shiki_Equals)
];

    public ObservableCollection<Models.SearchWith> SearchShikiList
    {
        get
        {
            return _searchShikiList;
        }
    }

    private Models.SearchWith _selectedSearchShiki = new(SearchShiki.Contains, "Contains");
    public Models.SearchWith SelectedSearchShiki
    {
        get
        {
            return _selectedSearchShiki;
        }
        set
        {
            if (_selectedSearchShiki == value)
                return;

            _selectedSearchShiki = value;
            OnPropertyChanged(nameof(SelectedSearchShiki));
        }
    }

    // 
    private string _searchQuery = "";
    public string SearchQuery
    {
        get
        {
            return _searchQuery;
        }
        set
        {
            if (_searchQuery == value)
                return;

            _searchQuery = value;
            OnPropertyChanged(nameof(SearchQuery));
            //SearchExecCommand.NotifyCanExecuteChanged();
        }
    }

    private string _searchPageSubTitleResultCount = "";
    public string SearchPageSubTitleResultCount
    {
        get
        {
            _searchPageSubTitleResultCount = string.Format(MPDCtrlX.Properties.Resources.SearchPage_SubTitle_ResultCount, SearchResult?.Count);
            return _searchPageSubTitleResultCount;
        }
    }

    #endregion

    #region == Playlists ==  

    private ObservableCollection<Playlist> _playlists = [];
    public ObservableCollection<Playlist> Playlists
    {
        get
        {
            return _playlists;
        }
        set
        {
            if (_playlists == value)
                return;

            _playlists = value;
            OnPropertyChanged(nameof(Playlists));
        }
    }
    /*
    private Playlist? _selectedPlaylist;
    public Playlist? SelectedPlaylist
    {
        get
        {
            return _selectedPlaylist;
        }
        set
        {
            if (_selectedPlaylist != value)
            {
                _selectedPlaylist = value;
                OnPropertyChanged(nameof(SelectedPlaylist));
            }
        }
    }
    */
    #endregion

    #region == Playlist Items ==

    private ObservableCollection<SongInfo> _playlistSongs = [];
    public ObservableCollection<SongInfo> PlaylistSongs
    {
        get
        {
            return _playlistSongs;
        }
        set
        {
            if (_playlistSongs != value)
            {
                _playlistSongs = value;
                OnPropertyChanged(nameof(PlaylistSongs));
                OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));
            }
        }
    }

    private SongInfo? _selectedPlaylistSong;
    public SongInfo? SelectedPlaylistSong
    {
        get
        {
            return _selectedPlaylistSong;
        }
        set
        {
            if (_selectedPlaylistSong != value)
            {
                _selectedPlaylistSong = value;
                OnPropertyChanged(nameof(SelectedPlaylistSong));
            }
        }
    }

    private string _playlistPageSubTitleSongCount = "";
    public string PlaylistPageSubTitleSongCount
    {
        get
        {
            _playlistPageSubTitleSongCount = string.Format(MPDCtrlX.Properties.Resources.PlaylistPage_SubTitle_SongCount, PlaylistSongs.Count);
            return _playlistPageSubTitleSongCount;
        }
    }

    #endregion

    #region == Settings ==

    public static string AlbumCacheFolderPath
    {
        get => App.AppDataCacheFolder;
    }

    private string _albumCacheFolderSizeFormatted = string.Empty;

    public string AlbumCacheFolderSizeFormatted
    {
        get => _albumCacheFolderSizeFormatted;
        set
        {
            if (_albumCacheFolderSizeFormatted == value)
                return;

            _albumCacheFolderSizeFormatted = value;
            OnPropertyChanged(nameof(AlbumCacheFolderSizeFormatted));
        }
    }

    #endregion

    #endregion

    #region == Debug ==

    private string _debugCommandText = string.Empty;
    public string DebugCommandText
    {
        get
        {
            return _debugCommandText;
        }
        set
        {
            if (_debugCommandText == value)
                return;

            _debugCommandText = value;
            OnPropertyChanged(nameof(DebugCommandText));
        }
    }


    private string _debugIdleText = string.Empty;
    public string DebugIdleText
    {
        get
        {
            return _debugIdleText;
        }
        set
        {
            if (_debugIdleText == value)
                return;

            _debugIdleText = value;
            OnPropertyChanged(nameof(DebugIdleText));
        }
    }

    #endregion

    #region == Options ==

    private bool _isUpdateOnStartup = true;
    public bool IsUpdateOnStartup
    {
        get { return _isUpdateOnStartup; }
        set
        {
            if (_isUpdateOnStartup == value)
                return;

            _isUpdateOnStartup = value;

            OnPropertyChanged(nameof(IsUpdateOnStartup));
        }
    }

    private bool _isAutoScrollToNowPlaying = false;
    public bool IsAutoScrollToNowPlaying
    {
        get { return _isAutoScrollToNowPlaying; }
        set
        {
            if (_isAutoScrollToNowPlaying == value)
                return;

            _isAutoScrollToNowPlaying = value;

            OnPropertyChanged(nameof(IsAutoScrollToNowPlaying));
        }
    }

    private bool _isSaveLog;
    public bool IsSaveLog
    {
        get { return _isSaveLog; }
        set
        {
            if (_isSaveLog == value)
                return;

            _isSaveLog = value;

            OnPropertyChanged(nameof(IsSaveLog));
        }
    }

    private bool _isDownloadAlbumArt = true;
    public bool IsDownloadAlbumArt
    {
        get { return _isDownloadAlbumArt; }
        set
        {
            if (_isDownloadAlbumArt == value)
                return;

            _isDownloadAlbumArt = value;

            OnPropertyChanged(nameof(IsDownloadAlbumArt));
        }
    }

    private bool _isDownloadAlbumArtEmbeddedUsingReadPicture = true;
    public bool IsDownloadAlbumArtEmbeddedUsingReadPicture
    {
        get { return _isDownloadAlbumArtEmbeddedUsingReadPicture; }
        set
        {
            if (_isDownloadAlbumArtEmbeddedUsingReadPicture == value)
                return;

            _isDownloadAlbumArtEmbeddedUsingReadPicture = value;

            OnPropertyChanged(nameof(IsDownloadAlbumArtEmbeddedUsingReadPicture));
        }
    }

    #endregion

    #region == Profile settings ==

    private readonly ObservableCollection<Profile> _profiles = [];
    public ObservableCollection<Profile> Profiles
    {
        get { return _profiles; }
    }

    private Profile? _currentProfile;
    public Profile? CurrentProfile
    {
        get { return _currentProfile; }
        set
        {
            if (_currentProfile == value)
                return;

            _currentProfile = value;
            OnPropertyChanged(nameof(CurrentProfile));

            SelectedProfile = _currentProfile;

            if (_currentProfile is not null)
            {
                _volume = _currentProfile.Volume;
                OnPropertyChanged(nameof(Volume));

                Host = _currentProfile.Host;
                Port = _currentProfile.Port.ToString();
                _password = _currentProfile.Password;
                OnPropertyChanged(nameof(Password));
            }
        }
    }

    private Profile? _selectedProfile;
    public Profile? SelectedProfile
    {
        get
        {
            return _selectedProfile;
        }
        set
        {
            if (_selectedProfile == value)
                return;

            _selectedProfile = value;

            OnPropertyChanged(nameof(SelectedProfile));
        }
    }

    private bool _setIsDefault = true;
    public bool SetIsDefault
    {
        get { return _setIsDefault; }
        set
        {
            if (_setIsDefault == value)
                return;

            _setIsDefault = value;

            OnPropertyChanged(nameof(SetIsDefault));
        }
    }
    /*
    private Profile? _selectedProfile;
    public Profile? SelectedProfile
    {
        get
        {
            return _selectedProfile;
        }
        set
        {
            if (_selectedProfile == value)
                return;

            _selectedProfile = value;

            if (_selectedProfile is not null)
            {
                //ClearError(nameof(Host));
                //ClearError(nameof(Port));
                Host = _selectedProfile.Host;
                Port = _selectedProfile.Port.ToString();
                Password = _selectedProfile.Password;
                SetIsDefault = _selectedProfile.IsDefault;
            }
            else
            {
                //ClearError(nameof(Host));
                //ClearError(nameof(Port));
                Host = "";
                Port = "6600";
                Password = "";
            }

            OnPropertyChanged(nameof(SelectedProfile));

            // "quietly"
            if (_selectedProfile is not null)
            {
                _selectedQuickProfile = _selectedProfile;
                OnPropertyChanged(nameof(SelectedQuickProfile));
            }
        }
    }
    */

    public bool IsProfileSwitchOK
    {
        get
        {
            if (IsBusy || IsConnecting || IsWorking || (Profiles.Count <= 1))
                return false;
            else
                return true;
        }
    }


    /*
    private Profile? _selectedQuickProfile;
    public Profile? SelectedQuickProfile
    {
        get
        {
            return _selectedQuickProfile;
        }
        set
        {
            if (_selectedQuickProfile == value)
                return;

            if (IsProfileSwitchOK)
            {
                _selectedQuickProfile = value;

                if (_selectedQuickProfile is not null)
                {
                    SelectedProfile = _selectedQuickProfile;
                    CurrentProfile = _selectedQuickProfile;

                    ChangeConnection(_selectedQuickProfile);
                }
            }

            OnPropertyChanged(nameof(SelectedQuickProfile));
        }
    }
    */

    private string _host = "";
    public string Host
    {
        get { return _host; }
        set
        {
            //ClearError(nameof(Host));
            _host = value;
            /*
            // Validate input.
            if (value == "")
            {
                //SetError(nameof(Host), MPDCtrlX.Properties.Resources.Settings_ErrorHostMustBeSpecified);

            }
            else if (value == "localhost")
            {
                _host = "127.0.0.1";
            }
            else
            {
                IPAddress ipAddress;
                try
                {
                    ipAddress = IPAddress.Parse(value);
                    if (ipAddress is not null)
                    {
                        _host = value;
                    }
                }
                catch
                {
                    //System.FormatException
                    //SetError(nameof(Host), MPDCtrlX.Properties.Resources.Settings_ErrorHostInvalidAddressFormat);
                }
            }
            */

            OnPropertyChanged(nameof(Host));
        }
    }

    private IPAddress? _hostIpAddress;
    public IPAddress? HostIpAddress
    {
        get { return _hostIpAddress; }
        set
        {
            //if (_hostIpAddress is null) return;
            //if (_hostIpAddress.Equals(value))  return;

            _hostIpAddress = value;

            OnPropertyChanged(nameof(HostIpAddress));
        }
    }

    private int _port = 6600;
    public string Port
    {
        get => _port.ToString();
        set
        {
            //ClearError(nameof(Port));

            if (value == "")
            {
                //SetError(nameof(Port), MPDCtrlX.Properties.Resources.Settings_ErrorPortMustBeSpecified);
                _port = 6600;
            }
            else
            {
                // Validate input. Test with i;
                if (Int32.TryParse(value, out int i))
                {
                    //Int32.TryParse(value, out _defaultPort)
                    // Change the value only when test was successfull.
                    _port = i;
                    //ClearError(nameof(Port));
                }
                else
                {
                    //SetError(nameof(Port), MPDCtrlX.Properties.Resources.Settings_ErrorInvalidPortNaN);
                    _port = 6600;
                }
            }

            OnPropertyChanged(nameof(Port));
        }
    }

    private string _password = "";
    public string Password
    {
        get => DummyPassword(_password);
        set
        {
            // Don't. if (_password == value) ...

            _password = value;

            //OnPropertyChanged(nameof(IsNotPasswordSet));
            //OnPropertyChanged(nameof(IsPasswordSet));
            OnPropertyChanged(nameof(Password));
        }
    }

    private static string Encrypt(string s)
    {
        if (string.IsNullOrEmpty(s)) { return ""; }

        byte[] entropy = [0x72, 0xa2, 0x12, 0x04];

        // Uses System.Security.Cryptography.ProtectedData
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                byte[] userData = System.Text.Encoding.UTF8.GetBytes(s);
                byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(userData, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return System.Convert.ToBase64String(encryptedData);
            }
            catch
            {
                Debug.WriteLine($"Encrypt fail.");
                return s;
            }
        }
        //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        else
        {
            string encryptionKey = "withas";
            byte[] sBytes = Encoding.Unicode.GetBytes(s);
            using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(sBytes, 0, sBytes.Length);
                    cs.Close();
                }
                s = Convert.ToBase64String(ms.ToArray());
            }
            return s;
        }
    }

    private static string Decrypt(string s)
    {
        if (string.IsNullOrEmpty(s)) { return ""; }

        byte[] entropy = [0x72, 0xa2, 0x12, 0x04];

        // Uses System.Security.Cryptography.ProtectedData
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                byte[] encryptedData = System.Convert.FromBase64String(s);
                byte[] userData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);

                return System.Text.Encoding.UTF8.GetString(userData);
            }
            catch
            {
                Debug.WriteLine($"Decrypt fail.");
                return "";
            }
        }
        //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        else
        {
            string encryptionKey = "withas";
            s = s.Replace(" ", "+");
            byte[] sBytes = Convert.FromBase64String(s);
            using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(sBytes, 0, sBytes.Length);
                    cs.Close();
                }
                s = Encoding.Unicode.GetString(ms.ToArray());
            }
            
            return s;
        }
    }

    private static string DummyPassword(string s)
    {
        if (string.IsNullOrEmpty(s)) { return ""; }
        string e = "";
        for (int i = 1; i <= s.Length; i++)
        {
            e += "*";
        }
        return e;
    }
    /*
    private string _settingProfileEditMessage = "";
    public string SettingProfileEditMessage
    {
        get
        {
            return _settingProfileEditMessage;
        }
        set
        {
            _settingProfileEditMessage = value;
            OnPropertyChanged(nameof(SettingProfileEditMessage));
        }
    }

    public bool IsPasswordSet
    {
        get
        {
            if (SelectedProfile is not null)
            {
                if (!string.IsNullOrEmpty(SelectedProfile.Password))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsNotPasswordSet
    {
        get
        {
            if (IsPasswordSet)
                return false;
            else
                return true;
        }
    }



    private bool _isSwitchingProfile;
    public bool IsSwitchingProfile
    {
        get
        {
            return _isSwitchingProfile;
        }
        set
        {
            if (_isSwitchingProfile == value)
                return;

            _isSwitchingProfile = value;
            OnPropertyChanged(nameof(IsSwitchingProfile));
        }
    }

    private string _changePasswordDialogMessage = "";
    public string ChangePasswordDialogMessage
    {
        get { return _changePasswordDialogMessage; }
        set
        {
            if (_changePasswordDialogMessage == value)
                return;

            _changePasswordDialogMessage = value;
            OnPropertyChanged(nameof(ChangePasswordDialogMessage));
        }
    }
    */

    private bool _isRememberAsProfile = true;
    public bool IsRememberAsProfile
    {
        get
        {
            return _isRememberAsProfile;
        }
        set
        {
            if (_isRememberAsProfile == value)
                return;

            _isRememberAsProfile = value;
            OnPropertyChanged(nameof(IsRememberAsProfile));
        }
    }

    #endregion

    #region == Status Messages == 

    private string _statusBarMessage = "";
    public string StatusBarMessage
    {
        get
        {
            return _statusBarMessage;
        }
        set
        {
            _statusBarMessage = value;
            OnPropertyChanged(nameof(StatusBarMessage));
        }
    }

    private string _connectionStatusMessage = "";
    public string ConnectionStatusMessage
    {
        get
        {
            return _connectionStatusMessage;
        }
        set
        {
            _connectionStatusMessage = value;
            OnPropertyChanged(nameof(ConnectionStatusMessage));
        }
    }

    private string _mpdStatusMessage = "";
    public string MpdStatusMessage
    {
        get
        {
            return _mpdStatusMessage;
        }
        set
        {
            _mpdStatusMessage = value;
            OnPropertyChanged(nameof(MpdStatusMessage));

            if (_mpdStatusMessage != "")
                _isMpdStatusMessageContainsText = true;
            else
                _isMpdStatusMessageContainsText = false;
            OnPropertyChanged(nameof(IsMpdStatusMessageContainsText));
        }
    }

    private bool _isMpdStatusMessageContainsText;
    public bool IsMpdStatusMessageContainsText
    {
        get
        {
            return _isMpdStatusMessageContainsText;
        }
    }

    private string _infoBarInfoTitle = "";
    public string InfoBarInfoTitle
    {
        get
        {
            return _infoBarInfoTitle;
        }
        set
        {
            _infoBarInfoTitle = value;
            OnPropertyChanged(nameof(InfoBarInfoTitle));
        }
    }

    private string _infoBarInfoMessage = "";
    public string InfoBarInfoMessage
    {
        get
        {
            return _infoBarInfoMessage;
        }
        set
        {
            _infoBarInfoMessage = value;
            OnPropertyChanged(nameof(InfoBarInfoMessage));
        }
    }

    private string _infoBarAckTitle = "";
    public string InfoBarAckTitle
    {
        get
        {
            return _infoBarAckTitle;
        }
        set
        {
            _infoBarAckTitle = value;
            OnPropertyChanged(nameof(InfoBarAckTitle));
        }
    }

    private string _infoBarAckMessage = "";
    public string InfoBarAckMessage
    {
        get
        {
            return _infoBarAckMessage;
        }
        set
        {
            _infoBarAckMessage = value;
            OnPropertyChanged(nameof(InfoBarAckMessage));
        }
    }

    private string _infoBarErrTitle = "";
    public string InfoBarErrTitle
    {
        get
        {
            return _infoBarErrTitle;
        }
        set
        {
            _infoBarErrTitle = value;
            OnPropertyChanged(nameof(InfoBarErrTitle));
        }
    }

    private string _infoBarErrMessage = "";
    public string InfoBarErrMessage
    {
        get
        {
            return _infoBarErrMessage;
        }
        set
        {
            _infoBarErrMessage = value;
            OnPropertyChanged(nameof(InfoBarErrMessage));
        }
    }


    private static readonly string _pathDefaultNoneButton = "";
    private static readonly string _pathDisconnectedButton = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M3.88,13.46L2.46,14.88L4.59,17L2.46,19.12L3.88,20.54L6,18.41L8.12,20.54L9.54,19.12L7.41,17L9.54,14.88L8.12,13.46L6,15.59L3.88,13.46M14,15H20V19H14V15Z";

    private static readonly string _pathConnectingButton = "M11 14H9C9 9.03 13.03 5 18 5V7C14.13 7 11 10.13 11 14M18 11V9C15.24 9 13 11.24 13 14H15C15 12.34 16.34 11 18 11M7 4C7 2.89 6.11 2 5 2S3 2.89 3 4 3.89 6 5 6 7 5.11 7 4M11.45 4.5H9.45C9.21 5.92 8 7 6.5 7H3.5C2.67 7 2 7.67 2 8.5V11H8V8.74C9.86 8.15 11.25 6.5 11.45 4.5M19 17C20.11 17 21 16.11 21 15S20.11 13 19 13 17 13.89 17 15 17.89 17 19 17M20.5 18H17.5C16 18 14.79 16.92 14.55 15.5H12.55C12.75 17.5 14.14 19.15 16 19.74V22H22V19.5C22 18.67 21.33 18 20.5 18Z";
    private static readonly string _pathConnectedButton = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";
    //private static string _pathConnectedButton = "";
    //private static string _pathDisconnectedButton = "";
    private static readonly string _pathNewConnectionButton = "M20,4C21.11,4 22,4.89 22,6V18C22,19.11 21.11,20 20,20H4C2.89,20 2,19.11 2,18V6C2,4.89 2.89,4 4,4H20M8.5,15V9H7.25V12.5L4.75,9H3.5V15H4.75V11.5L7.3,15H8.5M13.5,10.26V9H9.5V15H13.5V13.75H11V12.64H13.5V11.38H11V10.26H13.5M20.5,14V9H19.25V13.5H18.13V10H16.88V13.5H15.75V9H14.5V14A1,1 0 0,0 15.5,15H19.5A1,1 0 0,0 20.5,14Z";
    private static readonly string _pathErrorInfoButton = "M23,12L20.56,14.78L20.9,18.46L17.29,19.28L15.4,22.46L12,21L8.6,22.47L6.71,19.29L3.1,18.47L3.44,14.78L1,12L3.44,9.21L3.1,5.53L6.71,4.72L8.6,1.54L12,3L15.4,1.54L17.29,4.72L20.9,5.54L20.56,9.22L23,12M20.33,12L18.5,9.89L18.74,7.1L16,6.5L14.58,4.07L12,5.18L9.42,4.07L8,6.5L5.26,7.09L5.5,9.88L3.67,12L5.5,14.1L5.26,16.9L8,17.5L9.42,19.93L12,18.81L14.58,19.92L16,17.5L18.74,16.89L18.5,14.1L20.33,12M11,15H13V17H11V15M11,7H13V13H11V7";

    private string _statusButton = _pathDefaultNoneButton;
    public string StatusButton
    {
        get
        {
            return _statusButton;
        }
        set
        {
            if (_statusButton == value)
                return;

            _statusButton = value;
            OnPropertyChanged(nameof(StatusButton));
        }
    }

    private static readonly string _pathMpdOkButton = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";

    private static readonly string _pathMpdAckErrorButton = "M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z";

    private string _mpdStatusButton = _pathMpdOkButton;
    public string MpdStatusButton
    {
        get
        {
            return _mpdStatusButton;
        }
        set
        {
            if (_mpdStatusButton == value)
                return;

            _mpdStatusButton = value;
            OnPropertyChanged(nameof(MpdStatusButton));
        }
    }

    private bool _isUpdatingMpdDb;
    public bool IsUpdatingMpdDb
    {
        get
        {
            return _isUpdatingMpdDb;
        }
        set
        {
            _isUpdatingMpdDb = value;
            OnPropertyChanged(nameof(IsUpdatingMpdDb));
        }
    }

    private string _mpdVersion = "";
    public string MpdVersion
    {
        get
        {
            if (_mpdVersion != "")
                return "MPD Protocol v" + _mpdVersion;
            else
                return _mpdVersion;

        }
        set
        {
            if (value == _mpdVersion)
                return;

            _mpdVersion = value;
            OnPropertyChanged(nameof(MpdVersion));
        }
    }

    //private string _shortStatusWIthMpdVersion = "";
    public string ShortStatusWIthMpdVersion
    {
        get
        {

            if (IsConnected)
            {
                if (!string.IsNullOrEmpty(_mpdVersion))
                {
                    if (CurrentProfile is not null)
                    {
                        return $"Connected to {CurrentProfile.Name} with MPD Protocol v{_mpdVersion}";
                    }
                    else
                    {
                        return $"Connected with MPD Protocol v{_mpdVersion}";
                    }
                }
                else
                {
                    if (CurrentProfile is not null)
                    {
                        return $"Connected to {CurrentProfile.Name}";
                    }
                    else
                    {
                        return "Connected";
                    }
                }
            }
            else if (IsConnecting)
            {
                if (CurrentProfile is not null)
                {
                    return $"Connecting to {CurrentProfile.Name}...";
                }
                else
                {
                    return "Connecting...";
                }
            }
            else
            {
                if (IsNotConnectingNorConnected)
                {
                    return "Not connected";
                }


                return "Not connected";
            }


        }
    }

    #endregion

    #region == Popups ==

    //private List<string> queueListviewSelectedQueueSongIdsForPopup = [];
    //private List<string> searchResultListviewSelectedQueueSongUriForPopup = [];
    //private List<string> songFilesListviewSelectedQueueSongUriForPopup = [];

    private bool _isConfirmClearQueuePopupVisible;
    public bool IsConfirmClearQueuePopupVisible
    {
        get
        {
            return _isConfirmClearQueuePopupVisible;
        }
        set
        {
            if (_isConfirmClearQueuePopupVisible == value)
                return;

            _isConfirmClearQueuePopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmClearQueuePopupVisible));
        }
    }

    private bool _isSelectedSaveToPopupVisible;
    public bool IsSelectedSaveToPopupVisible
    {
        get
        {
            return _isSelectedSaveToPopupVisible;
        }
        set
        {
            if (_isSelectedSaveToPopupVisible == value)
                return;

            _isSelectedSaveToPopupVisible = value;
            OnPropertyChanged(nameof(IsSelectedSaveToPopupVisible));
        }
    }

    private bool _isSelectedSaveAsPopupVisible;
    public bool IsSelectedSaveAsPopupVisible
    {
        get
        {
            return _isSelectedSaveAsPopupVisible;
        }
        set
        {
            if (_isSelectedSaveAsPopupVisible == value)
                return;

            _isSelectedSaveAsPopupVisible = value;
            OnPropertyChanged(nameof(IsSelectedSaveAsPopupVisible));
        }
    }

    private bool _isConfirmDeleteQueuePopupVisible;
    public bool IsConfirmDeleteQueuePopupVisible
    {
        get
        {
            return _isConfirmDeleteQueuePopupVisible;
        }
        set
        {
            if (_isConfirmDeleteQueuePopupVisible == value)
                return;

            _isConfirmDeleteQueuePopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmDeleteQueuePopupVisible));
        }
    }

    private bool _isConfirmUpdatePlaylistSongsPopupVisible;
    public bool IsConfirmUpdatePlaylistSongsPopupVisible
    {
        get
        {
            return _isConfirmUpdatePlaylistSongsPopupVisible;
        }
        set
        {
            if (_isConfirmUpdatePlaylistSongsPopupVisible == value)
                return;

            _isConfirmUpdatePlaylistSongsPopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmUpdatePlaylistSongsPopupVisible));
        }
    }

    private bool _isConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible;
    public bool IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible
    {
        get
        {
            return _isConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible;
        }
        set
        {
            if (_isConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible == value)
                return;

            _isConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible));
        }
    }

    private bool _isConfirmDeletePlaylistSongPopupVisible;
    public bool IsConfirmDeletePlaylistSongPopupVisible
    {
        get
        {
            return _isConfirmDeletePlaylistSongPopupVisible;
        }
        set
        {
            if (_isConfirmDeletePlaylistSongPopupVisible == value)
                return;

            _isConfirmDeletePlaylistSongPopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmDeletePlaylistSongPopupVisible));
        }
    }

    private bool _isConfirmPlaylistClearPopupVisible;
    public bool IsConfirmPlaylistClearPopupVisible
    {
        get
        {
            return _isConfirmPlaylistClearPopupVisible;
        }
        set
        {
            if (_isConfirmPlaylistClearPopupVisible == value)
                return;

            _isConfirmPlaylistClearPopupVisible = value;
            OnPropertyChanged(nameof(IsConfirmPlaylistClearPopupVisible));
        }
    }

    private bool _isSearchResultSelectedSaveAsPopupVisible;
    public bool IsSearchResultSelectedSaveAsPopupVisible
    {
        get
        {
            return _isSearchResultSelectedSaveAsPopupVisible;
        }
        set
        {
            if (_isSearchResultSelectedSaveAsPopupVisible == value)
                return;

            _isSearchResultSelectedSaveAsPopupVisible = value;
            OnPropertyChanged(nameof(IsSearchResultSelectedSaveAsPopupVisible));
        }
    }

    private bool _isSearchResultSelectedSaveToPopupVisible;
    public bool IsSearchResultSelectedSaveToPopupVisible
    {
        get
        {
            return _isSearchResultSelectedSaveToPopupVisible;
        }
        set
        {
            if (_isSearchResultSelectedSaveToPopupVisible == value)
                return;

            _isSearchResultSelectedSaveToPopupVisible = value;
            OnPropertyChanged(nameof(IsSearchResultSelectedSaveToPopupVisible));
        }
    }

    private bool _sSongFilesSelectedSaveAsPopupVisible;
    public bool IsSongFilesSelectedSaveAsPopupVisible
    {
        get
        {
            return _sSongFilesSelectedSaveAsPopupVisible;
        }
        set
        {
            if (_sSongFilesSelectedSaveAsPopupVisible == value)
                return;

            _sSongFilesSelectedSaveAsPopupVisible = value;
            OnPropertyChanged(nameof(IsSongFilesSelectedSaveAsPopupVisible));
        }
    }

    private bool _isSongFilesSelectedSaveToPopupVisible;
    public bool IsSongFilesSelectedSaveToPopupVisible
    {
        get
        {
            return _isSongFilesSelectedSaveToPopupVisible;
        }
        set
        {
            if (_isSongFilesSelectedSaveToPopupVisible == value)
                return;

            _isSongFilesSelectedSaveToPopupVisible = value;
            OnPropertyChanged(nameof(IsSongFilesSelectedSaveToPopupVisible));
        }
    }

    #endregion

    #region == Events ==

    // DebugWindow
    public delegate void DebugWindowShowHideEventHandler();
    public event DebugWindowShowHideEventHandler? DebugWindowShowHide;

    public event EventHandler<string>? DebugCommandOutput;

    public event EventHandler<string>? DebugIdleOutput;

    public delegate void DebugCommandClearEventHandler();
    public event DebugCommandClearEventHandler? DebugCommandClear;

    public delegate void DebugIdleClearEventHandler();
    public event DebugIdleClearEventHandler? DebugIdleClear;

    // AckWindow
    //public event EventHandler<string>? AckWindowOutput;
    // ErrWindow
    //public event EventHandler<string>? ErrWindowOutput;

    public delegate void AckWindowClearEventHandler();
    public event AckWindowClearEventHandler? AckWindowClear;

    // Queue listview ScrollIntoView
    public event EventHandler<int>? ScrollIntoView;

    // Queue listview ScrollIntoView and select (for filter and first time loading the queue)
    public event EventHandler<int>? ScrollIntoViewAndSelect;

    public event EventHandler<string>? UpdateProgress;

    public event EventHandler<string>? CurrentSongChanged;

    //public event EventHandler? QueueSaveAsDialogShow;
    //public event EventHandler? QueueSaveToDialogShow;
    //public event EventHandler<List<string>>? QueueListviewSaveToDialogShow;
    
    public event EventHandler? QueueHeaderVisibilityChanged;
    public event EventHandler? QueueFindWindowVisibilityChangedSetFocus;

    public event EventHandler? SearchHeaderVisibilityChanged;
    public event EventHandler? PlaylistHeaderVisibilityChanged;
    public event EventHandler? FilesHeaderVisibilityChanged;

    //public event EventHandler<List<string>>? SearchPageAddToPlaylistDialogShow;
    //public event EventHandler<List<string>>? FilesPageAddToPlaylistDialogShow;

    public event EventHandler<string>? PlaylistRenameToDialogShow;


    //public event EventHandler<NodeTree>? GoToSelectedPage;
    //public delegate void NavigationViewMenuItemsLoadedEventHandler();
    //public event NavigationViewMenuItemsLoadedEventHandler? NavigationViewMenuItemsLoaded;
    //

    //public event EventHandler? NavigationViewMenuItemsLoaded;
    public event EventHandler? GoToSettingsPage;

    #endregion

    #region == Services == 

    private readonly IMpcService _mpc;

    #endregion

    private readonly InitWindow _initWin;
    private readonly IDialogService _dialog;

    public MainViewModel(IMpcService mpcService, InitWindow initWin, IDialogService dialogService)
    {
        // MPD Service dependency injection.
        _mpc = mpcService;
        _initWin = initWin;
        _dialog = dialogService;

        #region == Subscribe to events ==

        _mpc.IsBusy += new MpcService.IsBusyEvent(OnMpcIsBusy);

        _mpc.MpdIdleConnected += new MpcService.IsMpdIdleConnectedEvent(OnMpdIdleConnected);

        _mpc.DebugCommandOutput += new MpcService.DebugCommandOutputEvent(OnDebugCommandOutput);
        _mpc.DebugIdleOutput += new MpcService.DebugIdleOutputEvent(OnDebugIdleOutput);

        _mpc.ConnectionStatusChanged += new MpcService.ConnectionStatusChangedEvent(OnConnectionStatusChanged);
        _mpc.ConnectionError += new MpcService.ConnectionErrorEvent(OnConnectionError);

        _mpc.MpdPlayerStatusChanged += new MpcService.MpdPlayerStatusChangedEvent(OnMpdPlayerStatusChanged);
        _mpc.MpdCurrentQueueChanged += new MpcService.MpdCurrentQueueChangedEvent(OnMpdCurrentQueueChanged);
        _mpc.MpdPlaylistsChanged += new MpcService.MpdPlaylistsChangedEvent(OnMpdPlaylistsChanged);

        _mpc.MpdAckError += new MpcService.MpdAckErrorEvent(OnMpdAckError);
        _mpc.MpdFatalError += new MpcService.MpdFatalErrorEvent(OnMpdFatalError);

        _mpc.MpdAlbumArtChanged += new MpcService.MpdAlbumArtChangedEvent(OnAlbumArtChanged);

        //_mpc.MpcInfo += new MpcService.MpcInfoEvent(OnMpcInfoEvent);

        // [Background][UI] etc
        _mpc.MpcProgress += new MpcService.MpcProgressEvent(OnMpcProgress);
        this.UpdateProgress += (sender, arg) => { this.OnUpdateProgress(arg); };

        #endregion

        #region == Init Song's time elapsed timer. ==  

        // Init Song's time elapsed timer.
        _elapsedTimer = new System.Timers.Timer(1000); // adjust this when _elapsedTimeMultiplier value is not 1.
        _elapsedTimer.Elapsed += new System.Timers.ElapsedEventHandler(ElapsedTimer);

        #endregion

        #region == Load settings ==

        System.IO.Directory.CreateDirectory(App.AppDataFolder);

        LoadSettings();

        #endregion

        #region == Themes ==

        // Sets default if not set in "load settings".
        if (_currentTheme is null)
        {
            CurrentTheme = _themes[0]; // needs this.
            _currentTheme = _themes[0]; // just because VS IDE complains to me to set.
        }

        // On linux there seems to be a bug where user prefered color is not picked up.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            FluentAvaloniaTheme? faTheme = ((Application.Current as App)!.Styles[0] as FluentAvaloniaTheme);
            //_faTheme!.PreferSystemTheme = true;
            faTheme!.CustomAccentColor = Avalonia.Media.Color.FromRgb(28, 96, 168);
        }

        #endregion

#if DEBUG
        IsSaveLog = true;
        IsEnableDebugWindow = true;

#else
        IsSaveLog = false;
        IsEnableDebugWindow = false;
#endif
    }

    #region == Startup and Shutdown ==

    private void LoadSettings()
    {
        #region == Load app setting  ==
        try
        {
            // Load config file.
            if (File.Exists(App.AppConfigFilePath))
            {
                var xdoc = XDocument.Load(App.AppConfigFilePath);
                if (xdoc.Root is not null)
                {
                    #region == Window setting ==

                    // Main Window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow is not null)
                    {
                        int wY = 24;
                        int wX = 24;

                        var hoge = mainWindow.Attribute("top");
                        if (hoge is not null)
                        {
                            //w.Top = double.Parse(hoge.Value);
                            //wY = int.Parse(hoge.Value);
                            if (Int32.TryParse(hoge.Value, out wY))
                            {
                                WindowTop = wY;
                            }
                        }

                        hoge = mainWindow.Attribute("left");
                        if (hoge is not null)
                        {
                            //w.Left = double.Parse(hoge.Value);
                            //wX = int.Parse(hoge.Value);
                            if (Int32.TryParse(hoge.Value, out wX))
                            {
                                WindowLeft = wX;
                            }
                        }
                        //w.Position = new PixelPoint(wX, wY);

                        hoge = mainWindow.Attribute("height");
                        if (hoge is not null)
                        {
                            if (!string.IsNullOrEmpty(hoge.Value))
                            {
                                //w.Height = double.Parse(hoge.Value);
                                WindowHeight = double.Parse(hoge.Value);
                            }
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge is not null)
                        {
                            if (!string.IsNullOrEmpty(hoge.Value))
                            {
                                //w.Width = double.Parse(hoge.Value);
                                WindowWidth = double.Parse(hoge.Value);
                            }
                        }

                        hoge = mainWindow.Attribute("state");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                //w.WindowState = WindowState.Maximized;
                                // Since there is no restorebounds in AvaloniaUI.....
                                WindowState = WindowState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                //w.WindowState = WindowState.Normal;
                                WindowState = WindowState.Normal;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                //w.WindowState = WindowState.Normal;
                                WindowState = WindowState.Normal;
                            }
                        }
                    }

                    #endregion

                    #region == Theme ==

                    var thm = xdoc.Root.Element("Theme");
                    if (thm is not null)
                    {
                        var hoge = thm.Attribute("ThemeName");
                        if (hoge is not null)
                        {
                            if ((hoge.Value == "Dark") || hoge.Value == "Light" || hoge.Value == "System")
                            {
                                Theme? theme = _themes.FirstOrDefault(x => x.Name == hoge.Value);
                                if (theme is not null)
                                {
                                    CurrentTheme = theme;
                                }
                            }
                        }
                    }

                    #endregion

                    #region == Options ==

                    var opts = xdoc.Root.Element("Options");
                    if (opts is not null)
                    {
                        var hoge = opts.Attribute("AutoScrollToNowPlaying");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsAutoScrollToNowPlaying = true;
                            }
                            else
                            {
                                IsAutoScrollToNowPlaying = false;
                            }
                        }

                        hoge = opts.Attribute("UpdateOnStartup");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsUpdateOnStartup = true;
                            }
                            else
                            {
                                IsUpdateOnStartup = false;
                            }
                        }

                        hoge = opts.Attribute("ShowDebugWindow");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsShowDebugWindow = true;

                            }
                            else
                            {
                                IsShowDebugWindow = false;
                            }
                        }

                        hoge = opts.Attribute("SaveLog");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsSaveLog = true;

                            }
                            else
                            {
                                IsSaveLog = false;
                            }
                        }

                        hoge = opts.Attribute("DownloadAlbumArt");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsDownloadAlbumArt = true;

                            }
                            else
                            {
                                IsDownloadAlbumArt = false;
                            }
                        }

                        hoge = opts.Attribute("DownloadAlbumArtEmbeddedUsingReadPicture");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsDownloadAlbumArtEmbeddedUsingReadPicture = true;

                            }
                            else
                            {
                                IsDownloadAlbumArtEmbeddedUsingReadPicture = false;
                            }
                        }
                    }

                    #endregion

                    #region == Profiles  ==

                    var xProfiles = xdoc.Root.Element("Profiles");
                    if (xProfiles is not null)
                    {
                        var profileList = xProfiles.Elements("Profile");

                        foreach (var p in profileList)
                        {
                            Profile pro = new();

                            if (p.Attribute("Name") is not null)
                            {
                                var s = p.Attribute("Name")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                    pro.Name = s;
                            }
                            if (p.Attribute("Host") is not null)
                            {
                                var s = p.Attribute("Host")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                    pro.Host = s;
                            }
                            if (p.Attribute("Port") is not null)
                            {
                                var s = p.Attribute("Port")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        pro.Port = Int32.Parse(s);
                                    }
                                    catch
                                    {
                                        pro.Port = 6600;
                                    }
                                }
                            }
                            if (p.Attribute("Password") is not null)
                            {
                                var s = p.Attribute("Password")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                    pro.Password = Decrypt(s);
                            }
                            if (p.Attribute("IsDefault") is not null)
                            {
                                var s = p.Attribute("IsDefault")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    if (s == "True")
                                    {
                                        pro.IsDefault = true;
                                    }
                                }
                            }
                            if (p.Attribute("Volume") is not null)
                            {
                                var s = p.Attribute("Volume")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        pro.Volume = double.Parse(s);
                                    }
                                    catch
                                    {
                                        pro.Volume = 50;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(pro.Host.Trim()))
                            {
                                if (pro.IsDefault)
                                {
                                    CurrentProfile = pro;

                                    //OnPropertyChanged(nameof(IsCurrentProfileSet));
                                }

                                Profiles.Add(pro);
                            }
                            else
                            {
                                Debug.WriteLine("Host info is empty. @LoadSettings");
                            }
                        }
                    }
                    #endregion

                    #region == Layout ==

                    var lay = xdoc.Root.Element("Layout");
                    if (lay is not null)
                    {
                        var leftpain = lay.Element("LeftPain");
                        if (leftpain is not null)
                        {
                            if (leftpain.Attribute("Width") is not null)
                            {
                                var s = leftpain.Attribute("Width")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        MainLeftPainWidth = Double.Parse(s);
                                    }
                                    catch
                                    {
                                        MainLeftPainWidth = 241;
                                    }
                                }
                            }

                            var hoge = leftpain.Attribute("NavigationViewMenuOpen");
                            if (hoge is not null)
                            {
                                // Call 
                                // "OnPropertyChanged(nameof(IsNavigationViewMenuOpen));"
                                // AFTER NavigationMenuItems is added.
                                if (hoge.Value == "True")
                                {
                                    // Don't apply change here.
                                    _isNavigationViewMenuOpen = true;

                                }
                                else
                                {
                                    // Don't apply change here.
                                    _isNavigationViewMenuOpen = false;
                                }
                            }
                        }

                        #region == Header columns ==

                        var Headers = lay.Element("Headers");
                        if (Headers is not null)
                        {
                            // Queue page
                            var Que = Headers.Element("Queue");
                            if (Que is not null)
                            {
                                var column = Que.Element("Position");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                            {
                                                IsQueueColumnHeaderPositionVisible = true;
                                            }
                                            else
                                            {
                                                IsQueueColumnHeaderPositionVisible = false;
                                            }
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderPositionWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderPositionWidth = 60;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("NowPlaying");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderNowPlayingVisible = true;
                                            else
                                                IsQueueColumnHeaderNowPlayingVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderNowPlayingWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderNowPlayingWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Title");
                                if (column is not null)
                                {
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderTitleWidth = Double.Parse(s);
                                                if (QueueColumnHeaderTitleWidth < 120)
                                                    QueueColumnHeaderTitleWidth = 160;
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderTitleWidth = 160;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Time");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderTimeVisible = true;
                                            else
                                                IsQueueColumnHeaderTimeVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderTimeWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderTimeWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Artist");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderArtistVisible = true;
                                            else
                                                IsQueueColumnHeaderArtistVisible= false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderArtistWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderArtistWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Album");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderAlbumVisible = true;
                                            else
                                                IsQueueColumnHeaderAlbumVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderAlbumWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderAlbumWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Disc");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderDiscVisible = true;
                                            else
                                                IsQueueColumnHeaderDiscVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderDiscWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderDiscWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Track");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderTrackVisible = true;
                                            else
                                                IsQueueColumnHeaderTrackVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderTrackWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderTrackWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("Genre");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderGenreVisible = true;
                                            else
                                                IsQueueColumnHeaderGenreVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderGenreWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderGenreWidth = 100;
                                            }
                                        }
                                    }
                                }
                                column = Que.Element("LastModified");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsQueueColumnHeaderLastModifiedVisible = true;
                                            else
                                                IsQueueColumnHeaderLastModifiedVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                QueueColumnHeaderLastModifiedWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                QueueColumnHeaderLastModifiedWidth = 53;
                                            }
                                        }
                                    }
                                }
                            }

                            // Files page
                            var Filp = Headers.Element("Files");
                            if (Filp is not null)
                            {
                                var column = Filp.Element("FileName");
                                if (column is not null)
                                {
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                FilesColumnHeaderTitleWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                //LibraryColumnHeaderTitleWidth = 160;
                                            }
                                        }
                                    }
                                }
                                column = Filp.Element("FilePath");
                                if (column is not null)
                                {
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                FilesColumnHeaderFilePathWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                //LibraryColumnHeaderFilePathWidth = 160;
                                            }
                                        }
                                    }
                                }
                            }

                            //Playlist page
                            var Playl = Headers.Element("Playlist");
                            if (Playl is not null)
                            {
                                var column = Playl.Element("Position");
                                if (column is not null)
                                {
                                    /*
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                            {
                                                IsPlaylistColumnHeaderPositionVisible = true;
                                            }
                                            else
                                            {
                                                IsPlaylistColumnHeaderPositionVisible = false;
                                            }
                                        }
                                    }
                                    */
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderPositionWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderPositionWidth = 60;
                                            }
                                        }
                                    }
                                }
                                /*
                                column = Playl.Element("NowPlaying");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderNowPlayingVisible = true;
                                            else
                                                IsPlaylistColumnHeaderNowPlayingVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderNowPlayingWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderNowPlayingWidth = 53;
                                            }
                                        }
                                    }
                                }
                                */
                                column = Playl.Element("Title");
                                if (column is not null)
                                {
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderTitleWidth = Double.Parse(s);
                                                if (PlaylistColumnHeaderTitleWidth < 120)
                                                    PlaylistColumnHeaderTitleWidth = 160;
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderTitleWidth = 160;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Time");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderTimeVisible = true;
                                            else
                                                IsPlaylistColumnHeaderTimeVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderTimeWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderTimeWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Artist");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderArtistVisible = true;
                                            else
                                                IsPlaylistColumnHeaderArtistVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderArtistWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderArtistWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Album");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderAlbumVisible = true;
                                            else
                                                IsPlaylistColumnHeaderAlbumVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderAlbumWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderAlbumWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Disc");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderDiscVisible = true;
                                            else
                                                IsPlaylistColumnHeaderDiscVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderDiscWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderDiscWidth = 62;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Track");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderTrackVisible = true;
                                            else
                                                IsPlaylistColumnHeaderTrackVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderTrackWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderTrackWidth = 62;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Genre");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderGenreVisible = true;
                                            else
                                                IsPlaylistColumnHeaderGenreVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderGenreWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderGenreWidth = 100;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("LastModified");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderLastModifiedVisible = true;
                                            else
                                                IsPlaylistColumnHeaderLastModifiedVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderLastModifiedWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderLastModifiedWidth = 53;
                                            }
                                        }
                                    }
                                }
                            }

                            //Search page
                            var Seal = Headers.Element("Search");
                            if (Seal is not null)
                            {
                                var column = Seal.Element("Position");
                                if (column is not null)
                                {
                                    /*
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                            {
                                                IsPlaylistColumnHeaderPositionVisible = true;
                                            }
                                            else
                                            {
                                                IsPlaylistColumnHeaderPositionVisible = false;
                                            }
                                        }
                                    }
                                    */
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderPositionWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderPositionWidth = 60;
                                            }
                                        }
                                    }
                                }
                                /*
                                column = Playl.Element("NowPlaying");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsPlaylistColumnHeaderNowPlayingVisible = true;
                                            else
                                                IsPlaylistColumnHeaderNowPlayingVisible = false;
                                        }
                                    }
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                PlaylistColumnHeaderNowPlayingWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                PlaylistColumnHeaderNowPlayingWidth = 53;
                                            }
                                        }
                                    }
                                }
                                */
                                column = Seal.Element("Title");
                                if (column is not null)
                                {
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderTitleWidth = Double.Parse(s);
                                                if (SearchColumnHeaderTitleWidth < 120)
                                                    SearchColumnHeaderTitleWidth = 160;
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderTitleWidth = 160;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Time");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderTimeVisible = true;
                                            else
                                                IsSearchColumnHeaderTimeVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderTimeWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderTimeWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Artist");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderArtistVisible = true;
                                            else
                                                IsSearchColumnHeaderArtistVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderArtistWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderArtistWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Album");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderAlbumVisible = true;
                                            else
                                                IsSearchColumnHeaderAlbumVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderAlbumWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderAlbumWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Disc");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderDiscVisible = true;
                                            else
                                                IsSearchColumnHeaderDiscVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderDiscWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderDiscWidth = 62;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Track");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderTrackVisible = true;
                                            else
                                                IsSearchColumnHeaderTrackVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderTrackWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderTrackWidth = 62;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("Genre");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderGenreVisible = true;
                                            else
                                                IsSearchColumnHeaderGenreVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderGenreWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderGenreWidth = 100;
                                            }
                                        }
                                    }
                                }
                                column = Seal.Element("LastModified");
                                if (column is not null)
                                {
                                    if (column.Attribute("Visible") is not null)
                                    {
                                        var s = column.Attribute("Visible")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            if (s == "True")
                                                IsSearchColumnHeaderLastModifiedVisible = true;
                                            else
                                                IsSearchColumnHeaderLastModifiedVisible = false;
                                        }
                                    }
                                    
                                    if (column.Attribute("Width") is not null)
                                    {
                                        var s = column.Attribute("Width")?.Value;
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            try
                                            {
                                                SearchColumnHeaderLastModifiedWidth = Double.Parse(s);
                                            }
                                            catch
                                            {
                                                SearchColumnHeaderLastModifiedWidth = 53;
                                            }
                                        }
                                    }
                                }
                            }

                        }

                        #endregion

                    }

                    #endregion

                }
                else
                {
                    Debug.WriteLine("Oops. xdoc.Root is null.");
                }
            }

            IsFullyLoaded = true;
        }
        catch (System.IO.FileNotFoundException ex)
        {
            Debug.WriteLine("Oops. @Load(AppConfigFilePath)");
            if (IsSaveLog)
            {
                Dispatcher.UIThread.Post(() => { App.AppendErrorLog("System.IO.FileNotFoundExceptionLoad(AppConfigFilePath)", ex.Message); });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Oops. @Load(AppConfigFilePath)");
            if (IsSaveLog)
            {
                Dispatcher.UIThread.Post(() => { App.AppendErrorLog("ExceptionLoad(AppConfigFilePath)", ex.Message); });
            }
        }

        #endregion

        if (Profiles.Count <= 0) return;
        
        if (CurrentProfile is not null) return;
        var prof = Profiles.FirstOrDefault(x => x.IsDefault);
        CurrentProfile = prof ?? Profiles[0];
        //OnPropertyChanged(nameof(IsCurrentProfileSet));
    }

    public async void OnWindowLoaded(object? sender, EventArgs e)
    {
        if (CurrentProfile is null)
        {
            //ConnectionStatusMessage = MPDCtrlX.Properties.Resources.Init_NewConnectionSetting; // no need. 
            StatusButton = _pathNewConnectionButton;

            // Show connection setting
            //IsConnectionSettingShow = true;

            //var InitWin = new InitWindow();// use DI.
            _initWin.DataContext = this;
            await _initWin.ShowDialog(owner: App.GetService<MainWindow>());

            return;
        }

        //IsConnectionSettingShow = false;

        // set this "quietly"
        _volume = CurrentProfile.Volume;
        OnPropertyChanged(nameof(Volume));

        // start the connection
        _ = Task.Run(()=>Start(CurrentProfile.Host, CurrentProfile.Port));

    }

    private void SaveSettings(Window sender)
    {
        // Make sure Window and settings have been fully loaded and not overriding with empty data.
        if (!IsFullyLoaded)
        {
            return;
        }

        double windowWidth = 780;

        #region == Save App Setting ==

        // Config xml file
        XmlDocument doc = new();
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

        // Root Document Element
        XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
        doc.AppendChild(root);

        XmlAttribute attrs;

        // MainWindow
        if (sender is MainWindow w)
        {
            #region == Window settings ==

            // Main Window element
            XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

            //Window w = (sender as Window);
            // Main Window attributes
            attrs = doc.CreateAttribute("height");
            if (w.WindowState == WindowState.Normal)
            {
                attrs.Value = w.Height.ToString();
            }
            else
            {
                attrs.Value = w.WinRestoreHeight.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("width");
            if (w.WindowState == WindowState.Normal)
            {
                attrs.Value = w.Width.ToString();
                windowWidth = w.Width;
            }
            else
            {
                attrs.Value = w.WinRestoreWidth.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("top");
            if (w.WindowState == WindowState.Normal)
            {
                attrs.Value = w.Position.Y.ToString();
            }
            else
            {
                attrs.Value = w.WinRestoreTop.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("left");
            if (w.WindowState == WindowState.Normal)
            {
                attrs.Value = w.Position.X.ToString();
            }
            else
            {
                attrs.Value = w.WinRestoreLeft.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("state");
            if (w.WindowState == WindowState.Maximized)
            {
                attrs.Value = "Maximized";
            }
            else if (w.WindowState == WindowState.Normal)
            {
                attrs.Value = "Normal";

            }
            else if (w.WindowState == WindowState.Minimized)
            {
                attrs.Value = "Minimized";
            }
            mainWindow.SetAttributeNode(attrs);

            // set MainWindow element to root.
            root.AppendChild(mainWindow);



            #endregion

            #region == Layout ==

            XmlElement lay = doc.CreateElement(string.Empty, "Layout", string.Empty);

            XmlElement leftpain;
            XmlAttribute lAttrs;

            // LeftPain
            leftpain = doc.CreateElement(string.Empty, "LeftPain", string.Empty);
            lAttrs = doc.CreateAttribute("Width");
            if (IsFullyLoaded) // instead of IsFullyRendered
            {
                if (windowWidth > (MainLeftPainActualWidth - 24))
                {
                    lAttrs.Value = MainLeftPainActualWidth.ToString();
                }
                else
                {
                    lAttrs.Value = "241";
                }
            }
            else
            {
                lAttrs.Value = MainLeftPainWidth.ToString();
            }
            leftpain.SetAttributeNode(lAttrs);

            lAttrs = doc.CreateAttribute("NavigationViewMenuOpen");
            if (_isNavigationViewMenuOpen)
            {
                lAttrs.Value = "True";
            }
            else
            {
                lAttrs.Value = "False";
            }
            leftpain.SetAttributeNode(lAttrs);

            //
            lay.AppendChild(leftpain);

            #region == Header columns ==

            XmlElement headers = doc.CreateElement(string.Empty, "Headers", string.Empty);

            #region == Queue header ==
            /*
            // Queue page
            XmlElement queueHeader;
            XmlElement queueHeaderColumn;
            XmlAttribute qAttrs;

            queueHeader = doc.CreateElement(string.Empty, "Queue", string.Empty);

            // Position
            queueHeaderColumn = doc.CreateElement(string.Empty, "Position", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderPositionVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderPositionWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Now Playing
            queueHeaderColumn = doc.CreateElement(string.Empty, "NowPlaying", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderNowPlayingVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderNowPlayingWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Title skip visibility
            queueHeaderColumn = doc.CreateElement(string.Empty, "Title", string.Empty);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderTitleWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Time
            queueHeaderColumn = doc.CreateElement(string.Empty, "Time", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderTimeVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderTimeWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Artist
            queueHeaderColumn = doc.CreateElement(string.Empty, "Artist", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderArtistVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderArtistWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Album
            queueHeaderColumn = doc.CreateElement(string.Empty, "Album", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderAlbumVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderAlbumWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Disc
            queueHeaderColumn = doc.CreateElement(string.Empty, "Disc", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderDiscVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderDiscWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Track
            queueHeaderColumn = doc.CreateElement(string.Empty, "Track", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderTrackVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderTrackWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Genre
            queueHeaderColumn = doc.CreateElement(string.Empty, "Genre", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderGenreVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderGenreWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            // Last Modified
            queueHeaderColumn = doc.CreateElement(string.Empty, "LastModified", string.Empty);

            qAttrs = doc.CreateAttribute("Visible");
            qAttrs.Value = IsQueueColumnHeaderLastModifiedVisible.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            qAttrs = doc.CreateAttribute("Width");
            qAttrs.Value = QueueColumnHeaderLastModifiedWidth.ToString();
            queueHeaderColumn.SetAttributeNode(qAttrs);

            queueHeader.AppendChild(queueHeaderColumn);

            //
            headers.AppendChild(queueHeader);
            */
            #endregion

            #region == Files header ==
            /*
            // FilesPage
            XmlElement filesHeader;
            XmlElement filesHeaderColumn;
            XmlAttribute fAttrs;

            filesHeader = doc.CreateElement(string.Empty, "Files", string.Empty);

            filesHeaderColumn = doc.CreateElement(string.Empty, "FileName", string.Empty);

            fAttrs = doc.CreateAttribute("Width");
            fAttrs.Value = LibraryColumnHeaderTitleWidth.ToString();
            filesHeaderColumn.SetAttributeNode(fAttrs);
            //
            filesHeader.AppendChild(filesHeaderColumn);


            filesHeaderColumn = doc.CreateElement(string.Empty, "FilePath", string.Empty);

            fAttrs = doc.CreateAttribute("Width");
            fAttrs.Value = LibraryColumnHeaderFilePathWidth.ToString();
            filesHeaderColumn.SetAttributeNode(fAttrs);
            //
            filesHeader.AppendChild(filesHeaderColumn);

            //
            headers.AppendChild(filesHeader);
            */
            #endregion

            #region == Playlist header ==
            /*
            // Playlist page
            XmlElement PlaylistHeader;
            XmlElement PlaylistHeaderColumn;
            XmlAttribute pAttrs;

            PlaylistHeader = doc.CreateElement(string.Empty, "Playlist", string.Empty);

            // Position
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Position", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderPositionVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderPositionWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Now Playing
            //PlaylistHeaderColumn = doc.CreateElement(string.Empty, "NowPlaying", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderNowPlayingVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            //pAttrs = doc.CreateAttribute("Width");
            //pAttrs.Value = PlaylistColumnHeaderNowPlayingWidth.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            //PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Title skip visibility
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Title", string.Empty);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderTitleWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Time
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Time", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderTimeVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderTimeWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Artist
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Artist", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderArtistVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderArtistWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Album
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Album", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderAlbumVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderAlbumWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Disc
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Disc", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderDiscVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderDiscWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Track
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Track", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderTrackVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderTrackWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Genre
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Genre", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderGenreVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderGenreWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Last Modified
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "LastModified", string.Empty);

            pAttrs = doc.CreateAttribute("Visible");
            pAttrs.Value = IsPlaylistColumnHeaderLastModifiedVisible.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderLastModifiedWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            //
            headers.AppendChild(PlaylistHeader);
            */
            #endregion

            #region == Search header == 
            /*
            // Search page
            XmlElement SearchHeader;
            XmlElement SearchHeaderColumn;
            XmlAttribute sAttrs;

            SearchHeader = doc.CreateElement(string.Empty, "Search", string.Empty);

            // Position
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Position", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderPositionVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderPositionWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Now Playing
            //PlaylistHeaderColumn = doc.CreateElement(string.Empty, "NowPlaying", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderNowPlayingVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            //pAttrs = doc.CreateAttribute("Width");
            //pAttrs.Value = PlaylistColumnHeaderNowPlayingWidth.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            //PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Title skip visibility
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Title", string.Empty);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderTitleWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Time
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Time", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderTimeVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderTimeWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Artist
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Artist", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderArtistVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderArtistWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Album
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Album", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderAlbumVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderAlbumWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Disc
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Disc", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderDiscVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderDiscWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Track
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Track", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderTrackVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderTrackWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Genre
            SearchHeaderColumn = doc.CreateElement(string.Empty, "Genre", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderGenreVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderGenreWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            // Last Modified
            SearchHeaderColumn = doc.CreateElement(string.Empty, "LastModified", string.Empty);

            sAttrs = doc.CreateAttribute("Visible");
            sAttrs.Value = IsSearchColumnHeaderLastModifiedVisible.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            sAttrs = doc.CreateAttribute("Width");
            sAttrs.Value = SearchColumnHeaderLastModifiedWidth.ToString();
            SearchHeaderColumn.SetAttributeNode(sAttrs);

            SearchHeader.AppendChild(SearchHeaderColumn);

            //
            headers.AppendChild(SearchHeader);
            */
            #endregion

            // TODO: more


            ////
            lay.AppendChild(headers);

            #endregion

            ////
            root.AppendChild(lay);

            #endregion
        }

        #region == Options ==

        XmlElement opts = doc.CreateElement(string.Empty, "Options", string.Empty);

        //
        attrs = doc.CreateAttribute("AutoScrollToNowPlaying");
        if (IsAutoScrollToNowPlaying)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        // 
        attrs = doc.CreateAttribute("UpdateOnStartup");
        if (IsUpdateOnStartup)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ShowDebugWindow");
        if (IsShowDebugWindow)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("SaveLog");
        if (IsSaveLog)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArt");
        if (IsDownloadAlbumArt)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArtEmbeddedUsingReadPicture");
        if (IsDownloadAlbumArtEmbeddedUsingReadPicture)
        {
            attrs.Value = "True";
        }
        else
        {
            attrs.Value = "False";
        }
        opts.SetAttributeNode(attrs);

        /// 
        root.AppendChild(opts);

        #endregion

        #region == Profiles  ==

        XmlElement xProfiles = doc.CreateElement(string.Empty, "Profiles", string.Empty);

        XmlElement xProfile;
        XmlAttribute xAttrs;

        if (Profiles.Count == 1)
            Profiles[0].IsDefault = true;

        foreach (var p in Profiles)
        {
            xProfile = doc.CreateElement(string.Empty, "Profile", string.Empty);

            xAttrs = doc.CreateAttribute("Name");
            xAttrs.Value = p.Name;
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Host");
            xAttrs.Value = p.Host;
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Port");
            xAttrs.Value = p.Port.ToString();
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Password");
            xAttrs.Value = Encrypt(p.Password);
            xProfile.SetAttributeNode(xAttrs);

            if (p.IsDefault)
            {
                xAttrs = doc.CreateAttribute("IsDefault");
                xAttrs.Value = "True";
                xProfile.SetAttributeNode(xAttrs);
            }

            xAttrs = doc.CreateAttribute("Volume");
            if (p == CurrentProfile)
            {
                xAttrs.Value = _volume.ToString();
            }
            else
            {
                xAttrs.Value = p.Volume.ToString();
            }
            xProfile.SetAttributeNode(xAttrs);


            xProfiles.AppendChild(xProfile);
        }

        // TODO:
        if (IsRememberAsProfile)
        {

        }
        root.AppendChild(xProfiles);

        #endregion

        #region == Theme ==

        XmlElement thm = doc.CreateElement(string.Empty, "Theme", string.Empty);

        attrs = doc.CreateAttribute("ThemeName");
        attrs.Value = _currentTheme.Name;
        thm.SetAttributeNode(attrs);

        /// 
        root.AppendChild(thm);

        #endregion

        try
        {
            if (!Directory.Exists(App.AppDataFolder))
            {
                Directory.CreateDirectory(App.AppDataFolder);
            }

            doc.Save(App.AppConfigFilePath);
        }
        //catch (System.IO.FileNotFoundException) { }
        catch (Exception ex)
        {
            if (IsSaveLog)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    App.AppendErrorLog("Exception@OnWindowClosing", ex.Message);
                });
            }
        }

        try
        {
            if (IsConnected)
            {
                _mpc.MpdStop = true;

                // TODO: Although it's a good thing to close...this causes anoying exception in the debug output. 
                _mpc.MpdDisconnect(false);
            }
        }
        catch { }

        if (IsSaveLog)
        {
            // Save error logs.
            Dispatcher.UIThread.Post(() =>
            {
                App.SaveErrorLog();
            });
        }

        #endregion
    }

    // Closing
    public void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (sender is Window w)
        {
            SaveSettings(w);
        }
    }

    #endregion

    #region == Methods ==

    private async Task Start(string host, int port)
    {
        HostIpAddress = null;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, AddressFamily.InterNetwork);
            if (addresses.Length > 0)
            {
                HostIpAddress = addresses[0];
                /*
                Debug.WriteLine($"IP addresses for {host}: {HostIpAddress}");
                foreach (var ip in addresses)
                {
                    Debug.WriteLine(ip);
                }
                */
            }
            else
            {
                // TODO::
                ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
                //StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";

                InfoBarAckTitle = "Error";
                InfoBarAckMessage = "Could not retrive IP Address from the hostname.";
                IsShowAckWindow = true;

                return;
            }
        }
        catch (Exception)
        {
            // TODO::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            //StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";

            InfoBarAckTitle = "Error";
            InfoBarAckMessage = "Could not retrive IP Address from the hostname.";
            IsShowAckWindow = true;

            return;
        }

        // Start MPD connection.
        _ = Task.Run(() => _mpc.MpdIdleConnect(HostIpAddress.ToString(), port));
    }

    private async Task LoadInitialData()
    {
        IsBusy = true;

        await Task.Delay(5);

        CommandResult result = await _mpc.MpdIdleSendPassword(_password);

        if (result.IsSuccess)
        {
            bool r = await _mpc.MpdCommandConnectionStart(_mpc.MpdHost, _mpc.MpdPort, _mpc.MpdPassword);

            if (r)
            {
                // Testing:
                //await _mpc.MpdIdleQueryProtocol();

                if (IsUpdateOnStartup)
                {
                    await _mpc.MpdSendUpdate();
                }

                result = await _mpc.MpdIdleQueryStatus();

                if (result.IsSuccess)
                {
                    await Task.Delay(5);
                    UpdateStatus();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryCurrentSong();

                    await Task.Delay(50);
                    UpdateCurrentSong();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryPlaylists();

                    await Task.Delay(50);
                    UpdatePlaylists();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryCurrentQueue();

                    await Task.Delay(50);
                    UpdateCurrentQueue();

                    await Task.Delay(300);
                    await _mpc.MpdQueryListAlbumArtists();
                    UpdateAlbumsAndArtists();

                    await Task.Delay(50);

                    // This no longer needed since it is aquired as needed basis.
                    //await _mpc.MpdIdleQueryListAll();
                    //await Task.Delay(5);
                    //UpdateLibrary();

                    //UpdateProgress?.Invoke(this, "");

                    // Idle start.
                    _mpc.MpdIdleStart();
                }

            }
        }

        IsBusy = false;

        await Task.Delay(500);

        // MPD protocol ver check.
        if (_mpc.MpdVerText != "")
        {
            if (CompareVersionString(_mpc.MpdVerText, "0.20.0") == -1)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    MpdStatusButton = _pathMpdAckErrorButton;
                    //StatusBarMessage = string.Format(MPDCtrlX.Properties.Resources.StatusBarMsg_MPDVersionIsOld, _mpc.MpdVerText);
                    MpdStatusMessage = string.Format(MPDCtrlX.Properties.Resources.StatusBarMsg_MPDVersionIsOld, _mpc.MpdVerText);
                });
            }
        }
    }

    private void UpdateStatus()
    {
        UpdateButtonStatus();

        Dispatcher.UIThread.Post(async () =>
        {
            UpdateProgress?.Invoke(this, "[UI] Status updating...");

            bool isSongChanged = false;
            bool isCurrentSongWasNull = false;

            if (CurrentSong is not null)
            {
                if (CurrentSong.Id != _mpc.MpdStatus.MpdSongID)
                {
                    isSongChanged = true;

                    // Clear IsPlaying icon
                    CurrentSong.IsPlaying = false;

                    //
                    if (_mpc.MpdCurrentSong is not null)
                    {
                        _mpc.MpdCurrentSong.IsPlaying = false;
                    }
                    AlbumCover = null;
                    //IsAlbumArtVisible = false;
                    AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                }
            }
            else
            {
                // just in case
                if (_mpc.MpdCurrentSong is not null)
                {
                    _mpc.MpdCurrentSong.IsPlaying = false;
                }

                isCurrentSongWasNull = true;
            }

            if (Queue.Count > 0)
            {
                if (isSongChanged || isCurrentSongWasNull)
                {
                    // Sets Current Song
                    var item = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                    if (item is not null)
                    {
                        //Debug.WriteLine("Currentsong is set. @UpdateStatus()");
                        CurrentSong = (item as SongInfoEx);
                        CurrentSong.IsPlaying = true;
                        CurrentSong.IsAlbumCoverNeedsUpdate = true;

                        //CurrentSong.IsSelected = true;

                        if (IsAutoScrollToNowPlaying)
                        {
                            ScrollIntoView?.Invoke(this, CurrentSong.Index);
                        }

                        //IsAlbumArtVisible = false;
                        AlbumCover = null;
                        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

                        // AlbumArt
                        if (!string.IsNullOrEmpty(CurrentSong.File))
                        {
                            if (IsDownloadAlbumArt && CurrentSong.IsAlbumCoverNeedsUpdate)
                            {
                                //Debug.WriteLine("getting album cover. @UpdateStatus()");
                                var res = await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);

                                if (res != null) 
                                {
                                    if (res.IsSuccess && (res.AlbumCover?.SongFilePath != null) && (CurrentSong.File != null))
                                    {
                                        if (res.AlbumCover?.SongFilePath == CurrentSong.File)
                                        {
                                            if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading))
                                            {
                                                AlbumCover = res.AlbumCover;
                                                AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                                //IsAlbumArtVisible = true;
                                                SaveAlbumCoverImage(CurrentSong, res.AlbumCover);
                                                CurrentSong.IsAlbumCoverNeedsUpdate = false;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("item is null. @UpdateStatus()");
                        // TODO:
                        CurrentSong = null;
                        AlbumCover = null;
                        //IsAlbumArtVisible = false;
                        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                    }
                }
            }
            else
            {
                //Debug.WriteLine("Queue.Count == 0. @UpdateStatus()");
                // TODO:
                //CurrentSong = null;
                AlbumCover = null;
                //IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
            }

            UpdateProgress?.Invoke(this, "");
        });
    }

    private void UpdateButtonStatus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                //Play button
                switch (_mpc.MpdStatus.MpdState)
                {
                    case Status.MpdPlayState.Play:
                        {
                            PlayButton = _pathPauseButton;
                            break;
                        }
                    case Status.MpdPlayState.Pause:
                        {
                            PlayButton = _pathPlayButton;
                            break;
                        }
                    case Status.MpdPlayState.Stop:
                        {
                            PlayButton = _pathPlayButton;
                            break;
                        }

                        //_pathStopButton
                }

                if (_mpc.MpdStatus.MpdVolumeIsReturned)
                {
                    //Debug.WriteLine($"Volume is set to {_mpc.MpdStatus.MpdVolume}. @UpdateButtonStatus()");

                    double tmpVol = Convert.ToDouble(_mpc.MpdStatus.MpdVolume);
                    if (_volume != tmpVol)
                    {
                        // "quietly" update.
                        _volume = tmpVol;
                        OnPropertyChanged(nameof(Volume));

                    }
                }

                _random = _mpc.MpdStatus.MpdRandom;
                OnPropertyChanged(nameof(Random));

                _repeat = _mpc.MpdStatus.MpdRepeat;
                OnPropertyChanged(nameof(Repeat));

                _consume = _mpc.MpdStatus.MpdConsume;
                OnPropertyChanged(nameof(Consume));

                _single = _mpc.MpdStatus.MpdSingle;
                OnPropertyChanged(nameof(Single));

                //start elapsed timer.
                if (_mpc.MpdStatus.MpdState == Status.MpdPlayState.Play)
                {
                    // no need to care about "double" updates for time.
                    Time = Convert.ToInt32(_mpc.MpdStatus.MpdSongTime);
                    Time *= _elapsedTimeMultiplier;
                    _elapsed = Convert.ToInt32(_mpc.MpdStatus.MpdSongElapsed);
                    _elapsed *= _elapsedTimeMultiplier;
                    if (!_elapsedTimer.Enabled)
                        _elapsedTimer.Start();
                }
                else
                {
                    _elapsedTimer.Stop();
                    // no need to care about "double" updates for time.
                    Time = Convert.ToInt32(_mpc.MpdStatus.MpdSongTime);
                    Time *= _elapsedTimeMultiplier;
                    _elapsed = Convert.ToInt32(_mpc.MpdStatus.MpdSongElapsed);
                    _elapsed *= _elapsedTimeMultiplier;
                    OnPropertyChanged(nameof(Elapsed));
                    OnPropertyChanged(nameof(ElapsedFormatted));
                }

                //
                //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
            catch
            {
                Debug.WriteLine("Error@UpdateButtonStatus");
            }
        });
    }

    private void UpdateCurrentSong()
    {
        Dispatcher.UIThread.Post( () =>
        {
            bool isSongChanged = false;
            bool isCurrentSongWasNull = false;

            if (CurrentSong != null)
            {
                if (CurrentSong.Id != _mpc.MpdStatus.MpdSongID)
                {
                    isSongChanged = true;

                    // Clear IsPlaying icon
                    CurrentSong.IsPlaying = false;

                    //
                    if (_mpc.MpdCurrentSong is not null)
                    {
                        _mpc.MpdCurrentSong.IsPlaying = false;
                    }

                    //IsAlbumArtVisible = false;
                    //AlbumArt = _albumArtDefault;
                }

                if (CurrentSong.IsAlbumCoverNeedsUpdate)
                {
                    isSongChanged = true;
                }
            }
            else
            {
                isCurrentSongWasNull = true;
            }

            if (_mpc.MpdCurrentSong != null)
            {
                if (_mpc.MpdCurrentSong.Id == _mpc.MpdStatus.MpdSongID)
                {
                    //CurrentSong = _mpc.MpdCurrentSong; // needed this..
                    //CurrentSong.IsPlaying = true; // don't because the object is not from queue. It's gonna be duplicated IsPlaying.

                    if (isSongChanged || isCurrentSongWasNull)
                    {
                        // AlbumArt
                        if (!string.IsNullOrEmpty(_mpc.MpdCurrentSong.File))
                        {
                            if (IsDownloadAlbumArt)
                            {
                                // looks like no need to.
                                /*
                                var res = await _mpc.MpdQueryAlbumArt(_mpc.MpdCurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                                if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading) && (res.AlbumCover?.SongFilePath != null))
                                {
                                    if (res.AlbumCover?.SongFilePath == _mpc.MpdCurrentSong.File)
                                    {
                                        if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading))
                                        {
                                            AlbumCover = res.AlbumCover;
                                            AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                            //IsAlbumArtVisible = true;
                                            if (CurrentSong is not null)
                                            {
                                                SaveAlbumCoverImage(CurrentSong, res.AlbumCover);
                                                CurrentSong.IsAlbumCoverNeedsUpdate = false;
                                            }
                                        }
                                    }
                                }
                                */
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("if (isSongChanged || isCurrentSongWasNull). @UpdateCurrentSong()");
                    }
                    /*
                    // TODO: do I need this?
                    if (IsAutoScrollToNowPlaying)
                        ScrollIntoView?.Invoke(this, CurrentSong.Index);
                    */
                }
                else
                {
                    Debug.WriteLine("_mpc.MpdCurrentSong.Id != _mpc.MpdStatus.MpdSongID. @UpdateCurrentSong()");
                }
            }
            else
            {
                //Debug.WriteLine("_mpc.MpdCurrentSong is null. @UpdateCurrentSong()");
            }
        });
    }

    private void UpdateCurrentQueue()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            /*
            if (IsSwitchingProfile)
                return;
            */
            IsQueueFindVisible = false;

            if (Queue.Count > 0)
            {
                UpdateProgress?.Invoke(this, "[UI] Updating the queue...");
                await Task.Delay(20);

                /*
                if (IsSwitchingProfile)
                    return;
                */
                IsWorking = true;

                try
                {
                    // The simplest way, but all the selections and listview position will be cleared. Kind of annoying when moving items.
                    #region == simple & fast == 
                    /*
                    UpdateProgress?.Invoke(this, "[UI] Loading the queue...");

                    Dispatcher.UIThread.Post(() =>
                    {
                        Queue = new ObservableCollection<SongInfoEx>(_mpc.CurrentQueue);

                        UpdateProgress?.Invoke(this, "[UI] Checking current song after Queue update.");

                        // Set Current and NowPlaying.
                        var curitem = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                        if (curitem is not null)
                        {
                            if (CurrentSong is not null)
                            {
                                if (CurrentSong.Id != curitem.Id)
                                {
                                    CurrentSong = curitem;
                                    CurrentSong.IsPlaying = true;

                                    if (IsAutoScrollToNowPlaying)
                                        // ScrollIntoView while don't change the selection 
                                        ScrollIntoView?.Invoke(this, CurrentSong.Index);

                                    // AlbumArt


                                }
                                else
                                {
                                    curitem.IsPlaying = true;

                                    if (IsAutoScrollToNowPlaying)
                                        // ScrollIntoView while don't change the selection 
                                        ScrollIntoView?.Invoke(this, curitem.Index);

                                }
                            }
                        }
                        else
                        {
                            CurrentSong = null;

                            //IsAlbumArtVisible = false;
                            AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                        }

                        UpdateProgress?.Invoke(this, "");

                        IsWorking = false;

                        UpdateProgress?.Invoke(this, "");

                    });
                    */
                    #endregion

                    #region == better way ==


                    IsWorking = true;

                    UpdateProgress?.Invoke(this, "[UI] Updating the queue...");

                    if (_mpc.CurrentQueue.Count == 0)
                    {
                        Queue.Clear();

                        CurrentSong = null;

                        //IsAlbumArtVisible = false;
                        AlbumCover = null;
                        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

                        UpdateProgress?.Invoke(this, "");

                        IsWorking = false;

                        return;
                    }

                    // tmp list of deletion
                    List<SongInfoEx> tmpQueue = [];

                    // deletes items that does not exists in the new queue. 
                    foreach (var sng in Queue)
                    {
                        UpdateProgress?.Invoke(this, "[UI] Queue list updating...(checking deleted items)");

                        IsWorking = true;

                        var queitem = _mpc.CurrentQueue.FirstOrDefault(i => i.Id == sng.Id);
                        if (queitem is null)
                        {
                            // add to tmp deletion list.
                            tmpQueue.Add(sng);
                        }
                    }

                    // loop the tmp deletion list and remove.
                    foreach (var hoge in tmpQueue)
                    {
                        UpdateProgress?.Invoke(this, "[UI] Queue list updating...(deletion)");

                        IsWorking = true;

                        Queue.Remove(hoge);
                    }

                    // update or add item from the new queue list.
                    foreach (var sng in _mpc.CurrentQueue)
                    {
                        UpdateProgress?.Invoke(this, $"[UI] Queue list updating...(checking and adding new items {sng.Id})");

                        IsWorking = true;

                        var fuga = Queue.FirstOrDefault(i => i.Id == sng.Id);
                        if (fuga is not null)
                        {
                            // this cuase strange selection problem.
                            //sng.IsSelected = fuga.IsSelected;
                            fuga.IsSelected = false; // so clear it for now.
                            fuga.IsPlaying = false;

                            // Just update.
                            //fuga = sng; // < sort won't work. why...

                            fuga.Pos = sng.Pos;
                            //fuga.Id = sng.Id;
                            fuga.LastModified = sng.LastModified;
                            //fuga.Time = sng.Time; // format exception
                            fuga.Title = sng.Title;
                            fuga.Artist = sng.Artist;
                            fuga.Album = sng.Album;
                            fuga.AlbumArtist = sng.AlbumArtist;
                            fuga.Composer = sng.Composer;
                            fuga.Date = sng.Date;
                            fuga.Duration = sng.Duration;
                            fuga.File = sng.File;
                            fuga.Genre = sng.Genre;
                            fuga.Track = sng.Track;

                            fuga.Index = sng.Index;

                        }
                        else
                        {
                            Queue.Add(sng);
                        }
                    }

                    // Sorting.
                    // Sort here because Queue list may have been re-ordered.
                    UpdateProgress?.Invoke(this, "[UI] Queue list sorting...");

                    // AvaloniaUI/WinUI3 doesn't have this.
                    //var collectionView = CollectionViewSource.GetDefaultView(Queue);
                    // no need to add because It's been added when "loading".
                    //collectionView.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
                    //collectionView.Refresh();

                    // This is not good, all the selections will be cleared, but no problem for Avalonia UI?.
                    Queue = new ObservableCollection<SongInfoEx>(Queue.OrderBy(n => n.Index));
                    //UpdateProgress?.Invoke(this, "");

                    UpdateProgress?.Invoke(this, "[UI] Checking current song after Queue update.");

                    // Set Current and NowPlaying.
                    var curitem = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                    if (curitem is not null)
                    {
                        bool asdf = false;
                        if (CurrentSong is not null)
                        {
                            if (CurrentSong.Id == curitem.Id)
                            {
                                asdf = curitem.IsAlbumCoverNeedsUpdate;
                            }
                        }
                        else
                        {
                            asdf = true;
                        }

                        CurrentSong = curitem;
                        CurrentSong.IsPlaying = true;
                        CurrentSong.IsAlbumCoverNeedsUpdate = asdf;

                        //if (IsAutoScrollToNowPlaying)
                        //{
                        //    // use ScrollIntoViewAndSelect instead of ScrollIntoView
                        //    ScrollIntoViewAndSelect?.Invoke(this, CurrentSong.Index);
                        //}

                        //AlbumArt
                        if (IsDownloadAlbumArt && CurrentSong.IsAlbumCoverNeedsUpdate)
                        {
                            var res = await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                            if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading) && (res.AlbumCover?.SongFilePath != null))
                            {
                                if (res.AlbumCover?.SongFilePath == CurrentSong.File)
                                {
                                    AlbumCover = res.AlbumCover;
                                    AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                    SaveAlbumCoverImage(CurrentSong, res.AlbumCover);
                                    CurrentSong.IsAlbumCoverNeedsUpdate = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        CurrentSong = null;

                        //IsAlbumArtVisible = false;
                        AlbumCover = null;
                        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                    }

                    UpdateProgress?.Invoke(this, "");

                    IsWorking = false;


                    #endregion
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception@UpdateCurrentQueue: " + e.Message);
                    UpdateProgress?.Invoke(this, "Exception@UpdateCurrentQueue: " + e.Message);
                    IsWorking = false;
                    App.AppendErrorLog("Exception@UpdateCurrentQueue", e.Message);

                    return;
                }
                finally
                {
                    IsWorking = false;
                    UpdateProgress?.Invoke(this, "");
                }

                IsWorking = false;
            }
            else
            {
                //Debug.WriteLine("Queue.Count == 0. @UpdateCurrentQueue()");

                UpdateProgress?.Invoke(this, "[UI] Loading the queue...");
                await Task.Delay(20);
                /*
                if (IsSwitchingProfile)
                    return;
                */
                IsWorking = true;

                try
                {
                    IsWorking = true;

                    UpdateProgress?.Invoke(this, "[UI] Loading the queue...");

                    Queue = new ObservableCollection<SongInfoEx>(_mpc.CurrentQueue);

                    UpdateProgress?.Invoke(this, "[UI] Queue checking current song...");

                    bool isNeedToFindCurrentSong = false;

                    if (CurrentSong is not null)
                    {
                        if (CurrentSong.Id != _mpc.MpdStatus.MpdSongID)
                        {
                            isNeedToFindCurrentSong = true;

                            CurrentSong.IsPlaying = false;
                        }
                        else
                        {
                            if (_mpc.MpdCurrentSong is not null)
                            {
                                // This means CurrentSong is already aquired by "currentsong" command.
                                if (_mpc.MpdCurrentSong.Id == _mpc.MpdStatus.MpdSongID)
                                {
                                    // Set Current(again) and NowPlaying.

                                    // the reason not to use CurrentSong is that it points different instance (set by "currentsong" command and currentqueue). 
                                    var curitem = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                                    if (curitem is not null)
                                    {
                                        CurrentSong = curitem;
                                        CurrentSong.IsPlaying = true;
                                        //CurrentSong.IsSelected = true;

                                        if (IsAutoScrollToNowPlaying)
                                            // use ScrollIntoViewAndSelect instead of ScrollIntoView
                                            ScrollIntoViewAndSelect?.Invoke(this, CurrentSong.Index);

                                        // AlbumArt
                                        if (IsDownloadAlbumArt && CurrentSong.IsAlbumCoverNeedsUpdate)
                                        {
                                            var res = await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                                            if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading) && (res.AlbumCover?.SongFilePath != null))
                                            {
                                                if (res.AlbumCover?.SongFilePath == CurrentSong.File)
                                                {
                                                    AlbumCover = res.AlbumCover;
                                                    AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                                    SaveAlbumCoverImage(CurrentSong, res.AlbumCover);
                                                    CurrentSong.IsAlbumCoverNeedsUpdate = false;
                                                }
                                            }
                                        }
                                    }
                                    /*
                                    // the reason not to use CurrentSong is that it points different instance (set by "currentsong" command and currentqueue). 
                                    _mpc.MpdCurrentSong.IsPlaying = true;

                                    // just in case. < no. don't override.
                                    //CurrentSong.IsPlaying = true;

                                    // currentsong command does not return pos, so it's needed to be set.
                                    CurrentSong.Index = _mpc.MpdCurrentSong.Index;

                                    _mpc.MpdCurrentSong.IsSelected = true;

                                    if (IsAutoScrollToNowPlaying)
                                        // use ScrollIntoViewAndSelect instead of ScrollIntoView
                                        ScrollIntoViewAndSelect?.Invoke(this, CurrentSong.Index);
                                    */
                                }
                                else
                                {
                                    Debug.WriteLine("_mpc.MpdCurrentSong.Id != _mpc.MpdStatus.MpdSongID. @UpdateCurrentQueue()");
                                    isNeedToFindCurrentSong = true;
                                }
                            }
                            else
                            {
                                //Debug.WriteLine("_mpc.MpdCurrentSong is null. @UpdateCurrentQueue()");
                                isNeedToFindCurrentSong = true;
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("CurrentSong is null. @UpdateCurrentQueue()");
                        isNeedToFindCurrentSong = true;
                    }

                    if (isNeedToFindCurrentSong)
                    {
                        // Set Current and NowPlaying.
                        var curitem = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                        if (curitem is not null)
                        {
                            //Debug.WriteLine($"Currentsong is set. {curitem.Title}. @UpdateCurrentQueue()");

                            CurrentSong = curitem;
                            CurrentSong.IsPlaying = true;
                            //CurrentSong.IsSelected = true;

                            if (IsAutoScrollToNowPlaying)
                                // use ScrollIntoViewAndSelect instead of ScrollIntoView
                                ScrollIntoViewAndSelect?.Invoke(this, CurrentSong.Index);

                            // AlbumArt
                            /*
                            if (_mpc.AlbumCover.SongFilePath != curitem.File)
                            {
                                //IsAlbumArtVisible = false;
                                //AlbumArt = _albumArtDefault;

                                if (!string.IsNullOrEmpty(CurrentSong.File))
                                {
                                    //isAlbumArtChanged = true;
                                }
                            }
                            */

                            // AlbumArt
                            if (IsDownloadAlbumArt && CurrentSong.IsAlbumCoverNeedsUpdate)
                            {
                                var res = await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                                if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading) && (res.AlbumCover?.SongFilePath != null))
                                {
                                    if (res.AlbumCover?.SongFilePath == CurrentSong.File)
                                    {
                                        AlbumCover = res.AlbumCover;
                                        AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                        SaveAlbumCoverImage(CurrentSong, res.AlbumCover);
                                        CurrentSong.IsAlbumCoverNeedsUpdate = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Looks like starting up with playback status Stop. @UpdateCurrentQueue()");
                            // just in case.
                            CurrentSong = null;
                            AlbumCover = null;
                            //IsAlbumArtVisible = false;
                            AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                        }
                    }

                    // Add SortDescription to the Listview.
                    UpdateProgress?.Invoke(this, "[UI] Queue list sorting...");
                    //var collectionView = CollectionViewSource.GetDefaultView(Queue);
                    //collectionView.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
                    //collectionView.Refresh();
                    UpdateProgress?.Invoke(this, "");

                    UpdateProgress?.Invoke(this, "");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception@UpdateCurrentQueue: " + e.Message);
                    //StatusBarMessage = "Exception@UpdateCurrentQueue: " + e.Message;
                    IsWorking = false;

                    App.AppendErrorLog("Exception@UpdateCurrentQueue", e.Message);

                    return;
                }
                finally
                {
                    IsWorking = false;
                    UpdateProgress?.Invoke(this, "");
                }


            }
            /*
            if (CurrentSong is not null)
                if (IsDownloadAlbumArt)
                    if (isAlbumArtChanged)
                    {
                        //UpdateProgress?.Invoke(this, "[UI] Queue QueryAlbumArt.");
                        await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                        //UpdateProgress?.Invoke(this, "");
                    }
            */
            /*
            if (IsDownloadAlbumArt)
            {
                //Debug.WriteLine("if (IsDownloadAlbumArt)");
                if (CurrentSong != null)
                {
                    //Debug.WriteLine("if (CurrentSong != null)" + " " + CurrentSong.File);
                    if (isAlbumArtChanged)
                    {
                        //Debug.WriteLine("if (isAlbumArtChanged)");
                        await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                    }
                }
            }
            */


            IsWorking = false;
        });
    }
    
    private void UpdateAlbumsAndArtists()
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Sort
            //CultureInfo ci = CultureInfo.CurrentCulture;
            //StringComparer comp = StringComparer.Create(ci, true);

            UpdateProgress?.Invoke(this, "[UI] Updating the AlbumArtists...");
            Artists = new ObservableCollection<AlbumArtist>(_mpc.AlbumArtists);// COPY. //.OrderBy(x => x.Name, comp)

            UpdateProgress?.Invoke(this, "[UI] Updating the Albums...");
            Albums = new ObservableCollection<AlbumEx>(_mpc.Albums); // COPY. // Sort .OrderBy(x => x.Name, comp)

            UpdateProgress?.Invoke(this, "");
            /* 
             * little bit too much to check all albums every time app starts.
            // Get album pictures.
            if (!IsAlbumArtsDownLoaded)
            {
                GetAlbumPictures(_albums);
            }
            */
        });
    }

    private void UpdatePlaylists()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            /*
            if (IsSwitchingProfile)
                return;
            */
            UpdateProgress?.Invoke(this, "[UI] Playlists loading...");

            await Task.Delay(10);

            bool isListChanged = false;
            //IsBusy = true;
            IsWorking = true;

            if (Playlists.Count == 0)
            {
                // this is the initial load, so use this flag to sort it later.
                isListChanged = true;
            }

            UpdateProgress?.Invoke(this, "[UI] Playlists loading...");
            Playlists = new ObservableCollection<Playlist>(_mpc.Playlists);
            UpdateProgress?.Invoke(this, "");

            NodeMenuPlaylists playlistDir = _mainMenuItems.PlaylistsDirectory;

            if (playlistDir is not null)
            {
                /*
                // Sort playlists.
                List<string> slTmp = [];

                foreach (var v in _mpc.Playlists)
                {
                    slTmp.Add(v.Name);
                }
                slTmp.Sort();
                */

                foreach (var hoge in Playlists)
                {
                    var fuga = playlistDir.Children.FirstOrDefault(i => i.Name == hoge.Name);
                    if (fuga is null)
                    {
                        NodeMenuPlaylistItem playlistNode = new(hoge.Name)
                        {
                            IsUpdateRequied = true
                        };
                        playlistDir.Children.Add(playlistNode);
                        isListChanged = true;
                    }
                }

                List<NodeTree> tobedeleted = [];
                foreach (var hoge in playlistDir.Children)
                {
                    var fuga = Playlists.FirstOrDefault(i => i.Name == hoge.Name);
                    if (fuga is null)
                    {
                        tobedeleted.Add(hoge);
                        isListChanged = true;
                    }
                    else
                    {
                        if (hoge is NodeMenuPlaylistItem nmpi)
                        {
                            nmpi.IsUpdateRequied = true;
                        }
                    }
                }

                foreach (var hoge in tobedeleted)
                {
                    playlistDir.Children.Remove(hoge);
                    isListChanged = true;
                }

                // Sort > this was causing NavigationViewItem selection to reset..so >> only isChanged then sort it.
                if (isListChanged && playlistDir.Children.Count > 1)
                {
                    CultureInfo ci = CultureInfo.CurrentCulture;
                    StringComparer comp = StringComparer.Create(ci, true);
                    // 
                    playlistDir.Children = new ObservableCollection<NodeTree>(playlistDir.Children.OrderBy(x => x.Name, comp));  //<<This freaking resets selection of NavigationViewItem!
                }

                // Update playlist if selected
                if (SelectedNodeMenu is NodeMenuPlaylistItem nmpli)
                {
                    if (nmpli.IsUpdateRequied && nmpli.Selected)
                    {
                        GetPlaylistSongs(nmpli);

                        if (isListChanged)
                        {
                            // TODO: need to check if this is needed.
                            GoToJustPlaylistPage(nmpli);
                        }
                    }
                }

                if (_isNavigationViewMenuOpen)
                {
                    playlistDir.Expanded = true;
                }
            }

            //IsBusy = false;
            IsWorking = false;

            // apply open/close after this menu is loaded.
            OnPropertyChanged(nameof(IsNavigationViewMenuOpen));

            if (!string.IsNullOrEmpty(RenamedSelectPendingPlaylistName))
            {
                GoToRenamedPlaylistPage(RenamedSelectPendingPlaylistName);
                RenamedSelectPendingPlaylistName = string.Empty;
            }
        });
    }

    private Task UpdateLibraryMusicAsync()
    {
        // Music files
        Dispatcher.UIThread.Post(async () => 
        {
            UpdateProgress?.Invoke(this, "[UI] Library songs loading...");

            //IsBusy = true;
            IsWorking = true;
            await Task.Yield();

            /*
            Dispatcher.UIThread.Post(() => {
                MusicEntries.Clear();
            });
            */

            var tmpMusicEntries = new ObservableCollection<NodeFile>();

            foreach (var songfile in _mpc.LocalFiles)
            {
                /*
                if (IsSwitchingProfile)
                    break;
                */
                //await Task.Delay(5);

                //if (IsSwitchingProfile)
                //    break;

                //IsBusy = true;
                IsWorking = true;
                await Task.Yield();

                if (string.IsNullOrEmpty(songfile.File)) continue;

                try
                {
                    Uri uri = new(@"file:///" + songfile.File);
                    if (uri.IsFile)
                    {
                        string filename = System.IO.Path.GetFileName(songfile.File);//System.IO.Path.GetFileName(uri.LocalPath);
                        NodeFile hoge = new(filename, uri, songfile.File);
                        /*

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MusicEntries.Add(hoge);
                        });
                        */
                        tmpMusicEntries.Add(hoge);
                    }
                    /*
                    if (IsSwitchingProfile)
                        break;
                    */
                }
                catch (Exception e)
                {
                    Debug.WriteLine(songfile + e.Message);

                    //IsBusy = false;
                    IsWorking = false;
                    await Task.Yield();
                    App.AppendErrorLog("Exception@UpdateLibraryMusic", e.Message);
                    return;// Task.FromResult(false);
                }
            }
            /*
            if (IsSwitchingProfile)
                return Task.FromResult(false);
            */
            //IsBusy = true;
            IsWorking = true;

            UpdateProgress?.Invoke(this, "[UI] Library songs loading...");
            MusicEntries = new ObservableCollection<NodeFile>(tmpMusicEntries);// COPY

            _musicEntriesFiltered = _musicEntriesFiltered = new ObservableCollection<NodeFile>(tmpMusicEntries);
            OnPropertyChanged(nameof(MusicEntriesFiltered));

            UpdateProgress?.Invoke(this, "");
            //IsBusy = false;
            IsWorking = false;
            await Task.Yield();
        });

        return Task.CompletedTask;
    }

    private Task UpdateLibraryDirectoriesAsync()
    {
        // Directories
        Dispatcher.UIThread.Post(async () => 
        {
            UpdateProgress?.Invoke(this, "[UI] Library directories loading...");

            //IsBusy = true;
            IsWorking = true;
            await Task.Yield();

            try
            {
                var tmpMusicDirectories = new DirectoryTreeBuilder("");
                //tmpMusicDirectories.Load([.. _mpc.LocalDirectories]);
                //_musicDirectories.Load(_mpc.LocalDirectories.ToList<String>());
                await tmpMusicDirectories.Load(_mpc.LocalDirectories);

                IsWorking = true;
                await Task.Yield();

                UpdateProgress?.Invoke(this, "[UI] Library directories loading...");

                MusicDirectories = new ObservableCollection<NodeTree>(tmpMusicDirectories.Children);// COPY
                if (MusicDirectories.Count > 0)
                {
                    if (MusicDirectories[0] is NodeDirectory nd)
                    {
                        _selectedNodeDirectory = nd;
                        OnPropertyChanged(nameof(SelectedNodeDirectory));
                    }
                }

                /*
                MusicDirectoriesSource = new HierarchicalTreeDataGridSource<NodeTree>(tmpMusicDirectories.Children)
                {
                    Columns =
                    {
                        new HierarchicalExpanderColumn<NodeTree>(
                            new TextColumn<NodeTree, string>("Directory", x => x.Name),
                            x => x.Children)
                    },
                };
                MusicDirectoriesSource.Expand(0);
                */

                UpdateProgress?.Invoke(this, "");

                //IsBusy = false;
                IsWorking = false;
                await Task.Yield();

            }
            catch (Exception e)
            {
                Debug.WriteLine("_musicDirectories.Load: " + e.Message);


                //IsBusy = false;
                IsWorking = false;
                await Task.Yield();
                App.AppendErrorLog("Exception@UpdateLibraryDirectories", e.Message);
                //return Task.FromResult(false);
            }
            finally
            {

                //IsBusy = false;
                IsWorking = false;
                await Task.Yield();
                UpdateProgress?.Invoke(this, "");
            }
        });

        return Task.CompletedTask;
    }

    private void GetPlaylistSongs(NodeMenuPlaylistItem playlistNode)
    {
        Dispatcher.UIThread.Post(async () => 
        {
            if (playlistNode is null)
                return;

            IsWorking = true;

            if (playlistNode.PlaylistSongs.Count > 0)
            {
                playlistNode.PlaylistSongs.Clear();
            }

            CommandPlaylistResult result = await _mpc.MpdQueryPlaylistSongs(playlistNode.Name);
            if (result.IsSuccess)
            {
                if (result.PlaylistSongs is not null)
                {
                    playlistNode.PlaylistSongs = new ObservableCollection<SongInfo>(result.PlaylistSongs);//result.PlaylistSongs

                    if (SelectedNodeMenu == playlistNode)
                    {
                        UpdateProgress?.Invoke(this, "[UI] Playlist loading...");
                        PlaylistSongs = playlistNode.PlaylistSongs; // just use this.
                        UpdateProgress?.Invoke(this, "");
                    }

                    playlistNode.IsUpdateRequied = false;
                }
            }

            IsWorking = false;
        });
    }

    private void GoToJustPlaylistPage(NodeMenuPlaylistItem playlist)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var hoge in MainMenuItems)
            {
                if (hoge is not NodeMenuPlaylists) continue;
                foreach (var fuga in hoge.Children)
                {
                    if (fuga is not NodeMenuPlaylistItem) continue;
                    if (fuga != playlist) continue;
                    if (!IsNavigationViewMenuOpen) continue;
                    fuga.Selected = true;
                    SelectedNodeMenu = fuga;
                    SelectedPlaylistName = fuga.Name;
                                    
                    break;
                }
            }
        });
    }

    private void GetFiles(NodeMenuFiles filestNode)
    {
        if (filestNode is null)
            return;

        if (filestNode.IsAcquired)
        {
            return;
        }

        if (MusicEntries.Count > 0)
            MusicEntries.Clear();

        if (MusicDirectories.Count > 0)
            MusicDirectories.Clear();

        filestNode.IsAcquired = true;

        Task.Run(async () =>
        {
            await Task.Delay(10);
            //await Task.Yield();

            Dispatcher.UIThread.Post(() =>
            {
                IsWorking = true;
            });
            await Task.Yield();

            CommandResult result = await _mpc.MpdQueryListAll().ConfigureAwait(false);
            if (result.IsSuccess)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    filestNode.IsAcquired = true;
                });

                //await UpdateLibraryMusicAsync().ConfigureAwait(false);
                //await UpdateLibraryDirectoriesAsync().ConfigureAwait(false);
                var dirTask = UpdateLibraryDirectoriesAsync();
                var fileTask = UpdateLibraryMusicAsync();
                await Task.WhenAll(dirTask, fileTask);
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    filestNode.IsAcquired = false;
                });
                Debug.WriteLine("fail to get MpdQueryListAll: " + result.ErrorMessage);
            }

            Dispatcher.UIThread.Post(() =>
            {
                IsWorking = false;
            });
        });
    }

    private void GetArtistSongs(AlbumArtist? artist)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (artist is null)
            {
                Debug.WriteLine("GetArtistSongs: artist is null, returning.");
                return;
            }

            IsWorking = true;
            await Task.Yield();

            var r = await SearchArtistSongs(artist.Name).ConfigureAwait(ConfigureAwaitOptions.None);

            UpdateProgress?.Invoke(this, "");

            if (!r.IsSuccess)
            {
                Debug.WriteLine("GetArtistSongs: SearchArtistSongs returned false, returning.");

                IsWorking = false;
                await Task.Yield();
                return;
            }
            if (artist is null)
            {
                Debug.WriteLine("GetArtistSongs: SelectedAlbumArtist is null, returning.");

                IsWorking = false;
                await Task.Yield();
                return;
            }

            if (r.SearchResult is null)
            {
                Debug.WriteLine("GetArtistSongs: SearchResult is null, returning.");
                IsWorking = false;
                await Task.Yield();
                return;
            }

            foreach (var slbm in artist.Albums)
            {
                if (artist is null)
                {
                    Debug.WriteLine("Artist is null, cannot add song to album.");
                    break;
                }

                if (SelectedAlbumArtist != artist)
                {
                    Debug.WriteLine("GetArtistSongs: SelectedAlbumArtist is not the same as artist, returning.");
                    break;
                    //return;
                }

                if (slbm.IsSongsAcquired)
                {
                    //Debug.WriteLine("GetArtistSongs: Album's song is already acquired, skipping.");
                    continue;
                }

                foreach (var song in r.SearchResult)
                {
                    if (song.Album.Equals(slbm.Name, StringComparison.CurrentCulture))
                    {
                        slbm.Songs.Add(song);
                    }
                }

                slbm.IsSongsAcquired = true;
            }

            IsWorking = false;
            await Task.Yield();
        });
    }

    private async Task<CommandSearchResult> SearchArtistSongs(string name)
    {
        if (name is null)
        {
            CommandSearchResult result = new()
            {
                IsSuccess = false,
                ErrorMessage = "SearchArtistSongs: name is null"
            };
            return result;
        }

        string queryShiki = "==";
        var res = await _mpc.MpdSearch("AlbumArtist", queryShiki, name); // AlbumArtist looks for VALUE in AlbumArtist and falls back to Artist tags if AlbumArtist does not exist. 

        if (!res.IsSuccess)
        {
            Debug.WriteLine("SearchArtistSongs failed: " + res.ErrorMessage);
        }
        else
        {
            //Debug.WriteLine(res.ResultText);
        }

        UpdateProgress?.Invoke(this, "");
        return res;
    }

    private async Task<CommandSearchResult> SearchAlbumSongs(string name)
    {
        if (name is null)
        {
            CommandSearchResult result = new()
            {
                IsSuccess = false,
                ErrorMessage = "SearchAlbumSongs: name is null"
            };
            return result;
        }

        string queryShiki = "==";
        var res = await _mpc.MpdSearch("Album", queryShiki, name); // No name.Trim() because of "=="

        if (!res.IsSuccess)
        {
            Debug.WriteLine("SearchAlbumSongs failed: " + res.ErrorMessage);
        }

        return res;
    }
    
    private void GetAlbumPictures(IEnumerable<object>? albumExItems)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            if (albumExItems is null)
            {
                return;
            }

            if (Albums.Count < 1)
            {
                //Debug.WriteLine("GetAlbumPictures: Albums.Count < 1, returning.");
                return;
            }

            //UpdateProgress?.Invoke(this, "[UI] Loading album covers ...");
            //IsBusy = true;
            //IsWorking = true;

            foreach (var item in albumExItems)
            {
                await Task.Yield();

                if (item is not AlbumEx album)
                {
                    continue;
                }
                if (album is null)
                {
                    //Debug.WriteLine("GetAlbumPictures: album is null, skipping.");
                    continue;
                }

                if (album.IsImageAcquired)
                {
                    //Debug.WriteLine($"GetAlbumPictures: {album.Name} IsImageAcquired is true, skipping.");
                    continue;
                }

                if (album.IsImageLoading)
                {
                    continue;
                }
                album.IsImageLoading = true;

                if (string.IsNullOrEmpty(album.Name.Trim()))
                {
                    Debug.WriteLine($"GetAlbumPictures: album.Name is null or empty, skipping. {album.AlbumArtist}");
                    continue;
                }

                var strArtist = album.AlbumArtist.Trim();
                if (string.IsNullOrEmpty(strArtist))
                {
                    strArtist = "Unknown Artist";
                }
                else
                {
                    strArtist = SanitizeFilename(strArtist);
                }

                var strAlbum = album.Name.Trim();
                if (string.IsNullOrEmpty(strAlbum))
                {
                    strAlbum = "Unknown Album";
                }
                else
                {
                    strAlbum = SanitizeFilename(strAlbum);
                }

                string filePath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".bmp";

                if (File.Exists(filePath))
                {
                    album.IsImageLoading = true;

                    try
                    {
                        Bitmap? bitmap = new(filePath);
                        album.AlbumImage = bitmap;
                        album.IsImageAcquired = true;
                        album.IsImageLoading = false;
                    }
                    catch (Exception e)
                    {
                        album.IsImageLoading = false;
                        Debug.WriteLine("GetAlbumPictures: Exception while loading: " + filePath + Environment.NewLine + e.Message);
                        continue;
                    }

                    //await Task.Yield();
                    await Task.Delay(5);
                    //await Task.Yield();

                    album.IsImageLoading = false;
                    //Debug.WriteLine($"GetAlbumPictures: Successfully loaded album art from cache {filePath}");
                }
                else
                {
                    album.IsImageLoading = true;

                    string fileTempPath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".tmp";
                    string strDirPath = System.IO.Path.Combine(App.AppDataCacheFolder, strArtist);

                    if (File.Exists(fileTempPath))
                    {
                        album.IsImageLoading = false;
                        continue; // Skip if temp file exists, it means the album art has found to have no image.
                    }

                    var ret = await SearchAlbumSongs(album.Name);
                    if (!ret.IsSuccess)
                    {
                        Debug.WriteLine("GetAlbumPictures: SearchAlbumSongs failed: " + ret.ErrorMessage);

                        album.IsImageLoading = false;
                        continue;
                    }

                    if (ret.SearchResult is null)
                    {
                        album.IsImageLoading = false;
                        Debug.WriteLine("GetAlbumPictures: ret.SearchResult is null, skipping.");
                        continue;
                    }

                    var sresult = new ObservableCollection<SongInfo>(ret.SearchResult);
                    if (sresult.Count < 1)
                    {
                        album.IsImageLoading = false;
                        Debug.WriteLine("GetAlbumPictures: ret.SearchResult.Count < 1, skipping. -> " + album.Name);
                        continue;
                    }

                    bool isWaitFailed = false;
                    bool isCoverExists = false;
                    bool isNoAlbumCover = false;

                    foreach (var albumsong in sresult)
                    {
                        await Task.Yield();

                        if (albumsong is null)
                        {
                            continue;
                        }

                        var aat = albumsong.AlbumArtist.Trim();
                        if (string.IsNullOrEmpty(aat))
                        {
                            aat = albumsong.Artist.Trim();
                        }
                        //if (aat == album.AlbumArtist)
                        if (string.Equals(aat, album.AlbumArtist, StringComparison.CurrentCulture))
                        {
                            //Debug.WriteLine($"GetAlbumPictures: Processing song {albumsong.File} from album {album.Name}");
                            var r = await _mpc.MpdQueryAlbumArtForAlbumView(albumsong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                            if (!r.IsSuccess)
                            {
                                isNoAlbumCover = r.IsNoBinaryFound;
                                isWaitFailed = r.IsWaitFailed;
                                album.IsImageLoading = false;
                                //Debug.WriteLine($"MpdQueryAlbumArtForAlbumView failed: {r.ErrorMessage}");
                                continue;
                            }
                            if (r.AlbumCover is null) continue;
                            if (!r.AlbumCover.IsSuccess) continue;

                            album.IsImageAcquired = true;
                            album.IsImageLoading = false;

                            //Dispatcher.UIThread.Post(() =>
                            //{
                            album.AlbumImage = r.AlbumCover.AlbumImageSource;
                            //});
                            Directory.CreateDirectory(strDirPath);
                            album.AlbumImage?.Save(filePath, 100);

                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {albumsong.File}");
                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {album.Name} by {album.AlbumArtist}");

                            //Debug.WriteLine(System.IO.Path.Combine(strArtist, strAlbum) + ".bmp");

                            isCoverExists = true;

                            // Testing
                            //await Task.Delay(10);
                            //await Task.Yield();

                            break; // Break after first successful album art retrieval.
                        }
                        else
                        {
                            //Debug.WriteLine($" {album.Name} > {album.AlbumArtist} : {albumsong.AlbumArtist},  {albumsong.Artist}");
                        }
                    }

                    // File saved. Don't save temp file.
                    if (isCoverExists) continue;

                    // WaitFiled. Don't save temp file.
                    if (isWaitFailed) continue;

                    if (isNoAlbumCover)
                    {
                        try
                        {
                            Directory.CreateDirectory(strDirPath);
                            DateTimeOffset dto = new(DateTime.UtcNow);
                            // Get the unix timestamp in seconds
                            var unixTime = dto.ToUnixTimeSeconds().ToString();

                            await using StreamWriter file = new(fileTempPath);
                            await file.WriteLineAsync(unixTime);
                            file.Close();
                            // Testing
                            await Task.Delay(10);
                            await Task.Yield();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("GetAlbumPictures: Exception while saving album art DUMMY file: " + e.Message);
                        }
                    }

                    album.IsImageLoading = false;

                    await Task.Yield();
                }
            }

            //UpdateProgress?.Invoke(this, "");
            //IsBusy = false;
            //IsWorking = false;
        });
    }

    /*
    private async void GetAlbumPictures(ObservableCollection<AlbumEx> albums)
    {
        if (albums.Count < 1)
        {
            Debug.WriteLine("GetAlbumPictures: Albums.Count < 1, returning.");
            return;
        }
        if (IsAlbumArtsDownLoaded)
        {
            return;
        }

        UpdateProgress?.Invoke(this, "[UI] Loading album covers ...");
        //IsBusy = true;
        IsWorking = true;

        IsAlbumArtsDownLoaded = true;

        foreach (var album in albums)
        {
            if (album is null)
            {
                Debug.WriteLine("GetAlbumPictures: album is null, skipping.");
                continue;
            }
            if (album.IsImageAcquired)
            {
                Debug.WriteLine("GetAlbumPictures: album.IsImageAcquired is true, skipping.");
                continue;
            }
            if (string.IsNullOrEmpty(album.Name.Trim()))
            {
                //Debug.WriteLine($"GetAlbumPictures: album.Name is null or empty, skipping. {album.AlbumArtist}");
                continue;
            }

            var strArtist = EscapeFilePathNames(album.AlbumArtist).Trim();
            var strAlbum = EscapeFilePathNames(album.Name).Trim();
            if (string.IsNullOrEmpty(strArtist))
            {
                strArtist = "Unknown Artist";
            }

            string strDirPath = System.IO.Path.Combine(App.AppDataCacheFolder, strArtist);
            string filePath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".bmp";
            string fileTempPath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".tmp";

            if (File.Exists(filePath))
            {
                try
                {
                    Bitmap? bitmap = new(filePath);
                    album.AlbumImage = bitmap;
                    album.IsImageAcquired = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("GetAlbumPictures: Exception while loading: " + filePath + Environment.NewLine + e.Message);
                    continue;
                }

                // Testing
                await Task.Delay(5);
                await Task.Yield();

                //Debug.WriteLine($"GetAlbumPictures: Successfully loaded album art from cache {filePath}");
            }
            else
            {
                if (File.Exists(fileTempPath))
                {
                    continue; // Skip if temp file exists, it means the album art has found to have no image.
                }

                bool isCoverExists = false;

                // TODO: Different artists have the same album name eg."Greatest Hits" like bob dylan and 2pac. Use better MpdSearch query.
                var ret = await SearchAlbumSongs(album.Name);
                if (!ret.IsSuccess)
                {
                    Debug.WriteLine("GetAlbumPictures: SearchAlbumSongs failed: " + ret.ErrorMessage);
                    continue;
                }

                if (ret.SearchResult is not null)
                {
                    var sresult = new ObservableCollection<SongInfo>(ret.SearchResult);
                    if (sresult.Count < 1)
                    {
                        Debug.WriteLine("GetAlbumPictures: ret.SearchResult.Count < 1, skipping. -> " + album.Name);
                        continue;
                    }

                    foreach (var albumsong in sresult)
                    {
                        if (albumsong is null)
                        {
                            continue;
                        }

                        var aat = albumsong.Artist.Trim();
                        if (string.IsNullOrEmpty(aat))
                        {
                            aat = albumsong.AlbumArtist;
                        }
                        if (aat == album.AlbumArtist)
                        {
                            Debug.WriteLine($"GetAlbumPictures: Processing song {albumsong.File} from album {album.Name}");
                            var r = await _mpc.MpdQueryAlbumArtForAlbumView(albumsong.File, true);
                            if (!r.IsSuccess) continue;
                            if (r.AlbumCover is null) continue;
                            if (!r.AlbumCover.IsSuccess) continue;
                            //Dispatcher.UIThread.Post(() =>
                            //{
                            album.AlbumImage = r.AlbumCover.AlbumImageSource;
                            album.IsImageAcquired = true;
                            //});

                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {albumsong.File}");
                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {album.Name} by {album.AlbumArtist}");

                            Directory.CreateDirectory(strDirPath);
                            album.AlbumImage?.Save(filePath, 100);

                            //Debug.WriteLine("GetAlbumPictures: Successfully saved at " + Path.Combine(AppDataCacheFolder, Path.Combine(strArtist, strAlbum)) + ".bmp");

                            isCoverExists = true;

                            // Testing
                            await Task.Delay(10);
                            await Task.Yield();

                            break; // Break after first successful album art retrieval.
                        }
                        else
                        {
                            //Debug.WriteLine($" {album.Name} > {album.AlbumArtist} : {albumsong.AlbumArtist},  {albumsong.Artist}");
                        }
                    }

                    if (isCoverExists) continue;

                    try
                    {
                        Directory.CreateDirectory(strDirPath);
                        DateTimeOffset dto = new(DateTime.UtcNow);
                        // Get the unix timestamp in seconds
                        var unixTime = dto.ToUnixTimeSeconds().ToString();

                        await using StreamWriter file = new(fileTempPath);
                        await file.WriteLineAsync(unixTime);
                        file.Close();

                        // Testing
                        await Task.Delay(10);
                        await Task.Yield();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("GetAlbumPictures: Exception while saving album art DUMMY file: " + e.Message);
                    }
                }
                else
                {
                    Debug.WriteLine("GetAlbumPictures: ret.SearchResult is null, skipping.");
                }
            }
        }
        Debug.WriteLine("Done loading AlbumCovers.");

        UpdateProgress?.Invoke(this, "");
        //IsBusy = false;
        IsWorking = false;
    }
    */

    private static void SaveAlbumCoverImage(SongInfoEx? current, AlbumImage? album)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if ((current?.File) != (album?.SongFilePath))
            {
                //Debug.WriteLine($"NOT ({current?.File} == {album?.SongFilePath})");
                return;
            }

            // save album cover to cache.
            //var strAlbum = current?.Album ?? string.Empty;

            var strArtist = current?.AlbumArtist.Trim();
            if (string.IsNullOrEmpty(strArtist))
            {
                strArtist = current?.Artist.Trim();
                if (string.IsNullOrEmpty(strArtist))
                {
                    strArtist = "Unknown Artist";
                }
            }
            strArtist = SanitizeFilename(strArtist);

            var strAlbum = current?.Album ?? string.Empty;
            if (string.IsNullOrEmpty(strAlbum))
            {
                strAlbum = "Unknown Album";
            }
            else
            {
                strAlbum = SanitizeFilename(strAlbum);
            }

            string strDirPath = System.IO.Path.Combine(App.AppDataCacheFolder, strArtist);
            string filePath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".bmp";
            try
            {
                Directory.CreateDirectory(strDirPath);
                album?.AlbumImageSource?.Save(filePath, 100);
                //Debug.WriteLine($"SaveAlbumCoverImage: saved album art {strArtist}, {strAlbum}");
            }
            catch (Exception e)
            {
                Debug.WriteLine("SaveAlbumCoverImage: Exception while saving album art: " + e.Message);
            }
        });
    }

    /*
    private static string EscapeFilePathNames(string str)
    {
        string s = str.Replace("<", "LT");
        s = s.Replace(">", "GT");
        s = s.Replace(":", "COL");
        s = s.Replace("\"", "DQ");
        s = s.Replace("/", "FS");
        s = s.Replace("\\", "BS");
        s = s.Replace("/", "FS");
        s = s.Replace("|", "PIP");
        s = s.Replace("?", "QM");
        s = s.Replace("*", "ASTR");

        return s;
    }
    */
    public static string SanitizeFilename(string name)
    {
        // 1. Get the list of invalid characters for the current system
        // and add additional common invalid path characters.
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // 2. Create a regex pattern to match invalid characters.
        // We escape the characters to ensure they are interpreted literally.
        string invalidCharsPattern = "[" + Regex.Escape(new string(invalidChars)) + "]";

        // 3. Replace all invalid characters with the replacement character.
        string sanitizedName = Regex.Replace(name, invalidCharsPattern, "_");

        // 4. Handle reserved Windows filenames (e.g., CON, PRN, NUL).
        string[] reservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];
        if (Array.Exists(reservedNames, s => s.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase)))
        {
            sanitizedName = $"_{sanitizedName}_";
        }

        // 5. Trim trailing periods and spaces, which are invalid on Windows.
        sanitizedName = sanitizedName.TrimEnd('.', ' ');

        // 6. Ensure the filename isn't empty after sanitizing.
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            return "Untitled";
        }

        return sanitizedName;
    }

    private static int CompareVersionString(string a, string b)
    {
        return (new System.Version(a)).CompareTo(new System.Version(b));
    }

    private static async Task<long> GetFolderSize(string path)
    {
        long totalSize = 0;

        if (!Directory.Exists(path))
        {
            return totalSize;
        }

        DirectoryInfo directoryInfo = new(path);

        // Add the size of files in the current directory
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            totalSize += file.Length;
        }

        // Recursively add the size of files in subdirectories
        foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
        {
            totalSize += await GetFolderSize(subDirectory.FullName);
        }

        //long totalSize = dInfo.EnumerateFiles().Sum(file => file.Length);
        //totalSize += dInfo.EnumerateDirectories().Sum(dir => GetDirectorySize(dir.FullName));

        return totalSize;
    }


    private static string ToFileSizeString(long size)
    {
        if (size < 1024)
        {
            return size.ToString("F0") + " bytes";
        }
        else if ((size >> 10) < 1024)
        {
            return (size / 1024F).ToString("F1") + " KB";
        }
        else if ((size >> 20) < 1024)
        {
            return ((size >> 10) / 1024F).ToString("F1") + " MB";
        }
        else if ((size >> 30) < 1024)
        {
            return ((size >> 20) / 1024F).ToString("F1") + " GB";
        }
        else if ((size >> 40) < 1024)
        {
            return ((size >> 30) / 1024F).ToString("F1") + " TB";
        }
        else if ((size >> 50) < 1024)
        {
            return ((size >> 40) / 1024F).ToString("F1") + " PB";
        }
        else
        {
            return ((size >> 50) / 1024F).ToString("F0") + " EB";
        }
    }

    public async Task GetCacheFolderSize()
    {
        AlbumCacheFolderSizeFormatted = ToFileSizeString(await GetFolderSize(App.AppDataCacheFolder).ConfigureAwait(true));
    }

    public static void DeleteAllContents(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
                System.Console.WriteLine($"Successfully deleted directory and its contents: {directoryPath}");
            }
            catch (IOException e)
            {
                System.Console.WriteLine($"Error deleting directory: {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                System.Console.WriteLine($"Access denied to directory: {e.Message}");
            }
        }
        else
        {
            System.Console.WriteLine($"Directory not found: {directoryPath}");
        }
    }

    #region == MPD event callback == 

    private void OnMpdIdleConnected(MpcService sender)
    {
        Debug.WriteLine("OK MPD " + _mpc.MpdVerText + " @OnMpdConnected");

        // Connected from InitWindow, so save and clean up. 
        Dispatcher.UIThread.Post( () =>
        {
            MpdVersion = _mpc.MpdVerText;

            //MpdStatusMessage = MpdVersion;// + ": " + MPDCtrlX.Properties.Resources.MPD_StatusConnected;

            MpdStatusButton = _pathMpdOkButton;

            IsConnected = true;

            IsShowAckWindow = false;
            IsShowErrWindow = false;

            if (_initWin is not null)
            {
                if (_initWin.IsActive || _initWin.IsVisible)
                {
                    if (IsRememberAsProfile)
                    {
                        var prof = new Profile
                        {
                            Name = _host,
                            Host = _host,
                            Port = _port,
                            Password = _password,
                            IsDefault = true,
                            Volume = _volume
                        };

                        if (!string.IsNullOrEmpty(prof.Host.Trim()))
                        {
                            prof.IsDefault = true;
                            CurrentProfile = prof;
                            Profiles.Add(prof);

                            //OnPropertyChanged(nameof(IsCurrentProfileSet));
                        }
                        else
                        {
                            Debug.WriteLine("Host info is empty. @OnMpdIdleConnected");
                        }
                    }
                    _initWin.Close();
                }
            }
        });

        // 
        _ = Task.Run(LoadInitialData);
    }

    private void OnMpdPlayerStatusChanged(MpcService sender)
    {
        if (_mpc.MpdStatus.MpdError != "")
        {
            MpdStatusMessage = MpdVersion + ": " + MPDCtrlX.Properties.Resources.MPD_StatusError + " - " + _mpc.MpdStatus.MpdError;
            MpdStatusButton = _pathMpdAckErrorButton;
        }
        else
        {
            MpdStatusMessage = "";
            MpdStatusButton = _pathMpdOkButton;
        }

        UpdateStatus();
    }

    private void OnMpdCurrentQueueChanged(MpcService sender)
    {
        UpdateCurrentQueue();
    }

    private void OnMpdPlaylistsChanged(MpcService sender)
    {
        UpdatePlaylists();
    }

    private void OnAlbumArtChanged(MpcService sender)
    {
        //
    }

    private void OnDebugCommandOutput(MpcService sender, string data)
    {
        if (IsShowDebugWindow)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DebugCommandOutput?.Invoke(this, data);
            });
        }
    }

    private void OnDebugIdleOutput(MpcService sender, string data)
    {
        if (IsShowDebugWindow)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DebugIdleOutput?.Invoke(this, data);
            });
        }
    }

    private void OnConnectionError(MpcService sender, string msg)
    {
        if (string.IsNullOrEmpty(msg))
            return;

        IsConnected = false;
        IsConnecting = false;
        IsConnectionSettingShow = true;

        ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_ConnectionError + ": " + msg;
        StatusButton = _pathErrorInfoButton;

        StatusBarMessage = ConnectionStatusMessage;

        InfoBarAckTitle = MPDCtrlX.Properties.Resources.ConnectionStatus_ConnectionError;
        InfoBarAckMessage = msg;
        IsShowAckWindow = true;
    }

    private void OnConnectionStatusChanged(MpcService sender, MpcService.ConnectionStatus status)
    {

        if (status == MpcService.ConnectionStatus.NeverConnected)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_NeverConnected;
            StatusButton = _pathDisconnectedButton;
        }
        else if (status == MpcService.ConnectionStatus.Connected)
        {
            IsConnected = true;
            IsConnecting = false;
            IsNotConnectingNorConnected = false;
            IsConnectionSettingShow = false;

            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_Connected;
            StatusButton = _pathConnectedButton;
        }
        else if (status == MpcService.ConnectionStatus.Connecting)
        {
            IsConnected = false;
            IsConnecting = true;
            IsNotConnectingNorConnected = false;
            //IsConnectionSettingShow = true;

            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_Connecting;
            StatusButton = _pathConnectingButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.ConnectFailTimeout)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_ConnectFail_Timeout");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_ConnectFail_Timeout;
            StatusButton = _pathErrorInfoButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.SeeConnectionErrorEvent)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;
            
            _elapsedTimer.Stop();

            Debug.WriteLine("ConnectionStatus_SeeConnectionErrorEvent");
            StatusButton = _pathErrorInfoButton;
        }
        else if (status == MpcService.ConnectionStatus.Disconnected)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_Disconnected");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_Disconnected;
            StatusButton = _pathErrorInfoButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.DisconnectedByHost)
        {
            // TODO: not really usued now...

            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_DisconnectedByHost");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_DisconnectedByHost;
            StatusButton = _pathErrorInfoButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.Disconnecting)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = false;
            //IsConnectionSettingShow = true;

            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_Disconnecting;
            StatusButton = _pathConnectingButton;

            StatusBarMessage = ConnectionStatusMessage;
            //Debug.WriteLine("ConnectionStatus_Disconnecting");
        }
        else if (status == MpcService.ConnectionStatus.DisconnectedByUser)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            //IsConnectionSettingShow = true;

            //Debug.WriteLine("ConnectionStatus_DisconnectedByUser");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_DisconnectedByUser;
            StatusButton = _pathDisconnectedButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.SendFailNotConnected)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_SendFail_NotConnected");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_SendFail_NotConnected;
            StatusButton = _pathErrorInfoButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.SendFailTimeout)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_SendFail_Timeout");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_SendFail_Timeout;
            StatusButton = _pathErrorInfoButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
    }

    private void OnMpdAckError(MpcService sender, string ackMsg, string origin)
    {
        if (string.IsNullOrEmpty(ackMsg))
            return;

        Debug.WriteLine($"MpdAckError: {ackMsg}");

        string s = ackMsg;
        string patternStr = @"[\[].+?[\]]";//@"[{\[].+?[}\]]";
        s = System.Text.RegularExpressions.Regex.Replace(s, patternStr, string.Empty);
        s = s.Replace("ACK ", string.Empty);
        s = s.Replace("{} ", string.Empty);

        /*
        Dispatcher.UIThread.Post(() =>
        {
            AckWindowOutput?.Invoke(this, MpdVersion + ": " + MPDCtrlX.Properties.Resources.MPD_CommandError + " - " + s + Environment.NewLine);
        });
        */

        if (origin.Equals("Command", StringComparison.OrdinalIgnoreCase))
        {
            InfoBarInfoTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_CommandResponse; 
        }
        else if (origin.Equals("Idle", StringComparison.OrdinalIgnoreCase))
        {
            InfoBarInfoTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_IdleResponse;
        }
        else
        {
            InfoBarInfoTitle = MpdVersion;
        }

        InfoBarInfoMessage = s;

        IsShowInfoWindow = true;
    }

    private void OnMpdFatalError(MpcService sender, string errMsg, string origin)
    {
        if (string.IsNullOrEmpty(errMsg))
            return;

        Debug.WriteLine($"MpdFatalError: {errMsg}");

        string s = errMsg;
        string patternStr = @"[\[].+?[\]]";//@"[{\[].+?[}\]]";
        s = System.Text.RegularExpressions.Regex.Replace(s, patternStr, string.Empty);
        s = s.Replace("ACK ", string.Empty);
        s = s.Replace("{} ", string.Empty);
        /*
        Dispatcher.UIThread.Post(() =>
        {
            ErrWindowOutput?.Invoke(this, MpdVersion + ": " + MPDCtrlX.Properties.Resources.MPD_CommandError + " - " + s + Environment.NewLine);
        });
        */

        if (origin.Equals("Command", StringComparison.OrdinalIgnoreCase))
        {
            InfoBarErrTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_CommandResponse;
        }
        else if (origin.Equals("Idle", StringComparison.OrdinalIgnoreCase))
        {
            InfoBarErrTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_IdleResponse;
        }
        else
        {
            InfoBarErrTitle = MpdVersion;
        }

        InfoBarErrMessage = s;

        IsShowErrWindow = true;
    }

    private void OnMpcProgress(MpcService sender, string msg)
    {
        StatusBarMessage = msg;
    }

    private void OnUpdateProgress(string msg)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusBarMessage = msg;
        });
    }

    private void OnMpcIsBusy(MpcService sender, bool on)
    {
        //this.IsBusy = on;
    }

    #endregion

    #endregion

    #region == Timers ==

    private readonly System.Timers.Timer _elapsedTimer;
    private void ElapsedTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if ((_elapsed < _time) && (_mpc.MpdStatus.MpdState == Status.MpdPlayState.Play))
        {
            Dispatcher.UIThread.Post(() =>
            {
                _elapsed += 1;
                OnPropertyChanged(nameof(Elapsed));
                OnPropertyChanged(nameof(ElapsedFormatted));
            });
            //Debug.WriteLine($"ElapsedTimer: {_elapsed}/{_time}");
        }
        else
        {
            _elapsedTimer.Stop();
        }
    }

    #endregion

    #region == Commands ==

    #region == Playback play ==

    [RelayCommand]
    public async Task Play()
    {
        if (IsBusy) return;

        if (Queue.Count < 1) { return; }

        switch (_mpc.MpdStatus.MpdState)
        {
            case Status.MpdPlayState.Play:
                {
                    //State>>Play: So, send Pause command
                    await _mpc.MpdPlaybackPause();
                    break;
                }
            case Status.MpdPlayState.Pause:
                {
                    //State>>Pause: So, send Resume command
                    await _mpc.MpdPlaybackResume(Convert.ToInt32(_volume));
                    break;
                }
            case Status.MpdPlayState.Stop:
                {
                    //State>>Stop: So, send Play command
                    await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume));
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [RelayCommand]
    public async Task PlayNext()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackNext(Convert.ToInt32(_volume));
    }

    [RelayCommand]
    public async Task PlayPrev()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackPrev(Convert.ToInt32(_volume));
    }

    [RelayCommand]
    public async Task ChangeSong()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) return;
        if (_selectedQueueSong is null) return;

        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), _selectedQueueSong.Id);
    }

    [RelayCommand]
    public async Task PlayPause()
    {
        if (IsBusy) return;
        await _mpc.MpdPlaybackPause();
    }

    [RelayCommand]
    public async Task PlayStop()
    {
        if (IsBusy) return;
        await _mpc.MpdPlaybackStop();
    }

    #endregion

    #region == Playback opts ==

    [RelayCommand]
    public async Task SetRandom()
    {
        if (IsBusy) return;
        await _mpc.MpdSetRandom(_random);
    }

    [RelayCommand]
    public async Task SetRpeat()
    {
        if (IsBusy) return;
        await _mpc.MpdSetRepeat(_repeat);
    }

    [RelayCommand]
    public async Task SetConsume()
    {
        if (IsBusy) return;
        await _mpc.MpdSetConsume(_consume);
    }

    [RelayCommand]
    public async Task SetSingle()
    {
        if (IsBusy) return;
        await _mpc.MpdSetSingle(_single);
    }

    #endregion

    #region == Playback seek and volume ==

    [RelayCommand]
    public async Task SetVolume()
    {
        if (IsBusy) return;
        await _mpc.MpdSetVolume(Convert.ToInt32(_volume));
    }

    [RelayCommand]
    public async Task SetSeek()
    {
        if (IsBusy) return;
        double elapsed = _elapsed / _elapsedTimeMultiplier;
        await _mpc.MpdPlaybackSeek(_mpc.MpdStatus.MpdSongID, elapsed);
    }

    [RelayCommand]
    public async Task VolumeMute()
    {
        if (IsBusy) return;
        await _mpc.MpdSetVolume(0);
    }

    [RelayCommand]
    public void VolumeDown()
    {
        if (IsBusy) return;
        if (_volume >= 10)
        {
            Volume -= 10;
            //await _mpc.MpdSetVolume(Convert.ToInt32(_volume - 10));
        }
        else
        {
            Volume = 0;
        }
    }

    [RelayCommand]
    public void VolumeUp()
    {
        if (IsBusy) return;
        if (_volume <= 90)
        {
            Volume += 10;
            //await _mpc.MpdSetVolume(Convert.ToInt32(_volume + 10));
        }
        else
        {
            Volume = 100;
        }
    }

    #endregion

    #region == Queue ==

    // called from code behind.
    public async void QueueSaveAsDialog_Execute(string playlistName)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;
        if (Queue.Count == 0)
            return;

        await _mpc.MpdSave(playlistName);
    }

    // Save to
    [RelayCommand]
    public async Task QueueSaveTo()
    {
        if (Queue.Count == 0)
            return;

        var result = await _dialog.ShowAddToDialog(this);

        if (result is null)
        {
            return;
        }

#pragma warning disable IDE0305
        _ = AddTo(result.PlaylistName, Queue.Select(s => s.File).ToList());
#pragma warning restore IDE0305

        //QueueSaveToDialogShow?.Invoke(this, EventArgs.Empty);
    }

    public async Task AddTo(string playlistName, List<string> uris)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;
        if (uris.Count == 0)
            return;

        await _mpc.MpdPlaylistAdd(playlistName, uris);
    }

    /*
    // common used by many
    public async void AddToPlaylist_Execute(string playlistName, List<string> list)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;

        if (list.Count < 1)
            return;

        await _mpc.MpdPlaylistAdd(playlistName, list);
    }
    */
    /*
    // called from code behind.
    public async void QueueSaveToDialog_Execute(string playlistName)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;
        if (Queue.Count == 0)
            return;

#pragma warning disable IDE0305
        await _mpc.MpdPlaylistAdd(playlistName, Queue.Select(s => s.File).ToList());
#pragma warning restore IDE0305
    }
    */


    // Add selected to playlist
    [RelayCommand]
    public async Task QueueListviewSaveSelectedTo(object obj)
    {
        if (obj is null)
        {
            return;
        }

        System.Collections.IList items = (System.Collections.IList)obj;

        if (items.Count == 0)
        {
            return;
        }

        var collection = items.Cast<SongInfoEx>();

        List<string> selectedList = [];

        foreach (var item in collection)
        {
            selectedList.Add(item.File);
        }

        //QueueListviewSaveToDialogShow?.Invoke(this, selectedList);

        var result = await _dialog.ShowAddToDialog(this);

        if (result is null)
        {
            return;
        }

        _ = AddTo(result.PlaylistName, selectedList);
    }

    // TODO:  Enter or double click from code behind.
    [RelayCommand]
    public async Task QueueListviewEnterKey()
    {
        if (Queue.Count < 1)
        {
            return;
        }
        if (_selectedQueueSong is null)
        {
            return;
        }

        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), _selectedQueueSong.Id);
    }

    // TODO: Actually used as ContextMenu Play.
    [RelayCommand]
    public async Task QueueListviewLeftDoubleClick(SongInfoEx song)
    {
        if (Queue.Count < 1)
        {
            return;
        }
        if (_selectedQueueSong is null)
        {
            return;
        }
        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), song.Id);
    }

    // Command to clear the queue listview.
    [RelayCommand]
    public async Task QueueClearWithoutPrompt()
    {
        if (Queue.Count == 0) { return; }

        await _mpc.MpdPlaybackStop();
        await _mpc.MpdClear();
    }

    // Command to delete selected songs from the queue listview.
    [RelayCommand]
    public async Task QueueListviewDeleteSelectedWithoutPrompt(object obj)
    {
        if (IsBusy) return;
        if (IsWorking) return;

        if (obj is null) return;
        System.Collections.IList items = (System.Collections.IList)obj;

        var collection = items.Cast<SongInfoEx>();

        List<string> deleteIdList = [];
        foreach (var item in collection)
        {
            deleteIdList.Add((item as SongInfoEx).Id);
        }
        // or
        //deleteIdList.AddRange(collection.Select(item => (item as SongInfoEx).Id));

        switch (deleteIdList.Count)
        {
            case 1:
                await _mpc.MpdDeleteId(deleteIdList[0]);
                break;
            case >= 1:
                await _mpc.MpdDeleteId(deleteIdList);
                break;
        }
    }

    // Move selected songs up in the queue listview.
    [RelayCommand]
    public async Task QueueListviewMoveUp(object obj)
    {
        if (obj is null) return;

        if (Queue.Count <= 1) return;

        if (obj is not SongInfoEx song) return;
        
        Dictionary<string, string> idToNewPos = [];

        try
        {
            int i = Int32.Parse(song.Pos);

            if (i == 0) return;

            i -= 1;

            idToNewPos.Add(song.Id, i.ToString());
        }
        catch
        {
            return;
        }

        await _mpc.MpdMoveId(idToNewPos);
    }

    // Move down
    [RelayCommand]
    public async Task QueueListviewMoveDown(object obj)
    {
        if (obj is null) return;

        if (Queue.Count <= 1)
            return;

        if (obj is not SongInfoEx song) return;
        
        Dictionary<string, string> idToNewPos = [];

        try
        {
            var i = Int32.Parse(song.Pos);

            if (i >= Queue.Count - 1) return;

            i += 1;

            idToNewPos.Add(song.Id, i.ToString());
        }
        catch
        {
            return;
        }

        await _mpc.MpdMoveId(idToNewPos);
    }

    // Sort reverse
    [RelayCommand]
    public async Task QueueListviewSortReverse()
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (Queue.Count <= 1) return;

        var sorted = new ObservableCollection<SongInfoEx>(Queue.OrderByDescending(x => x.Index));

        Dictionary<string, string> idToNewPos = [];
        int i = 0;
        foreach (var item in sorted)
        {
            idToNewPos.Add(item.Id, i.ToString());
            i++;
        }

        await _mpc.MpdMoveId(idToNewPos);
    }

    // Sort
    [RelayCommand]
    public async Task QueueListviewSortBy(string key)
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (Queue.Count == 0) return;

        if (string.IsNullOrEmpty(key))
            return;

        if (Queue.Count <= 1) return;

        var ci = CultureInfo.CurrentCulture;
        var comp = StringComparer.Create(ci, true);

        ObservableCollection<SongInfoEx>? sorted;
        switch (key)
        {
            case "title":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.Title, comp));
                break;
            case "time":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.TimeSort));
                break;
            case "artist":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.Artist, comp));
                break;
            case "album":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.Album, comp));
                break;
            case "disc":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.DiscSort));
                break;
            case "track":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.TrackSort));
                break;
            case "genre":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.Genre, comp));
                break;
            case "lastmodified":
                sorted = new ObservableCollection<SongInfoEx>(Queue.OrderBy(x => x.LastModified));
                break;
            default:
                return;
        }

        Dictionary<string, string> idToNewPos = [];
        int i = 0;
        foreach (var item in sorted)
        {
            idToNewPos.Add(item.Id, i.ToString());
            i++;
        }

        await _mpc.MpdMoveId(idToNewPos);
    }

    // ScrollIntoNowPlaying
    [RelayCommand]
    public void ScrollIntoNowPlaying()
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (Queue.Count == 0) return;
        if (CurrentSong is null) return;
        if (Queue.Count < CurrentSong.Index + 1) return;

        _mainMenuItems.QueueDirectory.Selected = true;
        ScrollIntoView?.Invoke(this, CurrentSong.Index);
    }

    public bool FilterQueueClearCanExecute()
    {
        if (string.IsNullOrEmpty(FilterQueueQuery))
            return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(FilterQueueClearCanExecute))]
    public void FilterQueueClear()
    {
        FilterQueueQuery = "";
    }

    [RelayCommand]
    public void QueueFindShowHide()
    {
        if (IsQueueFindVisible)
        {
            IsQueueFindVisible = false;
            return;
        }

        if (Queue is null)
        {
            return;
        }

        SelectedQueueFilterSong = null;
        QueueForFilter.Clear();

        QueueForFilter = new ObservableCollection<SongInfoEx>(Queue);
        /*
        var collectionView = CollectionViewSource.GetDefaultView(QueueForFilter);
        collectionView.Filter = x =>
        {
            return false;
        };
        */
        FilterQueueQuery = "";

        IsQueueFindVisible = true;

        // Set focus textbox in code behind.
        QueueFindWindowVisibilityChangedSetFocus?.Invoke(this, EventArgs.Empty);
    }

    //
    [RelayCommand]
    public void QueueFilterSelect()//object obj
    {
        if (Queue.Count <= 1)
            return;
        if (SelectedQueueFilterSong is null)
        {
            return;
        }

        //IsQueueFindVisible = false;

        ScrollIntoViewAndSelect?.Invoke(this, SelectedQueueFilterSong.Index);
        
        /*
        if (obj is null)
            return;

        if (obj is null) return;

        if (Queue.Count <= 1)
            return;

        
        if (obj is SongInfoEx song)
        {
            //IsQueueFindVisible = false;

            ScrollIntoViewAndSelect?.Invoke(this, song.Index);
        }
        */
    }

    #endregion

    #region == Search ==

    [RelayCommand]
    public async Task SearchExec()
    {
        // Allow empty string.
        //if (string.IsNullOrEmpty(SearchQuery)) return; 

        string queryShiki = "contains";
        if (SelectedSearchShiki.Shiki == SearchShiki.Equals)
        {
            queryShiki = "==";
        }

        var res = await _mpc.MpdSearch(SelectedSearchTag.Key.ToString(), queryShiki, SearchQuery);
        if (res.IsSuccess)
        {
            if (res.SearchResult is not null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    this.SearchResult = new ObservableCollection<SongInfo>(res.SearchResult); // COPY ON PURPOSE
                });
            }
            else
            {
                Debug.WriteLine("Search result is null.");
            }
        }
        else
        {
            Debug.WriteLine("Search failed: " + res.ErrorMessage);
            this.SearchResult?.Clear();
        }

        UpdateProgress?.Invoke(this, "");
    }

    // Save to
    [RelayCommand]
    public async Task SearchListviewSaveSelectedTo(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        var collection = items.Cast<SongInfo>();

        List<string> uriList = [];

        foreach (var item in collection)
        {
            uriList.Add(item.File);
        }

        if (uriList.Count > 0)
        {
            //SearchPageAddToPlaylistDialogShow?.Invoke(this, uriList);

            var result = await _dialog.ShowAddToDialog(this);

            if (result is null)
            {
                return;
            }

            _ = AddTo(result.PlaylistName, uriList);
        }
    }

    // Add to playlist
    [RelayCommand]
    public async Task SearchPageSongsAddToPlaylist(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<SongInfo> list)
        {
            List<string> uriList = [];

            foreach (var item in list)
            {
                uriList.Add(item.File);
            }

            if (uriList.Count > 0)
            {
                //SearchPageAddToPlaylistDialogShow?.Invoke(this, uriList);
                var result = await _dialog.ShowAddToDialog(this);

                if (result is null)
                {
                    return;
                }

                _ = AddTo(result.PlaylistName, uriList);
            }
        }
    }

    #endregion

    #region == Files ==

    [RelayCommand]
    public async Task FilesListviewAddSelectedToQueue(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        if (items.Count > 1)
        {
            var collection = items.Cast<NodeFile>();

            List<string> uriList = [];

            foreach (var item in collection)
            {
                uriList.Add((item as NodeFile).OriginalFileUri);
            }

            await _mpc.MpdAdd(uriList);
        }
        else
        {
            if ((items.Count == 1) && (items[0] is NodeFile nf))
                await _mpc.MpdAdd(nf.OriginalFileUri);
        }
    }

    // Save to
    [RelayCommand]
    public async Task FilesListviewSaveSelectedTo(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        var collection = items.Cast<NodeFile>();

        List<string> uriList = [];

        foreach (var item in collection)
        {
            uriList.Add(item.OriginalFileUri);
        }

        if (uriList.Count == 0)
        {
            return;
        }

        //FilesPageAddToPlaylistDialogShow?.Invoke(this, uriList);

        var result = await _dialog.ShowAddToDialog(this);

        if (result is null)
        {
            return;
        }

        _ = AddTo(result.PlaylistName, uriList);
    }

    [RelayCommand]
    public static async Task FilesListviewCopySelectedFilePath(object obj)
    {
        if (obj is null) return;

        if (obj is not NodeFile file)
        {
            return;
        }

        if (string.IsNullOrEmpty(file.FilePath))
        {
            return;
        }

        var mainWin = App.GetService<MainWindow>();
        var clipboard = TopLevel.GetTopLevel(mainWin)?.Clipboard;
        if (clipboard != null)
        {
            string originalString = file.FilePath;
            string newString = originalString.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (!newString.EndsWith(System.IO.Path.DirectorySeparatorChar))
            {
                newString += System.IO.Path.DirectorySeparatorChar;
            }
            newString += file.Name;
            await clipboard.SetTextAsync(newString);
        }
    }

    // Play all
    [RelayCommand]
    public async Task FilesPlay(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<NodeFile> list)
        {
            if (list.Count > 0)
            {
                Dispatcher.UIThread.Post(() => {
                    Queue.Clear();
                    CurrentSong = null;        
                });

                List<string> uriList = [];

                foreach (var song in list)
                {
                    uriList.Add(song.OriginalFileUri);
                }

                await _mpc.MpdMultiplePlay(uriList, Convert.ToInt32(_volume));

                // get album cover.
                await Task.Yield();
                await Task.Delay(200);
                UpdateCurrentSong();
            }
        }
    }

    // Queue all
    [RelayCommand]
    public async Task FilesAddToQueue(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<NodeFile> list)
        {
            if (list.Count > 1)
            {
                List<string> uriList = [];

                foreach (var song in list)
                {
                    uriList.Add(song.OriginalFileUri);
                }

                await _mpc.MpdAdd(uriList);
            }
            else
            {
                if ((list.Count == 1) && (list[0] is NodeFile si))
                {
                    await _mpc.MpdAdd(si.OriginalFileUri);
                }
            }
        }
    }

    // Add to playlist
    [RelayCommand]
    public async Task FilesPageFilesAddToPlaylist(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<NodeFile> list)
        {
            List<string> uriList = [];

            foreach (var item in list)
            {
                uriList.Add(item.OriginalFileUri);
            }

            if (uriList.Count > 0)
            {
                //FilesPageAddToPlaylistDialogShow?.Invoke(this, uriList);

                var result = await _dialog.ShowAddToDialog(this);

                if (result is null)
                {
                    return;
                }

                _ = AddTo(result.PlaylistName, uriList);
            }
        }
    }

    [RelayCommand]
    public async Task FilesListviewPlayThis(object obj)
    {
        if (obj is null) return;

        if (obj is NodeFile song)
        {
            Dispatcher.UIThread.Post(() => {
                Queue.Clear();
                CurrentSong = null;
            });

            await _mpc.MpdSinglePlay(song.OriginalFileUri, Convert.ToInt32(_volume));

            // get album cover.
            await Task.Yield();
            await Task.Delay(200);
            UpdateCurrentSong();
        }
    }

    [RelayCommand]
    public async Task FilesListviewAddThis(object obj)
    {
        if (obj is null) return;

        if (obj is NodeFile song)
        {
            await _mpc.MpdAdd(song.OriginalFileUri);
        }
    }

    #endregion

    #region == Albums ==

    [RelayCommand]
    public async Task AlbumsItemInvoked(object obj)
    {
        if (obj is null)
        {
            Debug.WriteLine("AlbumsItemInvokedCommand: obj is null, returning.");
            return;
        }

        if (obj is not AlbumEx)
        {
            Debug.WriteLine("AlbumsItemInvokedCommand: obj is not AlbumEx, returning.");
            return;
        }

        if (obj is not AlbumEx album)
        {
            Debug.WriteLine("AlbumsItemInvokedCommand: album is null, returning.");
            return;
        }

        //Debug.WriteLine("AlbumsItemInvokedCommand: Invoked with Album name: " + album.Name);

        if (!album.IsSongsAcquired)
        {
            //album.IsSongsAcquired = true;

            if (!string.IsNullOrEmpty(album.AlbumArtist.Trim()))
            {
                //Debug.WriteLine($"AlbumsItemInvokedCommand: Album artist is not empty, searching by album artist. ({album.AlbumArtist})");
                var r = await SearchArtistSongs(album.AlbumArtist);//.ConfigureAwait(ConfigureAwaitOptions.None);// no trim() here.

                if (r.IsSuccess)
                {
                    if (r.SearchResult is null)
                    {
                        Debug.WriteLine("AlbumsItemInvokedCommand: SearchResult is null, returning.");
                        return;
                    }

                    foreach (var song in r.SearchResult)
                    {
                        if ((song.AlbumArtist.Equals(album.AlbumArtist, StringComparison.CurrentCulture)) || (song.Artist.Equals(album.AlbumArtist, StringComparison.CurrentCulture)))
                        {
                            //if (song.Album.Trim() == album.Name.Trim())
                            if (song.Album.Equals(album.Name))
                            {
                                //Debug.WriteLine($"{song.Album}=={album.Name}?...{song.Title}");
                                album.Songs.Add(song);
                            }
                        }
                    }
                    album.IsSongsAcquired = true;

                    //InvokedAlbum = album;
                    SelectedAlbum = album;
                    await Task.Yield();
                    IsAlbumContentPanelVisible = true;

                    /*
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (r.SearchResult is null)
                        {
                            Debug.WriteLine("AlbumsItemInvokedCommand: SearchResult is null, returning.");
                            return;
                        }

                        foreach (var song in r.SearchResult)
                        {
                            if ((song.AlbumArtist == album.AlbumArtist) || (song.Artist == album.AlbumArtist))
                            {
                                if (song.Album.Trim() == album.Name.Trim())
                                {
                                    album.Songs.Add(song);
                                }
                            }
                        }
                        album.IsSongsAcquired = true;

                        //InvokedAlbum = album;
                        SelectedAlbum = album;
                        IsAlbumContentPanelVisible = true;
                    });
                    */
                }
                else
                {
                    Debug.WriteLine("AlbumsItemInvokedCommand: SearchArtistSongs returned false, returning.");
                    return;
                }
            }
            else
            {
                Debug.WriteLine($"AlbumsItemInvokedCommand: No album artist, trying to search by album name. ({album.Name})");

                if (!string.IsNullOrEmpty(album.Name.Trim()))
                {
                    var r = await SearchAlbumSongs(album.Name); // no trim() here.
                    if (r.IsSuccess)
                    {
                        if (r.SearchResult is null)
                        {
                            Debug.WriteLine("GetAlbumArtistSongs: SearchResult is null, returning.");
                            return;
                        }

                        foreach (var song in r.SearchResult)
                        {
                            album.Songs.Add(song);
                        }
                        album.IsSongsAcquired = true;

                        //InvokedAlbum = album;
                        SelectedAlbum = album;
                        await Task.Yield();
                        IsAlbumContentPanelVisible = true;
                        /*
                        Dispatcher.UIThread.Post(() =>
                        {

                        });
                        */
                    }
                    else
                    {
                        Debug.WriteLine("GetAlbumArtistSongs: SearchArtistSongs returned false, returning.");
                        return;
                    }
                }
                else
                {
                    Debug.WriteLine("AlbumsItemInvokedCommand: No album name, no artist name.");
                    // This should not happen.
                }
            }
        }
        else
        {
            //InvokedAlbum = album;
            SelectedAlbum = album;
            await Task.Delay(50);
            IsAlbumContentPanelVisible = true;
        }

        UpdateProgress?.Invoke(this, "");
    }

    [RelayCommand]
    public void AlbumsCloseAlbumContentPanel()
    {
        IsAlbumContentPanelVisible = false;
    }

    #endregion

    #region == Playlist ==

    [RelayCommand]
    public async Task PlaylistLoadPlaylist()
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (SelectedNodeMenu is null)
            return;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem)
            return;

        await _mpc.MpdLoadPlaylist(SelectedNodeMenu.Name);
    }

    [RelayCommand]
    public async Task PlaylistClearLoadPlaylist()
    {
        /*
        if (IsBusy)
            return;
        if (IsWorking) return;
        */
        if (SelectedNodeMenu is null)
            return;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem)
            return;

        Dispatcher.UIThread.Post(() => {
            Queue.Clear();
            CurrentSong = null;        
        });

        //await _mpc.MpdPlaybackStop(); // nomore needed -> changed _mpc.MpdChangePlaylist

        await _mpc.MpdChangePlaylist(SelectedNodeMenu.Name, Convert.ToInt32(_volume));

        //await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume)); // nomore needed -> changed _mpc.MpdChangePlaylist

        // get album cover.
        //await _mpc.MpdIdleQueryCurrentSong();// nomore needed -> changed _mpc.MpdChangePlaylist
        await Task.Yield();
        await Task.Delay(200);
        UpdateCurrentSong();
    }

    [RelayCommand]
    public void PlaylistRenamePlaylist(string playlist)
    {
        if (string.IsNullOrEmpty(_selectedPlaylistName))
        {
            return;
        }

        if (_selectedPlaylistName != playlist)
        {
            return;
        }

        PlaylistRenameToDialogShow?.Invoke(this, playlist);
    }

    // Rename playlist.
    public async void PlaylistRenamePlaylist_Execute(string oldPlaylistName, string newPlaylistName)
    {
        var ret = await _mpc.MpdRenamePlaylist(oldPlaylistName, newPlaylistName);

        if (ret.IsSuccess)
        {
            //SelectedPlaylistName = newPlaylistName;
            RenamedSelectPendingPlaylistName = newPlaylistName;

            // This is not going to work because renamed listviewitem is not yet created.
            //GoToRenamedPlaylistPage(newPlaylistName);
        }
    }

    private void GoToRenamedPlaylistPage(string playlist)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RenamedSelectPendingPlaylistName = string.Empty;
            _playlistPageSubTitleSongCount = ""; 
            OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));

            /*
            foreach (var fuga in _mainMenuItems.PlaylistsDirectory.Children)
            {
                if (fuga is NodeMenuPlaylistItem)
                {
                    if (string.Equals(playlist, fuga.Name))
                    {
                        Debug.WriteLine($"{playlist} is now selected....");
                        IsNavigationViewMenuOpen = true;
                        fuga.Selected = true;
                        SelectedNodeMenu = fuga;
                        SelectedPlaylistName = fuga.Name;
                        break;
                    }
                }
            }
            */

            foreach (var hoge in MainMenuItems)
            {
                if (hoge is NodeMenuPlaylists)
                {
                    foreach (var fuga in hoge.Children)
                    {
                        if (fuga is NodeMenuPlaylistItem)
                        {
                            if (string.Equals(playlist, fuga.Name))
                            {
                                Debug.WriteLine($"{playlist} is now selected....");
                                IsNavigationViewMenuOpen = true;
                                fuga.Selected = true;
                                SelectedNodeMenu = fuga;
                                SelectedPlaylistName = fuga.Name;
                                break;
                            }
                        }
                    }
                }
            }
        });

    }

    // CheckPlaylistNameExists when Rename playlists.
    public bool CheckIfPlaylistExists(string playlistName)
    {
        bool match = false;

        if (Playlists.Count > 0)
        {
            foreach (var hoge in Playlists)
            {
                if (string.Equals(playlistName, hoge.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    match = true;
                    break;
                }
            }
        }

        return match;
    }

    [RelayCommand]
    public async Task PlaylistRemovePlaylistWithoutPrompt(string playlist)
    {
        if (string.IsNullOrEmpty(_selectedPlaylistName))
        {
            return;
        }

        if (_selectedPlaylistName != playlist)
        {
            return;
        }

        var ret = await _mpc.MpdRemovePlaylist(_selectedPlaylistName);

        if (ret.IsSuccess)
        {
            SelectedPlaylistName = string.Empty;
            RenamedSelectPendingPlaylistName = string.Empty;
            // Clear listview 
            PlaylistSongs.Clear();
            //CurrentPage = App.GetService<QueuePage>();

            _playlistPageSubTitleSongCount = string.Empty;
            OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));
        }
    }

    // Deletes song in a playlist.
    [RelayCommand]
    public async Task PlaylistListviewDeleteSelectedWithoutPrompt(object obj)
    {
        /*
        // This caused problem
        if (string.IsNullOrEmpty(_selectedPlaylistName))
        {
            Debug.WriteLine("string.IsNullOrEmpty(_selectedPlaylistName) @PlaylistListviewDeleteSelectedWithoutPromptCommand_Execute");
            return;
        }
        */

        if (SelectedNodeMenu is NodeMenuPlaylistItem nmpli)
        {
            if (nmpli.IsUpdateRequied)
            {
                Debug.WriteLine("nmpli.IsUpdateRequied @PlaylistListviewDeleteSelectedWithoutPromptCommand_Execute");
                return;
            }
            else 
            {
                if (obj is SongInfo song)
                {
                    await _mpc.MpdPlaylistDelete(nmpli.Name, song.Index);
                }
            }
        }
        else
        {
            Debug.WriteLine("SelectedNodeMenu is NOT NodeMenuPlaylistItem nmpli @PlaylistListviewDeleteSelectedWithoutPromptCommand_Execute");
            return;
        }
    }

    // Playlist Clear
    [RelayCommand]
    public async Task PlaylistClearPlaylistWithoutPrompt(string playlist)
    {
        if (string.IsNullOrEmpty(_selectedPlaylistName))
        {
            return;
        }

        if (_selectedPlaylistName != playlist)
        {
            return;
        }

        var ret = await _mpc.MpdPlaylistClear(playlist);

        if (ret.IsSuccess)
        {
            PlaylistSongs.Clear();
        }
    }

    // double clicked in a playlist listview (currently NOT USED)
    [RelayCommand]
    public async Task PlaylistSongsListviewLeftDoubleClick(SongInfo song)
    {
        if (SelectedPlaylistSong is null) return;
        if (IsBusy) return;

        await _mpc.MpdAdd(song.File);
    }

    #endregion

    #region == Common Listview selected/collection songs command ==

    // used in context menu of Search result, Playlist etc.
    [RelayCommand]
    public async Task SongsListviewAddSelectedToQueue(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        switch (items.Count)
        {
            case > 1:
            {
                var collection = items.Cast<SongInfo>();

                List<string> uriList = [];

                foreach (var item in collection)
                {
                    uriList.Add((item as SongInfo).File);
                }

                await _mpc.MpdAdd(uriList);
                break;
            }
            case 1 when (items[0] is SongInfo si):
                await _mpc.MpdAdd(si.File);
                break;
        }
    }

    // Play (Used in Albums, Artists, Search pages etc.)
    [RelayCommand]
    public async Task SongsPlay(object obj)
    {
        if (obj is null) return;

        if (obj is not ObservableCollection<SongInfo> list) return;
        if (list.Count <= 0) return;
        Dispatcher.UIThread.Post(() => {
            Queue.Clear();
            CurrentSong = null;        
        });

        List<string> uriList = [];

        foreach (var song in list)
        {
            uriList.Add(song.File);
        }

        await _mpc.MpdMultiplePlay(uriList, Convert.ToInt32(_volume));

        // get album cover.
        await Task.Yield();
        await Task.Delay(200);
        UpdateCurrentSong();
    }

    // Add to queue (Used in Albums, Artists, Search pages etc.)
    [RelayCommand]
    public async Task SongsAddToQueue(object obj)
    {
        if (obj is null) return;

        if (obj is not ObservableCollection<SongInfo> list) return;
        switch (list.Count)
        {
            case > 1:
            {
                List<string> uriList = [];

                foreach (var song in list)
                {
                    uriList.Add(song.File);
                }
                //uriList.AddRange(list.Select(song => song.File));

                await _mpc.MpdAdd(uriList);
                break;
            }
            case 1 when (list[0] is SongInfo si):
                await _mpc.MpdAdd(si.File);
                break;
        }
    }

    [RelayCommand]
    public async Task SongsListviewPlayThis(object obj)
    {
        if (obj is null) return;

        if (obj is SongInfo song)
        {
            Queue.Clear();
            CurrentSong = null;

            await _mpc.MpdSinglePlay(song.File, Convert.ToInt32(_volume));
        }
    }

    [RelayCommand]
    public async Task SongsListviewAddThis(object obj)
    {
        if (obj is null) return;

        if (obj is SongInfo song)
        {
            await _mpc.MpdAdd(song.File);
        }
    }

    #endregion

    #region == Settings ==

    [RelayCommand]
    public async Task ShowProfileEditDialog()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var res = await _dialog.ShowProfileEditDialog(SelectedProfile);

        if (res is null)
        {
            return;
        }

        SelectedProfile = res;

        if (SelectedProfile.IsDefault)
        {
            foreach (var hoge in Profiles)
            {
                hoge.IsDefault = false;
            }

            SelectedProfile.IsDefault = true;
        }
        else
        {
            var fuga = Profiles.FirstOrDefault(x => x.IsDefault == true);
            if (fuga is null)
            {
                if (Profiles.Count > 0)
                {
                    Profiles[0].IsDefault = true;
                }
            }
        }
    }

    [RelayCommand]
    public async Task ShowProfileAddDialog()
    {
        var pro = await _dialog.ShowProfileAddDialog();

        if (pro is null)
        {
            return;
        }

        if (pro.IsDefault)
        {
            foreach(var hoge in Profiles)
            {
                hoge.IsDefault = false;
            }
        }
        else
        {
            var fuga = Profiles.FirstOrDefault(x => x.IsDefault == true);
            if (fuga is null)
            {
                pro.IsDefault = true;
            }
        }

        Profiles.Add(pro);
        OnPropertyChanged(nameof(Profiles));
        SelectedProfile = pro;

        if (Profiles.Count > 0)
        {
            IsConnectButtonEnabled = true;
        }
    }

    [RelayCommand]
    public void ShowProfileRemoveNoneDialog()
    {
        if (Profiles.Count <= 0)
        {
            return;
        }

        if (SelectedProfile is null)
        {
            return;
        }

        bool isDefault = SelectedProfile.IsDefault;

        if (Profiles.Remove(SelectedProfile))
        {
            SelectedProfile = null;

            if (Profiles.Count > 0)
            {
                if (isDefault)
                {
                    Profiles[0].IsDefault = true;
                }

                SelectedProfile = Profiles[0];
            }
        }

        OnPropertyChanged(nameof(Profiles));

        if (Profiles.Count == 0)
        {
            IsConnectButtonEnabled = false;
        }
    }

    [RelayCommand]
    public void GoToSettings()
    {
        //IsSettingsShow = false;
        GoToSettingsPage?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public async Task ClearAlbumCacheFolder()
    {
        DeleteAllContents(App.AppDataCacheFolder);

        // Update folder size.
        await GetCacheFolderSize();
    }

    public bool SaveProfileCanExecute()
    {
        //if (SelectedProfile is not null) return false;
        if (string.IsNullOrEmpty(Host)) return false;
        if (_port == 0) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(SaveProfileCanExecute))]
    public void SaveProfile(object obj)
    {
        if (obj is null) return;
        //if (SelectedProfile is not null) return;
        if (string.IsNullOrEmpty(Host)) return;
        if (_port == 0) return;
        /*
        Profile pro = new()
        {
            Host = _host,
            Port = _port
        };
        */
        /*
        // for Unbindable PasswordBox.
        var passwordBox = obj as PasswordBox;
        if (passwordBox is not null)
        {
            if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                Password = passwordBox.Password;
            }
        }
        */

        /*
        if (SetIsDefault)
        {
            foreach (var p in Profiles)
            {
                p.IsDefault = false;
            }
            pro.IsDefault = true;
        }
        else
        {
            pro.IsDefault = false;
        }

        pro.Name = Host + ":" + _port.ToString();

        Profiles.Add(pro);
        OnPropertyChanged(nameof(IsCurrentProfileSet));

        SelectedProfile = pro;

        SettingProfileEditMessage = MPDCtrlX.Properties.Resources.Settings_ProfileSaved;

        if (CurrentProfile is null)
        {
            SetIsDefault = true;
            pro.IsDefault = true;
            CurrentProfile = pro;
        }
        */
    }

    [RelayCommand]
    public void UpdateProfile(object obj)
    {
        if (obj is null) return;
        //if (SelectedProfile is null) return;
        if (string.IsNullOrEmpty(Host)) return;
        if (_port == 0) return;
        /*
        SelectedProfile.Host = _host;
        SelectedProfile.Port = _port;
        */
        /*
        // for Unbindable PasswordBox.
        var passwordBox = obj as PasswordBox;
        if (passwordBox is not null)
        {
            if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                SelectedProfile.Password = passwordBox.Password;
                Password = passwordBox.Password;

                if (SelectedProfile == CurrentProfile)
                {
                    // No need since _mpc uses password when it connects.
                    //_mpc.MpdPassword = passwordBox.Password;
                }
            }
        }
        */
        /*
        if (SetIsDefault)
        {
            foreach (var p in Profiles)
            {
                p.IsDefault = false;
            }
            SelectedProfile.IsDefault = true;
        }
        else
        {
            if (SelectedProfile.IsDefault)
            {
                SelectedProfile.IsDefault = false;
                Profiles[0].IsDefault = true;
            }
            else
            {
                SelectedProfile.IsDefault = false;
            }
        }

        SelectedProfile.Name = Host + ":" + _port.ToString();

        SettingProfileEditMessage = MPDCtrlX.Properties.Resources.Settings_ProfileUpdated;
        */
    }

    #endregion

    #region == Connection ==

    public bool ReConnectWithSelectedProfileCanExecute()
    {
        if (IsBusy) return false;
        if (IsConnecting) return false;
        if (SelectedProfile is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(ReConnectWithSelectedProfileCanExecute))]
    public void ReConnectWithSelectedProfile()
    {
        if (IsBusy) return;
        if (IsConnecting) return;
        if (SelectedProfile is null) return;

        IsBusy = true;
        IsWorking = true;

        // Disconnect if connected.
        if (IsConnected)
        {
            _mpc.MpdStop = true;
            _mpc.MpdDisconnect(true);
            _mpc.MpdStop = false;
        }

        // Save volume.
        SelectedProfile.Volume = Convert.ToInt32(Volume);
        // Set current.
        CurrentProfile = SelectedProfile;

        // Clearing values
        MpdVersion = string.Empty;
        CurrentSong = null;
        SelectedQueueSong = null;

        SelectedNodeMenu = null;

        Queue.Clear();
        _mpc.CurrentQueue.Clear();

        _mpc.MpdStatus.Reset();

        _mainMenuItems.PlaylistsDirectory?.Children.Clear();

        Playlists.Clear();
        _mpc.Playlists.Clear();

        SelectedPlaylistSong = null;

        if (_mainMenuItems.FilesDirectory is not null)
            _mainMenuItems.FilesDirectory.IsAcquired = false;

        MusicEntries.Clear();

        _musicDirectories.IsCanceled = true;
        if (_musicDirectories.Children.Count > 0)
            _musicDirectories.Children[0].Children.Clear();
        MusicDirectories.Clear();

        FilterMusicEntriesQuery = "";

        SearchResult?.Clear();
        SearchQuery = "";

        SelectedPlaylistName = string.Empty;
        SelectedPlaylistSong = null;
        SelectedAlbum = null;
        SelectedAlbumArtist = null;
        SelectedAlbumSongs = [];
        SelectedArtistAlbums = null;
        SelectedAlbumArtist = null;

        // TODO: more?

        //IsAlbumArtVisible = false;
        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

        _ = Task.Run(() => Start(_host, _port));
        /*
        ConnectionResult r = await _mpc.MpdIdleConnect(_host, _port);

        if (r.IsSuccess)
        {
            //CurrentProfile = prof;

            if (SelectedNodeMenu?.Children.Count > 0)
            {

                SelectedNodeMenu = MainMenuItems[0];
            }
        }
        */

        //IsSwitchingProfile = false;
        IsBusy = false;
        IsWorking = false;
    }


    public bool ChangeConnectionProfileCanExecute()
    {
        if (IsBusy) return false;
        if (string.IsNullOrWhiteSpace(Host)) return false;
        if (string.IsNullOrEmpty(Host)) return false;
        if (IsConnecting) return false;
        //if ((SelectedProfile is not null) && CurrentProfile is null) return false;
        return true;
    }
    [RelayCommand(CanExecute = nameof(ChangeConnectionProfileCanExecute))]
    public async Task ChangeConnectionProfile(object obj)
    {
        if (obj is null) return;
        if (string.IsNullOrEmpty(Host)) return;
        if (string.IsNullOrWhiteSpace(Host)) return;
        if (_port == 0) return;
        if (IsConnecting) return;
        if (IsBusy) return;
        if (IsWorking) return;

        //IsSwitchingProfile = true;

        if (IsConnected)
        {
            _mpc.MpdStop = true;
            _mpc.MpdDisconnect(true);
            _mpc.MpdStop = false;
        }

        // Save volume.
        if (CurrentProfile is not null)
            CurrentProfile.Volume = Convert.ToInt32(Volume);

        /*
        // Validate Host input.
        if (Host == "")
        {
            //SetError(nameof(Host), "Error: Host must be specified."); //TODO: translate
            OnPropertyChanged(nameof(Host));
            return;
        }
        else
        {
            if (Host == "localhost")
            {
                Host = "127.0.0.1";
            }

            IPAddress? ipAddress = null;
            try
            {
                ipAddress = IPAddress.Parse(Host);
                if (ipAddress is not null)
                {
                    //ClearError(nameof(Host));
                }
            }
            catch
            {
                //System.FormatException
                //SetError(nameof(Host), "Error: Invalid address format."); //TODO: translate

                return;
            }
        }
        */

        HostIpAddress = null;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(Host, AddressFamily.InterNetwork);
            if (addresses.Length > 0)
            {
                HostIpAddress = addresses[0];
            }
            else
            {
                //TODO: translate.
                //SetError(nameof(Host), "Error: Could not retrive IP Address from the hostname.");
                OnPropertyChanged(nameof(Host));
                // TODO::
                ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
                StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
                return;
            }
        }
        catch (Exception)
        {
            //TODO: translate.
            //SetError(nameof(Host), "Error: Could not retrive IP Address from the hostname. (SocketException)");
            OnPropertyChanged(nameof(Host));
            // TODO::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
            return;
        }

        if (_port == 0)
        {
            //TODO: translate.
            //SetError(nameof(Port), "Error: Port must be specified.");
            OnPropertyChanged(nameof(Host));
            return;
        }
        /*
        // for Unbindable PasswordBox.
        var passwordBox = obj as PasswordBox;
        if (passwordBox is not null)
        {
            if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                Password = passwordBox.Password;
            }
        }
        */
        // Clear current...
        if (CurrentSong is not null)
        {
            CurrentSong.IsPlaying = false;
            CurrentSong = null;
        }
        if (CurrentSong is not null)
        {
            SelectedQueueSong = null;
        }

        
        Dispatcher.UIThread.Post(() =>
        {
            SelectedNodeMenu = null;

            Queue.Clear();
            _mpc.CurrentQueue.Clear();

            _mpc.MpdStatus.Reset();

            _mainMenuItems.PlaylistsDirectory?.Children.Clear();

            Playlists.Clear();
            _mpc.Playlists.Clear();
            //SelectedPlaylist = null;

            SelectedPlaylistSong = null;

            if (_mainMenuItems.FilesDirectory is not null)
                _mainMenuItems.FilesDirectory.IsAcquired = false;

            MusicEntries.Clear();

            //MusicDirectories.Clear();// Don't
            //_mpc.LocalDirectories.Clear();// Don't
            //_mpc.LocalFiles.Clear();// Don't
            //SelectedNodeDirectory.Children.Clear();// Don't
            _musicDirectories.IsCanceled = true;
            if (_musicDirectories.Children.Count > 0)
                _musicDirectories.Children[0].Children.Clear();
            MusicDirectories.Clear();

            FilterMusicEntriesQuery = "";

            SelectedQueueSong = null;
            CurrentSong = null;

            SearchResult?.Clear();
            SearchQuery = "";

            //IsAlbumArtVisible = false;
            AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

            // TODO: more
        });

        IsConnecting = true;

        if (HostIpAddress is null) return;
        //ConnectionResult r = await _mpc.MpdIdleConnect(_host, _port);
        //ConnectionResult r = await _mpc.MpdIdleConnect(HostIpAddress.ToString(), _port);

        // Start MPD connection.
        ConnectionResult r = await _mpc.MpdIdleConnect(HostIpAddress.ToString(), _port);

        if (r.IsSuccess)
        {
            //IsSettingsShow = false;

            if (CurrentProfile is null)
            {
                // Create new profile
                Profile prof = new()
                {
                    Name = _host + ":" + _port.ToString(),
                    Host = _host,
                    //HostIpAddress = _hostIpAddress,
                    Port = _port,
                    Password = _password,
                    IsDefault = true
                };

                CurrentProfile = prof;
                //SelectedProfile = prof;

                Profiles.Add(prof);
                //OnPropertyChanged(nameof(IsCurrentProfileSet));
            }
            else
            {
                var prof = new Profile      
                {
                    Name = _host,
                    Host = _host,
                    Port = _port,
                    Password = _password,
                    IsDefault = true,
                    Volume = _volume
                };

                if (!string.IsNullOrEmpty(prof.Host.Trim()))
                {
                    CurrentProfile = prof;
                    Profiles.Add(prof);

                    //OnPropertyChanged(nameof(IsCurrentProfileSet));
                }
                else
                {
                    Debug.WriteLine("Host info is empty. @OnMpdIdleConnected");
                }
                /*
                //SelectedProfile = new Profile();
                if (SelectedProfile is not null)
                {
                    SelectedProfile.Host = _host;
                    //SelectedProfile.HostIpAddress = _hostIpAddress;
                    SelectedProfile.Port = _port;
                    SelectedProfile.Password = _password;

                    if (SetIsDefault)
                    {
                        foreach (var p in Profiles)
                        {
                            p.IsDefault = false;
                        }

                        SelectedProfile.IsDefault = true;
                    }
                    else
                    {
                        SelectedProfile.IsDefault = false;
                    }

                    SelectedProfile.Name = Host + ":" + _port.ToString();

                    CurrentProfile = SelectedProfile;
                }
                */
            }
        }

        //IsSwitchingProfile = false;
    }

    private async void ChangeConnection(Profile prof)
    {
        if (IsConnecting) return;
        if (IsBusy) return;
        if (IsWorking) return;

        IsBusy = true;
        //IsSwitchingProfile = true;

        if (IsConnected)
        {
            _mpc.MpdStop = true;
            _mpc.MpdDisconnect(true);
            _mpc.MpdStop = false;
        }

        // Save volume.
        if (CurrentProfile is not null)
            CurrentProfile.Volume = Volume;

        // Clear current...
        if (CurrentSong is not null)
        {
            CurrentSong.IsPlaying = false;
            CurrentSong = null;
        }
        if (CurrentSong is not null)
        {
            SelectedQueueSong = null;
        }

        
        Dispatcher.UIThread.Post(() =>
        {
            SelectedNodeMenu = null;

            SelectedQueueSong = null;
            CurrentSong = null;

            _mpc.MpdStatus.Reset();

            Queue.Clear();
            _mpc.CurrentQueue.Clear();

            _mainMenuItems.PlaylistsDirectory?.Children.Clear();

            Playlists.Clear();
            _mpc.Playlists.Clear();
            //SelectedPlaylist = null;

            SelectedPlaylistSong = null;

            if (_mainMenuItems.FilesDirectory is not null)
                _mainMenuItems.FilesDirectory.IsAcquired = false;

            MusicEntries.Clear();

            // TODO: not good when directory is being built.
            //MusicDirectories.Clear(); // Don't
            //_mpc.LocalDirectories.Clear();// Don't
            //_mpc.LocalFiles.Clear();// Don't
            //SelectedNodeDirectory.Children.Clear();// Don't
            _musicDirectories.IsCanceled = true;
            if (_musicDirectories.Children.Count > 0)
                _musicDirectories.Children[0].Children.Clear();
            //MusicDirectories.Clear();

            FilterMusicEntriesQuery = "";

            SearchResult?.Clear();
            SearchQuery = "";

            IsAlbumArtVisible = false;
            AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

            HostIpAddress = null;

            // TODO: more?
        });

        _volume = prof.Volume;
        OnPropertyChanged(nameof(Volume));


        Host = prof.Host;

        HostIpAddress = null;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(Host, AddressFamily.InterNetwork);
            if (addresses.Length > 0)
            {
                HostIpAddress = addresses[0];
            }
            else
            {
                //SetError(nameof(Host), "Error: Could not retrive IP Address from the hostname."); //TODO: translate.

                // TODO:::::::
                ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
                StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
                return;
            }
        }
        catch (Exception)
        {
            //SetError(nameof(Host), "Error: Could not retrive IP Address from the hostname. (SocketException)"); //TODO: translate.

            // TODO:::::::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";

            return;
        }

        _port = prof.Port;
        Password = prof.Password;

        IsConnecting = true;

        if (HostIpAddress is null) return;
        //ConnectionResult r = await _mpc.MpdIdleConnect(_host, _port);
        ConnectionResult r = await _mpc.MpdIdleConnect(HostIpAddress.ToString(), _port);

        if (r.IsSuccess)
        {
            CurrentProfile = prof;

            SelectedNodeMenu = MainMenuItems[0];
        }

        //IsSwitchingProfile = false;
        IsBusy = false;
    }

    [RelayCommand]
    public void TryConnect()
    {
        Debug.WriteLine("_host: "+ _host);
        _ = Task.Run(() => Start(_host, _port));
    }

    #endregion

    #region == QueueListview header colums Show/Hide ==

    [RelayCommand]
    public void QueueColumnHeaderPositionShowHide()
    {
        if (IsQueueColumnHeaderPositionVisible)
        {
            IsQueueColumnHeaderPositionVisible = false;
            QueueColumnHeaderPositionWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderPositionVisible = true;
            QueueColumnHeaderPositionWidth = 60;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderNowPlayingShowHide()
    {
        if (IsQueueColumnHeaderNowPlayingVisible)
        {
            IsQueueColumnHeaderNowPlayingVisible = false;
            QueueColumnHeaderNowPlayingWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderNowPlayingVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderTimeShowHide()
    {
        if (IsQueueColumnHeaderTimeVisible)
        {

            IsQueueColumnHeaderTimeVisible = false;
            QueueColumnHeaderTimeWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderTimeVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderArtistShowHide()
    {
        if (IsQueueColumnHeaderArtistVisible)
        {
            IsQueueColumnHeaderArtistVisible = false;
            QueueColumnHeaderArtistWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderArtistVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderAlbumShowHide()
    {
        if (IsQueueColumnHeaderAlbumVisible)
        {
            IsQueueColumnHeaderAlbumVisible = false;
            QueueColumnHeaderAlbumWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderAlbumVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderDiscShowHide()
    {
        if (IsQueueColumnHeaderDiscVisible)
        {
            IsQueueColumnHeaderDiscVisible = false;
            QueueColumnHeaderDiscWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderDiscVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderTrackShowHide()
    {
        if (IsQueueColumnHeaderTrackVisible)
        {
            IsQueueColumnHeaderTrackVisible = false;
            QueueColumnHeaderTrackWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderTrackVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderGenreShowHide()
    {
        if (IsQueueColumnHeaderGenreVisible)
        {
            IsQueueColumnHeaderGenreVisible = false;
            QueueColumnHeaderGenreWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderGenreVisible = true;
        }
    }

    [RelayCommand]
    public void QueueColumnHeaderLastModifiedShowHide()
    {
        if (IsQueueColumnHeaderLastModifiedVisible)
        {
            IsQueueColumnHeaderLastModifiedVisible = false;
            QueueColumnHeaderLastModifiedWidth = 0;
        }
        else
        {
            IsQueueColumnHeaderLastModifiedVisible = true;
        }
    }

    #endregion

    #region == PlaylistListview header colums Show/Hide ==

    [RelayCommand]
    public void PlaylistColumnHeaderPositionShowHide()
    {
        if (IsPlaylistColumnHeaderPositionVisible)
        {
            IsPlaylistColumnHeaderPositionVisible = false;
            PlaylistColumnHeaderPositionWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderPositionVisible = true;
            PlaylistColumnHeaderPositionWidth = 60;
        }
    }

    /*
    public IRelayCommand PlaylistColumnHeaderNowPlayingShowHideCommand { get; }
    public static bool PlaylistColumnHeaderNowPlayingShowHideCommand_CanExecute()
    {
        return true;
    }
    public void PlaylistColumnHeaderNowPlayingShowHideCommand_Execute()
    {
        if (IsPlaylistColumnHeaderNowPlayingVisible)
        {
            IsPlaylistColumnHeaderNowPlayingVisible = false;
            PlaylistColumnHeaderNowPlayingWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderNowPlayingVisible = true;
        }

        // Notify code behind to do some work around ...
        PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }
    */

    [RelayCommand]
    public void PlaylistColumnHeaderTimeShowHide()
    {
        if (IsPlaylistColumnHeaderTimeVisible)
        {

            IsPlaylistColumnHeaderTimeVisible = false;
            PlaylistColumnHeaderTimeWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderTimeVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderArtistShowHide()
    {
        if (IsPlaylistColumnHeaderArtistVisible)
        {
            IsPlaylistColumnHeaderArtistVisible = false;
            PlaylistColumnHeaderArtistWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderArtistVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderAlbumShowHide()
    {
        if (IsPlaylistColumnHeaderAlbumVisible)
        {
            IsPlaylistColumnHeaderAlbumVisible = false;
            PlaylistColumnHeaderAlbumWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderAlbumVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderDiscShowHide()
    {
        if (IsPlaylistColumnHeaderDiscVisible)
        {
            IsPlaylistColumnHeaderDiscVisible = false;
            PlaylistColumnHeaderDiscWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderDiscVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderTrackShowHide()
    {
        if (IsPlaylistColumnHeaderTrackVisible)
        {
            IsPlaylistColumnHeaderTrackVisible = false;
            PlaylistColumnHeaderTrackWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderTrackVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderGenreShowHide()
    {
        if (IsPlaylistColumnHeaderGenreVisible)
        {
            IsPlaylistColumnHeaderGenreVisible = false;
            PlaylistColumnHeaderGenreWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderGenreVisible = true;
        }
    }

    [RelayCommand]
    public void PlaylistColumnHeaderLastModifiedShowHide()
    {
        if (IsPlaylistColumnHeaderLastModifiedVisible)
        {
            IsPlaylistColumnHeaderLastModifiedVisible = false;
            PlaylistColumnHeaderLastModifiedWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderLastModifiedVisible = true;
        }
    }

    #endregion

    #region == SearchListview header colums Show/Hide ==

    [RelayCommand]
    public void SearchColumnHeaderPositionShowHide()
    {
        if (IsSearchColumnHeaderPositionVisible)
        {
            IsSearchColumnHeaderPositionVisible = false;
            SearchColumnHeaderPositionWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderPositionVisible = true;
            SearchColumnHeaderPositionWidth = 60;
        }
    }

    /*
    public IRelayCommand PlaylistColumnHeaderNowPlayingShowHideCommand { get; }
    public static bool PlaylistColumnHeaderNowPlayingShowHideCommand_CanExecute()
    {
        return true;
    }
    public void PlaylistColumnHeaderNowPlayingShowHideCommand_Execute()
    {
        if (IsPlaylistColumnHeaderNowPlayingVisible)
        {
            IsPlaylistColumnHeaderNowPlayingVisible = false;
            PlaylistColumnHeaderNowPlayingWidth = 0;
        }
        else
        {
            IsPlaylistColumnHeaderNowPlayingVisible = true;
        }

        // Notify code behind to do some work around ...
        PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }
    */

    [RelayCommand]
    public void SearchColumnHeaderTimeShowHide()
    {
        if (IsSearchColumnHeaderTimeVisible)
        {

            IsSearchColumnHeaderTimeVisible = false;
            SearchColumnHeaderTimeWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderTimeVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderArtistShowHide()
    {
        if (IsSearchColumnHeaderArtistVisible)
        {
            IsSearchColumnHeaderArtistVisible = false;
            SearchColumnHeaderArtistWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderArtistVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderAlbumShowHide()
    {
        if (IsSearchColumnHeaderAlbumVisible)
        {
            IsSearchColumnHeaderAlbumVisible = false;
            SearchColumnHeaderAlbumWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderAlbumVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderDiscShowHide()
    {
        if (IsSearchColumnHeaderDiscVisible)
        {
            IsSearchColumnHeaderDiscVisible = false;
            SearchColumnHeaderDiscWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderDiscVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderTrackShowHide()
    {
        if (IsSearchColumnHeaderTrackVisible)
        {
            IsSearchColumnHeaderTrackVisible = false;
            SearchColumnHeaderTrackWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderTrackVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderGenreShowHide()
    {
        if (IsSearchColumnHeaderGenreVisible)
        {
            IsSearchColumnHeaderGenreVisible = false;
            SearchColumnHeaderGenreWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderGenreVisible = true;
        }
    }

    [RelayCommand]
    public void SearchColumnHeaderLastModifiedShowHide()
    {
        if (IsSearchColumnHeaderLastModifiedVisible)
        {
            IsSearchColumnHeaderLastModifiedVisible = false;
            SearchColumnHeaderLastModifiedWidth = 0;
        }
        else
        {
            IsSearchColumnHeaderLastModifiedVisible = true;
        }
    }


    #endregion

    #region == DebugWindow and AckWindow ==

    [RelayCommand]
    public void ClearDebugCommandText()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugCommandClear?.Invoke();
        });
    }

    [RelayCommand]
    public void ClearDebugIdleText()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugIdleClear?.Invoke();
        });
    }

    [RelayCommand]
    public void ShowDebugWindow()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugWindowShowHide?.Invoke();
        });
    }

    [RelayCommand]
    public void ClearAckText()
    {
        
        Dispatcher.UIThread.Post(() => {
            AckWindowClear?.Invoke();
        });
    }

    [RelayCommand]
    public void ShowAckWindow()
    {
        if (IsShowAckWindow)
            IsShowAckWindow = false;
        else
            IsShowAckWindow = true;
    }

    #endregion

    #region == Find ==

    [RelayCommand]
    public void ShowFind()
    {
        if (SelectedNodeMenu is NodeMenuQueue)
        {
            QueueFindShowHide();
        }
        else if (SelectedNodeMenu is NodeMenuSearch)
        {

        }
        else
        {
            SelectedNodeMenu = _mainMenuItems.SearchDirectory;

            IsQueueFindVisible = false;
        }
    }


    #endregion

    #region == Other commands == 

    [RelayCommand]

    public void Escape()
    {
        //IsChangePasswordDialogShow = false;

        //IsSettingsShow = false; //Don't.

        IsQueueFindVisible = false; // not working?

        // Popups
        if (IsConfirmClearQueuePopupVisible) { IsConfirmClearQueuePopupVisible = false; }
        if (IsSelectedSaveToPopupVisible) { IsSelectedSaveToPopupVisible = false; }
        if (IsSelectedSaveAsPopupVisible) { IsSelectedSaveAsPopupVisible = false; }
        if (IsConfirmDeleteQueuePopupVisible) { IsConfirmDeleteQueuePopupVisible = false; }
        if (IsConfirmUpdatePlaylistSongsPopupVisible) { IsConfirmUpdatePlaylistSongsPopupVisible = false; }
        if (IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible) { IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible = false; }
        if (IsConfirmDeletePlaylistSongPopupVisible) { IsConfirmDeletePlaylistSongPopupVisible = false; }
        if (IsConfirmPlaylistClearPopupVisible) { IsConfirmPlaylistClearPopupVisible = false; }
        if (IsSearchResultSelectedSaveAsPopupVisible) { IsSearchResultSelectedSaveAsPopupVisible = false; }
        if (IsSearchResultSelectedSaveToPopupVisible) { IsSearchResultSelectedSaveToPopupVisible = false; }
        if (IsSongFilesSelectedSaveAsPopupVisible) { IsSongFilesSelectedSaveAsPopupVisible = false; }
        if (IsSongFilesSelectedSaveToPopupVisible) { IsSongFilesSelectedSaveToPopupVisible = false; }

        IsAlbumArtPanelIsOpen = false;
        IsAlbumContentPanelVisible = false;
    }

    [RelayCommand]
    public void AlbumsCoverOverlayPanelClose()
    {
        IsAlbumArtPanelIsOpen = false;
    }

    // Jump menu from CurrentSong
    [RelayCommand]
    public void JumpToAlbumPage()
    {
        if (CurrentSong is null)
        {
            return;
        }
        if (string.IsNullOrEmpty(CurrentSong.Album.Trim()))
        {
            return;
        }
        var items = Albums.Where(i => i.Name == CurrentSong.Album);
        if (items is null) return;
        var asdf = CurrentSong.AlbumArtist;
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            asdf = CurrentSong.Artist;
        }

        // no artist name
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            var hoge = items.FirstOrDefault(x => x.Name == CurrentSong.Album);
            // found it
            SelectedAlbum = hoge;
            if (SelectedAlbum is not null)
            {
                //GoToAlbumsPageAndScrollTo?.Invoke(this, Albums.IndexOf(SelectedAlbum));
                GoToAlbumPage();
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (item.AlbumArtist != asdf) continue;
                // found it
                SelectedAlbum = item;
                if (SelectedAlbum is not null)
                {
                    //GoToAlbumsPageAndScrollTo?.Invoke(this, Albums.IndexOf(SelectedAlbum));
                    GoToAlbumPage();
                }
                break;
            }
        }
    }

    private void GoToAlbumPage()
    {
        IsNavigationViewMenuOpen = true;
        _mainMenuItems.AlbumsDirectory.Selected = true;
        //GoToSelectedPage?.Invoke(this, _mainMenuItems.AlbumsDirectory);

        /*
        foreach (var hoge in MainMenuItems)
        {
            if (hoge is not NodeMenuLibrary) continue;
            foreach (var fuga in hoge.Children)
            {
                if (fuga is not NodeMenuAlbum) continue;
                IsNavigationViewMenuOpen = true;
                fuga.Selected = true;
                break;
            }
        }
        */
    }

    [RelayCommand]
    public void JumpToArtistPage()
    {
        if (CurrentSong is null)
        {
            return;
        }

        var asdf = CurrentSong.AlbumArtist;
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            asdf = CurrentSong.Artist;
        }

        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            return;
        }

        var item = Artists.FirstOrDefault(i => i.Name == asdf);
        if (item is null) return;
        SelectedAlbumArtist = item;
        GoToArtistPage();
    }

    private void GoToArtistPage()
    {
        IsNavigationViewMenuOpen = true;
        _mainMenuItems.ArtistsDirectory.Selected = true;
        //GoToSelectedPage?.Invoke(this, _mainMenuItems.ArtistsDirectory);
        /*
        foreach (var hoge in MainMenuItems)
        {
            if (hoge is not NodeMenuLibrary) continue;
            foreach (var fuga in hoge.Children)
            {
                if (fuga is not NodeMenuArtist) continue;
                IsNavigationViewMenuOpen = true;
                fuga.Selected = true;
                break;
            }
        }
        */
    }

    [RelayCommand]
    public void ListviewGoToAlbumPage(SongInfo song)
    {
        if (song is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(song.Album.Trim()))
        {
            return;
        }

        var items = Albums.Where(i => i.Name == song.Album);
        if (items is null) return;
        var asdf = song.AlbumArtist;
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            asdf = song.Artist;
        }

        // no artist name
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            var hoge = items.FirstOrDefault(x => x.Name == song.Album);
            // found it
            SelectedAlbum = hoge;
            if (SelectedAlbum is not null)
            {
                GoToAlbumPage();
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (item.AlbumArtist != asdf) continue;
                // found it
                SelectedAlbum = item;
                if (SelectedAlbum is not null)
                {
                    GoToAlbumPage();
                }
                break;
            }
        }
    }

    [RelayCommand]
    public void ListviewGoToArtistPage(SongInfo song)
    {
        if (song is null)
        {
            return;
        }

        var asdf = song.AlbumArtist;
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            asdf = song.Artist;
        }

        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            return;
        }

        var item = Artists.FirstOrDefault(i => i.Name == asdf);
        if (item is null) return;
        SelectedAlbumArtist = item;
        GoToArtistPage();
    }

    #endregion


    #endregion
}
