using Dietphone.Models;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class SugarViewModel : ViewModelBase
    {
        public Sugar Sugar { get; private set; }
        private readonly Factories factories;

        public SugarViewModel(Sugar sugar, Factories factories)
        {
            Sugar = sugar;
            this.factories = factories;
        }

        public string BloodSugar
        {
            get
            {
                var result = Sugar.BloodSugar;
                return result.ToStringOrEmpty();
            }
            set
            {
                var oldValue = Sugar.BloodSugar;
                var newValue = oldValue.TryGetValueOf(value);
                var settings = factories.Settings;
                var constrains = new Constrains { Max = settings.SugarUnit == SugarUnit.mgdL ? 540 : 30 };
                Sugar.BloodSugar = constrains.Constraint(newValue);
                OnPropertyChanged("BloodSugar");
            }
        }
    }
}
