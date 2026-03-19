using CozyTest.ViewModels.CuratorVM;
using System.Windows.Controls;

namespace CozyTest.Shells.Curator
{
    /// <summary>
    /// Логика взаимодействия для PublicationDetails.xaml
    /// </summary>
    public partial class PublicationDetails : UserControl
    {
        PublicDetailsViewModel PublicDetailsViewModel = new();
        public PublicationDetails()
        {
            InitializeComponent();
            DataContext = PublicDetailsViewModel;
        }
    }
}
