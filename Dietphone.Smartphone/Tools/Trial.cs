using System;

namespace Dietphone.Tools
{
    public interface Trial
    {
        void IsTrial(Action<bool> callback);
        void Show();
    }
}
