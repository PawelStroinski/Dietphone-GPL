using Dietphone.Models;
using Dietphone.Tools;

namespace Dietphone.ViewModels
{
    public class SugarEditingViewModel : InlineViewModel<SugarViewModel>
    {
        public const string SUGAR = "SUGAR";

        public override void Tombstone()
        {
            var state = StateProvider.State;
            state[SUGAR] = Subject.Sugar.Serialize(string.Empty);
        }

        protected override void Untombstone()
        {
            var state = StateProvider.State;
            if (state.ContainsKey(SUGAR))
            {
                var stateValue = (string)state[SUGAR];
                var untombstoned = stateValue.Deserialize<Sugar>(string.Empty);
                Subject.Sugar.CopyFrom(untombstoned);
            }
        }

        protected override void ClearTombstoning()
        {
            var state = StateProvider.State;
            state.Remove(SUGAR);
        }
    }
}
