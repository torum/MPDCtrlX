using Avalonia.Controls;
using Avalonia.Threading;
using MPDCtrlX.Models;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MPDCtrlX.Services;

/*
 * WinUI3/AvaloniaUI does not provide a way to get the restore bounds of a window like WPF does when it is maximized or minimized.
 * So, we need to manually track the restore bounds of the window when it is in the restored state.
 * AppWindow_Changed(WinUI3)/Window_PositionChanged(AvaloniaUI) and Window_SizeChanged event handlers are called when the window state changes, such as when the window is maximized, minimized, or restored.
 * Make sure to update the restore bounds when the window is in the restored state.
 * Also, use Window_Activated event handler to update the restore bounds when the window is activated, because sometimes the window state change events are not fired when the window is activated.
 * This way, even if the window is closed while maximized or minimized, we can still save the restore bounds to the configuration file.
        WinRestoreWidth = (int)mw.RestoreWidth;
        WinRestoreHeight = (int)mw.RestoreHeight;
        WinRestoreLeft = mw.RestoreLeft;
        WinRestoreTop = mw.RestoreTop;
 */

internal sealed class ConfigurationService : IConfigurationService
{
    private bool _isInitialized;
    private bool _isConfigLoaded;
    private string? _configFilePath = string.Empty;
    private XDocument? _xdoc;

    public ConfigurationService()
    {
        _isConfigLoaded = false;
        CurrentConfiguration = new Models.Configuration();
    }

    public Models.Configuration CurrentConfiguration { get; private set; }

    public void Initialize(string configFilePath)
    {
        _configFilePath = configFilePath;
        _isConfigLoaded = false;
        _xdoc = null;
        _isInitialized = true;

        CurrentConfiguration = new Models.Configuration();
    }

