using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace Dietphone
{
    public static class TestsExtensionMethods
    {
        public static void ChangesProperty(
            this INotifyPropertyChanged viewModel, string propertyName, Action action)
        {
            var changed = false;
            var propertyChangedEventHandler = new PropertyChangedEventHandler((_, eventArguments) =>
            {
                if (eventArguments.PropertyName == propertyName)
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
            Assert.IsTrue(changed, string.Format("Expected property {0} change", propertyName));
        }

        public static void NotChangesProperty(
            this INotifyPropertyChanged viewModel, string propertyName, Action action)
        {
            var changed = false;
            var propertyChangedEventHandler = new PropertyChangedEventHandler((_, eventArguments) =>
            {
                if (eventArguments.PropertyName == propertyName)
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
            Assert.IsFalse(changed, string.Format("Expected property {0} not changed", propertyName));
        }
    }
}
