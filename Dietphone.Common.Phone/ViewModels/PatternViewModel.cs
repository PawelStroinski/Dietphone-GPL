using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;

namespace Dietphone.ViewModels
{
    public class PatternViewModel
    {
        public Pattern Pattern { get; private set; }
        public MealItemViewModel Match { get; private set; }
        public MealViewModel From { get; private set; }
        public InsulinViewModel Insulin { get; private set; }
        public SugarViewModel Before { get; private set; }
        public IList<SugarViewModel> After { get; private set; }
        public MealItemViewModel For { get; private set; }
        public bool HasAlternatives { get; private set; }
        private readonly Navigator navigator;
        private readonly Action save;
        private readonly Action showAlternatives;

        public PatternViewModel(Pattern pattern, Factories factories,
            IList<InsulinCircumstanceViewModel> allCircumstances, bool hasAlternatives,
            IEnumerable<MealNameViewModel> names, MealNameViewModel defaultName, Navigator navigator, Action save,
            Action showAlternatives)
        {
            Pattern = pattern;
            Match = new MealItemViewModel(pattern.Match, factories);
            From = new MealViewModel(pattern.From, factories);
            From.Names = names;
            From.DefaultName = defaultName;
            Insulin = new InsulinViewModel(pattern.Insulin, factories, allCircumstances: allCircumstances);
            Before = new SugarViewModel(pattern.Before, factories);
            After = pattern.After
                .Select(sugar => new SugarViewModel(sugar, factories))
                .ToList();
            For = new MealItemViewModel(pattern.For, factories);
            HasAlternatives = hasAlternatives;
            this.navigator = navigator;
            this.save = save;
            this.showAlternatives = showAlternatives;
        }

        public string Factor
        {
            get
            {
                return string.Format("{0}%", Math.Round(Pattern.Factor * 100));
            }
        }

        public void GoToMeal()
        {
            save();
            navigator.GoToMealEditing(From.Id);
        }

        public void GoToInsulin()
        {
            save();
            navigator.GoToInsulinEditing(Insulin.Id);
        }

        public void ShowAlternatives()
        {
            CheckHasAlternatives();
            showAlternatives();
        }

        private void CheckHasAlternatives()
        {
            if (!HasAlternatives)
                throw new InvalidOperationException("No alternatives found.");
        }
    }
}