    public async Task<Models.Configuration> LoadConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }

        var config = new Models.Configuration();

        if (_isConfigLoaded)
        {
            Debug.WriteLine("LoadConfigurationAsync() - Already loaded, returning current configuration.");
            return CurrentConfiguration ?? config;
        }
        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }
        if (!System.IO.File.Exists(_configFilePath))
        {
            Debug.WriteLine($"Configuration file not found: {_configFilePath}");
            // Set _isConfigLoaded to true so that SaveConfigurationAsync() can create the file with default settings.
            _isConfigLoaded = true;
            return config;
        }

        Debug.WriteLine("LoadConfigurationAsync()");

        try
        {
            _xdoc = XDocument.Load(App.AppConfigFilePath);
            if (_xdoc.Root is null)
            {
                return config;
            }

            #region == Window setting ==
            
            // Main Window element
            var mainWindow = _xdoc.Root.Element("MainWindow");
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
                        config.MainWindow.Top = wY;
                    }
                }

                hoge = mainWindow.Attribute("left");
                if (hoge is not null)
                {
                    //w.Left = double.Parse(hoge.Value);
                    //wX = int.Parse(hoge.Value);
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.MainWindow.Left = wX;
                    }
                }
                //w.Position = new PixelPoint(wX, wY);

                hoge = mainWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        //w.Height = double.Parse(hoge.Value);
                        config.MainWindow.Height = double.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        //w.Width = double.Parse(hoge.Value);
                        config.MainWindow.Width = double.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        //w.WindowState = WindowState.Maximized;
                        // Since there is no restorebounds in AvaloniaUI.....
                        config.MainWindow.WindowState = WindowState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        //w.WindowState = WindowState.Normal;
                        config.MainWindow.WindowState = WindowState.Normal;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        //w.WindowState = WindowState.Normal;
                        config.MainWindow.WindowState = WindowState.Normal;
                    }
                }
            }

            #endregion

            #region == Theme ==

            var thm = _xdoc.Root.Element("Theme");
            if (thm is not null)
            {
                var hoge = thm.Attribute("ThemeName");
                if (hoge is not null)
                {
                    if ((hoge.Value == "Dark") || hoge.Value == "Light" || hoge.Value == "System")
                    {
                        Theme? theme = Models.ThemeTypeLabel.GetThemes().FirstOrDefault(x => x.Name == hoge.Value);
                        if (theme is not null)
                        {
                            config.ThemeSetting = theme;
                        }
                    }
                }
            }

            #endregion

            #region == Options ==

            var opts = _xdoc.Root.Element("Options");
            if (opts is not null)
            {
                var hoge = opts.Attribute("AutoScrollToNowPlaying");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsAutoScrollToNowPlaying = true;
                    }
                    else
                    {
                        config.IsAutoScrollToNowPlaying = false;
                    }
                }

                hoge = opts.Attribute("ArtistSortWithoutThePrefix");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsArtistSortWithoutThePrefix = true;
                    }
                    else
                    {
                        config.IsArtistSortWithoutThePrefix = false;
                    }
                }

                hoge = opts.Attribute("AlbumSortWithoutThePrefix");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsAlbumSortWithoutThePrefix = true;
                    }
                    else
                    {
                        config.IsAlbumSortWithoutThePrefix = false;
                    }
                }

                hoge = opts.Attribute("UpdateOnStartup");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsUpdateOnStartup = true;
                    }
                    else
                    {
                        config.IsUpdateOnStartup = false;
                    }
                }

                hoge = opts.Attribute("ShowDebugWindow");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsShowDebugWindow = true;

                    }
                    else
                    {
                        config.IsShowDebugWindow = false;
                    }
                }

                hoge = opts.Attribute("SaveLog");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsSaveLog = true;

                    }
                    else
                    {
                        config.IsSaveLog = false;
                    }
                }

                hoge = opts.Attribute("DownloadAlbumArt");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsDownloadAlbumArt = true;

                    }
                    else
                    {
                        config.IsDownloadAlbumArt = false;
                    }
                }

                hoge = opts.Attribute("DownloadAlbumArtEmbeddedUsingReadPicture");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsDownloadAlbumArtEmbeddedUsingReadPicture = true;

                    }
                    else
                    {
                        config.IsDownloadAlbumArtEmbeddedUsingReadPicture = false;
                    }
                }

                hoge = opts.Attribute("ForceSetVolumeExplicitly");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsForceSetVolumeExplicitly = true;

                    }
                    else
                    {
                        config.IsForceSetVolumeExplicitly = false;
                    }
                }

                hoge = opts.Attribute("IsMPRISIntegrationEnabled");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsMPRISIntegrationEnabled = true;

                    }
                    else
                    {
                        config.IsMPRISIntegrationEnabled = false;
                    }
                }
            }

            #endregion

            #region == Profiles  ==

            var xProfiles = _xdoc.Root.Element("Profiles");
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
                                pro.Port = Int32.Parse(s, CultureInfo.InvariantCulture);
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
                            pro.Password = Common.EncryptDecrypt.Decrypt(s);
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
                                pro.Volume = double.Parse(s, CultureInfo.InvariantCulture);
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
                            config.CurrentProfile = pro;

                        }
                        config.Profiles.Add(pro);
                    }
                }
            }

            #endregion

            #region == Layout ==

            var lay = _xdoc.Root.Element("Layout");
            if (lay is not null)
            {
                var leftpain = lay.Element("LeftPain");
                if (leftpain is not null)
                {
                    var hoge = leftpain.Attribute("NavigationViewMenuOpen");
                    if (hoge is not null)
                    {
                        if (hoge.Value == "True")
                        {
                            config.IsNavigationViewMenuOpen = true;
                        }
                        else
                        {
                            config.IsNavigationViewMenuOpen = false;
                        }
                    }
                }
            }

            /*
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
                                MainLeftPainWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderPositionWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderNowPlayingWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderTitleWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderTimeWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        IsQueueColumnHeaderArtistVisible = false;
                                }
                            }
                            if (column.Attribute("Width") is not null)
                            {
                                var s = column.Attribute("Width")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        QueueColumnHeaderArtistWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderAlbumWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderDiscWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderTrackWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderGenreWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        QueueColumnHeaderLastModifiedWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        FilesColumnHeaderTitleWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        FilesColumnHeaderFilePathWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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

                            if (column.Attribute("Width") is not null)
                            {
                                var s = column.Attribute("Width")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        PlaylistColumnHeaderPositionWidth = Double.Parse(s, CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        PlaylistColumnHeaderPositionWidth = 60;
                                    }
                                }
                            }
                        }
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
                                        PlaylistColumnHeaderTitleWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderTimeWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderArtistWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderAlbumWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderDiscWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderTrackWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderGenreWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        PlaylistColumnHeaderLastModifiedWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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

                            if (column.Attribute("Width") is not null)
                            {
                                var s = column.Attribute("Width")?.Value;
                                if (!string.IsNullOrEmpty(s))
                                {
                                    try
                                    {
                                        SearchColumnHeaderPositionWidth = Double.Parse(s, CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        SearchColumnHeaderPositionWidth = 60;
                                    }
                                }
                            }
                        }

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
                                        SearchColumnHeaderTitleWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderTimeWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderArtistWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderAlbumWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderDiscWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderTrackWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderGenreWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
                                        SearchColumnHeaderLastModifiedWidth = Double.Parse(s, CultureInfo.InvariantCulture);
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
            */
            #endregion

            _isConfigLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading configuration: {ex}"); 
            Dispatcher.UIThread.Post(() => { App.AppendErrorLog("Error loading configuration", ex.Message); });
        }

        CurrentConfiguration = config;

        return CurrentConfiguration;
    }

    public async Task SaveConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }
        // TODO: Make sure Window and settings have been fully loaded and not overriding with empty data.
        if (!_isConfigLoaded)
        {
            // Don't return, because we want to save the configuration even if it hasn't been loaded yet, like the first time.
            //return;
        }
        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }

        Debug.WriteLine("SaveSettings");

        // TODO: Load xdoc from global variable if exists, and apply changes to it, then save it back to file.
        //_xdoc

        var config = CurrentConfiguration;

        XmlDocument doc = new();
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

        // Root Document Element
        XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
        doc.AppendChild(root);

        XmlAttribute attrs;

        #region == Window settings ==

        var w = config.MainWindow;
        //double windowWidth = 780;

        // Main Window element
        XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

        // Main Window attributes
        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == WindowState.Normal ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        if (w.WindowState == WindowState.Normal)
        {
            attrs.Value = w.Width.ToString(CultureInfo.InvariantCulture);
            //windowWidth = w.Width;
        }
        else
        {
            attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        }
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == WindowState.Normal ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == WindowState.Normal ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
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
        /*
        var lAttrs = doc.CreateAttribute("Width");
        if (IsFullyLoaded) // instead of IsFullyRendered
        {
            lAttrs.Value = windowWidth > (MainLeftPainActualWidth - 24) ? MainLeftPainActualWidth.ToString(CultureInfo.InvariantCulture) : "241";
        }
        else
        {
            lAttrs.Value = MainLeftPainWidth.ToString(CultureInfo.InvariantCulture);
        }
        leftpain.SetAttributeNode(lAttrs);
        */
        attrs = doc.CreateAttribute("NavigationViewMenuOpen");
        attrs.Value = config.IsNavigationViewMenuOpen ? "True" : "False";
        leftpain.SetAttributeNode(attrs);

        //
        lay.AppendChild(leftpain);

        #region == Header columns ==

        var headers = doc.CreateElement(string.Empty, "Headers", string.Empty);

        //
        lay.AppendChild(headers);

        #endregion

        ////
        root.AppendChild(lay);

        #endregion

        #region == Options ==

        var opts = doc.CreateElement(string.Empty, "Options", string.Empty);

        //
        attrs = doc.CreateAttribute("AutoScrollToNowPlaying");
        attrs.Value = config.IsAutoScrollToNowPlaying ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ArtistSortWithoutThePrefix");
        attrs.Value = config.IsArtistSortWithoutThePrefix ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("AlbumSortWithoutThePrefix");
        attrs.Value = config.IsAlbumSortWithoutThePrefix ? "True" : "False";
        opts.SetAttributeNode(attrs);

        // 
        attrs = doc.CreateAttribute("UpdateOnStartup");
        attrs.Value = config.IsUpdateOnStartup ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ShowDebugWindow");
        attrs.Value = config.IsShowDebugWindow ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("SaveLog");
        attrs.Value = config.IsSaveLog ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArt");
        attrs.Value = config.IsDownloadAlbumArt ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("DownloadAlbumArtEmbeddedUsingReadPicture");
        attrs.Value = config.IsDownloadAlbumArtEmbeddedUsingReadPicture ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("ForceSetVolumeExplicitly");
        attrs.Value = config.IsForceSetVolumeExplicitly ? "True" : "False";
        opts.SetAttributeNode(attrs);

        //
        attrs = doc.CreateAttribute("IsMPRISIntegrationEnabled");
        attrs.Value = config.IsMPRISIntegrationEnabled ? "True" : "False";
        opts.SetAttributeNode(attrs);

        /// 
        root.AppendChild(opts);

        #endregion

        #region == Profiles  ==

        var xProfiles = doc.CreateElement(string.Empty, "Profiles", string.Empty);

        XmlElement xProfile;
        XmlAttribute xAttrs;

        if (config.Profiles.Count == 1)
            config.Profiles[0].IsDefault = true;

        foreach (var p in config.Profiles)
        {
            xProfile = doc.CreateElement(string.Empty, "Profile", string.Empty);

            xAttrs = doc.CreateAttribute("Name");
            xAttrs.Value = p.Name;
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Host");
            xAttrs.Value = p.Host;
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Port");
            xAttrs.Value = p.Port.ToString(CultureInfo.InvariantCulture);
            xProfile.SetAttributeNode(xAttrs);

            xAttrs = doc.CreateAttribute("Password");
            xAttrs.Value = Common.EncryptDecrypt.Encrypt(p.Password);
            xProfile.SetAttributeNode(xAttrs);

            if (p.IsDefault)
            {
                xAttrs = doc.CreateAttribute("IsDefault");
                xAttrs.Value = "True";
                xProfile.SetAttributeNode(xAttrs);
            }

            xAttrs = doc.CreateAttribute("Volume");
            xAttrs.Value = p == config.CurrentProfile ? 20.ToString(CultureInfo.InvariantCulture) : p.Volume.ToString(CultureInfo.InvariantCulture);
            xProfile.SetAttributeNode(xAttrs);

            xProfiles.AppendChild(xProfile);
        }

        root.AppendChild(xProfiles);

        #endregion

        #region == Theme ==

        XmlElement thm = doc.CreateElement(string.Empty, "Theme", string.Empty);

        attrs = doc.CreateAttribute("ThemeName");
        attrs.Value = config.ThemeSetting.Name;
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

            //doc.Save(App.AppConfigFilePath);
            var temporaryPath = App.AppConfigFilePath + ".tmp";
            doc.Save(temporaryPath);
            File.Move(temporaryPath, App.AppConfigFilePath, overwrite: true);
        }
        //catch (System.IO.FileNotFoundException) { }
        catch (Exception ex)
        {
            if (config.IsSaveLog)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    App.AppendErrorLog("Exception@OnWindowClosing", ex.Message);
                });
            }
        }

        if (config.IsSaveLog)
        {
            // Save error logs.
            Dispatcher.UIThread.Post(App.SaveErrorLog);
        }

    }
}
