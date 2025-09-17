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
using System.Text;
using System.Threading.Tasks;


namespace MPDCtrlX.Common;

public class ListBoxWrapPanelBehaviors
{
    // The target is ListBox, and the value is an IEnumerable of objects.
    public static readonly AttachedProperty<IEnumerable<object>> VisibleItemsProperty =
        AvaloniaProperty.RegisterAttached<ListBoxWrapPanelBehaviors, ListBox, IEnumerable<object>>("VisibleItems");

    public static IEnumerable<object> GetVisibleItems(ListBox element)
    {
        return element.GetValue(VisibleItemsProperty);
    }

    public static void SetVisibleItems(ListBox element, IEnumerable<object> value)
    {
        element.SetValue(VisibleItemsProperty, value);
    }

    static ListBoxWrapPanelBehaviors()
    {
        // This class handler is triggered whenever a ListBox is loaded into the visual tree.
        ListBox.LoadedEvent.AddClassHandler<ListBox>((sender, e) =>
        {
            var listBox = sender;

            if (listBox.Tag != null)
            {
                //Debug.WriteLine("(listBox.Tag != null) @ListBoxWrapPanelBehaviors");
                return;
            }

            listBox.Tag = "LoadedEvent_ListBoxWrapPanelBehaviors";

            var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

            var wrapPanel = listBox.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();

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

    private static void UpdateVisibleItems(ListBox listBox, ScrollViewer scrollViewer, WrapPanel wrapPanel)
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
