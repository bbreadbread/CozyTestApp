using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Safety_Wheel
{
    public partial class ClosedWindow : MetroWindow, INotifyPropertyChanged
    {
        private string _warningText2;
        public string WarningText2
        {
            get => _warningText2;
            set
            {
                _warningText2 = value;
                OnPropertyChanged(nameof(WarningText2));
            }
        }
        private string _warningText1;
        public string WarningText1
        {
            get => _warningText1;
            set
            {
                _warningText1 = value;
                OnPropertyChanged(nameof(WarningText1));
            }
        }
        public ClosedWindow(string txtWrng1, string txtWrng2)
        {
            WarningText1 = txtWrng1;
            WarningText2 = txtWrng2;
            InitializeComponent();

            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
