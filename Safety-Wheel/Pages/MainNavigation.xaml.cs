using MahApps.Metro.Controls;
using CozyTest.ViewModels;
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
using CozyTest.Models;
using CozyTest.Services;
using System.Windows.Media.Animation;
using static CozyTest.ViewModels.MainViewModel;
using CozyTest.Pages.Participant;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;

namespace CozyTest.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для MainNavigation.xaml
    /// </summary>
    public partial class MainNavigation : Page
    {
        public MainNavigation()
        {
            PartTest._isTestActivated = false;
            InitializeComponent();
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

                //!
                switch (CurrentUser.TypeUser)
                {
                    case 1 or 2: 
                        mw.VM.CurrentContent = App.Services.GetRequiredService<CuratorWelcomePageViewModel>(); ;
                        break;
                    default:
                        mw.VM.CurrentContent = null; 
                        break;
                }
            }
        }

    }
}