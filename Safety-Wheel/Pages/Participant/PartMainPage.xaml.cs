using MahApps.Metro.Controls;
using Safety_Wheel.ViewModels;
using Safety_Wheel.ViewModels.ParticipantVM;
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
using static Safety_Wheel.ViewModels.ParticipantVM.MainViewModelPart;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для ParticipantMainPage.xaml
    /// </summary>
    public partial class PartMainPage : Page
    {
        public static Frame GlobalFramePart = new();
        public MainViewModelPart partVM { get; }
        public PartMainPage()
        {
            partVM = new MainViewModelPart();
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
                DataContext = partVM;
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
                        case MainMenuType.Home:
                            mw.CuratorManagerFlyout.IsOpen = true;
                            break;

                        case MainMenuType.Profile:
                            mw.FilterMonthFlyout.IsOpen = true;
                            break;
                    }
                }
            }
        }
    }
}
