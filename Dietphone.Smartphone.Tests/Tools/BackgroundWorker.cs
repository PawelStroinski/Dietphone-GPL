using Dietphone.ViewModels;

namespace Dietphone.Smartphone.Tests
{
    public class BackgroundWorkerNoOp : BackgroundWorkerBase
    {
        public BackgroundWorkerNoOp() : base(false)
        {
        }

        public override void RunWorkerAsync()
        {
        }
    }

    public class BackgroundWorkerSyncFactory : BackgroundWorkerFactory
    {
        public bool Called { get; private set; }
        public bool NoOp;

        public BackgroundWorkerBase Create()
        {
            Called = true;
            if (NoOp)
                return new BackgroundWorkerNoOp();
            else
                return new BackgroundWorkerSync(verbose: false);
        }

        public BackgroundWorkerBase CreateVerbose()
        {
            Called = true;
            if (NoOp)
                return new BackgroundWorkerNoOp();
            else
                return new BackgroundWorkerSync(verbose: true);
        }
    }
}
