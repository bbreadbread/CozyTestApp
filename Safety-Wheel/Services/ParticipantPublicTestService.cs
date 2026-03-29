using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using OpenTK.Audio.OpenAL;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    class ParticipantPublicTestService
    {

        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<ParticipantsPublicTest> ParticipantsPublicTests { get; set; } = new();

        public void GetAll(int testId)
        {
            var lines = _db.ParticipantsPublicTests
                .ToList();
            ParticipantsPublicTests.Clear();

            foreach (var line in lines)
            {
                ParticipantsPublicTests.Add(line);
            }
        }
        public int Commit() => _db.SaveChanges();

        public void Remove(ParticipantsPublicTest participantsPublicTest)
        {
            _db.Remove(participantsPublicTest);
            if (Commit() > 0)
                if (ParticipantsPublicTests.Contains(participantsPublicTest))
                    ParticipantsPublicTests.Remove(participantsPublicTest);
        }

        public void Add(ParticipantsPublicTest participantsPublicTest)
        {
            var _participantsPublicTest = new ParticipantsPublicTest
            {
                TestId = participantsPublicTest.TestId,
                ParticipantId = participantsPublicTest.ParticipantId,
                ResponsibleId = participantsPublicTest.ResponsibleId,
            };
            _db.Add(_participantsPublicTest);
            ParticipantsPublicTests.Add(_participantsPublicTest);
            Commit();
        }

        public bool IsBindingUserTest(int userId)
        {
            var list = _db.ParticipantsPublicTests
                .Include(x => x.Participant)
                .FirstOrDefault(p => p.ParticipantId == userId);

            if (list == null) return false;
            return true;
        }

        public void SwitchParticipantPublicStatus(int testId, ObservableCollection<Participant> participants)
        {
            try
            {
                var existingParticipantIds = ParticipantsPublicTests
                    .Where(ppt => ppt.TestId == testId)
                    .Select(ppt => ppt.ParticipantId)
                    .ToHashSet();

                foreach (var participant in participants)
                {
                    if (!existingParticipantIds.Contains(participant.Id))
                    {
                        Add(new ParticipantsPublicTest
                        {
                            TestId = testId,
                            ParticipantId = participant.Id,
                            ResponsibleId = CurrentUser.Id
                        });
                    }
                }

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public void SwitchParticipantPublicStatus(int userId, int testId)
        {
            try
            {
                var user = ParticipantsPublicTests
                    .FirstOrDefault(ug => ug.ParticipantId == userId && ug.TestId == testId);

                if (user != null)
                {
                    Remove(user);
                    _db.SaveChanges();
                }
                else
                {
                    Add(new ParticipantsPublicTest
                    {
                        TestId = testId,
                        ParticipantId = userId,
                        ResponsibleId = CurrentUser.Id
                    });
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public void SwitchParticipantPublicStatus(int testId, int groupId, List<Participant> participants)
        {
            try
            {
                var curatorGroups = _db.Groups
                    .Where(g => g.CuratorId == CurrentUser.Id)
                    .Include(g => g.Participants)
                    .ToList();

                bool isCurrentlyPublishedForAll = IsPublishedForAll(testId, participants);

                if (isCurrentlyPublishedForAll)
                {
                    foreach (var participant in participants)
                    {
                        bool isInOtherPublishedGroup = curatorGroups
                            .Where(g => g.Id != groupId)
                            .Where(g => g.Participants.Any(p => p.Id == participant.Id))
                            .Any(g => IsTestPublishedForGroup(g.Id, testId));

                        if (!isInOtherPublishedGroup)
                        {
                            var existing = _db.ParticipantsPublicTests
                                .FirstOrDefault(ppt => ppt.ParticipantId == participant.Id && ppt.TestId == testId);

                            if (existing != null)
                            {
                                _db.Remove(existing);

                                if (ParticipantsPublicTests.Contains(existing))
                                    ParticipantsPublicTests.Remove(existing);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var participant in participants)
                    {
                        var existing = _db.ParticipantsPublicTests
                            .FirstOrDefault(ppt => ppt.ParticipantId == participant.Id && ppt.TestId == testId);

                        if (existing == null)
                        {
                            var newPublication = new ParticipantsPublicTest
                            {
                                TestId = testId,
                                ParticipantId = participant.Id,
                                ResponsibleId = CurrentUser.Id
                            };

                            _db.Add(newPublication);
                            ParticipantsPublicTests.Add(newPublication);
                        }
                    }
                }

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private bool IsTestPublishedForGroup(int groupId, int testId)
        {
            var participants = GetParticipantsForGroup(groupId);
            return IsPublishedForAll(testId, participants);
        }

        private List<Participant> GetParticipantsForGroup(int groupId)
        {
            return _db.Participants
                .Include(p => p.Groups)
                .Where(u => u.Groups.Any(p => p.Id == groupId))
                .ToList();
        }

        private bool IsPublishedForAll(int testId, List<Participant> participants)
        {
            if (!participants.Any()) return false;

            foreach (var participant in participants)
            {
                bool isPublished = _db.ParticipantsPublicTests
                    .Any(ppt => ppt.ParticipantId == participant.Id && ppt.TestId == testId);

                if (!isPublished) return false;
            }

            return true;
        }

        public bool IsPublished(int testId, int participantId)
        {
            return ParticipantsPublicTests.Any(pt => pt.TestId == testId && pt.ParticipantId == participantId);
        }
    }
}
