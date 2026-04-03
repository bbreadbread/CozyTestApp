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

        private GroupService _groupService = new();
        private GroupsViewModel _groupsViewModel;
        public string _name;
        public string _description;
        public string _currentUser;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(Name); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(Description); } }
        public string CurrentUser { get { return _currentUser; } set { _currentUser = value; OnPropertyChanged(CurrentUser); } }

        public RelayCommand AddGroupCommand { get; }

        public CreateEditGroupViewModel(INavigationService navigationService,
 IDialogService dialogService, GroupsViewModel groupsViewModel) : base(navigationService, dialogService)
        {
            _groupsViewModel = groupsViewModel;
            AddGroupCommand = new RelayCommand(_ => AddGroup());

            if (_groupsViewModel.SelectedGroup != null)
            {
                Name = _groupsViewModel.SelectedGroup.Name;
                Description = _groupsViewModel.SelectedGroup.Description;
                CurrentUser = _groupsViewModel.SelectedGroup.Curator.Name;
            }
            else CurrentUser = CozyTest.CurrentUser.Name;
        }

        public void AddGroup()
        {
            if (_groupsViewModel.SelectedGroup != null)
            {
                _groupService.Update(new Models.Group
                {
                    Id = _groupsViewModel.SelectedGroup.Id,
                    Name = Name,
                    Description = Description,
                    CuratorId = CozyTest.CurrentUser.Id
                });
            }
            else
            {
                _groupService.Add(new Models.Group
                {
                    Name = Name,
                    Description = Description,
                    CuratorId = CozyTest.CurrentUser.Id
                });
            }
            
            _groupsViewModel.GroupsList.Clear();
            _groupService.GetAllGroupsForUser();

            foreach (var g in _groupService.Group) {
                _groupsViewModel.GroupsList.Add(g);
            }
        }
    }
}
