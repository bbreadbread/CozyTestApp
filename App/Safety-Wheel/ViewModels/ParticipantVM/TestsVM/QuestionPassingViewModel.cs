using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

namespace CozyTest.ViewModels.ParticipantVM.TestsVM
{
    public class QuestionPassingViewModel : ObservableObject
    {
        private readonly OptionService _optionService;
        private readonly CorrespondenceService _correspondenceService;

        public Question Question { get; set; }
        
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
        private bool _IsCorrect;
        public bool IsCorrect
        {
            get => _IsCorrect;
            set
            {
                _IsCorrect = value;
                OnPropertyChanged();
            }
        }
        public string QuestionText { get; set; }
        public string ImagePath { get; set; }
        public string Comments { get; set; }

        public ObservableCollection<OptionPassingViewModel> Options { get; set; } = new();
        public ObservableCollection<OptionPassingViewModel> ConstantOptions { get; set; } = new();
        public ObservableCollection<OptionPassingViewModel> MatchingOptions { get; set; } = new();
        public ObservableCollection<MatchingPairViewModel> MatchingPairs { get; set; } = new();

        private string _textAnswer;
        public string TextAnswer
        {
            get => _textAnswer;
            set => SetProperty(ref _textAnswer, value);
        }

        public int IsQuestionType { get; set; }

        public QuestionPassingViewModel(Question question, OptionService optionService, CorrespondenceService correspondenceService)
        {
            Question = question;
            _optionService = optionService;
            _correspondenceService = correspondenceService;

            if (question.QuestionTypeId is int q)
            {
                switch (q)
                {
                    case 1: IsQuestionType = 1; break;
                    case 2: IsQuestionType = 2; break;
                    case 3: IsQuestionType = 3; break;
                }
            }
        }

        public async Task LoadOptionsAsync()
        {
            var options = Question.Options?.OrderBy(o => o.Number).ToList() ?? new List<Option>();

            switch (Question.QuestionTypeId)
            {
                case 1:
                    foreach (var opt in options)
                    {
                        var optVM = new OptionPassingViewModel
                        {
                            Option = opt,
                            IsSelected = false
                        };
                        Options.Add(optVM);
                    }
                    break;

                case 2:
                    TextAnswer = string.Empty;
                    break;

                case 3:
                    var correspondences = await _correspondenceService.GetByQuestionIdAsync(Question.Id);
                    var constantIds = correspondences.Select(c => c.ConstantId).Distinct().ToList();

                    foreach (var opt in options.Where(o => constantIds.Contains(o.Id)))
                    {
                        var optVM = new OptionPassingViewModel
                        {
                            Option = opt,
                            IsConstant = true
                        };
                        ConstantOptions.Add(optVM);
                    }

                    foreach (var opt in options.Where(o => !constantIds.Contains(o.Id)))
                    {
                        var optVM = new OptionPassingViewModel
                        {
                            Option = opt
                        };
                        MatchingOptions.Add(optVM);
                    }

                    for (int i = 0; i < ConstantOptions.Count; i++)
                    {
                        var constantOpt = ConstantOptions[i];
                        var corrData = correspondences.FirstOrDefault(c => c.ConstantId == constantOpt.Option.Id);

                        var matchingPair = new MatchingPairViewModel
                        {
                            ConstantOption = constantOpt,
                            AvailableMatches = new ObservableCollection<OptionPassingViewModel>(MatchingOptions),
                            SelectedMatch = null
                        };

                        if (corrData != null)
                        {
                            var matchedOption = MatchingOptions.FirstOrDefault(m => m.Option.Id == corrData.СorrespondingId);
                            if (matchedOption != null)
                            {
                                matchingPair.SelectedMatch = matchedOption;
                            }
                        }

                        MatchingPairs.Add(matchingPair);
                    }
                    break;
            }
        }

        public void LoadContent()
        {
            QuestionText = Question?.TestQuest;
            ImagePath = Question?.PicturePath;
            Comments = Question?.Comments;
        }

        public string AbsoluteImagePath => GetAbsoluteImagePath(ImagePath);

        public static string GetAbsoluteImagePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "user_tests_images",
                normalizedPath);
        }
    }

    public class OptionPassingViewModel : ObservableObject
    {
        public Option Option { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsConstant { get; set; }
        public string DisplayText => Option?.TextAnswer ?? string.Empty;
        public bool IsImage => Option?.IsImage ?? false;
        public string AbsoluteImagePath => QuestionPassingViewModel.GetAbsoluteImagePath(Option?.TextAnswer);
    }

    public class MatchingPairViewModel : ObservableObject
    {
        public OptionPassingViewModel ConstantOption { get; set; }
        public ObservableCollection<OptionPassingViewModel> AvailableMatches { get; set; }

        private OptionPassingViewModel _selectedMatch;
        public OptionPassingViewModel SelectedMatch
        {
            get => _selectedMatch;
            set => SetProperty(ref _selectedMatch, value);
        }
    }
}