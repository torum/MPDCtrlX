using FluentAvalonia.UI.Controls;
using MPDCtrlX.Models;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPDCtrlX.Services;

public class DialogService : IDialogService
{
    public record AddToDialogResult(string PlaylistName, bool AsNew);

    public DialogService()
    {

    }

    public async Task<AddToDialogResult?> ShowAddToDialog(ViewModels.MainViewModel vm)
    {
        if (vm is null)
        {
            return null;
        }

        var dialog = new ContentDialog
        {
            Title = MPDCtrlX.Properties.Resources.Dialog_Title_SelectPlaylist,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.SaveToDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        if (dialog.Content is not SaveToDialog dialogContent)
        {
            return null;
        }

        // Sort
        CultureInfo ci = CultureInfo.CurrentCulture;
        StringComparer comp = StringComparer.Create(ci, true);

        //asdf.PlaylistListBox.ItemsSource = vm?.Playlists;
        //asdf.PlaylistListBox.ItemsSource = new ObservableCollection<Playlist>(vm.Playlists.OrderBy(x => x.Name, comp));
        dialogContent.PlaylistComboBox.ItemsSource = new ObservableCollection<Playlist>(vm.Playlists.OrderBy(x => x.Name, comp));

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.Content is Views.Dialogs.SaveToDialog dlg)
        {

            if (dlg.CreateNewCheckBox.IsChecked is true)
            {
                var str = dlg.TextBoxPlaylistName.Text ?? string.Empty;

                if (!string.IsNullOrEmpty(str.Trim()))
                {
                    //vm?.QueueSaveToDialog_Execute(str.Trim());
                    return new AddToDialogResult(str.Trim(), true);
                }
            }
            else
            {
                var plselitem = dlg.PlaylistComboBox.SelectedItem;

                if (plselitem is Models.Playlist pl)
                {
                    if (!string.IsNullOrWhiteSpace(pl.Name))
                    {
                        //vm?.QueueSaveToDialog_Execute(pl.Name.Trim());
                        return new AddToDialogResult(pl.Name, false);
                    }

                }
            }
        }

        return null;
    }

    public async Task ShowProfileEditDialog(Profile slectedProfile)
    {
        if (slectedProfile is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "EDIT",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.ProfileDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        if (dialog.Content is not Views.Dialogs.ProfileDialog dlg)
        {
            return;
        }

        dlg.SetProfile(slectedProfile);

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var ret = dlg.GetProfile();
        if (ret is not null)
        {
            slectedProfile = ret;
        }
    }

    public async Task<Profile?> ShowProfileAddDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "ADD",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.ProfileDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        if (dialog.Content is not Views.Dialogs.ProfileDialog dlg)
        {
            return null;
        }

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        var ret = dlg.GetProfileAsNew();
        if (ret is not null)
        {
            return ret;
        }

        return null;
    }
}
