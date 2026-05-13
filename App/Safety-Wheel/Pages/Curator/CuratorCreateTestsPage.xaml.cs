using Microsoft.Win32;
using CozyTest.Models;
using CozyTest.ViewModels.CreateTestsVM;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace CozyTest.Pages.Curator
{
    public partial class CuratorCreateTestsPage : UserControl
    {
        Test? _test = null;
        public CuratorCreateTestsPage()
        {
            InitializeComponent();
        }
        public CuratorCreateTestsPage(Test? test)
        {
            _test = test;
            InitializeComponent();
        }

    }
}