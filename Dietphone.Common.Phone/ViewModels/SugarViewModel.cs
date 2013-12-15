using System;
using Dietphone.Models;
using Dietphone.Tools;
using Dietphone.Views;

namespace Dietphone.ViewModels
{
    public class SugarViewModel : TypedViewModel
    {
        public Sugar Sugar { get; private set; }
        private readonly Factories factories;

        public SugarViewModel(Sugar sugar, Factories factories)
        {
            Sugar = sugar;
            this.factories = factories;
        }

        public override DateTime DateTime
        {
            get
            {
                var universal = Sugar.DateTime;
                return universal.ToLocalTime();
            }
            set
            {
                var universal = value.ToUniversalTime();
                if (Sugar.DateTime != universal)
                {
                    Sugar.DateTime = universal;
                    NotifyDateTimeChange();
                }
            }
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

        public override string Text
        {
            get
            {
                if (BloodSugar == string.Empty)
                    return string.Empty;
                else
                    return string.Format(factories.Settings.SugarUnit == SugarUnit.mgdL
                        ? Translations.BloodSugarMgdL : Translations.BloodSugarMmolL, BloodSugar);
            }
        }
    }
}
