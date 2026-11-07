using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System.Collections.Specialized;

namespace MPDCtrlX.Views.Controls;

public class VirtualizingUniformPanel : VirtualizingPanel
{
    private readonly Dictionary<int, Control> _realizedContainers = new();
    private readonly Dictionary<Control, int> _realizedIndices = new();
    private Rect _viewport;

    private List<object> _lastVisibleDataItems = new();
    public event EventHandler<List<object>>? VisibleItemsChanged;

    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<VirtualizingUniformPanel, double>(nameof(ItemWidth), defaultValue: 100);

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<VirtualizingUniformPanel, double>(nameof(ItemHeight), defaultValue: 100);

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public VirtualizingUniformPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        ItemWidthProperty.Changed.AddClassHandler<VirtualizingUniformPanel>((panel, _) => panel.InvalidateMeasure());
        ItemHeightProperty.Changed.AddClassHandler<VirtualizingUniformPanel>((panel, _) => panel.InvalidateMeasure());
    }

    protected override void OnItemsChanged(IReadOnlyList<object?>? items, NotifyCollectionChangedEventArgs e)
    {
        if (items is not null)
        {
            base.OnItemsChanged(items, e);
        }

        InvalidateMeasure();
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (_viewport == e.EffectiveViewport)
            return;

        _viewport = e.EffectiveViewport;
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var items = Items;
        if (items == null || ItemContainerGenerator == null)
            return new Size(0, 0);

        if (items.Count == 0)
        {
            RecycleAll();
            return new Size(0, 0);
        }

        double baseWidth = double.IsInfinity(availableSize.Width) ? ItemWidth : availableSize.Width;
        int columns = Math.Max(1, (int)(baseWidth / ItemWidth));
        int rows = (int)Math.Ceiling((double)items.Count / columns);

        double dynamicItemWidth = baseWidth / columns;

        double totalWidth = baseWidth;
        double totalHeight = rows * ItemHeight;

        //var viewport = _viewport.Height > 0 ? _viewport : new Rect(0, 0, baseWidth, availableSize.Height);
        var viewport = _viewport;
        int firstVisibleRow = (int)Math.Floor(viewport.Top / ItemHeight);
        int lastVisibleRow = (int)Math.Ceiling(viewport.Bottom / ItemHeight);

        firstVisibleRow = Math.Max(0, firstVisibleRow);
        lastVisibleRow = Math.Min(rows - 1, lastVisibleRow);

        int firstVisibleIndex = firstVisibleRow * columns;
        int lastVisibleIndex = Math.Min(items.Count - 1, ((lastVisibleRow + 1) * columns) - 1);

        List<int> toRecycle = new();
        foreach (var (index, container) in _realizedContainers)
        {
            if (index < firstVisibleIndex || index > lastVisibleIndex || index >= items.Count || !ReferenceEquals(container.DataContext, items[index]))
            {
                toRecycle.Add(index);
            }
        }

        foreach (var index in toRecycle)
        {
            var container = _realizedContainers[index];
            ItemContainerGenerator.ClearItemContainer(container);
            RemoveInternalChildRange(Children.IndexOf(container), 1);
            _realizedIndices.Remove(container);
            _realizedContainers.Remove(index);
        }

        for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
        {
            if (!_realizedContainers.ContainsKey(i))
            {
                object? item = items[i];

                if (ItemContainerGenerator.NeedsContainer(item, i, out object? recycleKey))
                {
                    var container = ItemContainerGenerator.CreateContainer(item, i, recycleKey);
                    ItemContainerGenerator.PrepareItemContainer(container, item, i);

                    _realizedContainers[i] = container;
                    _realizedIndices[container] = i;

                    AddInternalChild(container);
                    ItemContainerGenerator.ItemContainerPrepared(container, item, i);
                }
            }

            if (_realizedContainers.TryGetValue(i, out var visualContainer))
            {
                visualContainer.Measure(new Size(dynamicItemWidth, ItemHeight));
            }
        }

        var currentVisibleDataItems = new List<object>();
        foreach (var child in Children)
        {
            if (child.DataContext is object dataItem)
            {
                currentVisibleDataItems.Add(dataItem);
            }
        }

        // Only fire if the composition of visible items actually changed (avoids spamming during minor jitter)
        if (!_lastVisibleDataItems.SequenceEqual(currentVisibleDataItems))
        {
            _lastVisibleDataItems = currentVisibleDataItems;
            // Run on a separate callback stack slice to avoid interrupting active layout measure logic
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                VisibleItemsChanged?.Invoke(this, currentVisibleDataItems);
            });
        }

        return new Size(totalWidth, totalHeight);
    }

    private void RecycleAll()
    {
        foreach (var container in _realizedContainers.Values)
        {
            ItemContainerGenerator?.ClearItemContainer(container);
            //RemoveInternalChildRange(Children.IndexOf(container), 1);
            var childIndex = Children.IndexOf(container);
            if (childIndex >= 0)
            {
                RemoveInternalChildRange(childIndex, 1);
            }
        }

        _realizedContainers.Clear();
        _realizedIndices.Clear();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var items = Items;
        if (items == null || items.Count == 0) return finalSize;

        int columns = Math.Max(1, (int)(finalSize.Width / ItemWidth));
        double dynamicItemWidth = finalSize.Width / columns;

        foreach (var kvp in _realizedContainers)
        {
            int index = kvp.Key;
            var child = kvp.Value;

            int row = index / columns;
            int col = index % columns;

            double x = col * dynamicItemWidth;
            double y = row * ItemHeight;

            if (child is Layoutable layoutableChild)
            {
                layoutableChild.Width = dynamicItemWidth;
                layoutableChild.Height = ItemHeight;
                layoutableChild.HorizontalAlignment = HorizontalAlignment.Stretch;
                layoutableChild.VerticalAlignment = VerticalAlignment.Stretch;
            }

            child.Arrange(new Rect(x, y, dynamicItemWidth, ItemHeight));
        }

        return finalSize;
    }

    protected override IEnumerable<Control> GetRealizedContainers() => _realizedContainers.Values;
    protected override Control? ContainerFromIndex(int index) => _realizedContainers.TryGetValue(index, out var c) ? c : null;
    protected override int IndexFromContainer(Control container) => _realizedIndices.TryGetValue(container, out int idx) ? idx : -1;

    protected override Control? ScrollIntoView(int index)
    {

        if (index < 0 || index >= (Items?.Count ?? 0)) return null;

        if (_realizedContainers.TryGetValue(index, out var container))
        {
            container.BringIntoView();
            return container;
        }

        var scrollViewer = this.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault();

        if (scrollViewer is null || ItemWidth <= 0 || ItemHeight <= 0)
        {
            return null;
        }

        var width = Bounds.Width > 0 ? Bounds.Width : scrollViewer.Viewport.Width;
        if (width <= 0)
        {
            return null;
        }

        var columns = Math.Max(1, (int)(width / ItemWidth));
        var itemTop = (index / columns) * ItemHeight;
        var itemBottom = itemTop + ItemHeight;
        var viewportTop = scrollViewer.Offset.Y;
        var viewportBottom = viewportTop + scrollViewer.Viewport.Height;

        var targetOffset = itemTop < viewportTop
            ? itemTop
            : itemBottom > viewportBottom
                ? itemBottom - scrollViewer.Viewport.Height
                : viewportTop;

        var maxOffset = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        scrollViewer.Offset = new Vector(
            scrollViewer.Offset.X,
            Math.Clamp(targetOffset, 0, maxOffset));

        return null;
    }

    //protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap) => from;

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var items = Items;
        if (items is null || items.Count == 0)
        {
            return null;
        }

        // Find the realized container that owns the focused element.
        int current = -1;
        for (Visual? v = from as Visual; v is not null; v = v.GetVisualParent())
        {
            if (v is Control c && _realizedIndices.TryGetValue(c, out int idx))
            {
                current = idx;
                break;
            }
        }

        if (current < 0)
        {
            return ContainerFromIndex(direction == NavigationDirection.Last ? items.Count - 1 : 0);
        }

        int columns = Math.Max(1, (int)(Bounds.Width / ItemWidth));

        int target = direction switch
        {
            NavigationDirection.Left or NavigationDirection.Previous => current - 1,
            NavigationDirection.Right or NavigationDirection.Next => current + 1,
            NavigationDirection.Up => current - columns,
            NavigationDirection.Down => current + columns,
            NavigationDirection.First => 0,
            NavigationDirection.Last => items.Count - 1,
            _ => -1
        };

        if (target < 0 || target >= items.Count || target == current)
        {
            return null;
        }

        // Scrolls the target into view and returns its container.
        return ScrollIntoView(target) ?? ContainerFromIndex(target);
    }
}
