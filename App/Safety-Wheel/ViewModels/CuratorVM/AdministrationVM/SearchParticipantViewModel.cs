using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class SearchParticipantViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка привязки тестируемых к Вам";

        private readonly ParticipantsViewModel _parentViewModel;
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly GroupService _groupService;

        private ObservableCollection<Participant> _allParticipants = new();
        private ObservableCollection<Participant> _boundParticipants = new();

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set { _selectedParticipant = value; OnPropertyChanged(); }
        }

        private Participant _selectedCurrentParticipant;
        public Participant SelectedCurrentParticipant
        {
            get => _selectedCurrentParticipant;
            set { _selectedCurrentParticipant = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Participant> _participantsList = new();
        public ObservableCollection<Participant> ParticipantsList
        {
            get => _participantsList;
            set { _participantsList = value; OnPropertyChanged(nameof(ParticipantsList)); }
        }

        private ObservableCollection<Participant> _participantsCurrentList = new();
        public ObservableCollection<Participant> ParticipantsCurrentList
        {
            get => _participantsCurrentList;
            set { _participantsCurrentList = value; OnPropertyChanged(nameof(ParticipantsCurrentList)); }
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
            set { _searchText = value; OnPropertyChanged(); ApplyFiltersBind(); }
        }

        private Curator _selectedCuratorComboBox;
        public Curator SelectedCuratorComboBox
        {
            get => _selectedCuratorComboBox;
            set { _selectedCuratorComboBox = value; OnPropertyChanged(); ApplyFiltersBind(); }
        }

        private ObservableCollection<Curator> _curatorsList;
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set { _curatorsList = value; OnPropertyChanged(nameof(CuratorsList)); }
        }

        public SearchParticipantViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            CuratorService curatorService,
            GroupService groupService,
            ParticipantsViewModel participantsViewModel) : base(navigationService, dialogService)
        {
            _participantService = participantService;
            _curatorService = curatorService;
            _groupService = groupService;
            _parentViewModel = participantsViewModel;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var allParticipantsTask = _participantService.GetAllActiveAsync(CurrentUser.Id);
                var boundParticipantsTask = _participantService.GetAllBindAsync(CurrentUser.Id);
                var curatorsTask = _curatorService.GetAllAsync();

                await Task.WhenAll(allParticipantsTask, boundParticipantsTask, curatorsTask);

                _allParticipants = new ObservableCollection<Participant>(await allParticipantsTask);
                _boundParticipants = new ObservableCollection<Participant>(await boundParticipantsTask);
                ParticipantsCurrentList = new ObservableCollection<Participant>(_boundParticipants);
                CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);

                UpdateParticipantsList();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateParticipantsList()
        {
            var boundIds = _boundParticipants.Select(p => p.Id).ToHashSet();
            var available = _allParticipants.Where(p => !boundIds.Contains(p.Id));

            ParticipantsList = new ObservableCollection<Participant>(available);
            ApplyFiltersBind();
        }

        public async void BindParticipant(Participant participant)
        {
            if (participant == null) return;

            await _participantService.UpdateParticipantBindForCuratorAsync(participant.Id, CurrentUser.Id, true);

            _allParticipants.Remove(participant);
            _boundParticipants.Add(participant);
            ParticipantsCurrentList.Add(participant);

            UpdateParticipantsList();

            if (_parentViewModel != null)
            {
                await _parentViewModel.ReloadParticipantsAsync();
                await _parentViewModel.LoadForCurrentParticipantAsync();
            }
        }

        public async void RemoveParticipant(Participant participant)
        {
            if (participant == null) return;

            await _participantService.UpdateParticipantBindForCuratorAsync(participant.Id, CurrentUser.Id, false);

            _boundParticipants.Remove(participant);
            ParticipantsCurrentList.Remove(participant);
            _allParticipants.Add(participant);

            UpdateParticipantsList();

            if (_parentViewModel != null)
            {
                await _parentViewModel.ReloadParticipantsAsync();
                if (_parentViewModel.SelectedParticipant?.Id == participant.Id)
                {
                    await _parentViewModel.LoadForCurrentParticipantAsync();
                }
            }
        }

        private void ApplyFiltersBind()
        {
            var filtered = ParticipantsList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p =>
                    p.Name != null &&
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedCuratorComboBox != null)
            {
                filtered = filtered.Where(p => p.CuratorCreateId == SelectedCuratorComboBox.Id);
            }

            FilteredParticipantsList = new ObservableCollection<Participant>(filtered);
        }

        public void SelectParticipantForParent(Participant participant)
        {
            _parentViewModel.SelectedParticipant = participant;
            _parentViewModel.ParticipantsVisibility = Visibility.Visible;
        }
    }
}