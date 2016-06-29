using System;
using System.Collections.Generic;
using System.Reflection;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;

namespace Dietphone
{
    public class LazyViewFinder : IMvxViewFinder
    {
        private readonly Func<Assembly> viewAssembly;
        private bool mapHasBeenBuilt;
        private IDictionary<Type, Type> map;

        public LazyViewFinder(Func<Assembly> viewAssembly)
        {
            this.viewAssembly = viewAssembly;
        }

        public Type GetViewType(Type viewModelType)
        {
            EnsureMapHasBeenBuilt();
            Type viewType;
            map.TryGetValue(viewModelType, out viewType);
            return viewType;
        }

        private void EnsureMapHasBeenBuilt()
        {
            if (mapHasBeenBuilt)
                return;
            BuildMap();
            mapHasBeenBuilt = true;
        }

        private void BuildMap()
        {
            var builder = new MvxViewModelViewLookupBuilder();
            var resolvedViewAssembly = viewAssembly();
            map = builder.Build(new[] { resolvedViewAssembly });
        }
    }
}
