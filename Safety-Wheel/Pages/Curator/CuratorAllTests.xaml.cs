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

namespace Safety_Wheel.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для CuratorAllTests.xaml
    /// </summary>
    public partial class CuratorAllTests : UserControl
    {
        Topic _subject;
        public CuratorAllTests(Topic subject = null)
        {
            _subject = subject;
            DataContext = new CuratorAllTestViewModel(_subject);
            InitializeComponent();
        }
        private void RemoveTest_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button btn &&
                btn.Tag is Test test &&
                DataContext is CuratorAllTestViewModel vm)
            {
                vm.RemoveTest(test);
            }
        }


        private async void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border ||
                border.Tag is not TestListItemViewModel vm ||
                DataContext is not CuratorAllTestViewModel dm)
                return;

            dm.IsLoading = true;

            if (vm.IsCreateCard)
            {
                CuratorMainPage.GlobalFrameCurator
                    ?.Navigate(new CuratorCreateTestsPage(null));
                return;
            }

            if (vm.Test == null)
                return;



            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            CuratorMainPage.GlobalFrameCurator
                ?.Navigate(new CuratorCreateTestsPage(vm.Test));

            dm.IsLoading = false;
        }
    }
}
