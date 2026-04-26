using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using MPDCtrlX.Models;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.Views;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;

#pragma warning disable IDE0028

namespace MPDCtrlX.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public string AppVersion
    {
        get
        {
            if (!string.IsNullOrEmpty(field)) return field;
            
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = assembly.Version;
            field = $"{version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision}";

            return field;
        }
    } = string.Empty;

    #region == Layout ==

    #region == Window and loading flag ==

    public int WindowTop;
    public int WindowLeft;
    public double WindowHeight;
    public double WindowWidth;
    public WindowState WindowState = WindowState.Normal;

    public bool IsFullyLoaded
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    // TODO: no longer used...
    public double MainLeftPainActualWidth
    {
        get;
        set
        {
            if (value == field) return;

            field = value;

            OnPropertyChanged();
        }
    } = 241;

    // TODO: no longer used...
    public double MainLeftPainWidth
    {
        get;
        set
        {
            if (value == field) return;

            field = value;

            OnPropertyChanged();
        }
    } = 241;

    private bool _isNavigationViewMenuOpen = true;
    public bool IsNavigationViewMenuOpen
    {
        get => _isNavigationViewMenuOpen;
        set
        {
            if (_isNavigationViewMenuOpen == value)
                return;

            _isNavigationViewMenuOpen = value;

            OnPropertyChanged();

            // Needed this. Otherwise, there will be some weird visual bugs when expanding/collapsing the menu via hamburger menu button.
            Dispatcher.UIThread.Post(() =>
            {
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
            });
        }
    }

    #endregion

    #region == Queue column headers ==

    // Posiotion header
    public bool IsQueueColumnHeaderPositionVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderPositionWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 60;

    // NowPlaying header
    public bool IsQueueColumnHeaderNowPlayingVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderNowPlayingWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 32;

    // Title header (not user customizable)
    public double QueueColumnHeaderTitleWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;

    public bool IsQueueColumnHeaderTimeVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderTimeWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsQueueColumnHeaderArtistVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderArtistWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsQueueColumnHeaderAlbumVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderAlbumWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsQueueColumnHeaderDiscVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderDiscWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsQueueColumnHeaderTrackVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderTrackWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    // Genre header
    public bool IsQueueColumnHeaderGenreVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderGenreWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 100;

    public bool IsQueueColumnHeaderLastModifiedVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            QueueHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double QueueColumnHeaderLastModifiedWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;


    #endregion

    #region == Files column headers == 

    public double FilesColumnHeaderTitleWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value > 12)
                field = value;

            OnPropertyChanged();
        }
    } = 260;

    public double FilesColumnHeaderFilePathWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value > 12)
                field = value;

            OnPropertyChanged();
        }
    } = 250;

    public bool IsFilesColumnHeaderFilePathVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            FilesHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    #endregion

    #region == Search column headers ==

    public bool IsSearchColumnHeaderPositionVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderPositionWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 60;

    public double SearchColumnHeaderTitleWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;

    public bool IsSearchColumnHeaderTimeVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderTimeWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsSearchColumnHeaderArtistVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderArtistWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsSearchColumnHeaderAlbumVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderAlbumWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsSearchColumnHeaderDiscVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderDiscWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsSearchColumnHeaderTrackVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderTrackWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsSearchColumnHeaderGenreVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderGenreWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 100;

    public bool IsSearchColumnHeaderLastModifiedVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            SearchHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double SearchColumnHeaderLastModifiedWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;


    #endregion

    #region == PlaylistItem headers == 

    public bool IsPlaylistColumnHeaderPositionVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderPositionWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 60;

    public double PlaylistColumnHeaderTitleWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;

    public bool IsPlaylistColumnHeaderTimeVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderTimeWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsPlaylistColumnHeaderArtistVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderArtistWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsPlaylistColumnHeaderAlbumVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderAlbumWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 120;

    public bool IsPlaylistColumnHeaderDiscVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderDiscWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsPlaylistColumnHeaderTrackVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

    } = true;

    public double PlaylistColumnHeaderTrackWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 62;

    public bool IsPlaylistColumnHeaderGenreVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

    } = true;

    public double PlaylistColumnHeaderGenreWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 100;

    public bool IsPlaylistColumnHeaderLastModifiedVisible
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
            // Notify code behind to do some work around ...
            PlaylistHeaderVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    public double PlaylistColumnHeaderLastModifiedWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = 180;

    #endregion

    #endregion

    #region == Themes ==


    public ObservableCollection<Theme> Themes { get; set; } = [
            new Theme() { Id = 0, Name = "System", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_System, IconData="M7.5,2C5.71,3.15 4.5,5.18 4.5,7.5C4.5,9.82 5.71,11.85 7.53,13C4.46,13 2,10.54 2,7.5A5.5,5.5 0 0,1 7.5,2M19.07,3.5L20.5,4.93L4.93,20.5L3.5,19.07L19.07,3.5M12.89,5.93L11.41,5L9.97,6L10.39,4.3L9,3.24L10.75,3.12L11.33,1.47L12,3.1L13.73,3.13L12.38,4.26L12.89,5.93M9.59,9.54L8.43,8.81L7.31,9.59L7.65,8.27L6.56,7.44L7.92,7.35L8.37,6.06L8.88,7.33L10.24,7.36L9.19,8.23L9.59,9.54M19,13.5A5.5,5.5 0 0,1 13.5,19C12.28,19 11.15,18.6 10.24,17.93L17.93,10.24C18.6,11.15 19,12.28 19,13.5M14.6,20.08L17.37,18.93L17.13,22.28L14.6,20.08M18.93,17.38L20.08,14.61L22.28,17.15L18.93,17.38M20.08,12.42L18.94,9.64L22.28,9.88L20.08,12.42M9.63,18.93L12.4,20.08L9.87,22.27L9.63,18.93Z"},
            new Theme() { Id = 1, Name = "Dark", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_Dark, IconData="M17.75,4.09L15.22,6.03L16.13,9.09L13.5,7.28L10.87,9.09L11.78,6.03L9.25,4.09L12.44,4L13.5,1L14.56,4L17.75,4.09M21.25,11L19.61,12.25L20.2,14.23L18.5,13.06L16.8,14.23L17.39,12.25L15.75,11L17.81,10.95L18.5,9L19.19,10.95L21.25,11M18.97,15.95C19.8,15.87 20.69,17.05 20.16,17.8C19.84,18.25 19.5,18.67 19.08,19.07C15.17,23 8.84,23 4.94,19.07C1.03,15.17 1.03,8.83 4.94,4.93C5.34,4.53 5.76,4.17 6.21,3.85C6.96,3.32 8.14,4.21 8.06,5.04C7.79,7.9 8.75,10.87 10.95,13.06C13.14,15.26 16.1,16.22 18.97,15.95M17.33,17.97C14.5,17.81 11.7,16.64 9.53,14.5C7.36,12.31 6.2,9.5 6.04,6.68C3.23,9.82 3.34,14.64 6.35,17.66C9.37,20.67 14.19,20.78 17.33,17.97Z"},
            new Theme() { Id = 2, Name = "Light", Label = MPDCtrlX.Properties.Resources.Settings_Opts_Themes_Light, IconData="M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,2L14.39,5.42C13.65,5.15 12.84,5 12,5C11.16,5 10.35,5.15 9.61,5.42L12,2M3.34,7L7.5,6.65C6.9,7.16 6.36,7.78 5.94,8.5C5.5,9.24 5.25,10 5.11,10.79L3.34,7M3.36,17L5.12,13.23C5.26,14 5.53,14.78 5.95,15.5C6.37,16.24 6.91,16.86 7.5,17.37L3.36,17M20.65,7L18.88,10.79C18.74,10 18.47,9.23 18.05,8.5C17.63,7.78 17.1,7.15 16.5,6.64L20.65,7M20.64,17L16.5,17.36C17.09,16.85 17.62,16.22 18.04,15.5C18.46,14.77 18.73,14 18.87,13.21L20.64,17M12,22L9.59,18.56C10.33,18.83 11.14,19 12,19C12.82,19 13.63,18.83 14.37,18.56L12,22Z"}
        ];

    private Theme _currentTheme;
    public Theme CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme == value) return;

            _currentTheme = value;
            OnPropertyChanged();

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

    public bool IsConnected { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));
            OnPropertyChanged(nameof(IsNotConnecting));

            IsConnecting = !field;

            if (!field)
            {
                IsNotConnectingNorConnected = true;
            }
            if (field)
            {
                IsConnectButtonEnabled = true;
            }
        } }

    public bool IsConnecting { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotConnecting));
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));

            OnPropertyChanged(nameof(IsProfileSwitchOK));
            if (field)
            {
                IsConnectButtonEnabled = false;
            }
        } }

    public bool IsNotConnectingNorConnected { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShortStatusWIthMpdVersion));

            if (field)
            {
                IsConnectButtonEnabled = true;
            }
        } } = true;

    public bool IsConnectButtonEnabled { get; set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        } } = true;

    public bool IsNotConnecting => !IsConnecting;

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
    public bool IsConnectionSettingShow { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        } }

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

    public bool IsAlbumArtVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsAlbumArtPanelIsOpen
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
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

    public bool IsBusy
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            Dispatcher.UIThread.Post(() => { OnPropertyChanged(); });
            OnPropertyChanged(nameof(IsProfileSwitchOK));
            //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            //Dispatcher.UIThread.Post(async () => { CommandManager.InvalidateRequerySuggested()});
        }
    }

    public bool IsWorking
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            Dispatcher.UIThread.Post(() => {
                OnPropertyChanged();
                WorkingStateChanged?.Invoke(this, value);
            }, DispatcherPriority.Input);

            OnPropertyChanged(nameof(IsProfileSwitchOK));
        }
    }

    public bool IsShowInfoWindow
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!field)
            {
                InfoBarInfoTitle = string.Empty;
                InfoBarInfoMessage = string.Empty;
            }

            OnPropertyChanged();
        }
    }

    public bool IsShowAckWindow
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!field)
            {
                InfoBarAckTitle = string.Empty;
                InfoBarAckMessage = string.Empty;
            }

            OnPropertyChanged();
        }
    }

    public bool IsShowErrWindow
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (!field)
            {
                InfoBarErrTitle = string.Empty;
                InfoBarErrMessage = string.Empty;
            }

            OnPropertyChanged();
        }
    }

    public bool IsShowDebugWindow

    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();

            if (field)
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

    public bool IsEnableDebugWindow { get; set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        } }

    #endregion

    #region == CurrentSong, Playback controls, AlbumArt ==  

    public SongInfoEx? CurrentSong { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
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
        } }

    public string CurrentSongTitle => CurrentSong is not null ? CurrentSong.Title : string.Empty;

    public string CurrentSongArtist
    {
        get
        {
            if (CurrentSong is not null)
            {
                if (!string.IsNullOrEmpty(CurrentSong.Artist))
                    return CurrentSong.Artist.Trim();
                else
                    return string.Empty;
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
            if (CurrentSong is not null)
            {
                if (!string.IsNullOrEmpty(CurrentSong.Album))
                    return CurrentSong.Album.Trim();
                else
                    return string.Empty;
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
            if (CurrentSong is not null)
            {
                if (!string.IsNullOrEmpty(CurrentSong.Artist))
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
            if (CurrentSong is not null)
            {
                if (!string.IsNullOrEmpty(CurrentSong.Album))
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

    public bool IsCurrentSongNotNull { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        } }

    public string CurrentSongStringForWindowTitle
    {
        get
        {
            if (CurrentSong is not null)
            {
                string s = string.Empty;

                if (!string.IsNullOrEmpty(CurrentSong.Title))
                {
                    s = CurrentSong.Title.Trim();
                }

                if (!string.IsNullOrEmpty(CurrentSong.Artist))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        s += " by ";
                    }
                    s += $"{CurrentSong.Artist.Trim()}"; 
                }

                if (!string.IsNullOrEmpty(CurrentSong.Album))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        s += " from ";
                    }
                    s += $"{CurrentSong.Album.Trim()}";
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
    public string PlayButton { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        } } = _pathPlayButton;

    private double _volume = 20;
    public double Volume
    {
        get => _volume;
        set
        {
            if (_volume == value) return;
            _volume = value;
            OnPropertyChanged();

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
        await _mpc.MpdSetVolume(Convert.ToInt32(_volume));
    }

    private bool _repeat;
    public bool Repeat
    {
        get => _repeat;
        set
        {
            _repeat = value;
            OnPropertyChanged();

            if (_mpc.MpdStatus.MpdRepeat != value)
            {
                _ = SetRpeat();
            }
        }
    }

    private bool _random;
    public bool Random
    {
        get => _random;
        set
        {
            _random = value;
            OnPropertyChanged();

            if (_mpc.MpdStatus.MpdRandom != value)
            {
                _ = SetRandom();
            }
        }
    }

    private bool _consume;
    public bool Consume
    {
        get => _consume;
        set
        {
            _consume = value;
            OnPropertyChanged();

            if (_mpc.MpdStatus.MpdConsume != value)
            {
                _ = SetConsume();
            }
        }
    }

    private bool _single;
    public bool Single
    {
        get => _single;
        set
        {
            _single = value;
            OnPropertyChanged();

            if (_mpc.MpdStatus.MpdSingle != value)
            {
                _ = SetSingle();
            }
        }
    }

    public int Time { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeFormatted));
        } }

    public string TimeFormatted
    {
        get
        {
            int min;

            var sec = Time / _elapsedTimeMultiplier;

            min = sec / 60;
            var s = sec % 60;
            var hour = min / 60;
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
            return $"{hour}:{min:00}:{s:00}";
        }
    }

    private readonly int _elapsedTimeMultiplier = 1;// or 10
    private int _elapsed ;
    public int Elapsed
    {
        get => _elapsed;
        set
        {
            if ((value < Time) && _elapsed != value)
            {
                _elapsed = value;
                Dispatcher.UIThread.Post(() =>
                {
                    OnPropertyChanged();
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
            var sec = _elapsed / _elapsedTimeMultiplier;

            var min = sec / 60;
            var s = sec % 60;
            var hour = min / 60;
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
            return $"{hour}:{min:00}:{s:00}";
        }
    }

    private System.Timers.Timer? _elapsedDelayTimer = null;
    private void DoChangeElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_elapsed < Time)
        {
            _ = SetSeek();
        }
    }

    #endregion

    #region == AlbumArt == 

    public AlbumImage? AlbumCover
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;

            if (field is null)
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

            OnPropertyChanged();
        }
    }

    private readonly Bitmap? _albumArtBitmapSourceDefault = null;

    public Bitmap? AlbumArtBitmapSource
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region == NavigationView/TreeView Menu (Queue, Files, Search, Playlists, Playlist) ==

    #region == MenuTree ==

    private readonly MenuTreeBuilder _mainMenuItems = new("");
    public ObservableCollection<NodeTree> MainMenuItems
    {
        get => _mainMenuItems.Children;
        set
        {
            _mainMenuItems.Children = value;
            OnPropertyChanged();
        }
    }

    public NodeTree? SelectedNodeMenu
    {
        get;
        set
        {
            if (field == value)
                return;

            if (value is null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    CurrentPage = null; // needed this.

                    SelectedPlaylistName = string.Empty;
                    RenamedSelectPendingPlaylistName = string.Empty;
                    PlaylistSongs.Clear();
                });

                field = value;
                OnPropertyChanged();
                return;
            }

            if (value is NodeMenuQueue)
            {
                /*
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();
                    await Task.Delay(10);
                    CurrentPage = App.GetService<QueuePage>();
                    IsWorking = false;
                });
                */
                CurrentPage = App.GetService<QueuePage>();
            }
            else if (value is NodeMenuSearch)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();
                    await Task.Delay(10);
                    CurrentPage = App.GetService<SearchPage>();
                    IsWorking = false;
                });
                //CurrentPage = App.GetService<SearchPage>();
            }
            else if (value is NodeMenuLibrary)
            {
                // Do nothing.
                // Do not remove.
            }
            else if (value is NodeMenuArtist)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();
                    await Task.Delay(10);
                    if ((Artists.Count > 0) && (SelectedAlbumArtist is null))
                    {
                        SelectedAlbumArtist = Artists[0];
                    }
                    CurrentPage = App.GetService<ArtistPage>();

                    IsWorking = false;
                });
            }
            else if (value is NodeMenuAlbum nmb)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();
                    await Task.Delay(10);

                    CurrentPage = App.GetService<AlbumPage>();

                    await Task.Delay(1);
                    IsWorking = false;
                });
            }
            else if (value is NodeMenuFiles nml)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    IsWorking = true;
                    await Task.Yield();
                    await Task.Delay(1);

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
                // Do not remove.
            }
            else if (value is NodeMenuPlaylistItem nmpli)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    //CurrentPage = null; // Just for the animation of page transition...

                    CurrentPage = App.GetService<PlaylistItemPage>();

                    SelectedPlaylistSong = null;
                    PlaylistSongs = nmpli.PlaylistSongs;
                    SelectedPlaylistName = nmpli.Name;

                    if ((nmpli.PlaylistSongs.Count == 0) || nmpli.IsUpdateRequied)
                    {
                        GetPlaylistSongs(nmpli);
                    }
                });
            }
            else if (value is NodeMenu)
            {
                if (value.Name != "root")
                    throw new NotImplementedException();
            }
            else
            {
                Debug.WriteLine($"SelectedNodeMenu is of type {value.GetType().Name}, which is not handled.");
                throw new NotImplementedException();
            }

            field = value;
            OnPropertyChanged();
        }
    } = new NodeMenu("root");

    public UserControl? CurrentPage
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            this.OnPropertyChanged();
        }
    }

    public string SelectedPlaylistName
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string RenamedSelectPendingPlaylistName
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    #endregion

    #region == Queue ==  

    public ObservableCollection<SongInfoEx> Queue
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(QueuePageSubTitleSongCount));
        }
    } = [];

    public SongInfoEx? SelectedQueueSong
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsQueueFindVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            QueueForFilter.Clear();
            FilterQueueQuery = "";
            OnPropertyChanged();
        }
    }

    private bool FilterSongInfoEx(SongInfoEx song)
    {
        return song.Title.Contains(FilterQueueQuery, StringComparison.CurrentCultureIgnoreCase);// InvariantCultureIgnoreCase
    }

    public ObservableCollection<SongInfoEx> QueueForFilter
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public SearchTags SelectedQueueFilterTags
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = SearchTags.Title;

    public string FilterQueueQuery
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();

            if (field == string.Empty)
            {
                return;
            }

            var filtered = Queue.Where(FilterSongInfoEx); // Queue.Where(song => FilterSongInfoEx(song));

            QueueForFilter = new ObservableCollection<SongInfoEx>(filtered);
        }
    } = string.Empty;

    public SongInfoEx? SelectedQueueFilterSong
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string QueuePageSubTitleSongCount
    {
        get 
        {
            field = string.Format(MPDCtrlX.Properties.Resources.QueuePage_SubTitle_SongCount, Queue.Count);
            return field;
        }
    } = string.Empty;

    #endregion

    #region == Files ==

    private readonly DirectoryTreeBuilder _musicDirectories = new("");
    public ObservableCollection<NodeTree> MusicDirectories
    {
        get => _musicDirectories.Children;
        set
        {
            _musicDirectories.Children = value;
            OnPropertyChanged();
        }
    }

    private NodeDirectory _selectedNodeDirectory = new(".", new Uri(@"file:///./"));
    public NodeDirectory SelectedNodeDirectory
    {
        get => _selectedNodeDirectory;
        set
        {
            if (_selectedNodeDirectory == value)
                return;

            _selectedNodeDirectory = value;
            OnPropertyChanged();

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
                    var filtered = MusicEntries.Where(song => song.Name.Contains(FilterMusicEntriesQuery, StringComparison.InvariantCultureIgnoreCase));
                    _musicEntriesFiltered = new ObservableCollection<NodeFile>(filtered);
                }
                else
                {
                    _musicEntriesFiltered = new ObservableCollection<NodeFile>(MusicEntries);
                }
            }
            else
            {
                FilterFiles();
            }

            OnPropertyChanged(nameof(MusicEntriesFiltered));
        }
    }

    public ObservableCollection<NodeFile> MusicEntries
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilesPageSubTitleFileCount));
        }
    } = [];

    private ObservableCollection<NodeFile> _musicEntriesFiltered = [];
    public ObservableCollection<NodeFile> MusicEntriesFiltered
    {
        get => _musicEntriesFiltered;
        set
        {
            if (_musicEntriesFiltered == value)
                return;

            _musicEntriesFiltered = value;
            OnPropertyChanged();
        }
    }

    private void FilterFiles()
    {
        _musicEntriesFiltered.Clear();

        foreach (var entry in MusicEntries)
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

    public string FilterMusicEntriesQuery
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();

            if (_selectedNodeDirectory is null)
                return;

            if (_selectedNodeDirectory.DireUri.LocalPath == "/")
            {
                if (FilterMusicEntriesQuery != "")
                {
                    var filtered = MusicEntries.Where(song =>
                        song.Name.Contains(FilterMusicEntriesQuery, StringComparison.InvariantCultureIgnoreCase));
                    MusicEntriesFiltered = new ObservableCollection<NodeFile>(filtered);
                }
                else
                {
                    MusicEntriesFiltered = new ObservableCollection<NodeFile>(MusicEntries);
                }
            }
            else
            {
                FilterFiles();
                OnPropertyChanged(nameof(MusicEntriesFiltered));
            }
        }
    } = string.Empty;

    public string FilesPageSubTitleFileCount
    {
        get
        {
            field = string.Format(MPDCtrlX.Properties.Resources.FilesPage_SubTitle_FileCount, MusicEntries.Count);
            return field;
        }
    } = string.Empty;

    public bool IsFilesFindVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    #endregion

    #region == Artists ==

    public ObservableCollection<AlbumArtist> Artists
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            SelectedAlbumArtist = null;
            SelectedArtistAlbums = null;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ArtistPageSubTitleArtistCount));
        }
    } = [];

    public string ArtistPageSubTitleArtistCount
    {
        get
        {
            field = string.Format(MPDCtrlX.Properties.Resources.ArtistPage_SubTitle_ArtistCount, Artists.Count);
            return field;
        }
    } = string.Empty;

    public AlbumArtist? SelectedAlbumArtist
    {
        get => field;
        set
        {
            if (field == value)
            {
                return;
            }
            
            field = value;
            OnPropertyChanged();

            if (field is null)
            {
                SelectedArtistAlbums = null;
                return;
            }

            SelectedArtistAlbums = field?.Albums;
            //OnPropertyChanged(nameof(ArtistPageSubTitleArtistAlbumCount));
            if (SelectedArtistAlbums is null)
            {
                return;
            }
            /*
            // Test
            Task.Run(async () =>
            {
                IsWorking = true;
                await Task.Yield();
                await Task.Delay(20);
                GetArtistSongs(SelectedArtistAlbums);
                GetAlbumPictures(SelectedArtistAlbums);

                IsWorking = false;
                await Task.Yield();
            }, _cts.Token);
            */
            Dispatcher.UIThread.Post(async () =>  // Test
            {
                IsWorking = true;
                await Task.Yield();
                await GetArtistSongsAsync(SelectedAlbumArtist); // await or not?
                await Task.Delay(20);
                await GetAlbumPicturesAsync(SelectedArtistAlbums);// await or not?
                IsWorking = false;
            }, DispatcherPriority.Default);
        }
    }

    public ObservableCollection<Album>? SelectedArtistAlbums
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (field is not null)
            {
                if (IsAlbumSortWithoutThePrefix)
                {
                    // Sort
                    var ci = CultureInfo.CurrentCulture;
                    var comp = StringComparer.Create(ci, true);
                    field = new ObservableCollection<Album>(field.OrderBy(x => x.NameSort, comp)); // COPY. // Sort without prefix like "The" or "A".
                }
            }

            OnPropertyChanged();
        }
    } = [];

    // Filter Artists
    public ObservableCollection<AlbumArtist> ArtistsForFilter
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string FilterArtistQuery
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();

            if (field == "")
            {
                return;
            }

            var filtered = Artists.Where(FilterArtists);

            ArtistsForFilter = new ObservableCollection<AlbumArtist>(filtered);

        }
    } = "";

    private bool FilterArtists(AlbumArtist artist)
    {
        return artist.Name.Contains(FilterArtistQuery, StringComparison.CurrentCultureIgnoreCase);// InvariantCultureIgnoreCase
    }

    public bool IsArtistFindVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            ArtistsForFilter.Clear();

            FilterArtistQuery = "";

            OnPropertyChanged();
        }
    }

    public AlbumArtist? SelectedFilterAlbumArtist
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region == Albums ==

    public ObservableCollection<AlbumEx> Albums
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            SelectedAlbum = null;

            OnPropertyChanged();
            OnPropertyChanged(nameof(AlbumPageSubTitleAlbumCount));
        }
    } = [];

    public bool IsAlbumContentPanelVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = false;

    public AlbumEx? SelectedAlbum
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedAlbumSongs));

            //OpenAlbumPane?.Invoke(this, EventArgs.Empty);

            /*
            if (_selectedAlbum is not null)
            {
                Dispatcher.UIThread.Post(async () =>  // Test
                {
                    IsWorking = true;
                    //await Task.Yield();
                    //await Task.Delay(20);
                    await AlbumsItemInvoked(_selectedAlbum);
                    IsWorking = false;

                    await Task.Yield();
                    await Task.Delay(20);
                    IsAlbumContentPanelVisible = true;

                }, DispatcherPriority.Default);
            }
            */
        }
    } = new();

    public ObservableCollection<SongInfo> SelectedAlbumSongs
    {
        get
        {
            if (SelectedAlbum is not null)
            {
                return SelectedAlbum.Songs;
            }

            return field;
        }
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string AlbumPageSubTitleAlbumCount
    {
        get
        {
            field = string.Format(MPDCtrlX.Properties.Resources.AlbumPage_SubTitle_AlbumCount, Albums.Count);
            return field;
        }
    } = "";

    public IEnumerable<object>? VisibleViewportItemsAlbumEx
    {
        get;
        set
        {
            field = value;

            //OnPropertyChanged(nameof(VisibleViewportItemsAlbumEx));

            if (VisibleViewportItemsAlbumEx is null)
            {
                return;
            }

            try
            {
                _ = Task.Run(() => GetAlbumPicturesAsync(VisibleViewportItemsAlbumEx), _cts.Token);
                //GetAlbumPictures(VisibleViewportItemsAlbumEx);
            }
            catch (Exception ex)
            {
                _ = ex;
                Debug.WriteLine($"Exception @VisibleViewportItemsAlbumEx {ex}");
            }
        }
    }

    // Filter Albums
    public ObservableCollection<AlbumEx> AlbumsForFilter
    {   
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string FilterAlbumQuery
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();

            if (field == "")
            {
                return;
            }

            var filtered = Albums.Where(album => FilterAlbums(album));

            AlbumsForFilter = new ObservableCollection<AlbumEx>(filtered);
        }
    } = "";

    private bool FilterAlbums(AlbumEx album)
    {
        return album.Name.Contains(FilterAlbumQuery, StringComparison.CurrentCultureIgnoreCase);// InvariantCultureIgnoreCase
    }

    public bool IsAlbumFindVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            AlbumsForFilter.Clear();

            FilterAlbumQuery = "";

            OnPropertyChanged();
        }
    }

    public AlbumEx? SelectedFilterAlbum
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }


    #endregion

    #region == Search ==

    public ObservableCollection<SongInfo>? SearchResult
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SearchPageSubTitleResultCount));
        }
    } = [];

    // Search Tags

    public ObservableCollection<Models.SearchOption> SearchTagList { get; } = [
        new Models.SearchOption(SearchTags.Title, MPDCtrlX.Properties.Resources.ListviewColumnHeader_Title),
        new Models.SearchOption(SearchTags.Artist, MPDCtrlX.Properties.Resources.ListviewColumnHeader_Artist),
        new Models.SearchOption(SearchTags.Album, MPDCtrlX.Properties.Resources.ListviewColumnHeader_Album),
        new Models.SearchOption(SearchTags.Genre, MPDCtrlX.Properties.Resources.ListviewColumnHeader_Genre),
        new Models.SearchOption(SearchTags.Any, MPDCtrlX.Properties.Resources.SearchOption_Any)
    ];

    public Models.SearchOption SelectedSearchTag
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new(SearchTags.Title, MPDCtrlX.Properties.Resources.ListviewColumnHeader_Title);

    // Search Shiki (contain/==)

    public ObservableCollection<Models.SearchWith> SearchShikiList { get; } = [
    new Models.SearchWith(SearchShiki.Contains, MPDCtrlX.Properties.Resources.Search_Shiki_Contains),
        new Models.SearchWith(SearchShiki.Equals, MPDCtrlX.Properties.Resources.Search_Shiki_Equals)
];

    public Models.SearchWith SelectedSearchShiki
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new(SearchShiki.Contains, "Contains");

    // 
    public string SearchQuery
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            //SearchExecCommand.NotifyCanExecuteChanged();
        }
    } = "";

    public string SearchPageSubTitleResultCount
    {
        get
        {
            field = string.Format(MPDCtrlX.Properties.Resources.SearchPage_SubTitle_ResultCount, SearchResult?.Count);
            return field;
        }
    } = "";

    #endregion

    #region == Playlists ==  

    public ObservableCollection<Playlist> Playlists
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

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

    public ObservableCollection<SongInfo> PlaylistSongs
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));
        }
    } = [];

    public SongInfo? SelectedPlaylistSong
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string PlaylistPageSubTitleSongCount
    {
        get
        {
            field = string.Format(MPDCtrlX.Properties.Resources.PlaylistPage_SubTitle_SongCount, PlaylistSongs.Count);
            return field;
        }

        private set;
    } = "";

    #endregion

    #region == Settings ==

    public static string AlbumCacheFolderPath => App.AppDataCacheFolder;

    public string AlbumCacheFolderSizeFormatted
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    #endregion

    #endregion

    #region == Debug ==

    public string DebugCommandText
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;


    public string DebugIdleText
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    #endregion

    #region == Options ==

    public bool IsUpdateOnStartup
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsAutoScrollToNowPlaying
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = false;

    public bool IsArtistSortWithoutThePrefix
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsAlbumSortWithoutThePrefix
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsSaveLog
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDownloadAlbumArt
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = true;

    public bool IsDownloadAlbumArtEmbeddedUsingReadPicture
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = true;

    #endregion

    #region == Profile settings ==

    public ObservableCollection<Profile> Profiles { get; } = [];

    public Profile? CurrentProfile
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();

            SelectedProfile = field;

            if (field is null) return;
            _volume = field.Volume;
            OnPropertyChanged(nameof(Volume));

            Host = field.Host;
            Port = field.Port.ToString();
            _password = field.Password;
            OnPropertyChanged(nameof(Password));
        }
    }

    public Profile? SelectedProfile
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SetIsDefault
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

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

    private string _host = "";
    public string Host
    {
        get => _host;
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

            OnPropertyChanged();
        }
    }

    public IPAddress? HostIpAddress { get; set
        {
            //if (_hostIpAddress is null) return;
            //if (_hostIpAddress.Equals(value))  return;

            field = value;
            OnPropertyChanged();
        } }

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

            OnPropertyChanged();
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
            OnPropertyChanged();
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
                Debug.WriteLine("Encrypt fail.");
                return s;
            }
        }
        //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        else
        {
            try
            {
                string encryptionKey = "withas";
                byte[] sBytes = Encoding.Unicode.GetBytes(s);
                using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
                {
                    // Obsolete and deprecated in .NET 10.
                    //Rfc2898DeriveBytes pdb = new(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
                    //encryptor.Key = pdb.GetBytes(32);
                    //encryptor.IV = pdb.GetBytes(16);
                    // New since .NET 10.
                    encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1, 32);
                    encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1, 16);

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
            catch
            {
                Debug.WriteLine($"Encrypt fail.");
                return s;
            }
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
            try
            {
                string encryptionKey = "withas";
                s = s.Replace(" ", "+");
                byte[] sBytes = Convert.FromBase64String(s);
                using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
                {
                    // Obsolete and deprecated in .NET 10.
                    //Rfc2898DeriveBytes pdb = new(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1);
                    //encryptor.Key = pdb.GetBytes(32);
                    //encryptor.IV = pdb.GetBytes(16);
                    // New since .NET 10.
                    encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1, 32);
                    encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, entropy, 10000, HashAlgorithmName.SHA1, 16);

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
            catch
            {
                Debug.WriteLine($"Decrypt fail.");
                return "";
            }
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

    public bool IsRememberAsProfile
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    #endregion

    #region == Audio Outputs ==

    public ObservableCollection<AudioOutput> AudioOutputs
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public AudioOutput? SelectedAudioOutput
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region == Status Messages == 

    public string StatusBarMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string ConnectionStatusMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string MpdStatusMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();

            IsMpdStatusMessageContainsText = field != "";
            OnPropertyChanged(nameof(IsMpdStatusMessageContainsText));
        }
    } = "";

    public bool IsMpdStatusMessageContainsText { get; private set; }

    public string InfoBarInfoTitle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string InfoBarInfoMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string InfoBarAckTitle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string InfoBarAckMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string InfoBarErrTitle
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";

    public string InfoBarErrMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "";


    private static readonly string _pathDefaultNoneButton = "";
    private static readonly string _pathDisconnectedButton = "M4,1C2.89,1 2,1.89 2,3V7C2,8.11 2.89,9 4,9H1V11H13V9H10C11.11,9 12,8.11 12,7V3C12,1.89 11.11,1 10,1H4M4,3H10V7H4V3M14,13C12.89,13 12,13.89 12,15V19C12,20.11 12.89,21 14,21H11V23H23V21H20C21.11,21 22,20.11 22,19V15C22,13.89 21.11,13 20,13H14M3.88,13.46L2.46,14.88L4.59,17L2.46,19.12L3.88,20.54L6,18.41L8.12,20.54L9.54,19.12L7.41,17L9.54,14.88L8.12,13.46L6,15.59L3.88,13.46M14,15H20V19H14V15Z";

    private static readonly string _pathConnectingButton = "M11 14H9C9 9.03 13.03 5 18 5V7C14.13 7 11 10.13 11 14M18 11V9C15.24 9 13 11.24 13 14H15C15 12.34 16.34 11 18 11M7 4C7 2.89 6.11 2 5 2S3 2.89 3 4 3.89 6 5 6 7 5.11 7 4M11.45 4.5H9.45C9.21 5.92 8 7 6.5 7H3.5C2.67 7 2 7.67 2 8.5V11H8V8.74C9.86 8.15 11.25 6.5 11.45 4.5M19 17C20.11 17 21 16.11 21 15S20.11 13 19 13 17 13.89 17 15 17.89 17 19 17M20.5 18H17.5C16 18 14.79 16.92 14.55 15.5H12.55C12.75 17.5 14.14 19.15 16 19.74V22H22V19.5C22 18.67 21.33 18 20.5 18Z";
    private static readonly string _pathConnectedButton = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";
    //private static string _pathConnectedButton = "";
    //private static string _pathDisconnectedButton = "";
    private static readonly string _pathNewConnectionButton = "M20,4C21.11,4 22,4.89 22,6V18C22,19.11 21.11,20 20,20H4C2.89,20 2,19.11 2,18V6C2,4.89 2.89,4 4,4H20M8.5,15V9H7.25V12.5L4.75,9H3.5V15H4.75V11.5L7.3,15H8.5M13.5,10.26V9H9.5V15H13.5V13.75H11V12.64H13.5V11.38H11V10.26H13.5M20.5,14V9H19.25V13.5H18.13V10H16.88V13.5H15.75V9H14.5V14A1,1 0 0,0 15.5,15H19.5A1,1 0 0,0 20.5,14Z";
    private static readonly string _pathErrorInfoButton = "M23,12L20.56,14.78L20.9,18.46L17.29,19.28L15.4,22.46L12,21L8.6,22.47L6.71,19.29L3.1,18.47L3.44,14.78L1,12L3.44,9.21L3.1,5.53L6.71,4.72L8.6,1.54L12,3L15.4,1.54L17.29,4.72L20.9,5.54L20.56,9.22L23,12M20.33,12L18.5,9.89L18.74,7.1L16,6.5L14.58,4.07L12,5.18L9.42,4.07L8,6.5L5.26,7.09L5.5,9.88L3.67,12L5.5,14.1L5.26,16.9L8,17.5L9.42,19.93L12,18.81L14.58,19.92L16,17.5L18.74,16.89L18.5,14.1L20.33,12M11,15H13V17H11V15M11,7H13V13H11V7";

    public string StatusButton { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        } } = _pathDefaultNoneButton;

    private static readonly string _pathMpdOkButton = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";

    private static readonly string _pathMpdAckErrorButton = "M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z";

    public string MpdStatusButton { get; set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        } } = _pathMpdOkButton;

    public bool IsUpdatingMpdDb
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
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
            OnPropertyChanged();
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
                return IsNotConnectingNorConnected ? "Not connected" : "Not connected";
            }
        }
    }

    #endregion

    #region == Popups ==

    //private List<string> queueListviewSelectedQueueSongIdsForPopup = [];
    //private List<string> searchResultListviewSelectedQueueSongUriForPopup = [];
    //private List<string> songFilesListviewSelectedQueueSongUriForPopup = [];

    public bool IsConfirmClearQueuePopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelectedSaveToPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelectedSaveAsPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmDeleteQueuePopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmUpdatePlaylistSongsPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmMultipleDeletePlaylistSongsNotSupportedPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmDeletePlaylistSongPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmPlaylistClearPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSearchResultSelectedSaveAsPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSearchResultSelectedSaveToPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSongFilesSelectedSaveAsPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSongFilesSelectedSaveToPopupVisible
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region == Events ==

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
    public event EventHandler? QueueHeaderVisibilityChanged;
    public event EventHandler? SearchHeaderVisibilityChanged;
    public event EventHandler? PlaylistHeaderVisibilityChanged;
    public event EventHandler? FilesHeaderVisibilityChanged;
    public event EventHandler<string>? PlaylistRenameToDialogShow;
    public event EventHandler? GoToSettingsPage;
    public event EventHandler? AlbumsCollectionHasBeenReset;
    public event EventHandler? UserCanExecuteChanged;
    public event EventHandler<bool>? WorkingStateChanged;

    #endregion

    #region == Services == 

    private readonly IMpcService _mpc;

    #endregion

    private readonly InitWindow _initWin;
    private readonly IDialogService _dialog;

    private readonly CancellationTokenSource _cts = new();

    public MainViewModel(IMpcService mpcService, InitWindow initWin, IDialogService dialogService)
    {
        // MPD Service dependency injection.
        _mpc = mpcService;
        _initWin = initWin;
        _dialog = dialogService;

        #region == Subscribe to events ==

        _mpc.IsBusy += OnMpcIsBusy;
        _mpc.MpdIdleConnected += OnMpdIdleConnected;
        _mpc.DebugCommandOutput += OnDebugCommandOutput;
        _mpc.DebugIdleOutput += OnDebugIdleOutput;
        _mpc.ConnectionStatusChanged += OnConnectionStatusChanged;
        _mpc.ConnectionError += OnConnectionError;
        _mpc.MpdPlayerStatusChanged += OnMpdPlayerStatusChanged;
        _mpc.MpdCurrentQueueChanged += OnMpdCurrentQueueChanged;
        _mpc.MpdPlaylistsChanged += OnMpdPlaylistsChanged;
        _mpc.MpdOutputChanged += OnMpdOutputChanged;
        _mpc.MpdAckError += OnMpdAckError;
        _mpc.MpdFatalError += OnMpdFatalError;
        _mpc.MpdAlbumArtChanged += OnAlbumArtChanged;

        //_mpc.MpcInfo += new MpcService.MpcInfoEvent(OnMpcInfoEvent);

        // [Background][UI] etc
        _mpc.MpcProgress += OnMpcProgress;
        this.UpdateProgress += (sender, arg) => { this.OnUpdateProgress(arg); };

        #endregion

        #region == Init Song's time elapsed timer. ==  

        // Init Song's time elapsed timer.
        _elapsedTimer = new System.Timers.Timer(1000); // adjust this when _elapsedTimeMultiplier value is not 1.
        _elapsedTimer.Elapsed += ElapsedTimer;

        #endregion

        #region == Load settings ==

        System.IO.Directory.CreateDirectory(App.AppDataFolder);

        LoadSettings();

        #endregion

        #region == Themes ==

        // Sets default if not set in "load settings".
        if (_currentTheme is null)
        {
            CurrentTheme = Themes[0]; // needs this.
            _currentTheme = Themes[0]; // just because VS IDE complains to me to set.
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
                                Theme? theme = Themes.FirstOrDefault(x => x.Name == hoge.Value);
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

                        hoge = opts.Attribute("ArtistSortWithoutThePrefix");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsArtistSortWithoutThePrefix = true;
                            }
                            else
                            {
                                IsArtistSortWithoutThePrefix = false;
                            }
                        }

                        hoge = opts.Attribute("AlbumSortWithoutThePrefix");
                        if (hoge is not null)
                        {
                            if (hoge.Value == "True")
                            {
                                IsAlbumSortWithoutThePrefix = true;
                            }
                            else
                            {
                                IsAlbumSortWithoutThePrefix = false;
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

        try
        {
            // start the connection
            //await Task.Run(async () => await StartAsync(CurrentProfile.Host, CurrentProfile.Port), _cts.Token);
            // let's not await for faster start up.
            _ = Task.Run(() => StartAsync(CurrentProfile.Host, CurrentProfile.Port), _cts.Token);
        }
        catch (Exception ex)
        {
            _ = ex;
            Debug.WriteLine($"Exception @OnWindowLoaded {ex}");
        }

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
            attrs.Value = w.WindowState == WindowState.Normal ? w.Height.ToString() : w.WinRestoreHeight.ToString();
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
            attrs.Value = w.WindowState == WindowState.Normal ? w.Position.Y.ToString() : w.WinRestoreTop.ToString();
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("left");
            attrs.Value = w.WindowState == WindowState.Normal ? w.Position.X.ToString() : w.WinRestoreLeft.ToString();
            mainWindow.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("state");
            attrs.Value = w.WindowState switch
            {
                WindowState.Maximized => "Maximized",
                WindowState.Normal => "Normal",
                WindowState.Minimized => "Minimized",
                _ => attrs.Value
            };
            mainWindow.SetAttributeNode(attrs);

            // set MainWindow element to root.
            root.AppendChild(mainWindow);



            #endregion

            #region == Layout ==

            var lay = doc.CreateElement(string.Empty, "Layout", string.Empty);

            // LeftPain
            var leftpain = doc.CreateElement(string.Empty, "LeftPain", string.Empty);
            var lAttrs = doc.CreateAttribute("Width");
            if (IsFullyLoaded) // instead of IsFullyRendered
            {
                lAttrs.Value = windowWidth > (MainLeftPainActualWidth - 24) ? MainLeftPainActualWidth.ToString() : "241";
            }
            else
            {
                lAttrs.Value = MainLeftPainWidth.ToString();
            }
            leftpain.SetAttributeNode(lAttrs);

            lAttrs = doc.CreateAttribute("NavigationViewMenuOpen");
            lAttrs.Value = _isNavigationViewMenuOpen ? "True" : "False";
            leftpain.SetAttributeNode(lAttrs);

            //
            lay.AppendChild(leftpain);

            #region == Header columns ==

            var headers = doc.CreateElement(string.Empty, "Headers", string.Empty);

            ////
            lay.AppendChild(headers);

            #endregion

            ////
            root.AppendChild(lay);

            #endregion
        }

        #region == Options ==

        var opts = doc.CreateElement(string.Empty, "Options", string.Empty);

        //
        attrs = doc.CreateAttribute("AutoScrollToNowPlaying");
        attrs.Value = IsAutoScrollToNowPlaying ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ArtistSortWithoutThePrefix");
        attrs.Value = IsArtistSortWithoutThePrefix ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("AlbumSortWithoutThePrefix");
        attrs.Value = IsAlbumSortWithoutThePrefix ? "True" : "False";
        opts.SetAttributeNode(attrs);

        // 
        attrs = doc.CreateAttribute("UpdateOnStartup");
        attrs.Value = IsUpdateOnStartup ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ShowDebugWindow");
        attrs.Value = IsShowDebugWindow ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("SaveLog");
        attrs.Value = IsSaveLog ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArt");
        attrs.Value = IsDownloadAlbumArt ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArtEmbeddedUsingReadPicture");
        attrs.Value = IsDownloadAlbumArtEmbeddedUsingReadPicture ? "True" : "False";
        opts.SetAttributeNode(attrs);

        /// 
        root.AppendChild(opts);

        #endregion

        #region == Profiles  ==

        var xProfiles = doc.CreateElement(string.Empty, "Profiles", string.Empty);

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
            xAttrs.Value = p == CurrentProfile ? _volume.ToString() : p.Volume.ToString();
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

        if (IsSaveLog)
        {
            // Save error logs.
            Dispatcher.UIThread.Post(App.SaveErrorLog);
        }

        #endregion
    }

    // Closing
    public void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        #region == Subscribe to events ==

        // Unsubscribe events to avoid callbacks after shutdown
        _mpc.IsBusy -= OnMpcIsBusy;
        _mpc.MpdIdleConnected -= OnMpdIdleConnected;
        _mpc.DebugCommandOutput -= OnDebugCommandOutput;
        _mpc.DebugIdleOutput -= OnDebugIdleOutput;
        _mpc.ConnectionStatusChanged -= OnConnectionStatusChanged;
        _mpc.ConnectionError -= OnConnectionError;
        _mpc.MpdPlayerStatusChanged -= OnMpdPlayerStatusChanged;
        _mpc.MpdCurrentQueueChanged -= OnMpdCurrentQueueChanged;
        _mpc.MpdPlaylistsChanged -= OnMpdPlaylistsChanged;
        _mpc.MpdOutputChanged -= OnMpdOutputChanged;
        _mpc.MpdAckError -= OnMpdAckError;
        _mpc.MpdFatalError -= OnMpdFatalError;
        _mpc.MpdAlbumArtChanged -= OnAlbumArtChanged;

        //_mpc.MpcInfo += new MpcService.MpcInfoEvent(OnMpcInfoEvent);

        // [Background][UI] etc
        _mpc.MpcProgress -= OnMpcProgress;
        this.UpdateProgress -= (sender, arg) => { this.OnUpdateProgress(arg); };

        #endregion

        try
        {
            if (IsConnected)
            {
                _mpc.MpdStop = true;

                _mpc.MpdDisconnect(false);
            }

            _cts.Cancel();
        }
        catch (Exception ex)
        { 
           Debug.WriteLine($"Exception @OnWindowClosing() {ex}");
        }

        if (sender is Window w)
        {
            SaveSettings(w);
        }
    }

    public void OnWindowClosed(object? sender, EventArgs e)
    {
        _cts?.Dispose();
    }

    #endregion

    #region == Methods ==

    private async Task StartAsync(string host, int port)
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

                InfoBarErrTitle = "Error";
                InfoBarErrMessage = "Could not retrive IP Address from the hostname.";
                IsShowErrWindow = true;

                return;
            }
        }
        catch (Exception)
        {
            // TODO::
            ConnectionStatusMessage = "Error: Could not retrive IP Address from the hostname.";
            //StatusBarMessage = "Error: Could not retrive IP Address from the hostname.";

            InfoBarErrTitle = "Error";
            InfoBarErrMessage = "Could not retrive IP Address from the hostname.";
            IsShowErrWindow = true;

            return;
        }

        try
        {
            // Start MPD connection.
            await Task.Run(async () => await _mpc.MpdIdleConnect(HostIpAddress.ToString(), port), _cts.Token);
            // let's not await for faster start up.
            //_ = Task.Run(() => _mpc.MpdIdleConnect(HostIpAddress.ToString(), port), _cts.Token);
        }
        catch (Exception ex)
        {
            _ = ex;
            Debug.WriteLine($"Exception @StartAsync {ex}");
        }
    }

    private async Task LoadInitialData()
    {
        // "UIThread.CheckAccess() = FALSE"

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
                    if (_mpc.Commands.Contains("update"))
                    {
                        await _mpc.MpdSendUpdate();
                    }
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
                    UpdateCommandStatus();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryPlaylists();

                    await Task.Delay(50);
                    UpdatePlaylists();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryCurrentQueue();

                    await Task.Delay(50);
                    UpdateCurrentQueue();

                    await Task.Delay(50);
                    await _mpc.MpdIdleQueryOutputs();
                    UpdateAudioOutputs();

                    await Task.Delay(300);
                    await _mpc.MpdQueryListAlbumArtists();

                    await Task.Delay(50);
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
        // "UIThread.CheckAccess() = FALSE"

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
                    _mpc.MpdCurrentSong?.IsPlaying = false;
                    AlbumCover = null;
                    //IsAlbumArtVisible = false;
                    AlbumArtBitmapSource = _albumArtBitmapSourceDefault;
                }
            }
            else
            {
                // just in case
                _mpc.MpdCurrentSong?.IsPlaying = false;

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
                        CurrentSong = item;
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

                                if ((res != null) && (CurrentSong is not null))
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
                    _mpc.MpdCurrentSong?.IsPlaying = false;

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
                            //fuga.IsSelected = false; // so clear it for now.
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
            var ci = CultureInfo.CurrentCulture;
            var comp = StringComparer.Create(ci, true);

            UpdateProgress?.Invoke(this, "[UI] Updating the AlbumArtists...");
            //Artists = new ObservableCollection<AlbumArtist>(_mpc.AlbumArtists);// COPY. 

            // Sort with AlbumArtist for Artist view.
            if (IsArtistSortWithoutThePrefix)
            {
                Artists = new ObservableCollection<AlbumArtist>(_mpc.AlbumArtists.OrderBy(x => x.NameSort, comp));// COPY. // Sort without prefix like "The".
            }
            else
            {
                Artists = new ObservableCollection<AlbumArtist>(_mpc.AlbumArtists.OrderBy(x => x.Name, comp));// COPY. // Sort 
            }

            UpdateProgress?.Invoke(this, "[UI] Updating the Albums...");
            //Albums = new ObservableCollection<AlbumEx>(_mpc.Albums); // COPY.

            // Sort with AlbumArtist for Album view.(TODO: Save preference. AlbumArtist/Album name)
            if (IsArtistSortWithoutThePrefix)
            {
                Albums = new ObservableCollection<AlbumEx>(_mpc.Albums.OrderBy(x => x.AlbumArtistSort, comp)); // COPY. // Sort without prefix like "The" or "A".
            }
            else
            {
                Albums = new ObservableCollection<AlbumEx>(_mpc.Albums.OrderBy(x => x.AlbumArtist, comp)); // COPY. // Sort 
            }

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

    private void UpdateAudioOutputs()
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateProgress?.Invoke(this, "[UI] Updating the audio outputs...");
            AudioOutputs = new ObservableCollection<AudioOutput>(_mpc.AudioOutputs);


            UpdateProgress?.Invoke(this, "");
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

    private void UpdateCommandStatus()
    {
        Dispatcher.UIThread.Post(async () => 
        {
            PlayCommand.NotifyCanExecuteChanged();
            PlayNextCommand.NotifyCanExecuteChanged();
            PlayPrevCommand.NotifyCanExecuteChanged();
            ChangeSongCommand.NotifyCanExecuteChanged();
            PlayPauseCommand.NotifyCanExecuteChanged();
            PlayStopCommand.NotifyCanExecuteChanged();
            SetRandomCommand.NotifyCanExecuteChanged();
            SetRpeatCommand.NotifyCanExecuteChanged();
            SetConsumeCommand.NotifyCanExecuteChanged();
            SetSingleCommand.NotifyCanExecuteChanged();
            SetVolumeCommand.NotifyCanExecuteChanged();
            SetSeekCommand.NotifyCanExecuteChanged();
            VolumeMuteCommand.NotifyCanExecuteChanged();
            VolumeDownCommand.NotifyCanExecuteChanged();
            VolumeUpCommand.NotifyCanExecuteChanged();
            
            QueueSaveToCommand.NotifyCanExecuteChanged();
            QueueListviewSaveSelectedToCommand.NotifyCanExecuteChanged();
            QueueClearWithoutPromptCommand.NotifyCanExecuteChanged();
            QueueListviewSortByCommand.NotifyCanExecuteChanged();
            QueueListviewSortReverseCommand.NotifyCanExecuteChanged();
            QueueListviewLeftDoubleClickCommand.NotifyCanExecuteChanged();
            QueueListviewDeleteSelectedWithoutPromptCommand.NotifyCanExecuteChanged();
            
            // TODO: more.

            UserCanExecuteChanged?.Invoke(this, EventArgs.Empty);
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

        // TODO:
        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            //await Task.Yield();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsWorking = true;
            });
            await Task.Yield();

            CommandResult result = await _mpc.MpdQueryListAll().ConfigureAwait(false);
            if (result.IsSuccess)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
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
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    filestNode.IsAcquired = false;
                });
                Debug.WriteLine("fail to get MpdQueryListAll: " + result.ErrorMessage);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsWorking = false;
            });
        }, _cts.Token);
    }

    private void GetAlbumSongs(AlbumEx album)
    {
        if (album is null)
        {
            Debug.WriteLine("GetAlbumSongs: obj is null, returning.");
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            //Debug.WriteLine("GetAlbumSongs: Invoked with Album name: " + album.Name);

            if (!album.IsSongsAcquired)
            {
                //album.IsSongsAcquired = true;

                if (!string.IsNullOrEmpty(album.AlbumArtist.Trim()))
                {
                    //Debug.WriteLine($"GetAlbumSongs: Album artist is not empty, searching by album artist. ({album.AlbumArtist})");
                    var r = await SearchArtistSongsAsync(album.AlbumArtist);//.ConfigureAwait(ConfigureAwaitOptions.None);// no trim() here.

                    if (r.IsSuccess)
                    {
                        if (r.SearchResult is null)
                        {
                            Debug.WriteLine("GetAlbumSongs: SearchResult is null, returning.");
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

                                    if ((!string.IsNullOrEmpty(song.Date)) && (!string.IsNullOrEmpty(song.Album)))
                                    {
                                        album.ReleaseYear = song.Date;
                                    }
                                }
                            }
                        }
                        album.IsSongsAcquired = true;

                        SelectedAlbum = album;
                        await Task.Yield();
                        IsAlbumContentPanelVisible = true;

                        /*
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (r.SearchResult is null)
                            {
                                Debug.WriteLine("GetAlbumSongs: SearchResult is null, returning.");
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
                        Debug.WriteLine("GetAlbumSongs: SearchArtistSongs returned false, returning.");
                        return;
                    }
                }
                else
                {
                    Debug.WriteLine($"GetAlbumSongs: No album artist, trying to search by album name. ({album.Name})");

                    if (!string.IsNullOrEmpty(album.Name.Trim()))
                    {
                        var r = await SearchAlbumSongsAsync(album.Name); // no trim() here.
                        if (r.IsSuccess)
                        {
                            if (r.SearchResult is null)
                            {
                                Debug.WriteLine("GetAlbumSongs: SearchResult is null, returning.");
                                return;
                            }

                            foreach (var song in r.SearchResult)
                            {
                                album.Songs.Add(song);

                                if ((!string.IsNullOrEmpty(song.Date)) && (!string.IsNullOrEmpty(song.Album)))
                                {
                                    album.ReleaseYear = song.Date;
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

                            });
                            */
                        }
                        else
                        {
                            Debug.WriteLine("GetAlbumSongs: SearchArtistSongs returned false, returning.");
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("GetAlbumSongs: No album name, no artist name.");
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
        });
    }

    private async Task GetArtistSongsAsync(AlbumArtist? artist)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (artist is null)
            {
                Debug.WriteLine("GetArtistSongs: artist is null, returning.");
                return;
            }

            IsWorking = true;
            await Task.Yield();

            var r = await SearchArtistSongsAsync(artist.Name);//.ConfigureAwait(ConfigureAwaitOptions.None);

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

                        if ((!string.IsNullOrEmpty(song.Date)) && (!string.IsNullOrEmpty(song.Album)))
                        {
                            slbm.ReleaseYear = song.Date;
                        }
                    }
                }

                slbm.IsSongsAcquired = true;
            }

            IsWorking = false;
            await Task.Yield();
        });
    }

    private async Task<CommandSearchResult> SearchArtistSongsAsync(string name)
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

    private async Task<CommandSearchResult> SearchAlbumSongsAsync(string name)
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
    
    private async Task GetAlbumPicturesAsync(IEnumerable<object>? albumExItems)
    {
        if (_mpc.MpdStop)
        {
            Debug.WriteLine("GetAlbumPictures: MpdStop");
            return;
        }

        if (albumExItems is null)
        {
            Debug.WriteLine("GetAlbumPictures: (AlbumExItems is null)");
            return;
        }

        if (!albumExItems.Any())
        {
            return;
        }

        if (_cts.Token.IsCancellationRequested)
        {
            Debug.WriteLine("IsCancellationRequested @GetAlbumPictures");
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (Albums.Count < 1)
            {
                //Debug.WriteLine("GetAlbumPictures: Albums.Count < 1, returning.");
                return;
            }

            //UpdateProgress?.Invoke(this, "[UI] Loading album covers ...");
            //IsWorking = true;
            //await Task.Yield();

            foreach (var item in albumExItems)
            {
                if (_mpc.MpdStop)
                {
                    return;
                }

                if (_cts.Token.IsCancellationRequested)
                {
                    Debug.WriteLine("IsCancellationRequested in foreach @GetAlbumPictures");
                    return;
                }

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

                if (string.IsNullOrEmpty(album.Name.Trim()))
                {
                    //Debug.WriteLine($"GetAlbumPictures: album.Name is null or empty, skipping. {album.AlbumArtist}");
                    continue;
                }

                if (album.IsImageLoading)
                {
                    continue;
                }
                album.IsImageLoading = true;
                //Debug.WriteLine($"GetAlbumPictures: Artist:{album.AlbumArtist}, Album:{album.Name}");

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
                
                string fileTempPath = System.IO.Path.Combine(App.AppDataCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".tmp";
                if (File.Exists(fileTempPath))
                {
                    album.IsImageLoading = false;
                    continue; // Skip if temp file exists, it means the album art has found to have no image.
                }

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
                    if (_mpc.MpdStop)
                    {
                        Debug.WriteLine("GetAlbumPictures: MpdStop in foreach1 loop");
                        break;
                    }

                    album.IsImageLoading = true;

                    var ret = await SearchAlbumSongsAsync(album.Name);
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

                    string strDirPath = System.IO.Path.Combine(App.AppDataCacheFolder, strArtist);
                    bool isWaitFailed = false;
                    bool isCoverExists = false;
                    bool isNoAlbumCover = false;

                    foreach (var albumsong in sresult)
                    {
                        await Task.Yield();

                        if (_mpc.MpdStop)
                        {
                            Debug.WriteLine("GetAlbumPictures: MpdStop in foreach2 loop");
                            break;
                        }

                        if (_cts.Token.IsCancellationRequested)
                        {
                            Debug.WriteLine("IsCancellationRequested in forearch2 @GetAlbumPictures");
                            return;
                        }

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
                            //Debug.WriteLine($"GetAlbumPictures: Album:{album.Name}, Song {albumsong.File}");
                            //Debug.WriteLine($"GetAlbumPictures: Processing song {albumsong.File} from album {album.Name}");
                            var r = await _mpc.MpdQueryAlbumArtForAlbumView(albumsong.File, IsDownloadAlbumArtEmbeddedUsingReadPicture);
                            if (!r.IsSuccess)
                            {
                                isNoAlbumCover = r.IsNoBinaryFound;
                                isWaitFailed = r.IsWaitFailed;
                                //album.IsImageLoading = false;// don't
                                //Debug.WriteLine($"MpdQueryAlbumArtForAlbumView failed: {r.ErrorMessage}");
                                continue;
                            }
                            if (r.AlbumCover is null) continue;
                            if (!r.AlbumCover.IsSuccess) continue;

                            //Dispatcher.UIThread.Post(() =>
                            //{
                            album.AlbumImage = r.AlbumCover.AlbumImageSource;
                            //});
                            Directory.CreateDirectory(strDirPath);
                            album.AlbumImage?.Save(filePath, 100);

                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {albumsong.File}");
                            //Debug.WriteLine($"GetAlbumPictures: Successfully retrieved album art for {album.Name} by {album.AlbumArtist}");

                            //Debug.WriteLine(System.IO.Path.Combine(strArtist, strAlbum) + ".bmp");

                            album.IsImageAcquired = true;
                            album.IsImageLoading = false;
                            isCoverExists = true;

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
                    if (isWaitFailed)
                    {
                        album.IsImageLoading = false;
                        continue;
                    }

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
            //IsWorking = false;
            //await Task.Yield();
        });
    }

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

    private static async Task<long> GetFolderSizeAsync(string path)
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
            totalSize += await GetFolderSizeAsync(subDirectory.FullName);
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

    public async Task GetCacheFolderSizeAsync()
    {
        AlbumCacheFolderSizeFormatted = ToFileSizeString(await GetFolderSizeAsync(App.AppDataCacheFolder).ConfigureAwait(true));
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

    private async void OnMpdIdleConnected(MpcService sender)
    {
        Debug.WriteLine("OK MPD " + _mpc.MpdVerText + " @OnMpdConnected");

        // Connected from InitWindow, so save and clean up. 
        Dispatcher.UIThread.Post( () =>
        {
            MpdVersion = _mpc.MpdVerText;

            //MpdStatusMessage = MpdVersion;// + ": " + MPDCtrlX.Properties.Resources.MPD_StatusConnected;

            MpdStatusButton = _pathMpdOkButton;

            IsConnected = true;

            // Not good.
            //IsShowAckWindow = false;
            //IsShowErrWindow = false;

            if (_initWin is not null)
            {
                if (_initWin.IsActive || _initWin.IsVisible)
                {
                    if (IsRememberAsProfile)
                    {
                        // TODO:
                        if (string.IsNullOrEmpty(_host))
                        {
                            _host = "localhost";
                        }

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
        await Task.Run(LoadInitialData, _cts.Token);
        //_ = Task.Run(LoadInitialData, _cts.Token);
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

    private void OnMpdOutputChanged(MpcService sender)
    {
        //Debug.WriteLine("OnMpdOutputChanged");
        UpdateAudioOutputs();
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

        InfoBarErrTitle = MPDCtrlX.Properties.Resources.ConnectionStatus_ConnectionError;
        InfoBarErrMessage = msg;
        IsShowErrWindow = true;
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

    private async void OnMpdAckError(MpcService sender, string ackMsg, string origin)
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

        if (_mpc.Commands.Contains("clearerror"))
        {
            await _mpc.MpdClearError();
        }
    }

    private async void OnMpdFatalError(MpcService sender, string errMsg, string origin)
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

        if (_mpc.Commands.Contains("clearerror"))
        {
            await _mpc.MpdClearError();
        }
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
        if ((_elapsed < Time) && (_mpc.MpdStatus.MpdState == Status.MpdPlayState.Play))
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

    [RelayCommand(CanExecute = nameof(PlayCanExecute))]
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
    public bool PlayCanExecute()
    {
        if (!_mpc.Commands.Contains("play")) { return false; }
        if (!_mpc.Commands.Contains("stop")) { return false; }
        if (!_mpc.Commands.Contains("pause")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(PlayNextCanExecute))]
    public async Task PlayNext()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackNext(Convert.ToInt32(_volume));
    }
    public bool PlayNextCanExecute()
    {
        if (!_mpc.Commands.Contains("next")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(PlayPrevCanExecute))]
    public async Task PlayPrev()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) { return; }

        await _mpc.MpdPlaybackPrev(Convert.ToInt32(_volume));
    }
    public bool PlayPrevCanExecute()
    {
        if (!_mpc.Commands.Contains("previous")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(ChangeSongCanExecute))]
    public async Task ChangeSong()
    {
        if (IsBusy) return;
        if (Queue.Count < 1) return;
        if (SelectedQueueSong is null) return;

        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), SelectedQueueSong.Id);
    }
    public bool ChangeSongCanExecute()
    {
        if (!_mpc.Commands.Contains("play")) { return false; }
        if (!_mpc.Commands.Contains("playid")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(PlayPauseCanExecute))]
    public async Task PlayPause()
    {
        if (IsBusy) return;
        await _mpc.MpdPlaybackPause();
    }
    public bool PlayPauseCanExecute()
    {
        if (!_mpc.Commands.Contains("pause")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(PlayStopCanExecute))]
    public async Task PlayStop()
    {
        if (IsBusy) return;
        await _mpc.MpdPlaybackStop();
    }
    public bool PlayStopCanExecute()
    {
        if (!_mpc.Commands.Contains("stop")) { return false; }
        return true;
    }

    #endregion

    #region == Playback opts ==

    [RelayCommand(CanExecute = nameof(SetRandomCanExecute))]
    public async Task SetRandom()
    {
        if (IsBusy) return;
        await _mpc.MpdSetRandom(_random);
    }
    public bool SetRandomCanExecute()
    {
        if (!_mpc.Commands.Contains("random")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(SetRpeatCanExecute))]
    public async Task SetRpeat()
    {
        if (IsBusy) return;
        await _mpc.MpdSetRepeat(_repeat);
    }
    public bool SetRpeatCanExecute()
    {
        if (!_mpc.Commands.Contains("repeat")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(SetConsumeCanExecute))]
    public async Task SetConsume()
    {
        if (IsBusy) return;
        await _mpc.MpdSetConsume(_consume);
    }
    public bool SetConsumeCanExecute()
    {
        if (!_mpc.Commands.Contains("consume")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(SetSingleCanExecute))]
    public async Task SetSingle()
    {
        if (IsBusy) return;
        await _mpc.MpdSetSingle(_single);
    }
    public bool SetSingleCanExecute()
    {
        if (!_mpc.Commands.Contains("single")) { return false; }
        return true;
    }

    #endregion

    #region == Playback seek and volume ==

    [RelayCommand(CanExecute = nameof(SetVolumeCanExecute))]
    public async Task SetVolume()
    {
        if (IsBusy) return;
        await _mpc.MpdSetVolume(Convert.ToInt32(_volume));
    }
    public bool SetVolumeCanExecute()
    {
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(SetSeekCanExecute))]
    public async Task SetSeek()
    {
        if (IsBusy) return;
        double elapsed = _elapsed / _elapsedTimeMultiplier;
        await _mpc.MpdPlaybackSeek(_mpc.MpdStatus.MpdSongID, elapsed);
    }
    public bool SetSeekCanExecute()
    {
        if (!_mpc.Commands.Contains("seekid")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(VolumeMuteCanExecute))]
    public async Task VolumeMute()
    {
        if (IsBusy) return;
        await _mpc.MpdSetVolume(0);
    }
    public bool VolumeMuteCanExecute()
    {
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(VolumeDownCanExecute))]
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
    public bool VolumeDownCanExecute()
    {
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(VolumeUpCanExecute))]
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
    public bool VolumeUpCanExecute()
    {
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        return true;
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
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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

    public bool PlaylistAddCanExecute()
    {
        if (!_mpc.Commands.Contains("playlistadd")) { return false; }
        return true;
    }

    public async Task AddTo(string playlistName, List<string> uris)
    {
        if (string.IsNullOrEmpty(playlistName))
            return;
        if (uris.Count == 0)
            return;

        await _mpc.MpdPlaylistAdd(playlistName, uris);
    }

    // Add selected to playlist
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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
    [RelayCommand(CanExecute = nameof(PlayCanExecute))]
    public async Task QueueListviewEnterKey()
    {
        if (Queue.Count < 1)
        {
            return;
        }
        if (SelectedQueueSong is null)
        {
            return;
        }

        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), SelectedQueueSong.Id);
    }

    // TODO: Actually used as ContextMenu Play.
    [RelayCommand(CanExecute = nameof(PlayCanExecute))]
    public async Task QueueListviewLeftDoubleClick(SongInfoEx song)
    {
        if (Queue.Count < 1)
        {
            return;
        }
        if (SelectedQueueSong is null)
        {
            return;
        }
        await _mpc.MpdPlaybackPlay(Convert.ToInt32(_volume), song.Id);
    }

    // Command to clear the queue listview.
    [RelayCommand(CanExecute = nameof(QueueClearWithoutPromptCanExecute))]
    public async Task QueueClearWithoutPrompt()
    {
        if (Queue.Count == 0) { return; }

        await _mpc.MpdPlaybackStop();
        await _mpc.MpdClear();
    }
    public bool QueueClearWithoutPromptCanExecute()
    {
        if (!_mpc.Commands.Contains("stop")) { return false; }
        if (!_mpc.Commands.Contains("clear")) { return false; }
        return true;
    }

    // Command to delete selected songs from the queue listview.
    [RelayCommand(CanExecute = nameof(QueueListviewDeleteSelectedWithoutPromptCanExecute))]
    public async Task QueueListviewDeleteSelectedWithoutPrompt(object obj)
    {
        if (IsBusy) return;

        if (IsWorking) return;

        if (obj is null) return;

        System.Collections.IList items = (System.Collections.IList)obj;

        if (items.Count == 0) return;

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
    public bool QueueListviewDeleteSelectedWithoutPromptCanExecute()
    {
        if (!_mpc.Commands.Contains("deleteid")) { return false; }
        return true;
    }

    // Move selected songs up in the queue listview.
    [RelayCommand(CanExecute = nameof(QueueListviewMoveCanExecute))]
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
    public bool QueueListviewMoveCanExecute()
    {
        if (!_mpc.Commands.Contains("moveid")) { return false; }
        return true;
    }

    // Move down
    [RelayCommand(CanExecute = nameof(QueueListviewMoveCanExecute))]
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
    [RelayCommand(CanExecute = nameof(QueueListviewMoveCanExecute))]
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
    [RelayCommand(CanExecute = nameof(QueueListviewMoveCanExecute))]
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

        if (Queue.Count <= 0)
        {
            return;
        }

        SelectedQueueFilterSong = null;
        QueueForFilter.Clear();

        QueueForFilter = new ObservableCollection<SongInfoEx>(Queue);

        FilterQueueQuery = "";

        IsQueueFindVisible = true;
    }

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
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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

    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
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
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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
    [RelayCommand(CanExecute = nameof(SongsPlayCanExecute))]
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
    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
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
    [RelayCommand(CanExecute = nameof(PlaylistAddCanExecute))]
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

    [RelayCommand(CanExecute = nameof(SongsPlayCanExecute))]
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

    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
    public async Task FilesListviewAddThis(object obj)
    {
        if (obj is null) return;

        if (obj is NodeFile song)
        {
            await _mpc.MpdAdd(song.OriginalFileUri);
        }
    }

    [RelayCommand]
    public void FilesFindShowHide()
    {
        if (IsFilesFindVisible)
        {
            IsFilesFindVisible = false;
            return;
        }

        if (MusicDirectories.Count <= 0) 
        {
            return;
        }

        IsFilesFindVisible = true;

    }

    #endregion

    #region == Albums ==

    [RelayCommand]
    public void AlbumsItemInvoked(object obj)
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

        GetAlbumSongs(album);
    }

    [RelayCommand]
    public void AlbumsCloseAlbumContentPanel()
    {
        IsAlbumContentPanelVisible = false;
    }

    [RelayCommand]
    public void AlbumsSortBy(object obj)
    {
        if (obj is null)
        {
            return;
        }
        if (obj is not string key)
        {
            return;
        }

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        var ci = CultureInfo.CurrentCulture;
        var comp = StringComparer.Create(ci, true);

        switch (key)
        {
            case "artist":
                if (IsArtistSortWithoutThePrefix)
                {
                    Albums = new ObservableCollection<AlbumEx>(Albums.OrderBy(x => x.AlbumArtistSort, comp)); // COPY. // Sort without prefix like "The".
                }
                else
                {
                    Albums = new ObservableCollection<AlbumEx>(Albums.OrderBy(x => x.AlbumArtist, comp)); // COPY. // Sort
                }
                break;
            case "album":
                if (IsAlbumSortWithoutThePrefix)
                {
                    Albums = new ObservableCollection<AlbumEx>(Albums.OrderBy(x => x.NameSort, comp)); // COPY. // Sort without prefix like "The" or "A".
                }
                else
                {
                    Albums = new ObservableCollection<AlbumEx>(Albums.OrderBy(x => x.Name, comp)); // COPY. // Sort
                }
                break;
            case "reverse":
                Albums = new ObservableCollection<AlbumEx>(Albums.Reverse<AlbumEx>());
                break;
        }

        SelectedAlbum = null;

        // Need this to load image.
        // Albums sort resets ObservableCollection which is not recognized by ListViewBehavior and does not UpdateVisibleItems,
        // so forcibly fire scroll event in AlbumsPage's code behind.
        AlbumsCollectionHasBeenReset?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void SelectedAlbumGoToArtistPage(AlbumEx album)
    {
        if (album is null)
        {
            return;
        }

        var asdf = album.AlbumArtist;
        if (string.IsNullOrEmpty(asdf.Trim()))
        {
            return;
        }

        var item = Artists.FirstOrDefault(i => i.Name == asdf);
        if (item is null) return;

        // Close pane.
        IsAlbumContentPanelVisible = false;

        GoToArtistPage(item);
    }

    [RelayCommand]
    public void AlbumFilterSelect(object obj)//
    {
        if (obj is null) return;
        if (obj is not AlbumEx album) return;
        if (Albums.Count <= 1) return;
        if (SelectedFilterAlbum is null) return;

        if (SelectedFilterAlbum.Name != album.Name) return; // TODO: culture compare.

        SelectedAlbum = album; // 
        //ScrollIntoViewAndSelect?.Invoke(this, SelectedFilterAlbumArtist.Index);
    }

    [RelayCommand]
    public void AlbumFindShowHide()
    {
        if (IsAlbumFindVisible)
        {
            IsAlbumFindVisible = false;
            return;
        }

        if (Albums.Count <= 0)
        {
            return;
        }

        SelectedFilterAlbum = null;
        AlbumsForFilter.Clear();

        FilterAlbumQuery = "";

        IsAlbumFindVisible = true;
    }

    #endregion

    #region == Artists ==

    [RelayCommand]
    public void ArtistFilterSelect(object obj)//
    {
        if (obj is null) return;
        if (obj is not AlbumArtist artist) return;
        if (Artists.Count <= 1)return;
        if (SelectedFilterAlbumArtist is null) return;

        if (SelectedFilterAlbumArtist.Name != artist.Name) return; // TODO: culture compare.

        SelectedAlbumArtist = artist; // Should scroll to the selected item because AutoScrollToSelectedItem is true in ArtistFilter ListView. 
        //ScrollIntoViewAndSelect?.Invoke(this, SelectedFilterAlbumArtist.Index);
    }

    [RelayCommand]
    public void ArtistFindShowHide()
    {
        if (IsArtistFindVisible)
        {
            IsArtistFindVisible = false;
            return;
        }

        if (Artists.Count <= 0)
        {
            return;
        }

        SelectedFilterAlbumArtist = null;
        ArtistsForFilter.Clear();

        // TODO:
        //QueueForFilter = new ObservableCollection<SongInfoEx>(Queue);

        FilterArtistQuery = "";

        IsArtistFindVisible = true;

        // Set focus textbox in code behind.
        //ArtistFindWindowVisibilityChangedSetFocus?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region == Playlist ==

    [RelayCommand(CanExecute = nameof(LoadPlaylistCanExecute))]
    public async Task PlaylistLoadPlaylist()
    {
        if (IsBusy) return;
        if (IsWorking) return;
        if (SelectedNodeMenu is null)
            return;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem list)
            return;

        if (list.PlaylistSongs.Count == 0)
        {
            return;
        }

        await _mpc.MpdLoadPlaylist(SelectedNodeMenu.Name);
    }
    public bool LoadPlaylistCanExecute()
    {
        if (!_mpc.Commands.Contains("load")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(ChangePlaylistCanExecute))]
    public async Task PlaylistClearLoadPlaylist()
    {
        /*
        if (IsBusy)
            return;
        if (IsWorking) return;
        */
        if (SelectedNodeMenu is null)
            return;
        if (SelectedNodeMenu is not NodeMenuPlaylistItem list)
            return;

        if (list.PlaylistSongs.Count == 0)
        {
            return;
        }

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
    public bool ChangePlaylistCanExecute()
    {
        if (!_mpc.Commands.Contains("stop")) { return false; }
        if (!_mpc.Commands.Contains("clear")) { return false; }
        if (!_mpc.Commands.Contains("load")) { return false; }
        if (!_mpc.Commands.Contains("play")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        if (!_mpc.Commands.Contains("currentsong")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(RenamePlaylistCanExecute))]
    public void PlaylistRenamePlaylist(string playlist)
    {
        if (string.IsNullOrEmpty(SelectedPlaylistName))
        {
            return;
        }

        if (SelectedPlaylistName != playlist)
        {
            return;
        }

        PlaylistRenameToDialogShow?.Invoke(this, playlist);
    }
    public bool RenamePlaylistCanExecute()
    {
        if (!_mpc.Commands.Contains("rename")) { return false; }
        return true;
    }

    // Rename playlist. Called from codebehind.
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
            PlaylistPageSubTitleSongCount = ""; 
            OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));

            foreach (var hoge in MainMenuItems)
            {
                if (hoge is not NodeMenuPlaylists) continue;
                foreach (var fuga in hoge.Children)
                {
                    if (fuga is not NodeMenuPlaylistItem) continue;
                    if (!string.Equals(playlist, fuga.Name)) continue;
                    //Debug.WriteLine($"{playlist} is now selected....");
                                
                    // Needed this. Otherwise, Playlist name wouldn't update. 
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsNavigationViewMenuOpen = true;
                        fuga.Selected = true;
                        SelectedNodeMenu = fuga;
                        SelectedPlaylistName = fuga.Name;
                    });
                    break;
                }
            }
        });
    }

    // CheckPlaylistNameExists when Rename playlists.
    public bool CheckIfPlaylistExists(string playlistName)
    {
        bool match = false;

        if (Playlists.Count <= 0) return match;
        foreach (var hoge in Playlists)
        {
            if (!string.Equals(playlistName, hoge.Name, StringComparison.CurrentCultureIgnoreCase)) continue;
            match = true;
            break;
        }

        return match;
    }

    [RelayCommand(CanExecute = nameof(PlaylistRemoveCanExecute))]
    public async Task PlaylistRemovePlaylistWithoutPrompt(string playlist)
    {
        if (string.IsNullOrEmpty(SelectedPlaylistName))
        {
            return;
        }

        if (SelectedPlaylistName != playlist)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            // Move selection before deleting
            _mainMenuItems.PlaylistsDirectory.Selected = true;
        });

        var ret = await _mpc.MpdRemovePlaylist(SelectedPlaylistName);

        if (ret.IsSuccess)
        {
            SelectedPlaylistName = string.Empty;
            RenamedSelectPendingPlaylistName = string.Empty;
            // Clear listview 
            PlaylistSongs.Clear();
            //CurrentPage = App.GetService<QueuePage>();

            PlaylistPageSubTitleSongCount = string.Empty;
            OnPropertyChanged(nameof(PlaylistPageSubTitleSongCount));
        }
    }
    public bool PlaylistRemoveCanExecute()
    {
        if (!_mpc.Commands.Contains("rm")) { return false; }
        return true;
    }

    // Deletes song in a playlist.
    [RelayCommand(CanExecute = nameof(PlaylistDeletePosCanExecute))]
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
        }
    }
    public bool PlaylistDeletePosCanExecute()
    {
        if (!_mpc.Commands.Contains("playlistdelete")) { return false; }
        return true;
    }

    // Playlist Clear
    [RelayCommand(CanExecute = nameof(PlaylistClearCanExecute))]
    public async Task PlaylistClearPlaylistWithoutPrompt(string playlist)
    {
        if (string.IsNullOrEmpty(SelectedPlaylistName))
        {
            return;
        }

        if (SelectedPlaylistName != playlist)
        {
            return;
        }

        var ret = await _mpc.MpdPlaylistClear(playlist);

        if (ret.IsSuccess)
        {
            PlaylistSongs.Clear();
        }
    }
    public bool PlaylistClearCanExecute()
    {
        if (!_mpc.Commands.Contains("playlistclear")) { return false; }
        return true;
    }

    // double clicked in a playlist listview (currently NOT USED)
    [RelayCommand]
    public async Task PlaylistSongsListviewLeftDoubleClick(SongInfo song)
    {
        if (SelectedPlaylistSong is null) return;
        if (IsBusy) return;

        await _mpc.MpdAdd(song.File);
    }

    // Remove duplicated songs in a playlist. 
    [RelayCommand(CanExecute = nameof(PlaylistRemoveDuplicatesCanExecute))]
    public void PlaylistRemoveDuplicates(string playlist)
    {
        if (string.IsNullOrEmpty(SelectedPlaylistName))
        {
            return;
        }

        if (SelectedPlaylistName != playlist)
        {
            return;
        }

        if (PlaylistSongs.Count <= 1)
        {
            return;
        }

        //var duplicates = PlaylistSongs.GroupBy(x => x.File).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        var uniqueSongs = PlaylistSongs.GroupBy(s => s.File).Select(g => g.First()).ToList();

        if (PlaylistSongs.Count == uniqueSongs.Count)
        {
            return;
        }

        List<string> uris = [];
        foreach (var song in uniqueSongs)
        {
            //Debug.WriteLine($"Unique song: {song.Title} - {song.File}");
            uris.Add(song.File);
        }

        _mpc.MpdPlaylistClear(SelectedPlaylistName);
        _mpc.MpdPlaylistAdd(SelectedPlaylistName, uris);
    }
    public bool PlaylistRemoveDuplicatesCanExecute()
    {
        if (!_mpc.Commands.Contains("playlistclear")) { return false; }
        if (!_mpc.Commands.Contains("playlistadd")) { return false; }
        return true;
    }

    #endregion

    #region == Common Listview selected/collection songs command ==

    // used in context menu of Search result, Playlist etc.
    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
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
    [RelayCommand(CanExecute = nameof(SongsPlayCanExecute))]
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
    public bool SongsPlayCanExecute()
    {
        if (!_mpc.Commands.Contains("clear")) { return false; }
        if (!_mpc.Commands.Contains("add")) { return false; }
        if (!_mpc.Commands.Contains("play")) { return false; }
        if (!_mpc.Commands.Contains("setvol")) { return false; }
        if (!_mpc.Commands.Contains("currentsong")) { return false; }
        return true;
    }

    // Add to queue (Used in Albums, Artists, Search pages etc.)
    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
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

    public bool AddToQueueCanExecute()
    {
        if (!_mpc.Commands.Contains("add")) { return false; }
        return true;
    }

    [RelayCommand(CanExecute = nameof(SongsPlayCanExecute))]
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

    [RelayCommand(CanExecute = nameof(AddToQueueCanExecute))]
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
        await GetCacheFolderSizeAsync();
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

    [RelayCommand(CanExecute = nameof(AudioOutputToggleCanExecute))]
    public async Task AudioOutputToggleEnable(AudioOutput item) 
    {
        if (item is null)
            return;
        if (string.IsNullOrEmpty(item.Id))
            return;
        if (AudioOutputs.Count == 0)
            return;

        //Debug.WriteLine($"Toggling audio output with id: {item.Id}");
        await _mpc.MpdToggleOutput(item.Id);
    }
    public bool AudioOutputToggleCanExecute()
    {
        if (!_mpc.Commands.Contains("toggleoutput")) { return false; }
        return true;
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
    public async Task ReConnectWithSelectedProfile()
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

        _mainMenuItems.FilesDirectory?.IsAcquired = false;

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
        IsShowAckWindow = false;
        IsShowErrWindow = false;

        // TODO: more?

        //IsAlbumArtVisible = false;
        AlbumArtBitmapSource = _albumArtBitmapSourceDefault;

        await Task.Run(async () => await StartAsync(_host, _port), _cts.Token);
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
        CurrentProfile?.Volume = Convert.ToInt32(Volume);

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
        CurrentSong?.IsPlaying = false;
        CurrentSong = null;
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

            _mainMenuItems.FilesDirectory?.IsAcquired = false;

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
                    Debug.WriteLine("Host info is empty. @ChangeConnectionProfile");
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
        CurrentProfile?.Volume = Volume;

        // Clear current...
        CurrentSong?.IsPlaying = false;
        CurrentSong = null;
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

            _mainMenuItems.FilesDirectory?.IsAcquired = false;

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
    public async Task TryConnect()
    {
        Debug.WriteLine("_host: "+ _host);
        await Task.Run(async () => await StartAsync(_host, _port), _cts.Token);
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
        else if (SelectedNodeMenu is NodeMenuAlbum)
        {
            AlbumFindShowHide();
        }
        else if (SelectedNodeMenu is NodeMenuArtist)
        {
            ArtistFindShowHide();
        }
        else if (SelectedNodeMenu is NodeMenuSearch)
        {
            // Nothing to do.
        }
        else if (SelectedNodeMenu is NodeMenuFiles)
        {
            FilesFindShowHide();
        }
        else
        {

        }
    }

    [RelayCommand]
    public void ShowSearch()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            IsWorking = true;
            await Task.Yield();
            await Task.Delay(100);
            IsNavigationViewMenuOpen = true; // Need this somehow.
            await Task.Yield();
            await Task.Delay(100);
            _mainMenuItems.SearchDirectory.Selected = true;
            //SelectedNodeMenu = _mainMenuItems.SearchDirectory;// not good
            IsWorking = false;
        });
    }

    #endregion

    #region == Other commands == 

    [RelayCommand]

    public void Escape()
    {
        //IsChangePasswordDialogShow = false;

        //IsSettingsShow = false; //Don't.

        IsQueueFindVisible = false;
        IsArtistFindVisible = false;
        IsFilesFindVisible = false;
        IsAlbumFindVisible = false;

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

            //SelectedAlbum = hoge;
            if (hoge is not null)
            {
                //GoToAlbumsPageAndScrollTo?.Invoke(this, Albums.IndexOf(SelectedAlbum));
                GoToAlbumPage(hoge);
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (item.AlbumArtist != asdf) continue;

                //SelectedAlbum = item;
                if (item is not null)
                {
                    //GoToAlbumsPageAndScrollTo?.Invoke(this, Albums.IndexOf(SelectedAlbum));
                    GoToAlbumPage(item);
                }
                break;
            }
        }
    }

    private void GoToAlbumPage(AlbumEx album)
    {
        if (album is null) return;
        Dispatcher.UIThread.Post(async () =>
        {
            IsWorking = true;
            await Task.Yield();
            await Task.Delay(100);
            // Not good
            //SelectedAlbum = null;
            //CurrentPage = App.GetService<AlbumPage>(); // not good
            //await Task.Yield();
            //await Task.Delay(100);
            SelectedAlbum = album;
            await Task.Yield();
            await Task.Delay(100);
            IsNavigationViewMenuOpen = true; // Need this somehow.
            await Task.Yield();
            await Task.Delay(100);
            _mainMenuItems.AlbumsDirectory.Selected = true;
            IsWorking = false;
        });

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

        GoToArtistPage(item);
    }

    private void GoToArtistPage(AlbumArtist artist)
    {
        if (artist is null) return;
        Dispatcher.UIThread.Post(async () =>
        {
            IsWorking = true;
            await Task.Yield();
            await Task.Delay(100);
            SelectedAlbumArtist = null; //Test.
            CurrentPage = App.GetService<ArtistPage>(); //Needed this...for the strange selection issue.
            await Task.Yield(); 
            await Task.Delay(100);
            SelectedAlbumArtist = artist;
            await Task.Yield(); 
            await Task.Delay(100); 
            IsNavigationViewMenuOpen = true; // Need this somehow.
            await Task.Yield(); 
            await Task.Delay(100);
            _mainMenuItems.ArtistsDirectory.Selected = true;
            IsWorking = false;
        });

        /*
        SelectedAlbumArtist = artist;
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

            //SelectedAlbum = hoge;
            if (hoge is not null)
            {
                GoToAlbumPage(hoge);
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (item.AlbumArtist != asdf) continue;

                //SelectedAlbum = item;
                if (item is not null)
                {
                    GoToAlbumPage(item);
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

        GoToArtistPage(item);
    }

    #endregion


    #endregion
}
