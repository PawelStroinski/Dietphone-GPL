// Code taken from http://www.japf.fr/2009/02/very-simple-mvvm-demo-application/ and adjusted for SL as well as added support for notification of all properties
// The following code is inspired by the work of Josh Smith
// http://joshsmithonwpf.wordpress.com/
// The default indexed property added from http://enginecore.blogspot.co.uk/2013/09/localization-in-xamarin-mvvmcross_18.html
using System.Diagnostics;
using System.Reflection;
using Dietphone.Views;
using MvvmCross.Core.ViewModels;

namespace Dietphone.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes in the application. Provides support for 
    /// property changes notification.
    /// </summary>
    public abstract class ViewModelBase : MvxViewModel
    {
        public ViewModelBase()
        {
            ShouldAlwaysRaiseInpcOnUserInterfaceThread(false);
        }

        /// <summary>
        /// Warns the developer if this object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        public void VerifyPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            // verify that the property name matches a real,  
            // public, instance property on this object.
            if (GetType().GetRuntimeProperty(propertyName) == null)
            {
                Debug.Assert(false, "Invalid property name: " + propertyName);
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            RaisePropertyChanged(propertyName);
        }

        public string this[string index] => Translations.ResourceManager.GetString(index);
    }
}
