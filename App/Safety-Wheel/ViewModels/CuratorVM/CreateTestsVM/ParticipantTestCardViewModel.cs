using CozyTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CozyTest.ViewModels.ParticipantVM
{
    public class ParticipantTestCardViewModel : ObservableObject
    {
        public Test Test { get; }
        private readonly List<Attempt> _attempts;
        private readonly ParticipantsAssignedTest? _assignment;
        private readonly ParticipantsPublicTest? _publicTest;
        private readonly bool _isPublicAccess;

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set { _isFavorite = value; OnPropertyChanged(); }
        }

        private string _testTypeName;
        public string TestTypeName
        {
            get => _testTypeName;
            private set { _testTypeName = value; OnPropertyChanged(); }
        }

        private string _topicName;
        public string TopicName
        {
            get => _topicName;
            private set { _topicName = value; OnPropertyChanged(); }
        }

        private int _questionsCount = 0;
        public int QuestionsCount
        {
            get => _questionsCount;
            set { _questionsCount = value; OnPropertyChanged(); }
        }

        private int _timeLimit;
        public int TimeLimit
        {
            get => _timeLimit;
            set { _timeLimit = value; OnPropertyChanged(); }
        }

        private int _maxAttempts;
        public int MaxAttempts
        {
            get => _maxAttempts;
            set { _maxAttempts = value; OnPropertyChanged(); }
        }

        private string _testName;
        public string TestName
        {
            get => _testName;
            private set { _testName = value; OnPropertyChanged(); }
        }

        private string _testDescription;
        public string TestDescription
        {
            get => _testDescription;
            private set { _testDescription = value; OnPropertyChanged(); }
        }

        private string _creatorName;
        public string CreatorName
        {
            get => _creatorName;
            private set { _creatorName = value; OnPropertyChanged(); }
        }

        private DateTime _dateOfCreating;
        public DateTime DateOfCreating
        {
            get => _dateOfCreating;
            private set { _dateOfCreating = value; OnPropertyChanged(); }
        }

        private int _attemptsUsed;
        public int AttemptsUsed
        {
            get => _attemptsUsed;
            private set { _attemptsUsed = value; OnPropertyChanged(); }
        }

        public int _availableAttempts;
        public int AvailableAttempts
        {
            get => _availableAttempts;
            private set { _availableAttempts = value; OnPropertyChanged(); }
        }

        private bool _canStart;
        public bool CanStart
        {
            get => _canStart;
            private set { _canStart = value; OnPropertyChanged(); }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            private set { _isCompleted = value; OnPropertyChanged(); }
        }

        private bool _isAssigned;
        public bool IsAssigned
        {
            get => _isAssigned;
            private set { _isAssigned = value; OnPropertyChanged(); }
        }

        private bool _isPublicAccessProperty;
        public bool IsPublicAccess
        {
            get => _isPublicAccessProperty;
            private set { _isPublicAccessProperty = value; OnPropertyChanged(); }
        }

        private bool _isAvailableByDate;
        public bool IsAvailableByDate
        {
            get => _isAvailableByDate;
            private set { _isAvailableByDate = value; OnPropertyChanged(); }
        }

        private string _assignmentInfo;
        public string AssignmentInfo
        {
            get => _assignmentInfo;
            private set { _assignmentInfo = value; OnPropertyChanged(); }
        }

        private string _statusInfo;
        public string StatusInfo
        {
            get => _statusInfo;
            private set { _statusInfo = value; OnPropertyChanged(); }
        }

        public ParticipantTestCardViewModel(
            Test test,
            List<Attempt> attempts,
            bool isFavorite,
            ParticipantsAssignedTest? assignment,
            ParticipantsPublicTest? publicTest = null)
        {
            Test = test;
            _attempts = attempts ?? new List<Attempt>();
            _isFavorite = isFavorite;
            _assignment = assignment;
            _publicTest = publicTest;
            _isPublicAccess = publicTest != null;

            InitializeProperties();
        }

        private void InitializeProperties()
        {
            TestTypeName = Test.TestType?.Name ?? "Тест";
            TopicName = Test.Topic?.Name ?? "";
            QuestionsCount = Test.PenaltyMax;
            TimeLimit = Test.TimeLimitSecond;
            MaxAttempts = Test.MaxNumPassing;
            TestName = Test.Name ?? "";
            TestDescription = Test.Description ?? "";
            CreatorName = Test.CuratorCreate?.Name ?? "";
            DateOfCreating = Test.DateOfCreating;
            AttemptsUsed = _attempts?.Count ?? 0;
            AvailableAttempts = MaxAttempts - AttemptsUsed;
            IsCompleted = _attempts?.Any(a => a.Status == "Завершен") ?? false;
            IsAssigned = _assignment != null;
            IsPublicAccess = _publicTest != null;

            UpdateDynamicProperties();
        }

        private void UpdateDynamicProperties()
        {
            IsAvailableByDate = (_assignment == null || !_assignment.DateTimeAssigned.HasValue)
                ? true
                : _assignment.DateTimeAssigned.Value.Date <= DateTime.Now.Date;

            CanStart = MaxAttempts > AttemptsUsed && IsAvailableByDate;
            AvailableAttempts = MaxAttempts - AttemptsUsed;

            if (_assignment == null)
                AssignmentInfo = "";
            else
            {
                string curatorName = _assignment.Curator?.Name ?? "куратором";
                string date = _assignment.DateTimeAssigned?.ToString("dd.MM.yyyy") ?? "";
                string isToday = _assignment.DateTimeAssigned?.Date == DateTime.Now.Date ? " (СЕГОДНЯ)" : "";
                AssignmentInfo = $"Назначен Вам от {curatorName} на {date}{isToday}";
            }

            if (IsCompleted)
            {
                var lastAttempt = _attempts?.OrderByDescending(a => a.FinishedAt).FirstOrDefault();
                if (lastAttempt != null && lastAttempt.Score.HasValue && Test.PenaltyMax > 0)
                {
                    double percent = (lastAttempt.Score.Value * 100.0) / Test.PenaltyMax;
                    StatusInfo = $"Последний: {lastAttempt.Score.Value}/{Test.PenaltyMax} ({percent:F0}%)";
                }
                else
                    StatusInfo = "Ответы были предоставлены";
            }
            else if (AvailableAttempts <= 0)
                StatusInfo = "Все попытки использованы";
            else if (AttemptsUsed > 0)
                StatusInfo = $"Осталось попыток: {AvailableAttempts}";
            else
                StatusInfo = "Нет зафиксированных ответов";
        }
    }
}