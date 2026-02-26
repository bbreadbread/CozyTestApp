using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Safety_Wheel.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeObj;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action execute)
        {
            _executeObj = _ => execute();
        }

        public RelayCommand(Action<object> execute)
        {
            _executeObj = execute;
        }

        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
        {
            _executeObj?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

}
