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

namespace Safety_Wheel.ForShellWindow
{
    /// <summary>
    /// Логика взаимодействия для CreateEditGroup.xaml
    /// </summary>
    public partial class CreateEditGroup : UserControl
    {
        public CreateEditGroup()
        {
            InitializeComponent();
        }

        private void Button_Bind(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new BindUserForGroup());
            window.Show();
        }
    }
}
