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
            messageDialog.Show("In TrialImpl.IsTrial");
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
            messageDialog.Show("In TrialImpl.WithService 1");
            var activityHolder = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = activityHolder.Activity;
            if (activity == null)
            {
                Error("No current activity.");
                return;
            }
            messageDialog.Show("In TrialImpl.WithService 2");
            var service = new InAppBillingServiceConnection(activity, PUBLIC_KEY);
            messageDialog.Show("In TrialImpl.WithService 3");
            service.OnInAppBillingError += (error, message) => Error($"{error} - {message}");
            service.OnConnected += () =>
            {
                messageDialog.Show("In TrialImpl.WithService 6");
                var billing = service.BillingHandler;
                messageDialog.Show("In TrialImpl.WithService 7");
                billing.BuyProductError += (code, sku) => Error($"Error {code} while buying {sku}");
                billing.InAppBillingProcesingError += Error;
                billing.OnGetProductsError += (code, items) => Error($"Error {code} while getting items {items}");
                billing.OnProductPurchasedError += (code, sku) => Error($"Error {code} after purchased {sku}");
                billing.OnPurchaseConsumedError += (code, token) => Error($"Error {code} consuming {token}");
                billing.QueryInventoryError += (code, skus) => Error($"Error {code} while getting inventory {skus}");
                messageDialog.Show("In TrialImpl.WithService 8");
                action(service);
                messageDialog.Show("In TrialImpl.WithService 9");
            };
            messageDialog.Show("In TrialImpl.WithService 4");
            service.Connect();
            messageDialog.Show("In TrialImpl.WithService 5");
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
                    Error("List of items to buy is null.");
                    return;
                }
                if (!products.Any())
                {
                    Error("List of items to buy is empty.");
                    return;
                }
                var product = products.First();
                var synchronizationContext = Application.SynchronizationContext;
                synchronizationContext.Post(_ => billing.BuyProduct(product), null);
            });
        }

        private void IsTrialDo(InAppBillingServiceConnection service, Action<bool> callback)
        {
            messageDialog.Show("In TrialImpl.IsTrialDo 1");
            var billing = service.BillingHandler;
            messageDialog.Show("In TrialImpl.IsTrialDo 2");
            var purchases = billing.GetPurchases(ItemType.Product);
            messageDialog.Show("In TrialImpl.IsTrialDo 3");
            var purchased = purchases.Any(purchase => purchase.ProductId == PRODUCT_ID);
            messageDialog.Show("In TrialImpl.IsTrialDo 4");
            var isTrial = !purchased;
            messageDialog.Show("In TrialImpl.IsTrialDo 5, isTrial=" + isTrial);
            timerFactory.Create(() => callback(isTrial), 1000);
        }

        private void Error(string message)
        {
            messageDialog.Show("There was an app registration error. It does not affect functionality.\n\n" + message);
        }

        //public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        //{
        //    if (serviceConnection != null)
        //        serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
        //}
    }
}
