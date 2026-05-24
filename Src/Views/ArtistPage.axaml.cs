using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;

namespace MPDCtrlX.Views;

internal sealed partial class ArtistPage : UserControl
{
    // Optional parameterless constructor for XAML Previewer
    public ArtistPage() { InitializeComponent(); }
    public ArtistPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();
    }

    private async void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (SelectedArtistAlbumsScrollViewer is null)
        {
            return;
        }

        //this.SelectedArtistAlbumsScrollViewer.ScrollToHome();
        ////DetailsPaneScrollViewer.ChangeView(0,0,1);

        await Task.Yield();
        await Task.Delay(100); // Wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            this.SelectedArtistAlbumsScrollViewer.ScrollToHome();
        });
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        if (e.NewSize.Width < 340)
        {
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
    }

    private void TglButtonArtistFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Yield(); // Ensure the UI has processed the opened event

            if (this.TglButtonArtistFilter is ToggleButton tb)
            {
                if (tb.IsChecked == true)
                {
                    if (FilterArtistQueryTextBox.Focusable)
                    {
                        this.FilterArtistQueryTextBox.Focus();
                    }
                }
            }

        }, DispatcherPriority.Render);
    }

    // FilterBox
    private void FilterArtistListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.FilterArtistListBox.SelectedItem is AlbumArtist artist)
        {
            //lb.ScrollIntoView(artist.Index);
            //lb.SelectedIndex = artist.Index;
            //song.IsSelected = true; // This is not working because Avalonia's ListBox doesn't clear selection outside of viewort. So we need to set SelectedIndex instead.
            //lb.SelectedItem = artist;
            var vm = App.GetService<MainViewModel>();
            vm?.ArtistFilterSelectCommand.Execute(artist);

        }
    }

    private void Page_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            TglButtonArtistFilter.IsChecked = false;
        }
    }

    private void FilterArtistPopup_Opened(object? sender, System.EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Yield(); // Ensure the UI has processed the opened event
            if (FilterArtistPopup.Focusable)
            {
                FilterArtistPopup.Focus();
            }

            if (FilterArtistQueryTextBox.Focusable)
            {
                this.FilterArtistQueryTextBox.Focus();
            }
        }, DispatcherPriority.Render);
    }
}