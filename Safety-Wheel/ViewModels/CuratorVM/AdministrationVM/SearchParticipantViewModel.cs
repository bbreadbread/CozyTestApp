using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class SearchParticipantViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка привязки тестируемых к Вам";

        private readonly ParticipantsViewModel _parentViewModel;
        public ParticipantService _participantService = new();
        public CuratorService _curatorService = new();

        private ObservableCollection<Participant> _allParticipants = new();  
        private ObservableCollection<Participant> _boundParticipants = new();

        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set { _selectedParticipant = value; OnPropertyChanged(); }
        }
        private Participant _selectedParticipant;

        public Participant SelectedCurrentParticipant
        {
            get => _selectedCurrentParticipant;
            set { _selectedCurrentParticipant = value; OnPropertyChanged(); }
        }
        private Participant _selectedCurrentParticipant;

        public ObservableCollection<Participant> ParticipantsList 
        {
            get => _participantsList;
            set { _participantsList = value; OnPropertyChanged(nameof(ParticipantsList)); }
        }
        private ObservableCollection<Participant> _participantsList = new();

        public ObservableCollection<Participant> ParticipantsCurrentList
        {
            get => _participantsCurrentList;
            set { _participantsCurrentList = value; OnPropertyChanged(nameof(ParticipantsCurrentList)); }
        }
        private ObservableCollection<Participant> _participantsCurrentList = new();

        public ObservableCollection<Participant> FilteredParticipantsList
        {
            get => _filteredParticipantsList;
            set { _filteredParticipantsList = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Participant> _filteredParticipantsList;

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFiltersBind(); }
        }
        private string _searchText;

        public Curator SelectedCuratorComboBox
        {
            get => _selectedCuratorComboBox;
            set { _selectedCuratorComboBox = value; OnPropertyChanged(); ApplyFiltersBind(); }
        }
        private Curator _selectedCuratorComboBox;

        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set { _curatorsList = value; OnPropertyChanged(nameof(CuratorsList)); }
        }
        private ObservableCollection<Curator> _curatorsList;

        public SearchParticipantViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantsViewModel participantsViewModel) : base(navigationService, dialogService)
        {
            _parentViewModel = participantsViewModel;
            LoadData();
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
        }

        private void LoadData()
        {
            _allParticipants = new ObservableCollection<Participant>(_participantService.GetAllActive(CurrentUser.Id));

            _boundParticipants = new ObservableCollection<Participant>(_participantService.GetAllBind(CurrentUser.Id));
            ParticipantsCurrentList = new ObservableCollection<Participant>(_boundParticipants);

            UpdateParticipantsList();
        }

        private void UpdateParticipantsList()
        {
            var boundIds = _boundParticipants.Select(p => p.Id).ToHashSet();
            var available = _allParticipants.Where(p => !boundIds.Contains(p.Id));

            ParticipantsList = new ObservableCollection<Participant>(available);
            ApplyFiltersBind();
        }

        public void BindParticipant(Participant participant)
        {
            if (participant == null) return;

            _participantService.UpdateParticipantBindForCurator(participant.Id, CurrentUser.Id, true);

            _allParticipants.Remove(participant);
            _boundParticipants.Add(participant);
            ParticipantsCurrentList.Add(participant);

            UpdateParticipantsList();
            _parentViewModel?.ApplyFilters();
        }

        public void RemoveParticipant(Participant participant)
        {
            if (participant == null) return;

            _participantService.UpdateParticipantBindForCurator(participant.Id, CurrentUser.Id, false);

            _boundParticipants.Remove(participant);
            ParticipantsCurrentList.Remove(participant);
            _allParticipants.Add(participant);

            UpdateParticipantsList();
            _parentViewModel?.ApplyFilters();
        }

        void ApplyFiltersBind()
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
        }
    }
}