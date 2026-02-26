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
using static Safety_Wheel.ViewModels.MainViewModel;
using Safety_Wheel.Pages.Student;

namespace Safety_Wheel.Pages.Teacher
{
    /// <summary>
    /// Логика взаимодействия для TeacherMainPage.xaml
    /// </summary>
    public partial class TeacherMainPage : Page
    {
        public static Frame GlobalFrame = new();
        public enum ViewTypes
        {
            Open,
            Search
        }

        public TeacherMainPage(int menuItemsCount = 0)
        {
            StudTest._isTestActivated = false;
            InitializeComponent();
            GlobalFrame = FrameTeacher;
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
            if (DatesMenuControl.IsPaneOpen)
            {
                var anim = new DoubleAnimation(48, TimeSpan.FromMilliseconds(250));
                DatesMenuControl.BeginAnimation(MahApps.Metro.Controls.HamburgerMenu.CompactPaneLengthProperty, anim);
                DatesMenuControl.IsPaneOpen = false;
                DatesMenuControl.SetCurrentValue(HamburgerMenu.SelectedIndexProperty, -1);
            }

            if (!e.IsItemOptions)
                return;

            if (e.InvokedItem is MenuItemViewModel item)
            {
                var menu = sender as MahApps.Metro.Controls.HamburgerMenu;
                if (Application.Current.MainWindow is MainWindow mw)
                {
                    switch (item.Tag)
                    {
                        case MainMenuType.TeacherManager:
                            mw.TeacherManagerFlyout.IsOpen = true;
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
