using Safety_Wheel.Models;
using Safety_Wheel.Services;

namespace UnitTestsForSafetyWheel
{
    public class SafetyWheelTest
    {
        private SafetyWheelContext CreateContext()
        {
            var context = TestDbContext.CreateDbContext();
            BaseDbService.Instance.SetContext(context);
            return context;
        }

        private void BaseData(SafetyWheelContext context)
        {
            var subject = new Subject
            {
                Id = 1,
                Name = "Математика"
            };

            var teacher = new Teacher
            {
                Id = 1,
                Login = "teacher1",
                Password = "1234",
                Name = "Иванов Иван"
            };

            var testType = new TestType
            {
                Id = 1,
                Name = "Обычный тест",
                Description = "Тест без ограничений",
                TimeLimitSecond = 600
            };

            var test = new Test
            {
                Id = 1,
                Name = "Математика Вариант 11",
                SubjectId = subject.Id,
                TeacherId = teacher.Id,
                PenaltyMax = 0,
                MaxScore = 100,
                DateOfCreating = DateTime.Now,
                IsPublic = true
            };

            var questionType = new QuestionType
            {
                Id = 1,
                Name = "Правильный ответ"
            };

            var question = new Question
            {
                Id = 1,
                TestId = test.Id,
                Number = 1,
                TestQuest = "2 + 2 = ?",
                PicturePath = null,
                Comments = "Вопрос на смекалку",
                QuestionType = questionType.Id
            };

            var option = new Option
            {
                Id = 1,
                QuestionId = question.Id,
                Number = 1,
                TextAnswer = "4",
                IsCorrect = true
            };

            var student = new Student
            {
                Id = 1,
                Name = "Петров Петр",
                Login = "student1",
                Password = "1234",
                TeachersId = teacher.Id
            };

            var attempt = new Attempt
            {
                Id = 1,
                StudentsId = student.Id,
                TestId = test.Id,
                StartedAt = DateTime.Now,
                FinishedAt = DateTime.Now.AddMinutes(5),
                Score = 100,
                Status = "Завершен (время истекло)",
                TestType = testType.Id
            };

            var studentAnswer = new StudentAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = question.Id,
                OptionId = option.Id,
                IsCorrect = true,
                AnsweredAt = DateTime.Now
            };

            context.Subjects.Add(subject);
            context.Teachers.Add(teacher);
            context.TestTypes.Add(testType);
            context.Tests.Add(test);
            context.QuestionTypes.Add(questionType);
            context.Questions.Add(question);
            context.Options.Add(option);
            context.Students.Add(student);
            context.Attempts.Add(attempt);
            context.StudentAnswers.Add(studentAnswer);

            context.SaveChanges();
        }


        //1
        [Fact]
        public void Add_StudentAnswerService_AddsAnswerToDatabase()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();

            var answer = new StudentAnswer
            {
                AttemptId = 1,
                QuestionId = 2,
                OptionId = 2,
                IsCorrect = true,
                AnsweredAt= DateTime.Now
            };

            // Act
            service.Add(answer);

