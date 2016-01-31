using System;
using System.Collections.Generic;
using System.Linq;
using Dietphone.Models;

namespace Dietphone.ViewModels
{
    public class ReplacementItemViewModel
    {
        public ReplacementItem ReplacementItem { get; private set; }
        public PatternViewModel Pattern { get; private set; }
        public IList<PatternViewModel> Alternatives { get; private set; }
        private readonly Action<IList<PatternViewModel>> showAlternatives;

        public ReplacementItemViewModel(ReplacementItem replacementItem, Factories factories,
            IList<InsulinCircumstanceViewModel> allCircumstances, IEnumerable<MealNameViewModel> names,
            MealNameViewModel defaultName, Navigator navigator, Action save, Action<IList<PatternViewModel>> showAlternatives)
        {
            ReplacementItem = replacementItem;
            Func<Pattern, bool, PatternViewModel> createPatternViewModel = (pattern, hasAlternatives) => 
                new PatternViewModel(pattern, factories, allCircumstances: allCircumstances,
                    hasAlternatives: hasAlternatives, names: names, defaultName: defaultName, navigator: navigator,
                    save: save, showAlternatives: ShowAlternatives);
            Pattern = createPatternViewModel(replacementItem.Pattern, replacementItem.Alternatives.Any());
            Alternatives = replacementItem.Alternatives
                .Select(pattern => createPatternViewModel(pattern, false))
                .ToList();
            this.showAlternatives = showAlternatives;
        }

        private void ShowAlternatives()
        {
            showAlternatives(Alternatives);
        }
    }
}
