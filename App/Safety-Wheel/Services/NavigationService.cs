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
        BaseViewModel? GetLast();

        BaseViewModel? GetCurrentViewModel();

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
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        }

        private void EnsureInitialized()
        {
            if (_mainVm == null)
            {
                throw new InvalidOperationException("NavigationService не инициализирован. Вызовите Initialize() перед использованием.");
            }
        }

        public bool CanGoBack => _backStack.Count > 0;

        public event Action CurrentViewModelChanged
        {
            add
            {
                EnsureInitialized();
                _mainVm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.CurrentContent))
                        value?.Invoke();
                };
            }
            remove { }
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            EnsureInitialized();

            if (_mainVm.CurrentContent != null)
            {
                _backStack.Push(_mainVm.CurrentContent);
            }

            _mainVm.CurrentContent = viewModel;
            _currentViewModel = viewModel;
        }

        public void GoBack()
        {
            EnsureInitialized();

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

        public BaseViewModel? GetLast()
        {
            if (_backStack.Count == 0)
                return null;

            return _backStack.Last();
        }

        public BaseViewModel? GetCurrentViewModel()
        {
            return _currentViewModel;
        }
    }
}