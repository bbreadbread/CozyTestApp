using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using ScottPlot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.StatisticsVM
{
    public class StatisticsCurrentTestViewModel : BaseViewModel
    {
        private readonly AttemptService _attemptService;
        private readonly ParticipantAnswerService _participantAnswerService;

        public Test CurrentTest { get; }

        private int _totalResponses;
        public int TotalResponses
        {
            get => _totalResponses;
            set { _totalResponses = value; OnPropertyChanged(); }
        }

        private double _averageScore;
        public double AverageScore
        {
            get => _averageScore;
            set { _averageScore = value; OnPropertyChanged(); }
        }

        private double _medianScore;
        public double MedianScore
        {
            get => _medianScore;
            set { _medianScore = value; OnPropertyChanged(); }
        }

        private double _minScore;
        public double MinScore
        {
            get => _minScore;
            set { _minScore = value; OnPropertyChanged(); }
        }

        private double _maxScore;
        public double MaxScore
        {
            get => _maxScore;
            set { _maxScore = value; OnPropertyChanged(); }
        }
        private int _countQuest;
        public int CountQuest
        {
            get => _countQuest;
            set { _countQuest = value; OnPropertyChanged(); }
        }

        private double[] _scoreDistributionData;
        public double[] ScoreDistributionData
        {
            get => _scoreDistributionData;
            set { _scoreDistributionData = value; OnPropertyChanged(); }
        }

        private string[] _scoreDistributionLabels;
        public string[] ScoreDistributionLabels
        {
            get => _scoreDistributionLabels;
            set { _scoreDistributionLabels = value; OnPropertyChanged(); }
        }

        private ObservableCollection<QuestionSummary> _QuestionsWrong = new();
        public ObservableCollection<QuestionSummary> QuestionsWrong
        {
            get => _QuestionsWrong;
            set { _QuestionsWrong = value; OnPropertyChanged(); }
        }

        private ObservableCollection<QuestionDetailStatistics> _questionDetails = new();
        public ObservableCollection<QuestionDetailStatistics> QuestionDetails
        {
            get => _questionDetails;
            set { _questionDetails = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Participant> _participants = new();
        public ObservableCollection<Participant> Participants
        {
            get => _participants;
            set { _participants = value; OnPropertyChanged(); }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
                _ = LoadTestStatisticsAsync();
            }
        }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public StatisticsCurrentTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            Test currentTest,
            AttemptService attemptService,
            ParticipantAnswerService participantAnswerService)
            : base(navigationService, dialogService)
        {
            CurrentTest = currentTest;
            _attemptService = attemptService;
            _participantAnswerService = participantAnswerService;

            RefreshCommand = new RelayCommand(async _ => await LoadTestStatisticsAsync());
            ClearFilterCommand = new RelayCommand(_ => SelectedParticipant = null);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var attempts = await _attemptService.GetAttemptsByTestAsync(CurrentTest.Id);
            var participantIds = attempts
                .Where(a => a.ParticipantId.HasValue)
                .Select(a => a.ParticipantId.Value)
                .Distinct()
                .ToList();

            var participants = attempts
                .Where(a => a.Participant != null)
                .Select(a => a.Participant)
                .DistinctBy(p => p.Id)
                .OrderBy(p => p.Name)
                .ToList();

            Participants = new ObservableCollection<Participant>(participants);

            await LoadTestStatisticsAsync();
        }

        private async Task LoadTestStatisticsAsync()
        {
            var attempts = await _attemptService.GetAttemptsByTestAsync(CurrentTest.Id);

            var filteredAttempts = SelectedParticipant == null
                ? attempts.Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue).ToList()
                : attempts.Where(a => a.ParticipantId == SelectedParticipant.Id
                                    && a.StartedAt.HasValue && a.FinishedAt.HasValue).ToList();

            TotalResponses = filteredAttempts.Count;

            var scores = filteredAttempts
                .Where(a => a.Score.HasValue)
                .Select(a => a.Score.Value)
                .OrderBy(s => s)
                .ToList();

            if (scores.Any())
            {
                CountQuest = (int)filteredAttempts[0].CountQuestions;
                AverageScore = Math.Round(scores.Average(), 1);
                MinScore = scores.Min();
                MaxScore = scores.Max();

                int count = scores.Count;
                MedianScore = count % 2 == 0
                    ? (scores[count / 2 - 1] + scores[count / 2]) / 2.0
                    : scores[count / 2];
            }
            else
            {
                CountQuest = scores.Count;
                AverageScore = 0;
                MedianScore = 0;
                MinScore = 0;
                MaxScore = 0;
            }

            var distribution = new double[(int)filteredAttempts[0].CountQuestions];
            var labels = Enumerable.Range(0, (int)filteredAttempts[0].CountQuestions).Select(i => i.ToString()).ToArray();

            foreach (var score in scores)
            {
                int index = (int)Math.Round((double)score);
                if (index >= 0 && index <= (int)filteredAttempts[0].CountQuestions)
                    distribution[index]++;
            }

            ScoreDistributionData = distribution;
            ScoreDistributionLabels = labels;
            var wrongs = new ObservableCollection<QuestionSummary>();
            var details = new ObservableCollection<QuestionDetailStatistics>();

            foreach (var question in CurrentTest.Questions.OrderBy(q => q.NumberActual))
            {
                var answers = _participantAnswerService.GetAnswersByQuestion(question.Id);

                var filteredAnswers = SelectedParticipant == null
                    ? answers.ToList()
                    : answers.Where(a => a.Attempt.ParticipantId == SelectedParticipant.Id).ToList();

                if (!filteredAnswers.Any()) continue;

                var totalAnswers = filteredAnswers.Count;
                var correctCount = filteredAnswers.Count(a => a.IsCorrect == true);
                var incorrectCount = totalAnswers - correctCount;
                var correctPercent = (double)correctCount / totalAnswers * 100;
                var incorrectPercent = (double)incorrectCount / totalAnswers * 100;

                if (incorrectPercent > 50)
                {
                    wrongs.Add(new QuestionSummary
                    {
                        QuestionText = question.TestQuest,
                        CorrectCount = correctCount,
                        TotalCount = totalAnswers
                    });
                }

                var optionStats = new ObservableCollection<OptionDetail>();
                if (question.Options?.Any() == true)
                {
                    foreach (var option in question.Options)
                    {
                        var optionCount = filteredAnswers.Count(a => a.OptionId == option.Id);
                        var optionPercent = totalAnswers > 0
                            ? Math.Round((double)optionCount / totalAnswers * 100, 1)
                            : 0;

                        optionStats.Add(new OptionDetail
                        {
                            Text = option.TextAnswer,
                            Count = optionCount,
                            Percentage = optionPercent,
                            IsCorrect = option.IsCorrect ?? false,
                            BarWidth = optionPercent
                        });
                    }
                }

                details.Add(new QuestionDetailStatistics
                {
                    QuestionText = question.TestQuest,
                    CorrectCount = correctCount,
                    TotalCount = totalAnswers,
                    Options = optionStats,
                    ChartData = optionStats.Select(o => (double)o.Count).ToArray(),
                    ChartLabels = optionStats.Select(o => o.Text).ToArray()
                });
            }

            QuestionsWrong = wrongs;
            QuestionDetails = details;
        }

    }

    public class QuestionSummary
    {
        public string QuestionText { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public string RatioText => $"{CorrectCount}/{TotalCount}";
    }

    public class QuestionDetailStatistics
    {
        public string QuestionText { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public string CorrectRatioText => $"Верных ответов: {CorrectCount} из {TotalCount}";
        public ObservableCollection<OptionDetail> Options { get; set; } = new();
        public double[] ChartData { get; set; }
        public string[] ChartLabels { get; set; }
    }

    public class OptionDetail
    {
        public string Text { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public bool IsCorrect { get; set; }
        public double BarWidth { get; set; }
        public string PercentageText => $"{Percentage:F1} %";
        public string CountText => $"{Count} ({Percentage:F0} %)";
    }
}