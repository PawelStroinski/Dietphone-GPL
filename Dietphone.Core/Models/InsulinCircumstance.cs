using Dietphone.Views;

namespace Dietphone.Models
{
    public class InsulinCircumstance : EntityWithId
    {
        public InsulinCircumstanceKind Kind { get; set; }
        private string customName = string.Empty;

        // Please note that setting Name may change Kind.
        public string Name
        {
            get
            {
                return GetNameByKind();
            }
            set
            {
                if (Name != value)
                {
                    Kind = InsulinCircumstanceKind.Custom;
                    customName = value;
                }
            }
        }

        private string GetNameByKind()
        {
            switch (Kind)
            {
                case InsulinCircumstanceKind.Custom:
                    return customName;
                case InsulinCircumstanceKind.Exercise:
                    return Translations.Exercise;
                case InsulinCircumstanceKind.Sickness:
                    return Translations.Sickness;
                case InsulinCircumstanceKind.Stress:
                    return Translations.Stress;
                default:
                    return string.Empty;
            }
        }
    }

    public enum InsulinCircumstanceKind
    {
        Custom,
        Exercise,
        Sickness,
        Stress
    }
}
