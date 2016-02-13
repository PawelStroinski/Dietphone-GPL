using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Platform;
using MvvmCross.WindowsPhone.Platform;
using Microsoft.Phone.Controls;
using MvvmCross.Platform;
using Dietphone.Tools;
using MvvmCross.WindowsPhone.Views;
using Dietphone.Views;

namespace Dietphone
{
    public class Setup : MvxPhoneSetup
    {
        public Setup(PhoneApplicationFrame rootFrame) : base(rootFrame)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new MyApp();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override IMvxPhoneViewsContainer CreateViewsContainer(PhoneApplicationFrame rootFrame)
        {
            var viewsContainer = new MultiAssemblyViewsContainer();
            var sometimesViewFinder = new LazyViewFinder(() => typeof(ProductEditing).Assembly);
            viewsContainer.AddSecondary(sometimesViewFinder);
            var rarelyViewFinder = new LazyViewFinder(() => typeof(ExportAndImport).Assembly);
            viewsContainer.AddSecondary(rarelyViewFinder);
            return viewsContainer;
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
            Mvx.RegisterType<Clipboard, ClipboardImpl>();
            Mvx.RegisterType<Trial, TrialImpl>();
            Mvx.RegisterType<Vibration, VibrationImpl>();
        }
    }
}
