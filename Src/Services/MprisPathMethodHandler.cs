using Avalonia.Media.Imaging;
using MPDCtrlX.Common;
using MPDCtrlX.Models;
using MPDCtrlX.Services.Contracts;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Tmds.DBus.Protocol;
using Path = System.IO.Path;

namespace MPDCtrlX.Services;

internal sealed class MprisPathMethodHandler(MprisPlayerController player, IMpcService mpc) : IPathMethodHandler
{
    private const string RootInterface = "org.mpris.MediaPlayer2";
    private const string PlayerInterface = "org.mpris.MediaPlayer2.Player";
    private const string PropertiesInterface = "org.freedesktop.DBus.Properties";
    private const string PlayerPath = "/org/mpris/MediaPlayer2";
    private const string IntrospectionXml = """
<node>
  <interface name="org.mpris.MediaPlayer2">
    <method name="Raise"/>
    <method name="Quit"/>
    <property name="CanQuit" type="b" access="read"/>
    <property name="CanRaise" type="b" access="read"/>
    <property name="HasTrackList" type="b" access="read"/>
    <property name="Identity" type="s" access="read"/>
    <property name="SupportedUriSchemes" type="as" access="read"/>
    <property name="SupportedMimeTypes" type="as" access="read"/>
  </interface>
  <interface name="org.mpris.MediaPlayer2.Player">
    <method name="Next"/>
    <method name="Previous"/>
    <method name="Pause"/>
    <method name="PlayPause"/>
    <method name="Stop"/>
    <method name="Play"/>
    <method name="Seek">
      <arg name="Offset" type="x" direction="in"/>
    </method>
    <method name="SetPosition">
      <arg name="TrackId" type="o" direction="in"/>
      <arg name="Position" type="x" direction="in"/>
    </method>
    <method name="OpenUri">
      <arg name="Uri" type="s" direction="in"/>
    </method>
    <signal name="Seeked">
      <arg name="Position" type="x"/>
    </signal>
    <property name="PlaybackStatus" type="s" access="read"/>
    <property name="LoopStatus" type="s" access="readwrite"/>
    <property name="Rate" type="d" access="read"/>
    <property name="Shuffle" type="b" access="readwrite"/>
    <property name="Metadata" type="a{sv}" access="read"/>
    <property name="Volume" type="d" access="readwrite"/>
    <property name="Position" type="x" access="read"/>
    <property name="CanGoNext" type="b" access="read"/>
    <property name="CanGoPrevious" type="b" access="read"/>
    <property name="CanPlay" type="b" access="read"/>
    <property name="CanPause" type="b" access="read"/>
    <property name="CanSeek" type="b" access="read"/>
    <property name="CanControl" type="b" access="read"/>
  </interface>
  <interface name="org.freedesktop.DBus.Properties">
    <method name="Get">
      <arg name="interface_name" type="s" direction="in"/>
      <arg name="property_name" type="s" direction="in"/>
      <arg name="value" type="v" direction="out"/>
    </method>
    <method name="GetAll">
      <arg name="interface_name" type="s" direction="in"/>
      <arg name="properties" type="a{sv}" direction="out"/>
    </method>
    <method name="Set">
      <arg name="interface_name" type="s" direction="in"/>
      <arg name="property_name" type="s" direction="in"/>
      <arg name="value" type="v" direction="in"/>
    </method>
    <signal name="PropertiesChanged">
      <arg name="interface_name" type="s"/>
      <arg name="changed_properties" type="a{sv}"/>
      <arg name="invalidated_properties" type="as"/>
    </signal>
  </interface>
</node>
""";
    private readonly SemaphoreSlim _albumArtLock = new(1, 1);
    private DBusConnection? _connection;
    private string? _albumArtSongFile;
    private string? _albumArtUri = string.Empty;

    // interface implementation
    public string Path => PlayerPath;
    // interface implementation
    public bool HandlesChildPaths => false;

    public ValueTask HandleMethodAsync(MethodContext context)
    {
        try
        {
            if (context.IsDBusIntrospectRequest)
            {
                using var writer = context.CreateReplyWriter("s");
                writer.WriteString(IntrospectionXml);
                context.Reply(writer.CreateMessage());
                return ValueTask.CompletedTask;
            }

            var request = context.Request;
            var member = request.MemberAsString;

            if (member is null)
            {
                context.ReplyUnknownMethodError();
                return ValueTask.CompletedTask;
            }

            if (request.InterfaceAsString == PropertiesInterface)
            {
                return HandleProperties(context, member);
            }

            if (request.InterfaceAsString == PlayerInterface)
            {
                return HandlePlayerMethod(context, member);
            }

            if (request.InterfaceAsString == RootInterface)
            {
                return HandleRootMethod(context, member);
            }

            context.ReplyUnknownMethodError();
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            context.ReplyError("org.freedesktop.DBus.Error.Failed", ex.Message);
        }

        return ValueTask.CompletedTask;
    }

