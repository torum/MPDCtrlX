using Avalonia.Threading;
using System.ComponentModel;

namespace MPDCtrlX.Core.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) 
            return;
        Dispatcher.UIThread.Post(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
        
    }
}
