using PVPlus2.ViewModels;
using System.Windows.Controls;

namespace PVPlus2.Views
{
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
            DataContext = new TestViewModel();
        }
    }
}
