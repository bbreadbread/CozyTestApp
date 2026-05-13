using CozyTest.Models;
using CozyTest.Pages.Curator;
using CozyTest.Pages.Participant;
using CozyTest.Services;
using CozyTest.ViewModels;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.CuratorVM.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM.ShowPassingVM;
using CozyTest.ViewModels.ParticipantVM;
using CozyTest.ViewModels.StatisticsVM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;

namespace CozyTest
{
    public partial class App : Application
    {
        public static readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var connectionString = ConfigurationManager
                    .ConnectionStrings["CozyTestDB"]
                    .ConnectionString;

                var services = new ServiceCollection();

                services.AddDbContextFactory<CozyTestContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddTransient<AttemptService>();
                services.AddTransient<CorrespondenceService>();
                services.AddTransient<CriteriaService>();
                services.AddTransient<CuratorService>();
                services.AddTransient<GroupService>();
                services.AddTransient<OptionService>();
                services.AddTransient<QuestionService>();
                services.AddTransient<TestService>();
                services.AddTransient<ParticipantService>();
                services.AddTransient<ParticipantAnswerService>();
                services.AddTransient<ParticipantPublicTestService>();
                services.AddTransient<ParticipantFavoriteTestService>();
                services.AddTransient<ParticipantAssignedTestService>();
                services.AddTransient<RequestService>();
                services.AddTransient<TopicService>();
                services.AddTransient<DTestTypeService>();

                services.AddSingleton<INavigationService, Services.NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<ShowPassingNavigationViewModel>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();

                //экзаменатор
                services.AddTransient<CuratorWelcomePageViewModel>();
                services.AddTransient<CuratorShowPassingTestsViewModel>();
                services.AddTransient<CreateEditParticipantViewModel>();
                services.AddTransient<CreateEditTopicViewModel>();
                services.AddTransient<SearchParticipantViewModel>();
                services.AddTransient<BindUserForGroupViewModel>();
                services.AddTransient<CreateEditGroupViewModel>();
                services.AddTransient<PublicDetailsViewModel>();
                services.AddTransient<AssignedDetailsViewModel>();
                services.AddTransient<CuratorShowAssignedPassingTestViewModel>();

                services.AddTransient<StatisticsViewModel>();
                
                services.AddTransient<CuratorShowPassingCurrentTestViewModel>();
                
                services.AddTransient<AuthorizationViewModel>();
                services.AddTransient<RegistrationViewModel>();

                services.AddTransient<AdminPanelViewModel>();
                services.AddTransient<ParticipantsViewModel>();
                services.AddTransient<CuratorsViewModel>();
                services.AddTransient<RequestsViewModel>();
                services.AddTransient<GroupsViewModel>();

                services.AddTransient<CuratorAllTestViewModel>();
                services.AddTransient<CuratorCreateTestViewModel>();
                services.AddTransient<ImportExcelViewModel>();

                //тестируемый
                services.AddTransient<PartAllTestViewModel>();
                services.AddTransient<PartProfileViewModel>();

                Services = services.BuildServiceProvider();

                var mainVm = Services.GetRequiredService<MainViewModel>();
                var navigationService = Services.GetRequiredService<INavigationService>();
                navigationService.Initialize(mainVm);

                var mainWindow = Services.GetRequiredService<MainWindow>();
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