    public async Task RefreshAlbumArtAsync()
    {
        await _albumArtLock.WaitAsync();

        try
        {
            var song = mpc.MpdCurrentSong;

            var songFile = song?.File;

            if (string.IsNullOrWhiteSpace(songFile))
            {
                if (_albumArtSongFile is not null)
                {
                    _albumArtSongFile = null;
                    _albumArtUri = null;
                    PublishMetadataChanged();
                }

                return;
            }

            if (string.Equals(_albumArtSongFile, songFile, StringComparison.Ordinal))
            {
                return;
            }

            _albumArtSongFile = songFile;
            _albumArtUri = null;
            //PublishMetadataChanged();

            Debug.WriteLine("Song changed. Refreshing album art...");

            var strArtist = song?.AlbumArtist.Trim();
            if (string.IsNullOrEmpty(strArtist))
            {
                strArtist = song?.Artist.Trim();
                if (string.IsNullOrEmpty(strArtist))
                {
                    strArtist = "Unknown Artist";
                }
            }
            strArtist = PathSanitizer.SanitizeFilename(strArtist);

            var strAlbum = song?.Album ?? string.Empty;
            if (string.IsNullOrEmpty(strAlbum))
            {
                strAlbum = "Unknown Album";
            }
            else
            {
                strAlbum = PathSanitizer.SanitizeFilename(strAlbum);
            }

            string strDirPath = System.IO.Path.Combine(App.AlbumCoverCacheFolder, strArtist);
            string filePath = System.IO.Path.Combine(App.AlbumCoverCacheFolder, System.IO.Path.Combine(strArtist, strAlbum)) + ".bmp";
            if (File.Exists(filePath))
            {
                _albumArtUri = new Uri(System.IO.Path.GetFullPath(filePath)).AbsoluteUri;
            }

            PublishMetadataChanged();

            return;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MPRIS album-art error: {ex}");
        }
        finally
        {
            _albumArtLock.Release();
        }
    }

    private static ValueTask HandleRootMethod(MethodContext context, string member)
    {
        if (member is "Raise" or "Quit")
        {
            using var writer = context.CreateReplyWriter(string.Empty);
            context.Reply(writer.CreateMessage());
            return ValueTask.CompletedTask;
        }

        return ReplyUnknownMethod(context);
    }

    private ValueTask HandlePlayerMethod(MethodContext context, string member)
    {
        var reader = context.Request.GetBodyReader();

        return member switch
        {
            "Play" => ReplyCommand(context, player.PlayAsync()),
            "Pause" => ReplyCommand(context, player.PauseAsync()),
            "PlayPause" => ReplyCommand(context, player.PlayPauseAsync()),
            "Stop" => ReplyCommand(context, player.StopAsync()),
            "Next" => ReplyCommand(context, player.NextAsync()),
            "Previous" => ReplyCommand(context, player.PreviousAsync()),
            //"Seek" => ReplyCommand(context, player.SeekAsync(reader.ReadInt64())),
            "Seek" => HandleSeek(context, reader),
            "SetPosition" => HandleSetPosition(context, reader),
            "OpenUri" => ReplyNotSupported(context),
            _ => ReplyUnknownMethod(context)
        };
    }

    private ValueTask HandleSetPosition(MethodContext context, Reader reader)
    {
        var trackId = reader.ReadObjectPathAsString();
        var position = reader.ReadInt64();

        if (trackId != GetTrackId())
        {
            context.ReplyError(
                "org.freedesktop.DBus.Error.InvalidArgs",
                "SetPosition was requested for a track that is no longer current.");
            return ValueTask.CompletedTask;
        }

        //return ReplyCommand(context, player.SetPositionAsync(position));
        var targetSeconds = Math.Clamp(
    position / 1_000_000.0,
    0.0,
    mpc.MpdStatus.MpdSongTime);

        return ReplySeekCommand(
            context,
            player.SetPositionAsync(position),
            (long)(targetSeconds * 1_000_000));
    }

