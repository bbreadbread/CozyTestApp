using Safety_Wheel.Models;
using Safety_Wheel.Services;
using Safety_Wheel.ViewModels.CreateTestsVM;
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
    /// Логика взаимодействия для TeacherAllTests.xaml
    /// </summary>
    public partial class TeacherAllTests : UserControl
    {
        Subject _subject;
        public TeacherAllTests(Subject subject = null)
        {
            _subject = subject;
            DataContext = new TeacherAllTestViewModel(_subject);
            InitializeComponent();
        }
        private void RemoveTest_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button btn &&
                btn.Tag is Test test &&
                DataContext is TeacherAllTestViewModel vm)
            {
                vm.RemoveTest(test);
            }
        }


        private async void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border ||
                border.Tag is not TestListItemViewModel vm ||
                DataContext is not TeacherAllTestViewModel dm)
                return;

            dm.IsLoading = true;

            if (vm.IsCreateCard)
            {
                TeacherMainPage.GlobalFrame
                    ?.Navigate(new TeacherCreateTestsPage(null));
                return;
            }

            if (vm.Test == null)
                return;



            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            TeacherMainPage.GlobalFrame
                ?.Navigate(new TeacherCreateTestsPage(vm.Test));

            dm.IsLoading = false;
        }
    }
}
