using MPDCtrlX.Models;
using MPDCtrlX.Services.Contracts;

namespace MPDCtrlX.Services;

/// <summary>
/// MPD operations and state for the MPRIS D-Bus adapter.
/// MPRIS positions and seek offsets are expressed in microseconds.
/// </summary>
internal sealed class MprisPlayerController(IMpcService mpcService)
{
    private readonly IMpcService _mpc = mpcService;

    public string PlaybackStatus => _mpc.MpdStatus.MpdState switch
    {
        Status.MpdPlayState.Play => "Playing",
        Status.MpdPlayState.Pause => "Paused",
        _ => "Stopped"
    };

    public double Volume => Math.Clamp(_mpc.MpdStatus.MpdVolume / 100.0, 0.0, 1.0);

    public bool CanControlVolume => _mpc.Commands.Contains("setvol", StringComparer.OrdinalIgnoreCase);

    public bool CanGoNext => _mpc.Commands.Contains("next", StringComparer.OrdinalIgnoreCase);

    public bool CanGoPrevious => _mpc.Commands.Contains("previous", StringComparer.OrdinalIgnoreCase);

    public bool CanSeek => _mpc.Commands.Contains("seekid", StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_mpc.MpdStatus.MpdSongID);

    public Task<CommandResult> PlayAsync() => _mpc.MpdPlaybackResume(_mpc.MpdStatus.MpdVolume);

    public Task<CommandResult> PauseAsync() => _mpc.MpdStatus.MpdState == Status.MpdPlayState.Play
            ? _mpc.MpdPlaybackPause()
            : Task.FromResult(new CommandResult());

    public Task<CommandResult> PlayPauseAsync() => _mpc.MpdStatus.MpdState == Status.MpdPlayState.Play
            ? _mpc.MpdPlaybackPause()
            : _mpc.MpdPlaybackResume(_mpc.MpdStatus.MpdVolume);

    public Task<CommandResult> StopAsync() => _mpc.MpdPlaybackStop();

    public Task<CommandResult> NextAsync() => _mpc.MpdPlaybackNext(_mpc.MpdStatus.MpdVolume);

    public Task<CommandResult> PreviousAsync() => _mpc.MpdPlaybackPrev(_mpc.MpdStatus.MpdVolume);

    public Task<CommandResult> SetVolumeAsync(double mprisVolume)
    {
        if (!CanControlVolume)
        {
            //return Task.FromResult(new CommandResult());
            return Task.FromResult(new CommandResult
            {
                IsSuccess = false,
                ErrorMessage = "MPD does not support setvol."
            });
        }

        var normalized = Math.Clamp(mprisVolume, 0.0, 1.0);
        var mpdVolume = (int)Math.Round( normalized * 100.0, MidpointRounding.AwayFromZero);

        return _mpc.MpdSetVolume(mpdVolume);
    }

    /// <summary>Applies an MPRIS Seek offset, in microseconds.</summary>
    public Task<CommandResult> SeekAsync(long offsetMicroseconds)
    {
        var songId = _mpc.MpdStatus.MpdSongID;
        if (!CanSeek || string.IsNullOrEmpty(songId))
        {
            return Task.FromResult(new CommandResult());
        }

        var targetSeconds = Math.Clamp(_mpc.MpdStatus.MpdSongElapsed + offsetMicroseconds / 1_000_000.0, 0.0, _mpc.MpdStatus.MpdSongTime);

        return _mpc.MpdPlaybackSeek(songId, targetSeconds);
    }

    /// <summary>Applies an MPRIS SetPosition value, in microseconds.</summary>
    public Task<CommandResult> SetPositionAsync(long positionMicroseconds)
    {
        var songId = _mpc.MpdStatus.MpdSongID;
        if (!CanSeek || string.IsNullOrEmpty(songId))
        {
            return Task.FromResult(new CommandResult());
        }

        var targetSeconds = Math.Clamp(positionMicroseconds / 1_000_000.0, 0.0, _mpc.MpdStatus.MpdSongTime);

        return _mpc.MpdPlaybackSeek(songId, targetSeconds);
    }

    public bool Shuffle => _mpc.MpdStatus.MpdRandom;

    public string LoopStatus => !_mpc.MpdStatus.MpdRepeat ? "None" : _mpc.MpdStatus.MpdSingle ? "Track" : "Playlist";

    public Task<CommandResult> SetShuffleAsync(bool value) => _mpc.MpdSetRandom(value);

    public async Task<CommandResult> SetLoopStatusAsync(string value)
    {
        var (repeat, single) = value switch
        {
            "None" => (false, false),
            "Track" => (true, true),
            "Playlist" => (true, false),
            _ => throw new ArgumentOutOfRangeException(
                nameof(value), value, "Invalid MPRIS LoopStatus.")
        };

        if (!_mpc.Commands.Contains("repeat") || !_mpc.Commands.Contains("single"))
        {
            return new CommandResult
            {
                IsSuccess = false,
                ErrorMessage = "MPD does not support repeat and single controls."
            };
        }

        // For Track, enable repeat before single; otherwise disable single first.
        var first = repeat ? await _mpc.MpdSetRepeat(true) : await _mpc.MpdSetSingle(false);

        if (!first.IsSuccess)
        {
            return first;
        }

        return repeat ? await _mpc.MpdSetSingle(single) : await _mpc.MpdSetRepeat(false);
    }
}