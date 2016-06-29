// Based on https://github.com/jamesmontemagno/Xamarin.Plugins/blob/master/Vibrate/Refractored.Xam.Vibrate.Android/Vibrate.cs
using Android.App;
using Android.Content;
using Android.OS;

namespace Dietphone.Tools
{
    public class VibrationImpl : Vibration
    {
        private const int BUTTON_PRESS_MILLISECONDS = 100;

        public void VibrateOnButtonPress()
        {
            VibrateMiliseconds(BUTTON_PRESS_MILLISECONDS);
        }

        private void VibrateMiliseconds(int miliseconds)
        {
            var context = Application.Context;
            using (var vibration = (Vibrator)context.GetSystemService(Context.VibratorService))
            {
                vibration.Vibrate(miliseconds);
            }
        }
    }
}