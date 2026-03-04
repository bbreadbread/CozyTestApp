using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Safety_Wheel.Pages.Curator;

namespace Safety_Wheel.ViewModels.ParticipantVM
{
    public class MainViewModelPart : ObservableObject
    {
        private ObservableCollection<MenuItemViewModel> _mainMenuItems;
        private ObservableCollection<MenuItemViewModel> _menuItems;
        private MenuItemViewModel _selectedMainMenuItem;

        public enum MainMenuType
        {
            Home,
            Profile
        }

        public MainViewModelPart()
        {
            CreateMainMenuItems();
            CreateMenuItems();
        }

        public void CreateMainMenuItems()
        {
            MainMenuItems = new ObservableCollection<MenuItemViewModel>
            {
                 new MenuItemViewModel(this)
                 {
                     Icon = new Image
                     {
                        Source = new BitmapImage(new Uri("pack://application:,,,/Images/statistic_icon.png")),
                        Width = 50,
                        Height = 50,
                        Stretch = Stretch.Uniform
                     },
                    Label = "Статистика",
                    ToolTip = "Статистика по тестам и студентам",
                    Tag = MainMenuType.Home
                 },
                new MenuItemViewModel(this)
                {
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/Images/profile_icon.png")),
                        Width = 50,
                        Height = 50,
                        Stretch = Stretch.Uniform
                    },
                    Label = "Результаты тестирования",
                    ToolTip = "Просмотр результатов тестирования студентов",
                    Tag = MainMenuType.Profile
                }
            };
        }

        public void CreateMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel> { };
        }

        public ObservableCollection<MenuItemViewModel> MainMenuItems
        {
            get => _mainMenuItems;
            set => SetProperty(ref _mainMenuItems, value);
        }

        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        public MenuItemViewModel SelectedMainMenuItem
        {
            get => _selectedMainMenuItem;
            set
            {
                if (value == null)
                    return;

                if (!SetProperty(ref _selectedMainMenuItem, value))
                    return;

                CuratorMainPage.GlobalFrameCurator?.Navigate(new Page());

                MenuItems.Clear();

                if (value.Tag is MainMenuType menuType)
                {
                    switch (menuType)
                    {
                        case MainMenuType.Home:
                            CuratorMainPage.GlobalFrameCurator?.Navigate(new CuratorWelcomePage(true));
                            break;

                        case MainMenuType.Profile:
                            CuratorMainPage.GlobalFrameCurator?.Navigate(new CuratorStatisticsPage());
                            break;

                        default:
                            WPFCustomMessageBox.CustomMessageBox.Show("Не найден путь для перемещения");
                            break;
                    }
                }
            }
        }

    }
}
