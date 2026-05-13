using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CozyTest.Pages.Participant;
using CozyTest.Services;
using CozyTest.ViewModels;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CozyTest
{
    public partial class MainWindow : MetroWindow
    {
        private readonly AttemptService _attemptService;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            try
            {
                var comboBoxStyle = this.Resources.Contains(typeof(ComboBox))
       ? this.Resources[typeof(ComboBox)] as Style
       : null;

                if (comboBoxStyle != null)
                {
                    foreach (Setter setter in comboBoxStyle.Setters)
                    {
                        if (setter.Property.Name == "Name")
                        {
                            System.Diagnostics.Debug.WriteLine($"Found Name setter with value: {setter.Value}");
                        }
                    }
                }

                InitializeComponent();
                VM = mainWindowViewModel;
                DataContext = VM;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ОШИБКА в конструкторе MainWindow: {ex.Message}\n\n{ex.StackTrace}");
                throw;
            }
        }

        public MainWindowViewModel VM { get; set; }

        public void UpdateUserName(string userName)
        {
                VM.UserFullName = userName ?? string.Empty;
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
           //PartPassingTestPage._isTestActivated
            //    ? "Попытка аннулируется."
             var txt = "Перед закрытием убедитесь, что сохранили прогресс";

            var confirm = new ClosedWindow("Вы намерены закрыть приложение", txt)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (confirm.ShowDialog() == false)
            {
                e.Cancel = true;
                return;
            }

            //if (PartPassingTestPage._isTestActivated)
            //{
            //    var attemptService = App.Services.GetRequiredService<AttemptService>();
            //    await attemptService.RemoveAsync(PartPassingTestPage._attempt);
            //}
        }

        public void RefreshUserRole()
        {
            var comboBox = FindName("RoleComboBox") as ComboBox;
            if (comboBox != null)
            {
                var binding = comboBox.GetBindingExpression(ComboBox.VisibilityProperty);
                binding?.UpdateTarget();
            }

            var navp = FindName("NavPanel") as StackPanel;
            if (navp != null)
            {
                var binding = navp.GetBindingExpression(StackPanel.VisibilityProperty);
                binding?.UpdateTarget();
            }
        }
    }
}