    private ValueTask HandleProperties(MethodContext context, string member)
    {
        var reader = context.Request.GetBodyReader();

        switch (member)
        {
            case "Get":
                {
                    var interfaceName = reader.ReadString();
                    var propertyName = reader.ReadString();

                    if (!TryGetProperty(interfaceName, propertyName, out var value))
                    {
                        context.ReplyError(
                            "org.freedesktop.DBus.Error.UnknownProperty",
                            $"Unknown MPRIS property: {propertyName}");
                        return ValueTask.CompletedTask;
                    }

                    using var writer = context.CreateReplyWriter("v");
                    writer.WriteVariant(value);
                    context.Reply(writer.CreateMessage());
                    return ValueTask.CompletedTask;
                }

            case "GetAll":
                {
                    var interfaceName = reader.ReadString();
                    var properties = GetProperties(interfaceName);

                    using var writer = context.CreateReplyWriter("a{sv}");
                    writer.WriteDictionary(properties);
                    context.Reply(writer.CreateMessage());
                    return ValueTask.CompletedTask;
                }

            case "Set":
                {
                    var interfaceName = reader.ReadString();
                    var propertyName = reader.ReadString();
                    var value = reader.ReadVariantValue();
                    /*
                    if (interfaceName != PlayerInterface || propertyName != "Volume")
                    {
                        context.ReplyError(
                            "org.freedesktop.DBus.Error.PropertyReadOnly",
                            $"MPRIS property is not writable: {propertyName}");
                        return ValueTask.CompletedTask;
                    }

                    return ReplyCommand(context, player.SetVolumeAsync(value.GetDouble()));
                    */
                    if (interfaceName != PlayerInterface)
                    {
                        return ReplyReadOnlyProperty(context, propertyName);
                    }

                    return propertyName switch
                    {
                        "Volume" => ReplyCommand(context, player.SetVolumeAsync(value.GetDouble())),
                        "Shuffle" => ReplyCommand(context, player.SetShuffleAsync(value.GetBool())),
                        "LoopStatus" => ReplyCommand(context, player.SetLoopStatusAsync(value.GetString())),
                        _ => ReplyReadOnlyProperty(context, propertyName)
                    };
                }

            default:
                return ReplyUnknownMethod(context);
        }
    }

    private static ValueTask ReplyReadOnlyProperty(MethodContext context, string propertyName)
    {
        context.ReplyError(
            "org.freedesktop.DBus.Error.PropertyReadOnly",
            $"MPRIS property is not writable: {propertyName}");
        return ValueTask.CompletedTask;
    }

    private Dictionary<string, VariantValue> GetProperties(string interfaceName)
    {
        if (interfaceName == RootInterface)
        {
            return new Dictionary<string, VariantValue>
            {
                ["CanQuit"] = VariantValue.Bool(false),
                ["CanRaise"] = VariantValue.Bool(false),
                ["HasTrackList"] = VariantValue.Bool(false),
                ["Identity"] = VariantValue.String("MPDCtrlX"),
                ["SupportedUriSchemes"] = VariantValue.Array(Array.Empty<string>()),
                ["SupportedMimeTypes"] = VariantValue.Array(Array.Empty<string>())
            };
        }

        if (interfaceName != PlayerInterface)
        {
            return [];
        }

        return new Dictionary<string, VariantValue>
        {
            ["PlaybackStatus"] = VariantValue.String(player.PlaybackStatus),
            //["LoopStatus"] = VariantValue.String(mpc.MpdStatus.MpdRepeat ? "Playlist" : "None"),
            ["LoopStatus"] = VariantValue.String(player.LoopStatus),
            ["Rate"] = VariantValue.Double(1.0),
            //["Shuffle"] = VariantValue.Bool(mpc.MpdStatus.MpdRandom),
            ["Shuffle"] = VariantValue.Bool(player.Shuffle),
            ["Metadata"] = BuildMetadata(),
            ["Volume"] = VariantValue.Double(player.Volume),
            ["Position"] = VariantValue.Int64(
                (long)(mpc.MpdStatus.MpdSongElapsed * 1_000_000)),
            ["CanGoNext"] = VariantValue.Bool(player.CanGoNext),
            ["CanGoPrevious"] = VariantValue.Bool(player.CanGoPrevious),
            ["CanPlay"] = VariantValue.Bool(true),
            ["CanPause"] = VariantValue.Bool(true),
            ["CanSeek"] = VariantValue.Bool(player.CanSeek),
            ["CanControl"] = VariantValue.Bool(true)
        };
    }

    private bool TryGetProperty(string interfaceName,string propertyName,out VariantValue value)
    {
        var properties = GetProperties(interfaceName);
        return properties.TryGetValue(propertyName, out value);
    }

    private VariantValue BuildMetadata()
    {
        var song = mpc.MpdCurrentSong;
        var metadata = new Dict<string, VariantValue>
        {
            ["mpris:trackid"] = VariantValue.ObjectPath(new ObjectPath(GetTrackId())),
            ["mpris:length"] = VariantValue.Int64(
                (long)(mpc.MpdStatus.MpdSongTime * 1_000_000))
        };

        if (song is not null)
        {
            metadata["xesam:title"] = VariantValue.String(song.Title);
            metadata["xesam:album"] = VariantValue.String(song.Album);
            metadata["xesam:artist"] = VariantValue.Array(
                string.IsNullOrWhiteSpace(song.Artist)
                    ? Array.Empty<string>()
                    : [song.Artist]);
        }
        else
        {
            metadata["xesam:title"] = VariantValue.String(string.Empty);
            metadata["xesam:album"] = VariantValue.String(string.Empty);
            metadata["xesam:artist"] = VariantValue.Array(Array.Empty<string>());
        }

        if (!string.IsNullOrEmpty(_albumArtUri))
        {
            metadata["mpris:artUrl"] = VariantValue.String(_albumArtUri);
        }

        return metadata;
    }

