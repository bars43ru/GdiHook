using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GdiHook.ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<bool> ClosingRequested;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Application.Current?.Dispatcher.InvokeAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected void RequestClosing(bool dialogResult = false)
        {
            ClosingRequested?.Invoke(this, dialogResult);
        }
    }
}
