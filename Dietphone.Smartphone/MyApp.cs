using System;
using System.Reflection;
using Dietphone.BinarySerializers;
using Dietphone.Models;
using Dietphone.ViewModels;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;

namespace Dietphone
{
    public class MyApp : MvxApplication
    {
        public static BinaryStreamProvider StreamProvider { private get; set; }
        private static Factories factories = null;
        private static readonly object factoriesLock = new object();

        public override void Initialize()
        {
            var assemblyWithViewModel = typeof(MainViewModel).GetTypeInfo().Assembly;
            var assemblyWithFactories = typeof(Factories).GetTypeInfo().Assembly;
            CreatableTypes(assemblyWithViewModel)
                .AsTypes()
                .RegisterAsDynamic();
            CreatableTypes(assemblyWithViewModel)
                .AsInterfaces()
                .RegisterAsDynamic();
            CreatableTypes(assemblyWithFactories)
                .AsInterfaces()
                .RegisterAsDynamic();
            Mvx.RegisterSingleton(() => Factories);
            Mvx.RegisterType<BackgroundWorkerFactory, BackgroundWorkerWrapperFactory>();
            RegisterAppStart<MainViewModel>();
        }

        public static Factories Factories
        {
            get
            {
                lock (factoriesLock)
                {
                    if (factories == null)
                    {
                        CreateFactories();
                    }
                    return factories;
                }
            }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException("Factories");
                }
                factories = value;
            }
        }

        public static string CurrentUiCulture
        {
            get
            {
                var settings = Factories.Settings;
                return settings.CurrentUiCulture;
            }
        }

        private static void CreateFactories()
        {
            var storageCreator = new BinaryStorageCreator(StreamProvider);
            factories = new FactoriesImpl();
            factories.StorageCreator = storageCreator;
            storageCreator.CultureName = CurrentProductCulture;
        }

        private static string CurrentProductCulture
        {
            get
            {
                var settings = Factories.Settings;
                return settings.CurrentProductCulture;
            }
        }
    }
}
