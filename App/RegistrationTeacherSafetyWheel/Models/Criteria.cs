namespace RegistrationCuratorCozyTest.Models
{
    public class Criteria : ObservableObject
    {
        private int _id;
        private int _testId;
        private string _name = "";
        private int _minPercent;
        private bool _isActive;
        private int _orderNumber;
        private Test _test;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int TestId
        {
            get => _testId;
            set => SetProperty(ref _testId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int MinPercent
        {
            get => _minPercent;
            set => SetProperty(ref _minPercent, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public int OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public Test Test
        {
            get => _test;
            set => SetProperty(ref _test, value);
        }
    }
}