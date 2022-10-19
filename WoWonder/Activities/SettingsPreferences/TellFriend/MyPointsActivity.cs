using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.Icu.Text;
using Android.Icu.Util;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using WoWonder.Activities.Base;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.TellFriend
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyPointsActivity : BaseActivity
    {
        #region Variables Basic

        private ImageView ImageUser;
        private TextView NameUser , TxtSubTitle;
        private TextView TextAddWallet;
        private RelativeLayout AddWalletLayouts;
        private AdView MAdView;
        private TextView NickName, TodayTime;
        private RelativeLayout More;
        private TextView PercentComment, PercentNewPost, PercentReactPost, PercentNewBlog;
        

        private PointMoreBottomDiagloFragment MorePoint;

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
                SetContentView(Resource.Layout.MyPointsLayout);

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

                MAdView?.Resume();
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

                MAdView?.Pause();
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
                ImageUser = FindViewById<ImageView>(Resource.Id.imageUser);
                NameUser = FindViewById<TextView>(Resource.Id.nameUser);
                TxtSubTitle = FindViewById<TextView>(Resource.Id.subTitle);

                AddWalletLayouts = FindViewById<RelativeLayout>(Resource.Id.AddWalletLayouts); 
                TextAddWallet = FindViewById<TextView>(Resource.Id.TextAddWallet);

                NickName = FindViewById<TextView>(Resource.Id.nickName);
                TodayTime = FindViewById<TextView>(Resource.Id.CurrentDateTime);

                PercentComment = FindViewById<TextView>(Resource.Id.PercentCommentPost);
                PercentNewPost = FindViewById<TextView>(Resource.Id.PercentCreatePost);
                PercentReactPost = FindViewById<TextView>(Resource.Id.PercentReactPost);
                PercentNewBlog = FindViewById<TextView>(Resource.Id.PercentCreateBlog);

              

                More = FindViewById<RelativeLayout>(Resource.Id.more);
                More.Click += More_Click;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                var myProfile = ListUtils.MyProfileList?.FirstOrDefault();
                if (myProfile != null)
                {
                    GlideImageLoader.LoadImage(this, myProfile.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    NameUser.Text = WoWonderTools.GetNameFinal(myProfile);
                    NickName.Text = "@" + myProfile.Username;

                    // date time
                    SimpleDateFormat sdf = new SimpleDateFormat("MMMM dd, yyyy");
                    TodayTime.Text = sdf.Format(Calendar.Instance.Time);

                    //TxtSubTitle.Text = GetString(Resource.String.Btn_Points) + " : " + myProfile.Points;
                    TxtSubTitle.Text = myProfile.Points; 
                }

                var setting = ListUtils.SettingsSiteList;
                if (setting != null)
                {
                    PercentComment.Text =setting.CommentsPoint + "%";
                    PercentNewPost.Text = setting.CreatepostPoint + "%";
                    PercentNewBlog.Text = setting.CreateblogPoint + "%";

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                            PercentReactPost.Text = setting.ReactionPoint + "%";
                            break;
                        case PostButtonSystem.Wonder:
                            PercentReactPost.Text = setting.WondersPoint + "%";
                            break;
                        case PostButtonSystem.DisLike:
                            PercentReactPost.Text = setting.DislikesPoint + "%";
                            break;
                        case PostButtonSystem.Like:
                            PercentReactPost.Text = setting.LikesPoint + "%";
                            break;
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void More_Click(object sender, EventArgs e)
        {
            MorePoint = new PointMoreBottomDiagloFragment();
            MorePoint.Show(SupportFragmentManager, "MorePoint");
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_MyPoints);
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
                        AddWalletLayouts.Click += AddWalletLayoutsOnClick;
                        break;
                    default:
                        AddWalletLayouts.Click -= AddWalletLayoutsOnClick;
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
                MAdView?.Destroy();

                ImageUser = null!;
                NameUser = null!;
                TxtSubTitle = null!;
                AddWalletLayouts = null!;
                TextAddWallet = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void AddWalletLayoutsOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this,typeof(TabbedWalletActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

    }
}