using OfficeOpenXml;
using CozyTest.Models;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;

namespace CozyTest.Services
{
    public class TestExportService
    {
        Test _test;
        TestService _testService;
        public TestExportService(TestService testService)
        {
            _testService = testService;
            ExcelPackage.License.SetNonCommercialPersonal("CozyTest");
        }

        public async void ExportTestToExcel(Test test, string savePath)
        {
            if (test == null)
                throw new ArgumentNullException(nameof(test));

            if (string.IsNullOrEmpty(savePath))
                throw new ArgumentNullException(nameof(savePath));
            _test = await _testService.GetTestById(test.Id);
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Пример");

            
            int currentRow = 1;
            worksheet.Cells[currentRow, 1].Value = "Название теста (только в первой строке)";
            worksheet.Cells[currentRow, 2].Value = "Номер вопроса (в каждой строке)*";
            worksheet.Cells[currentRow, 3].Value = "Тип вопроса (можно только в первой строке каждого вопроса)*";
            worksheet.Cells[currentRow, 4].Value = "Текст вопроса (можно только в первой строке каждого вопроса)";
            worksheet.Cells[currentRow, 5].Value = "Тип 1 - основное фото (заполняется по желанию), тип 2 - пусто, тип 3 - постоянная часть";
            worksheet.Cells[currentRow, 6].Value = "Типы 1 ,2 - текст ответа. Тип 3 - варианты ответов";
            worksheet.Cells[currentRow, 7].Value = "Ответ - путь к изображению? Если да - \"1\"";
            worksheet.Cells[currentRow, 8].Value = "Правильный ответ? (Да - \"1\" в клетку, нет - клетка пустая), если тип 3 - порядковый номер варианта соответсвия для постоянной части";
            worksheet.Cells[currentRow, 9].Value = "ДЛЯ ТИПОВ ВОПРОСОВ:                                               \"1\" - Варианты ответов (текстовые или фото) + по желанию основное фото  + по желанию основное фото.                                                            \"2\" - Текстовый ответ тестируемого (столбец F заполняется - 1 строка, всегда правильный ответ).                                                                                      \"3\" - Сопоставление постоянной части (E) ответов (F). Правильная комбинация указывается в столбце H.                                                                                ПРАВИЛА ВСТАВКИ ИЗОБРАЖЕНИЙ:                                                вставляете путь от папки Images/user_tests_images проекта, не упоминая саму папку. пример: Images/user_tests_images/tests_img/А.png. ВАШИ ИЗОБРАЖЕНИЯ ДОЛЖНЫ НАХОДИТСЯ ПО ЭТОМУ ПУТИ   Если изображение не отображается - проблема в пути.";

            currentRow++;

            worksheet.Cells[currentRow, 1].Value = _test.Name;

            var sortedQuestions = _test.Questions.OrderBy(q => q.NumberActual).ToList();

            foreach (var question in sortedQuestions)
            {
                var options = question.Options.Where(o => o.Version == question.Options.Max(p => p.Version)).OrderBy(o => o.Number).ToList();
                var constantOptions = options.Where(o => o.CorrespondingNumber != null).OrderBy(o => o.CorrespondingNumber).ToList();
                var regularOptions = options.Where(o => o.CorrespondingNumber == null).OrderBy(o => o.Number).ToList();

                if (question.QuestionTypeId == 1)
                {
                    worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                    worksheet.Cells[currentRow, 3].Value = 1;
                    worksheet.Cells[currentRow, 4].Value = question.TestQuest;

                    if (!string.IsNullOrEmpty(question.PicturePath))
                        worksheet.Cells[currentRow, 5].Value = CleanImagePath(question.PicturePath);

                    foreach (var option in regularOptions)
                    {
                        worksheet.Cells[currentRow, 6].Value = option.TextAnswer;

                        if (option.IsImage == true)
                            worksheet.Cells[currentRow, 7].Value = "1";

                        if (option.IsCorrect == true)
                            worksheet.Cells[currentRow, 8].Value = "1";

                        currentRow++;

                        if (option != regularOptions.Last())
                        {
                            worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                        }
                    }
                }
                else if (question.QuestionTypeId == 2)
                {
                    worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                    worksheet.Cells[currentRow, 3].Value = 2;
                    worksheet.Cells[currentRow, 4].Value = question.TestQuest;

                    if (!string.IsNullOrEmpty(question.PicturePath))
                        worksheet.Cells[currentRow, 5].Value = CleanImagePath(question.PicturePath);

                    if (regularOptions.Any())
                    {
                        var textOption = regularOptions.First();
                        worksheet.Cells[currentRow, 6].Value = textOption.TextAnswer;

                        if (textOption.IsImage == true)
                            worksheet.Cells[currentRow, 7].Value = "1";

                        worksheet.Cells[currentRow, 8].Value = "1";
                    }

                    currentRow++;
                }
                else if (question.QuestionTypeId == 3)
                {
                    var constantGroups = constantOptions.GroupBy(o => o.CorrespondingNumber).ToList();

                    foreach (var constantGroup in constantGroups)
                    {
                        var constantNum = constantGroup.Key;
                        var constantOption = constantOptions.FirstOrDefault(o => o.CorrespondingNumber == constantNum);

                        if (constantOption != null)
                        {
                            worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                            worksheet.Cells[currentRow, 3].Value = 3;
                            worksheet.Cells[currentRow, 4].Value = question.TestQuest;

                            if (!string.IsNullOrEmpty(question.PicturePath))
                                worksheet.Cells[currentRow, 5].Value = CleanImagePath(question.PicturePath);

                            worksheet.Cells[currentRow, 5].Value = constantOption.CorrespondingNumber;
                            worksheet.Cells[currentRow, 6].Value = constantOption.TextAnswer;

                            if (constantOption.IsImage == true)
                                worksheet.Cells[currentRow, 7].Value = "1";

                            currentRow++;
                        }

                        foreach (var correspondingOpt in constantGroup)
                        {
                            worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                            worksheet.Cells[currentRow, 6].Value = correspondingOpt.TextAnswer;

                            if (correspondingOpt.IsImage == true)
                                worksheet.Cells[currentRow, 7].Value = "1";

                            if (correspondingOpt.Number != 0)
                                worksheet.Cells[currentRow, 8].Value = correspondingOpt.Number.ToString();

                            currentRow++;
                        }
                    }

                    foreach (var regularOpt in regularOptions)
                    {
                        worksheet.Cells[currentRow, 2].Value = question.NumberActual;
                        worksheet.Cells[currentRow, 6].Value = regularOpt.TextAnswer;

                        if (regularOpt.IsImage == true)
                            worksheet.Cells[currentRow, 7].Value = "1";

                        currentRow++;
                    }
                }
            }

            worksheet.Cells.Style.WrapText = true;

            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 50;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 75;
            worksheet.Column(7).Width = 15;
            worksheet.Column(8).Width = 15;
            worksheet.Column(9).Width = 100;

            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            package.SaveAs(new FileInfo(savePath));
        }

        public bool ShowSaveDialogAndExport(Test test)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Сохранить тест в Excel",
                FileName = GetSafeFileName(test.Name) + ".xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportTestToExcel(test, saveFileDialog.FileName);
                return true;
            }

            return false;
        }

        private string CleanImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string normalizedPath = path.Replace('\\', '/');

            var match = Regex.Match(normalizedPath, @"(?:Images/user_tests_images/)?(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;

            return normalizedPath;
        }

        private string GetSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "exported_test";

            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string safeName = Regex.Replace(name, $"[{Regex.Escape(invalidChars)}]", "");
            safeName = safeName.Trim();

            return string.IsNullOrEmpty(safeName) ? "exported_test" : safeName;
        }
    }
}