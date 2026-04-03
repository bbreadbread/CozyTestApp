using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM
{
    public abstract class BaseViewModel : ObservableObject
    {
        public virtual string WindowTitle => "CozyTest";
        protected readonly INavigationService _navigationService;
        protected readonly IDialogService _dialogService;

        protected BaseViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
        }
    }
}
