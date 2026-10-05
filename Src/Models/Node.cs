using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MPDCtrlX.Models;

/// <summary>
/// Base class for Treeview Node and Listview Item.
/// </summary>
abstract public class Node : ObservableObject
{
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;

            OnPropertyChanged();
        }
    }

    public string PathIcon
    {
        get;
        protected set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
        }
    } = "M20,18H4V8H20M20,6H12L10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6Z";

    protected Node(string name)
    {
        _name = name;
    }
}

