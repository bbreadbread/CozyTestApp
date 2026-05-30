using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using ScottPlot.AxisPanels;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class BindUserForGroupViewModel : BaseViewModel
    {
        public override string WindowTitle => "Привязка тестируемых к выбранной группе";

        private readonly GroupsViewModel _groupsViewModel;
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly GroupService _groupService;
        private readonly IDialogService _dialogService;

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
            }
        }

        private Group _currentGroup;
        public Group CurrentGroup
        {
            get => _currentGroup;
            set { _currentGroup = value; OnPropertyChanged(); }
        }

        private Participant _selectedCurrentParticipant;
        public Participant SelectedCurrentParticipant
        {
            get => _selectedCurrentParticipant;
            set
            {
                _selectedCurrentParticipant = value;
                OnPropertyChanged();
            }
        }

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

        private ObservableCollection<Participant> _participantsCurrentList = new();
        public ObservableCollection<Participant> ParticipantsCurrentList
        {
            get => _participantsCurrentList;
            set
            {
                _participantsCurrentList = value;
                OnPropertyChanged(nameof(ParticipantsCurrentList));
            }
        }

        private ObservableCollection<Participant> _filteredParticipantsList = new();
        public ObservableCollection<Participant> FilteredParticipantsList
        {
            get => _filteredParticipantsList;
            set { _filteredParticipantsList = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFiltersBind();
            }
        }

        private Curator _selectedCuratorComboBox;
        public Curator SelectedCuratorComboBox
        {
            get => _selectedCuratorComboBox;
            set
            {
                _selectedCuratorComboBox = value;
                OnPropertyChanged();
                ApplyFiltersBind();
            }
        }

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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand BindParticipantCommand { get; }
        public RelayCommand RemoveParticipantCommand { get; }

        public BindUserForGroupViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            CuratorService curatorService,
            GroupService groupService,
            GroupsViewModel groupsViewModel, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _participantService = participantService;
            _curatorService = curatorService;
            _groupService = groupService;
            _groupsViewModel = groupsViewModel;
            _dialogService = dialogService;

            BindParticipantCommand = new RelayCommand(_ => _ = BindParticipantAsync(SelectedParticipant), _ => SelectedParticipant != null && !IsLoading);
            RemoveParticipantCommand = new RelayCommand(_ => _ = RemoveParticipantAsync(SelectedCurrentParticipant), _ => SelectedCurrentParticipant != null && !IsLoading);

            CurrentGroup = _groupsViewModel.SelectedGroup;

            _ = LoadDataAsync();
            _ = LoadCuratorsAsync();
        }

        private async Task LoadCuratorsAsync()
        {
            try
            {
                await _curatorService.InitializeAsync();
                CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки экзаменаторов: {ex.Message}", "Ошибка");
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);
                ParticipantsList = new ObservableCollection<Participant>(allParticipants);
                FilteredParticipantsList = new ObservableCollection<Participant>(allParticipants);

                if (CurrentGroup != null)
                {
                    var boundParticipants = await _participantService.GetAllParticipantForGroupAsync(CurrentGroup.Id);
                    ParticipantsCurrentList = new ObservableCollection<Participant>(boundParticipants);

                    if (_groupsViewModel != null)
                    {
                        _groupsViewModel.ParticipantsForGroupList = ParticipantsCurrentList;
                    }
                }

                ApplyFiltersBind();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task BindParticipantAsync(Participant participant)
        {
            if (participant == null || CurrentGroup == null) return;

            try
            {
                IsLoading = true;

                await _participantService.UpdateParticipantBindForGroupAsync(participant, CurrentGroup, true);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка привязки: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RemoveParticipantAsync(Participant participant)
        {
            if (participant == null || CurrentGroup == null) return;

            try
            {
                IsLoading = true;

                await _participantService.UpdateParticipantBindForGroupAsync(participant, CurrentGroup, false);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка удаления: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ApplyFiltersBind()
        {
            if (ParticipantsList == null) return;

            try
            {
                var availableParticipants = ParticipantsList
                    .Where(p => !ParticipantsCurrentList.Any(bound => bound.Id == p.Id))
                    .AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    availableParticipants = availableParticipants.Where(p =>
                        p.Name != null &&
                        p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (SelectedCuratorComboBox != null)
                {
                    availableParticipants = availableParticipants
                        .Where(p => p.CuratorCreateId == SelectedCuratorComboBox.Id);
                }

                FilteredParticipantsList = new ObservableCollection<Participant>(availableParticipants);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка фильтрации: {ex.Message}", "Ошибка");
            }
        }
    }
}