using System;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using Xamarin.InAppBilling;
using System.Linq;
using System.Collections.Generic;
using Android.App;

namespace Dietphone.Tools
{
    public sealed partial class TrialImpl : Trial
    {
        private readonly TimerFactory timerFactory;
        private readonly MessageDialog messageDialog;
        private static InAppBillingServiceConnection toDisconnect;
        private const string PRODUCT_ID = "registration";

        public TrialImpl(TimerFactory timerFactory, MessageDialog messageDialog)
        {
            this.timerFactory = timerFactory;
            this.messageDialog = messageDialog;
        }

        public void IsTrial(Action<bool> callback)
        {
            Logger.Debug("In TrialImpl.IsTrial");
            WithService(service => IsTrial(service, callback));
        }

        public void Show()
        {
            WithService(Show);
        }

        public static void Disconnect()
        {
            try
            {
                if (toDisconnect != null)
                    toDisconnect.Disconnect();
            }
            finally
            {
                toDisconnect = null;
            }
        }

        private void WithService(Action<InAppBillingServiceConnection> action)
        {
            Logger.Debug("In TrialImpl.WithService 1");
            var activityHolder = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = activityHolder.Activity;
            if (activity == null)
            {
                Logger.Error("No current activity.");
                return;
            }
            Logger.Debug("In TrialImpl.WithService 2");
            var service = new InAppBillingServiceConnection(activity, PUBLIC_KEY);
            Logger.Debug("In TrialImpl.WithService 3");
            service.OnInAppBillingError += (error, message) => Logger.Error($"{error} - {message}");
            service.OnConnected += () =>
            {
                Logger.Debug("In TrialImpl.WithService 6");
                var billing = service.BillingHandler;
                Logger.Debug("In TrialImpl.WithService 7");
                billing.BuyProductError += (code, sku) => Logger.Error($"Error {code} while buying {sku}");
                billing.InAppBillingProcesingError += Logger.Error;
                billing.OnGetProductsError += (code, items) => Logger.Error($"Error {code} when getting items {items}");
                billing.OnProductPurchasedError += (code, sku) => Logger.Error($"Error {code} after purchased {sku}");
                billing.OnPurchaseConsumedError += (code, token) => Logger.Error($"Error {code} consuming {token}");
                billing.QueryInventoryError += (code, skus) => Logger.Error($"Error {code} getting inventory {skus}");
                Logger.Debug("In TrialImpl.WithService 8");
                action(service);
                Logger.Debug("In TrialImpl.WithService 9");
            };
            Logger.Debug("In TrialImpl.WithService 4");
            service.Connect();
            Logger.Debug("In TrialImpl.WithService 5");
        }

        private void IsTrial(InAppBillingServiceConnection service, Action<bool> callback)
        {
            try
            {
                IsTrialDo(service, callback);
            }
            finally
            {
                service.Disconnect();
            }
        }

        private void Show(InAppBillingServiceConnection service)
        {
            Disconnect();
            toDisconnect = service;
            var billing = service.BillingHandler;
            var ids = new List<string> { PRODUCT_ID };
            billing.QueryInventoryAsync(ids, ItemType.Product).ContinueWith(continuation =>
            {
                var products = continuation.Result;
                if (products == null)
                {
                    Logger.Error("List of items to buy is null.");
                    return;
                }
                if (!products.Any())
                {
                    Logger.Error("List of items to buy is empty.");
                    return;
                }
                var product = products.First();
                var synchronizationContext = Application.SynchronizationContext;
                synchronizationContext.Post(_ => billing.BuyProduct(product), null);
            });
        }

        private void IsTrialDo(InAppBillingServiceConnection service, Action<bool> callback)
        {
            Logger.Debug("In TrialImpl.IsTrialDo 1");
            var billing = service.BillingHandler;
            Logger.Debug("In TrialImpl.IsTrialDo 2");
            var purchases = billing.GetPurchases(ItemType.Product);
            Logger.Debug("In TrialImpl.IsTrialDo 3");
            var purchased = purchases.Any(purchase => purchase.ProductId == PRODUCT_ID);
            Logger.Debug("In TrialImpl.IsTrialDo 4");
            var isTrial = !purchased;
            Logger.Debug("In TrialImpl.IsTrialDo 5, isTrial=" + isTrial);
            timerFactory.Create(() => callback(isTrial), 1000);
        }

        //public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        //{
        //    if (serviceConnection != null)
        //        serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
        //}
    }
}
