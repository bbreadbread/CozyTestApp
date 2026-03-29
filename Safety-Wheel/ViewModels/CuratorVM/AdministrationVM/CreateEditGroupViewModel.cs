using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditGroupViewModel : ObservableObject
    {
        AdminPanelViewModel _viewModel;

        private GroupService _groupService = new();
        public string _name;
        public string _description;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(Name); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(Description); } }
        public string CurrentUser { get { return CozyTest.CurrentUser.Name; } set { CurrentUser = CozyTest.CurrentUser.Name; OnPropertyChanged(CurrentUser); } }

        public RelayCommand AddGroupCommand { get; }

        public CreateEditGroupViewModel() { }
        public CreateEditGroupViewModel(AdminPanelViewModel viewModel)
        {
            _viewModel = viewModel;
            AddGroupCommand = new RelayCommand(_ => AddGroup());
        }

        public void AddGroup()
        {
            _groupService.Add(new Models.Group
            {
                Name = Name,
                Description = Description,
                CuratorId = CozyTest.CurrentUser.Id
            });
            _viewModel.ApplyFiltersGroups();
        }
    }
}
