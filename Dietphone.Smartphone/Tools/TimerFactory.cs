using System;
using System.Threading.Tasks;

namespace Dietphone.Tools
{
    public interface TimerFactory
    {
        void Create(Action callback, int dueTime);
    }

    public class TimerFactoryImpl : TimerFactory
    {
        public void Create(Action callback, int dueTime)
        {
            Task
                .Delay(dueTime)
                .ContinueWith(_ => callback());
        }
    }
}
