using CozyTest.Pages.Participant;
using System.Windows.Controls;
using System.Windows.Input;

namespace CozyTest.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для MainNavigation.xaml
    /// </summary>
    public partial class MainNavigation : UserControl
    {
        public MainNavigation()
        {
            InitializeComponent();
        }

        private void DataGridRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
            }
        }
    }
}