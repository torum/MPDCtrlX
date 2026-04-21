using CommunityToolkit.Mvvm.ComponentModel;

namespace MPDCtrlX.Models;

/// <summary>
/// Profile class for connection setting.
/// </summary>
public class Profile : ObservableObject
{
    public string Host
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /*
    private IPAddress _hostIpAddress;
    public IPAddress HostIpAddress
    {
        get { return _hostIpAddress; }
        set
        {
            if (_hostIpAddress == value)
                return;

            _hostIpAddress = value;
            NotifyPropertyChanged(nameof(HostIpAddress));
        }
    }
    */

    public int Port
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = 6600;

    public string Password
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string Name
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public bool IsDefault
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

    public double Volume
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 50;
}
