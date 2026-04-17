using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MPDCtrlX.Core.Models;

public class Playlist : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    public string LastModified
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(LastModifiedFormated));
        }
    } = string.Empty;

    public string LastModifiedFormated
    {
        get
        {
            DateTime lastModifiedDateTime = default; //new DateTime(1998,04,30)

            if (!string.IsNullOrEmpty(LastModified))
            {
                try
                {
                    lastModifiedDateTime = DateTime.Parse(LastModified, null, System.Globalization.DateTimeStyles.RoundtripKind);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Wrong LastModified timestamp format. " + LastModified);
                }
            }

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return lastModifiedDateTime.ToString(culture);
        }
    }
}
