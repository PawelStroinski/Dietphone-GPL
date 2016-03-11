// Some techniques used in the InitializeStreamProvider method are from https://github.com/MvvmCross/MvvmCross-Plugins/blob/b766150dcbd183c155850848060ec8522fcd53f7/File/MvvmCross.Plugins.File.Droid/MvxAndroidFileStore.cs
using Android.Content;
using MvvmCross.Droid.Platform;
using MvvmCross.Core.ViewModels;
using Dietphone.BinarySerializers;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid;
using Dietphone.Tools;

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

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
            InitializeStreamProvider();
            Mvx.RegisterType<MessageDialog, MessageDialogImpl>();
        }

        private void InitializeStreamProvider()
        {
            var globals = Mvx.Resolve<IMvxAndroidGlobals>();
            var context = globals.ApplicationContext;
            var root = context.FilesDir;
            var fileFactory = new NativeFileFactory(root.Path);
            MyApp.StreamProvider = new DroidBinaryStreamProvider(fileFactory, context);
        }
    }
}
