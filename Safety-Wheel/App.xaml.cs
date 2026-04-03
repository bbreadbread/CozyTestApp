using CozyTest.Services;
using CozyTest.ViewModels;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.CuratorVM.CreateTestsVM;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Navigation;


namespace CozyTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var services = new ServiceCollection();

                services.AddSingleton<INavigationService, Services.NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();

                services.AddSingleton<MainViewModel>(sp => new MainViewModel(
    sp.GetRequiredService<IDialogService>(),
    sp.GetRequiredService<INavigationService>()));

                services.AddTransient<CuratorWelcomePageViewModel>();
                services.AddTransient<AuthorizationViewModel>();

                services.AddTransient<ParticipantsViewModel>();
                services.AddTransient<CuratorsViewModel>();
                services.AddTransient<RequestsViewModel>();
                services.AddTransient<GroupsViewModel>();

                services.AddTransient<CuratorAllTestViewModel>();
                services.AddTransient<CuratorCreateTestViewModel>();
                services.AddTransient<ImportExcelViewModel>();

                Services = services.BuildServiceProvider();

                var nav = Services.GetRequiredService<INavigationService>();
                var mainVm = Services.GetRequiredService<MainViewModel>();
                nav.Initialize(mainVm);

                var mainWindow = new MainWindow(mainVm);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DI initialization failed: {ex.Message}");
                Shutdown();
            }
        }
    }

}
