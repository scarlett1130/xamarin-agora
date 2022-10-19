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
using WoWonderClient.Classes.Group;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateGroupActivity : BaseActivity
    {
        #region Variables Basic

        private PublisherAdView PublisherAdView;

        private TextView TvStep, TvStepTitle;
        private View ViewStep1, ViewStep2, ViewStep3, ViewStep4, ViewStep5;
        private EditText EtStep1, EtStep3;
        private RadioGroup RgStep5;
        private FlexboxLayout RgStep4;
        private AppCompatButton BtnNext;
        private int NStep;
        private string GroupTitle, GroupUsername, GroupAbout, Category, Privacy, CategoryId;
        private List<string> ArrayAdapter;
        private AppCompatButton BtnPrev;

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
                SetContentView(Resource.Layout.create_group_layout);

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
                //
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                ViewStep1 = FindViewById<View>(Resource.Id.view_step1);
                ViewStep2 = FindViewById<View>(Resource.Id.view_step2);
                ViewStep3 = FindViewById<View>(Resource.Id.view_step3);
                ViewStep4 = FindViewById<View>(Resource.Id.view_step4);
                ViewStep5 = FindViewById<View>(Resource.Id.view_step5);
                EtStep1 = FindViewById<EditText>(Resource.Id.et_step12);
                EtStep3 = FindViewById<EditText>(Resource.Id.et_step3);
                RgStep4 = FindViewById<FlexboxLayout>(Resource.Id.rg_step4);
                RgStep5 = FindViewById<RadioGroup>(Resource.Id.rg_step5);
                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                Methods.SetColorEditText(EtStep1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EtStep3, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                // Create category buttons
                CreateCategoryButtons();

                //
                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                //
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
                int count = CategoriesController.ListCategoriesGroup.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Group");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesGroup)
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
                TvStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/5";

                switch (NStep)
                {
                    case 1:
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetGroupTitle);
                        ViewStep1.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 2:
                        EtStep1.Hint = GetString(Resource.String.Lbl_GroupUsername);
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetGroupUserName);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 3:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Visible;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        HideKeyboard();
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Describe_Group);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 4:
                        HideKeyboard();
                        ArrayAdapter = CategoriesController.ListCategoriesGroup.Select(item => item.CategoriesName).ToList();
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Visible;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#4E586E"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#C6CBC7"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 5:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Visible;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectPrivacy);
                        ViewStep1.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep2.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep3.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep4.Background.SetTint(Color.ParseColor("#00E711"));
                        ViewStep5.Background.SetTint(Color.ParseColor("#4E586E"));
                        BtnNext.Text = GetString(Resource.String.Lbl_Save);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Group);
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
                        GroupTitle = EtStep1.Text;
                        if (GroupTitle.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep1.Text = "";
                        }
                        break;
                    case 2:
                        GroupUsername = EtStep1.Text;
                        if (GroupUsername.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 3:
                        GroupAbout = EtStep3.Text;
                        if (GroupAbout.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 4:
                        if (Category.Length > 0)
                        {
                            CategoryId = CategoriesController.ListCategoriesGroup.FirstOrDefault(categories => categories.CategoriesName == Category)?.CategoriesId;

                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 5:
                        if (RgStep5.CheckedRadioButtonId > 0)
                        {
                            var rb1 = FindViewById<RadioButton>(RgStep5.CheckedRadioButtonId);

                            if (rb1.Text.Equals("Public"))
                                Privacy = "1";
                            else
                                Privacy = "2";

                            // Create Group
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
                ViewStep5 = null!;
                EtStep1 = null!;
                EtStep3 = null!;
                RgStep4 = null!;
                RgStep5 = null!;
                BtnNext = null!;

                GroupTitle = "";
                GroupUsername = "";
                GroupAbout = "";
                Category = "";
                Privacy = "";
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
                AndHUD.Shared.Show(this, GetString(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Group.CreateGroupAsync(GroupUsername.Replace(" ", ""), GroupTitle, GroupAbout, CategoryId, Privacy);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case CreateGroupObject result:
                                    {
                                        AndHUD.Shared.Dismiss(this);
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                        Intent returnIntent = new Intent();
                                        if (result.GroupData != null)
                                            returnIntent?.PutExtra("groupItem", JsonConvert.SerializeObject(result.GroupData));
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
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

    }
}