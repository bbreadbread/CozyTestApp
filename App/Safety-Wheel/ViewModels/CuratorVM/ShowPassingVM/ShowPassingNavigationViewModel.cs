using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.ParticipantVM;
using CozyTest.ViewModels.StatisticsVM;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static CozyTest.ViewModels.MainViewModel;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class ShowPassingNavigationViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public enum MenuType
        {
            Passing,
            Assigned,
        }

        private ObservableCollection<MenuItemViewModel> _menuItems = new ObservableCollection<MenuItemViewModel> { };
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        private BaseViewModel _currentContent;
        public BaseViewModel CurrentContent
        {
            get => _currentContent;
            set
            {
                SetProperty(ref _currentContent, value);
            }
        }
        private MenuItemViewModel _selectedMenuItem;
        public MenuItemViewModel SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (value == null) return;
                if (!SetProperty(ref _selectedMenuItem, value)) return;

                _ = LoadContentAsync(value).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                            MessageBox.Show($"Ошибка: {t.Exception?.InnerException?.Message}"));
                    }
                }, TaskScheduler.Current);
            }
        }
        public ShowPassingNavigationViewModel(INavigationService navigationService, IDialogService dialogService) : base(navigationService, dialogService)
        {
            CreateMenuItems();
        }

        public void CreateMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel> {
                        new MenuItemViewModel(this)
                        {
                             Icon = new Image
                             {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/follow_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                             },
                            Label = "Статистика",
                            ToolTip = "Статистика по тестам и студентам",
                            Tag = MenuType.Passing
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/finish_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Результаты тестирования",
                            ToolTip = "Просмотр результатов тестирования студентов",
                            Tag = MenuType.Assigned
                        }
                    };

        }


        private async Task LoadContentAsync(MenuItemViewModel value)
        {
            CurrentContent = null;

            if (value.Tag is MenuType menuType)
            {
                switch (menuType)
                {
                    case MenuType.Passing:
                        CurrentContent = App.Services.GetRequiredService<CuratorShowPassingTestsViewModel>();
                        break;

                    case MenuType.Assigned:
                        CurrentContent = App.Services.GetRequiredService<CuratorShowAssignedPassingTestViewModel>();
                        break;
                }
            }
        }
    }
}
