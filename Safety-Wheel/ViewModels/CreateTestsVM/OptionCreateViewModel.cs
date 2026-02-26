using Safety_Wheel.Models;
using System.Diagnostics;
using System.Windows.Input;
using System.IO;

namespace Safety_Wheel.ViewModels.CreateTestsVM
{
    public class OptionCreateViewModel : ObservableObject
    {
        public Option NewOption { get; set; } = new();

        private bool _isGhost;
        public bool IsGhost
        {
            get => _isGhost;
            set => SetProperty(ref _isGhost, value);
        }

        public bool IsImageOption { get; }

        private readonly QuestionCreateViewModel _parent;

        public string Value
        {
            get => NewOption.TextAnswer ?? "";
            set
            {
                NewOption.TextAnswer = value;
                OnPropertyChanged();

                RecalculateGhostState();

                _parent.SyncGhostOptions();
                _parent.RecalculateQuestionType();
            }
        }

        public bool? IsCorrect
        {
            get => NewOption.IsCorrect;
            set
            {
                NewOption.IsCorrect = value;
                OnPropertyChanged();

                _parent.RecalculateQuestionType();
            }
        }

        public OptionCreateViewModel(
            bool isGhost,
            bool isImageOption,
            QuestionCreateViewModel parent)
        {
            IsGhost = isGhost;
            IsImageOption = isImageOption;
            _parent = parent;

            ShowPreviewCommand = new RelayCommand(_ => ShowPreview());
            DeleteCommand = new RelayCommand(_ => _parent.RemoveOption(this));
        }

        private void RecalculateGhostState()
        {
            IsGhost = string.IsNullOrWhiteSpace(Value);
        }

        public void SetOptionImage(string path)
        {
            Value = path;
        }

        public ICommand DeleteCommand { get; }

        public ICommand ShowPreviewCommand { get; }

        private void ShowPreview()
        {
            _parent.PreviewImagePath = NewOption.TextAnswer;
        }


    }
}
