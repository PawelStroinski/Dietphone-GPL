using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dietphone.Views
{
    public partial class Scores : UserControl
    {
        public event EventHandler ScoreClick;

        public Scores()
        {
            InitializeComponent();
        }

        private void Score_Click(object sender, MouseButtonEventArgs e)
        {
            if (ScoreClick != null)
                ScoreClick(sender, e);
        }
    }
}
