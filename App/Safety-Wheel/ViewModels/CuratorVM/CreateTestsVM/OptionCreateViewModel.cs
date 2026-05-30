using CozyTest.Models;
using CozyTest.Services;
using System.Windows.Input;
using System.IO;
using System.Collections.ObjectModel;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class OptionCreateViewModel : ObservableObject
    {
        public Option NewOption { get; set; } = new() { IsCorrect = false };

        public bool IsImage { get; set; }

        private readonly QuestionCreateViewModel _parent;
        private readonly CuratorCreateTestViewModel _testParent;
        public bool IsConstant { get; set; }
        public bool IsCorresponding { get; set; }

        private OptionCreateViewModel _selectedMatch;
        public OptionCreateViewModel SelectedMatch
        {
            get => _selectedMatch;
            set => SetProperty(ref _selectedMatch, value);
        }

        private ObservableCollection<OptionCreateViewModel> _availableMatches;
        public ObservableCollection<OptionCreateViewModel> AvailableMatches
        {
            get => _availableMatches;
            set => SetProperty(ref _availableMatches, value);
        }

        private bool _isAnswered;
        public bool IsAnswered
        {
            get => _isAnswered;
            set
            {
                _isAnswered = value;
                OnPropertyChanged();
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                _IsSelected = value;
                OnPropertyChanged();
            }
        }


        public string Value
        {
            get => NewOption.TextAnswer ?? "";
            set
            {
                NewOption.TextAnswer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AbsoluteImagePath));
            }
        }

        public string AbsoluteImagePath => _parent.GetAbsoluteImagePath(Value);

        public bool? IsCorrect
        {
            get => NewOption.IsCorrect;
            set
            {
                NewOption.IsCorrect = value;
                OnPropertyChanged();
            }
        }

        private bool _CanShow;
        public bool CanShow
        {
            get => _CanShow;
            set
            {
                _CanShow = value;
                OnPropertyChanged();
            }
        }
        public ICommand DeleteCommand { get; }
        public ICommand ShowFullScreenImageCommand { get; }

        public OptionCreateViewModel(bool isImage, QuestionCreateViewModel parent, CuratorCreateTestViewModel testParent, bool isComplianceOption = false)
        {
            IsImage = isImage;
            _parent = parent;
            _testParent = testParent;


            NewOption.IsImage = isImage;

            if (isComplianceOption)
                DeleteCommand = new RelayCommand(_ => _parent.RemoveComplianceOption(this));
            else
                DeleteCommand = new RelayCommand(_ => _parent.RemoveOption(this));

            ShowFullScreenImageCommand = new RelayCommand(_ => ShowFullScreenImage());
        }

        public void SetImagePath(string path)
        {
            Value = path;
        }

        private void ShowFullScreenImage()
        {
            if (!IsImage || string.IsNullOrEmpty(Value))
                return;

            string absolutePath = _parent.GetAbsoluteImagePath(Value);

            if (File.Exists(absolutePath))
            {
                FullScreenImageService.ShowImage(absolutePath);
            }
        }
    }
}