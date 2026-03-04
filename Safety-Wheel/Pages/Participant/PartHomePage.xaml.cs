using Notifications.Wpf;
using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для PartHomePage.xaml
    /// </summary>
    public partial class PartHomePage : Page
    {
        public static string TypeDiscipline { get; set; } = string.Empty;
        public int TypeTest { get; set; } = 0;

        TestService _testService = new();
        public static Models.Participant thisParticipant;
        private Button? _selectedButton;

        public static string NameDiscipline;
        ParticipantService ParticipantService = new();
        public static Models.Participant thisStudent;

        public PartHomePage()
        {
            InitializeComponent();
        }

    }
}