using CozyTest.Models;
using CozyTest.ViewModels.CreateTestsVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CozyTest.Pages.Curator
{
    public partial class CuratorAllTests : UserControl
    {
        public CuratorAllTests()
        {
            InitializeComponent();
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border &&
                border.Tag is TestListItemViewModel vm &&
                DataContext is CuratorAllTestViewModel dm)
            {
                 dm.OnCardClick(vm);
            }
        }

        private async void ArchiveTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.Tag is Test test &&
                DataContext is CuratorAllTestViewModel vm)
            {
                await vm.OnArchiveTestAsync(test);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}