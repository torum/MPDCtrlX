using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MPDCtrlX.Core.Models;

public class Playlist : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    private string _lastModified = string.Empty;
    public string LastModified
    {
        get
        {
            return _lastModified;
        }
        set
        {
            if (_lastModified == value)
                return;

            _lastModified = value;

            OnPropertyChanged(nameof(LastModified));
            OnPropertyChanged(nameof(LastModifiedFormated));
        }
    }

    public string LastModifiedFormated
    {
        get
        {
            DateTime _lastModifiedDateTime = default; //new DateTime(1998,04,30)

            if (!string.IsNullOrEmpty(_lastModified))
            {
                try
                {
                    _lastModifiedDateTime = DateTime.Parse(_lastModified, null, System.Globalization.DateTimeStyles.RoundtripKind);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Wrong LastModified timestamp format. " + _lastModified);
                }
            }

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return _lastModifiedDateTime.ToString(culture);
        }
    }
}
