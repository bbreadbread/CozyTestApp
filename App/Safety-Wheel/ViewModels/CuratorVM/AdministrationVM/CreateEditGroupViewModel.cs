using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditGroupViewModel : BaseViewModel
    {
        public override string WindowTitle => "Управление группой";

        private readonly GroupService _groupService;
        private readonly GroupsViewModel _groupsViewModel;
        private readonly IDialogService _dialogService;

        private string _name;
        private string _description;
        private string _currentUser;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public RelayCommand AddGroupCommand { get; }

        public CreateEditGroupViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            GroupService groupService,
            GroupsViewModel groupsViewModel) : base(navigationService, dialogService)
        {
            _groupService = groupService;
            _groupsViewModel = groupsViewModel;
            _dialogService = dialogService;
            AddGroupCommand = new RelayCommand(_ => _ = AddGroupAsync(), _ => !IsLoading);

            if (_groupsViewModel.SelectedGroup != null)
            {
                Name = _groupsViewModel.SelectedGroup.Name;
                Description = _groupsViewModel.SelectedGroup.Description;
                CurrentUser = _groupsViewModel.SelectedGroup.Curator?.Name ?? CozyTest.CurrentUser.Name;
            }
            else
            {
                CurrentUser = CozyTest.CurrentUser.Name;
            }
        }

        public async Task AddGroupAsync()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(Name))
                {
                    _dialogService.ShowMessage("Введите название группы", "Ошибка");
                    return;
                }

                if (_groupsViewModel.SelectedGroup != null)
                {
                    var updatedGroup = new Models.Group
                    {
                        Id = _groupsViewModel.SelectedGroup.Id,
                        Name = Name,
                        Description = Description,
                        CuratorId = CozyTest.CurrentUser.Id
                    };
                    await _groupService.UpdateAsync(updatedGroup);
                    _dialogService.ShowMessage("Группа обновлена", "Успех");
                }
                else
                {
                    var newGroup = new Models.Group
                    {
                        Name = Name,
                        Description = Description,
                        CuratorId = CozyTest.CurrentUser.Id
                    };
                    await _groupService.AddAsync(newGroup);
                    _dialogService.ShowMessage("Группа добавлена", "Успех");
                }

                await _groupsViewModel.LoadAsync();
                _groupsViewModel.ApplyFilters();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка сохранения группы: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}