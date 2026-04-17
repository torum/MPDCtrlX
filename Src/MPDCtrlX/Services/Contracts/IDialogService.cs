using MPDCtrlX.Core.Models;
using MPDCtrlX.Core.ViewModels;
using System.Threading.Tasks;
using static MPDCtrlX.Core.Services.DialogService;

namespace MPDCtrlX.Core.Services.Contracts;

public interface IDialogService
{
    Task<AddToDialogResult?> ShowAddToDialog(MainViewModel vm);

    Task<Profile?> ShowProfileEditDialog(Profile slectedProfile);

    Task<Profile?> ShowProfileAddDialog();
}
