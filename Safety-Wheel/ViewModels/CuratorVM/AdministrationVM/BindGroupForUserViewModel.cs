using CozyTest.Models;
using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    class BindGroupForUserViewModel : ObservableObject
    {
        GroupService _groupService = new();

        private ObservableCollection<Group> _groupsList;
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                SetProperty(ref _groupsList, value);
            }
        }


        private ObservableCollection<Group> _groupsListCurrent;
        public ObservableCollection<Group> GroupsListCurrent
        {
            get => _groupsListCurrent;
            set
            {
                SetProperty(ref _groupsListCurrent, value);
            }
        }

        public BindGroupForUserViewModel(Participant newPart)
        {
            _groupService.GetAllGroupsForUser(newPart.Id);
            GroupsListCurrent = new ObservableCollection<Group>(_groupService.Group);
            GroupsListCurrent = new ObservableCollection<Group>();

            GroupsList = new ObservableCollection<Group>(_groupService.Group);
        }
    }
}
