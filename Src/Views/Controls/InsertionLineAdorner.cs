using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace MPDCtrlX.Views.Controls;

public class InsertionLineAdorner : Control
{
    public double Y { get; set; }

    public override void Render(DrawingContext context)
    {
        var pen = new Pen(Brushes.DeepSkyBlue, 2);
        context.DrawLine(pen, new Avalonia.Point(0, Y), new Avalonia.Point(Bounds.Width, Y));
    }
}

