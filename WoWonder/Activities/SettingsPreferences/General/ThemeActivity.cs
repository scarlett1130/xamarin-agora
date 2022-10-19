using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using System;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ThemeActivity : BaseActivity
    {
        private TextView rbLight, rbDark, rbBattery;
        private string currentThemeMode;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Theme_Layout);

                currentThemeMode = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);

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

        private void InitComponent()
        {
            try
            {
                rbLight = FindViewById<TextView>(Resource.Id.rbLight);
                rbDark = FindViewById<TextView>(Resource.Id.rbDark);
                rbBattery = FindViewById<TextView>(Resource.Id.rbBatterySaver);

                rbLight.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#4F6C8D"));
                rbDark.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#4F6C8D"));
                rbBattery.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#4F6C8D"));

                rbBattery.Visibility = ViewStates.Gone;

                switch ((int)Build.VERSION.SdkInt)
                {
                    case >= 29:
                        rbBattery.Visibility = ViewStates.Visible;
                        break;
                }

                if (currentThemeMode == MainSettings.LightMode)
                {
                    rbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    rbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    rbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
                else if (currentThemeMode == MainSettings.DarkMode)
                {
                    rbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    rbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    rbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
                else
                {
                    rbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    rbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    rbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
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
                    toolBar.Title = GetString(Resource.String.Lbl_Theme);
                    //toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
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
                        rbLight.Click += SetLightModeClick;
                        rbDark.Click += SetDarkModeClick;
                        rbBattery.Click += SetBateryModeClick;
                        break;
                    default:
                        rbLight.Click -= SetLightModeClick;
                        rbDark.Click -= SetDarkModeClick;
                        rbBattery.Click -= SetBateryModeClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetBateryModeClick(object sender, EventArgs e)
        {
            if (currentThemeMode != MainSettings.DefaultMode)
            {
                MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

                switch ((int)Build.VERSION.SdkInt)
                {
                    case >= 29:
                        {
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                            var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                            switch (currentNightMode)
                            {
                                case UiMode.NightNo:
                                    // Night mode is not active, we're using the light theme
                                    MainSettings.ApplyTheme(MainSettings.LightMode);
                                    break;
                                case UiMode.NightYes:
                                    // Night mode is active, we're using dark theme
                                    MainSettings.ApplyTheme(MainSettings.DarkMode);
                                    break;
                            }

                            break;
                        }
                    default:
                        {
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                            var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                            switch (currentNightMode)
                            {
                                case UiMode.NightNo:
                                    // Night mode is not active, we're using the light theme
                                    MainSettings.ApplyTheme(MainSettings.LightMode);
                                    break;
                                case UiMode.NightYes:
                                    // Night mode is active, we're using dark theme
                                    MainSettings.ApplyTheme(MainSettings.DarkMode);
                                    break;
                            }

                            switch (Build.VERSION.SdkInt)
                            {
                                case >= BuildVersionCodes.Lollipop:
                                    Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                    Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                                    break;
                            }

                            Intent intent = new Intent(this, typeof(TabbedMainActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            intent.AddFlags(ActivityFlags.NoAnimation);
                            FinishAffinity();
                            OverridePendingTransition(0, 0);
                            StartActivity(intent);
                            break;
                        }
                }
            }
        }

        private void SetDarkModeClick(object sender, EventArgs e)
        {
            if (currentThemeMode != MainSettings.DarkMode)
            {
                MainSettings.ApplyTheme(MainSettings.DarkMode);
                MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.Lollipop:
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        break;
                }

                Intent intent = new Intent(this, typeof(TabbedMainActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.NoAnimation);
                FinishAffinity();
                OverridePendingTransition(0, 0);
                StartActivity(intent);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                rbLight = null!;
                rbDark = null!;
                rbBattery = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetLightModeClick(object sender, EventArgs e)
        {
            if (currentThemeMode != MainSettings.LightMode)
            {
                //Set Light Mode   
                MainSettings.ApplyTheme(MainSettings.LightMode);
                MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.Lollipop:
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        break;
                }

                Intent intent = new Intent(this, typeof(TabbedMainActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.NoAnimation);
                FinishAffinity();
                OverridePendingTransition(0, 0);
                StartActivity(intent);
            }
        }

    }
}