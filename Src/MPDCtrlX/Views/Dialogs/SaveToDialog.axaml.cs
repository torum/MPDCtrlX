using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace MPDCtrlX.Core.Views.Dialogs;

public partial class SaveToDialog : UserControl
{
    public SaveToDialog()
    {
        InitializeComponent();
    }

    private void InputField_OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        // We will set the focus into our input field just after it got attached to the visual tree.
        if (sender is InputElement inputElement)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                inputElement.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
    }


    private void CheckBox_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (CreateNewCheckBox.IsChecked is true)
        {
            TextBoxPlaylistName.IsVisible = true;
            PlaylistComboBox.IsEnabled = false;
        }
        else
        {
            TextBoxPlaylistName.IsVisible = false;
            PlaylistComboBox.IsEnabled = true;
        }
    }
}