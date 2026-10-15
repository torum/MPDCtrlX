using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace MPDCtrlX.Models;

public class Configuration
{
    public class WindowSettings
    {
        public double Width { get; set; } = 780;
        public double Height { get; set; } = 600;
        public int Left { get; set; } = 100;
        public int Top { get; set; } = 100;
        public WindowState WindowState { get; set; } = WindowState.Normal;

        public double RestoreWidth { get; set; } = 780;
        public double RestoreHeight { get; set; } = 600;
        public int RestoreLeft { get; set; } = 100;
        public int RestoreTop { get; set; } = 100;
    }

    public WindowSettings MainWindow { get; set; } = new WindowSettings();

    public Theme ThemeSetting { get; set; } = new Theme(ThemeType.System);

    public List<Profile> Profiles { get; set; } = [];

    public Profile? CurrentProfile { get; set; }

    public bool IsAutoScrollToNowPlaying { get; set; } = true;

    public bool IsArtistSortWithoutThePrefix { get; set; } = true;

    public bool IsAlbumSortWithoutThePrefix { get; set; } = true;

    public bool IsUpdateOnStartup { get; set; } = true;

    public bool IsShowDebugWindow { get; set; }

    public bool IsSaveLog { get; set; }

    public bool IsDownloadAlbumArt { get; set; } = true;

    public bool IsDownloadAlbumArtEmbeddedUsingReadPicture { get; set; } = true;

    public bool IsForceSetVolumeExplicitly { get; set; }


    public bool IsNavigationViewMenuOpen { get; set; }
}
