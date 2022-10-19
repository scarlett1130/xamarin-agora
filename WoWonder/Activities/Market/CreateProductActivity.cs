using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using TheArtOfDev.Edmodo.Cropper;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using Google.Android.Flexbox;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateProductActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView TvStep, TvStepTitle;
        private EditText EtStep1, EtStep2, EtStep5;
        private FlexboxLayout RgStep4;
        private TextView TvStep3, TvStep6, TvStep8;
        private RelativeLayout RlStep7;
        private RecyclerView MRecycler;
        private PublisherAdView PublisherAdView;
        private AttachmentsAdapter MAdapter;
        private View ViewStep1, ViewStep2, ViewStep3, ViewStep4, ViewStep5, ViewStep6, ViewStep7, ViewStep8;
        private AppCompatButton BtnNext, BtnPrev;
        private ImageView IvSelectPhoto;

        private int NStep;
        private List<string> ArrayAdapter;
        private string ProductName, ProductPrice, ProductCurrency, ProductCategory, ProductDescription;
        private string ProductType, Category;
        private string TypeDialog, PlaceText;

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
                SetContentView(Resource.Layout.create_product_layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);
                EtStep1 = FindViewById<EditText>(Resource.Id.et_step1);
                EtStep2 = FindViewById<EditText>(Resource.Id.et_step2);
                EtStep5 = FindViewById<EditText>(Resource.Id.et_step5);
                TvStep3 = FindViewById<TextView>(Resource.Id.tv_step3);
                TvStep6 = FindViewById<TextView>(Resource.Id.tv_step6);
                RgStep4 = FindViewById<FlexboxLayout>(Resource.Id.rg_step4);
                RlStep7 = FindViewById<RelativeLayout>(Resource.Id.rl_step7);
                TvStep8 = FindViewById<TextView>(Resource.Id.tv_step8);
                ViewStep1 = FindViewById<View>(Resource.Id.view_step1);
                ViewStep2 = FindViewById<View>(Resource.Id.view_step2);
                ViewStep3 = FindViewById<View>(Resource.Id.view_step3);
                ViewStep4 = FindViewById<View>(Resource.Id.view_step4);
                ViewStep5 = FindViewById<View>(Resource.Id.view_step5);
                ViewStep6 = FindViewById<View>(Resource.Id.view_step6);
                ViewStep7 = FindViewById<View>(Resource.Id.view_step7);
                ViewStep8 = FindViewById<View>(Resource.Id.view_step8);
                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);
                IvSelectPhoto = FindViewById<ImageView>(Resource.Id.btn_selectimage);

                // Create category buttons
                CreateCategoryButtons();

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                SetStepChild();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CreateCategoryButtons()
        {
            try
            {
                int count = CategoriesController.ListCategoriesProducts.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Product");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesProducts)
                {
                    AppCompatButton button = new AppCompatButton(this);
                    var ll = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    ll.SetMargins(10, 10, 10, 10);
                    button.LayoutParameters = ll;

                    button.Text = category.CategoriesName;
                    button.SetBackgroundResource(Resource.Drawable.liked_button_normal);
                    button.SetTextColor(Color.ParseColor("#b0b0b0"));
                    button.TextSize = 14;
                    button.SetAllCaps(false);
                    button.SetPadding(10, 0, 10, 0);
                    button.Click += CategoryOnClick;
                    RgStep4.AddView(button);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CategoryOnClick(object sender, EventArgs e)
        {
            if (BtnPrev != null)
            {
                BtnPrev.SetTextColor(Color.ParseColor("#b0b0b0"));
                BtnPrev.SetBackgroundResource(Resource.Drawable.liked_button_normal);
            }
            AppCompatButton BtnCurrent = sender as AppCompatButton;
            BtnCurrent.SetTextColor(Color.ParseColor("#ffffff"));
            BtnCurrent.SetBackgroundResource(Resource.Drawable.liked_button_pressed);
            Category = BtnCurrent.Text;

            BtnPrev = BtnCurrent;
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_CreateNewProduct);
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
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                MRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                /*var attach = new Attachments
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();*/
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
                        MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                        BtnNext.Click += BtnNext_Click;
                        TvStep3.Touch += TxtCurrencyOnTouch;
                        TvStep6.Touch += TxtConditionOnTouch;
                        TvStep8.Click += TxtLocationOnClick;
                        IvSelectPhoto.Click += SelectPhotoOnClick;
                        break;
                    default:
                        MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                        BtnNext.Click -= BtnNext_Click;
                        TvStep3.Touch -= TxtCurrencyOnTouch;
                        TvStep6.Touch -= TxtConditionOnTouch;
                        TvStep8.Click -= TxtLocationOnClick;
                        IvSelectPhoto.Click -= SelectPhotoOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBackPressed()
        {
            if (NStep > 1)
            {
                NStep -= 1;
                SetStepChild();
                return;
            }
            base.OnBackPressed();
        }

        private void DestroyBasic()
        {
            try
            {
                PublisherAdView?.Destroy();

                EtStep1 = null!;
                EtStep2 = null!;
                EtStep5 = null!;
                TvStep3 = null!;
                TvStep6 = null!;
                RgStep4 = null!;
                RlStep7 = null!;
                TvStep = null!;
                TvStepTitle = null!;
                ViewStep1 = null!;
                ViewStep2 = null!;
                ViewStep3 = null!;
                ViewStep4 = null!;
                ViewStep5 = null!;
                ViewStep6 = null!;
                ViewStep7 = null!;
                ViewStep8 = null!;
                MAdapter = null!;
                MRecycler = null!;
                PublisherAdView = null!;
                ArrayAdapter = null!;
                IvSelectPhoto = null!;
                TvStep8 = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetStepChild()
        {
            try
            {
                TvStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/8";

                switch (NStep)
                {
                    case 1:
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_name);
                        ViewStep1.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 2:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Visible;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_price);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 3:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Visible;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        HideKeyboard();
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Currency);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 4:
                        ArrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Visible;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 5:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Visible;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_description);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 6:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Visible;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Condition);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 7:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Visible;
                        MRecycler.Visibility = ViewStates.Visible;
                        TvStep8.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Image);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 8:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep2.Visibility = ViewStates.Gone;
                        TvStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        EtStep5.Visibility = ViewStates.Gone;
                        TvStep6.Visibility = ViewStates.Gone;
                        RlStep7.Visibility = ViewStates.Gone;
                        MRecycler.Visibility = ViewStates.Gone;
                        TvStep8.Visibility = ViewStates.Visible;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Image);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep6.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep7.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep8.Background.SetTint(Color.ParseColor("#4E586E"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Save);
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

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            if (item != null)
                            {
                                MAdapter.Remove(item);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtConditionOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                arrayAdapter.Add(GetText(Resource.String.Radio_New));
                arrayAdapter.Add(GetText(Resource.String.Radio_Used));

                TypeDialog = "Condition";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                dialogList.Title(GetText(Resource.String.Lbl_SelectCurrency)).TitleColorRes(Resource.Color.primary);
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

        private void TxtCurrencyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                switch (arrayAdapter?.Count)
                {
                    case > 0:
                        {
                            TypeDialog = "Currency";

                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            dialogList.Title(GetText(Resource.String.Lbl_SelectCurrency)).TitleColorRes(Resource.Color.primary);
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Currency Products");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                switch (CategoriesController.ListCategoriesProducts.Count)
                {
                    case > 0:
                        {
                            TypeDialog = "Categories";

                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                            dialogList.Title(GetText(Resource.String.Lbl_SelectCategories)).TitleColorRes(Resource.Color.primary);
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Products");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnFocusChange()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    default:
                        {
                            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Location when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void OnSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                    switch (list.Count)
                    {
                        case 0:
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                            break;
                        default:
                            {
                                //Show a progress
                                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                                var (apiStatus, respond) = await RequestsAsync.Market.CreateProductAsync(ProductName, ProductDescription, PlaceText, ProductPrice,
                                    ProductCurrency, ProductCategory, ProductType, list);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case CreateProductObject result:
                                                    {
                                                        AndHUD.Shared.Dismiss(this);
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                                        var listImage = list.Select(productPathImage => new Images { Id = "", ProductId = result.ProductId.ToString(), Image = productPathImage.FileSimple, ImageOrg = productPathImage.FileSimple }).ToList();

                                                        //Add new item to my  Product list
                                                        var user = ListUtils.MyProfileList?.FirstOrDefault();
                                                        ProductDataObject data = new ProductDataObject
                                                        {
                                                            Name = ProductName,
                                                            UserId = UserDetails.UserId,
                                                            Id = result.ProductId.ToString(),
                                                            Location = PlaceText,
                                                            Description = ProductDescription,
                                                            Category = ProductCategory,
                                                            Images = new List<Images>(listImage),
                                                            Price = ProductPrice,
                                                            Type = ProductType,
                                                            Seller = user,
                                                            Currency = ProductCurrency,
                                                            PostId = result.ProductPostId.ToString(),
                                                        };

                                                        if (TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter.MarketList != null)
                                                        {
                                                            TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.MarketList?.Insert(0, new Classes.ProductClass
                                                            {
                                                                Id = Convert.ToInt64(data.Id),
                                                                Type = Classes.ItemType.MyProduct,
                                                                Product = data
                                                            });
                                                            TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.NotifyDataSetChanged();
                                                        }

                                                        Intent returnIntent = new Intent();
                                                        returnIntent?.PutExtra("product", JsonConvert.SerializeObject(data));
                                                        SetResult(Result.Ok, returnIntent);

                                                        Finish();
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayAndHudErrorResult(this, respond);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 502 when resultCode == Result.Ok:
                        GetPlaceFromPicker(data);
                        break;
                    case CropImage.CropImageActivityRequestCode:
                        {
                            var result = CropImage.GetActivityResult(data);

                            switch (resultCode)
                            {
                                case Result.Ok:
                                    {
                                        switch (result.IsSuccessful)
                                        {
                                            case true:
                                                {
                                                    var resultUri = result.Uri;

                                                    switch (string.IsNullOrEmpty(resultUri.Path))
                                                    {
                                                        case false:
                                                            {
                                                                var productPathImage = resultUri.Path;
                                                                var attach = new Attachments
                                                                {
                                                                    Id = MAdapter.AttachmentList.Count + 1,
                                                                    TypeAttachment = "images[]",
                                                                    FileSimple = productPathImage,
                                                                    FileUrl = productPathImage
                                                                };

                                                                MAdapter.Add(attach);
                                                                break;
                                                            }
                                                        default:
                                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                                            break;
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

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        OpenDialogGallery();
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
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

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Condition":
                        TvStep6.Text = itemString;
                        if (TvStep6.Text.Equals(GetText(Resource.String.Radio_New)))
                            ProductType = "0";
                        else
                            ProductType = "1";
                        break;
                    case "Currency":
                        TvStep3.Text = itemString;
                        ProductCurrency = WoWonderTools.GetIdCurrency(itemString);
                        break;
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

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                switch (string.IsNullOrEmpty(placeAddress))
                {
                    //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    case false:
                        {
                            PlaceText = string.IsNullOrEmpty(PlaceText) switch
                            {
                                false => string.Empty,
                                _ => PlaceText
                            };

                            PlaceText = placeAddress;
                            TvStep8.Text = PlaceText;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery()
        {
            try
            {
                if (!WoWonderTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_AllowedFileUpload), GetText(Resource.String.Lbl_Ok));
                    return;
                }
                
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        {
                            Methods.Path.Chack_MyFolder();

                            //Open Image 
                            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                            CropImage.Activity()
                                .SetInitialCropWindowPaddingRatio(0)
                                .SetAutoZoomEnabled(true)
                                .SetMaxZoom(4)
                                .SetGuidelines(CropImageView.Guidelines.On)
                                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                                .SetOutputUri(myUri).Start(this);
                            break;
                        }
                    default:
                        {
                            if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                            {
                                Methods.Path.Chack_MyFolder();

                                //Open Image 
                                var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                                CropImage.Activity()
                                    .SetInitialCropWindowPaddingRatio(0)
                                    .SetAutoZoomEnabled(true)
                                    .SetMaxZoom(4)
                                    .SetGuidelines(CropImageView.Guidelines.On)
                                    .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                                    .SetOutputUri(myUri).Start(this);
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
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

        private void TxtLocationOnClick(object sender, EventArgs e)
        {
            TxtLocationOnFocusChange();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                switch (NStep)
                {
                    case 1:
                        ProductName = EtStep1.Text;
                        if (ProductName.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep1.Text = "";
                        }
                        break;
                    case 2:
                        ProductPrice = EtStep2.Text;
                        if (ProductPrice.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep2.Text = "";
                        }
                        break;
                    case 3:
                        if (TvStep3.Text.Length > 0 && !TvStep3.Text.Equals(GetString(Resource.String.Lbl_Select_Product_Currency)))
                        {
                            NStep += 1;
                            SetStepChild();
                            TvStep3.Text = GetString(Resource.String.Lbl_Select_Product_Currency);
                        }
                        break;
                    case 4:
                        if (Category.Length > 0)
                        {
                            var cat = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == Category);
                            if (cat != null)
                            {
                                ProductCategory = cat.CategoriesId;

                                NStep += 1;
                                SetStepChild();
                            }
                        }
                        break;
                    case 5:
                        ProductDescription = EtStep5.Text;
                        if (ProductDescription.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep5.Text = "";
                        }
                        break;
                    case 6:
                        if (TvStep6.Text.Length > 0 && !TvStep6.Text.Equals(GetString(Resource.String.Lbl_Select_New_Used)))
                        {
                            NStep += 1;
                            SetStepChild();
                            TvStep6.Text = GetString(Resource.String.Lbl_Select_New_Used);
                        }
                        break;
                    case 7:
                        var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                        switch (list.Count)
                        {
                            case 0:
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                                break;
                            default:
                                NStep += 1;
                                SetStepChild();
                                break;
                        }
                        break;
                    case 8:
                        if (TvStep8.Text.Length > 0 && !TvStep8.Text.Equals(GetString(Resource.String.Lbl_Select_Location)))
                        {
                            OnSave();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void SelectPhotoOnClick(object sender, EventArgs e)
        {
            OpenDialogGallery(); //requestCode >> 500 => Image Gallery
        }
    }
}