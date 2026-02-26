using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.ViewModels
{
    public class MenuItemViewModel : ObservableObject, IHamburgerMenuItemBase
    {
        private object _icon;
        private object _label;
        private object _toolTip;
        private object _tag;
        private bool _isVisible = true;

        public MenuItemViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public MainViewModel MainViewModel { get; }

        public object Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public object Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public object ToolTip
        {
            get => _toolTip;
            set => SetProperty(ref _toolTip, value);
        }

        public object Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }
}
