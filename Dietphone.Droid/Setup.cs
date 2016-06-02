// FilesDir use in InitializeStreamProvider method inspiered by https://github.com/MvvmCross/MvvmCross-Plugins/blob/b766150dcbd183c155850848060ec8522fcd53f7/File/MvvmCross.Plugins.File.Droid/MvxAndroidFileStore.cs
using Android.Content;
using MvvmCross.Droid.Platform;
using MvvmCross.Core.ViewModels;
using Dietphone.BinarySerializers;
using MvvmCross.Platform;
using Dietphone.Tools;
using Dietphone.ViewModels;
using MvvmCross.Binding.Bindings.Target.Construction;
using Android.Widget;
using Dietphone.TargetBindings;
using Dietphone.Controls;
using Dietphone.Views;
using Android.Views;

namespace Dietphone
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new MyApp();
        }

        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();
            InitializeStreamProvider();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
            RegisterTypes();
            SetTranslationsCulture();
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TabHost>("CurrentTab",
                target => new TabHostCurrentTab(target));
            registry.RegisterCustomBindingFactory<BackEditText>("TextOnFocusLost",
                target => new BackEditTextTextOnFocusLost(target));
            registry.RegisterCustomBindingFactory<EditText>("SelectRight",
                target => new EditTextSelectRight(target));
            registry.RegisterCustomBindingFactory<View>("Focus",
                target => new ViewFocus(target));
            registry.RegisterCustomBindingFactory<ImageButton>("OpaqueEnabled",
                target => new ImageButtonOpaqueEnabled(target));
            base.FillTargetFactories(registry);
        }

        private void InitializeStreamProvider()
        {
            var root = ApplicationContext.FilesDir;
            var fileFactory = new NativeFileFactory(root.Path);
            MyApp.StreamProvider = new DroidBinaryStreamProvider(fileFactory, ApplicationContext);
        }

        private void RegisterTypes()
        {
            Mvx.RegisterType<Clipboard, ClipboardImpl>();
            Mvx.RegisterType<JournalViewModel, GroupingJournalViewModel>();
            Mvx.RegisterType<MessageDialog, MessageDialogImpl>();
            Mvx.RegisterType<NavigationService, NavigationServiceImpl>();
            Mvx.RegisterType<ProductListingViewModel, GroupingProductListingViewModel>();
            //Mvx.RegisterSingleton<Trial>(() => new TrialImpl());
            Mvx.RegisterType<Trial, TrialImpl>();
            Mvx.RegisterType<Vibration, VibrationImpl>();
        }

        private void SetTranslationsCulture()
        {
            Translations.Culture = MyApp.CurrentUiCultureInfo;
        }
    }
}
