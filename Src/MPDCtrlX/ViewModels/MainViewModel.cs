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
using Microsoft.VisualBasic;
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
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace MPDCtrlX.ViewModels;

public class MainDummyViewModel
{
   public MainDummyViewModel()
    {
        // This is a dummy view model for the MainView.
        // It is used to avoid a build error in the xaml preview.
        // It is not used in the application.
    }
}

public partial class MainViewModel : ViewModelBase //ObservableObject
{
    private string _appVersion = string.Empty;
    public string AppVersion
    {
        get
        {
            if (!string.IsNullOrEmpty(_appVersion)) return _appVersion;
            
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = assembly.Version;
            _appVersion = $"{version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision}";

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
            this.NotifyPropertyChanged(nameof(IsFullyLoaded));
        }
    }

    private bool _isFullyRendered;
    public bool IsFullyRendered
    {
        get
        {
            return _isFullyRendered;
        }
        set
        {
            if (_isFullyRendered == value)
                return;

            _isFullyRendered = value;
            this.NotifyPropertyChanged(nameof(IsFullyRendered));
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

            NotifyPropertyChanged(nameof(MainLeftPainActualWidth));
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

            NotifyPropertyChanged(nameof(MainLeftPainWidth));
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
            NotifyPropertyChanged(nameof(IsNavigationViewMenuOpen));

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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderPositionVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderPositionWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderNowPlayingVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderNowPlayingWidth));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderTitleWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderTimeVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderTimeWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderArtistVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderArtistWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderAlbumVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderAlbumWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderDiscVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderDiscWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderTrackVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderTrackWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderGenreVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderGenreWidth));
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

            NotifyPropertyChanged(nameof(IsQueueColumnHeaderLastModifiedVisible));
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

            NotifyPropertyChanged(nameof(QueueColumnHeaderLastModifiedWidth));
        }
    }


    #endregion

    #region == Files column headers == 

    private double _libraryColumnHeaderTitleWidth = 260;
    public double LibraryColumnHeaderTitleWidth
    {
        get
        {
            return _libraryColumnHeaderTitleWidth;
        }
        set
        {
            if (value == _libraryColumnHeaderTitleWidth)
                return;

            if (value > 12)
                _libraryColumnHeaderTitleWidth = value;

            NotifyPropertyChanged(nameof(LibraryColumnHeaderTitleWidth));
        }
    }

    private double _libraryColumnHeaderFilePathWidth = 250;
    public double LibraryColumnHeaderFilePathWidth
    {
        get
        {
            return _libraryColumnHeaderFilePathWidth;
        }
        set
        {
            if (value == _libraryColumnHeaderFilePathWidth)
                return;

            if (value > 12)
                _libraryColumnHeaderFilePathWidth = value;

            NotifyPropertyChanged(nameof(LibraryColumnHeaderFilePathWidth));
        }
    }
    #endregion

    #region == PlaylistItem headers == 

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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderPositionWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderTitleWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderTimeWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderArtistWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderAlbumWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderDiscWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderTrackWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderGenreWidth));
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

            NotifyPropertyChanged(nameof(PlaylistColumnHeaderLastModifiedWidth));
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
            NotifyPropertyChanged(nameof(CurrentTheme));

            FluentAvaloniaTheme? _faTheme = ((Application.Current as App)!.Styles[0] as FluentAvaloniaTheme);
            if (_currentTheme.Id == 1)
            {
                _faTheme!.PreferSystemTheme = false;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else if (_currentTheme.Id == 2)
            {
                _faTheme!.PreferSystemTheme = false;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else
            {
                _faTheme!.PreferSystemTheme = true;
                (Application.Current as App)!.RequestedThemeVariant = ThemeVariant.Default;
            }
        }
    }

    #endregion

    #region == Status and Visibility switch flags ==  

    private bool _isConnected;
    public bool IsConnected
    {
        get
        {
            return _isConnected;
        }
        set
        {
            if (_isConnected == value)
                return;

            _isConnected = value;
            NotifyPropertyChanged(nameof(IsConnected));
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
            NotifyPropertyChanged(nameof(IsConnecting));
            NotifyPropertyChanged(nameof(IsNotConnecting));

            NotifyPropertyChanged(nameof(IsProfileSwitchOK));
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
            NotifyPropertyChanged(nameof(IsNotConnectingNorConnected));
        }
    }

    public bool IsNotConnecting
    {
        get
        {
            return !_isConnecting;
        }
    }

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
                if (CurrentProfile is null)
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
                if (CurrentProfile is null)
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

            NotifyPropertyChanged(nameof(IsSettingsShow));

        }
    }

    private bool _isConnectionSettingShow;
    public bool IsConnectionSettingShow
    {
        get { return _isConnectionSettingShow; }
        set
        {
            if (_isConnectionSettingShow == value)
                return;

            _isConnectionSettingShow = value;
            NotifyPropertyChanged(nameof(IsConnectionSettingShow));
        }
    }

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
            NotifyPropertyChanged(nameof(IsChangePasswordDialogShow));
        }
    }

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
            NotifyPropertyChanged(nameof(IsAlbumArtVisible));
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

            NotifyPropertyChanged(nameof(IsAlbumArtPanelIsOpen));
        }
    }

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
            NotifyPropertyChanged(nameof(IsAlbumArtsDownLoaded));
        }
    }

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
            NotifyPropertyChanged(nameof(IsBusy));

            NotifyPropertyChanged(nameof(IsProfileSwitchOK));

            
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
            NotifyPropertyChanged(nameof(IsWorking));

            NotifyPropertyChanged(nameof(IsProfileSwitchOK));

            
            //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
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

            NotifyPropertyChanged(nameof(IsShowAckWindow));
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

            NotifyPropertyChanged(nameof(IsShowErrWindow));
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

            NotifyPropertyChanged(nameof(IsShowDebugWindow));

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

            NotifyPropertyChanged(nameof(IsEnableDebugWindow));
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
            NotifyPropertyChanged(nameof(CurrentSong));
            NotifyPropertyChanged(nameof(CurrentSongTitle));
            NotifyPropertyChanged(nameof(CurrentSongArtist));
            NotifyPropertyChanged(nameof(CurrentSongAlbum));
            NotifyPropertyChanged(nameof(IsCurrentSongArtistNotNull));
            NotifyPropertyChanged(nameof(IsCurrentSongAlbumNotNull));

            //NotifyPropertyChanged(nameof(CurrentSongStringForWindowTitle));
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
                return String.Empty;
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
            NotifyPropertyChanged(nameof(IsCurrentSongNotNull));
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
            NotifyPropertyChanged(nameof(PlayButton));
        }
    }

    private double _volume = 20;
    public double Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            if (_volume != value)
            {
                _volume = value;
                NotifyPropertyChanged(nameof(Volume));

                if (_mpc is not null)
                {
                    if (Convert.ToDouble(_mpc.MpdStatus.MpdVolume) != _volume)
                    {
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
            }
        }
    }

    private System.Timers.Timer? _volumeDelayTimer = null;
    private void DoChangeVolume(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_mpc is not null)
        {
            if (Convert.ToDouble(_mpc.MpdStatus.MpdVolume) != _volume)
            {
                if (SetVolumeCommand.CanExecute(null))
                {
                    SetVolumeCommand.Execute(null);
                }
            }
        }
    }

    private bool _repeat;
    public bool Repeat
    {
        get { return _repeat; }
        set
        {
            _repeat = value;
            NotifyPropertyChanged(nameof(Repeat));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdRepeat != value)
                {
                    if (SetRpeatCommand.CanExecute(null))
                    {
                        SetRpeatCommand.Execute(null);
                    }
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
            NotifyPropertyChanged(nameof(Random));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdRandom != value)
                {
                    if (SetRandomCommand.CanExecute(null))
                    {
                        SetRandomCommand.Execute(null);
                    }
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
            NotifyPropertyChanged(nameof(Consume));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdConsume != value)
                {
                    if (SetConsumeCommand.CanExecute(null))
                    {
                        SetConsumeCommand.Execute(null);
                    }
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
            NotifyPropertyChanged(nameof(Single));

            if (_mpc is not null)
            {
                if (_mpc.MpdStatus.MpdSingle != value)
                {
                    if (SetSingleCommand.CanExecute(null))
                    {
                        SetSingleCommand.Execute(null);
                    }
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
            NotifyPropertyChanged(nameof(Time));
        }
    }

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
                NotifyPropertyChanged(nameof(Elapsed));

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

    private System.Timers.Timer? _elapsedDelayTimer = null;
    private void DoChangeElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_mpc is not null && (_elapsed < _time) && SetSeekCommand.CanExecute(null))
        {
            SetSeekCommand.Execute(null);
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

            NotifyPropertyChanged(nameof(AlbumCover));
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
            NotifyPropertyChanged(nameof(AlbumArtBitmapSource));
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
            NotifyPropertyChanged(nameof(MainMenuItems));
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
            else if (value is NodeMenuLibrary)
            {
                // Do nothing.
            }

            else if (value is NodeMenuArtist nma)
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
                if (!IsAlbumArtsDownLoaded)
                {
                    GetAlbumPictures(_albums);
                }
            }
            else if (value is NodeMenuAlbum nmb)
            {
                //IsWorking = true;
                CurrentPage = App.GetService<AlbumPage>();
                //IsWorking = false;

                // Get album pictures.
                if (!IsAlbumArtsDownLoaded)
                {
                    GetAlbumPictures(_albums);
                }
            }
            else if (value is NodeMenuFiles nml)
            {
                CurrentPage = App.GetService<FilesPage>();

                if (!nml.IsAcquired || (MusicDirectories.Count <= 1) && (MusicEntries.Count == 0))
                {
                    GetFiles(nml);
                }
            }
            else if (value is NodeMenuPlaylists)
            {
                // Do nothing
                //CurrentPage = App.GetService<PlaylistsPage>();
            }
            else if (value is NodeMenuPlaylistItem nmpli)
            {
                CurrentPage = App.GetService<PlaylistItemPage>();

                Dispatcher.UIThread.Post(() =>
                {
                    SelectedPlaylistSong = null;
                    PlaylistSongs = nmpli.PlaylistSongs;
                    SelectedPlaylistName = nmpli.Name;
                });
                
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
            NotifyPropertyChanged(nameof(SelectedNodeMenu));
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
            this.NotifyPropertyChanged(nameof(CurrentPage));
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
            NotifyPropertyChanged(nameof(SelectedPlaylistName));
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
            NotifyPropertyChanged(nameof(RenamedSelectPendingPlaylistName));
        }
    }

    /*
    private bool _isQueueVisible = true;
    public bool IsQueueVisible
    {
        get { return _isQueueVisible; }
        set
        {
            if (_isQueueVisible == value)
                return;

            _isQueueVisible = value;
            NotifyPropertyChanged(nameof(IsQueueVisible));
        }
    }

    private bool _isPlaylistsVisible = true;
    public bool IsPlaylistsVisible
    {
        get { return _isPlaylistsVisible; }
        set
        {
            if (_isPlaylistsVisible == value)
                return;

            _isPlaylistsVisible = value;
            NotifyPropertyChanged(nameof(IsPlaylistsVisible));
        }
    }

    private bool _isPlaylistItemVisible = true;
    public bool IsPlaylistItemVisible
    {
        get { return _isPlaylistItemVisible; }
        set
        {
            if (_isPlaylistItemVisible == value)
                return;

            _isPlaylistItemVisible = value;
            NotifyPropertyChanged(nameof(IsPlaylistItemVisible));
        }
    }

    private bool _isLibraryVisible = true;
    public bool IsLibraryVisible
    {
        get { return _isLibraryVisible; }
        set
        {
            if (_isLibraryVisible == value)
                return;

            _isLibraryVisible = value;
            NotifyPropertyChanged(nameof(IsLibraryVisible));
        }
    }

    private bool _isSearchVisible = true;
    public bool IsSearchVisible
    {
        get { return _isSearchVisible; }
        set
        {
            if (_isSearchVisible == value)
                return;

            _isSearchVisible = value;
            NotifyPropertyChanged(nameof(IsSearchVisible));
        }
    }

    private bool _isAlbumVisible = true;
    public bool IsAlbumVisible
    {
        get { return _isAlbumVisible; }
        set
        {
            if (_isAlbumVisible == value)
                return;

            _isAlbumVisible = value;
            NotifyPropertyChanged(nameof(IsAlbumVisible));
        }
    }


    private bool _isArtistVisible = true;
    public bool IsArtistVisible
    {
        get { return _isArtistVisible; }
        set
        {
            if (_isArtistVisible == value)
                return;

            _isArtistVisible = value;
            NotifyPropertyChanged(nameof(IsArtistVisible));
        }
    }
    */

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
            NotifyPropertyChanged(nameof(Queue));
            NotifyPropertyChanged(nameof(QueuePageSubTitleSongCount));
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
            NotifyPropertyChanged(nameof(SelectedQueueSong));
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

            NotifyPropertyChanged(nameof(IsQueueFindVisible));
        }
    }

    private bool FilterSongInfoEx(SongInfoEx song)
    {
        return song.Title.Contains(FilterQueueQuery, StringComparison.InvariantCultureIgnoreCase);
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
            NotifyPropertyChanged(nameof(QueueForFilter));
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
            NotifyPropertyChanged(nameof(SelectedQueueFilterTags));

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
            NotifyPropertyChanged(nameof(FilterQueueQuery));

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
            NotifyPropertyChanged(nameof(SelectedQueueFilterSong));
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
            NotifyPropertyChanged(nameof(MusicDirectories));
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
            NotifyPropertyChanged(nameof(SelectedNodeDirectory));

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

            NotifyPropertyChanged(nameof(MusicEntriesFiltered));
            
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
            NotifyPropertyChanged(nameof(MusicEntries));
            NotifyPropertyChanged(nameof(FilesPageSubTitleFileCount));
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
            NotifyPropertyChanged(nameof(MusicEntriesFiltered));
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
            NotifyPropertyChanged(nameof(FilterMusicEntriesQuery));

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
                NotifyPropertyChanged(nameof(MusicEntriesFiltered));
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
            NotifyPropertyChanged(nameof(Artists));
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

                NotifyPropertyChanged(nameof(SelectedAlbumArtist));

                SelectedArtistAlbums = _selectedAlbumArtist?.Albums;
               
                GetArtistSongs(_selectedAlbumArtist);
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
            NotifyPropertyChanged(nameof(SelectedArtistAlbums));
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
            NotifyPropertyChanged(nameof(Albums));
            NotifyPropertyChanged(nameof(AlbumPageSubTitleAlbumCount));
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
            NotifyPropertyChanged(nameof(IsAlbumContentPanelVisible));
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
            NotifyPropertyChanged(nameof(SelectedAlbum));
            NotifyPropertyChanged(nameof(SelectedAlbumSongs));

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
            NotifyPropertyChanged(nameof(SelectedAlbumSongs));
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

    /*
    private AlbumEx? _invokedAlbum = new();
    public AlbumEx? InvokedAlbum
    {
        get { return _invokedAlbum; }
        set
        {
            if (_invokedAlbum == value)
                return;

            _invokedAlbum = value;
            NotifyPropertyChanged(nameof(InvokedAlbum));
            NotifyPropertyChanged(nameof(InvokedAlbumSongs));
        }
    }

    private ObservableCollection<SongInfo> _invokedAlbumSongs = [];
    public ObservableCollection<SongInfo> InvokedAlbumSongs
    {
        get 
        {
            if (_invokedAlbum is not null)
            {
                return _invokedAlbum.Songs;
            }

            return _invokedAlbumSongs; 
        }
        set
        {
            if (_invokedAlbumSongs == value)
                return;

            _invokedAlbumSongs = value;
            NotifyPropertyChanged(nameof(InvokedAlbumSongs));
        }
    }
    */

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
            NotifyPropertyChanged(nameof(SearchResult));
            NotifyPropertyChanged(nameof(SearchPageSubTitleResultCount));
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
            NotifyPropertyChanged(nameof(SelectedSearchTag));
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
            NotifyPropertyChanged(nameof(SelectedSearchShiki));
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
            NotifyPropertyChanged(nameof(SearchQuery));
            SearchExecCommand.NotifyCanExecuteChanged();
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
            NotifyPropertyChanged(nameof(Playlists));
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
                NotifyPropertyChanged(nameof(SelectedPlaylist));
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
                NotifyPropertyChanged(nameof(PlaylistSongs));
                NotifyPropertyChanged(nameof(PlaylistPageSubTitleSongCount));
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
                NotifyPropertyChanged(nameof(SelectedPlaylistSong));
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
            NotifyPropertyChanged(nameof(DebugCommandText));
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
            NotifyPropertyChanged(nameof(DebugIdleText));
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

            NotifyPropertyChanged(nameof(IsUpdateOnStartup));
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

            NotifyPropertyChanged(nameof(IsAutoScrollToNowPlaying));
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

            NotifyPropertyChanged(nameof(IsSaveLog));
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

            NotifyPropertyChanged(nameof(IsDownloadAlbumArt));
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

            NotifyPropertyChanged(nameof(IsDownloadAlbumArtEmbeddedUsingReadPicture));
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
            NotifyPropertyChanged(nameof(CurrentProfile));

            //SelectedProfile = _currentProfile;

            if (_currentProfile is not null)
            {
                _volume = _currentProfile.Volume;
                NotifyPropertyChanged(nameof(Volume));

                Host = _currentProfile.Host;
                Port = _currentProfile.Port.ToString();
                _password = _currentProfile.Password;

            }
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

            NotifyPropertyChanged(nameof(SelectedProfile));

            // "quietly"
            if (_selectedProfile is not null)
            {
                _selectedQuickProfile = _selectedProfile;
                NotifyPropertyChanged(nameof(SelectedQuickProfile));
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

            NotifyPropertyChanged(nameof(SelectedQuickProfile));
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

            NotifyPropertyChanged(nameof(Host));
        }
    }

    private IPAddress? _hostIpAddress;
    public IPAddress? HostIpAddress
    {
        get { return _hostIpAddress; }
        set
        {
            if (_hostIpAddress == value)
                return;

            _hostIpAddress = value;

            NotifyPropertyChanged(nameof(HostIpAddress));
        }
    }

    private int _port = 6600;
    public string Port
    {
        get { return _port.ToString(); }
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

            NotifyPropertyChanged(nameof(Port));
        }
    }

    private string _password = "";
    public string Password
    {
        get
        {
            return DummyPassword(_password);
        }
        set
        {
            // Don't. if (_password == value) ...

            _password = value;

            //NotifyPropertyChanged(nameof(IsNotPasswordSet));
            //NotifyPropertyChanged(nameof(IsPasswordSet));
            NotifyPropertyChanged(nameof(Password));
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
            string EncryptionKey = "withas";
            byte[] sBytes = Encoding.Unicode.GetBytes(s);
            using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(EncryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
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
            string EncryptionKey = "withas";
            s = s.Replace(" ", "+");
            byte[] sBytes = Convert.FromBase64String(s);
            using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new(EncryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
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
            NotifyPropertyChanged(nameof(SettingProfileEditMessage));
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

    private bool _setIsDefault = true;
    public bool SetIsDefault
    {
        get { return _setIsDefault; }
        set
        {
            if (_setIsDefault == value)
                return;

            _setIsDefault = value;

            NotifyPropertyChanged(nameof(SetIsDefault));
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
            NotifyPropertyChanged(nameof(IsSwitchingProfile));
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
            NotifyPropertyChanged(nameof(ChangePasswordDialogMessage));
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
            NotifyPropertyChanged(nameof(IsRememberAsProfile));
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
            NotifyPropertyChanged(nameof(StatusBarMessage));
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
            NotifyPropertyChanged(nameof(ConnectionStatusMessage));
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
            NotifyPropertyChanged(nameof(MpdStatusMessage));

            if (_mpdStatusMessage != "")
                _isMpdStatusMessageContainsText = true;
            else
                _isMpdStatusMessageContainsText = false;
            NotifyPropertyChanged(nameof(IsMpdStatusMessageContainsText));
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
            NotifyPropertyChanged(nameof(InfoBarAckTitle));
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
            NotifyPropertyChanged(nameof(InfoBarAckMessage));
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
            NotifyPropertyChanged(nameof(InfoBarErrTitle));
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
            NotifyPropertyChanged(nameof(InfoBarErrMessage));
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
            NotifyPropertyChanged(nameof(StatusButton));
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
            NotifyPropertyChanged(nameof(MpdStatusButton));
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
            NotifyPropertyChanged(nameof(IsUpdatingMpdDb));
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
            NotifyPropertyChanged(nameof(MpdVersion));
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
            NotifyPropertyChanged(nameof(IsConfirmClearQueuePopupVisible));
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
            NotifyPropertyChanged(nameof(IsSelectedSaveToPopupVisible));
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
            NotifyPropertyChanged(nameof(IsSelectedSaveAsPopupVisible));
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
            NotifyPropertyChanged(nameof(IsConfirmDeleteQueuePopupVisible));
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
            NotifyPropertyChanged(nameof(IsConfirmUpdatePlaylistSongsPopupVisible));
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
            NotifyPropertyChanged(nameof(IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible));
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
            NotifyPropertyChanged(nameof(IsConfirmDeletePlaylistSongPopupVisible));
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
            NotifyPropertyChanged(nameof(IsConfirmPlaylistClearPopupVisible));
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
            NotifyPropertyChanged(nameof(IsSearchResultSelectedSaveAsPopupVisible));
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
            NotifyPropertyChanged(nameof(IsSearchResultSelectedSaveToPopupVisible));
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
            NotifyPropertyChanged(nameof(IsSongFilesSelectedSaveAsPopupVisible));
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
            NotifyPropertyChanged(nameof(IsSongFilesSelectedSaveToPopupVisible));
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
    public event EventHandler<string>? AckWindowOutput;
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
    public event EventHandler? QueueSaveToDialogShow;
    public event EventHandler<List<string>>? QueueListviewSaveToDialogShow;
    
    public event EventHandler? QueueHeaderVisibilityChanged;
    public event EventHandler? QueueFindWindowVisibilityChanged_SetFocus;

    public event EventHandler<List<string>>? SearchPageAddToPlaylistDialogShow;
    public event EventHandler<List<string>>? FilesPageAddToPlaylistDialogShow;

    public event EventHandler<string>? PlaylistRenameToDialogShow;


    //public delegate void NavigationViewMenuItemsLoadedEventHandler();
    //public event NavigationViewMenuItemsLoadedEventHandler? NavigationViewMenuItemsLoaded;
    //

    //public event EventHandler? NavigationViewMenuItemsLoaded;

    #endregion

    #region == Services == 

    private readonly IMpcService _mpc;

    #endregion

    private readonly InitWindow _initWin;

    public MainViewModel(IMpcService mpcService, InitWindow initWin)
    {
        // MPD Service dependency injection.
        _mpc = mpcService;
        _initWin = initWin;

        #region == Init commands ==

        PlayCommand = new RelayCommand(PlayCommand_ExecuteAsync, PlayCommand_CanExecute);
        PlayStopCommand = new RelayCommand(PlayStopCommand_Execute, PlayStopCommand_CanExecute);
        PlayPauseCommand = new RelayCommand(PlayPauseCommand_Execute, PlayPauseCommand_CanExecute);
        PlayNextCommand = new RelayCommand(PlayNextCommand_ExecuteAsync, PlayNextCommand_CanExecute);
        PlayPrevCommand = new RelayCommand(PlayPrevCommand_ExecuteAsync, PlayPrevCommand_CanExecute);
        ChangeSongCommand = new RelayCommand(ChangeSongCommand_ExecuteAsync, ChangeSongCommand_CanExecute);

        SetRpeatCommand = new RelayCommand(SetRpeatCommand_ExecuteAsync, SetRpeatCommand_CanExecute);
        SetRandomCommand = new RelayCommand(SetRandomCommand_ExecuteAsync, SetRandomCommand_CanExecute);
        SetConsumeCommand = new RelayCommand(SetConsumeCommand_ExecuteAsync, SetConsumeCommand_CanExecute);
        SetSingleCommand = new RelayCommand(SetSingleCommand_ExecuteAsync, SetSingleCommand_CanExecute);

        SetVolumeCommand = new RelayCommand(SetVolumeCommand_ExecuteAsync, SetVolumeCommand_CanExecute);
        VolumeMuteCommand = new RelayCommand(VolumeMuteCommand_Execute, VolumeMuteCommand_CanExecute);
        VolumeUpCommand = new RelayCommand(VolumeUpCommand_Execute, VolumeUpCommand_CanExecute);
        VolumeDownCommand = new RelayCommand(VolumeDownCommand_Execute, VolumeDownCommand_CanExecute);

        SetSeekCommand = new RelayCommand(SetSeekCommand_ExecuteAsync, SetSeekCommand_CanExecute);
        /*
        PlaylistListviewEnterKeyCommand = new RelayCommand(PlaylistListviewEnterKeyCommand_ExecuteAsync, PlaylistListviewEnterKeyCommand_CanExecute);
        PlaylistListviewLoadPlaylistCommand = new RelayCommand(PlaylistListviewLoadPlaylistCommand_ExecuteAsync, PlaylistListviewLoadPlaylistCommand_CanExecute);
        PlaylistListviewClearLoadPlaylistCommand = new RelayCommand(PlaylistListviewClearLoadPlaylistCommand_ExecuteAsync, PlaylistListviewClearLoadPlaylistCommand_CanExecute);
        PlaylistListviewLeftDoubleClickCommand = new GenericRelayCommand<Playlist>(param => PlaylistListviewLeftDoubleClickCommand_ExecuteAsync(param), param => PlaylistListviewLeftDoubleClickCommand_CanExecute());
        ChangePlaylistCommand = new RelayCommand(ChangePlaylistCommand_ExecuteAsync, ChangePlaylistCommand_CanExecute);
        */
        PlaylistRenamePlaylistCommand = new GenericRelayCommand<string>(param => PlaylistRenamePlaylistCommand_Execute(param), param => PlaylistRenamePlaylistCommand_CanExecute());


        PlaylistRemovePlaylistWithoutPromptCommand = new GenericRelayCommand<string>(param => PlaylistRemovePlaylistWithoutPromptCommand_Execute(param), param => PlaylistRemovePlaylistWithoutPromptCommand_CanExecute());
        //PlaylistListviewRemovePlaylistCommand = new GenericRelayCommand<Playlist>(param => PlaylistListviewRemovePlaylistCommand_Execute(param), param => PlaylistListviewRemovePlaylistCommand_CanExecute());
        //PlaylistListviewConfirmRemovePlaylistPopupCommand = new RelayCommand(PlaylistListviewConfirmRemovePlaylistPopupCommand_Execute, PlaylistListviewConfirmRemovePlaylistPopupCommand_CanExecute);
        PlaylistClearPlaylistWithoutPromptCommand = new GenericRelayCommand<string>(param => PlaylistClearPlaylistWithoutPromptCommand_Execute(param), param => PlaylistClearPlaylistWithoutPromptCommand_CanExecute());
        //PlaylistListviewClearCommand = new RelayCommand(PlaylistListviewClearCommand_Execute, PlaylistListviewClearCommand_CanExecute);
        //PlaylistListviewClearPopupCommand = new RelayCommand(PlaylistListviewClearPopupCommand_Execute, PlaylistListviewClearPopupCommand_CanExecute);

        //
        PlaylistSongsListviewLeftDoubleClickCommand = new GenericRelayCommand<SongInfo>(param => PlaylistSongsListviewLeftDoubleClickCommand_ExecuteAsync(param), param => PlaylistSongsListviewLeftDoubleClickCommand_CanExecute());
        //SongFilesListviewLeftDoubleClickCommand = new GenericRelayCommand<NodeFile>(param => SongFilesListviewLeftDoubleClickCommand_ExecuteAsync(param), param => SongFilesListviewLeftDoubleClickCommand_CanExecute());

        QueueClearWithoutPromptCommand = new RelayCommand(QueueClearWithoutPromptCommand_ExecuteAsync, QueueClearWithoutPromptCommand_CanExecute);
        //QueueSaveAsCommand = new RelayCommand(QueueSaveAsCommand_ExecuteAsync, QueueSaveAsCommand_CanExecute);
        QueueSaveToCommand = new RelayCommand(QueueSaveToCommand_ExecuteAsync, QueueSaveToCommand_CanExecute);

        QueueListviewMoveUpCommand = new GenericRelayCommand<object>(param => QueueListviewMoveUpCommand_Execute(param), param => QueueListviewMoveUpCommand_CanExecute());
        QueueListviewMoveDownCommand = new GenericRelayCommand<object>(param => QueueListviewMoveDownCommand_Execute(param), param => QueueListviewMoveDownCommand_CanExecute());

        QueueListviewSaveSelectedToCommand = new GenericRelayCommand<object>(param => QueueListviewSaveSelectedToCommand_Execute(param), param => QueueListviewSaveSelectedToCommand_CanExecute());

        QueueListviewEnterKeyCommand = new RelayCommand(QueueListviewEnterKeyCommand_ExecuteAsync, QueueListviewEnterKeyCommand_CanExecute);
        QueueListviewLeftDoubleClickCommand = new GenericRelayCommand<SongInfoEx>(param => QueueListviewLeftDoubleClickCommand_ExecuteAsync(param), param => QueueListviewLeftDoubleClickCommand_CanExecute());

        QueueListviewDeleteSelectedWithoutPromptCommand = new GenericRelayCommand<object>(param => QueueListviewDeleteSelectedWithoutPromptCommand_Execute(param), param => QueueListviewDeleteSelectedWithoutPromptCommand_CanExecute());
        //QueueListviewDeleteCommand = new GenericRelayCommand<object>(param => QueueListviewDeleteCommand_Execute(param), param => QueueListviewDeleteCommand_CanExecute());
        //QueueListviewConfirmDeletePopupCommand = new RelayCommand(QueueListviewConfirmDeletePopupCommand_Execute, QueueListviewConfirmDeletePopupCommand_CanExecute);

        ShowSettingsCommand = new RelayCommand(ShowSettingsCommand_Execute, ShowSettingsCommand_CanExecute);
        SettingsOKCommand = new RelayCommand(SettingsOKCommand_Execute, SettingsOKCommand_CanExecute);

        SaveProfileCommand = new GenericRelayCommand<object>(param => SaveProfileCommand_Execute(param), param => SaveProfileCommand_CanExecute());
        UpdateProfileCommand = new GenericRelayCommand<object>(param => UpdateProfileCommand_Execute(param), param => UpdateProfileCommand_CanExecute());
        ChangeConnectionProfileCommand = new GenericRelayCommand<object>(param => ChangeConnectionProfileCommand_Execute(param), param => ChangeConnectionProfileCommand_CanExecute());

        ShowChangePasswordDialogCommand = new GenericRelayCommand<object>(param => ShowChangePasswordDialogCommand_Execute(param), param => ShowChangePasswordDialogCommand_CanExecute());
        ChangePasswordDialogOKCommand = new GenericRelayCommand<object>(param => ChangePasswordDialogOKCommand_Execute(param), param => ChangePasswordDialogOKCommand_CanExecute());
        ChangePasswordDialogCancelCommand = new RelayCommand(ChangePasswordDialogCancelCommand_Execute, ChangePasswordDialogCancelCommand_CanExecute);

        EscapeCommand = new RelayCommand(EscapeCommand_ExecuteAsync, EscapeCommand_CanExecute);

        QueueColumnHeaderPositionShowHideCommand = new RelayCommand(QueueColumnHeaderPositionShowHideCommand_Execute, QueueColumnHeaderPositionShowHideCommand_CanExecute);
        QueueColumnHeaderNowPlayingShowHideCommand = new RelayCommand(QueueColumnHeaderNowPlayingShowHideCommand_Execute, QueueColumnHeaderNowPlayingShowHideCommand_CanExecute);
        QueueColumnHeaderTimeShowHideCommand = new RelayCommand(QueueColumnHeaderTimeShowHideCommand_Execute, QueueColumnHeaderTimeShowHideCommand_CanExecute);
        QueueColumnHeaderArtistShowHideCommand = new RelayCommand(QueueColumnHeaderArtistShowHideCommand_Execute, QueueColumnHeaderArtistShowHideCommand_CanExecute);
        QueueColumnHeaderAlbumShowHideCommand = new RelayCommand(QueueColumnHeaderAlbumShowHideCommand_Execute, QueueColumnHeaderAlbumShowHideCommand_CanExecute);
        QueueColumnHeaderDiscShowHideCommand = new RelayCommand(QueueColumnHeaderDiscShowHideCommand_Execute, QueueColumnHeaderDiscShowHideCommand_CanExecute);
        QueueColumnHeaderTrackShowHideCommand = new RelayCommand(QueueColumnHeaderTrackShowHideCommand_Execute, QueueColumnHeaderTrackShowHideCommand_CanExecute);
        QueueColumnHeaderGenreShowHideCommand = new RelayCommand(QueueColumnHeaderGenreShowHideCommand_Execute, QueueColumnHeaderGenreShowHideCommand_CanExecute);
        QueueColumnHeaderLastModifiedShowHideCommand = new RelayCommand(QueueColumnHeaderLastModifiedShowHideCommand_Execute, QueueColumnHeaderLastModifiedShowHideCommand_CanExecute);

        ShowFindCommand = new RelayCommand(ShowFindCommand_Execute, ShowFindCommand_CanExecute);

        QueueFindShowHideCommand = new RelayCommand(QueueFindShowHideCommand_Execute, QueueFindShowHideCommand_CanExecute);

        SearchExecCommand = new RelayCommand(SearchExecCommand_Execute, SearchExecCommand_CanExecute);
        //FilterMusicEntriesClearCommand = new RelayCommand(FilterMusicEntriesClearCommand_Execute, FilterMusicEntriesClearCommand_CanExecute);

        FilterQueueClearCommand = new RelayCommand(FilterQueueClearCommand_Execute, FilterQueueClearCommand_CanExecute);

        //SearchListviewSaveSelectedAsCommand = new GenericRelayCommand<object>(param => SearchListviewSaveSelectedAsCommand_Execute(param), param => SearchListviewSaveSelectedAsCommand_CanExecute());
        //SearchResultListviewSaveSelectedAsPopupCommand = new GenericRelayCommand<string>(param => SearchResultListviewSaveSelectedAsPopupCommand_Execute(param), param => SearchResultListviewSaveSelectedAsPopupCommand_CanExecute());
        SearchListviewSaveSelectedToCommand = new GenericRelayCommand<object>(param => SearchListviewSaveSelectedToCommand_Execute(param), param => SearchListviewSaveSelectedToCommand_CanExecute());
        //SearchResultListviewSaveSelectedToPopupCommand = new GenericRelayCommand<Playlist>(param => SearchResultListviewSaveSelectedToPopupCommand_Execute(param), param => SearchResultListviewSaveSelectedToPopupCommand_CanExecute());


        SongsListviewAddSelectedToQueueCommand = new GenericRelayCommand<object>(param => SongsListviewAddSelectedToQueueCommand_Execute(param), param => SongsListviewAddSelectedToQueueCommand_CanExecute());
        FilesListviewAddSelectedToQueueCommand = new GenericRelayCommand<object>(param => FilesListviewAddSelectedToQueueCommand_Execute(param), param => FilesListviewAddSelectedToQueueCommand_CanExecute());

        //SongFilesListviewSaveSelectedAsCommand = new GenericRelayCommand<object>(param => SongFilesListviewSaveSelectedAsCommand_Execute(param), param => SongFilesListviewSaveSelectedAsCommand_CanExecute());
        //SongFilesListviewSaveSelectedAsPopupCommand = new GenericRelayCommand<string>(param => SongFilesListviewSaveSelectedAsPopupCommand_Execute(param), param => SongFilesListviewSaveSelectedAsPopupCommand_CanExecute());
        //SongFilesListviewSaveSelectedToCommand = new GenericRelayCommand<object>(param => SongFilesListviewSaveSelectedToCommand_Execute(param), param => SongFilesListviewSaveSelectedToCommand_CanExecute());
        //SongFilesListviewSaveSelectedToPopupCommand = new GenericRelayCommand<Playlist>(param => SongFilesListviewSaveSelectedToPopupCommand_Execute(param), param => SongFilesListviewSaveSelectedToPopupCommand_CanExecute());


        ScrollIntoNowPlayingCommand = new RelayCommand(ScrollIntoNowPlayingCommand_Execute, ScrollIntoNowPlayingCommand_CanExecute);

        ClearDebugCommandTextCommand = new RelayCommand(ClearDebugCommandTextCommand_Execute, ClearDebugCommandTextCommand_CanExecute);
        ClearDebugIdleTextCommand = new RelayCommand(ClearDebugIdleTextCommand_Execute, ClearDebugIdleTextCommand_CanExecute);
        ShowDebugWindowCommand = new RelayCommand(ShowDebugWindowCommand_Execute, ShowDebugWindowCommand_CanExecute);

        ClearAckTextCommand = new RelayCommand(ClearAckTextCommand_Execute, ClearAckTextCommand_CanExecute);
        ShowAckWindowCommand = new RelayCommand(ShowAckWindowCommand_Execute, ShowAckWindowCommand_CanExecute);

        PlaylistListviewDeleteSelectedWithoutPromptCommand = new GenericRelayCommand<object>(param => PlaylistListviewDeleteSelectedWithoutPromptCommand_Execute(param), param => PlaylistListviewDeleteSelectedWithoutPromptCommand_CanExecute());
        //PlaylistListviewDeletePosCommand = new GenericRelayCommand<object>(param => PlaylistListviewDeletePosCommand_Execute(param), param => PlaylistListviewDeletePosCommand_CanExecute());
        //PlaylistListviewDeletePosPopupCommand = new RelayCommand(PlaylistListviewDeletePosPopupCommand_Execute, PlaylistListviewDeletePosPopupCommand_CanExecute);
        //PlaylistListviewConfirmDeletePosNotSupportedPopupCommand = new RelayCommand(PlaylistListviewConfirmDeletePosNotSupportedPopupCommand_Execute, PlaylistListviewConfirmDeletePosNotSupportedPopupCommand_CanExecute);

        //QueueFilterSelectCommand = new GenericRelayCommand<object>(param => QueueFilterSelectCommand_Execute(param), param => QueueFilterSelectCommand_CanExecute());
        QueueFilterSelectCommand = new RelayCommand(QueueFilterSelectCommand_Execute, QueueFilterSelectCommand_CanExecute);

        PlaylistLoadPlaylistCommand = new RelayCommand(PlaylistLoadPlaylistCommand_Execute, PlaylistLoadPlaylistCommand_CanExecute);
        PlaylistClearLoadPlaylistCommand = new RelayCommand(PlaylistClearLoadPlaylistCommand_Execute, PlaylistClearLoadPlaylistCommand_CanExecute);

        //GetArtistsCommand = new RelayCommand(GetArtistsCommand_ExecuteAsync, GetArtistsCommand_CanExecute);
        
        AlbumsItemInvokedCommand = new GenericRelayCommand<object>(param => AlbumsItemInvokedCommand_Execute(param), param => AlbumsItemInvokedCommand_CanExecute());
        AlbumsCloseAlbumContentPanelCommand = new RelayCommand(AlbumsCloseAlbumContentPanelCommand_Execute, AlbumsCloseAlbumContentPanelCommand_CanExecute);

        AlbumsCoverOverlayPanelCloseCommand = new RelayCommand(AlbumsCoverOverlayPanelCloseCommand_Execute, AlbumsCoverOverlayPanelCloseCommand_CanExecute);

        JumpToAlbumPageCommand = new RelayCommand(JumpToAlbumPageCommand_Execute, JumpToAlbumPageCommand_CanExecute);
        JumpToArtistPageCommand = new RelayCommand(JumpToArtistPageCommand_Execute, JumpToArtistPageCommand_CanExecute);

        QueueListviewSortReverseCommand = new RelayCommand(QueueListviewSortReverseCommand_Execute, QueueListviewSortReverseCommand_CanExecute);

        QueueListviewSortByCommand = new GenericRelayCommand<string>(param => QueueListviewSortByCommand_Execute(param), param => QueueListviewSortByCommand_CanExecute());

        SongsPlayCommand = new GenericRelayCommand<object>(param => SongsPlayCommand_Execute(param), param => SongsPlayCommand_CanExecute());
        SongsAddToQueueCommand = new GenericRelayCommand<object>(param => SongsAddToQueueCommand_Execute(param), param => SongsAddToQueueCommand_CanExecute());

        SongsListviewPlayThisCommand = new GenericRelayCommand<object>(param => SongsListviewPlayThisCommand_Execute(param), param => SongsListviewPlayThisCommand_CanExecute());
        SongsListviewAddThisCommand = new GenericRelayCommand<object>(param => SongsListviewAddThisCommand_Execute(param), param => SongsListviewAddThisCommand_CanExecute());

        SearchPageSongsAddToPlaylistCommand = new GenericRelayCommand<object>(param => SearchPageSongsAddToPlaylistCommand_Execute(param), param => SearchPageSongsAddToPlaylistCommand_CanExecute());

        FilesPlayCommand = new GenericRelayCommand<object>(param => FilesPlayCommand_Execute(param), param => FilesPlayCommand_CanExecute());
        FilesAddToQueueCommand = new GenericRelayCommand<object>(param => FilesAddToQueueCommand_Execute(param), param => FilesAddToQueueCommand_CanExecute());
        FilesPageFilesAddToPlaylistCommand = new GenericRelayCommand<object>(param => FilesPageFilesAddToPlaylistCommand_Execute(param), param => FilesPageFilesAddToPlaylistCommand_CanExecute());
        FilesListviewSaveSelectedToCommand = new GenericRelayCommand<object>(param => FilesListviewSaveSelectedToCommand_Execute(param), param => FilesListviewSaveSelectedToCommand_CanExecute());

        FilesListviewPlayThisCommand = new GenericRelayCommand<object>(param => FilesListviewPlayThisCommand_Execute(param), param => FilesListviewPlayThisCommand_CanExecute());
        FilesListviewAddThisCommand = new GenericRelayCommand<object>(param => FilesListviewAddThisCommand_Execute(param), param => FilesListviewAddThisCommand_CanExecute());

        TryConnectCommand = new RelayCommand(TryConnectCommand_Execute, TryConnectCommand_CanExecute);

        #endregion

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
        _elapsedTimer = new System.Timers.Timer(100);
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

        IsShowDebugWindow = false;

#if DEBUG
        IsSaveLog = true;
        IsEnableDebugWindow = true;

#else
        IsSaveLog = false;
        IsEnableDebugWindow = false;
#endif
    }

    #region == Startup and Shutdown ==

    public void LoadSettings()
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
                                // Since there is no restorebounds in AvaloniaUI.
                                WindowState = WindowState.Normal;
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

                                    NotifyPropertyChanged(nameof(IsCurrentProfileSet));

                                    Profiles.Add(pro);
                                }
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
                                // "NotifyPropertyChanged(nameof(IsNavigationViewMenuOpen));"
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
                                                LibraryColumnHeaderTitleWidth = Double.Parse(s);
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
                                                LibraryColumnHeaderFilePathWidth = Double.Parse(s);
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
                                    /*
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
                                    */
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
                                    /*
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
                                    */
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
                                    /*
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
                                    */
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
                                    /*
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
                                    */
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
                                                PlaylistColumnHeaderDiscWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Track");
                                if (column is not null)
                                {
                                    /*
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
                                    */
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
                                                PlaylistColumnHeaderTrackWidth = 53;
                                            }
                                        }
                                    }
                                }
                                column = Playl.Element("Genre");
                                if (column is not null)
                                {
                                    /*
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
                                    */
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
                                    /*
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
                                    */
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

            // TODO: Since AvaloniaUI does not call OnContentRendered....
            IsFullyRendered = true;
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
        NotifyPropertyChanged(nameof(IsCurrentProfileSet));
    }

    // Startup
    public void OnWindowLoaded(object? sender, EventArgs e)
    {
        if (CurrentProfile is null)
        {
            //ConnectionStatusMessage = MPDCtrlX.Properties.Resources.Init_NewConnectionSetting; // no need. 
            StatusButton = _pathNewConnectionButton;

            // Show connection setting
            IsConnectionSettingShow = true;

            //var InitWin = new InitWindow();// use DI.
            _initWin.DataContext = this;
            _initWin.ShowDialog(owner: App.GetService<MainWindow>());

            return;
        }

        IsConnectionSettingShow = false;

        // set this "quietly"
        _volume = CurrentProfile.Volume;
        NotifyPropertyChanged(nameof(Volume));

        // start the connection
        Start(CurrentProfile.Host, CurrentProfile.Port);

    }

    // On window's content rendered <<< TODO: Not called in AvaloniaUI
    public void OnContentRendered(object? sender, EventArgs e)
    {
        IsFullyRendered = true;
    }

    // Closing
    public void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        // Make sure Window and settings have been fully loaded and not overriding with empty data.
        if (!IsFullyLoaded)
            return;

        // TODO: Since AvaloniaUI does not call OnContentRendered....
        IsFullyRendered = true;

        // This is a dirty work around for AvaloniaUI.
        QueuePage? qp = App.GetService<QueuePage>();
        qp?.SaveQueueHeaderWidth();
        FilesPage? fp = App.GetService<FilesPage>();
        fp?.SaveFilesHeaderWidth();
        PlaylistItemPage? pp = App.GetService<PlaylistItemPage>();
        pp?.SavePlaylistItemsHeaderWidth();

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
        if (sender is Window w)
        {
            #region == Window settings ==

            // Main Window element
            XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

            //Window w = (sender as Window);
            // Main Window attributes
            attrs = doc.CreateAttribute("height");
            if (w.WindowState == WindowState.Maximized)
            {
                //attrs.Value = w.RestoreBounds.Height.ToString();
            }
            else
            {
                attrs.Value = w.Height.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("width");
            if (w.WindowState == WindowState.Maximized)
            {
                //attrs.Value = w.RestoreBounds.Width.ToString();
                //windowWidth = w.RestoreBounds.Width;
            }
            else
            {
                attrs.Value = w.Width.ToString();
                windowWidth = w.Width;

            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("top");
            if (w.WindowState == WindowState.Maximized)
            {
                //attrs.Value = w.RestoreBounds.Top.ToString();
            }
            else
            {
                //attrs.Value = w.Top.ToString();
                attrs.Value = w.Position.Y.ToString();
            }
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("left");
            if (w.WindowState == WindowState.Maximized)
            {
                //attrs.Value = w.RestoreBounds.Left.ToString();
            }
            else
            {
                //attrs.Value = w.Left.ToString();
                attrs.Value = w.Position.X.ToString();
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
            if (IsFullyRendered) // << Not called in AvaloniaUI
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


            // FilesPage
            XmlElement filesHeader;
            XmlElement filesHeaderColumn;
            XmlAttribute fAttrs;

            filesHeader = doc.CreateElement(string.Empty, "Files", string.Empty);

            filesHeaderColumn = doc.CreateElement(string.Empty, "FileName", string.Empty);
            /*
            fAttrs = doc.CreateAttribute("Visible");
            fAttrs.Value = 
            filesHeaderColumn.SetAttributeNode(fAttrs);
            */
            fAttrs = doc.CreateAttribute("Width");
            fAttrs.Value = LibraryColumnHeaderTitleWidth.ToString();
            filesHeaderColumn.SetAttributeNode(fAttrs);
            //
            filesHeader.AppendChild(filesHeaderColumn);


            filesHeaderColumn = doc.CreateElement(string.Empty, "FilePath", string.Empty);
            /*
            fAttrs = doc.CreateAttribute("Visible");
            fAttrs.Value = 
            filesHeaderColumn.SetAttributeNode(fAttrs);
            */
            fAttrs = doc.CreateAttribute("Width");
            fAttrs.Value = LibraryColumnHeaderFilePathWidth.ToString();
            filesHeaderColumn.SetAttributeNode(fAttrs);
            //
            filesHeader.AppendChild(filesHeaderColumn);

            //
            headers.AppendChild(filesHeader);


            // Playlist page
            XmlElement PlaylistHeader;
            XmlElement PlaylistHeaderColumn;
            XmlAttribute pAttrs;

            PlaylistHeader = doc.CreateElement(string.Empty, "Playlist", string.Empty);

            // Position
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Position", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderPositionVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

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

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderTimeVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderTimeWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Artist
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Artist", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderArtistVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderArtistWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Album
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Album", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderAlbumVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderAlbumWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Disc
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Disc", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderDiscVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderDiscWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Track
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Track", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderTrackVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderTrackWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Genre
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "Genre", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderGenreVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderGenreWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            // Last Modified
            PlaylistHeaderColumn = doc.CreateElement(string.Empty, "LastModified", string.Empty);

            //pAttrs = doc.CreateAttribute("Visible");
            //pAttrs.Value = IsPlaylistColumnHeaderLastModifiedVisible.ToString();
            //PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            pAttrs = doc.CreateAttribute("Width");
            pAttrs.Value = PlaylistColumnHeaderLastModifiedWidth.ToString();
            PlaylistHeaderColumn.SetAttributeNode(pAttrs);

            PlaylistHeader.AppendChild(PlaylistHeaderColumn);

            //
            headers.AppendChild(PlaylistHeader);

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

        if (IsRememberAsProfile)
        {
            root.AppendChild(xProfiles);
        }

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
                _mpc.MpdDisconnect();
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

    #endregion

    #region == Methods ==

    private async void Start(string host, int port)
    {
        HostIpAddress = null;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            if (addresses.Length > 0)
            {
                HostIpAddress = addresses[0];
            }
            else
            {
                // TODO::
                ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
                StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
                return;
            }
        }
        catch (Exception)
        {
            // TODO::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
            return;
        }

        // Start MPD connection.
        _ = Task.Run(() => _mpc.MpdIdleConnect(HostIpAddress.ToString(), port));
    }

    private async void LoadInitialData()
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
                MpdStatusButton = _pathMpdAckErrorButton;
                //StatusBarMessage = string.Format(MPDCtrlX.Properties.Resources.StatusBarMsg_MPDVersionIsOld, _mpc.MpdVerText);
                MpdStatusMessage = string.Format(MPDCtrlX.Properties.Resources.StatusBarMsg_MPDVersionIsOld, _mpc.MpdVerText);
            }
        }
    }

    private void UpdateStatus()
    {
        UpdateButtonStatus();

        UpdateProgress?.Invoke(this, "[UI] Status updating...");

        Dispatcher.UIThread.Post(async () =>
        {
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

                        //CurrentSong.IsSelected = true;

                        if (IsAutoScrollToNowPlaying)
                        {
                            ScrollIntoView?.Invoke(this, CurrentSong.Index);
                        }

                        //IsAlbumArtVisible = false;
                        AlbumCover = null;
                        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

                        // AlbumArt
                        if (!String.IsNullOrEmpty(CurrentSong.File))
                        {
                            if (IsDownloadAlbumArt)
                            {
                                //Debug.WriteLine("getting album cover. @UpdateStatus()");
                                var res = await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);

                                if (res.AlbumCover?.SongFilePath == CurrentSong.File)
                                {
                                    if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading))
                                    {
                                        Dispatcher.UIThread.Post(() =>
                                        {
                                            AlbumCover = res.AlbumCover;
                                            AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                            //IsAlbumArtVisible = true;
                                        });
                                    }
                                }

                                CurrentSong.IsAlbumCoverNeedsUpdate = false;
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
        });

        UpdateProgress?.Invoke(this, "");
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

                if (_mpc.MpdStatus.MpdVolumeIsSet)
                {
                    double tmpVol = Convert.ToDouble(_mpc.MpdStatus.MpdVolume);
                    if (_volume != tmpVol)
                    {
                        // "quietly" update.
                        _volume = tmpVol;
                        NotifyPropertyChanged(nameof(Volume));
                    }
                }

                _random = _mpc.MpdStatus.MpdRandom;
                NotifyPropertyChanged(nameof(Random));

                _repeat = _mpc.MpdStatus.MpdRepeat;
                NotifyPropertyChanged(nameof(Repeat));

                _consume = _mpc.MpdStatus.MpdConsume;
                NotifyPropertyChanged(nameof(Consume));

                _single = _mpc.MpdStatus.MpdSingle;
                NotifyPropertyChanged(nameof(Single));

                //start elapsed timer.
                if (_mpc.MpdStatus.MpdState == Status.MpdPlayState.Play)
                {
                    // no need to care about "double" updates for time.
                    Time = Convert.ToInt32(_mpc.MpdStatus.MpdSongTime * 10);

                    _elapsed = Convert.ToInt32(_mpc.MpdStatus.MpdSongElapsed * 10);

                    if (!_elapsedTimer.Enabled)
                        _elapsedTimer.Start();
                }
                else
                {
                    _elapsedTimer.Stop();

                    // no need to care about "double" updates for time.
                    Time = Convert.ToInt32(_mpc.MpdStatus.MpdSongTime * 10);

                    _elapsed = Convert.ToInt32(_mpc.MpdStatus.MpdSongElapsed * 10);
                    //NotifyPropertyChanged(nameof(Elapsed));
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
        Dispatcher.UIThread.Post(async () =>
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
                        if (!String.IsNullOrEmpty(_mpc.MpdCurrentSong.File))
                        {
                            if (IsDownloadAlbumArt)
                            {
                                //Debug.WriteLine("getting album cover. @UpdateCurrentSong()");
                                /*
                                //await _mpc.MpdQueryAlbumArt(CurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                                var res = await _mpc.MpdQueryAlbumArt(_mpc.MpdCurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);

                                if (res.AlbumCover?.SongFilePath == _mpc.MpdCurrentSong.File)
                                {
                                    if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading))
                                    {
                                        AlbumCover = res.AlbumCover;
                                        //AlbumArtBitmapSource = res.AlbumCover?.AlbumImageSource;
                                    }
                                }
                                else
                                {
                                    //AlbumCover = null;
                                    ////IsAlbumArtVisible = false;
                                    //AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                                }
                                */

                                var res = await _mpc.MpdQueryAlbumArt(_mpc.MpdCurrentSong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);

                                if (res.AlbumCover?.SongFilePath == _mpc.MpdCurrentSong.File)
                                {
                                    if ((res.AlbumCover.IsSuccess) && (!res.AlbumCover.IsDownloading))
                                    {
                                        Dispatcher.UIThread.Post(() =>
                                        {
                                            AlbumCover = res.AlbumCover;
                                            AlbumArtBitmapSource = AlbumCover.AlbumImageSource;
                                            //IsAlbumArtVisible = true;
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("if (isSongChanged || isCurrentSongWasNull). @UpdateCurrentSong()");
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

    private async void UpdateCurrentQueue()
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

            Dispatcher.UIThread.Post(() =>
            {
                IsWorking = true;
            });

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

                Dispatcher.UIThread.Post(() =>
                {
                    IsWorking = true;

                    UpdateProgress?.Invoke(this, "[UI] Updating the queue...");

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

                    // AvaloniaUI doesn't have this.
                    //var collectionView = CollectionViewSource.GetDefaultView(Queue);
                    // no need to add because It's been added when "loading".
                    //collectionView.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
                    //collectionView.Refresh();

                    // This is not good, all the selections will be cleared, but no problem?.
                    Queue = new ObservableCollection<SongInfoEx>(Queue.OrderBy(n => n.Index));
                    //UpdateProgress?.Invoke(this, "");

                    UpdateProgress?.Invoke(this, "[UI] Checking current song after Queue update.");

                    // Set Current and NowPlaying.
                    var curitem = Queue.FirstOrDefault(i => i.Id == _mpc.MpdStatus.MpdSongID);
                    if (curitem is not null)
                    {
                        CurrentSong = curitem;
                        CurrentSong.IsPlaying = true;
                        
                        // Don't. because it's not like song is changed.
                        /*
                        if (IsAutoScrollToNowPlaying)
                            // ScrollIntoView while don't change the selection 
                            ScrollIntoView?.Invoke(this, CurrentSong.Index);
                        */
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
                });

                #endregion
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception@UpdateCurrentQueue: " + e.Message);

                UpdateProgress?.Invoke(this, "Exception@UpdateCurrentQueue: " + e.Message);

                Dispatcher.UIThread.Post(() =>
                {
                    IsWorking = false;
                    App.AppendErrorLog("Exception@UpdateCurrentQueue", e.Message);
                });

                return;
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsWorking = false;
                });
                UpdateProgress?.Invoke(this, "");
            }

            Dispatcher.UIThread.Post(() =>
            {
                IsWorking = false;
            });
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

            Dispatcher.UIThread.Post(() =>
            {
                IsWorking = true;
            });

            try
            {
                Dispatcher.UIThread.Post(() =>
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
                                        /*
                                        if (_mpc.AlbumCover.SongFilePath != curitem.File)
                                        {
                                            //IsAlbumArtVisible = false;
                                            //AlbumArt = _albumArtDefault;

                                            if (!String.IsNullOrEmpty(CurrentSong.File))
                                            {
                                                //isAlbumArtChanged = true;
                                            }
                                        }
                                        */
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

                                if (!String.IsNullOrEmpty(CurrentSong.File))
                                {
                                    //isAlbumArtChanged = true;
                                }
                            }
                            */
                        }
                        else
                        {
                            Debug.WriteLine("Oops. Maybe start up with playback Stop? @UpdateCurrentQueue()");
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
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception@UpdateCurrentQueue: " + e.Message);

                StatusBarMessage = "Exception@UpdateCurrentQueue: " + e.Message;

                Dispatcher.UIThread.Post(() =>
                {
                    IsWorking = false;

                    App.AppendErrorLog("Exception@UpdateCurrentQueue", e.Message);
                });

                return;
            }
            finally 
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsWorking = false;
                });
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

        Dispatcher.UIThread.Post(() =>
        {
            IsWorking = false;
        });
    }
    
    private void UpdateAlbumsAndArtists()
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
    }

    private async void UpdatePlaylists()
    {
        /*
        if (IsSwitchingProfile)
            return;
        */
        UpdateProgress?.Invoke(this, "[UI] Playlists loading...");

        await Task.Delay(10);
        
        Dispatcher.UIThread.Post( () => 
        {
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
            NotifyPropertyChanged(nameof(IsNavigationViewMenuOpen));

            if (!string.IsNullOrEmpty(RenamedSelectPendingPlaylistName))
            {
                GoToRenamedPlaylistPage(RenamedSelectPendingPlaylistName);
                RenamedSelectPendingPlaylistName = string.Empty;
            }
        });
    }

    private Task<bool> UpdateLibraryMusicAsync()
    {
        // Music files
        /*
        if (IsSwitchingProfile)
            return Task.FromResult(false);
        */

        UpdateProgress?.Invoke(this, "[UI] Library songs loading...");

        //IsBusy = true;
        IsWorking = true;

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

                //Application.Current?.Dispatcher.Invoke(() => { (App.Current as App)?.AppendErrorLog("Exception@UpdateLibraryMusic", e.Message); });
                Dispatcher.UIThread.Post(() => 
                {
                    //IsBusy = false;
                    IsWorking = false;
                    App.AppendErrorLog("Exception@UpdateLibraryMusic", e.Message);
                });
                return Task.FromResult(false);
            }
        }
        /*
        if (IsSwitchingProfile)
            return Task.FromResult(false);
        */
        //IsBusy = true;
        IsWorking = true;

        
        Dispatcher.UIThread.Post(() => {
            UpdateProgress?.Invoke(this, "[UI] Library songs loading...");
            MusicEntries = new ObservableCollection<NodeFile>(tmpMusicEntries);// COPY

            _musicEntriesFiltered = _musicEntriesFiltered = new ObservableCollection<NodeFile>(tmpMusicEntries);
            NotifyPropertyChanged(nameof(MusicEntriesFiltered));

            UpdateProgress?.Invoke(this, "");
            //IsBusy = false;
            IsWorking = false;
        });

        //IsBusy = false;
        IsWorking = false;
        return Task.FromResult(true);
    }

    private Task<bool> UpdateLibraryDirectoriesAsync()
    {
        // Directories
        /*
        if (IsSwitchingProfile)
            return Task.FromResult(false);
        */

        UpdateProgress?.Invoke(this, "[UI] Library directories loading...");

        //IsBusy = true;
        IsWorking = true;

        try
        {
            var tmpMusicDirectories = new DirectoryTreeBuilder("");
            //tmpMusicDirectories.Load([.. _mpc.LocalDirectories]);
            //_musicDirectories.Load(_mpc.LocalDirectories.ToList<String>());
            tmpMusicDirectories.Load(_mpc.LocalDirectories);

            IsWorking = true;

            Dispatcher.UIThread.Post(() => {

                UpdateProgress?.Invoke(this, "[UI] Library directories loading...");

                MusicDirectories = new ObservableCollection<NodeTree>(tmpMusicDirectories.Children);// COPY
                if (MusicDirectories.Count > 0)
                {
                    if (MusicDirectories[0] is NodeDirectory nd)
                        _selectedNodeDirectory = nd;
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

            });
            UpdateProgress?.Invoke(this, "");

            //IsBusy = false;
            IsWorking = false;
        }
        catch (Exception e)
        {
            Debug.WriteLine("_musicDirectories.Load: " + e.Message);

            Dispatcher.UIThread.Post(() => 
            {
                //IsBusy = false;
                IsWorking = false; 
                App.AppendErrorLog("Exception@UpdateLibraryDirectories", e.Message); 
            });
            return Task.FromResult(false);
        }
        finally
        {
            //IsBusy = false;
            IsWorking = false;
        }

        //IsBusy = false;
        IsWorking = false;
        UpdateProgress?.Invoke(this, "");

        return Task.FromResult(true);
    }

    private void GetPlaylistSongs(NodeMenuPlaylistItem playlistNode)
    {
        if (playlistNode is null)
            return;

        Dispatcher.UIThread.Post(async () => {
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

    private async void GetArtistSongs(AlbumArtist? artist)
    {
        if (artist is null)
        {
            Debug.WriteLine("GetArtistSongs: artist is null, returning.");
            return;
        }

        var r = await SearchArtistSongs(artist.Name).ConfigureAwait(ConfigureAwaitOptions.None);

        Dispatcher.UIThread.Post(() =>
        {
            UpdateProgress?.Invoke(this, "");
        });

        if (!r.IsSuccess)
        {
            Debug.WriteLine("GetArtistSongs: SearchArtistSongs returned false, returning.");
            return;
        }
        if (artist is null)
        {
            Debug.WriteLine("GetArtistSongs: SelectedAlbumArtist is null, returning.");
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (r.SearchResult is null)
            {
                Debug.WriteLine("GetArtistSongs: SearchResult is null, returning.");
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
                    if (song.Album.Equals(slbm.Name))
                    {
                        slbm.Songs.Add(song);
                    }
                }

                slbm.IsSongsAcquired = true;
            }
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
            else
            {
                //Debug.WriteLine($"GetAlbumPictures: Processing album ({album.Name})");
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

    private static int CompareVersionString(string a, string b)
    {
        return (new System.Version(a)).CompareTo(new System.Version(b));
    }

    #region == MPD event callback == 

    private void OnMpdIdleConnected(MpcService sender)
    {
        Debug.WriteLine("OK MPD " + _mpc.MpdVerText + " @OnMpdConnected");

        MpdVersion = _mpc.MpdVerText;

        //MpdStatusMessage = MpdVersion;// + ": " + MPDCtrlX.Properties.Resources.MPD_StatusConnected;

        MpdStatusButton = _pathMpdOkButton;

        // Need this to show CurrentSong.
        IsConnected = true;

        IsShowAckWindow = false;
        IsShowErrWindow = false;

        // Connected from InitWindow, so save and clean up. 
        Dispatcher.UIThread.Post(() =>
        {
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
                            CurrentProfile = prof;
                            Profiles.Add(prof);

                            NotifyPropertyChanged(nameof(IsCurrentProfileSet));
                        }
                        else
                        {
                            Debug.WriteLine("Host info is empty. @OnMpdIdleConnected");
                        }
                        Debug.WriteLine($"Password = {_password}");
                    }

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _initWin.Close();
                    });
                }
            }
        });

        // 
        _ = Task.Run(() => LoadInitialData());
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
        Dispatcher.UIThread.Post(() =>
        {
            /*
            if (AlbumCover is null)
            {
                IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                return;
            }

            if ((AlbumCover.IsDownloading) || (!AlbumCover.IsSuccess))
            {
                AlbumCover = null;
                IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                return;
            }

            if ((CurrentSong is null) || (AlbumCover.AlbumImageSource is null))
            {
                AlbumCover = null;
                IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                return;
            }

            if (String.IsNullOrEmpty(CurrentSong?.File))
            {
                AlbumCover = null;
                IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                return;
            }
            */
            
            if ((_mpc.MpdCurrentSong?.File == AlbumCover?.SongFilePath) || (CurrentSong?.File == AlbumCover?.SongFilePath))
            {
                //AlbumArtBitmapSource = AlbumCover?.AlbumImageSource;
                //IsAlbumArtVisible = true;

                // save album cover to cache.
                var strAlbum = CurrentSong?.Album ?? string.Empty;
                if (!string.IsNullOrEmpty(strAlbum.Trim()))
                {
                    var aat = CurrentSong?.Artist.Trim();
                    if (string.IsNullOrEmpty(aat))
                    {
                        aat = CurrentSong?.AlbumArtist;
                    }
                    var strArtist = aat;
                    if (string.IsNullOrEmpty(strArtist))
                    {
                        strArtist = "Unknown Artist";
                    }
                    strArtist = EscapeFilePathNames(strArtist).Trim();
                    strAlbum = EscapeFilePathNames(strAlbum).Trim();

                    string strDirPath = System.IO.Path.Combine(App.AppDataCacheFolder, strArtist);
                    string filePath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".bmp";
                    try
                    {
                        Directory.CreateDirectory(strDirPath);
                        AlbumCover?.AlbumImageSource?.Save(filePath, 100);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("OnAlbumArtChanged: Exception while saving album art: " + e.Message);
                    }
                }
            }
            else
            {
                AlbumCover = null;
                //IsAlbumArtVisible = false;
                AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
            }
        });

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
        else if (status == MpcService.ConnectionStatus.ConnectFail_Timeout)
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
        }
        else if (status == MpcService.ConnectionStatus.DisconnectedByUser)
        {
            IsConnected = false;
            IsConnecting = false;
            IsNotConnectingNorConnected = true;
            //IsConnectionSettingShow = true;

            Debug.WriteLine("ConnectionStatus_DisconnectedByUser");
            ConnectionStatusMessage = MPDCtrlX.Properties.Resources.ConnectionStatus_DisconnectedByUser;
            StatusButton = _pathDisconnectedButton;

            StatusBarMessage = ConnectionStatusMessage;
        }
        else if (status == MpcService.ConnectionStatus.SendFail_NotConnected)
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
        else if (status == MpcService.ConnectionStatus.SendFail_Timeout)
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

        Dispatcher.UIThread.Post(() =>
        {
            AckWindowOutput?.Invoke(this, MpdVersion + ": " + MPDCtrlX.Properties.Resources.MPD_CommandError + " - " + s + Environment.NewLine);
        });

        if (origin.Equals("Command"))
        {
            InfoBarAckTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_CommandError;
        }
        else if (origin.Equals("Idle"))
        {
            InfoBarAckTitle = MpdVersion;// + " " + MPDCtrlX.Properties.Resources.MPD_StatusError; // TODO:
        }
        else
        {
            InfoBarAckTitle = MpdVersion;
        }

        InfoBarAckMessage = s;

        IsShowAckWindow = true;
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

        if (origin.Equals("Command"))
        {
            InfoBarErrTitle = MpdVersion + " " + MPDCtrlX.Properties.Resources.MPD_CommandError;
        }
        else if (origin.Equals("Idle"))
        {
            InfoBarErrTitle = MpdVersion;// + " " + MPDCtrlX.Properties.Resources.MPD_StatusError; // TODO:
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
        StatusBarMessage = msg;
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
            _elapsed += 1;
            NotifyPropertyChanged(nameof(Elapsed));
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

    public IRelayCommand PlayCommand { get; }
    public bool PlayCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void PlayCommand_ExecuteAsync()
    {
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

    public IRelayCommand PlayNextCommand { get; }
    public bool PlayNextCommand_CanExecute()
    {
        if (IsBusy) return false;
        //if (Queue.Count < 1) { return false; }
        return true;
    }
    public async void PlayNextCommand_ExecuteAsync()
    {
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackNext(Convert.ToInt32(_volume));
    }

    public IRelayCommand PlayPrevCommand { get; }
    public bool PlayPrevCommand_CanExecute()
    {
        if (IsBusy) return false;
        //if (Queue.Count < 1) { return false; }
        return true;
    }
    public async void PlayPrevCommand_ExecuteAsync()
    {
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackPrev(Convert.ToInt32(_volume));
    }

    public IRelayCommand ChangeSongCommand { get; set; }
    public bool ChangeSongCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (Queue.Count < 1) { return false; }
        if (_selectedQueueSong is null) { return false; }
        return true;
    }
    public async void ChangeSongCommand_ExecuteAsync()
    {
        if (_selectedQueueSong is not null)
            await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), _selectedQueueSong.Id);
    }

    public IRelayCommand PlayPauseCommand { get; }
    public bool PlayPauseCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void PlayPauseCommand_Execute()
    {
        await _mpc.MpdPlaybackPause();
    }

    public IRelayCommand PlayStopCommand { get; }
    public bool PlayStopCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void PlayStopCommand_Execute()
    {
        await _mpc.MpdPlaybackStop();
    }

    #endregion

    #region == Playback opts ==

    public IRelayCommand SetRandomCommand { get; }
    public bool SetRandomCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetRandomCommand_ExecuteAsync()
    {
        await _mpc.MpdSetRandom(_random);
    }

    public IRelayCommand SetRpeatCommand { get; }
    public bool SetRpeatCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetRpeatCommand_ExecuteAsync()
    {
        await _mpc.MpdSetRepeat(_repeat);
    }

    public IRelayCommand SetConsumeCommand { get; }
    public bool SetConsumeCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetConsumeCommand_ExecuteAsync()
    {
        await _mpc.MpdSetConsume(_consume);
    }

    public IRelayCommand SetSingleCommand { get; }
    public bool SetSingleCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetSingleCommand_ExecuteAsync()
    {
        await _mpc.MpdSetSingle(_single);
    }

    #endregion

    #region == Playback seek and volume ==

    public IRelayCommand SetVolumeCommand { get; }
    public bool SetVolumeCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetVolumeCommand_ExecuteAsync()
    {
        await _mpc.MpdSetVolume(Convert.ToInt32(_volume));
    }

    public IRelayCommand SetSeekCommand { get; }
    public bool SetSeekCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void SetSeekCommand_ExecuteAsync()
    {
        double elapsed = _elapsed / 10;
        await _mpc.MpdPlaybackSeek(_mpc.MpdStatus.MpdSongID, elapsed);
    }

    public IRelayCommand VolumeMuteCommand { get; }
    public bool VolumeMuteCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public async void VolumeMuteCommand_Execute()
    {
        await _mpc.MpdSetVolume(0);
    }

    public IRelayCommand VolumeDownCommand { get; }
    public bool VolumeDownCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public void VolumeDownCommand_Execute()
    {
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

    public IRelayCommand VolumeUpCommand { get; }
    public bool VolumeUpCommand_CanExecute()
    {
        if (IsBusy) return false;
        return true;
    }
    public void VolumeUpCommand_Execute()
    {
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
    public IRelayCommand QueueSaveToCommand { get; }
    public static bool QueueSaveToCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) return false;
        */
        return true;
    }
    public void QueueSaveToCommand_ExecuteAsync()
    {
        if (Queue.Count == 0)
            return;

        QueueSaveToDialogShow?.Invoke(this, EventArgs.Empty);
    }

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

    // TODO:  Enter or double click from code behind.
    public IRelayCommand QueueListviewEnterKeyCommand { get; set; }
    public static bool QueueListviewEnterKeyCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (IsWorking) return false;
        return true;
    }
    public async void QueueListviewEnterKeyCommand_ExecuteAsync()
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
    public IRelayCommand QueueListviewLeftDoubleClickCommand { get; set; }
    public static bool QueueListviewLeftDoubleClickCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (IsWorking) return false;
        //if (Queue.Count < 1) { return false; }
        //if (_selectedQueueSong is null) { return false; }
        return true;
    }
    public async void QueueListviewLeftDoubleClickCommand_ExecuteAsync(SongInfoEx song)
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
    public IRelayCommand QueueClearWithoutPromptCommand { get; }
    public static bool QueueClearWithoutPromptCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (IsWorking) return false;
        //if (Queue.Count == 0) { return false; }
        return true;
    }
    public async void QueueClearWithoutPromptCommand_ExecuteAsync()
    {
        if (Queue.Count == 0) { return; }

        await _mpc.MpdPlaybackStop();
        await _mpc.MpdClear();
    }

    // Command to delete selected songs from the queue listview.
    public ICommand QueueListviewDeleteSelectedWithoutPromptCommand { get; }
    public bool QueueListviewDeleteSelectedWithoutPromptCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        //if (SelectedQueueSong is null) return false;
        return true;
    }
    public async void QueueListviewDeleteSelectedWithoutPromptCommand_Execute(object obj)
    {
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
    public IRelayCommand QueueListviewMoveUpCommand { get; }
    public static bool QueueListviewMoveUpCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) return false;
        */
        return true;
    }
    public async void QueueListviewMoveUpCommand_Execute(object obj)
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
    public IRelayCommand QueueListviewMoveDownCommand { get; }
    public static bool QueueListviewMoveDownCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) return false;
        */
        return true;
    }
    public async void QueueListviewMoveDownCommand_Execute(object obj)
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
    public IRelayCommand QueueListviewSortReverseCommand { get; }
    public bool QueueListviewSortReverseCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) { return false; }
        //if (CurrentSong is null) { return false; }
        return true;
    }
    public async void QueueListviewSortReverseCommand_Execute()
    {
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
    public IRelayCommand QueueListviewSortByCommand { get; }
    public bool QueueListviewSortByCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) return false;
        return true;
    }
    public async void QueueListviewSortByCommand_Execute(string key)
    {
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

    // Add selected to playlist
    public IRelayCommand QueueListviewSaveSelectedToCommand { get; }
    public static bool QueueListviewSaveSelectedToCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (Queue.Count == 0) return false;
        if (SelectedQueueSong is null) return false;
        */
        return true;
    }
    public void QueueListviewSaveSelectedToCommand_Execute(object obj)
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

        QueueListviewSaveToDialogShow?.Invoke(this, selectedList);
    }

    // ScrollIntoNowPlaying
    public IRelayCommand ScrollIntoNowPlayingCommand { get; }
    public bool ScrollIntoNowPlayingCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        //if (Queue.Count == 0) { return false; }
        //if (CurrentSong is null) { return false; }
        return true;
    }
    public void ScrollIntoNowPlayingCommand_Execute()
    {
        if (Queue.Count == 0) return;
        if (CurrentSong is null) return;
        if (Queue.Count < CurrentSong.Index + 1) return;

        // should I?
        //SelectedQueueSong = CurrentSong;

        ScrollIntoView?.Invoke(this, CurrentSong.Index);
    }

    public IRelayCommand FilterQueueClearCommand { get; }
    public bool FilterQueueClearCommand_CanExecute()
    {
        if (string.IsNullOrEmpty(FilterQueueQuery))
            return false;
        return true;
    }
    public void FilterQueueClearCommand_Execute()
    {
        FilterQueueQuery = "";
    }

    public IRelayCommand QueueFindShowHideCommand { get; }
    public static bool QueueFindShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueFindShowHideCommand_Execute()
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
        QueueFindWindowVisibilityChanged_SetFocus?.Invoke(this, EventArgs.Empty);
    }

    //
    public IRelayCommand QueueFilterSelectCommand { get; set; }
    public static bool QueueFilterSelectCommand_CanExecute()
    {
        return true;
    }
    public void QueueFilterSelectCommand_Execute()//object obj
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

    public IRelayCommand SearchExecCommand { get; }
    public static bool SearchExecCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (string.IsNullOrEmpty(SearchQuery))
            return false;
        */
        return true;
    }
    public async void SearchExecCommand_Execute()
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
    public IRelayCommand SearchListviewSaveSelectedToCommand { get; }
    public static bool SearchListviewSaveSelectedToCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (SearchResult is null) return false;
        //if (SearchResult.Count == 0) return false;
        return true;
    }
    public void SearchListviewSaveSelectedToCommand_Execute(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        var collection = items.Cast<SongInfo>();

        List<String> uriList = [];

        foreach (var item in collection)
        {
            uriList.Add(item.File);
        }

        if (uriList.Count > 0)
        {
            SearchPageAddToPlaylistDialogShow?.Invoke(this, uriList);
        }
    }

    // Add to playlist
    public IRelayCommand SearchPageSongsAddToPlaylistCommand { get; set; }
    public static bool SearchPageSongsAddToPlaylistCommand_CanExecute()
    {
        return true;
    }
    public void SearchPageSongsAddToPlaylistCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<SongInfo> list)
        {
            List<String> uriList = [];

            foreach (var item in list)
            {
                uriList.Add(item.File);
            }

            if (uriList.Count > 0)
            {
                SearchPageAddToPlaylistDialogShow?.Invoke(this, uriList);
            }
        }
    }

    #endregion

    #region == Files ==
    
    //
    public IRelayCommand FilesListviewAddSelectedToQueueCommand { get; }
    public static bool FilesListviewAddSelectedToQueueCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (IsWorking) return false;
        //if (MusicEntries.Count == 0) return false;
        return true;
    }
    public async void FilesListviewAddSelectedToQueueCommand_Execute(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        if (items.Count > 1)
        {
            var collection = items.Cast<NodeFile>();

            List<String> uriList = [];

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
    public IRelayCommand FilesListviewSaveSelectedToCommand { get; }
    public static bool FilesListviewSaveSelectedToCommand_CanExecute()
    {
        return true;
    }
    public void FilesListviewSaveSelectedToCommand_Execute(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        var collection = items.Cast<NodeFile>();

        List<String> uriList = [];

        foreach (var item in collection)
        {
            uriList.Add(item.OriginalFileUri);
        }

        if (uriList.Count > 0)
        {
            FilesPageAddToPlaylistDialogShow?.Invoke(this, uriList);
        }
    }

    /*
    public IRelayCommand SongFilesListviewSaveSelectedAsCommand { get; }
    public bool SongFilesListviewSaveSelectedAsCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (MusicEntries.Count == 0) return false;
        return true;
    }
    public void SongFilesListviewSaveSelectedAsCommand_Execute(object obj)
    {
        if (obj is null) return;

        List<NodeFile> selectedList = [];

        
        Dispatcher.UIThread.Post(() =>
        {
            System.Collections.IList items = (System.Collections.IList)obj;

            var collection = items.Cast<NodeFile>();

            foreach (var item in collection)
            {
                selectedList.Add(item as NodeFile);
            }
        });

        List<string> fileUrisToAddList = [];

        foreach (var item in selectedList)
        {
            if (!string.IsNullOrEmpty(item.OriginalFileUri))
                fileUrisToAddList.Add(item.OriginalFileUri);
        }

        if (fileUrisToAddList.Count == 0)
            return;

        songFilesListviewSelectedQueueSongUriForPopup = fileUrisToAddList;

        IsSongFilesSelectedSaveAsPopupVisible = true;

    }

    public IRelayCommand SongFilesListviewSaveSelectedAsPopupCommand { get; }
    public bool SongFilesListviewSaveSelectedAsPopupCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (MusicEntries.Count == 0) return false;
        return true;
    }
    public void SongFilesListviewSaveSelectedAsPopupCommand_Execute(string playlistName)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;
        if (songFilesListviewSelectedQueueSongUriForPopup.Count < 1)
            return;

        await _mpc.MpdPlaylistAdd(playlistName, songFilesListviewSelectedQueueSongUriForPopup);

        songFilesListviewSelectedQueueSongUriForPopup.Clear();

        IsSongFilesSelectedSaveAsPopupVisible = false;
    }

    public IRelayCommand SongFilesListviewSaveSelectedToCommand { get; }
    public bool SongFilesListviewSaveSelectedToCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (MusicEntries.Count == 0) return false;
        return true;
    }
    public void SongFilesListviewSaveSelectedToCommand_Execute(object obj)
    {
        if (obj is null) return;
        List<NodeFile> selectedList = [];

        
        Dispatcher.UIThread.Post(() =>
        {
            System.Collections.IList items = (System.Collections.IList)obj;

            var collection = items.Cast<NodeFile>();

            foreach (var item in collection)
            {
                selectedList.Add(item as NodeFile);
            }
        });

        List<string> fileUrisToAddList = [];

        foreach (var item in selectedList)
        {
            if (!string.IsNullOrEmpty(item.OriginalFileUri))
                fileUrisToAddList.Add(item.OriginalFileUri);
        }

        if (fileUrisToAddList.Count == 0)
            return;

        songFilesListviewSelectedQueueSongUriForPopup = fileUrisToAddList;

        IsSongFilesSelectedSaveToPopupVisible = true;
    }

    public IRelayCommand SongFilesListviewSaveSelectedToPopupCommand { get; }
    public bool SongFilesListviewSaveSelectedToPopupCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (MusicEntries.Count == 0) return false;
        return true;
    }
    public void SongFilesListviewSaveSelectedToPopupCommand_Execute(Playlist playlist)
    {
        if (playlist is null)
            return;

        if (string.IsNullOrEmpty(playlist.Name))
            return;
        if (songFilesListviewSelectedQueueSongUriForPopup.Count < 1)
            return;

        await _mpc.MpdPlaylistAdd(playlist.Name, songFilesListviewSelectedQueueSongUriForPopup);

        songFilesListviewSelectedQueueSongUriForPopup.Clear();

        IsSongFilesSelectedSaveToPopupVisible = false;
    }

    public IRelayCommand FilterMusicEntriesClearCommand { get; }
    public bool FilterMusicEntriesClearCommand_CanExecute()
    {
        if (string.IsNullOrEmpty(FilterMusicEntriesQuery))
            return false;
        return true;
    }
    public void FilterMusicEntriesClearCommand_Execute()
    {
        FilterMusicEntriesQuery = "";
    }


    // double clicked 
    public IRelayCommand SongFilesListviewLeftDoubleClickCommand { get; set; }
    public bool SongFilesListviewLeftDoubleClickCommand_CanExecute()
    {
        //if (IsWorking) return false;
        if (IsBusy) return false;
        if (MusicEntries.Count == 0) return false;
        return true;
    }
    public async void SongFilesListviewLeftDoubleClickCommand_ExecuteAsync(NodeFile song)
    {
        if (song is not null)
        {
            await _mpc.MpdAdd(song.OriginalFileUri);
        }
    }

    */

    // Play all
    public IRelayCommand FilesPlayCommand { get; set; }
    public static bool FilesPlayCommand_CanExecute()
    {
        return true;
    }
    public async void FilesPlayCommand_Execute(object obj)
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

                List<String> uriList = [];

                foreach (var song in list)
                {
                    uriList.Add(song.OriginalFileUri);
                }

                await _mpc.MpdMultiplePlay(uriList);

                // get album cover.
                await Task.Yield();
                await Task.Delay(200);
                UpdateCurrentSong();
            }
        }
    }

    // Queue all
    public IRelayCommand FilesAddToQueueCommand { get; set; }
    public static bool FilesAddToQueueCommand_CanExecute()
    {
        return true;
    }
    public async void FilesAddToQueueCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<NodeFile> list)
        {
            if (list.Count > 1)
            {
                List<String> uriList = [];

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
    public IRelayCommand FilesPageFilesAddToPlaylistCommand { get; set; }
    public static bool FilesPageFilesAddToPlaylistCommand_CanExecute()
    {
        return true;
    }
    public void FilesPageFilesAddToPlaylistCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is ObservableCollection<NodeFile> list)
        {
            List<String> uriList = [];

            foreach (var item in list)
            {
                uriList.Add(item.OriginalFileUri);
            }

            if (uriList.Count > 0)
            {
                FilesPageAddToPlaylistDialogShow?.Invoke(this, uriList);
            }
        }
    }

    public IRelayCommand FilesListviewPlayThisCommand { get; set; }
    public static bool FilesListviewPlayThisCommand_CanExecute()
    {
        return true;
    }
    public async void FilesListviewPlayThisCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is NodeFile song)
        {
            Dispatcher.UIThread.Post(() => {
                Queue.Clear();
                CurrentSong = null;
            });

            await _mpc.MpdSinglePlay(song.OriginalFileUri);

            // get album cover.
            await Task.Yield();
            await Task.Delay(200);
            UpdateCurrentSong();
        }
    }

    public IRelayCommand FilesListviewAddThisCommand { get; set; }
    public static bool FilesListviewAddThisCommand_CanExecute()
    {
        return true;
    }
    public async void FilesListviewAddThisCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is NodeFile song)
        {
            await _mpc.MpdAdd(song.OriginalFileUri);
        }
    }


    #endregion

    #region == Albums ==

    public IRelayCommand AlbumsItemInvokedCommand { get; set; }
    public static bool AlbumsItemInvokedCommand_CanExecute()
    {
        return true;
    }
    public async void AlbumsItemInvokedCommand_Execute(object obj)
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
                        if ((song.AlbumArtist.Equals(album.AlbumArtist)) || (song.Artist.Equals(album.AlbumArtist)))
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

    public IRelayCommand AlbumsCloseAlbumContentPanelCommand { get; set; }
    public static bool AlbumsCloseAlbumContentPanelCommand_CanExecute()
    {
        return true;
    }
    public void AlbumsCloseAlbumContentPanelCommand_Execute()
    {
        IsAlbumContentPanelVisible = false;
    }

    #endregion

    #region == Playlist ==

    /*
    public IRelayCommand ChangePlaylistCommand { get; set; }
    public bool ChangePlaylistCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        return true;
    }
    public async void ChangePlaylistCommand_ExecuteAsync()
    {
        if (_selectedPlaylist is null)
            return;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return;

        
        Dispatcher.UIThread.Post(() =>
        {
            Queue.Clear();
        });

        await _mpc.MpdChangePlaylist(_selectedPlaylist.Name);
    }

    public ICommand PlaylistListviewLeftDoubleClickCommand { get; set; }
    public bool PlaylistListviewLeftDoubleClickCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        return true;
    }
    public async void PlaylistListviewLeftDoubleClickCommand_ExecuteAsync(Playlist playlist)
    {
        if (_selectedPlaylist is null)
            return;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return;

        if (_selectedPlaylist != playlist)
            return;

        await _mpc.MpdLoadPlaylist(playlist.Name);
    }

    public IRelayCommand PlaylistListviewEnterKeyCommand { get; set; }
    public bool PlaylistListviewEnterKeyCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        return true;
    }
    public async void PlaylistListviewEnterKeyCommand_ExecuteAsync()
    {
        if (_selectedPlaylist is null)
            return;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return;

        await _mpc.MpdLoadPlaylist(_selectedPlaylist.Name);
    }

    public IRelayCommand PlaylistListviewLoadPlaylistCommand { get; set; }
    public bool PlaylistListviewLoadPlaylistCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        return true;
    }
    public async void PlaylistListviewLoadPlaylistCommand_ExecuteAsync()
    {
        if (_selectedPlaylist is null)
            return;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return;

        await _mpc.MpdLoadPlaylist(_selectedPlaylist.Name);
    }

    public IRelayCommand PlaylistListviewClearLoadPlaylistCommand { get; set; }
    public bool PlaylistListviewClearLoadPlaylistCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (IsWorking) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        return true;
    }
    public async void PlaylistListviewClearLoadPlaylistCommand_ExecuteAsync()
    {
        if (_selectedPlaylist is null)
            return;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return;

        
        Dispatcher.UIThread.Post(() =>
        {
            Queue.Clear();
        });

        await _mpc.MpdChangePlaylist(_selectedPlaylist.Name);
    }    

    */

    public IRelayCommand PlaylistLoadPlaylistCommand { get; }
    public static bool PlaylistLoadPlaylistCommand_CanExecute()
    {
        /*
        if (SelectedNodeMenu is null)
            return false;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem)
            return false;
        if (IsBusy) return false;
        if (IsWorking) return false;
        */
        return true;
    }
    public async void PlaylistLoadPlaylistCommand_Execute()
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (SelectedNodeMenu is null)
            return;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem)
            return;

        await _mpc.MpdLoadPlaylist(SelectedNodeMenu.Name);
    }

    public IRelayCommand PlaylistClearLoadPlaylistCommand { get; }
    public static bool PlaylistClearLoadPlaylistCommand_CanExecute()
    {
        /*
        if (SelectedNodeMenu is null)
            return false;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem)
            return false;
        */
        /*
        if (IsBusy) return false;
        if (IsWorking) return false;
        */
        return true;
    }
    public async void PlaylistClearLoadPlaylistCommand_Execute()
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

        await _mpc.MpdChangePlaylist(SelectedNodeMenu.Name);

        //await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume)); // nomore needed -> changed _mpc.MpdChangePlaylist

        // get album cover.
        //await _mpc.MpdIdleQueryCurrentSong();// nomore needed -> changed _mpc.MpdChangePlaylist
        await Task.Yield();
        await Task.Delay(200);
        UpdateCurrentSong();
    }

    public IRelayCommand PlaylistRenamePlaylistCommand { get; set; }
    public static bool PlaylistRenamePlaylistCommand_CanExecute()
    {
        return true;
    }
    public void PlaylistRenamePlaylistCommand_Execute(string playlist)
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
    public async void PlaylistRenamePlaylist_Execute(String oldPlaylistName, String newPlaylistName)
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

    public IRelayCommand PlaylistRemovePlaylistWithoutPromptCommand { get; set; }
    public static bool PlaylistRemovePlaylistWithoutPromptCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        */
        return true;
    }
    public async void PlaylistRemovePlaylistWithoutPromptCommand_Execute(string playlist)
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
        }
    }

    // Deletes song in a playlist.
    public IRelayCommand PlaylistListviewDeleteSelectedWithoutPromptCommand { get; set; }
    public static bool PlaylistListviewDeleteSelectedWithoutPromptCommand_CanExecute()
    {
        //if (SelectedPlaylistSong is null) return false;
        //if (IsBusy) return false;
        return true;
    }
    public async void PlaylistListviewDeleteSelectedWithoutPromptCommand_Execute(object obj)
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
    public IRelayCommand PlaylistClearPlaylistWithoutPromptCommand { get; set; }
    public static bool PlaylistClearPlaylistWithoutPromptCommand_CanExecute()
    {
        /*
        if (IsBusy) return false;
        if (_selectedPlaylist is null)
            return false;
        if (string.IsNullOrEmpty(_selectedPlaylist.Name))
            return false;
        */
        return true;
    }
    public async void PlaylistClearPlaylistWithoutPromptCommand_Execute(string playlist)
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
    public IRelayCommand PlaylistSongsListviewLeftDoubleClickCommand { get; set; }
    public bool PlaylistSongsListviewLeftDoubleClickCommand_CanExecute()
    {
        //if (IsWorking) return false;
        if (SelectedPlaylistSong is null) return false;
        if (IsBusy) return false;
        return true;
    }
    public async void PlaylistSongsListviewLeftDoubleClickCommand_ExecuteAsync(SongInfo song)
    {
        await _mpc.MpdAdd(song.File);
    }

    #endregion

    #region == Common Listview selected/collection songs command ==

    // used in context menu of Search result, Playlist etc.
    public IRelayCommand SongsListviewAddSelectedToQueueCommand { get; }
    public static bool SongsListviewAddSelectedToQueueCommand_CanExecute()
    {
        //if (IsBusy) return false;
        //if (IsWorking) return false;
        return true;
    }
    public async void SongsListviewAddSelectedToQueueCommand_Execute(object obj)
    {
        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        switch (items.Count)
        {
            case > 1:
            {
                var collection = items.Cast<SongInfo>();

                List<String> uriList = [];

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
    public IRelayCommand SongsPlayCommand { get; set; }
    public static bool SongsPlayCommand_CanExecute()
    {
        return true;
    }
    public async void SongsPlayCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is not ObservableCollection<SongInfo> list) return;
        if (list.Count <= 0) return;
        Dispatcher.UIThread.Post(() => {
            Queue.Clear();
            CurrentSong = null;        
        });

        List<String> uriList = [];

        foreach (var song in list)
        {
            uriList.Add(song.File);
        }

        await _mpc.MpdMultiplePlay(uriList);

        // get album cover.
        await Task.Yield();
        await Task.Delay(200);
        UpdateCurrentSong();
    }

    // Add to queue (Used in Albums, Artists, Search pages etc.)
    public IRelayCommand SongsAddToQueueCommand { get; set; }
    public static bool SongsAddToQueueCommand_CanExecute()
    {
        return true;
    }
    public async void SongsAddToQueueCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is not ObservableCollection<SongInfo> list) return;
        switch (list.Count)
        {
            case > 1:
            {
                List<String> uriList = [];

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


    // common used by many
    public async void AddToPlaylist_Execute(string playlistName, List<string> list)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;

        if (list.Count < 1)
            return;

        await _mpc.MpdPlaylistAdd(playlistName, list);
    }

    public IRelayCommand SongsListviewPlayThisCommand { get; set; }
    public static bool SongsListviewPlayThisCommand_CanExecute()
    {
        return true;
    }
    public async void SongsListviewPlayThisCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is SongInfo song)
        {
            Dispatcher.UIThread.Post(() => {
                Queue.Clear();
                CurrentSong = null;        
            });

            await _mpc.MpdSinglePlay(song.File);

            // get album cover.
            await Task.Yield();
            await Task.Delay(200);
            UpdateCurrentSong();
        }
    }

    public IRelayCommand SongsListviewAddThisCommand { get; set; }
    public static bool SongsListviewAddThisCommand_CanExecute()
    {
        return true;
    }
    public async void SongsListviewAddThisCommand_Execute(object obj)
    {
        if (obj is null) return;

        if (obj is SongInfo song)
        {
            await _mpc.MpdAdd(song.File);
        }
    }

    #endregion

    #region == Settings ==

    public IRelayCommand ShowSettingsCommand { get; }
    public bool ShowSettingsCommand_CanExecute()
    {
        if (IsConnecting) return false;
        return true;
    }
    public void ShowSettingsCommand_Execute()
    {
        if (IsConnecting) return;

        if (IsSettingsShow)
        {
            IsSettingsShow = false;
        }
        else
        {
            IsSettingsShow = true;
        }
    }

    public IRelayCommand SettingsOKCommand { get; }
    public static bool SettingsOKCommand_CanExecute()
    {
        return true;
    }
    public void SettingsOKCommand_Execute()
    {
        IsSettingsShow = false;
    }

    public IRelayCommand SaveProfileCommand { get; }
    public bool SaveProfileCommand_CanExecute()
    {
        //if (SelectedProfile is not null) return false;
        if (String.IsNullOrEmpty(Host)) return false;
        if (_port == 0) return false;
        return true;
    }
    public void SaveProfileCommand_Execute(object obj)
    {
        if (obj is null) return;
        //if (SelectedProfile is not null) return;
        if (String.IsNullOrEmpty(Host)) return;
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
            if (!String.IsNullOrEmpty(passwordBox.Password))
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
        NotifyPropertyChanged(nameof(IsCurrentProfileSet));

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

    public IRelayCommand UpdateProfileCommand { get; }
    public static bool UpdateProfileCommand_CanExecute()
    {
        //if (SelectedProfile is null) return false;
        return true;
    }
    public void UpdateProfileCommand_Execute(object obj)
    {
        if (obj is null) return;
        //if (SelectedProfile is null) return;
        if (String.IsNullOrEmpty(Host)) return;
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
            if (!String.IsNullOrEmpty(passwordBox.Password))
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

    public IRelayCommand ChangeConnectionProfileCommand { get; }
    public bool ChangeConnectionProfileCommand_CanExecute()
    {
        if (IsBusy) return false;
        if (string.IsNullOrWhiteSpace(Host)) return false;
        if (String.IsNullOrEmpty(Host)) return false;
        if (IsConnecting) return false;
        //if ((SelectedProfile is not null) && CurrentProfile is null) return false;
        return true;
    }
    public async void ChangeConnectionProfileCommand_Execute(object obj)
    {
        if (obj is null) return;
        if (String.IsNullOrEmpty(Host)) return;
        if (string.IsNullOrWhiteSpace(Host)) return;
        if (_port == 0) return;
        if (IsConnecting) return;
        if (IsBusy) return;
        if (IsWorking) return;

        //IsSwitchingProfile = true;

        if (IsConnected)
        {
            _mpc.MpdStop = true;
            _mpc.MpdDisconnect();
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
            NotifyPropertyChanged(nameof(Host));
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
            var addresses = await Dns.GetHostAddressesAsync(Host);
            if (addresses.Length > 0)
            {
                HostIpAddress = addresses[0];
            }
            else
            {
                //TODO: translate.
                //SetError(nameof(Host), "Error: Could not retrive IP Address from the hostname.");
                NotifyPropertyChanged(nameof(Host));
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
            NotifyPropertyChanged(nameof(Host));
            // TODO::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";
            return;
        }

        if (_port == 0)
        {
            //TODO: translate.
            //SetError(nameof(Port), "Error: Port must be specified.");
            NotifyPropertyChanged(nameof(Host));
            return;
        }
        /*
        // for Unbindable PasswordBox.
        var passwordBox = obj as PasswordBox;
        if (passwordBox is not null)
        {
            if (!String.IsNullOrEmpty(passwordBox.Password))
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
            IsSettingsShow = false;

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
                NotifyPropertyChanged(nameof(IsCurrentProfileSet));
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

                    NotifyPropertyChanged(nameof(IsCurrentProfileSet));
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
            _mpc.MpdDisconnect();
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
        NotifyPropertyChanged(nameof(Volume));


        Host = prof.Host;

        HostIpAddress = null;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(Host);
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

    #endregion

    #region == Dialogs ==

    public IRelayCommand ShowChangePasswordDialogCommand { get; }
    public bool ShowChangePasswordDialogCommand_CanExecute()
    {
        //if (SelectedProfile is null) return false;
        if (String.IsNullOrEmpty(Host)) return false;
        if (_port == 0) return false;
        return true;
    }
    public void ShowChangePasswordDialogCommand_Execute(object obj)
    {
        if (IsChangePasswordDialogShow)
        {
            IsChangePasswordDialogShow = false;
        }
        else
        {
            if (obj is null) return;
            /*
            // for Unbindable PasswordBox.
            var passwordBox = obj as PasswordBox;
            if (passwordBox is not null)
                passwordBox.Password = "";
            */
            IsChangePasswordDialogShow = true;
        }
    }

    public IRelayCommand ChangePasswordDialogOKCommand { get; }
    public bool ChangePasswordDialogOKCommand_CanExecute()
    {
        //if (SelectedProfile is null) return false;
        if (String.IsNullOrEmpty(Host)) return false;
        if (_port == 0) return false;
        return true;
    }
    public static void ChangePasswordDialogOKCommand_Execute(object obj)
    {
        if (obj is null) return;
        /*
        // MultipleCommandParameterConverter!
        var values = (object[])obj;
        
        if ((values[0] is PasswordBox) && (values[1] is PasswordBox))
        {
            if ((values[0] as PasswordBox)?.Password == _password)
            {
                if (SelectedProfile is not null)
                {
                    SelectedProfile.Password = (values[1] as PasswordBox)?.Password ?? ""; //allow empty string.

                    Password = SelectedProfile.Password;
                    NotifyPropertyChanged(nameof(IsPasswordSet));
                    NotifyPropertyChanged(nameof(IsNotPasswordSet));
                }

                if (values[0] is PasswordBox pss1)
                    pss1.Password = "";
                if (values[1] is PasswordBox pss2)
                    pss2.Password = "";

                // TODO:
                if (SelectedProfile == CurrentProfile)
                {
                    //_mpc.MpdPassword = SelectedProfile.Password;
                }

                SettingProfileEditMessage = MPDCtrlX.Properties.Resources.ChangePasswordDialog_PasswordUpdated;

            }
            else
            {
                ChangePasswordDialogMessage = MPDCtrlX.Properties.Resources.ChangePasswordDialog_CurrentPasswordMismatch;
                return;
            }

            IsChangePasswordDialogShow = false;
        }
        */
    }

    public IRelayCommand ChangePasswordDialogCancelCommand { get; }
    public static bool ChangePasswordDialogCancelCommand_CanExecute()
    {
        return true;
    }
    public void ChangePasswordDialogCancelCommand_Execute()
    {
        IsChangePasswordDialogShow = false;
    }

    #endregion

    #region == QueueListview header colums Show/Hide ==

    public IRelayCommand QueueColumnHeaderPositionShowHideCommand { get; }
    public static bool QueueColumnHeaderPositionShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderPositionShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderNowPlayingShowHideCommand { get; }
    public static bool QueueColumnHeaderNowPlayingShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderNowPlayingShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderTimeShowHideCommand { get; }
    public static bool QueueColumnHeaderTimeShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderTimeShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderArtistShowHideCommand { get; }
    public static bool QueueColumnHeaderArtistShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderArtistShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderAlbumShowHideCommand { get; }
    public static bool QueueColumnHeaderAlbumShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderAlbumShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderDiscShowHideCommand { get; }
    public static bool QueueColumnHeaderDiscShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderDiscShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderTrackShowHideCommand { get; }
    public static bool QueueColumnHeaderTrackShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderTrackShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderGenreShowHideCommand { get; }
    public static bool QueueColumnHeaderGenreShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderGenreShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    public IRelayCommand QueueColumnHeaderLastModifiedShowHideCommand { get; }
    public static bool QueueColumnHeaderLastModifiedShowHideCommand_CanExecute()
    {
        return true;
    }
    public void QueueColumnHeaderLastModifiedShowHideCommand_Execute()
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

        // Notify code behind to do some work around ...
        QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }


    #endregion

    #region == DebugWindow and AckWindow ==

    public IRelayCommand ClearDebugCommandTextCommand { get; }
    public static bool ClearDebugCommandTextCommand_CanExecute()
    {
        return true;
    }
    public void ClearDebugCommandTextCommand_Execute()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugCommandClear?.Invoke();
        });
    }

    public IRelayCommand ClearDebugIdleTextCommand { get; }
    public static bool ClearDebugIdleTextCommand_CanExecute()
    {
        return true;
    }
    public void ClearDebugIdleTextCommand_Execute()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugIdleClear?.Invoke();
        });
    }

    public IRelayCommand ShowDebugWindowCommand { get; }
    public static bool ShowDebugWindowCommand_CanExecute()
    {
        return true;
    }
    public void ShowDebugWindowCommand_Execute()
    {
        
        Dispatcher.UIThread.Post(() => {
            DebugWindowShowHide?.Invoke();
        });
    }

    public IRelayCommand ClearAckTextCommand { get; }
    public static bool ClearAckTextCommand_CanExecute()
    {
        return true;
    }
    public void ClearAckTextCommand_Execute()
    {
        
        Dispatcher.UIThread.Post(() => {
            AckWindowClear?.Invoke();
        });
    }

    public IRelayCommand ShowAckWindowCommand { get; }
    public static bool ShowAckWindowCommand_CanExecute()
    {
        return true;
    }
    public void ShowAckWindowCommand_Execute()
    {
        if (IsShowAckWindow)
            IsShowAckWindow = false;
        else
            IsShowAckWindow = true;
    }

    #endregion

    #region == Find ==

    public IRelayCommand ShowFindCommand { get; }
    public static bool ShowFindCommand_CanExecute()
    {
        return true;
    }
    public void ShowFindCommand_Execute()
    {
        if (SelectedNodeMenu is NodeMenuQueue)
        {
            QueueFindShowHideCommand_Execute();
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

    public IRelayCommand EscapeCommand { get; }
    public static bool EscapeCommand_CanExecute()
    {
        return true;
    }
    public void EscapeCommand_ExecuteAsync()
    {
        IsChangePasswordDialogShow = false;

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

    public IRelayCommand AlbumsCoverOverlayPanelCloseCommand { get; set; }
    public static bool AlbumsCoverOverlayPanelCloseCommand_CanExecute()
    {
        return true;
    }
    public void AlbumsCoverOverlayPanelCloseCommand_Execute()
    {
        IsAlbumArtPanelIsOpen = false;
    }

    // Jump menu from CurrentSong
    public IRelayCommand JumpToAlbumPageCommand { get; set; }
    public static bool JumpToAlbumPageCommand_CanExecute()
    {
        return true;
    }
    public void JumpToAlbumPageCommand_Execute()
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
    }

    public IRelayCommand JumpToArtistPageCommand { get; set; }
    public static bool JumpToArtistPageCommand_CanExecute()
    {
        return true;
    }
    public void JumpToArtistPageCommand_Execute()
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
    }

    #endregion

    public IRelayCommand TryConnectCommand { get; }
    public static bool TryConnectCommand_CanExecute()
    {
        return true;
    }
    public void TryConnectCommand_Execute()
    {
        Start(_host, _port);
    }


    #endregion
}
