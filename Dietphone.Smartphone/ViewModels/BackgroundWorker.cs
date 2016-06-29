using System.ComponentModel;

namespace Dietphone.ViewModels
{
    public abstract class BackgroundWorkerBase
    {
        public event DoWorkEventHandler DoWork;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;
        public abstract void RunWorkerAsync();
        private readonly bool verbose;

        public BackgroundWorkerBase(bool verbose)
        {
            this.verbose = verbose;
        }

        protected virtual void OnDoWork(DoWorkEventArgs eventArguments)
        {
            DoWork(this, eventArguments);
        }

        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs eventArguments)
        {
            ThrowErrorIfVerbose(eventArguments);
            if (RunWorkerCompleted != null)
                RunWorkerCompleted(this, eventArguments);
        }

        private void ThrowErrorIfVerbose(RunWorkerCompletedEventArgs eventArguments)
        {
            if (verbose && eventArguments.Error != null)
            {
                throw eventArguments.Error;
            }
        }
    }

    public interface BackgroundWorkerFactory
    {
        BackgroundWorkerBase Create();
        BackgroundWorkerBase CreateVerbose();
    }

    public class BackgroundWorkerWrapper : BackgroundWorkerBase
    {
        public BackgroundWorkerWrapper(bool verbose) : base(verbose)
        {
        }

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
            return new BackgroundWorkerWrapper(verbose: false);
        }

        public BackgroundWorkerBase CreateVerbose()
        {
            return new BackgroundWorkerWrapper(verbose: true);
        }
    }

    public class BackgroundWorkerSync : BackgroundWorkerBase
    {
        public BackgroundWorkerSync(bool verbose) : base(verbose)
        {
        }

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
}
