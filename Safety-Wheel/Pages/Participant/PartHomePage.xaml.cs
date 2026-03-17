using Safety_Wheel.Models;
using Safety_Wheel.Pages.Curator;
using Safety_Wheel.Services;
using Safety_Wheel.ViewModels.CreateTestsVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для PartHomePage.xaml
    /// </summary>
    public partial class PartHomePage : Page
    {
        Topic _subject;

        public PartHomePage(Topic subject = null)
        {
            _subject = subject;
            DataContext = new CuratorAllTestViewModel(_subject);
            InitializeComponent();
        }

        //private void RemoveTest_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;

        //    if (sender is Button btn &&
        //        btn.Tag is Test test &&
        //        DataContext is CuratorAllTestViewModel vm)
        //    {
        //        vm.RemoveTest(test);
        //    }
        //}


        private async void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border ||
                border.Tag is not TestListItemViewModel vm ||
                DataContext is not CuratorAllTestViewModel dm)
                return;

            dm.IsLoading = true;

            if (vm.IsCreateCard)
            {
                MainNavigation.GlobalFrameCurator
                    ?.Navigate(new CuratorCreateTestsPage(null));
                return;
            }

            if (vm.Test == null)
                return;



            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            MainNavigation.GlobalFrameCurator
                ?.Navigate(new CuratorCreateTestsPage(vm.Test));

            dm.IsLoading = false;
        }

    }
}