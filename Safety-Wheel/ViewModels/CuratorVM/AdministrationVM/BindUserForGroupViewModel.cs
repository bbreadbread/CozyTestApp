using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class BindUserForGroupViewModel : BaseViewModel
    {
        private GroupsViewModel _groupsViewModel;

        public ParticipantService _participantService = new();
        public CuratorService _curatorService = new();
        public GroupService _groupService = new();


        public Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
            }
        }
        
        
        public Group _currentGroup;
        public Group CurrentGroup
        {
            get { return _currentGroup; }
            set { _currentGroup = value; OnPropertyChanged(); }
        }


        public Participant _selectedCurrentParticipant;
        public Participant SelectedCurrentParticipant
        {
            get { return _selectedCurrentParticipant; }
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


        private ObservableCollection<Participant> _filteredParticipantsList;
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

        public BindUserForGroupViewModel(IDialogService dialogService, INavigationService navigationService, GroupsViewModel groupsViewModel) : base(navigationService,dialogService )
        {
            _groupsViewModel = groupsViewModel;
            CurrentGroup = _groupsViewModel.SelectedGroup; 
            LoadData();       
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
        }

        private void LoadData()
        {
            var allParticipants = _participantService.GetAll(CurrentUser.Id);
            ParticipantsList = FilteredParticipantsList = new ObservableCollection<Participant>(allParticipants);

            if (CurrentGroup != null)
            {
                var boundParticipants = _participantService.GetAllParticipantForGroup(CurrentGroup.Id);
                ParticipantsCurrentList = new ObservableCollection<Participant>(boundParticipants);

                _groupsViewModel.ParticipantsForGroupList = ParticipantsCurrentList;
            }
        }

        public void BindParticipant(Participant participant)
        {
            _participantService.UpdateParticipantBindForGroup(participant, CurrentGroup, true);
            LoadData();
        }

        public void RemoveParticipant(Participant participant)
        {
            _participantService.UpdateParticipantBindForGroup(participant, CurrentGroup, false);
            LoadData();
        }

        void ApplyFiltersBind()
        {
            if (ParticipantsList == null) return;

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
    }
}