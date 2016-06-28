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
            var activityHolder = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = activityHolder.Activity;
            if (activity == null)
            {
                Logger.Error("No current activity.");
                return;
            }
            var service = new InAppBillingServiceConnection(activity, PUBLIC_KEY);
            service.OnInAppBillingError += (error, message) => Logger.Error($"{error} - {message}");
            service.OnConnected += () =>
            {
                var billing = service.BillingHandler;
                billing.BuyProductError += (code, sku) => Logger.Error($"Error {code} while buying {sku}");
                billing.InAppBillingProcesingError += Logger.Error;
                billing.OnGetProductsError += (code, items) => Logger.Error($"Error {code} when getting items {items}");
                billing.OnProductPurchasedError += (code, sku) => Logger.Error($"Error {code} after purchased {sku}");
                billing.OnPurchaseConsumedError += (code, token) => Logger.Error($"Error {code} consuming {token}");
                billing.QueryInventoryError += (code, skus) => Logger.Error($"Error {code} getting inventory {skus}");
                action(service);
            };
            service.Connect();
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
            var billing = service.BillingHandler;
            var purchases = billing.GetPurchases(ItemType.Product);
            var purchased = purchases.Any(purchase => purchase.ProductId == PRODUCT_ID);
            var isTrial = !purchased;
            timerFactory.Create(() => callback(isTrial), 1000);
        }
    }
}
