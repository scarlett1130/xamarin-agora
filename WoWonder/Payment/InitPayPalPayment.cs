using System;
using System.Linq;
using Android.App;
using Android.Gms.Wallet;
using Com.Braintreepayments.Api.Dropin;
using Com.Braintreepayments.Api.Dropin.Utils;
using Com.Braintreepayments.Api.Internal;
using Com.Braintreepayments.Api.Models;
using Com.Braintreepayments.Cardform.View;
using Com.Paypal.Android.Sdk.Onetouch.Core;
using WoWonder.Helpers.Utils;

namespace WoWonder.Payment
{
    public class InitPayPalPayment
    {
        private readonly Activity ActivityContext;
        public const int PayPalDataRequestCode = 7171;

        public InitPayPalPayment(Activity activity)
        {
            try
            {
                ActivityContext = activity;

                SignatureVerification.SEnableSignatureVerification = false;
                PayPalOneTouchCore.UseHardcodedConfig(activity, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Paypal
        public void BtnPaypalOnClick(string price, string payType)
        {
            try
            {
                var dropInRequest = InitPayPal(price, payType);
                if (dropInRequest == null)
                    return;

                ActivityContext.StartActivityForResult(dropInRequest.GetIntent(ActivityContext), PayPalDataRequestCode);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private DropInRequest InitPayPal(string price, string payType)
        {
            try
            {
                var option = ListUtils.MyProfileList.FirstOrDefault();

                var currency = ListUtils.SettingsSiteList?.Currency ?? "USD";

                var googlePayment = new GooglePaymentRequest()
                    .InvokeTransactionInfo(TransactionInfo.NewBuilder()
                        .SetTotalPrice(price)
                        .SetCurrencyCode(currency)
                        .SetTotalPriceStatus(WalletConstants.TotalPriceStatusFinal)
                        .Build())
                    .PaypalEnabled(true)
                    .EmailRequired(true);

                ThreeDSecurePostalAddress billingAddress = new ThreeDSecurePostalAddress()
                    .InvokeGivenName("Jill")
                    .InvokeSurname("Doe")
                    .InvokePhoneNumber("5551234567")
                    .InvokeStreetAddress("555 Smith St")
                    .InvokeExtendedAddress("#2")
                    .InvokeLocality("Chicago")
                    .InvokeRegion("IL")
                    .InvokePostalCode("12345")
                    .InvokeCountryCodeAlpha2("US");

                ThreeDSecureAdditionalInformation additionalInformation = new ThreeDSecureAdditionalInformation()
                    .InvokeAccountId("account-id");

                ThreeDSecureRequest threeDSecureRequest = new ThreeDSecureRequest()
                    .InvokeAmount(price)
                    .InvokeVersionRequested(ThreeDSecureRequest.Version2);

                if (option != null)
                    threeDSecureRequest.InvokeEmail(option.Email).InvokeMobilePhoneNumber(option.PhoneNumber);

                threeDSecureRequest
                    .InvokeBillingAddress(billingAddress)
                    .InvokeAdditionalInformation(additionalInformation);

                var name = payType switch
                {
                    "Funding" => "Donate to funding",
                    "membership" => "Account upgrade request",
                    "AddFunds" => "Add to balance",
                    _ => "Pay the card"
                };

                PayPalRequest paypalRequest = new PayPalRequest(price)
                    .InvokeCurrencyCode(currency)
                    .InvokeMerchantAccountId(AppSettings.MerchantAccountId)
                    .InvokeDisplayName(AppSettings.ApplicationName)
                    .InvokeBillingAgreementDescription(name)
                    //.InvokeLandingPageType("billing")
                    .InvokeIntent(PayPalRequest.IntentAuthorize);

                DropInRequest dropInRequest = new DropInRequest();
                //.ClientToken(MAuthorization)

                if (ListUtils.SettingsSiteList != null)
                {
                    switch (ListUtils.SettingsSiteList.PaypalMode)
                    {
                        case "sandbox":
                            dropInRequest.TokenizationKey(AppSettings.SandboxTokenizationKey);
                            break;
                        case "live":
                            dropInRequest.TokenizationKey(AppSettings.ProductionTokenizationKey);
                            break;
                        default:
                            dropInRequest.TokenizationKey(AppSettings.ProductionTokenizationKey);
                            break;
                    }
                }

                dropInRequest.RequestThreeDSecureVerification(true)
                    .CollectDeviceData(true)
                    .InvokeGooglePaymentRequest(googlePayment)
                    .PaypalRequest(paypalRequest)
                    .MaskCardNumber(true)
                    .MaskSecurityCode(true)
                    .AllowVaultCardOverride(false)
                    .VaultCard(true)
                    .VaultManager(false)
                    .InvokeCardholderNameStatus(CardForm.FieldDisabled)
                    .InvokeThreeDSecureRequest(threeDSecureRequest);

                return dropInRequest;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
         
        public static void DisplayResult(PaymentMethodNonce paymentMethodNonce, string deviceData)
        {
            try
            {
                var mNonce = paymentMethodNonce;
                var mPaymentMethodType = PaymentMethodType.ForType(mNonce);

                var mPaymentMethodIcon = mPaymentMethodType.Drawable;
                var mPaymentMethodTitle = (paymentMethodNonce.TypeLabel);
                var mPaymentMethodDescription = (paymentMethodNonce.Description);

                var mNonceString = ": " + mNonce.Nonce;

                string details = "";
                if (mNonce is CardNonce cardNonce)
                {
                    details = "Card Last Two: " + cardNonce.LastTwo + "\n";
                    details += "3DS isLiabilityShifted: " + cardNonce.ThreeDSecureInfo.IsLiabilityShifted + "\n";
                    details += "3DS isLiabilityShiftPossible: " + cardNonce.ThreeDSecureInfo.IsLiabilityShiftPossible;
                }
                else if (mNonce is PayPalAccountNonce paypalAccountNonce)
                {
                    details = "First name: " + paypalAccountNonce.FirstName + "\n";
                    details += "Last name: " + paypalAccountNonce.LastName + "\n";
                    details += "Email: " + paypalAccountNonce.Email + "\n";
                    details += "Phone: " + paypalAccountNonce.Phone + "\n";
                    details += "Payer id: " + paypalAccountNonce.PayerId + "\n";
                    details += "Client metadata id: " + paypalAccountNonce.ClientMetadataId + "\n";
                    details += "Billing address: " + paypalAccountNonce.BillingAddress + "\n";
                    details += "Shipping address: " + paypalAccountNonce.ShippingAddress;
                }
                else if (mNonce is VenmoAccountNonce venmoAccountNonce)
                {
                    details = "Username: " + venmoAccountNonce.Username;
                }
                else if (mNonce is GooglePaymentCardNonce googlePaymentCardNonce)
                {
                    details = "Underlying Card Last Two: " + googlePaymentCardNonce.LastTwo + "\n";
                    details += "Email: " + googlePaymentCardNonce.Email + "\n";
                    details += "Billing address: " + googlePaymentCardNonce.BillingAddress + "\n";
                    details += "Shipping address: " + googlePaymentCardNonce.ShippingAddress;
                }

                Console.WriteLine(details);
                Console.WriteLine("Device Data: " + deviceData);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}