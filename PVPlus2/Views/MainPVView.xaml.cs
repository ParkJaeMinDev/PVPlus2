using System.Windows.Controls;
using PVPlus2.ViewModels;

namespace PVPlus2.Views
{
    public partial class MainPVView : UserControl
    {
        public MainPVView()
        {
            InitializeComponent();
            DataContext = new MainPVViewModel();
        }
    }
}
