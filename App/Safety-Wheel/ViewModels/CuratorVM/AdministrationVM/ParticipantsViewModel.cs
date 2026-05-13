using CozyTest.ForShellWindow;
using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class ParticipantsViewModel : BaseAdminViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly ParticipantService _participantService;
        private readonly GroupService _groupService;
        private readonly CuratorService _curatorService;
        private readonly IServiceProvider _serviceProvider;

        private ObservableCollection<Participant> _participantsList = new();
        public ObservableCollection<Participant> ParticipantsList
        {
            get => _participantsList;
            set
            {
                _participantsList = value;
                OnPropertyChanged(nameof(ParticipantsList));
            }
        }

        private ObservableCollection<Group> _groupsListCurrent = new();
        public ObservableCollection<Group> GroupsListCurrent
        {
            get => _groupsListCurrent;
            set => SetProperty(ref _groupsListCurrent, value);
        }

        private ObservableCollection<Curator> _curatorsListCurrent = new();
        public ObservableCollection<Curator> CuratorsListCurrent
        {
            get => _curatorsListCurrent;
            set => SetProperty(ref _curatorsListCurrent, value);
        }

        private ObservableCollection<Test> _publishTestListCurrent = new();
        public ObservableCollection<Test> PublishTestListCurrent
        {
            get => _publishTestListCurrent;
            set => SetProperty(ref _publishTestListCurrent, value);
        }

        private Participant _selectedParticipant = new();
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (SetProperty(ref _selectedParticipant, value))
                {
                    _ = LoadForCurrentParticipantAsync();
                    ParticipantsVisibility = Visibility.Visible;
                }
            }
        }

        private Visibility _participantsVisibility = Visibility.Collapsed;
        public Visibility ParticipantsVisibility
        {
            get => _participantsVisibility;
            set => SetProperty(ref _participantsVisibility, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand SearchParticipantCommand { get; }
        public RelayCommand AddParticipantCommand { get; }
        public RelayCommand EditParticipantCommand { get; }
        public RelayCommand ArchiveParticipantCommand { get; }

        public ParticipantsViewModel(
    IDialogService dialogService,
    INavigationService navigationService,
    ParticipantService participantService,
    GroupService groupService,
    CuratorService curatorService,
    IServiceProvider serviceProvider)
    : base(dialogService, navigationService, participantService, curatorService, null, null, groupService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _participantService = participantService;
            _groupService = groupService;
            _curatorService = curatorService;
            _serviceProvider = serviceProvider;

            _ = InitializeAsync();

            SearchParticipantCommand = new RelayCommand(_ => SearchParticipant());
            AddParticipantCommand = new RelayCommand(_ => AddParticipant());
            EditParticipantCommand = new RelayCommand(_ => EditParticipant(), _ => SelectedParticipant != null);
            ArchiveParticipantCommand = new RelayCommand(_ => _ = ArchiveParticipantAsync(), _ => SelectedParticipant != null);
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                if (CurrentUser.TypeUser == 1)
                {
                    await _participantService.InitializeForAdminAsync();
                    BindIsChecked = false;
                    CreateIsChecked = false;
                    AdminModeOn = true;
                }
                else
                {
                    await _participantService.InitializeAsync();
                    BindIsChecked = true;
                    CreateIsChecked = false;
                }

                ActiveIsChecked = true;
                ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
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

        private void SearchParticipant()
        {
            try
            {
                var vm = ActivatorUtilities.CreateInstance<SearchParticipantViewModel>(_serviceProvider, this);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка открытия поиска: {ex.Message}", "Ошибка");
            }
        }

        private void AddParticipant()
        {
            try
            {
                SelectedParticipant = null;
                var vm = ActivatorUtilities.CreateInstance<CreateEditParticipantViewModel>(_serviceProvider, this);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка открытия добавления: {ex.Message}", "Ошибка");
            }
        }

        private void EditParticipant()
        {
            try
            {
                if (SelectedParticipant == null) return;
                var vm = ActivatorUtilities.CreateInstance<CreateEditParticipantViewModel>(_serviceProvider, this);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка открытия редактирования: {ex.Message}", "Ошибка");
            }
        }

        public async Task LoadForCurrentParticipantAsync()
        {
            if (SelectedParticipant == null) return;

            await _groupService.GetAllGroupsForUserAsync(SelectedParticipant.Id);
            GroupsListCurrent = new ObservableCollection<Group>(_groupService.Groups);

            await _curatorService.GetAllByUserAsync(SelectedParticipant.Id);
            CuratorsListCurrent = new ObservableCollection<Curator>(_curatorService.Curators);

            PublishTestListCurrent = new ObservableCollection<Test>(await _participantService.GetAllPublicTestsForParticipantAsync(SelectedParticipant.Id));
        }

        private async Task ArchiveParticipantAsync()
        {
            if (SelectedParticipant == null) return;

            try
            {
                IsLoading = true;
                await _participantService.UpdateParticipantArchiveStatusAsync(SelectedParticipant.Id);

                await ReloadParticipantsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка архивации: {ex.Message}", "Ошибка");
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

                IEnumerable<Participant> queryList;

                if (CurrentUser.TypeUser == 1)
                {
                    if (AdminModeOn)
                    {
                        await _participantService.GetAllParticipantsForAdminAsync();
                        queryList = _participantService.Participants;
                    }
                    else
                    {
                        var allParticipants = _participantService.Participants.AsEnumerable();
                        var boundParticipants = await _participantService.GetAllBindAsync(CurrentUser.Id);
                        var createdParticipants = allParticipants.Where(o => o.CuratorCreateId == CurrentUser.Id);

                        var selectedFilters = new List<IEnumerable<Participant>>();

                        if ((BindIsChecked && CreateIsChecked) || (!BindIsChecked && !CreateIsChecked))
                        {
                            selectedFilters.Add(boundParticipants);
                            selectedFilters.Add(createdParticipants);
                        }
                        else if (BindIsChecked && !CreateIsChecked)
                        {
                            selectedFilters.Add(boundParticipants);
                        }
                        else if (!BindIsChecked && CreateIsChecked)
                        {
                            selectedFilters.Add(createdParticipants);
                        }

                        queryList = selectedFilters.Any()
                            ? selectedFilters.SelectMany(x => x).GroupBy(p => p.Id).Select(g => g.First())
                            : allParticipants;
                    }
                }
                else
                {
                    var allAvailableParticipants = _participantService.Participants.AsEnumerable();
                    var boundParticipants = await _participantService.GetAllBindAsync(CurrentUser.Id);
                    var createdParticipants = allAvailableParticipants.Where(o => o.CuratorCreateId == CurrentUser.Id);

                    var selectedFilters = new List<IEnumerable<Participant>>();

                    if ((BindIsChecked && CreateIsChecked) || (!BindIsChecked && !CreateIsChecked))
                    {
                        selectedFilters.Add(boundParticipants);
                        selectedFilters.Add(createdParticipants);
                    }
                    else if (BindIsChecked && !CreateIsChecked)
                    {
                        selectedFilters.Add(boundParticipants);
                    }
                    else if (!BindIsChecked && CreateIsChecked)
                    {
                        selectedFilters.Add(createdParticipants);
                    }

                    queryList = selectedFilters.Any()
                        ? selectedFilters.SelectMany(x => x).GroupBy(p => p.Id).Select(g => g.First())
                        : allAvailableParticipants;
                }

                if (ActiveIsChecked && !ArchiveIsChecked)
                    queryList = queryList.Where(o => o.IsArchive == false || o.IsArchive == null);
                else if (ArchiveIsChecked && !ActiveIsChecked)
                    queryList = queryList.Where(o => o.IsArchive == true);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    queryList = queryList.Where(p =>
                        p.Name != null &&
                        p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                ParticipantsList.Clear();
                foreach (var participant in queryList)
                {
                    ParticipantsList.Add(participant);
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

        public async Task ReloadParticipantsAsync()
        {
            try
            {
                IsLoading = true;

                if (CurrentUser.TypeUser == 1)
                {
                    await _participantService.InitializeForAdminAsync();
                }
                else
                {
                    await _participantService.GetAllParticipantsAsync(CurrentUser.Id);
                }

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