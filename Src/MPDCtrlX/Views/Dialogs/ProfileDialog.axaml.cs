using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MPDCtrlX.Core.Models;
using System.Diagnostics;

namespace MPDCtrlX.Core.Views.Dialogs;

public partial class ProfileDialog : UserControl
{
    private Profile? _pro;

    public ProfileDialog()
    {
        InitializeComponent();
    }

    public void SetProfile(Profile pro)
    {
        if (pro is null)
        {
            return;
        }

        _pro = pro;

        this.HostTextBox.Text = pro.Host;
        this.PortTextBox.Text = pro.Port.ToString();
        this.PasswordBox.Text = pro.Password;
        this.IsDefaultCheckBox.IsChecked = pro.IsDefault;
        //this.IsRememberCheckBox.IsChecked = true;
    }

    public Profile? GetProfile() 
    {
        if (_pro is null)
        {
            return null;
        }

        /*
    // Validate Host input.
    if (Host == "")
    {
        //SetError(nameof(Host), "Error: Host must be specified."); //TODO: translate
        NotifyPropertyChanged(nameof(Host));
        return;
    }
    else
    {
        if (Host == "localhost")
        {
            Host = "127.0.0.1";
        }

        IPAddress? ipAddress = null;
        try
        {
            ipAddress = IPAddress.Parse(Host);
            if (ipAddress is not null)
            {
                //ClearError(nameof(Host));
            }
        }
        catch
        {
            //System.FormatException
            //SetError(nameof(Host), "Error: Invalid address format."); //TODO: translate

            return;
        }
    }
    */

        _pro.Host = this.HostTextBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(this.PortTextBox.Text))
        {
            _pro.Port = (int)6600;
        }
        else
        {
            try
            {
                _pro.Port = int.Parse(this.PortTextBox.Text);
            }
            catch 
            {
                _pro.Port = (int)6600;
            }
        }

        _pro.Password = this.PasswordBox.Text ?? string.Empty;

        _pro.IsDefault = this.IsDefaultCheckBox.IsChecked ?? false;

        _pro.Name = _pro.Host + ":" + _pro.Port.ToString();

        return _pro;
    }


    public Profile? GetProfileAsNew()
    {
        Profile pro = new()
        {
            Host = this.HostTextBox.Text ?? string.Empty
        };

        if (string.IsNullOrEmpty(this.PortTextBox.Text))
        {
            pro.Port = (int)6600;
        }
        else
        {
            try
            {
                pro.Port = int.Parse(this.PortTextBox.Text);
            }
            catch
            {
                pro.Port = (int)6600;
            }
        }

        pro.Password = this.PasswordBox.Text ?? string.Empty;

        pro.IsDefault = this.IsDefaultCheckBox.IsChecked ?? false;

        pro.Name = pro.Host + ":" + pro.Port.ToString();

        return pro;
    }
}