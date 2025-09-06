using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MPDCtrlX.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MPDCtrlX.Common;

public class ItemsControlStackPanelBehaviors
{
    // The target is ScrollViewer(with ItemsControl inside), and the value is an IEnumerable of objects.
    public static readonly AttachedProperty<IEnumerable<object>> VisibleItemsProperty =
        AvaloniaProperty.RegisterAttached<ItemsControlStackPanelBehaviors, ScrollViewer, IEnumerable<object>>("VisibleItems");

    public static IEnumerable<object> GetVisibleItems(ScrollViewer element)
    {
        return element.GetValue(VisibleItemsProperty);
    }

    public static void SetVisibleItems(ScrollViewer element, IEnumerable<object> value)
    {
        element.SetValue(VisibleItemsProperty, value);
    }

    static ItemsControlStackPanelBehaviors()
    {
        //Debug.WriteLine("ItemsControlStackPanelBehaviors");

        // This class handler is triggered whenever a ListBox is loaded into the visual tree.
        ScrollViewer.LoadedEvent.AddClassHandler<ScrollViewer>((sender, e) =>
        {
            //var listBox = sender;
            var scrollViewer = sender;

            if (scrollViewer.Tag != null)
            {
                //Debug.WriteLine("(listBox.Tag != null) @ItemsControlStackPanelBehaviors");
                return;
            }

            scrollViewer.Tag = "LoadedEvent_ItemsControlStackPanelBehaviors";

            //var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            var listBox = scrollViewer.GetVisualDescendants().OfType<ItemsControl>().FirstOrDefault();
            if (listBox is null)
            {
                return;
            }

            //var wrapPanel = listBox.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();
            var wrapPanel = listBox.GetVisualDescendants().OfType<StackPanel>().FirstOrDefault();
            if (wrapPanel is null)
            {
                return;
            }

            if ((scrollViewer != null) && (wrapPanel != null))
            {
                // Subscribe to the scroll event to update the visible items.
                scrollViewer.ScrollChanged += (s, args) => UpdateVisibleItems(listBox, scrollViewer, wrapPanel);
                // .. size changed event too.
                scrollViewer.SizeChanged += (s, args) => UpdateVisibleItems(listBox, scrollViewer, wrapPanel);

                // Call it once initially to set the property.
                UpdateVisibleItems(listBox, scrollViewer, wrapPanel);
            }
        });
    }

    private static void UpdateVisibleItems(ItemsControl listBox, ScrollViewer scrollViewer, StackPanel wrapPanel)
    {
        //var _scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null) return;

        //var _wrapPanel = listBox.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();
        if (wrapPanel == null) return;

        var viewportRect = new Rect(new Point(scrollViewer.Offset.X, scrollViewer.Offset.Y), scrollViewer.Viewport);

        var visibleObjects = wrapPanel.Children.Where(child => child.Bounds.Intersects(viewportRect)).ToList();

        var visibleItems = new List<object>();

        foreach (var itemContainer in visibleObjects)
        {
            var dataItem = itemContainer.DataContext;
            if (dataItem != null)
            {
                visibleItems.Add(dataItem);
            }
        }

        // Set the new value of the attached property.
        listBox.SetValue(VisibleItemsProperty, visibleItems);
    }
}
