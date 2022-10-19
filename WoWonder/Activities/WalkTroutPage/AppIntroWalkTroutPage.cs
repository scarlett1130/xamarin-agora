using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using WoWonder.Activities.Base;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Services;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.WalkTroutPage
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AppIntroWalkTroutPage : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    { 
        #region Variables Basic

        private string Caller = "";
        private ViewPager2 VpIntro;
        private TabLayout TabLayout;
        private static readonly int MaxPages = 4;
        private MainTabAdapter PagerAdapter;
        private TextView TvSkip, TvNext;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this, true);
                 
                SetContentView(Resource.Layout.walkthrough);

                Caller = Intent?.GetStringExtra("class") ?? "";

                //Get Value And Set Toolbar
                InitComponent();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => PostUpdaterHelper.FetchFirstNewsFeedApiPosts(true), ApiRequest.LoadSuggestedUser, ApiRequest.LoadSuggestedGroup, ApiRequest.LoadSuggestedPage, ApiRequest.GetMyGroups, ApiRequest.GetMyPages });
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
           
        #region Functions

        private void InitComponent()
        {
            try
            {
                VpIntro = FindViewById<ViewPager2>(Resource.Id.vp2Intro);
                TvSkip = FindViewById<TextView>(Resource.Id.tv_onboard_skip);
                TvNext = FindViewById<TextView>(Resource.Id.tv_onboard_next);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tab_layout);

                PagerAdapter = new MainTabAdapter(this);
                PagerAdapter.AddFragment(new AnimFragment1() , "0"); //loction 
                PagerAdapter.AddFragment(new AnimFragment2() , "1"); //contact
                PagerAdapter.AddFragment(new AnimFragment4() , "2"); //recurd 
                PagerAdapter.AddFragment(new AnimFragment3(), "3"); //camrea
                VpIntro.Adapter = PagerAdapter;
                VpIntro.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                new TabLayoutMediator(TabLayout, VpIntro, this).Attach();
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
                        TvSkip.Click += TvSkip_Click;
                        TvNext.Click += TvNext_Click;
                        break;
                    default:
                        TvSkip.Click -= TvSkip_Click;
                        TvNext.Click -= TvNext_Click;
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
        }

         
        #endregion

        #region Event

        private void TvNext_Click(object sender, EventArgs e)
        {
            try
            {
                int current = VpIntro.CurrentItem + 1;
                if(current < MaxPages)
                {
                    VpIntro.SetCurrentItem(current, false);
                }
                else
                {
                    DonePressed();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TvSkip_Click(object sender, EventArgs e)
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    switch (ListUtils.SettingsSiteList?.MembershipSystem)
                    {
                        case "1":
                            {
                                var intent = new Intent(this, typeof(GoProActivity));
                                intent.PutExtra("class", "register");
                                StartActivity(intent);
                                break;
                            }
                        default:
                            { 
                                switch (AppSettings.AddAllInfoPorfileAfterRegister)
                                {
                                    case true:
                                        {
                                            Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                            StartActivity(newIntent);
                                            break;
                                        }
                                    default:
                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                        break;
                                }

                                break;
                            }
                    }
                }
                else if (Caller.Contains("free"))
                {
                    var intent = new Intent(this, typeof(MyProfileActivity));
                    intent.PutExtra("class", "free");
                    StartActivity(intent);
                }
                else
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }

                Finish();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
         
        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted && PermissionsController.CheckPermissionStorage():
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 108:
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

        #region Functions

        private void Pressed(int page)
        {
            try
            {
                if (page == 0) //Location
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                    {
                        new PermissionsController(this).RequestPermission(105);
                    } 
                }
                else if (page == 1) //Contacts
                {
                    if ((int)Build.VERSION.SdkInt > 23 && AppSettings.InvitationSystem)
                    {
                        new PermissionsController(this).RequestPermission(101);
                    } 
                }
                else if (page == 2) // Record
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                    {
                        new PermissionsController(this).RequestPermission(102);
                    } 
                }
                else if (page == 3) // Storage & Camera
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(108);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Do something when users tap on Done button.
        private void DonePressed()
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    switch (ListUtils.SettingsSiteList?.MembershipSystem)
                    {
                        case "1":
                            {
                                var intent = new Intent(this, typeof(GoProActivity));
                                intent.PutExtra("class", "register");
                                StartActivity(intent);
                                break;
                            }
                        default:
                            {
                                switch (AppSettings.AddAllInfoPorfileAfterRegister)
                                {
                                    case true:
                                        {
                                            Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                            StartActivity(newIntent);
                                            break;
                                        }
                                    default:
                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                        break;
                                }

                                break;
                            }
                    }
                }
                else if (Caller.Contains("free"))
                {
                    var intent = new Intent(this, typeof(MyProfileActivity));
                    intent.PutExtra("class", "free");
                    StartActivity(intent);
                }
                else
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly AppIntroWalkTroutPage Activity;

            public MyOnPageChangeCallback(AppIntroWalkTroutPage activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            //public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            //{
            //    try
            //    {
            //        base.OnPageScrolled(position, positionOffset, positionOffsetPixels);
            //        Activity.Pressed(position);

            //        switch (position)
            //        {
            //            case 3:
            //                Activity.TvNext.Text = Activity.GetText(Resource.String.Lbl_Done);
            //                break;
            //            default:
            //                Activity.TvNext.Text = Activity.GetText(Resource.String.Lbl_Next);
            //                break;
            //        }
            //    }
            //    catch (Exception exception)
            //    {
            //        Methods.DisplayReportResultTrack(exception);
            //    }
            //}

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    Activity.Pressed(position);

                    switch (position)
                    {
                        case 3:
                            Activity.TvNext.Text = Activity.GetText(Resource.String.Lbl_Done);
                            break;
                        default:
                            Activity.TvNext.Text = Activity.GetText(Resource.String.Lbl_Next);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
             
        }
    }
}