using System;
using System.Reflection;
using MvvmCross.WindowsPhone.Views;

namespace Dietphone
{
    public class MultiAssemblyViewsContainer : MvxPhoneViewsContainer
    {
        private static Assembly mainAssembly = Assembly.GetExecutingAssembly();

        protected override string GetConventionalXamlUrlForView(Type viewType)
        {
            return GetAssemblyPrefixForXamlUrl(viewType.Assembly)
                + base.GetConventionalXamlUrlForView(viewType);
        }

        private static string GetAssemblyPrefixForXamlUrl(Assembly viewAssembly)
        {
            return viewAssembly == mainAssembly ? string.Empty
                : $"/{viewAssembly.GetName().Name};component";
        }
    }
}
