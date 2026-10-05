using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MPDCtrlX.Models;

/// <summary>
/// Base class for Treeview Node.
/// </summary>
public class NodeTree : Node
{
    public bool Selected
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public bool Expanded
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public NodeTree? Parent
    {
        get;

        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    }

    public ObservableCollection<NodeTree> Children
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = [];

    protected NodeTree(string name) : base(name)
    {
    }
}
