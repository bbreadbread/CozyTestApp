using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Windows.Controls;
using System.Windows.Input;

namespace CozyTest.ForShellWindow
{
    /// <summary>
    /// Логика взаимодействия для SearchParticipantShell.xaml
    /// </summary>
    public partial class SearchParticipantShell : UserControl
    {
        public SearchParticipantShell()
        {
            InitializeComponent();
        }

        private void ListView_Add(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SearchParticipantViewModel vm && vm.SelectedParticipant != null)
            {
                vm.BindParticipant(vm.SelectedParticipant);
            }
        }
        private void ListView_Remove(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SearchParticipantViewModel vm && vm.SelectedCurrentParticipant != null)
            {
                vm.RemoveParticipant(vm.SelectedCurrentParticipant);
            }
        }
    }
}
