using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.CreateTestsVM
{
    public class ShellGradingSystemViewModel:BaseViewModel
    {
        public override string WindowTitle => "Система оценивания для теста";

        ShellGradingSystemViewModel(
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService, dialogService)
        {

        }
    }
}
