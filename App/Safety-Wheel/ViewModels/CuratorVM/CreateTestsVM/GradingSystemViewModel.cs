using CozyTest.Services;
using CozyTest.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.CreateTestsVM
{
    public class GradingSystemViewModel : BaseViewModel
    {
        private readonly CriteriaService _criteriaService;
        public Test Test { get; set; }

        public ObservableCollection<GradingItemViewModel> GradingItems { get; set; } = new();

        private bool _isGradingEnabled;
        public bool IsGradingEnabled
        {
            get => _isGradingEnabled;
            set
            {
                if (SetProperty(ref _isGradingEnabled, value))
                {
                    foreach (var item in GradingItems)
                    {
                        item.IsActive = value;
                    }
                }
            }
        }

        public ICommand SaveCommand { get; }

        public GradingSystemViewModel(INavigationService navigationService, IDialogService dialogService, CriteriaService criteriaService, Test test)
            : base(navigationService, dialogService)
        {
            _criteriaService = criteriaService;
            Test = test;
            _criteriaService.GetAllByTestAsync(Test.Id);
            var criteria = _criteriaService.Criteria.ToList();

            if (criteria.Count == 0)
            {
                InitializeDefaultItems();
            }
                
            LoadExistingCriteria();
            SaveCommand = new RelayCommand(_ => Save());
        }

        private void InitializeDefaultItems()
        {
            var defaults = new[]
            {
                ("5 (отлично)", 85, 5),
                ("4 (хорошо)", 70, 4),
                ("3 (удовлетворительно)", 50, 3),
                ("2 (неудовлетворительно)", 0, 2),
                ("1 (неявка/незачёт)", 0, 1)
            };

            foreach (var (name, percent, order) in defaults)
            {
                GradingItems.Add(new GradingItemViewModel
                {
                    OrderNumber = order,
                    DefaultName = name,
                    DefaultPercent = percent,
                    IsLast = order == 1
                });
            }
        }

        private async void LoadExistingCriteria()
        {
            await _criteriaService.GetAllByTestAsync(Test.Id);
            var criteria = _criteriaService.Criteria.OrderBy(p=>p.OrderNumber).ToList();

            if (criteria.Any())
            {
                IsGradingEnabled = criteria.First().IsActive;

                foreach (var criterion in criteria)
                {
                    var item = GradingItems.FirstOrDefault(g => g.OrderNumber == criterion.OrderNumber);
                    if (item != null)
                    {
                        item.Name = criterion.Name;
                        item.MinPercent = criterion.MinPercent;
                        item.IsActive = criterion.IsActive;
                        item.CriterionId = criterion.Id;
                    }
                }
            }
            else
            {
                IsGradingEnabled = false;

                foreach (var item in GradingItems)
                {
                    item.IsActive = false;
                    item.CriterionId = 0;
                }
            }
        }

        private async void Save()
        {
            foreach (var item in GradingItems)
            {
                var isActive = IsGradingEnabled && item.IsActive;

                if (item.CriterionId > 0)
                {
                    var existing = _criteriaService.Criteria.FirstOrDefault(c => c.Id == item.CriterionId);
                    if (existing != null)
                    {
                        existing.Name = item.Name;
                        existing.MinPercent = item.MinPercent;
                        existing.IsActive = isActive;
                        existing.OrderNumber = item.OrderNumber;
                        await _criteriaService.UpdateAsync(existing);
                    }
                }
                else if (isActive)
                {
                    var newCriteria = new Criteria
                    {
                        TestId = Test.Id,
                        Name = item.Name,
                        MinPercent = item.MinPercent,
                        IsActive = true,
                        OrderNumber = item.OrderNumber
                    };
                    await _criteriaService.AddAsync(newCriteria);
                    item.CriterionId = newCriteria.Id;
                }
            }
        }
    }

    public class GradingItemViewModel : ObservableObject
    {
        public int OrderNumber { get; set; }
        public string DefaultName { get; set; }
        public int DefaultPercent { get; set; }
        public int CriterionId { get; set; }

        private string _name;
        public string Name
        {
            get => _name ?? DefaultName;
            set => SetProperty(ref _name, value);
        }

        private int _minPercent;
        public int MinPercent
        {
            get => _minPercent == 0 ? DefaultPercent : _minPercent;
            set => SetProperty(ref _minPercent, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsLast { get; set; }
    }
}