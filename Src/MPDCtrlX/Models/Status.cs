namespace MPDCtrlX.Core.Models;

public class Status
{
    public enum MpdPlayState
    {
        Play, Pause, Stop
    };

    public MpdPlayState MpdState { get; set; }

    public int MpdVolume { get; set; } = 20;

    public bool MpdVolumeIsReturned { get; set; }

    public bool MpdVolumeIsSet { get; set; } 

    public bool MpdRepeat { get; set; }

    public bool MpdRandom { get; set; }
    public bool MpdConsume { get; set; }

    public bool MpdSingle { get; set; }

    public string MpdSongID { get; set; } = "";

    public double MpdSongTime { get; set; } 

    public double MpdSongElapsed { get; set; } 
    public string MpdError { get; set; } = "";

    public void Reset()
    {
        MpdVolume = 20;
        MpdVolumeIsSet = false;
        MpdVolumeIsReturned = false;
        MpdRepeat = false;
        MpdRandom = false;
        MpdConsume = false;
        MpdSongID = "";
        MpdSongTime = 0;
        MpdSongElapsed = 0;
        MpdError = "";
    }
}
