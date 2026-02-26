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

namespace Safety_Wheel.Pages.Teacher
{
    /// <summary>
    /// Логика взаимодействия для TeacherWelcomePage.xaml
    /// </summary>
    public partial class TeacherWelcomePage : Page
    {
        bool _isAttemts = false;
        public TeacherWelcomePage()
        {
            InitializeComponent();
        }
        
        public TeacherWelcomePage(bool isAttemts)
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
                var vm = DataContext as MainViewModel;
                vm?.ResetApplicationState();
            }
        }
    }
}
