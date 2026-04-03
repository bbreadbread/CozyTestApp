using CozyTest.Models;
using CozyTest.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class GroupsViewModel : BaseViewModel
    {

        private readonly GroupService _groupService = new();
        private readonly ParticipantService _participantService = new();

        private ObservableCollection<Group> _groupsList;
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                if (SetProperty(ref _groupsList, value))
                {
                    ApplyFilters();
                }
            }
        }

        private ObservableCollection<Participant> _participantsForGroupList;
        public ObservableCollection<Participant> ParticipantsForGroupList
        {
            get => _participantsForGroupList;
            set => SetProperty(ref _participantsForGroupList, value);
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value) && SelectedGroup != null)
                {
                    ParticipantsForGroupList = _participantService.GetAllParticipantForGroup(SelectedGroup.Id);
                }
            }
        }

        private string _searchTextGroup;
        public string SearchTextGroup
        {
            get => _searchTextGroup;
            set
            {
                _searchTextGroup = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand BindUserForGroupCommand { get; }

        public GroupsViewModel(IDialogService dialogService, INavigationService navigationService) : base(navigationService, dialogService)
        {
            _groupService.GetAllGroupsForUser();
            GroupsList = new ObservableCollection<Group>(_groupService.Group);

            AddGroupCommand = new RelayCommand(_ => AddGroup());
            EditGroupCommand = new RelayCommand(_ => EditGroup(), _ => SelectedGroup != null);
            DeleteGroupCommand = new RelayCommand(_ => DeleteGroup(), _ => CanDeleteGroup());
            BindUserForGroupCommand = new RelayCommand(_ => BindUserForGroup(), _ => SelectedGroup != null);
        }

        private void AddGroup()
        {
            SelectedGroup = null;

            var vm = new CreateEditGroupViewModel( _navigationService, _dialogService, this);
           _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void EditGroup()
        {
            if (SelectedGroup == null) return;

            var vm = new CreateEditGroupViewModel(_navigationService, _dialogService, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void BindUserForGroup()
        {
            if (SelectedGroup == null) return;

            var vm = new BindUserForGroupViewModel(_dialogService,_navigationService, this);
           _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private bool CanDeleteGroup()
        {
            return SelectedGroup != null &&
                   (ParticipantsForGroupList == null || ParticipantsForGroupList.Count == 0);
        }

        private void DeleteGroup()
        {
            if (SelectedGroup == null) return;

            if (!CanDeleteGroup())
            {
                _dialogService.ShowMessage("Нельзя удалить группу, в которой есть участники!", "Ошибка");
                return;
            }

            if (_dialogService.ShowConfirmation(
                $"Вы уверены, что хотите удалить группу '{SelectedGroup.Name}'?",
                "Подтверждение удаления"))
            {
                _groupService.Delete(SelectedGroup);
                ApplyFilters();
                _dialogService.ShowMessage("Группа удалена", "Успех");
            }
        }

        public void ApplyFilters()
        {
            var query = _groupService.Group.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTextGroup))
            {
                query = query.Where(p =>
                    p.Name != null &&
                    p.Name.Contains(SearchTextGroup, StringComparison.OrdinalIgnoreCase));
            }

            GroupsList.Clear();
            foreach (var r in query)
            {
                GroupsList.Add(r);
            }

            if (SelectedGroup != null && !GroupsList.Contains(SelectedGroup))
            {
                SelectedGroup = null;
            }
        }
    }
}