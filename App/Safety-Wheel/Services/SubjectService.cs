using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Internal;

namespace CozyTest.Services
{
    public class TopicService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Topic> Topics { get; } = new();

        public TopicService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Topic subject)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Topic
            {
                Name = subject.Name
            };
            await db.Topics.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public async Task GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            var subjects = await db.Topics
                .Include(s => s.Tests)
                .ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Topics.Clear();
                foreach (var subject in subjects)
                    Topics.Add(subject);
            });
        }

        public async Task UpdateAsync(Topic group)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Topics.FindAsync(group.Id);
            if (existing != null)
            {
                existing.Name = group.Name;
                await db.SaveChangesAsync();
            }
        }

        public async Task AddTopicAsync(Topic topic)
        {
            using var context = _factory.CreateDbContext();

            context.Topics.Add(topic);
            await context.SaveChangesAsync();

            Topics.Add(topic);
        }

        public async Task UpdateTopicAsync(Topic topic)
        {
            using var context = _factory.CreateDbContext();

            var existingTopic = await context.Topics.FindAsync(topic.Id);
            if (existingTopic != null)
            {
                existingTopic.Name = topic.Name;
                context.Entry(existingTopic).State = EntityState.Modified;
                await context.SaveChangesAsync();

                var index = Topics.IndexOf(Topics.FirstOrDefault(t => t.Id == topic.Id));
                if (index >= 0)
                {
                    Topics[index] = existingTopic;
                }
            }
        }

        public async Task DeleteTopicAsync(int id)
        {
            using var context = _factory.CreateDbContext();
            var topic = await context.Topics.FindAsync(id);
            if (topic != null)
            {
                context.Topics.Remove(topic);
                await context.SaveChangesAsync();

                var toRemove = Topics.FirstOrDefault(t => t.Id == id);
                if (toRemove != null)
                {
                    Topics.Remove(toRemove);
                }
            }
        }

        public bool HasTestsWithTopic(int topicId)
        {
            using var context = _factory.CreateDbContext();
            return context.Tests.Any(t => t.TopicId == topicId);
        }
    }
}