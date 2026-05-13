using CozyTest.Models;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Windows.Controls;
using System.Windows.Input;

namespace CozyTest.ForShellWindow
{
    /// <summary>
    /// Логика взаимодействия для BindUserForGroup.xaml
    /// </summary>
    public partial class BindUserForGroup : UserControl
    {
        public BindUserForGroup()
        {
            InitializeComponent();
        }
        private async void ListView_Add(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BindUserForGroupViewModel vm && vm.SelectedParticipant != null)
            {
                await vm.BindParticipantAsync(vm.SelectedParticipant);
            }
        }
        private async void ListView_Remove(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BindUserForGroupViewModel vm && vm.SelectedCurrentParticipant != null)
            {
                await vm.RemoveParticipantAsync(vm.SelectedCurrentParticipant);
            }
        }
    }
}
