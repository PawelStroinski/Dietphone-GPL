// The GetPackageInfo method based on https://forums.xamarin.com/discussion/22243/how-to-get-versioncode,
// the Sign method based on http://buchananweb.co.uk/security01i.aspx
// and the Send method based on http://stackoverflow.com/a/6117969
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Telephony;
using Java.Lang;
using MvvmCross.Platform;
using Newtonsoft.Json;

namespace Dietphone.Tools
{
    public static partial class Logger
    {
        private const string JSON_MIME_TYPE = "application/json";
        private const string INFO = "info";
        private const string DEBUG = "debug";
        private const string ERROR = "error";
        private const string PLATFORM = "droid";

        public static void Info(string message)
        {
            Log(message, level: INFO);
        }

        public static void Debug(string message)
        {
            Log(message, level: DEBUG);
        }

        public static void Error(string message)
        {
            Log(message, level: ERROR);
        }

        private static void Log(string message, string level)
        {
            var packageInfo = GetPackageInfo();
            var occurred = JavaSystem.CurrentTimeMillis();
            var signature = Sign(message + ":" + occurred);
            var content = new Dictionary<string, object>
            {
                { "app", packageInfo.PackageName },
                { "platform", PLATFORM },
                { "version", packageInfo.VersionCode.ToString() },
                { "level", level },
                { "message", message },
                { "device-id", GetDeviceId() },
                { "device-model", DeviceModel },
                { "os-version", OsVersion },
                { "occurred", occurred },
                { "signature", signature }
            };
            Send(JsonConvert.SerializeObject(content));
        }

        private static PackageInfo GetPackageInfo()
        {
            var context = Application.Context;
            var packageManager = context.PackageManager;
            return packageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MatchDefaultOnly);
        }

        private static string Sign(string input)
        {
            var secretBytes = Encoding.UTF8.GetBytes(Secret);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hmac = new HMACSHA256(secretBytes);
            var signatureBytes = hmac.ComputeHash(inputBytes);
            var signatureInHexFormat = string.Concat(signatureBytes.Select(item => item.ToString("X2")));
            return signatureInHexFormat;
        }

        private static string GetDeviceId()
        {
            string deviceId = null;
            var context = Application.Context;
            var telephonyManager = context.GetSystemService(Context.TelephonyService) as TelephonyManager;
            if (telephonyManager != null)
                deviceId = telephonyManager.DeviceId;
            if (string.IsNullOrEmpty(deviceId))
                deviceId = Build.Serial;
            if (string.IsNullOrEmpty(deviceId))
                deviceId = "?";
            return deviceId;
        }

        private static string DeviceModel => $"{Build.Brand} {Build.Model}";

        private static string OsVersion => $"{Build.VERSION.Release} {Build.VERSION.SdkInt} {Build.VERSION.Codename}";

        private static void Send(string json)
        {
            var content = new StringContent(json, Encoding.UTF8, mediaType: JSON_MIME_TYPE);
            var client = new HttpClient();
            client.PostAsync(URL, content).ContinueWith(result =>
            {
                if (result.IsGeneralSuccess())
                {
                    var response = result.Result;
                    var httpContent = response.Content;
                    var contentTask = httpContent.ReadAsStringAsync();
                    var responseContent = contentTask.Result;
                    Mvx.Trace($"Logged '{json}' and got '{response.StatusCode}' and content '{responseContent}'");
                }
                else
                    Mvx.Trace($"Logging '{json}' failed with an exception '{result.Exception}'");
            });
        }
    }
}