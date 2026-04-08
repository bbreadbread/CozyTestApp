using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM
{
    public abstract class BaseViewModel : ObservableObject
    {
        public virtual string WindowTitle => "CozyTest";
        protected readonly INavigationService _navigationService;
        protected readonly IDialogService _dialogService;
        public ICommand GoBackCommand { get; }
        protected BaseViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService; 
            GoBackCommand = new RelayCommand(_ => ExecuteGoBack(), _ => CanExecuteGoBack());
        }

        protected virtual void ExecuteGoBack()
        {
            _navigationService.GoBack();
        }

        protected virtual bool CanExecuteGoBack()
        {
            return _navigationService.CanGoBack;
        }
    }
}
