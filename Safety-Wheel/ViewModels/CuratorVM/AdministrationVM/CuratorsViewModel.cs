using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CuratorsViewModel : BaseAdminViewModel
    {
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
                    LoadForCurrentCurator();
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

        public ICommand AddCuratorCommand { get; }
        public ICommand EditCuratorCommand { get; }
        public ICommand ArchiveCuratorCommand { get; }
        public ICommand AdminStatusCuratorCommand { get; }
        public ICommand ArchiveTestCommand { get; }
        public ICommand PublishTestCommand { get; }

        public CuratorsViewModel(IDialogService dialogService, INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            ActiveIsChecked = true;

            AddCuratorCommand = new RelayCommand(_ => AddCurator());
            EditCuratorCommand = new RelayCommand(_ => EditCurator(), _ => SelectedCurator != null);
            ArchiveCuratorCommand = new RelayCommand(_ => ArchiveCurator(), _ => SelectedCurator != null);
            AdminStatusCuratorCommand = new RelayCommand(_ => AdminStatusCurator(), _ => SelectedCurator != null);
            ArchiveTestCommand = new RelayCommand(_ => ArchiveTest(), _ => SelectedTest != null);
            PublishTestCommand = new RelayCommand(_ => PublishTest(), _ => SelectedTest != null);
        }

        private void AddCurator()
        {
            var vm = new CreateEditCuratorAdminViewModel(_dialogService, _navigationService, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void EditCurator()
        {
            if (SelectedCurator == null) return;

            var vm = new CreateEditCuratorAdminViewModel(_dialogService, _navigationService, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void PublishTest()
        {
            if (SelectedTest == null) return;

            // Навигация к настройкам публикации теста
            // var vm = new TestPublishSettingsViewModel(SelectedTest.Id);
            // _dialogService.ShowWindow<ShellWindow>(vm);

            _dialogService.ShowMessage($"Настройки публикации теста: {SelectedTest.Name}");
        }

        private void LoadForCurrentCurator()
        {
            if (SelectedCurator != null)
            {
                _testService.GetAll(SelectedCurator.Id);
                TestsForCuratorList = new ObservableCollection<Test>(_testService.Tests);

                ParticipantsForCuratorList = new ObservableCollection<Participant>(
                    _participantService.GetAllBind(teacherId: SelectedCurator.Id));

                _groupService.GetAllGroupsForCurator(SelectedCurator.Id);
                GroupsForCuratorList = new ObservableCollection<Group>(_groupService.Group);
            }
        }

        private void ArchiveCurator()
        {
            if (SelectedCurator == null) return;

            if (_dialogService.ShowConfirmation(
                $"Отправить куратора '{SelectedCurator.Name}' в архив?",
                "Подтверждение"))
            {
                _curatorService.UpdateCuratorArchiveStatus(SelectedCurator.Id);
                ApplyFilters();
            }
        }

        private void AdminStatusCurator()
        {
            if (SelectedCurator == null) return;

            if (SelectedCurator.IsAdmin == null) SelectedCurator.IsAdmin = false;
            bool newStatus = (bool)!SelectedCurator.IsAdmin;
            string action = newStatus ? "назначить администратором" : "снять с роли администратора";

            if (_dialogService.ShowConfirmation(
                $"Вы уверены, что хотите {action} '{SelectedCurator.Name}'?",
                "Изменение статуса"))
            {
                _curatorService.UpdateCuratorAdminStatus(SelectedCurator.Id);
                ApplyFilters();
            }
        }

        private void ArchiveTest()
        {
            if (SelectedTest == null) return;

            if (_dialogService.ShowConfirmation(
                $"Отправить тест '{SelectedTest.Name}' в архив?",
                "Подтверждение"))
            {
                _testService.ArchiveTest(SelectedTest.Id);
                LoadForCurrentCurator();
            }
        }

        public override void ApplyFilters()
        {
            var query = _curatorService.Curators.AsEnumerable();

            if (ActiveIsChecked && !ArchiveIsChecked)
                query = query.Where(o => o.IsArchive == false || o.IsArchive == null);
            else if (ArchiveIsChecked && !ActiveIsChecked)
                query = query.Where(o => o.IsArchive == true);

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

        public void ReloadCurators()
        {
            _curatorService.GetAll();

            ApplyFilters();
        }
    }
}