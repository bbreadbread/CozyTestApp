using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class CuratorShowAssignedPassingTestViewModel : BaseViewModel
    {
        public CuratorShowAssignedPassingTestViewModel(INavigationService navigationService, IDialogService dialogService) : base(navigationService, dialogService)
        {
        }
    }
}
