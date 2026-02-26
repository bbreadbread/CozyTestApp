using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections.ObjectModel;

namespace Safety_Wheel.ViewModels.AttemptsVM
{
    public class AttemptDetailsViewModel : ObservableObject
    {
        private Attempt _attempt;
        private Student _student;
        private ObservableCollection<StudentAnswer> _answers;
        private StudentAnswerService _studentAnswerService = new();

        public Attempt Attempt
        {
            get => _attempt;
            set
            {
                if (SetProperty(ref _attempt, value))
                {
                    LoadAnswers();
                }
            }
        }

        public Student Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        public ObservableCollection<StudentAnswer> Answers
        {
            get => _answers;
            set => SetProperty(ref _answers, value);
        }

        public string Title => $"Попытка от {Attempt?.StartedAt:dd.MM.yyyy HH:mm}";

        public AttemptDetailsViewModel()
        {
            Answers = new ObservableCollection<StudentAnswer>();
        }

        public AttemptDetailsViewModel(Attempt attempt, Student student)
        {
            Answers = new ObservableCollection<StudentAnswer>();
            Attempt = attempt;
            Student = student;
        }

        private void LoadAnswers()
        {
            if (Attempt == null) return;

            Answers.Clear();
            _studentAnswerService.GetAll(attemptId: Attempt.Id);

            foreach (var answer in _studentAnswerService.StudentAnswers)
            {
                Answers.Add(answer);
            }
        }
    }
}