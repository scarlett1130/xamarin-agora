using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.TellFriend
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class WithdrawalsActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic
        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;
        private TextView TextMiniRequest;

        private TextView TxtMyBalance;
        private EditText TxtWithdrawMethod, TxtAmount, TxtPayPalEmail, TxtAccountNumber, TxtCountry, TxtAccountName, TxtSwiftCode, TxtAddress;
        private LinearLayout LayoutPayPalEmail, LayoutBank;
        private AppCompatButton BtnRequestWithdrawal;
        private double CountBalance;
        private string TypeDialog, TypeWithdrawMethod = "paypal";

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
                SetContentView(Resource.Layout.WithdrawalsLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_Data_User();
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

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                Avatar = FindViewById<ImageView>(Resource.Id.avatar);
                TxtProfileName = FindViewById<TextView>(Resource.Id.name);
                TxtUsername = FindViewById<TextView>(Resource.Id.tv_subname);

                TextMiniRequest = FindViewById<TextView>(Resource.Id.description);

                TxtMyBalance = FindViewById<TextView>(Resource.Id.myBalance);

                TxtWithdrawMethod = FindViewById<EditText>(Resource.Id.WithdrawMethodEditText);

                TxtAmount = FindViewById<EditText>(Resource.Id.AmountEditText);

                LayoutPayPalEmail = FindViewById<LinearLayout>(Resource.Id.LayoutPayPalEmail);
                TxtPayPalEmail = FindViewById<EditText>(Resource.Id.PayPalEmailEditText);

                LayoutBank = FindViewById<LinearLayout>(Resource.Id.LayoutBank);
                TxtAccountNumber = FindViewById<EditText>(Resource.Id.AccountNumberEditText);

                TxtCountry = FindViewById<EditText>(Resource.Id.CountryEditText);

                TxtAccountName = FindViewById<EditText>(Resource.Id.AccountNameEditText);

                TxtSwiftCode = FindViewById<EditText>(Resource.Id.SwiftCodeEditText);

                TxtAddress = FindViewById<EditText>(Resource.Id.AddressEditText);

                BtnRequestWithdrawal = FindViewById<AppCompatButton>(Resource.Id.RequestWithdrawalButton);

                Methods.SetColorEditText(TxtWithdrawMethod, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPayPalEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAccountNumber, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCountry, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAccountName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSwiftCode, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAddress, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtWithdrawMethod);
                Methods.SetFocusable(TxtCountry);

                AdsGoogle.Ad_AdMobNative(this);

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
                    toolBar.Title = GetText(Resource.String.Lbl_Withdrawals);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        TxtWithdrawMethod.Touch += TxtWithdrawMethodOnTouch;
                        TxtCountry.Touch += TxtCountryOnTouch;
                        BtnRequestWithdrawal.Click += BtnRequestWithdrawalOnClick;
                        break;
                    default:
                        TxtWithdrawMethod.Touch -= TxtWithdrawMethodOnTouch;
                        TxtCountry.Touch -= TxtCountryOnTouch;
                        BtnRequestWithdrawal.Click -= BtnRequestWithdrawalOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                TxtMyBalance = null!;
                TxtWithdrawMethod = null!;
                TxtAmount = null!;
                LayoutPayPalEmail = null!;
                TxtPayPalEmail = null!;
                LayoutBank = null!;
                TxtAccountNumber = null!;
                TxtCountry = null!;
                TxtAccountName = null!;
                TxtSwiftCode = null!;
                TxtAddress = null!;
                BtnRequestWithdrawal = null!;
                TypeDialog = null!;
                TypeWithdrawMethod = null!;
                Avatar = null!;
                TxtProfileName = null!;
                TxtUsername = null!;
                TextMiniRequest = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                TypeDialog = "Country";

                var countriesArray = WoWonderTools.GetCountryList(this);

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Country)).TitleColorRes(Resource.Color.primary);
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

        private void TxtWithdrawMethodOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "WithdrawMethod";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = new List<string>
                {
                    GetText(Resource.String.Btn_Paypal), GetText(Resource.String.Lbl_Bank)
                };

                dialogList.Title(GetText(Resource.String.Lbl_WithdrawMethod)).TitleColorRes(Resource.Color.primary);
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

        private async void BtnRequestWithdrawalOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CountBalance < Convert.ToDouble(TxtAmount.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ThereIsNoBalance), ToastLength.Long);
                    return;
                }

                if (Convert.ToDouble(TxtAmount.Text) < Convert.ToDouble(ListUtils.SettingsSiteList?.MWithdrawal))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CantRequestWithdrawals), ToastLength.Long);
                    return;
                }

                switch (TypeWithdrawMethod)
                {
                    case "paypal" when string.IsNullOrEmpty(TxtPayPalEmail.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtAmount.Text.Replace(" ", "")):
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                        return;
                    case "bank" when string.IsNullOrEmpty(TxtAmount.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtAccountNumber.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtCountry.Text.Replace(" ", ""))
                                     || string.IsNullOrEmpty(TxtAccountName.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtSwiftCode.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtAddress.Text.Replace(" ", "")):
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                        return;
                }

                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"type", TypeWithdrawMethod},
                        {"amount", TxtAmount.Text},
                    };

                    switch (TypeWithdrawMethod)
                    {
                        case "paypal":
                            dictionary.Add("paypal_email", TxtPayPalEmail.Text);
                            break;
                        case "bank":
                            dictionary.Add("iban", TxtAccountNumber.Text);
                            dictionary.Add("country", TxtCountry.Text);
                            dictionary.Add("full_name", TxtAccountName.Text);
                            dictionary.Add("swift_code", TxtSwiftCode.Text);
                            dictionary.Add("address", TxtAddress.Text);
                            break;
                    }

                    var (apiStatus, respond) = await RequestsAsync.Global.WithdrawAsync(dictionary);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result:
                                        Console.WriteLine(result.Message);
                                        AndHUD.Shared.Dismiss(this);
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_RequestSentWithdrawals), ToastLength.Long);

                                        Finish();
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayAndHudErrorResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

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

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "WithdrawMethod":
                        {
                            if (itemString == GetText(Resource.String.Btn_Paypal))
                            {
                                TypeWithdrawMethod = "paypal";
                                LayoutPayPalEmail.Visibility = ViewStates.Visible;
                                LayoutBank.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                TypeWithdrawMethod = "bank";

                                LayoutPayPalEmail.Visibility = ViewStates.Gone;
                                LayoutBank.Visibility = ViewStates.Visible;
                            }

                            TxtWithdrawMethod.Text = itemString;
                            break;
                        }
                    case "Country":
                        TxtCountry.Text = itemString;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void Get_Data_User()
        {
            try
            {
                switch (ListUtils.MyProfileList?.Count)
                {
                    case 0:
                        await ApiRequest.Get_MyProfileData_Api(this);
                        break;
                }

                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    GlideImageLoader.LoadImage(this, local.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtProfileName.Text = WoWonderTools.GetNameFinal(local);
                    TxtUsername.Text = "@" + local.Username;

                    CountBalance = Convert.ToDouble(local.Balance);
                    TextMiniRequest.Text = GetText(Resource.String.Lbl_Withdrawals_SubText2) + " $" + ListUtils.SettingsSiteList?.MWithdrawal;
                    TxtMyBalance.Text = "$" + CountBalance.ToString(CultureInfo.InvariantCulture);

                    TxtPayPalEmail.Text = local.PaypalEmail;

                    TxtWithdrawMethod.Text = GetText(Resource.String.Btn_Paypal);
                    TypeWithdrawMethod = "paypal";
                    LayoutPayPalEmail.Visibility = ViewStates.Visible;
                    LayoutBank.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}