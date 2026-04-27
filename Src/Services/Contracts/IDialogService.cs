using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System.Threading.Tasks;
using static MPDCtrlX.Services.DialogService;

namespace MPDCtrlX.Services.Contracts;

public interface IDialogService
{
    Task ShowKeybindingsDialog();

    Task<AddToDialogResult?> ShowAddToDialog(MainViewModel vm);

    Task<Profile?> ShowProfileEditDialog(Profile slectedProfile);

    Task<Profile?> ShowProfileAddDialog();
}
