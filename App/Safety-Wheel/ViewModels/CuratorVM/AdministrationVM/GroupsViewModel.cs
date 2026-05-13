using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class GroupsViewModel : BaseViewModel
    {
        private readonly GroupService _groupService;
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly IServiceProvider _serviceProvider;

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
                    _ = LoadParticipantsForGroupAsync(SelectedGroup.Id);
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand BindUserForGroupCommand { get; }

        public GroupsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            GroupService groupService,
            ParticipantService participantService,
            CuratorService curatorService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {
            _serviceProvider = serviceProvider;
            _participantService = participantService;
            _groupService = groupService;
            _curatorService = curatorService;

            AddGroupCommand = new RelayCommand(_ => AddGroup());
            EditGroupCommand = new RelayCommand(_ => EditGroup(), _ => SelectedGroup != null);
            DeleteGroupCommand = new RelayCommand(_ => _ = DeleteGroupAsync(), _ => CanDeleteGroup());
            BindUserForGroupCommand = new RelayCommand(_ => BindUserForGroup(), _ => SelectedGroup != null);

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                await _groupService.GetAllGroupsForUserAsync();
                GroupsList = new ObservableCollection<Group>(_groupService.Groups);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки групп: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadParticipantsForGroupAsync(int groupId)
        {
            try
            {
                IsLoading = true;
                var participants = await _participantService.GetAllParticipantForGroupAsync(groupId);
                ParticipantsForGroupList = new ObservableCollection<Participant>(participants);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки участников: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddGroup()
        {
            SelectedGroup = null;
            var vm = ActivatorUtilities.CreateInstance<CreateEditGroupViewModel>(_serviceProvider, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void EditGroup()
        {
            if (SelectedGroup == null) return;
            var vm = ActivatorUtilities.CreateInstance<CreateEditGroupViewModel>(_serviceProvider, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private void BindUserForGroup()
        {
            if (SelectedGroup == null) return;
            var vm = ActivatorUtilities.CreateInstance<BindUserForGroupViewModel>(_serviceProvider, this);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        private bool CanDeleteGroup()
        {
            return SelectedGroup != null &&
                   (ParticipantsForGroupList == null || ParticipantsForGroupList.Count == 0);
        }

        private async Task DeleteGroupAsync()
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
                await _groupService.DeleteAsync(SelectedGroup);
                await LoadAsync();
            }
        }

        public async void ApplyFilters()
        {
            try
            {
                var query = _groupService.Groups.AsEnumerable();

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
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка фильтрации: {ex.Message}", "Ошибка");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadAsync();
            ApplyFilters();
        }
    }
}