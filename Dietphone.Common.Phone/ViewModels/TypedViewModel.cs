namespace Dietphone.ViewModels
{
    public abstract class TypedViewModel : ViewModelWithDateAndText
    {
        public bool IsInsulin
        {
            get { return this is InsulinViewModel; }
        }

        public bool IsSugar
        {
            get { return this is SugarViewModel; }
        }

        public bool IsMeal
        {
            get { return this is MealViewModel; }
        }

        public bool IsNotMeal
        {
            get { return !(this is MealViewModel); }
        }
    }
}
