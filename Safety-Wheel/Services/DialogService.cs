using CozyTest.ForShellWindow;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.CuratorVM;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CozyTest.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "");
        bool ShowConfirmation(string message, string title = "");
        void ShowWindow<T>(BaseViewModel viewModel) where T : MetroWindow;
        void CloseWindow(BaseViewModel viewModel);

    }

    public class DialogService : IDialogService
    {
        private readonly Dictionary<BaseViewModel, MetroWindow> _openWindows = new();
        public void ShowWindow<T>(BaseViewModel viewModel) where T : MetroWindow
        {
            var window = (T)Activator.CreateInstance(typeof(T), viewModel);
            _openWindows[viewModel] = window;
            window.Closed += (s, e) => _openWindows.Remove(viewModel);
            window.Show();
        }

        public void ShowMessage(string message, string title = "")
        {
            MessageBox.Show(message, title);
        }
        public void CloseWindow(BaseViewModel viewModel)
        {
            if (_openWindows.TryGetValue(viewModel, out var window))
            {
                window.Close();
            }
        }
        public bool ShowConfirmation(string message, string title = "")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}
