using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Flexbox;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Page;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreatePageActivity : BaseActivity
    {
        #region Variables Basic

        private PublisherAdView PublisherAdView;

        private TextView TvStep, TvStepTitle;
        private View ViewStep1, ViewStep2, ViewStep3, ViewStep4;
        private EditText EtStep1, EtStep4;
        private FlexboxLayout RgStep3;
        private AppCompatButton BtnNext, BtnPrev;
        private int NStep;
        private string PageTitle, PageUsername, PageAbout, Category, CategoryId;
        private List<string> ArrayAdapter;

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
                SetContentView(Resource.Layout.create_page_layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                //
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                ViewStep1 = FindViewById<View>(Resource.Id.view_step1);
                ViewStep2 = FindViewById<View>(Resource.Id.view_step2);
                ViewStep3 = FindViewById<View>(Resource.Id.view_step3);
                ViewStep4 = FindViewById<View>(Resource.Id.view_step4);

                EtStep1 = FindViewById<EditText>(Resource.Id.et_step12);
                EtStep4 = FindViewById<EditText>(Resource.Id.et_step4);
                RgStep3 = FindViewById<FlexboxLayout>(Resource.Id.rg_step3);
                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                Methods.SetColorEditText(EtStep1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EtStep4, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

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
                int count = CategoriesController.ListCategoriesPage.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Page");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesPage)
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
                    RgStep3.AddView(button);
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
                TvStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/4";

                switch (NStep)
                {
                    case 1:
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetPageTitle);
                        ViewStep1.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 2:
                        EtStep1.Hint = GetString(Resource.String.Lbl_PageUsername);
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetPageUserName);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 4:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep4.Visibility = ViewStates.Visible;
                        RgStep3.Visibility = ViewStates.Gone;
                        PublisherAdView.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Describe_Page);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#4E586E"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Save);
                        break;
                    case 3:
                        HideKeyboard();
                        ArrayAdapter = CategoriesController.ListCategoriesPage.Select(item => item.CategoriesName).ToList();
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Visible;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                }
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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Page);
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
                        BtnNext.Click += BtnNext_Click;
                        break;
                    default:
                        BtnNext.Click -= BtnNext_Click;
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

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                switch (NStep)
                {
                    case 1:
                        PageTitle = EtStep1.Text;
                        if (PageTitle.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep1.Text = "";
                        }
                        break;
                    case 2:
                        PageUsername = EtStep1.Text;
                        if (PageUsername.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 4:
                        PageAbout = EtStep4.Text;
                        if (PageAbout.Length > 0)
                        {
                            OnSave();
                        }
                        break;
                    case 3:
                        if (Category.Length > 0)
                        {
                            CategoryId = CategoriesController.ListCategoriesPage.FirstOrDefault(categories => categories.CategoriesName == Category)?.CategoriesId;

                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                PublisherAdView?.Destroy();

                TvStep = null!;
                TvStepTitle = null!;
                ViewStep1 = null!;
                ViewStep2 = null!;
                ViewStep3 = null!;
                ViewStep4 = null!;
                EtStep1 = null!;
                EtStep4 = null!;
                RgStep3 = null!;
                BtnNext = null!;

                PageTitle = "";
                PageUsername = "";
                PageAbout = "";
                Category = "";
                NStep = 1;

                PublisherAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private async void OnSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }
                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Page.CreatePageAsync(PageUsername.Replace(" ", ""), PageTitle, CategoryId, PageAbout);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case CreatePageObject result:
                                    {
                                        AndHUD.Shared.Dismiss(this);
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                        Intent returnIntent = new Intent();
                                        if (result.PageData != null)
                                            returnIntent?.PutExtra("pageItem", JsonConvert.SerializeObject(result.PageData));
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
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        #endregion
    }
}