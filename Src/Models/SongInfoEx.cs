namespace MPDCtrlX.Models;

/// <summary>
/// Song class with some extra info. Extends SongInfo. (for queue)
/// </summary>
public class SongInfoEx : SongInfo
{
    // Queue specific

    public string Id { get; set; } = string.Empty;

    public string Pos
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

    public bool IsPlaying
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

    public bool IsAlbumCoverNeedsUpdate
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
}
