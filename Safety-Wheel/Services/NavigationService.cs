using CozyTest.ViewModels;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CozyTest.Services
{

    public interface INavigationService
    {
        void NavigateTo(BaseViewModel viewModel);
        void Initialize(MainViewModel mainVm);

        event Action CurrentViewModelChanged;
    }

    public class NavigationService : INavigationService
    {
        private MainViewModel _mainVm;

        public NavigationService() { }

        public void Initialize(MainViewModel mainVm)
        {
            _mainVm = mainVm;
        }

        public event Action CurrentViewModelChanged
        {
            add => _mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentContent))
                    value();
            };
            remove { /* ... */ }
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            _mainVm.CurrentContent = viewModel;
        }
    }
}