            // Assert
            Assert.Equal(2, context.StudentAnswers.Count());
        }


        //2
        [Fact]
        public void GetAll_ReturnsAllAnswers()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();
            // Act
            service.GetAll();

            // Assert
            Assert.Single(service.StudentAnswers);
        }

        //3
        [Fact]
        public void GetAll_CurrentAttempt()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();

            // Act
            service.GetAll(attemptId: 1);

            // Assert
            Assert.All(service.StudentAnswers, a => Assert.Equal(1, a.AttemptId));
        }

        //4
        [Fact]
        public void IsReady_WhenAllQuestionsAnswered_ReturnsTrue()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();

            var attempt = context.Attempts.First();
            var test = context.Tests.First();

            // Act
            var result = service.IsReady(attempt, test);

            // Assert
            Assert.True(result);
        }

        //5
        [Fact]
        public void IsReady_WhenNoAnswers_ReturnsFalse()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();
            context.StudentAnswers.RemoveRange(context.StudentAnswers);
            context.SaveChanges();

            var attempt = context.Attempts.First();
            var test = context.Tests.First();

            // Act
            var result = service.IsReady(attempt, test);

            // Assert
            Assert.False(result);
        }
        //6
        [Fact]
        public void GetQuestionCorrectness_WhenAnswerCorrect_ReturnsTrue()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();

            var attempt = context.Attempts.First();

            // Act
            var result = service.GetQuestionCorrectness(attempt, 1);

            // Assert
            Assert.True(result);
        }

        //7
        [Fact]
        public void GetQuestionCorrectness_WhenNoAnswer_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();
            context.StudentAnswers.RemoveRange(context.StudentAnswers);
            context.SaveChanges();

            var attempt = context.Attempts.First();

            // Act
            var result = service.GetQuestionCorrectness(attempt, 1);

            // Assert
            Assert.Null(result);
        }

        //8
        [Fact]
        public void Update_StudentAnswer()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentAnswerService();

            var answer = context.StudentAnswers.First();
            var answerForEqual = answer;
            answer.IsCorrect = false;

            // Act
            service.Update(answer);

            // Assert
            Assert.Equal(answer, answerForEqual);
        }

        //9
        [Fact]
        public void GetAll_WhenCalled_ReturnsTests()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TestService();

            // Act
            service.GetAll();

            // Assert
            Assert.NotEmpty(service.Tests);
        }

        //10
        [Fact]
        public void GetTestsBySubjectId_WhenSubjectExists_ReturnsFilteredTests()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TestService();

            // Act
            service.GetTestsBySubjectId(1);

            // Assert
            Assert.All(service.Tests, t => Assert.Equal(1, t.SubjectId));
        }

        //11
        [Fact]
        public void GetLastTest_WhenCalled_ReturnsLatestTest()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TestService();
            // Act
            var test = service.GetLastTest();

            // Assert
            Assert.NotNull(test);
        }

        //12
        [Fact]
        public void GetTestById_WhenIdExists_ReturnsTest()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TestService();
            // Act
            var test = service.GetTestById(1);

            // Assert
            Assert.Equal(1, test.Id);
        }

        //13
        [Fact]
        public void Remove_WhenTestExists_RemovesTestAndRelatedData()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TestService();
            var test = context.Tests.First();

            // Act
            service.Remove(test);

            // Assert
            Assert.Empty(context.Tests);
            Assert.Empty(context.Questions);
        }

        //14
        [Fact]
        public void GetQuestiosForCurrentTest_WhenCalled_ReturnsOrderedQuestions()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new QuestionService();

            // Act
            var questions = service.GetQuestiosForCurrentTest(1);

            // Assert
            Assert.Single(questions);
        }

        //15
        [Fact]
        public void DeleteByTest_WhenTestHasQuestions_RemovesQuestions()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new QuestionService();

            // Act
            service.DeleteQuestion(1);

            // Assert
            Assert.Empty(context.Questions);
        }

        //16
        [Fact]
        public void Update_WhenCalled_UpdatesQuestion()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new QuestionService();

            var question = context.Questions.First();
            question.TestQuest = "Updated";

            // Act
            service.Update(question);

            // Assert
            Assert.Equal("Updated", context.Questions.First().TestQuest);
        }

        //17
        [Fact]
        public void GetOptionsByQuestion_WhenCalled_ReturnsOptions()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new OptionService();

            // Act
            var options = service.GetOptionsByQuestion(1);

            // Assert
            Assert.Single(options);
        }

        //18
        [Fact]
        public void Remove_WhenCalled_RemovesOption()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new OptionService();

            var option = context.Options.First();

            // Act
            service.Remove(option);

            // Assert
            Assert.Empty(context.Options);
        }

        //19
        [Fact]
        public void UserExistsByLogin_WhenLoginExists_ReturnsTrue()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new TeacherService();

            // Act
            var result = service.UserExistsByLogin("student1");

            // Assert
            Assert.True(result);
        }

        //20
        [Fact]
        public void GetCurrentStudent_WhenIdExists_ReturnsStudent()
        {
            // Arrange
            var context = CreateContext();
            BaseData(context);
            var service = new StudentService();

            // Act
            var student = service.GetCurrentStudent(1);

            // Assert
            Assert.NotNull(student);
        }

    }
}