    private string GetTrackId()
    {
        var id = mpc.MpdCurrentSong?.Id;
        return string.IsNullOrEmpty(id)
            ? "/org/mpris/MediaPlayer2/TrackList/NoTrack"
            : $"/org/mpris/MediaPlayer2/track/{id}";
    }

    private static ValueTask ReplyCommand(MethodContext context, Task<MPDCtrlX.Models.CommandResult> command) => new(ReplyCommandAsync(context, command));

    private static async Task ReplyCommandAsync(MethodContext context, Task<MPDCtrlX.Models.CommandResult> command)
    {
        var result = await command.ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            context.ReplyError(
                "org.freedesktop.DBus.Error.Failed",
                result.ErrorMessage);
            return;
        }

        using var writer = context.CreateReplyWriter(string.Empty);
        context.Reply(writer.CreateMessage());
    }

    private static ValueTask ReplyNotSupported(MethodContext context)
    {
        context.ReplyError(
            "org.freedesktop.DBus.Error.NotSupported",
            "Opening URIs is not supported.");
        return ValueTask.CompletedTask;
    }

    private static ValueTask ReplyUnknownMethod(MethodContext context)
    {
        context.ReplyUnknownMethodError();
        return ValueTask.CompletedTask;
    }

    private ValueTask HandleSeek(MethodContext context, Reader reader)
    {
        var offset = reader.ReadInt64();
        var targetSeconds = Math.Clamp(
            mpc.MpdStatus.MpdSongElapsed + offset / 1_000_000.0,
            0.0,
            mpc.MpdStatus.MpdSongTime);

        return ReplySeekCommand(
            context,
            player.SeekAsync(offset),
            (long)(targetSeconds * 1_000_000));
    }

    private ValueTask ReplySeekCommand(MethodContext context, Task<MPDCtrlX.Models.CommandResult> command, long positionMicroseconds) => new(ReplySeekCommandAsync(context, command, positionMicroseconds));

    private async Task ReplySeekCommandAsync(MethodContext context, Task<MPDCtrlX.Models.CommandResult> command, long positionMicroseconds)
    {
        var result = await command.ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            context.ReplyError(
                "org.freedesktop.DBus.Error.Failed",
                result.ErrorMessage);
            return;
        }

        EmitSeeked(positionMicroseconds);

        using var writer = context.CreateReplyWriter(string.Empty);
        context.Reply(writer.CreateMessage());
    }

    public void AttachConnection(DBusConnection connection) => _connection = connection;

    public void DetachConnection() => _connection = null;

    public void PublishPlayerPropertiesChanged()
    {
        var allProperties = GetProperties(PlayerInterface);
        var changed = new Dictionary<string, VariantValue>();

        foreach (var name in new[]
        {
            "PlaybackStatus", "LoopStatus", "Shuffle", "Volume", "Metadata", "CanGoNext", "CanGoPrevious", "CanSeek", "CanControl"
        })
        {
            if (allProperties.TryGetValue(name, out var value))
            {
                changed[name] = value;
            }
        }

        EmitPropertiesChanged(PlayerInterface, changed);
    }

    public void PublishMetadataChanged() => EmitPropertiesChanged(
            PlayerInterface,
            new Dictionary<string, VariantValue>
            {
                ["Metadata"] = BuildMetadata()
            });

    private void EmitPropertiesChanged(string interfaceName,Dictionary<string, VariantValue> changed)
    {
        var connection = _connection;
        if (connection is null)
        {
            return;
        }

        try
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteSignalHeader(
                null,
                PlayerPath,
                PropertiesInterface,
                "PropertiesChanged",
                "sa{sv}as");
            writer.WriteString(interfaceName);
            writer.WriteDictionary(changed);
            writer.WriteArray(Array.Empty<string>());
            connection.TrySendMessage(writer.CreateMessage());
        }
        catch
        {
            // Signal failures should not disrupt MPD event processing.
        }
    }

    private void EmitSeeked(long positionMicroseconds)
    {
        var connection = _connection;
        if (connection is null)
        {
            return;
        }

        try
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteSignalHeader(
                null,
                PlayerPath,
                PlayerInterface,
                "Seeked",
                "x");
            writer.WriteInt64(positionMicroseconds);
            connection.TrySendMessage(writer.CreateMessage());
        }
        catch
        {
            // Signal failures should not disrupt playback.
        }
    }


}