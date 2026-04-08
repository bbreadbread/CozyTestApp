using CozyTest.ViewModels;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CozyTest.Services
{
    public interface INavigationService
    {
        void NavigateTo(BaseViewModel viewModel);
        void GoBack();
        bool CanGoBack { get; }
        void Initialize(MainViewModel mainVm);
        void ClearHistory();

        event Action CurrentViewModelChanged;
    }

    public class NavigationService : INavigationService
    {
        private MainViewModel _mainVm;
        private readonly Stack<BaseViewModel> _backStack = new Stack<BaseViewModel>();
        private BaseViewModel _currentViewModel;

        public NavigationService() { }

        public void Initialize(MainViewModel mainVm)
        {
            _mainVm = mainVm;
        }

        public bool CanGoBack => _backStack.Count > 0;

        public event Action CurrentViewModelChanged
        {
            add => _mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentContent))
                    value();
            };
            remove { }
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            if (_mainVm.CurrentContent != null)
            {
                _backStack.Push(_mainVm.CurrentContent);
            }

            _mainVm.CurrentContent = viewModel;
            _currentViewModel = viewModel;
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                var previousViewModel = _backStack.Pop();
                _mainVm.CurrentContent = previousViewModel;
                _currentViewModel = previousViewModel;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Нет страниц для возврата");
            }
        }

        public void ClearHistory()
        {
            _backStack.Clear();
        }
    }
}