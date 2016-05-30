using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace Dietphone.Tools
{
    public sealed class TrialImpl : Trial
    {
        public void IsTrial(Action<bool> callback)
        {
#if DEBUG
            callback(true);
#else
            callback(Guide.IsTrialMode);
#endif
        }

        public void Show()
        {
            Guide.ShowMarketplace(PlayerIndex.One);
        }
    }
}
