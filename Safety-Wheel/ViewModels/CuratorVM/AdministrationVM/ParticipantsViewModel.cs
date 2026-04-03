using CozyTest.ForShellWindow;
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
    public class ParticipantsViewModel : BaseAdminViewModel
    {
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

        private ObservableCollection<Group> _groupsListCurrent;
        public ObservableCollection<Group> GroupsListCurrent
        {
            get => _groupsListCurrent;
            set => SetProperty(ref _groupsListCurrent, value);
        }

        private ObservableCollection<Curator> _curatorsListCurrent;
        public ObservableCollection<Curator> CuratorsListCurrent
        {
            get => _curatorsListCurrent;
            set => SetProperty(ref _curatorsListCurrent, value);
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (SetProperty(ref _selectedParticipant, value))
                {
                    LoadForCurrentParticipant();
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


        public RelayCommand SearchParticipantCommand { get; }
        public RelayCommand AddParticipantCommand { get; }
        public RelayCommand EditParticipantCommand { get; }
        public RelayCommand ArchiveParticipantCommand { get; }

        public ParticipantsViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
            BindIsChecked = true;
            ActiveIsChecked = true;

            SearchParticipantCommand = new RelayCommand(_ => SearchParticipant());
            AddParticipantCommand = new RelayCommand(_ => AddParticipant());
            EditParticipantCommand = new RelayCommand(_ => EditParticipant(), _ => SelectedParticipant != null);
            ArchiveParticipantCommand = new RelayCommand(_ => ArchiveParticipant(), _ => SelectedParticipant != null);
        }

        private void SearchParticipant()
        {
            var createVmUser = new SearchParticipantViewModel(_dialogService, _navigationService, this);
            _dialogService.ShowWindow<ShellWindow>(createVmUser);
        }

        private void AddParticipant()
        {
            SelectedParticipant = null;
            var createVm = new CreateEditParticipantViewModel(_dialogService, _navigationService, this);
            _dialogService.ShowWindow<ShellWindow>(createVm);
        }

        private void EditParticipant()
        {
            if (SelectedParticipant == null) return;

            if (CurrentUser.TypeUser == 1)
            {
                var editVm = new CreateEditParticipantViewModel(_dialogService, _navigationService, this);
                _dialogService.ShowWindow<ShellWindow>(editVm);
            }
            else
            {
                var editVm = new CreateEditParticipantViewModel(_dialogService, _navigationService, this);
                _dialogService.ShowWindow<ShellWindow>(editVm);
            }
        }

        private void LoadForCurrentParticipant()
        {
            if (SelectedParticipant != null)
            {
                _groupService.GetAllGroupsForUser(SelectedParticipant.Id);
                GroupsListCurrent = new ObservableCollection<Group>(_groupService.Group);

                _curatorService.GetAll(SelectedParticipant.Id);
                CuratorsListCurrent = new ObservableCollection<Curator>(_curatorService.Curators);
            }
        }

        private void ArchiveParticipant()
        {
            if (SelectedParticipant == null) return;

            _participantService.UpdateParticipantArchiveStatus(SelectedParticipant.Id);
            ApplyFilters();
        }

        public override void ApplyFilters()
        {
            var queryList = _participantService.Participants.AsEnumerable();
            var queryListBind = _participantService.GetAllBind(CurrentUser.Id);

            if (BindIsChecked)
                queryList = queryListBind;

            if (CreateIsChecked)
                queryList = queryList.Where(o => o.CuratorCreateId == CurrentUser.Id);

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

        public void ReloadParticipants()
        {
            _participantService.GetAllParticipants(CurrentUser.Id);

            ApplyFilters();
        }
    }
}