using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using GdiHook.Interop;
using GdiHook.Services;
using GdiHook.DTO;

namespace GdiHook.ViewModel
{
    public class MainWindowViewModel: ViewModel
    {
        private IEnumerable<KeyValuePair<IntPtr, string>> _Windows = new List<KeyValuePair<IntPtr, string>>();
        public IEnumerable<KeyValuePair<IntPtr, string>> Windows
        {
            get => _Windows;
            set
            {
                _Windows = value;
                NotifyPropertyChanged();
            }
        }
        private KeyValuePair<IntPtr, string> _Selected;
        public KeyValuePair<IntPtr, string> Selected
        {
            get => _Selected;
            set
            {
                _Selected = value;
                NotifyPropertyChanged();
            }
        }

        private ControlElement _Control;
        public ControlElement Control
        {
            get => _Control;
            set
            {
                _Control = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand Refresh { get; }
        public ICommand Capture { get; }

        public MainWindowViewModel(HookСontrols hookСontrols)
        {
            Refresh = new Command((o) => {
                Windows = WindowHelper.GetOpenWindows().Select(hwnd => new KeyValuePair<IntPtr, string>(hwnd, WindowHelper.Title(hwnd)));
            }, (a) => true);
            Capture = new Command((o) => {
                Control = hookСontrols.Grab(Selected.Key);
                _ = Control;
            }, (a) => Selected.Key != IntPtr.Zero);
            Refresh.Execute(null);
        }
    }
}
