using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;

namespace CozyTest.ViewModels.CuratorVM
{
    public class PublicDetailsViewModel : ObservableObject
    {
        GroupService _groupService = new();

        private ObservableCollection<Group> _groupsList;

        public ObservableCollection<Group> GroupsList
        {
            get
            {
                _groupsList = _groupService.GetAllGroupsForCurator(CurrentUser.Id);
                return _groupsList;
            }
            set
            {
                _groupsList = value;
                OnPropertyChanged(nameof(GroupsList));
            }
        }

        public PublicDetailsViewModel() => GroupsList = new ObservableCollection<Group>(_groupService.Group);
    }
}
