using CozyTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.Services
{
    public enum LogActionType
    {
        Authorization, //0
        Create, //1
        Edit, //2
        Admin, //3
        Archive, //4
        Delete, //5
        Public, //6
        Assigned //7
    }

    public enum LogObjectType
    {
        Curator,
        Participant,
        Group,
        Test,
        Question,
        Application
    }

    public static class LogMessageTemplates
    {
        private static readonly Dictionary<(string Role, LogActionType Action, LogObjectType Object), string> Templates = new()
        {
            [("CozyTest.Models.Curator", LogActionType.Authorization, LogObjectType.Curator)] = "Куратор {0} зашел в аккаунт",
            [("CozyTest.Models.Participant", LogActionType.Authorization, LogObjectType.Application)] = "Тестируемый {0} отправил заявку на регистрацию",
            [("CozyTest.Models.Participant", LogActionType.Authorization, LogObjectType.Participant)] = "Тестируемый {0} зашел в аккаунт",

            [("CozyTest.Models.Curator", LogActionType.Create, LogObjectType.Application)] = "Куратор {0} обработал заявку на создание аккаунта от {1}. Заявка: {2}",
            [("CozyTest.Models.Curator", LogActionType.Create, LogObjectType.Curator)] = "Куратор {0} создал куратора {1}",
            [("CozyTest.Models.Curator", LogActionType.Create, LogObjectType.Participant)] = "Куратор {0} создал тестируемого {1}",
            [("CozyTest.Models.Curator", LogActionType.Create, LogObjectType.Group)] = "Куратор {0} создал группу {1}",
            [("CozyTest.Models.Curator", LogActionType.Create, LogObjectType.Test)] = "Куратор {0} создал тест {1}",
            
            [("CozyTest.Models.Curator", LogActionType.Admin, LogObjectType.Curator)] = "Куратор {0} поменял админство куратора {1}. Новый статус: {2}",

            [("CozyTest.Models.Curator", LogActionType.Edit, LogObjectType.Curator)] = "Куратор {0} редактировал куратора {1}",
            [("CozyTest.Models.Curator", LogActionType.Edit, LogObjectType.Participant)] = "Куратор {0} редактировал тестируемого {1}",
            [("CozyTest.Models.Curator", LogActionType.Edit, LogObjectType.Group)] = "Куратор {0} редактировал группу {1}",
            [("CozyTest.Models.Curator", LogActionType.Edit, LogObjectType.Test)] = "Куратор {0} редактировал тест {1}",
            [("CozyTest.Models.Participant", LogActionType.Edit, LogObjectType.Test)] = "Тестируемый {0} начал/закончил прохождение теста {1}",

            [("CozyTest.Models.Curator", LogActionType.Archive, LogObjectType.Curator)] = "Куратор {0} заархивировал куратора {1}",
            [("CozyTest.Models.Curator", LogActionType.Archive, LogObjectType.Participant)] = "Куратор {0} заархивировал тестируемого {1}",
            [("CozyTest.Models.Curator", LogActionType.Archive, LogObjectType.Test)] = "Куратор {0} {2} тест {1}",
            [("CozyTest.Models.Curator", LogActionType.Archive, LogObjectType.Question)] = "Куратор {0} заархивировал вопрос {1}",
      
            [("CozyTest.Models.Curator", LogActionType.Public, LogObjectType.Test)] = "Куратор {0} опубликовал тест {1}. Кому: {2}",

            [("CozyTest.Models.Curator", LogActionType.Assigned, LogObjectType.Test)] = "Куратор {0} назначит тест {1}. Кому: {2}",

            [("CozyTest.Models.Curator", LogActionType.Delete, LogObjectType.Group)] = "Куратор {0} удалил группу {1}",
        };

        public static string GetTemplate(string role, LogActionType action, LogObjectType objType)
        {
            if (Templates.TryGetValue((role, action, objType), out var template))
                return template;

            if (role.Contains("CozyTest.Models.Curator"))
            {
                if (Templates.TryGetValue(("CozyTest.Models.Curator", action, objType), out var curatorTemplate))
                    return curatorTemplate;
            }

            return null;
        }
    }

    public static class LogRoleNames
    {
        public static string GetRoleName(string role)
        {
            if (role.Contains("Curator"))
                return "Куратор";
            if (role.Contains("Participant"))
                return "Тестируемый";
            return "Пользователь";
        }
    }

    public class LogEntryBuilder
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string WhoMade { get; set; } = string.Empty;
        public string WhoRole { get; set; } = string.Empty;
        public LogActionType ActionType { get; set; }
        public LogObjectType ObjectType { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public string? Details { get; set; }

        public UserActionLog Build()
        {
            var template = LogMessageTemplates.GetTemplate(WhoRole, ActionType, ObjectType);
            string message;
            string typeWhoMade = WhoRole.Contains("Curator") ? "Curator" : (WhoRole.Contains("Participant") ? "Participant" : "User");
            string typeObject = ObjectType.ToString();

            if (template != null)
            {
                int formatArgCount = CountFormatArguments(template);
                switch (formatArgCount)
                {
                    case 3 when Details != null:
                        message = string.Format(template, WhoMade, ObjectName, Details);
                        break;
                    case 2:
                        message = string.Format(template, WhoMade, ObjectName);
                        break;
                    case 1:
                        message = string.Format(template, WhoMade);
                        break;
                    default:
                        message = template;
                        break;
                }
            }
            else
            {
                string roleName = LogRoleNames.GetRoleName(WhoRole);
                if (!string.IsNullOrEmpty(Details))
                    message = $"{roleName} {WhoMade} выполнил действие над объектом {ObjectName}: {Details}";
                else
                    message = $"{roleName} {WhoMade} выполнил действие над объектом {ObjectName}";
            }

            return new UserActionLog
            {
                TimeStamp = TimeStamp,
                TypeWhoMade = typeWhoMade,
                WhoMade = WhoMade,
                LevelLog = (int)ActionType,
                TypeObject = typeObject,
                Object = ObjectName,
                Message = message
            };
        }

        private int CountFormatArguments(string template)
        {
            int maxIndex = -1;
            for (int i = 0; i < template.Length; i++)
            {
                if (template[i] == '{' && i + 1 < template.Length && char.IsDigit(template[i + 1]))
                {
                    int j = i + 1;
                    while (j < template.Length && char.IsDigit(template[j]))
                        j++;

                    if (j > i + 1 && j < template.Length && template[j] == '}')
                    {
                        int index = int.Parse(template.Substring(i + 1, j - i - 1));
                        if (index > maxIndex)
                            maxIndex = index;
                    }
                }
            }
            return maxIndex + 1;
        }
    }

    public interface ILoggingService
    {
        Task LogAsync(LogEntryBuilder entry);
        Task LogAsync(string whoMade, string whoRole, LogActionType action, LogObjectType objectType, string objectName, string? details = null);
    }

    public class LoggingService : ILoggingService
    {
        private readonly CozyTestContext _db;

        public LoggingService(CozyTestContext db)
        {
            _db = db;
        }

        public async Task LogAsync(LogEntryBuilder entry)
        {
            var log = entry.Build();
            await _db.UserActionLogs.AddAsync(log);
            await _db.SaveChangesAsync();
        }

        public async Task LogAsync(string whoMade, string whoRole, LogActionType action,
            LogObjectType objectType, string objectName, string? details = null)
        {
            var entry = new LogEntryBuilder
            {
                WhoMade = whoMade,
                WhoRole = whoRole,
                ActionType = action,
                ObjectType = objectType,
                ObjectName = objectName,
                Details = details
            };
            await LogAsync(entry);
        }
    }
}