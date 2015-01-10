using System.Windows.Controls;
using System.Windows.Input;
using Dietphone.ViewModels;

namespace Dietphone.Views
{
    public partial class Pattern : UserControl
    {
        public Pattern()
        {
            InitializeComponent();
        }

        private void Meal_Tap(object sender, GestureEventArgs e)
        {
            ViewModel.GoToMeal();
        }

        private void Insulin_Tap(object sender, GestureEventArgs e)
        {
            ViewModel.GoToInsulin();
        }

        private PatternViewModel ViewModel
        {
            get
            {
                return (PatternViewModel)DataContext;
            }
        }
    }
}
