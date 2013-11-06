using System;
using System.ComponentModel;
using System.Linq;

namespace Dietphone.ViewModels
{
    public abstract class BackgroundWorkerBase
    {
        public event DoWorkEventHandler DoWork;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;
        public abstract void RunWorkerAsync();

        protected virtual void OnDoWork(DoWorkEventArgs eventArguments)
        {
            DoWork(this, eventArguments);
        }

        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs eventArguments)
        {
            RunWorkerCompleted(this, eventArguments);
        }
    }

    public interface BackgroundWorkerFactory
    {
        BackgroundWorkerBase Create();
    }

    public class BackgroundWorkerWrapper : BackgroundWorkerBase
    {
        public override void RunWorkerAsync()
        {
            var implementation = new BackgroundWorker();
            implementation.DoWork += (_, eventArguments) => OnDoWork(eventArguments);
            implementation.RunWorkerCompleted += (_, eventArguments) => OnRunWorkerCompleted(eventArguments);
            implementation.RunWorkerAsync();
        }
    }

    public class BackgroundWorkerWrapperFactory : BackgroundWorkerFactory
    {
        public BackgroundWorkerBase Create()
        {
            return new BackgroundWorkerWrapper();
        }
    }

    public class BackgroundWorkerSync : BackgroundWorkerBase
    {
        public override void RunWorkerAsync()
        {
            var doWorkEventArguments
                = new DoWorkEventArgs(null);
            OnDoWork(doWorkEventArguments);
            var runWorkerCompletedEventArguments
                = new RunWorkerCompletedEventArgs(doWorkEventArguments.Result, null, false);
            OnRunWorkerCompleted(runWorkerCompletedEventArguments);
        }
    }

    public class BackgroundWorkerSyncFactory : BackgroundWorkerFactory
    {
        public BackgroundWorkerBase Create()
        {
            return new BackgroundWorkerSync();
        }
    }
}
