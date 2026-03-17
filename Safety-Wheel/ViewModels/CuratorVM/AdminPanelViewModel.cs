using Safety_Wheel.Models;
using Safety_Wheel.Pages.Curator;
using Safety_Wheel.Services;
using System.Collections.ObjectModel;
using static Safety_Wheel.ViewModels.MainViewModel;

namespace Safety_Wheel.ViewModels.CuratorVM
{
    public class AdminPanelViewModel : ObservableObject
    {
        public BGroupsParticipantService service { get; set; }
        public enum TabType { Users, Requests, Groups }

        private TabType _selectedTab = TabType.Users;
        public TabType SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        ParticipantService _participantService = new();
        CuratorService _curatorService = new();
        ApplicationService _applicationsService = new();

        private ObservableCollection<Participant> _participantsList;
        public ObservableCollection<Participant> ParticipantsList
        {
            get => _participantsList;
            set
            {
                _participantsList = value;
                OnPropertyChanged(nameof(ParticipantsList));
            }
        }

        private ObservableCollection<Curator> _curatorsList = new();
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set
            {
                _curatorsList = value;
                OnPropertyChanged(nameof(CuratorsList));
            }
        }

        private ObservableCollection<Requests> _applicationsList;
        public ObservableCollection<Requests> ApplicationsList
        {
            get => _applicationsList;
            set
            {
                _applicationsList = value;
                OnPropertyChanged(nameof(ApplicationsList));
            }
        }

        private ObservableCollection<Group> _groupsList;
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                _groupsList = value;
                OnPropertyChanged(nameof(_groupsList));
            }
        }

        public Group? CurrentGroup { get; set; } = null;
        public Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (!SetProperty(ref _selectedParticipant, value))
                    return;
            }
        }

        public AdminPanelViewModel()
        {
            ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            ApplicationsList = new ObservableCollection<Requests>(_applicationsService.Applications);
        }
    }
}
