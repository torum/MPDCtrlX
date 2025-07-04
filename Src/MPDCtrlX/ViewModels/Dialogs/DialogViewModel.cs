using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPDCtrlX.ViewModels.Dialogs
{
    public class DialogViewModel : ViewModelBase
    {
        /*
        public IRelayCommand CloseCommand { get; }
        public bool CloseCommand_CanExecute()
        {
            return true;
        }
        public void CloseCommand_ExecuteAsync()
        {
            CloseDialogEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? CloseDialogEvent;
        */
        public DialogViewModel() 
        {
            //CloseCommand = new RelayCommand(CloseCommand_ExecuteAsync, CloseCommand_CanExecute);
        }


    }
}
