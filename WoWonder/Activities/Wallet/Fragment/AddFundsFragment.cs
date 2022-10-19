using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Com.Razorpay;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Wallet.Fragment
{
    public class AddFundsFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, IPaymentResultListener
    {
        #region  Variables Basic
        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;

        private TextView TxtMyBalance;
        public EditText TxtAmount;
        private AppCompatButton BtnContinue;
        public InitPayPalPayment InitPayPalPayment;
        public InitPayStackPayment PayStackPayment;
        public InitCashFreePayment CashFreePayment;
        public InitRazorPayPayment InitRazorPay;
        public InitPaySeraPayment PaySeraPayment;
        public string Price;
        private TabbedWalletActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedWalletActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.AddFundsLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitBuy();
                InitComponent(view);
                AddOrRemoveEvent(true);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitBuy()
        {
            try
            {
                InitPayPalPayment = AppSettings.ShowPaypal switch
                {
                    true => new InitPayPalPayment(Activity),
                    _ => InitPayPalPayment
                };

                InitRazorPay = AppSettings.ShowRazorPay switch
                {
                    true => new InitRazorPayPayment(Activity),
                    _ => InitRazorPay
                };

                PayStackPayment = AppSettings.ShowPayStack switch
                {
                    true => new InitPayStackPayment(Activity),
                    _ => PayStackPayment
                };

                CashFreePayment = AppSettings.ShowCashFree switch
                {
                    true => new InitCashFreePayment(Activity),
                    _ => CashFreePayment
                };

                PaySeraPayment = AppSettings.ShowPaySera switch
                {
                    true => new InitPaySeraPayment(Activity),
                    _ => PaySeraPayment
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent(View view)
        {
            try
            {
                Avatar = view.FindViewById<ImageView>(Resource.Id.avatar);
                TxtProfileName = view.FindViewById<TextView>(Resource.Id.name);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.tv_subname);

                TxtMyBalance = view.FindViewById<TextView>(Resource.Id.myBalance);

                TxtAmount = view.FindViewById<EditText>(Resource.Id.AmountEditText);
                BtnContinue = view.FindViewById<AppCompatButton>(Resource.Id.ContinueButton);

                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var userData = ListUtils.MyProfileList?.FirstOrDefault();
                if (userData != null)
                {
                    GlideImageLoader.LoadImage(GlobalContext, userData.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtProfileName.Text = WoWonderTools.GetNameFinal(userData);
                    TxtUsername.Text = "@" + userData.Username;

                    TxtMyBalance.Text = userData.Wallet;
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        BtnContinue.Click += BtnContinueOnClick;
                        break;
                    default:
                        BtnContinue.Click -= BtnContinueOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnContinueOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short);
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    return;
                }

                GlobalContext.TypeOpenPayment = "AddFundsFragment";
                Price = TxtAmount.Text;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                switch (AppSettings.ShowPaypal)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));
                        break;
                }

                switch (AppSettings.ShowCreditCard)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));
                        break;
                }

                switch (AppSettings.ShowBankTransfer)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));
                        break;
                }

                switch (AppSettings.ShowRazorPay)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));
                        break;
                }

                switch (AppSettings.ShowPayStack)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));
                        break;
                }

                switch (AppSettings.ShowCashFree)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));
                        break;
                }

                switch (AppSettings.ShowPaySera)
                {
                    case true:
                        arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));
                        break;
                }

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    InitPayPalPayment.BtnPaypalOnClick(Price, "AddFunds");
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
                else if (text == GetString(Resource.String.Lbl_RazorPay))
                {
                    InitRazorPay?.BtnRazorPayOnClick(Price, "AddFunds", "");
                }
                else if (text == GetString(Resource.String.Lbl_PayStack))
                {
                    var dialogBuilder = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(Resource.String.Lbl_PayStack).TitleColorRes(Resource.Color.primary);
                    dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                    {
                        try
                        {
                            switch (s.Length)
                            {
                                case <= 0:
                                    return;
                            }

                            var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                            switch (check)
                            {
                                case false:
                                    Methods.DialogPopup.InvokeAndShowDialog(Activity, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                    return;
                                default:
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                                    await PayStack(s.ToString());
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                    dialogBuilder.PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                    dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.Build().Show();
                }
                else if (text == GetString(Resource.String.Lbl_CashFree))
                {
                    OpenCashFreeDialog();
                }
                else if (text == GetString(Resource.String.Lbl_PaySera))
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                    await PaySera();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {

                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(Context, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "AddFunds");
                Context.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(Context, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "AddFunds");
                Context.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPaymentError(int code, string response)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_PaymentFailed) + ": " + response, ToastLength.Long);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                switch (string.IsNullOrEmpty(razorpayPaymentId))
                {
                    case false when Methods.CheckConnectivity():
                        {
                            var keyValues = new Dictionary<string, string>
                        {
                            {"merchant_amount", Price},
                        };

                            var (apiStatus, respond) = await RequestsAsync.Payments.RazorPayAsync(razorpayPaymentId, "wallet", keyValues).ConfigureAwait(false);
                            switch (apiStatus)
                            {
                                case 200:
                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            TxtAmount.Text = string.Empty;
                                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                                default:
                                    Methods.DisplayReportResult(Activity, respond);
                                    break;
                            }

                            break;
                        }
                    case false:
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(Price) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"amount", priceInt.ToString()},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePayStackAsync("wallet", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        PayStackPayment ??= new InitPayStackPayment(Activity);
                                        PayStackPayment.DisplayPayStackPayment(result.Url, "AddFunds", priceInt.ToString(), "");
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(Activity, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light)
                    .Title(GetText(Resource.String.Lbl_CashFree)).TitleColorRes(Resource.Color.primary)
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            switch (check)
                            {
                                case false:
                                    Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                    return;
                            }

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                                return;
                            }

                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                            await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    })
                    .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog())
                    .Build();

                var iconName = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = dialog.CustomView.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = dialog.CustomView.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = dialog.CustomView.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = WoWonderTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(string name, string email, string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                        {"amount", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeCashFreeAsync("wallet", AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CashFreeObject result:
                                        CashFreePayment ??= new InitCashFreePayment(Activity);
                                        CashFreePayment.DisplayCashFreePayment(result, "AddFunds", Price, "");
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(Activity, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PaySera()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"amount", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePaySeraAsync("wallet", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        PaySeraPayment ??= new InitPaySeraPayment(Activity);
                                        PaySeraPayment.DisplayPaySeraPayment(result.Url, "AddFunds", Price, "");
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(Activity, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

    }
}