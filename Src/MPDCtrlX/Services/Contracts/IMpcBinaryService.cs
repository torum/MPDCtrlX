using MPDCtrlX.Core.Models;
using System.Threading.Tasks;

namespace MPDCtrlX.Core.Services.Contracts;

public interface IMpcBinaryService
{
    //AlbumImage AlbumCover { get; }

    void MpdBinaryConnectionDisconnect();
    Task<bool> MpdBinaryConnectionStart(string host, int port, string password);
    Task<CommandImageResult> MpdQueryAlbumArt(string uri, bool isUsingReadpicture);
}
