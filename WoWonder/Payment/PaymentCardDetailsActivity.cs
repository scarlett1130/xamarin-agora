using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Com.Stripe.Android.View;
using Java.Lang;
using Org.Apache.Commons.Logging;
using Stripe;
using WoWonder.Activities.Base;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Math = System.Math;
using Token = Stripe.Token;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentCardDetailsActivity : BaseActivity
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtName;
        private AppCompatButton BtnApply;
        private CardMultilineWidget MultilineWidget;

        //private Stripe Stripe;
        private string Price, PayType, Id, TokenId;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.PaymentCardDetailsLayout);

                Id = Intent?.GetStringExtra("Id") ?? "";
                Price = Intent?.GetStringExtra("Price") ?? "";
                PayType = Intent?.GetStringExtra("payType") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitWalletStripe();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardExpire = (TextView)FindViewById(Resource.Id.card_expire);
                CardCvv = (TextView)FindViewById(Resource.Id.card_cvv);
                CardName = (TextView)FindViewById(Resource.Id.card_name);

                MultilineWidget = (CardMultilineWidget)FindViewById(Resource.Id.card_multiline_widget);

                EtName = (EditText)FindViewById(Resource.Id.et_name);
                BtnApply = (AppCompatButton)FindViewById(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_CreditCard);
                    toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MultilineWidget.CvcComplete += MultilineWidgetOnCvcComplete;
                    EtName.TextChanged += EtNameOnTextChanged;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    MultilineWidget.CvcComplete -= MultilineWidgetOnCvcComplete;
                    EtName.TextChanged -= EtNameOnTextChanged;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MultilineWidgetOnCvcComplete(object sender, EventArgs e)
        {
            try
            {
                if (MultilineWidget.Card != null && MultilineWidget.Card.ValidateCard() && MultilineWidget.ValidateAllFields())
                {
                    if (MultilineWidget.Card.Number.Trim().Length == 0)
                    {
                        CardNumber.Text = "**** **** **** ****";
                    }
                    else
                    {
                        string number = InsertPeriodically(MultilineWidget.Card.Number.Trim(), " ", 4);
                        CardNumber.Text = number;
                    }

                    if (MultilineWidget.Card.ExpMonth.ToString().Trim().Length == 0 && MultilineWidget.Card.ExpYear.ToString().Trim().Length == 0)
                    {
                        CardExpire.Text = "MM/YY";
                    }
                    else
                    {
                        CardExpire.Text = MultilineWidget.Card.ExpMonth + "/" + MultilineWidget.Card.ExpYear;
                    }

                    CardCvv.Text = MultilineWidget.Card.CVC.Trim().Length == 0 ? "***" : MultilineWidget.Card.CVC.Trim();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EtNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardName.Text = e?.Text?.ToString().Trim().Length == 0 ? GetString(Resource.String.Lbl_YourName) : e?.Text?.ToString().Trim();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Stripe
        private  void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MultilineWidget.Card.ValidateCard() && !string.IsNullOrEmpty(EtName.Text))
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var card = MultilineWidget.Card;
                   // StripeConfiguration.ApiKey = "sk_test_51JJn5THCqjEYLm0pTxgR3xf6yzoY1ynFIaULsB0kHxvdHekc1C8unIiHlRaXQkMgshEpq4gqImTy0JtlU4g5prHh0038FjteCN";

                    var options = new TokenCreateOptions
                    {
                        Card = new TokenCardOptions
                        {
                            Number = card.Number,
                            ExpMonth = card.ExpMonth.ToString(),
                            ExpYear = card.ExpYear.ToString(),
                            Cvc = card.CVC.ToString(),
                            Name= CardName.Text,
                        },
                    };

                    var service = new TokenService();
                  var  token=  service.Create(options);
                    OnSuccess(token);   
                    //Changes Ahsan Zaman
                    //Stripe.CreateToken(card, PaymentConfiguration.Instance.PublishableKey,this);
                    // OnSuccess(token);

                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseVerifyDataCard), ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                ToastUtils.ShowToast(this, exception.Message, ToastLength.Long);
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string InsertPeriodically(string text, string insert, int period)
        {
            try
            {
                var parts = SplitInParts(text, period);
                string formatted = string.Join(insert, parts);
                return formatted;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        #endregion

        #region Stripe

        private void InitWalletStripe()
        {
            try
            {
                var stripePublishableKey = ListUtils.SettingsSiteList?.StripeId ?? "pk_live_51JJn5THCqjEYLm0pyPU7ewiJx74qqdnQ6TqBv81tEYxorKP5OTupU0otLRo4XMztaex5Yc48xUM8O9l8bdgJHtfC00lkdxglhS";
                if (!string.IsNullOrEmpty(stripePublishableKey))
                {
                    PaymentConfiguration.Init(stripePublishableKey);
                    //Stripe = new Stripe(this, stripePublishableKey);
                    //Live URL
                      StripeConfiguration.ApiKey = "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";
                    //Test URL
                   // StripeConfiguration.ApiKey = "sk_test_51JJn5THCqjEYLm0pTxgR3xf6yzoY1ynFIaULsB0kHxvdHekc1C8unIiHlRaXQkMgshEpq4gqImTy0JtlU4g5prHh0038FjteCN";
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(Java.Lang.Exception error)
        {
            try
            {
                AndHUD.Shared.Dismiss(this);
                ToastUtils.ShowToast(this, error.Message, ToastLength.Long);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnSuccess(Token token)
        {
            try
            {
                //Changes Ahsan Zaman
                // Send token to your own web service
                //var stripeBankAccount = token.BankAccount;
                //var stripeCard = token.Card;
                //var stripeCreated = token.Created;
                TokenId = token.Id;
                //var stripeLiveMode = token.Livemode;
                //var stripeType = token.Type;
                //var stripeUsed = token.Used;

                if (Methods.CheckConnectivity())
                {
                    switch (PayType)
                    {
                        //send api  
                        case "Funding":
                            {
                                // Changes Ahsan Zaman
                                //var (apiStatus, respond) = await RequestsAsync.Payments.StripeAsync(TokenId, "fund", Id, Price);
                                double price = Convert.ToDouble(Price);
                                var options = new Stripe.ChargeCreateOptions
                                {
                                    Amount = (Convert.ToInt32(price) * 100),
                                    Currency = "usd",
                                    ReceiptEmail = UserDetails.Email,
                                    Source = TokenId,


                                };
                                //and Create Method of this object is doing the payment execution.  
                                var service = new Stripe.ChargeService();
                                Stripe.Charge charge = service.Create(options);
                            if (charge.Status == "succeeded")
                            {
                                   
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long);
                                        FundingViewActivity.GetInstance()?.StartApiService();
                                        break;

                                }
                                else
                                {
                                   
                                        Methods.DisplayReportResult(this, charge.Status);
                                   
                                }

                                break;
                            }
                        case "membership" when Methods.CheckConnectivity():
                        {
                                // Changes Ahsan Zaman

                                //  stripeCheckout.Checkout.CustomerCreation(TokenId, UserDetails.Email);
                                //stripeCheckout.Checkout.ChargePayment(Convert.ToDouble(Price), UserDetails.Email, TokenId);
                                //var (apiStatus, respond) = await RequestsAsync.Payments.StripeAsync(TokenId, "pro", Id, Price);
                                double price = Convert.ToDouble(Price);
                                var options = new Stripe.ChargeCreateOptions
                                {
                                    Amount = (Convert.ToInt32(price) * 100),
                                    Currency = "usd",
                                    ReceiptEmail = UserDetails.Email,
                                    Source = TokenId,


                                };
                                //and Create Method of this object is doing the payment execution.  
                                var service = new Stripe.ChargeService();
                                Stripe.Charge charge = service.Create(options);
                               if( charge.Status== "succeeded")
                                {
                                   
                                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                            if (dataUser != null)
                                            {
                                                dataUser.IsPro = "1";

                                                var sqlEntity = new SqLiteDatabase();
                                                sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);

                                            }

                                    ///Changes Ahsan Zaman
                                    //Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                                    //AlertDialog alert = dialog.Create();
                                    //alert.SetTitle("Success");
                                    //alert.SetMessage("Thank you for your payment. Your upgrade was successful.");
                                    //alert.SetButton("OK", (c, ev) =>
                                    //{
                                    //    // Ok button click task  
                                    //    StartActivity(typeof(MyProfileActivity));
                                    //    Methods.DisplayReportResult(this, "Thank you for your payment. Your upgrade was successful");
                                    //});
                                    //alert.Show();
                                    //StartActivity(typeof(MyProfileActivity));
                                    //ToastUtils.ShowToast(this,"", ToastLength.Long);
                                    var result = await AlertAsync(this, "Success", "Thank you for your payment. Your upgrade was successful.", "Ok");
                                    if(result)
                                        StartActivity(typeof(MyProfileActivity));
                                    break;


                                }
                                else
                                {
                                    
                                        Methods.DisplayReportResult(this, charge.StripeResponse);
                                    
                                }

                                break;
                            }
                        case "membership":
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            break;
                        case "AddFunds":
                            {
                                var tabbedWallet = TabbedWalletActivity.GetInstance();
                                if (Methods.CheckConnectivity() && tabbedWallet != null)
                                {
                                    double price = Convert.ToDouble(Price);
                                    var options = new Stripe.ChargeCreateOptions
                                    {
                                        Amount = (Convert.ToInt32(price) * 100),
                                        Currency = "usd",
                                        ReceiptEmail = UserDetails.Email,
                                        Source = TokenId,


                                    };
                                    //and Create Method of this object is doing the payment execution.  
                                    var service = new Stripe.ChargeService();
                                    Stripe.Charge charge = service.Create(options);
                                    if (charge.Status == "succeeded")
                                    ///Changes Ahsan Zaman
                                    // var (apiStatus, respond) = await RequestsAsync.Payments.StripeAsync(TokenId, "wallet", "", Price);

                                    {

                                        tabbedWallet.SendMoneyFragment.TxtAmount.Text = string.Empty;
                                            tabbedWallet.SendMoneyFragment.TxtEmail.Text = string.Empty;

                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Sent_successfully), ToastLength.Long);
                                            break;

                                    }
                                    else
                                    {
                                      
                                            Methods.DisplayReportResult(this, charge.Status);
                                      
                                    }
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                }

                                break;
                            }
                    }

                    AndHUD.Shared.Dismiss(this);
                   Finish();
                    StartActivity(typeof(MyProfileActivity));
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                ToastUtils.ShowToast(this, e.Message, ToastLength.Long);
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        }
        public Task<bool> AlertAsync(Context context, string title, string message, string positiveButton)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (var db = new AlertDialog.Builder(context))
            {
                db.SetTitle(title);
                db.SetMessage(message);
                
                db.SetPositiveButton(positiveButton, (sender, args) => { tcs.TrySetResult(true); });
                //db.SetNegativeButton(negativeButton, (sender, args) => { tcs.TrySetResult(false); });
              var dialog=  db.Show();
                int textColorId = Resources.GetIdentifier("Ok", "id", "android");
                TextView textColor = dialog.FindViewById<TextView>(textColorId);
                textColor?.SetTextColor(Color.Black);
            }

            return tcs.Task;
        }
        #endregion  
    }
}