using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CuratorsViewModel : BaseAdminViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly CuratorService _curatorService;
        private readonly ParticipantService _participantService;
        private readonly ParticipantPublicTestService _participantPublicTestService;
        private readonly TestService _testService;
        private readonly GroupService _groupService;

        private ObservableCollection<Curator> _curatorsList;
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set
            {
                _curatorsList = value;
                OnPropertyChanged(nameof(CuratorsList));
            }
        }

        private ObservableCollection<Participant> _participantsForCuratorList;
        public ObservableCollection<Participant> ParticipantsForCuratorList
        {
            get => _participantsForCuratorList;
            set => SetProperty(ref _participantsForCuratorList, value);
        }

        private ObservableCollection<Group> _groupsForCuratorList;
        public ObservableCollection<Group> GroupsForCuratorList
        {
            get => _groupsForCuratorList;
            set => SetProperty(ref _groupsForCuratorList, value);
        }

        private ObservableCollection<Test> _testsForCuratorList;
        public ObservableCollection<Test> TestsForCuratorList
        {
            get => _testsForCuratorList;
            set => SetProperty(ref _testsForCuratorList, value);
        }

        private Curator _selectedCurator;
        public Curator SelectedCurator
        {
            get => _selectedCurator;
            set
            {
                if (SetProperty(ref _selectedCurator, value))
                {
                    _ = LoadForCurrentCuratorAsync();
                    CuratorsVisibility = Visibility.Visible;
                }
            }
        }

        private Test _selectedTest;
        public Test SelectedTest
        {
            get => _selectedTest;
            set => SetProperty(ref _selectedTest, value);
        }

        private Visibility _curatorsVisibility = Visibility.Collapsed;
        public Visibility CuratorsVisibility
        {
            get => _curatorsVisibility;
            set => SetProperty(ref _curatorsVisibility, value);
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddCuratorCommand { get; }
        public ICommand EditCuratorCommand { get; }
        public ICommand ArchiveCuratorCommand { get; }
        public ICommand AdminStatusCuratorCommand { get; }
        public ICommand ArchiveTestCommand { get; }
        public ICommand PublishTestCommand { get; }

        public CuratorsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            CuratorService curatorService,
            ParticipantService participantService,
            TestService testService,
            GroupService groupService, 
            ParticipantPublicTestService participantPublicTestService)
            : base(dialogService, navigationService, participantService, curatorService, testService, null, groupService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _curatorService = curatorService;
            _participantService = participantService;
            _testService = testService;
            _groupService = groupService;
            _participantPublicTestService = participantPublicTestService;
            _ = InitializeAsync();

            AddCuratorCommand = new RelayCommand(_ => AddCurator(), _ => !IsLoading);
            EditCuratorCommand = new RelayCommand(_ => EditCurator(), _ => SelectedCurator != null && !IsLoading);
            ArchiveCuratorCommand = new RelayCommand(_ => _ = ArchiveCuratorAsync(), _ => SelectedCurator != null && !IsLoading);
            AdminStatusCuratorCommand = new RelayCommand(_ => _ = AdminStatusCuratorAsync(), _ => SelectedCurator != null && !IsLoading);
            ArchiveTestCommand = new RelayCommand(_ => _ = ArchiveTestAsync(), _ => SelectedTest != null && !IsLoading);
            PublishTestCommand = new RelayCommand(_ => PublishTest(), _ => SelectedTest != null && !IsLoading);
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await _curatorService.InitializeAsync();
                CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
                ActiveIsChecked = true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка инициализации: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddCurator()
        {
            try
            {
                var vm = new CreateEditCuratorAdminViewModel(_dialogService, _navigationService, this, _curatorService, _participantService);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка открытия добавления: {ex.Message}", "Ошибка");
            }
        }

        private void EditCurator()
        {
            try
            {
                if (SelectedCurator == null) return;

                var vm = new CreateEditCuratorAdminViewModel(_dialogService, _navigationService, this, _curatorService, _participantService);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка открытия редактирования: {ex.Message}", "Ошибка");
            }
        }

        private void PublishTest()
        {
            if (SelectedTest == null) return;

            try
            {
                var vm = new PublicDetailsViewModel(_dialogService, _navigationService, _participantService, _groupService, _participantPublicTestService);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при изменении статуса теста: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadForCurrentCuratorAsync()
        {
            if (SelectedCurator == null) return;


                await _testService.GetAllAsync(SelectedCurator.Id);
                TestsForCuratorList = new ObservableCollection<Test>(_testService.Tests);

                var participants = await _participantService.GetAllBindAsync(SelectedCurator.Id);
                ParticipantsForCuratorList = new ObservableCollection<Participant>(participants);

                await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, SelectedCurator.Id);
                GroupsForCuratorList = new ObservableCollection<Group>(_groupService.Groups);
        }

        private async Task ArchiveCuratorAsync()
        {
            if (SelectedCurator == null) return;
            if (SelectedCurator.IsAdmin == true && SelectedCurator.Id == CurrentUser.Id)
            {
                MessageBox.Show("Вы не можете архивировать самого себя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedCurator.IsAdmin == false)
            {
                MessageBox.Show("Вы не обладаете достаточными правами для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                IsLoading = true;

                string action = SelectedCurator.IsArchive == true ? "восстановить" : "отправить в архив";
                string message = SelectedCurator.IsArchive == true
                    ? $"Восстановить куратора '{SelectedCurator.Name}' из архива?"
                    : $"Отправить куратора '{SelectedCurator.Name}' в архив?";

                if (_dialogService.ShowConfirmation(message, "Подтверждение"))
                {
                    await _curatorService.UpdateCuratorArchiveStatusAsync(SelectedCurator.Id);
                    await ApplyFiltersAsync();

                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при изменении статуса куратора: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AdminStatusCuratorAsync()
        {
            if (SelectedCurator == null) return;
            if (SelectedCurator.IsAdmin == true && SelectedCurator.Id == CurrentUser.Id)
            {
                MessageBox.Show("Вы не можете поменять статус админства самому себе","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CurrentUser.TypeUser != 1)
            {
                MessageBox.Show("Вы не обладаете достаточными правами для смены админства","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedCurator.IsAdmin == null) SelectedCurator.IsAdmin = false;
                bool newStatus = (bool)!SelectedCurator.IsAdmin;
                string action = newStatus ? "назначить администратором" : "снять с роли администратора";

                if (_dialogService.ShowConfirmation(
                    $"Вы уверены, что хотите {action} '{SelectedCurator.Name}'?",
                    "Изменение статуса"))
                {
                    await _curatorService.UpdateCuratorAdminStatusAsync(SelectedCurator.Id);
                    await ApplyFiltersAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при изменении статуса: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ArchiveTestAsync()
        {
            if (SelectedTest == null) return;

            try
            {
                IsLoading = true;

                string action = SelectedTest.IsArchive == true ? "восстановить" : "отправить в архив";
                string message = SelectedTest.IsArchive == true
                    ? $"Восстановить тест '{SelectedTest.Name}' из архива?"
                    : $"Отправить тест '{SelectedTest.Name}' в архив?";

                if (_dialogService.ShowConfirmation(message, "Подтверждение"))
                {
                    await _testService.ArchiveTestAsync(SelectedTest.Id);
                    await LoadForCurrentCuratorAsync();
                    await ApplyFiltersAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при изменении статуса теста: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task ApplyFiltersAsync()
        {
            try
            {
                IsLoading = true;

                var query = _curatorService.Curators.AsEnumerable();

                if (ActiveIsChecked && !ArchiveIsChecked)
                    query = query.Where(o => o.IsArchive == false || o.IsArchive == null);
                else if (ArchiveIsChecked && !ActiveIsChecked)
                    query = query.Where(o => o.IsArchive == true);

                if (IsAdmin) query = query.Where(o => o.IsAdmin == true);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(p =>
                        p.Name != null &&
                        p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                CuratorsList.Clear();
                foreach (var curator in query)
                {
                    CuratorsList.Add(curator);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка фильтрации: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ReloadCuratorsAsync()
        {
            try
            {
                IsLoading = true;
                await _curatorService.GetAllAsync();
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка перезагрузки: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}