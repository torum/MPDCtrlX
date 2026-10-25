using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views.Controls;
using System.Diagnostics;

namespace MPDCtrlX.Views;

internal sealed partial class QueuePage : UserControl
{
    private bool _isHeaderWidthInitialized;

    #region == Drag and Drop ==
    private readonly DataFormat<SongInfoEx> _nodeFormat = DataFormat.CreateInProcessFormat<SongInfoEx>("SongInfoExFormat");
    private InsertionLineAdorner? _insertionLine;
    private AdornerLayer? _adornerLayer;
    private Point _pointerPressedPoint;
    private bool _isPressed;
    private bool _isPressedAndMoved;
    private bool _isDropAbove;
    #endregion

    // parameterless constructor for XAML Previewer
    public QueuePage()
    {
        var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        this.Loaded += QueuePage_Loaded;
        this.Unloaded += QueuePage_Unloaded;

        this.QueueListBox.AddHandler(InputElement.PointerPressedEvent, OnItemPointerPressed, RoutingStrategies.Tunnel);
    }

    #region == Drag and Drop ==

    private async void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (!vm.QueueListviewMoveCanExecute())
        {
            return;
        }

        if (e.Source is not Control control || control.DataContext is not SongInfoEx item)
        {
            //Debug.WriteLine($"Not DataContext is PlaylistItem: {e.Source}");
            return;
        }

        //Debug.WriteLine($"PointerPressed on {item.Text}");
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        _pointerPressedPoint = e.GetPosition(this);
        _isPressed = true;
        _isPressedAndMoved = false;

        while (_isPressed)
        {
            await Task.Delay(100);
            if (_isPressedAndMoved)
                break;
        }

        if (!_isPressedAndMoved)
        {
            //Debug.WriteLine("Pointer did not move");
            return;
        }

        var transferItem = DataTransferItem.Create(_nodeFormat, item);

        // 2. Prepare the v12 DataTransfer container payload
        var dragData = new DataTransfer();

        dragData.Add(transferItem);

