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

namespace Safety_Wheel.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для CuratorWelcomePage.xaml
    /// </summary>
    public partial class CuratorWelcomePage : Page
    {
        bool _isAttemts = false;
        public CuratorWelcomePage()
        {
            InitializeComponent();
        }
        
        public CuratorWelcomePage(bool isAttemts)
        {
            _isAttemts = isAttemts;
            InitializeComponent();
            if (isAttemts == true)
            {
                GeneralWelcome.Visibility = Visibility.Collapsed;
                AttemptsWelcome.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isAttemts == false)
            {
                var vm = DataContext as MainViewModelCurator;
                vm?.ResetApplicationState();
            }
        }

        private void GoTest_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Test");
        }
        private void GoResult_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Res");
        }
        private void GoStatistic_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Stat");
        }
        private void GoSetting_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Sett");
        }
    }
}
