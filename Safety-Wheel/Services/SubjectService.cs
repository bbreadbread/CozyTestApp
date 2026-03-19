using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.Services
{
    public class TopicService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Topic> Topics { get; set; } = new();

        public TopicService()
        {
            GetAll();
        }

        public void Add(Topic subject)
        {
            var _subject = new Topic
            {
                Name = subject.Name
            };
            _db.Add(_subject);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var subjects = _db.Topics
                .Include(s => s.Tests)
                .ToList();
            Topics.Clear();

            foreach (var subject in subjects)
            {
                Topics.Add(subject);
            }
        }
    }
}
