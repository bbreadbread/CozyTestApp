using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Safety_Wheel.ValidationRules
{
    public class NameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = (value ?? "").ToString().Trim();

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Поле не может быть пустым.");

            if (input.Any(char.IsDigit))
                return new ValidationResult(false, "Имя не должно содержать цифры.");

            return ValidationResult.ValidResult;
        }
    }
}
