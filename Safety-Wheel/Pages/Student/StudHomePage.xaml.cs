using Safety_Wheel.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Safety_Wheel.Pages.Student
{
    /// <summary>
    /// Логика взаимодействия для StudHomePage.xaml
    /// </summary>
    public partial class StudHomePage : Page
    {
        public static string NameDiscipline;
        StudentService StudentService = new();
        public static Models.Student thisStudent;

        public StudHomePage(int StudentID)
        {
            thisStudent = StudentService.GetCurrentStudent(StudentID);
            InitializeComponent();
            if (Application.Current.MainWindow is MainWindow mw)
                mw.VM.UserFullName = thisStudent.Name ?? string.Empty;
        }

        private void ButtonGoPdd_Click(object sender, RoutedEventArgs e)
        {
            NameDiscipline = "ПДД";
            NavigationService.Navigate(new StudSelectedTestsPage(NameDiscipline, thisStudent));
        }

        private void ButtonGoMedicine_Click(object sender, RoutedEventArgs e)
        {
            NameDiscipline = "Медицина";
            NavigationService.Navigate(new StudSelectedTestsPage(NameDiscipline, thisStudent));
        }
    }
}
