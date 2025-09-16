using MPDCtrlX.Models;
using MPDCtrlX.Services;
using MPDCtrlX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MPDCtrlX.Services.DialogService;

namespace MPDCtrlX.Services.Contracts;

public interface IDialogService
{
    Task<AddToDialogResult?> ShowAddToDialog(MainViewModel vm);

    Task ShowProfileEditDialog(Profile slectedProfile);

    Task<Profile?> ShowProfileAddDialog();
}
