using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace Dietphone.Tools
{
    public sealed class TrialImpl : Trial
    {
        public bool IsTrial()
        {
#if DEBUG
            return true;
#else
            return Guide.IsTrialMode;
#endif
        }

        public void Show()
        {
            Guide.ShowMarketplace(PlayerIndex.One);
        }
    }
}
