using System.Threading;

namespace Dietphone.Tools
{
    public interface TimerFactory
    {
        Timer Create(TimerCallback callback, object state, int dueTime, int period);
    }

    public class TimerFactoryImpl : TimerFactory
    {
        public Timer Create(TimerCallback callback, object state, int dueTime, int period)
        {
            return new Timer(callback: callback, state: state, dueTime: dueTime, period: period);
        }
    }
}
