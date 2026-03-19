using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using CozyTest.Services;

namespace CozyTest.ValidationRules
{
    public class LoginUniqueValidationRule : ValidationRule
    {
        public CuratorService CuratorService { get; set; } = new() ;
        public string OriginalLogin { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var login = (value ?? "").ToString().Trim();

            if (string.IsNullOrWhiteSpace(login))
                return new ValidationResult(false, "Поле не может быть пустым.");

            if (string.Equals(login, OriginalLogin, StringComparison.OrdinalIgnoreCase))
                return ValidationResult.ValidResult;
            if (CuratorService.UserExistsByLogin(login)) return new ValidationResult(false, "Пользователь с таким логином уже существует.");

            return ValidationResult.ValidResult;
        }
    }

    public class RequiredFieldValidationRule : ValidationRule
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