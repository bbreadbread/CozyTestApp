using MahApps.Metro.Controls;
using Safety_Wheel.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System.Windows.Media.Animation;
using static Safety_Wheel.ViewModels.MainViewModelCurator;
using Safety_Wheel.Pages.Participant;

namespace Safety_Wheel.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для CuratorMainPage.xaml
    /// </summary>
    public partial class CuratorMainPage : Page
    {
        public static Frame GlobalFrameCurator = new();
        public CuratorMainPage()
        {
            PartTest._isTestActivated = false;
            InitializeComponent();
            GlobalFrameCurator = FrameCurator;
        }

        private void DataGridRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mw)
            {
                DataContext = mw.VM;
            }
        }

        private void HamburgerMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {

            if (!e.IsItemOptions)
                return;

            if (e.InvokedItem is MenuItemViewModel item)
            {
                var menu = sender as MahApps.Metro.Controls.HamburgerMenu;
                if (Application.Current.MainWindow is MainWindow mw)
                {
                    switch (item.Tag)
                    {
                        case MainMenuType.CuratorManager:
                            mw.CuratorManagerFlyout.IsOpen = true;
                            break;

                        case MainMenuType.MonthFilter:
                            mw.FilterMonthFlyout.IsOpen = true;
                            break;
                    }
                }
            }
        }

    }
}
