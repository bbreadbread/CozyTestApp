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

        GroupService _groupService;

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

        public BindGroupForUserViewModel(GroupService groupService ,Participant newPart)
        {
            _groupService = groupService;
            _groupService.GetAllGroupsForUserAsync(newPart.Id);
            GroupsListCurrent = new ObservableCollection<Group>(_groupService.Groups);
            GroupsListCurrent = new ObservableCollection<Group>();

            GroupsList = new ObservableCollection<Group>(_groupService.Groups);
        }
    }
}
