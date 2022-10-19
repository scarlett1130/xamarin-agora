using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Q.Rorbin.Badgeview;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;

namespace WoWonder.Helpers.Utils
{
    public class BottomNavigationTab : Java.Lang.Object , View.IOnClickListener
    {
        private readonly TabbedMainActivity MainActivity;

        private LinearLayout HomeLayout, NotificationLayout, TrendingLayout, MoreLayout;
        private ImageView ImageHome, ImageNotification, ImageTrending, ImageMore;
        private TextView tvHome, tvNotifications, tvHot, tvFeatures;

        private readonly Color UnSelectColor = Color.White;
        private static int OpenNewsFeedTab = 1;

        public BottomNavigationTab(TabbedMainActivity activity)
        {
            try
            {
                MainActivity = activity;

                Initialize(); 
            }
            catch (Exception e)
            {
                 Methods.DisplayReportResultTrack(e);  
            }
        }

        private void Initialize()
        {
            try
            {
                HomeLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llHome);
                ImageHome = MainActivity.FindViewById<ImageView>(Resource.Id.ivHome);
                tvHome = MainActivity.FindViewById<TextView>(Resource.Id.tvHome);

                NotificationLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llNotification);
                ImageNotification = MainActivity.FindViewById<ImageView>(Resource.Id.ivNotification);
                tvNotifications = MainActivity.FindViewById<TextView>(Resource.Id.tvNotifications);

                TrendingLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llTrending);
                ImageTrending = MainActivity.FindViewById<ImageView>(Resource.Id.ivTrending);
                tvHot = MainActivity.FindViewById<TextView>(Resource.Id.tvHot);

                MoreLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llMore);
                ImageMore = MainActivity.FindViewById<ImageView>(Resource.Id.ivMore);
                tvFeatures = MainActivity.FindViewById<TextView>(Resource.Id.tvFeatures);

                HomeLayout?.SetOnClickListener(this);
                NotificationLayout?.SetOnClickListener(this);
                TrendingLayout?.SetOnClickListener(this);
                MoreLayout?.SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        public void SelectItem(int index)
        {
            try
            {
                ImageHome.SetColorFilter(UnSelectColor);
                ImageNotification.SetColorFilter(UnSelectColor);
                ImageTrending.SetColorFilter(UnSelectColor);
                ImageMore.SetColorFilter(UnSelectColor);

                tvHome.SetTextColor(UnSelectColor);
                tvNotifications.SetTextColor(UnSelectColor);
                tvHot.SetTextColor(UnSelectColor);
                tvFeatures.SetTextColor(UnSelectColor);

                switch (index)
                {
                    // Home
                    case 0:
                    {
                        ImageHome.SetColorFilter(Color.ParseColor("#E02020"));
                        tvHome.SetTextColor(Color.ParseColor("#E02020"));
                        MainActivity.ViewPager.SetCurrentItem(0, false);

                        if (WoWonderTools.CanAddPost())
                            MainActivity.FloatingActionButton.Visibility = AppSettings.ShowAddPostOnNewsFeed ? ViewStates.Visible : ViewStates.Gone;
                        else
                            MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;
                         
                        AdsGoogle.Ad_Interstitial(MainActivity);
                        break;
                    }
                    // Notification
                    case 1:
                    {
                        tvNotifications.SetTextColor(Color.ParseColor("#E02020"));
                        ImageNotification.SetColorFilter(Color.ParseColor("#E02020"));
                        MainActivity.ViewPager.SetCurrentItem(1, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_AppOpenManager(MainActivity);
                        break;
                    }
                    // Trending
                    case 2:
                    {
                        tvHot.SetTextColor(Color.ParseColor("#E02020"));
                        ImageTrending.SetColorFilter(Color.ParseColor("#E02020"));
                        MainActivity.ViewPager.SetCurrentItem(2, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                            AdsGoogle.Ad_RewardedVideo(MainActivity);

                        MainActivity.InAppReview();
                        break;
                    }
                    // More
                    case 3:
                    {
                        tvFeatures.SetTextColor(Color.ParseColor("#E02020"));
                        ImageMore.SetColorFilter(Color.ParseColor("#E02020"));
                        MainActivity.ViewPager.SetCurrentItem(3, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_RewardedInterstitial(MainActivity);
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.Get_MyProfileData_Api(MainActivity) });
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowBadge(int id, int count, bool showBadge)
        {
            try
            {
                switch (id)
                {
                    case < 0:
                        return;
                }

                switch (showBadge)
                {
                    case true:
                        switch (id)
                        {
                            // News_Feed_Tab
                            case 0:
                                ShowOrHideBadgeViewIcon(MainActivity, HomeLayout, count, true);
                                break;
                            // Notifications_Tab
                            case 1:
                                ShowOrHideBadgeViewIcon(MainActivity, NotificationLayout, count, true);
                                break;
                            // Trending_Tab
                            case 2:
                                ShowOrHideBadgeViewIcon(MainActivity, TrendingLayout, count, true);
                                break;
                            // More_Tab
                            case 3:
                                ShowOrHideBadgeViewIcon(MainActivity, MoreLayout, count, true);
                                break;
                        }

                        break;
                    default:
                        switch (id)
                        {
                            // News_Feed_Tab
                            case 0:
                                ShowOrHideBadgeViewIcon(MainActivity, HomeLayout);
                                break;
                            // Notifications_Tab
                            case 1:
                                ShowOrHideBadgeViewIcon(MainActivity, NotificationLayout);
                                break;
                            // Trending_Tab
                            case 2:
                                ShowOrHideBadgeViewIcon(MainActivity, TrendingLayout);
                                break;
                            // More_Tab
                            case 3:
                                ShowOrHideBadgeViewIcon(MainActivity, MoreLayout);
                                break;
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private QBadgeView BadgeNotification, BadgeTrending;
        private void ShowOrHideBadgeViewIcon(Activity mainActivity, LinearLayout linearLayoutImage, int count = 0, bool show = false)
        {
            try
            {
                mainActivity?.RunOnUiThread(() =>
                {
                    try
                    {
                        switch (show)
                        {
                            case true:
                            {
                                if (linearLayoutImage != null)
                                {
                                    if (linearLayoutImage.Id == NotificationLayout.Id)
                                    {
                                        BadgeNotification = new QBadgeView(mainActivity);
                                        int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                        BadgeNotification.BindTarget(linearLayoutImage);
                                        BadgeNotification.SetBadgeNumber(count);
                                        BadgeNotification.SetBadgeGravity(gravity);
                                        BadgeNotification.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                        BadgeNotification.SetGravityOffset(10, true);
                                    } 
                                    else if (linearLayoutImage.Id == TrendingLayout.Id)
                                    {
                                        BadgeTrending = new QBadgeView(mainActivity);
                                        int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                        BadgeTrending.BindTarget(linearLayoutImage);
                                        BadgeTrending.SetBadgeNumber(count);
                                        BadgeTrending.SetBadgeGravity(gravity);
                                        BadgeTrending.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                        BadgeTrending.SetGravityOffset(10, true);
                                    }   
                                }

                                break;
                            }
                            default:
                                if (linearLayoutImage?.Id == NotificationLayout.Id)
                                    BadgeNotification?.BindTarget(linearLayoutImage).Hide(true);
                                else if (linearLayoutImage?.Id == TrendingLayout.Id)
                                    BadgeTrending?.BindTarget(linearLayoutImage).Hide(true);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnClick(View v)
        {
            try
            {
                if (v.Id == HomeLayout.Id && OpenNewsFeedTab == 2)
                {
                    OpenNewsFeedTab = 1;
                    MainActivity?.NewsFeedTab?.MainRecyclerView?.ScrollToPosition(0);
                }
                else if (v.Id == HomeLayout.Id)
                {
                    OpenNewsFeedTab++;
                    SelectItem(0);
                }
                else if (v.Id == NotificationLayout?.Id)
                {
                    SelectItem(1);

                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;


                    ShowBadge(1, 0, false);

                }
                else if (v.Id == TrendingLayout?.Id && AppSettings.ShowTrendingPage)
                {
                    SelectItem(2);
                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;


                    ShowBadge(2, 0, false);

                }
                else if (v.Id == MoreLayout?.Id)
                {
                    SelectItem(3);

                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}