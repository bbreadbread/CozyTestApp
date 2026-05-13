using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Services
{
    public class OptionService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Option> Options { get; set; } = new();

        public OptionService()
        {
            GetAll();
        }

        public void Add(Option option, int j)
        {
            var _option = new Option
            {
                QuestionId = option.QuestionId,
                Number = j,
                TextAnswer = option.TextAnswer,
                IsCorrect = option.IsCorrect,
                Question = option.Question
            };
            _db.Add(_option);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll(int? questionId = null)
        {
            if (questionId != null)
            {
                var options = _db.Options
                    .Include(o => o.Question)
                    .Include(o => o.StudentAnswers)
                    .Where(o => o.QuestionId == questionId)
                    .ToList();
                Options.Clear();

                foreach (var option in options)
                {
                    Options.Add(option);
                }
            }
            else
            {
                var options = _db.Options
                    .Include(o => o.Question)
                    .Include(o => o.StudentAnswers)
                    .ToList();
                Options.Clear();

                foreach (var option in options)
                {
                    Options.Add(option);
                }
            }
        }

        public void Remove(Option option)
        {
            if (option == null)
                return;

            var answers = _db.StudentAnswers
                .Where(sa => sa.OptionId == option.Id)
                .ToList();

            _db.StudentAnswers.RemoveRange(answers);

            _db.Options.Remove(option);

            if (Commit() > 0)
                if (Options.Contains(option))
                    Options.Remove(option);
        }


        public void Update(Option option)
        {
            var existing = _db.Options.Find(option.Id);
            if (existing != null)
            {
                existing.QuestionId = option.QuestionId;
                existing.TextAnswer = option.TextAnswer;
                existing.IsCorrect = option.IsCorrect;
                Commit();
            }
        }

        public List<Option> GetOptionsByQuestion(int questionId)
        {
            return Options
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.Number)
                .ToList();
        }

    }
}
