using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.ParticipantVM;
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
        protected readonly ILoggingService _logger;
        private bool _adminModeOn;
        public virtual bool AdminModeOn
        {
            get => _adminModeOn;
            set
            {
                if (SetProperty(ref _adminModeOn, value))
                {
                    _ = OnAdminModeChangedAsync();
                }
            }
        }
        protected virtual Task OnAdminModeChangedAsync() => Task.CompletedTask;

        public ICommand GoBackCommand { get; }
        protected BaseViewModel(INavigationService navigationService, IDialogService dialogService, ILoggingService logger)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            _adminModeOn = CurrentUser.AdminModeOn;

            CurrentUser.AdminModeOnChanged += (_, _) =>
            {
                _adminModeOn = CurrentUser.AdminModeOn;
                OnPropertyChanged(nameof(AdminModeOn));
                _ = OnAdminModeChangedAsync();
            };

            GoBackCommand = new RelayCommand(_ => ExecuteGoBack(), _ => CanExecuteGoBack());
        }

        protected virtual void ExecuteGoBack()
        {
            if (_navigationService.GetCurrentViewModel() is PassingTestViewModel pvm && !pvm.IsTestFinished)
            {
                var dialog = new ClosedWindow("Тест не завершён!", "Если вы выйдете сейчас, тест будет принудительно завершён. Вы уверены?");
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    pvm.FinishTestAsync();
                    _navigationService.GoBack();
                }
            }
            else if (_navigationService.GetCurrentViewModel() is CuratorCreateTestViewModel cvm)
            {
                var dialog = new ClosedWindow("Изменения не сохранены!", "Если вы выйдете сейчас, все изменения не сохранятся. Вы уверены?");
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _navigationService.GoBack();
                }
            }
            else _navigationService.GoBack();
        }

        protected virtual bool CanExecuteGoBack()
        {
            return _navigationService.CanGoBack;
        }
    }
}