        // Initiate the asynchronous native drag frame
        // This will block synchronously until the user releases the cursor
        var result = await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Move);

        // Logic to remove node from its old parent if necessary

        if (result == DragDropEffects.Move)
        {
            e.Handled = true;
        }

        Debug.WriteLine(result == DragDropEffects.Move
            ? "DoDragDropAsync result = Succuses (Move)"
            : "DoDragDropAsync result = Fail (None)");

        _isPressedAndMoved = false;
        _isPressed = false;

        RemoveInsertionLine();
    }

    private async void ListBox_DragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
            return;
        }

        var draggedNode = e.DataTransfer.TryGetValue(_nodeFormat);

        if (draggedNode is SongInfoEx)
        {
            var pos = e.GetPosition(QueueListBox);
            AutoScroll(QueueListBox, pos);

            var currentPosition = e.GetPosition(QueueListBox);
            var visualTarget = QueueListBox.GetVisualAt(currentPosition);
            var targetListBoxItem = visualTarget?.FindAncestorOfType<ListBoxItem>();

            if (targetListBoxItem != null)
            {
                e.DragEffects = DragDropEffects.Move;
                e.Handled = true;
                /*
                // Determine whether to draw the line above or below the item
                var positionInItem = e.GetPosition(targetListBoxItem);
                double itemHeight = targetListBoxItem.Bounds.Height;
                double itemCenterY = itemHeight / 2;
                double insertionY;
                if (positionInItem.Y < itemCenterY)
                {
                    // Cursor is on the top half -> Intent is ABOVE
                    // Map local 0 back to the ListBox coordinate space
                    insertionY = targetListBoxItem.Bounds.Y;
                    _isDropAbove = true;
                }
                else
                {
                    // Cursor is on the bottom half -> Intent is BELOW
                    // Map local bottom back to the ListBox coordinate space
                    insertionY = targetListBoxItem.Bounds.Y + itemHeight;
                    _isDropAbove = false;
                }
                */

                var positionInItem = e.GetPosition(targetListBoxItem);
                var itemHeight = targetListBoxItem.Bounds.Height;
                _isDropAbove = positionInItem.Y < itemHeight / 2;

                var itemEdge = new Point(0, _isDropAbove ? 0 : itemHeight);
                var edgeInListBox = targetListBoxItem.TranslatePoint(itemEdge, QueueListBox);

                if (edgeInListBox is null)
                {
                    RemoveInsertionLine();
                    return;
                }

                ShowInsertionLine(edgeInListBox.Value.Y);

                /*
                
                // Get the bounding box of the item being hovered over
                var itemBounds = targetListBoxItem.Bounds;
                double itemCenterY = itemBounds.Y + (itemBounds.Height / 2);

                double insertionY;
                if (currentPosition.Y < itemCenterY)
                {
                    // Cursor is on the top half -> Draw line at the top edge of this item
                    insertionY = itemBounds.Y;
                }
                else
                {
                    // Cursor is on the bottom half -> Draw line at the bottom edge of this item
                    insertionY = itemBounds.Y + itemBounds.Height;
                }
                */

                // Trigger your adorner update method
                //ShowInsertionLine(insertionY);

                return;
            }
        }

        // Fallback: If they hover over empty space or an invalid format, reject the drop
        e.DragEffects = DragDropEffects.None;

        RemoveInsertionLine();
    }

    private async void ListBox_Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (e.DataTransfer.Formats.Contains(_nodeFormat))
        {
            var draggedNode = e.DataTransfer.TryGetValue(_nodeFormat);

            if (draggedNode is not null)
            {
                if ((e.Source as Control)?.DataContext is not SongInfoEx targetNode)
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }

                if (!ReferenceEquals(draggedNode, targetNode))
                {
                    // Success.

                    //Debug.WriteLine($"DraggedNode = {draggedNode.Text}, TargetNode = {targetNode?.Text}");

                    await vm.ReorderQueueSongs(draggedNode, targetNode, _isDropAbove);

                    // Report success.
                    e.DragEffects = DragDropEffects.Move;
                    e.Handled = true;

                    RemoveInsertionLine();
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;

                    RemoveInsertionLine();
                }
            }

            e.Handled = true;
            return;
        }

        /*
        // Handle file drops from outside of the application.
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.TryGetFiles();

            if (files != null)
            {
                // Convert IStorageItem to local paths (for Desktop use cases)
                var localPaths = files
                    .Select(file => file.TryGetLocalPath())
                    .Where(path => path != null)
                    ;//.ToArray();

                await vm.FilesDroped(localPaths);
            }

            e.Handled = true;
            return;
        }
        */
    }

    private void ShowInsertionLine(double y)
    {
        if (_adornerLayer == null)
            return;

        if (_insertionLine == null)
        {
            _insertionLine = new InsertionLineAdorner();
            AdornerLayer.SetAdornedElement(_insertionLine, this.QueueListBox);
            _adornerLayer.Children.Add(_insertionLine);
        }

        _insertionLine.Y = y;
        _insertionLine.InvalidateVisual();
    }

    private void RemoveInsertionLine()
    {
        if (_adornerLayer == null)
            return;

        if (_insertionLine != null)
        {
            _adornerLayer.Children.Remove(_insertionLine);
            _insertionLine = null;
        }
    }

    /*
    private async void ListBox_FileDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.TryGetFiles();

            if (files != null)
            {
                // 3. Convert IStorageItem to local paths (for Desktop use cases)
                var localPaths = files
                    .Select(file => file.TryGetLocalPath())
                    .Where(path => path != null)
                    .ToArray();

                if (localPaths.Length > 0)
                {
                    vm.AddFilesToPlaylist(localPaths!);
                }
            }

            e.Handled = true;
        }
    }
    */

    private static void AutoScroll(ListBox listBox, Avalonia.Point pointerPos)
    {
        const double scrollMargin = 30;

        if (listBox.Scroll != null)
        {
            if (pointerPos.Y < scrollMargin)
                listBox.Scroll.Offset = new Avalonia.Vector(listBox.Scroll.Offset.X, listBox.Scroll.Offset.Y - 10);

            else if (pointerPos.Y > listBox.Bounds.Height - scrollMargin)
                listBox.Scroll.Offset = new Avalonia.Vector(listBox.Scroll.Offset.X, listBox.Scroll.Offset.Y + 10);
        }
    }

    private void QueueListBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;
        /*
        if (vm.SelectedItem == null)
            return;

        bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (!ctrl)
            return;

        if (e.Key == Key.Up)
        {
            vm.MoveItemUp(vm.SelectedItem);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            vm.MoveItemDown(vm.SelectedItem);
            e.Handled = true;
        }
        */
    }

    private void QueueListBox_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPressed)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);

        // Calculate the distance moved
        var deltaX = Math.Abs(currentPoint.X - _pointerPressedPoint.X);
        var deltaY = Math.Abs(currentPoint.Y - _pointerPressedPoint.Y);

        if (deltaX > 4 || deltaY > 4)
        {
            // The pointer HAS moved beyond a tiny jitter threshold
            //Debug.WriteLine("Pointer is moved");
            _isPressedAndMoved = true;
            _isPressed = false;
        }
    }

    private void QueueListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isPressedAndMoved = false;
        _isPressed = false;
    }

    private void QueueListBox_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPressedAndMoved = false;
        _isPressed = false;
    }

    #endregion

    private void QueuePage_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }
        vm.ScrollIntoView += OnScrollIntoView;
        vm.ScrollIntoViewAndSelect += OnScrollIntoViewAndSelect;
        vm.QueueHeaderVisibilityChanged += this.OnQueueHeaderVisibilityChanged;

        _adornerLayer = AdornerLayer.GetAdornerLayer(QueueListBox);
    }

    private void QueuePage_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }
        vm.ScrollIntoView -= OnScrollIntoView;
        vm.ScrollIntoViewAndSelect -= OnScrollIntoViewAndSelect;
        vm.QueueHeaderVisibilityChanged -= this.OnQueueHeaderVisibilityChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel)
        {
            return;
        }

        if (_isHeaderWidthInitialized)
        {
            // Everytime page is changed come back, this loaded is called. So.
            return;
        }

        _isHeaderWidthInitialized = true;

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//vm.QueueColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        //this.Column1X.Width = 80;//vm.QueueColumnHeaderPositionWidth
        this.Column1.Width = 80;
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;

        /*
        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.QueueColumnHeaderTitleWidth);
        this.Column3.IsVisible = true;
        this.Column3X.Width = vm.QueueColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;
        */
        UpdateColumHeaders();
    }

    // ListBox dummy header Visibility option change.
    private void OnQueueHeaderVisibilityChanged(object? sender, System.EventArgs e)
    {
        UpdateColumHeaders();
    }

    private void UpdateColumHeaders()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Title

        // Time
        if (vm.IsQueueColumnHeaderTimeVisible)
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(70);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(0);
        }

        // Artist
        if (vm.IsQueueColumnHeaderArtistVisible)
        {
            this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(0);
        }

        // Album
        if (vm.IsQueueColumnHeaderAlbumVisible)
        {
            this.DummyHeader.ColumnDefinitions[5].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[5].Width = new GridLength(0);
        }

        // Disc
        if (vm.IsQueueColumnHeaderDiscVisible)
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
        }

        // Track
        if (vm.IsQueueColumnHeaderTrackVisible)
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(0);
        }

        // Genre
        if (vm.IsQueueColumnHeaderGenreVisible)
        {
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
        }

        // Last modified
        if (vm.IsQueueColumnHeaderLastModifiedVisible)
        {
            this.DummyHeader.ColumnDefinitions[9].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[9].Width = new GridLength(0);
        }
    }

    private async void OnScrollIntoView(object? sender, int ind)
    {
        try
        {
            await Task.Yield();
            await Task.Delay(100); // Wait for UI to update

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (this.QueueListBox is ListBox lb)
                {
                    // TODO: This does not work when Queue page is not IsAttachedToVisualTree()...
                    lb.ScrollIntoView(ind);
                }
            });
        }
        catch (Exception ex)
        {
            // Scrolling is cosmetic. Never let it crash the app.
            System.Diagnostics.Debug.WriteLine($"Exception @OnScrollIntoView {ex}");
        }
    }

    private async void OnScrollIntoViewAndSelect(object? sender, int ind)
    {
        try
        {
            await Task.Delay(100); // Need to wait for UI to update

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (this.QueueListBox is ListBox lb)
                {
                    lb.ScrollIntoView(ind);
                    //Avalonia's ListBox doesn't clear selection outside of viewort. So we need to set SelectedIndex.
                    lb.SelectedIndex = ind;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception @OnScrollIntoViewAndSelect {ex}");
        }
    }

    // Double click to play
    private void QueueListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (e.Source is Visual visual && visual.FindAncestorOfType<ListBoxItem>() is { } item)
        {
            item.IsSelected = true;

            if (DataContext is not MainViewModel vm) return;

            if (!vm.PlayCommand.CanExecute(null)) return;

            vm.QueueListviewEnterKeyCommand.Execute(null);
        }
    }

    // FilterBox
    private void FilterQueueListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if ((this.FilterQueueListBox.SelectedItem is SongInfoEx song) && (this.QueueListBox is ListBox lb))
        {
            lb.ScrollIntoView(song.Index);

            lb.SelectedIndex = song.Index;
            //song.IsSelected = true; // This is not working because Avalonia's ListBox doesn't clear selection outside of viewort. So we need to set SelectedIndex instead.
        }
    }

    // Sets focus in textbox
    private async void OnQueueFindTextBoxSetFocus(object? sender, System.EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await Task.Yield(); // Ensure the UI has processed the opened event

                if (FilterQueueQueryTextBox.IsVisible && FilterQueueQueryTextBox.Focusable)
                {
                    this.FilterQueueQueryTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception @OnQueueFindTextBoxSetFocus {ex}");
            }
        }, DispatcherPriority.Render);


        //await Task.Yield();
        //await Task.Delay(50); // Need to wait for UI to update
        /*
        Dispatcher.UIThread.Post(() =>
        {
            if (this.TglButtonQueueFilter is ToggleButton tb)
            {
                if (tb.IsChecked == true)
                {
                    this.FilterQueueQueryTextBox.Focus();
                }
            }
        });
        */
    }

    // Sets focus in textbox when clicked.
    private void TglButtonQueueFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.TglButtonQueueFilter is ToggleButton tb)
        {
            if (tb.IsChecked == true)
            {
                this.FilterQueueQueryTextBox.Focus();
            }
        }
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.QueueListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.NewSize.Width < 340)
        {
            vm.IsQueueColumnHeaderTimeVisible = false;
            vm.IsQueueColumnHeaderArtistVisible = false;
            vm.IsQueueColumnHeaderAlbumVisible = false;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
            this.FindFilterButtonStackPanel.IsVisible = false;
            this.SortToggleSplitButton.IsVisible = false;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = false;
            vm.IsQueueColumnHeaderAlbumVisible = false;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
            this.FindFilterButtonStackPanel.IsVisible = false;
            this.SortToggleSplitButton.IsVisible = false;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }

        UpdateColumHeaders();
    }

    private void Page_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            TglButtonQueueFilter.IsChecked = false;
        }
    }

    private void FilterQueuePopup_Opened(object? sender, System.EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Yield(); // Ensure the UI has processed the opened event
            if (FilterQueuePopup.Focusable)
            {
                FilterQueuePopup.Focus();
            }

            if (FilterQueueQueryTextBox.Focusable)
            {
                this.FilterQueueQueryTextBox.Focus();
            }
        }, DispatcherPriority.Render);
    }
}