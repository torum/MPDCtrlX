using MPDCtrlX.Models;

namespace MPDCtrlX.Services.Contracts;

public interface IMpcBinaryService
{
    //AlbumImage AlbumCover { get; }

    void MpdBinaryConnectionDisconnect();
    Task<bool> MpdBinaryConnectionStart(string host, int port, string password, CancellationToken cancellationToken = default);
    Task<CommandImageResult> MpdQueryAlbumArt(string uri, bool isUsingReadpicture);
}
