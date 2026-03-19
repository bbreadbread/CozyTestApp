using CozyTest.ViewModels.CuratorVM;
using System.Windows.Controls;

namespace CozyTest.Shells.Curator
{
    /// <summary>
    /// Логика взаимодействия для PublicationDetails.xaml
    /// </summary>
    public partial class PublicationDetails : UserControl
    {
        PublicDetailsViewModel _publicDetailsViewModel = new();
        public PublicationDetails(int testId)
        {
            InitializeComponent();
            _publicDetailsViewModel.currentTestId = testId;
            DataContext = _publicDetailsViewModel;
        }
    }
}
