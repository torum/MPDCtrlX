using CommunityToolkit.Mvvm.ComponentModel;

namespace MPDCtrlX.Models;

/// <summary>
/// Generic song file class. (for listall)
/// </summary>
public class SongFile : ObservableObject
{
    public string File { get; set; } = string.Empty;
}

