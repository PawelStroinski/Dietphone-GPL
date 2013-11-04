using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace Dietphone.Common.Phone.Tests
{
    public static class TestsExtensionMethods
    {
        public static void ChangesProperty(
            this INotifyPropertyChanged viewModel, string propertyName, Action action)
        {
            var changed = false;
            var propertyChangedEventHandler = new PropertyChangedEventHandler((_, e) =>
            {
                if (e.PropertyName == propertyName)
                    changed = true;
            });
            viewModel.PropertyChanged += propertyChangedEventHandler;
            try
            {
                action();
            }
            finally
            {
                viewModel.PropertyChanged -= propertyChangedEventHandler;
            }
            Assert.IsTrue(changed);
        }
    }
}
