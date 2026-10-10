using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPDCtrlX.Services.Contracts;
using System.Runtime.InteropServices;
using Tmds.DBus.Protocol;

namespace MPDCtrlX.Services;

internal sealed class MprisDbusHostedService(IEnumerable<IPathMethodHandler> methodHandlers, ILogger<MprisDbusHostedService> logger, IMpcService mpcService) : IHostedService, IDisposable
{
    private const string BusName = "org.mpris.MediaPlayer2.mpdcctrlx";
    private readonly IEnumerable<IPathMethodHandler> _methodHandlers = methodHandlers;
    private readonly ILogger<MprisDbusHostedService> _logger = logger;
    private readonly IMpcService _mpcService = mpcService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private DBusConnection? _connection;
    private IPathMethodHandler[] _activeHandlers = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        var handlers = _methodHandlers.ToArray();
        if (handlers.Length == 0)
        {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            _logger.LogWarning("No MPRIS D-Bus method handlers are registered.");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
            return;
        }

        DBusConnection? connection = null;

        try
        {
            //connection = new DBusConnection(DBusAddress.Session);
            var sessionAddress = DBusAddress.Session;
            if (string.IsNullOrWhiteSpace(sessionAddress))
            {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
                _logger.LogWarning("No D-Bus session bus address is available.");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
                return;
            }

            connection = new DBusConnection(sessionAddress);
            await connection.ConnectAsync();

            // Add handlers before requesting the well-known name so calls can be
            // dispatched as soon as the name becomes visible on the session bus.
            connection.AddMethodHandlers(handlers);

            if (!await connection.TryRequestNameAsync(BusName, RequestNameOptions.None))
            {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
                _logger.LogWarning("Could not acquire MPRIS bus name {BusName}.", BusName);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
                connection.Dispose();
                return;
            }

            _activeHandlers = handlers;

            foreach (var handler in handlers.OfType<MprisPathMethodHandler>())
            {
                handler.AttachConnection(connection);
            }

            //_mpcService.MpdPlayerStatusChanged += OnMpdPlayerStatusChanged;
            //_mpcService.MpdCurrentQueueChanged += OnMpdCurrentQueueChanged;
            _mpcService.MpdCurrentSongChanged += OnMpdCurrentSongChanged;
            //_mpcService.MpdIdleConnected += OnMpdIdleConnected;

            _connection = connection;
            if (_logger.IsEnabled(LogLevel.Information))
            {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
                _logger.LogInformation("Registered MPRIS service as {BusName}.", BusName);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
            }
        }
        catch (Exception ex)
        {
            connection?.Dispose();
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            _logger.LogWarning(ex, "Could not start the MPRIS D-Bus service.");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var connection = _connection;
        _connection = null;

        if (connection is null)
        {
            return;
        }

        //_mpcService.MpdPlayerStatusChanged -= OnMpdPlayerStatusChanged;
        //_mpcService.MpdCurrentQueueChanged -= OnMpdCurrentQueueChanged;
        //_mpcService.MpdIdleConnected -= OnMpdIdleConnected;
        _mpcService.MpdCurrentSongChanged += OnMpdCurrentSongChanged;

        foreach (var handler in _activeHandlers.OfType<MprisPathMethodHandler>())
        {
            handler.DetachConnection();
        }

        _activeHandlers = [];

        try
        {
            await connection.ReleaseNameAsync(BusName);
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
                _logger.LogDebug(ex, "Error releasing MPRIS bus name {BusName}.", BusName);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
            }
        }
        finally
        {
            connection.Dispose();
        }
    }

    //private void OnMpdPlayerStatusChanged(MpcService sender) => _ = RefreshMprisStateAsync();

    //private void OnMpdCurrentQueueChanged(MpcService sender) => _ = RefreshMprisStateAsync();

    private void OnMpdCurrentSongChanged(IMpcService sender) => _ = RefreshMprisStateAsync();

    //private void OnMpdIdleConnected(MpcService sender) => _ = RefreshMprisStateAsync(refreshStatus: true);

    private async Task RefreshMprisStateAsync()//bool refreshStatus = false
    {
        try
        {
            await _refreshLock.WaitAsync();
            /*
            if (refreshStatus)
            {
                var statusResult = await _mpcService.MpdQueryStatus();
                if (!statusResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Could not query initial MPD status: {Error}",
                        statusResult.ErrorMessage);
                    return;
                }
            }
            */

            try
            {
                var songId = _mpcService.MpdStatus.CurrentSongID;
                if (!string.IsNullOrEmpty(songId) && _mpcService.MpdCurrentSong?.Id != songId)
                {
                    var result = await _mpcService.MpdQueryCurrentSong();
                    if (!result.IsSuccess)
                    {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
                        _logger.LogWarning("Could not query the current MPD song: {Error}", result.ErrorMessage);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
                    }
                }

                foreach (var handler in _activeHandlers.OfType<MprisPathMethodHandler>())
                {
                    handler.PublishPlayerPropertiesChanged();
                    await handler.RefreshAlbumArtAsync();
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848 // Use the LoggerMessage delegates
            _logger.LogWarning(ex, "Failed to refresh MPRIS track state.");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
        }
    }

    public void Dispose()
    {
        _refreshLock?.Dispose();

        GC.SuppressFinalize(this);
    }

}