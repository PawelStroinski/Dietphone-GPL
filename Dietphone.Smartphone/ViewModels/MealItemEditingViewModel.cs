using System;
using Dietphone.Models;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class MealItemEditingViewModel : InlineViewModel<MealItemViewModel>
    {
        public const string MEAL_ITEM = "MEAL_ITEM";

        public override void Tombstone()
        {
            var state = StateProvider.State;
            state[MEAL_ITEM] = Subject.SerializeModel();
        }

        protected override void Untombstone()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(MEAL_ITEM))
            {
                var stateValue = (string)state[MEAL_ITEM];
                var untombstoned = stateValue.Deserialize<MealItem>(string.Empty);
                if (Subject.ProductId == untombstoned.ProductId)
                {
                    Subject.CopyFromModel(untombstoned);
                }
            }
        }

        protected override void ClearTombstoning()
        {
            var state = StateProvider.State;
            state.Remove(MEAL_ITEM);
        }
    }
}