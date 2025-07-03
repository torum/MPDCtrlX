using Avalonia.Controls;
using Avalonia.Threading;
using MPDCtrlX.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MPDCtrlX.Views;

public partial class QueuePage : UserControl
{
    private readonly MainViewModel? _viewModel;

    public QueuePage()
    {
        _viewModel = App.GetService<MainViewModel>();

        DataContext = _viewModel;

        if (_viewModel != null)
        {
            _viewModel.ScrollIntoView += (sender, arg) => { this.OnScrollIntoView(arg); };
            _viewModel.ScrollIntoViewAndSelect += (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
        }

        InitializeComponent();
    }

    public void UpdateHeaderWidth()
    {
        if (_viewModel != null)
        {
            // This is a dirty workaround for AvaloniaUI.
            _viewModel.QueueColumnHeaderPositionWidth = this.Column1X.Width;
            _viewModel.QueueColumnHeaderNowPlayingWidth = this.Column2X.Width;
            _viewModel.QueueColumnHeaderTitleWidth = this.Column3X.Width;
            _viewModel.QueueColumnHeaderTimeWidth = this.Column4X.Width;
            _viewModel.QueueColumnHeaderArtistWidth = this.Column5X.Width;
            _viewModel.QueueColumnHeaderAlbumWidth = this.Column6X.Width;
            _viewModel.QueueColumnHeaderDiscWidth = this.Column7X.Width;
            _viewModel.QueueColumnHeaderTrackWidth = this.Column8X.Width;
            _viewModel.QueueColumnHeaderGenreWidth = this.Column9X.Width;
            _viewModel.QueueColumnHeaderLastModifiedWidth = this.Column10X.Width;
        }
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI.
        if (_viewModel.QueueColumnHeaderPositionWidth > 10)
        {
            this.Column1X.Width = _viewModel.QueueColumnHeaderPositionWidth;
        }
        else
        {
            this.Column1X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderNowPlayingWidth > 10)
        {
            //this.Column2X.Width = _viewModel.QueueColumnHeaderNowPlayingWidth;
        }
        else
        {
            //this.Column2X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTitleWidth > 10)
        {
            this.Column3.Width = _viewModel.QueueColumnHeaderTitleWidth;
        }
        else
        {
            this.Column3X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTimeWidth > 10)
        {
            this.Column4X.Width = _viewModel.QueueColumnHeaderTimeWidth;
        }
        else
        {
            this.Column4X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderArtistWidth > 10)
        {
            this.Column5X.Width = _viewModel.QueueColumnHeaderArtistWidth;
        }
        else
        {
            this.Column5X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderAlbumWidth > 10)
        {
            this.Column6X.Width = _viewModel.QueueColumnHeaderAlbumWidth;
        }
        else
        {
            this.Column6X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderDiscWidth > 10)
        {
            this.Column7X.Width = _viewModel.QueueColumnHeaderDiscWidth;
        }
        else
        {
            this.Column7X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTrackWidth > 10)
        {
            this.Column8X.Width = _viewModel.QueueColumnHeaderTrackWidth;
        }
        else
        {
            this.Column8X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderGenreWidth > 10)
        {
            this.Column9X.Width = _viewModel.QueueColumnHeaderGenreWidth;
        }
        else
        {
            this.Column9X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderLastModifiedWidth > 10)
        {
            this.Column10X.Width = _viewModel.QueueColumnHeaderLastModifiedWidth;
        }
        else
        {
            this.Column10X.Width = 50; // Default width if not set
        }

    }

    private async void OnScrollIntoView(int ind)
    {
        await Task.Delay(100); // Wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                //lb.AutoScrollToSelectedItem = true;
                lb.ScrollIntoView(ind);
            }
        });
    }

    private async void OnScrollIntoViewAndSelect(int ind)
    {
        await Task.Delay(1000); // Wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                lb.ScrollIntoView(ind);

                var test = _viewModel?.Queue.FirstOrDefault(x => x.IsPlaying == true);
                if (test != null)
                {
                    //lb.ScrollIntoView(test.Index);
                    test.IsSelected = true;
                }

                //lb.AutoScrollToSelectedItem = true;
            }
        });
    }

}