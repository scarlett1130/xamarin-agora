using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.ViewPager2.Widget;
using Com.Braintreepayments.Api.Dropin;
using Com.Razorpay;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using WoWonder.Activities.Wallet.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using BaseActivity = WoWonder.Activities.Base.BaseActivity;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Wallet
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedWalletActivity : BaseActivity, IPaymentResultListener, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        public ViewPager2 ViewPager;
        private TabLayout TabLayout;

        public SendMoneyFragment SendMoneyFragment;
        public AddFundsFragment AddFundsFragment;
        public string TypeOpenPayment = "";
        private static TabbedWalletActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.TabbedWalletLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                //SetupBottomNavigationView();
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

                switch (AppSettings.ShowRazorPay)
                {
                    case true:
                        AddFundsFragment?.InitRazorPay?.StopRazorPay();
                        break;
                }

                switch (AppSettings.ShowPayStack)
                {
                    case true:
                        AddFundsFragment?.PayStackPayment?.StopPayStack();
                        break;
                }

                switch (AppSettings.ShowCashFree)
                {
                    case true:
                        AddFundsFragment?.CashFreePayment?.StopCashFree();
                        break;
                }

                switch (AppSettings.ShowPaySera)
                {
                    case true:
                        AddFundsFragment?.PaySeraPayment?.StopPaySera();
                        break;
                }

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
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();
                //BottomBar = FindViewById<ReadableBottomBar>(Resource.Id.readableBottomBar);
                //BottomBar.SetOnItemSelectListener(this); 
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
                    toolBar.Title = GetString(Resource.String.Lbl_WalletCredits);
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

        public static TabbedWalletActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                SendMoneyFragment = null!;
                AddFundsFragment = null!;
                TypeOpenPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                SendMoneyFragment = new SendMoneyFragment();
                AddFundsFragment = new AddFundsFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(SendMoneyFragment, GetText(Resource.String.Lbl_SendMoney));
                Adapter.AddFragment(AddFundsFragment, GetText(Resource.String.Lbl_AddFunds));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Permissions && Result

        //Result 
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (TypeOpenPayment)
                {
                    case "AddFundsFragment" when requestCode == InitPayPalPayment.PayPalDataRequestCode:
                        switch (resultCode)
                        {
                            case Result.Ok:
                            {
                                DropInResult dropInResult = (DropInResult)data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                                if (dropInResult != null)
                                {
                                    InitPayPalPayment.DisplayResult(dropInResult.PaymentMethodNonce, dropInResult.DeviceData);
                                    if (Methods.CheckConnectivity())
                                    {
                                        var (apiStatus, respond) = await RequestsAsync.Global.TopUpWalletAsync(UserDetails.UserId, AddFundsFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        switch (apiStatus)
                                        {
                                            case 200:
                                                RunOnUiThread(() =>
                                                {
                                                    try
                                                    {
                                                        AddFundsFragment.TxtAmount.Text = string.Empty;

                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Methods.DisplayReportResultTrack(e);
                                                    }
                                                });
                                                break;
                                            default:
                                                Methods.DisplayReportResult(this, respond);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }
                                }
                            }
                                break;
                            case Result.Canceled:
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long);
                                break;
                            case Result.FirstUser:
                                var ss = (Exception)data.GetSerializableExtra(DropInActivity.ExtraError);
                                Toast.MakeText(this, ss?.Message, ToastLength.Long)?.Show();
                                break;
                        } 
                        break; 
                    default:
                        {
                            switch (requestCode)
                            {
                                case 1202 when resultCode == Result.Ok:
                                    {
                                        var userObject = data.GetStringExtra("DataUser");
                                        switch (string.IsNullOrEmpty(userObject))
                                        {
                                            case false:
                                                {
                                                    var userData = JsonConvert.DeserializeObject<UserDataObject>(userObject);
                                                    if (userData != null)
                                                    {
                                                        SendMoneyFragment.TxtEmail.Text = WoWonderTools.GetNameFinal(userData);
                                                        SendMoneyFragment.UserId = userData.UserId;
                                                    }

                                                    break;
                                                }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OnPaymentError(int code, string response)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                ToastUtils.ShowToast(this, "Payment failed: " + response, ToastLength.Long);
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
                            var priceInt = Convert.ToInt32(AddFundsFragment?.Price) * 100;
                            var keyValues = new Dictionary<string, string>
                        {
                            {"merchant_amount", priceInt.ToString()},
                        };

                            var (apiStatus, respond) = await RequestsAsync.Payments.RazorPayAsync(razorpayPaymentId, "wallet", keyValues).ConfigureAwait(false);
                            switch (apiStatus)
                            {
                                case 200:
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            AddFundsFragment.TxtAmount.Text = string.Empty;
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                                default:
                                    Methods.DisplayReportResult(this, respond);
                                    break;
                            }

                            break;
                        }
                    case false:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnConfigureTab(TabLayout.Tab p0, int p1)
        {
            try
            {
                p0.SetText(Adapter.GetFragment(p1));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}