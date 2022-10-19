using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Braintreepayments.Api.Dropin;
using Com.Razorpay;
using InAppBilling.Lib;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Activities.Fundings.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonder.PaymentGoogle;
using WoWonderClient;
using WoWonderClient.Classes.Funding;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using BaseActivity = WoWonder.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FundingViewActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, IPaymentResultListener, IBillingPaymentListener
    {
        #region Variables Basic

        private ImageView ImageUser, ImageFunding, IconBack, Avatar;
        private TextView TxtMore, TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtFundRaise, TxtFundAmount, TxtDonation, Username;
        private ProgressBar ProgressBar;
        private AppCompatButton BtnDonate, BtnShare, BtnContact;
        private LinearLayout RecentDonationsLayout;
        private RecyclerView MRecycler;
        private RecentDonationAdapter MAdapter;
        private LinearLayoutManager LayoutManager;

        private FundingDataObject DataObject;
        private InitPayPalPayment InitPayPalPayment;
        private BillingSupport BillingSupport;

        private InitRazorPayPayment InitRazorPay;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitPaySeraPayment PaySeraPayment;
        private string DialogType = "";
        private string CodeName;
        private static FundingViewActivity Instance;

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
                SetContentView(Resource.Layout.FundingViewLayout);
                 
                Instance = this;
                
                //Get Value And Set Toolbar
                InitBuy();
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                LoadData();
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
                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    BillingSupport?.Destroy();

                switch (AppSettings.ShowRazorPay)
                {
                    case true:
                        InitRazorPay?.StopRazorPay();
                        break;
                }

                switch (AppSettings.ShowPayStack)
                {
                    case true:
                        PayStackPayment?.StopPayStack();
                        break;
                }
                
                switch (AppSettings.ShowCashFree)
                {
                    case true:
                        CashFreePayment?.StopCashFree();
                        break;
                }

                switch (AppSettings.ShowPaySera)
                {
                    case true:
                        PaySeraPayment?.StopPaySera();
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

        private void InitBuy()
        {
            try
            {
                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    BillingSupport = new BillingSupport(this, InAppBillingGoogle.ProductId, AppSettings.TripleDesAppServiceProvider, InAppBillingGoogle.ListProductSku, this);

                InitPayPalPayment = AppSettings.ShowPaypal switch
                {
                    true => new InitPayPalPayment(this),
                    _ => InitPayPalPayment
                };

                InitRazorPay = AppSettings.ShowRazorPay switch
                {
                    true => new InitRazorPayPayment(this),
                    _ => InitRazorPay
                };

                PayStackPayment = AppSettings.ShowPayStack switch
                {
                    true => new InitPayStackPayment(this),
                    _ => PayStackPayment
                };

                CashFreePayment = AppSettings.ShowCashFree switch
                {
                    true => new InitCashFreePayment(this),
                    _ => CashFreePayment
                };

                PaySeraPayment = AppSettings.ShowPaySera switch
                {
                    true => new InitPaySeraPayment(this),
                    _ => PaySeraPayment
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        private void InitComponent()
        {
            try
            {
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageFunding = FindViewById<ImageView>(Resource.Id.imageFunding);
                IconBack = FindViewById<ImageView>(Resource.Id.iv_back);

                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtFundRaise = FindViewById<TextView>(Resource.Id.raised);
                TxtFundAmount = FindViewById<TextView>(Resource.Id.TottalAmount);
                TxtDonation = FindViewById<TextView>(Resource.Id.timedonation);
                BtnDonate = FindViewById<AppCompatButton>(Resource.Id.DonateButton);
                BtnShare = FindViewById<AppCompatButton>(Resource.Id.share);
                BtnContact = FindViewById<AppCompatButton>(Resource.Id.cont);
                Avatar = FindViewById<ImageView>(Resource.Id.avatar);
                Username = FindViewById<TextView>(Resource.Id.name);

                RecentDonationsLayout = FindViewById<LinearLayout>(Resource.Id.layout_recent_donations);
                RecentDonationsLayout.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);

                TxtMore = FindViewById<TextView>(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMore, IonIconsFonts.More);
                if (TxtMore != null)
                {
                    TxtMore.SetTextSize(ComplexUnitType.Sp, 20f);
                    TxtMore.Visibility = ViewStates.Gone;
                }
                 
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                
                BtnContact.Visibility = AppSettings.MessengerIntegration switch
                {
                    false => ViewStates.Gone,
                    _ => BtnContact.Visibility
                };
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
                    toolBar.Title = " ";
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
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new RecentDonationAdapter(this)
                {
                    UserList = new ObservableCollection<RecentDonation>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter); 
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
                        TxtMore.Click += TxtMoreOnClick;
                        BtnDonate.Click += BtnDonateOnClick;
                        IconBack.Click += IconBackOnClick;
                        BtnShare.Click += BtnShareOnClick;
                        BtnContact.Click += BtnContactOnClick;
                        TxtTime.Click += UserImageAvatarOnClick;
                        TxtUsername.Click += UserImageAvatarOnClick;
                        ImageUser.Click += UserImageAvatarOnClick;
                        break;
                    default:
                        TxtMore.Click -= TxtMoreOnClick;
                        BtnDonate.Click -= BtnDonateOnClick;
                        IconBack.Click -= IconBackOnClick;
                        BtnShare.Click -= BtnShareOnClick;
                        BtnContact.Click -= BtnContactOnClick;
                        TxtTime.Click -= UserImageAvatarOnClick;
                        TxtUsername.Click -= UserImageAvatarOnClick;
                        ImageUser.Click -= UserImageAvatarOnClick;
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
                ImageUser = null!;
                ImageFunding = null!;
                IconBack = null!;
                TxtUsername = null!;
                TxtTime = null!;
                TxtTitle = null!;
                TxtDescription = null!;
                TxtFundRaise = null!;
                TxtFundAmount = null!;
                TxtDonation = null!;
                BtnDonate = null!;
                BtnShare = null!;
                BtnContact = null!;
                ProgressBar = null!;
                TxtMore = null!;
                InitRazorPay = null!;
                PayStackPayment = null!;
                CashFreePayment = null!;
                PaySeraPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static FundingViewActivity GetInstance()
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
        #endregion

        #region Events

        private void UserImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(this, DataObject.UserData.UserId, DataObject.UserData);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Contact User
        private void BtnContactOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (AppSettings.MessengerIntegration)
                {
                    case true when AppSettings.ShowDialogAskOpenMessenger:
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialog.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                        dialog.Content(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                        break;
                    }
                    case true:
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                        break;
                    default:
                        WoWonderTools.OpenProfile(this, DataObject.UserData.UserId, DataObject.UserData);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ShareEvent();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //BAck
        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "More";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                bool owner = DataObject.UserId == UserDetails.UserId;
                switch (owner)
                {
                    case true:
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                        break;
                }
                
                dialogList.Title(GetText(Resource.String.Lbl_More)).TitleColorRes(Resource.Color.primary);
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

        //Event Menu >> Edit
        private void EditEvent()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditFundingActivity));
                intent.PutExtra("FundingObject", JsonConvert.SerializeObject(DataObject));
                StartActivityForResult(intent, 253);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, InitializeWoWonder.WebsiteUrl + "/show_fund/" + DataObject.HashedId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    //Share Plugin same as video
                    case false:
                        return;
                    default:
                        await CrossShare.Current.Share(new ShareMessage
                        {
                            Title = DataObject.Title,
                            Text = DataObject.Description,
                            Url = InitializeWoWonder.WebsiteUrl + "/show_fund/" + DataObject.HashedId
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //open Payment
        private void BtnDonateOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "Donate";

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(Resource.String.Lbl_Donate).TitleColorRes(Resource.Color.primary);
                dialog.Input(Resource.String.Lbl_DonateCode, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        switch (s.Length)
                        {
                            case <= 0:
                                return;
                        }
                        CodeName = s.ToString();

                        if (Convert.ToDouble(CodeName) > Convert.ToDouble(DataObject.Amount))
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CantDonate) + " " + TxtFundAmount.Text, ToastLength.Long);
                            return;
                        }
                         
                        DialogType = "Payment";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        switch (AppSettings.ShowInAppBilling)
                        {
                            case true when InitializeWoWonder.IsExtended && Convert.ToInt64(CodeName) <= 100:
                                arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));
                                break;
                        }

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
                });
                dialog.InputType(InputTypes.ClassText);
                dialog.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                 
                switch (requestCode)
                {
                    case 253 when resultCode == Result.Ok:
                        {
                            if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                            var item = JsonConvert.DeserializeObject<FundingDataObject>(data.GetStringExtra("itemData") ?? "");
                            if (item != null)
                            {
                                DataObject = item;

                                TxtUsername.Text = Methods.FunString.DecodeString(item.UserData.Name);

                                TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " +
                                               Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);

                                TxtTitle.Text = Methods.FunString.DecodeString(item.Title);
                                TxtDescription.Text = Methods.FunString.DecodeString(item.Description);

                                ProgressBar.Progress = Convert.ToInt32(item.Bar);

                                //$0 Raised of $1000000
                                TxtFundRaise.Text = "$" + item.Raised.ToString(CultureInfo.InvariantCulture) + " " + GetString(Resource.String.Lbl_RaisedOf) + " " + "$" + item.Amount;
                            }

                            break;
                        }
                    case InitPayPalPayment.PayPalDataRequestCode:
                        switch (resultCode)
                        {
                            case Result.Ok:
                            {
                                DropInResult dropInResult = (DropInResult)data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                                if (dropInResult != null)
                                {
                                    InitPayPalPayment.DisplayResult(dropInResult.PaymentMethodNonce, dropInResult.DeviceData);
                                    await FundingPay();
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
                    case 2654 when resultCode == Result.Ok:
                        StartApiService();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                    InitPayPalPayment.BtnPaypalOnClick(CodeName, "Funding");
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    switch (CodeName)
                    {
                        case "5": // Donation with Amount 5
                            BillingSupport?.PurchaseNow("donation5");
                            break;
                        case "10":  // Donation with Amount 10
                            BillingSupport?.PurchaseNow("donation10");
                            break;
                        case  "15": // Donation with Amount 15
                            BillingSupport?.PurchaseNow("donation15");
                            break;
                        case  "20": // Donation with Amount 20
                            BillingSupport?.PurchaseNow("donation20");
                            break;
                        case  "25": // Donation with Amount 25
                            BillingSupport?.PurchaseNow("donation25");
                            break;
                        case  "30": // Donation with Amount 30
                            BillingSupport?.PurchaseNow("donation30");
                            break;
                        case  "35": // Donation with Amount 35
                            BillingSupport?.PurchaseNow("donation35");
                            break;
                        case  "40": // Donation with Amount 40
                            BillingSupport?.PurchaseNow("donation40");
                            break;
                        case  "45": // Donation with Amount 45
                            BillingSupport?.PurchaseNow("donation45");
                            break;
                        case  "50": // Donation with Amount 50
                            BillingSupport?.PurchaseNow("donation50");
                            break;
                        case  "55": // Donation with Amount 55
                            BillingSupport?.PurchaseNow("donation55");
                            break;
                        case  "60": // Donation with Amount 60
                            BillingSupport?.PurchaseNow("donation60");
                            break;
                        case  "65": // Donation with Amount 65
                            BillingSupport?.PurchaseNow("donation65");
                            break;
                        case  "70": // Donation with Amount 70
                            BillingSupport?.PurchaseNow("donation70");
                            break;
                        case  "75": // Donation with Amount 75
                            BillingSupport?.PurchaseNow("donation75");
                            break;
                        case  "80": // Donation with Amount 80
                        case  "85": // Donation with Amount 85
                        case  "90": // Donation with Amount 90
                        case  "95": // Donation with Amount 95
                        case  "100": // Donation with Amount 100
                        case "Funding": // Donation with Amount long 100
                            BillingSupport?.PurchaseNow("donationdefulte");
                            break;
                    } 
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
                    InitRazorPay?.BtnRazorPayOnClick(CodeName, "Funding" , "");
                }
                else if (text == GetString(Resource.String.Lbl_PayStack)) 
                {
                    DialogType = "PayStack";
                     
                    var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
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
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                    return;
                                default:
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

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
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                    await PaySera();  
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Edit))
                {
                    EditEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Copy))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                    dialogBuilder.Content(GetText(Resource.String.Lbl_DeleteFunding));
                    dialogBuilder.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            // Send Api delete  
                            if (Methods.CheckConnectivity())
                            {
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Funding.DeleteFundingAsync(DataObject.Id) });

                                var instance = FundingActivity.GetInstance();
                                var dataFunding = instance?.FundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataFunding != null)
                                {
                                    instance?.FundingTab?.MAdapter?.FundingList.Remove(dataFunding);
                                    instance.FundingTab?.MAdapter?.NotifyItemRemoved(instance.FundingTab.MAdapter.FundingList.IndexOf(dataFunding));
                                }

                                var dataMyFunding = instance?.MyFundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataMyFunding != null)
                                {
                                    instance?.MyFundingTab?.MAdapter?.FundingList.Remove(dataMyFunding);
                                    instance.MyFundingTab?.MAdapter?.NotifyItemRemoved(instance.MyFundingTab.MAdapter.FundingList.IndexOf(dataMyFunding));
                                }
                                 
                                var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                                var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal2 != null)
                                {
                                    foreach (var postData in dataGlobal2)
                                    {
                                        recycler.RemoveByRowIndex(postData);
                                    }
                                }

                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var diff = adapterGlobal?.ListDiffer;
                                var dataGlobal = diff?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal != null)
                                {
                                    foreach (var postData in dataGlobal)
                                    {
                                        WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                    }
                                }

                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogBuilder.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.ItemsCallback(this).Build().Show();
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
                if (DialogType != "Delete")
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
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
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", DataObject.Id);
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
                StartActivityForResult(intent, 2654);
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
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", DataObject.Id);
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
                StartActivity(intent);
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
                        var keyValues = new Dictionary<string, string>
                        {
                            {"merchant_amount", CodeName},
                            {"fund_id", DataObject.Id}
                        };

                        var (apiStatus, respond) = await RequestsAsync.Payments.RazorPayAsync(razorpayPaymentId, "fund", keyValues);
                        switch (apiStatus)
                        {
                            case 200:
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long);
                                StartApiService();
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

        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(CodeName) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"amount", priceInt.ToString()},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePayStackAsync("fund", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case InitializePaymentObject result:
                                    PayStackPayment ??= new InitPayStackPayment(this);
                                    PayStackPayment.DisplayPayStackPayment(result.Url, "Funding", priceInt.ToString(), DataObject.Id);
                                    break;
                            }

                            break;
                        }
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
                        {"amount", CodeName},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePaySeraAsync("fund", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case InitializePaymentObject result:
                                    PaySeraPayment ??= new InitPaySeraPayment(this);
                                    PaySeraPayment.DisplayPaySeraPayment(result.Url, "Funding", CodeName, DataObject.Id);
                                    break;
                            }

                            break;
                        }
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
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light)
                    .Title(GetText(Resource.String.Lbl_CashFree)).TitleColorRes(Resource.Color.primary)
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            switch (check)
                            {
                                case false:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                    return;
                            } 

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                                return;
                            }

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short);

                            await CashFree(TxtName.Text , TxtEmail.Text , TxtPhone.Text);
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

        private async Task CashFree(string name, string email , string phone)
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
                        {"amount", CodeName},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeCashFreeAsync("fund", AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "" , ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case CashFreeObject result:
                                    CashFreePayment ??= new InitCashFreePayment(this);
                                    CashFreePayment.DisplayCashFreePayment(result, "Funding", CodeName, DataObject.Id);
                                    break;
                            }

                            break;
                        }
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Billing

        public void OnPaymentError(string error)
        {
            Console.WriteLine(error);
        }

        public async void OnPaymentSuccess(IList<Purchase> result)
        {
            await FundingPay();
        }

        #endregion
    
        private void GetDataFunding(FundingDataObject dataObject)
        {
            try
            { 
                if (dataObject != null)
                { 
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, dataObject.Image, ImageFunding, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    Username.Text = WoWonderTools.GetNameFinal(dataObject.UserData);
                    TxtUsername.Text = WoWonderTools.GetNameFinal(dataObject.UserData);

                    bool success = int.TryParse(dataObject.Time, out var number);
                    switch (success)
                    {
                        case true:
                            Console.WriteLine("Converted '{0}' to {1}.", dataObject.Time, number);
                            TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(number, false);
                            TxtDonation.Text = Methods.Time.TimeAgo(number, false);
                            break;
                        default:
                            Console.WriteLine("Attempted conversion of '{0}' failed.", dataObject.Time ?? "<null>");
                            TxtTime.Text = Methods.Time.ReplaceTime(dataObject.Time);
                            TxtDonation.Text = dataObject.Time;
                            break;
                    }
                     
                    TxtTitle.Text = Methods.FunString.DecodeString(dataObject.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(dataObject.Description);
                    
                    TxtMore.Visibility = ViewStates.Visible;

                    try
                    {
                        dataObject.Raised = dataObject.Raised.Replace(AppSettings.CurrencyFundingPriceStatic, "");
                        dataObject.Amount = dataObject.Amount.Replace(AppSettings.CurrencyFundingPriceStatic, "");

                        decimal d = decimal.Parse(dataObject.Raised, CultureInfo.InvariantCulture);
                        TxtFundRaise.Text = GetText(Resource.String.Lbl_Collected) + " " + AppSettings.CurrencyFundingPriceStatic + d.ToString("0.00");

                        decimal amount = decimal.Parse(dataObject.Amount, CultureInfo.InvariantCulture);
                        TxtFundAmount.Text = GetText(Resource.String.Lbl_Goal) + " " + AppSettings.CurrencyFundingPriceStatic + amount.ToString("0.00");
                    }
                    catch (Exception exception)
                    {
                        TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Raised;
                        TxtFundAmount.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Amount;
                        Methods.DisplayReportResultTrack(exception);
                    }
                      
                    BtnContact.Visibility = dataObject.UserData.UserId == UserDetails.UserId ? ViewStates.Gone  : ViewStates.Visible;

                    ProgressBar.Progress = Convert.ToInt32(dataObject.Bar?.ToString("0") ?? "0");

                    if (dataObject.IsDonate != null && dataObject.IsDonate.Value == 1)
                    {
                        BtnDonate.Visibility = ViewStates.Gone;
                    }

                    switch (dataObject.RecentDonations?.Count)
                    {
                        case > 0:
                            MAdapter.UserList = new ObservableCollection<RecentDonation>(dataObject.RecentDonations);
                            MAdapter.NotifyDataSetChanged();

                            RecentDonationsLayout.Visibility = ViewStates.Visible;
                            break;
                        default:
                            RecentDonationsLayout.Visibility = ViewStates.Gone;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private async Task FundingPay()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Funding.FundingPayAsync(DataObject.Id, CodeName);
                    switch (apiStatus)
                    {
                        case 200:
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long); 
                            StartApiService();
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent?.GetStringExtra("ItemObject") ?? "");
                if (DataObject != null)
                { 
                    GetDataFunding(DataObject);
                }
                 
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFundingById });
        }
         
        private async Task GetFundingById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Funding.GetFundingByIdAsync(DataObject.Id);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case GetFundingByIdObject result:
                                    GetDataFunding(result.Data);
                                    break;
                            }

                            break;
                        }
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}