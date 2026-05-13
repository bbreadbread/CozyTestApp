using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using Safety_Wheel.Services;

namespace Safety_Wheel.ValidationRules
{
    public class TeacherLoginValidationRule : ValidationRule
    {
        public TeacherService TeacherService { get; set; }
        public string OriginalLogin { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var login = (value ?? "").ToString().Trim();

            if (string.IsNullOrWhiteSpace(login))
                return new ValidationResult(false, "Поле не может быть пустым.");

            if (!string.IsNullOrEmpty(OriginalLogin) && string.Equals(login, OriginalLogin, StringComparison.OrdinalIgnoreCase))
                return ValidationResult.ValidResult;

            if(TeacherService.GetTeacherById(CurrentUser.Id).Login != login)
            if (TeacherService.UserExistsByLogin(login))
                return new ValidationResult(false, "Пользователь с таким логином уже существует.");

            return ValidationResult.ValidResult;
        }
    }

    public class TeacherRequiredValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = (value ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Поле не может быть пустым.");
            return ValidationResult.ValidResult;
        }
    }
}