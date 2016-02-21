using Dietphone.ViewModels;
using System.Windows.Navigation;

namespace Dietphone.Views
{
    public partial class Settings : PageBase
    {
        private new SettingsViewModel ViewModel { get { return (SettingsViewModel)base.ViewModel; } }

        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnInitializePage()
        {
            ViewModel.Untombstone();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                ViewModel.Tombstone();
            }
        }
    }
}