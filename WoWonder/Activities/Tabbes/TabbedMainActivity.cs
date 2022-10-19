using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.CardView.Widget;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Google.Android.Play.Core.Install.Model;
using Google.Android.Material.FloatingActionButton;
using TheArtOfDev.Edmodo.Cropper;
using MeoNavLib.Com; 
using Newtonsoft.Json;
using Plugin.Geolocator;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Advertise;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Events;
using WoWonder.Activities.Live.Page;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.Market;
using WoWonder.Activities.NativePost;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.NativePost.Services;
using WoWonder.Activities.ReelsVideo;
using WoWonder.Activities.Search;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Story;
using WoWonder.Activities.Story.Adapters; 
using WoWonder.Activities.Tabbes.Fragment;
using WoWonder.Activities.Videos;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils; 
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;
using LayoutDirection = Android.Views.LayoutDirection;
using Task = System.Threading.Tasks.Task;
using Uri = Android.Net.Uri; 
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.Notification;
using WoWonder.Activities.SettingsPreferences.Support;
using WoWonder.Activities.PopularPosts;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.Upgrade;

namespace WoWonder.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme" , Exported = false, WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedMainActivity : AppCompatActivity, MaterialDialog.IListCallback, View.IOnClickListener
    {
        #region Variables Basic

        private static TabbedMainActivity Instance; 
        private CustomNavigationController FragmentBottomNavigator;
        public ViewPager2 ViewPager;
        private MainTabAdapter TabAdapter;
        public NewsFeedNative NewsFeedTab;
        public NotificationsFragment NotificationsTab;
        public TrendingFragment TrendingTab;
        private MoreFragment MoreTab;
        private MeowBottomNavigation MeoNavigationTabBar;
        private CardView DefaultNavigationTabBar;
        private BottomNavigationTab BottomNavigationTab;

        public FloatingActionButton FloatingActionButton;
        private string ImageType = "";
        private static string CountNotificationsStatic = "0", CountMessagesStatic = "0", CountFriendStatic = "0";
        private static bool RecentlyBackPressed; 
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        
        private PowerManager.WakeLock Wl;

        private RelativeLayout RlFilter;
        private RelativeLayout RlSearch, RlMessage, RlReels, RlAdd, RlMore;

        private MaterialDialog mDialog;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            { 
                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                 
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                //AddFlagsWakeLock();
                 
                Methods.App.FullScreenApp(this);
                 
                // Create your application here
                SetContentView(Resource.Layout.Tabbed_Main_Layout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();  
                AddFragmentsTabs();

                Task.Factory.StartNew(GetGeneralAppData);
                SetService();
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
                NewsFeedTab?.StartHandler();  
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
                NewsFeedTab?.RemoveHandler();
                NewsFeedTab?.MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                NewsFeedTab?.MainRecyclerView?.StopVideo();
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
                Glide.Get(this).TrimMemory((int)level); 
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
                GC.Collect();
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
                NewsFeedTab?.MainRecyclerView?.ReleasePlayer();
                NewsFeedTab?.MainRecyclerView?.ApiPostAsync?.InsertTheLatestPosts();

                NewsFeedTab?.RemoveHandler();

                OffWakeLock();
                base.OnDestroy(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
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

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                {
                    MeoNavigationTabBar?.SetBackgroundBottomColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);
                    MeoNavigationTabBar?.SetCircleColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);
                }   
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
                MeoNavigationTabBar = FindViewById<MeowBottomNavigation>(Resource.Id.ntb_horizontal);
                DefaultNavigationTabBar = FindViewById<CardView>(Resource.Id.bottomnavigationtab);

                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                {
                    MeoNavigationTabBar.Visibility = ViewStates.Visible;
                    DefaultNavigationTabBar.Visibility = ViewStates.Gone;

                    FragmentBottomNavigator = new CustomNavigationController(this, MeoNavigationTabBar);
                     
                    MeoNavigationTabBar.LayoutDirection = AppSettings.FlowDirectionRightToLeft switch
                    {
                        true => LayoutDirection.Rtl,
                        _ => MeoNavigationTabBar.LayoutDirection
                    };
                }
                else
                {
                    MeoNavigationTabBar.Visibility = ViewStates.Gone;
                    DefaultNavigationTabBar.Visibility = ViewStates.Visible;

                    BottomNavigationTab = new BottomNavigationTab(this);
                }

                ViewPager = FindViewById<ViewPager2>(Resource.Id.vp_horizontal_ntb);
                TabAdapter = new MainTabAdapter(this);
                 
                RlSearch = FindViewById<RelativeLayout>(Resource.Id.rlSearch);
                RlMessage = FindViewById<RelativeLayout>(Resource.Id.rl_message);
                RlFilter = FindViewById<RelativeLayout>(Resource.Id.rlFilter);
                RlReels = FindViewById<RelativeLayout>(Resource.Id.rlReels);
                RlAdd = FindViewById<RelativeLayout>(Resource.Id.rlAdd);
                RlMore = FindViewById<RelativeLayout>(Resource.Id.rlMore);

                FloatingActionButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                if (WoWonderTools.CanAddPost())
                    FloatingActionButton.Visibility = AppSettings.ShowAddPostOnNewsFeed ? ViewStates.Visible : ViewStates.Gone;
                else
                    FloatingActionButton.Visibility = ViewStates.Gone;
                
                RlAdd.Visibility = AppSettings.ShowBottomAddOnTab ? ViewStates.Visible : ViewStates.Gone;
                RlFilter.Visibility = AppSettings.ShowFilterPost ? ViewStates.Visible : ViewStates.Gone;

                //RlMessage.Visibility = AppSettings.MessengerIntegration ? ViewStates.Visible : ViewStates.Gone;
                RlMessage.Visibility = ViewStates.Gone;
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
                        FloatingActionButton.Click += Btn_AddPost_OnClick;
                        RlSearch.Click += RlSearchOnClick;
                        RlFilter.Click += RlFilterOnClick;
                        RlMessage.Click += RlMessageOnClick;
                        RlAdd.Click += BtnAddOnClick;
                        RlReels.Click += RlReelsOnClick;
                        RlMore.Click += RlMoreOnClick;
                        break;
                    default: 
                        FloatingActionButton.Click -= Btn_AddPost_OnClick;
                        RlSearch.Click -= RlSearchOnClick;
                        RlFilter.Click -= RlFilterOnClick;
                        RlMessage.Click -= RlMessageOnClick;
                        RlAdd.Click -= BtnAddOnClick;
                        RlReels.Click -= RlReelsOnClick;
                        RlMore.Click -= RlMoreOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RlReelsOnClick(object sender, EventArgs e)
        {
            try
            { 
                var intent = new Intent(this, typeof(ReelsVideoDetailsActivity));
                intent.PutExtra("Type", "VideoReels");
                intent.PutExtra("VideosCount", ListUtils.VideoReelsList.Count);
                //intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoReelsList.ToList()));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public static TabbedMainActivity GetInstance()
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

        //Add 
        private void BtnAddOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (AppSettings.ShowAdvertise) arrayAdapter.Add(GetText(Resource.String.Lbl_Create_Ad));
                if (AppSettings.ShowEvents) arrayAdapter.Add(GetText(Resource.String.Lbl_Create_Events));
                if (AppSettings.ShowMarket) arrayAdapter.Add(GetText(Resource.String.Lbl_CreateNewProduct));
                if (AppSettings.ShowCommunitiesPages) arrayAdapter.Add(GetText(Resource.String.Lbl_Create_New_Page));
                if (AppSettings.ShowCommunitiesGroups) arrayAdapter.Add(GetText(Resource.String.Lbl_Create_New_Group));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void RlSearchOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void RlMessageOnClick(object sender, EventArgs e)
        {
            try
            {
                Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChatApp");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void RlFilterOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterPostDialogFragment dialogFragment = new FilterPostDialogFragment();
                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        //Event Open page add post
        private void Btn_AddPost_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "Normal");
                intent.PutExtra("PostId", UserDetails.UserId);
                //intent.PutExtra("itemObject", JsonConvert.SerializeObject(PageData));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RlMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var viewGroup = FindViewById<ViewGroup>(Android.Resource.Id.Content);
                var dlgView = LayoutInflater.From(this).Inflate(Resource.Layout.DialogMenu, viewGroup, false);

                var tvBrowserUsers = dlgView.FindViewById<TextView>(Resource.Id.tv_browser_users);
                var tvDonate = dlgView.FindViewById<TextView>(Resource.Id.tv_donate);
                var tvPatriotTV = dlgView.FindViewById<TextView>(Resource.Id.tv_patriot_tv);
                var tvGroups = dlgView.FindViewById<TextView>(Resource.Id.tv_groups);
                var tvEvents = dlgView.FindViewById<TextView>(Resource.Id.tv_events);
                var tvGeneralAccount = dlgView.FindViewById<TextView>(Resource.Id.tv_general_account);
                var tvPrivacy = dlgView.FindViewById<TextView>(Resource.Id.tv_privacy);
                var tvNotifications = dlgView.FindViewById<TextView>(Resource.Id.tv_notifications);
                var tvHelp = dlgView.FindViewById<TextView>(Resource.Id.tv_help);
                var tvMyProfile = dlgView.FindViewById<TextView>(Resource.Id.tv_my_profile);
                var tvPopularPosts = dlgView.FindViewById<TextView>(Resource.Id.tv_popular_posts);
                var tvPages = dlgView.FindViewById<TextView>(Resource.Id.tv_pages);
                var tvLiveStreaming = dlgView.FindViewById<TextView>(Resource.Id.tv_live_streaming);
                var tvUpgradeNow = dlgView.FindViewById<TextView>(Resource.Id.tv_upgrade_now);

                tvBrowserUsers.SetOnClickListener(this);
                tvDonate.SetOnClickListener(this);
                tvPatriotTV.SetOnClickListener(this);
                tvGroups.SetOnClickListener(this);
                tvEvents.SetOnClickListener(this);
                tvGeneralAccount.SetOnClickListener(this);
                tvPrivacy.SetOnClickListener(this);
                tvNotifications.SetOnClickListener(this);
                tvHelp.SetOnClickListener(this);

                tvMyProfile.SetOnClickListener(this);
                tvPopularPosts.SetOnClickListener(this);
                tvPages.SetOnClickListener(this);
                tvLiveStreaming.SetOnClickListener(this);
                tvUpgradeNow.SetOnClickListener(this);

                dialogList.CustomView(dlgView, false);
                //dialogList.Items(arrayAdapter);
                //dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                mDialog = dialogList.ItemsCallback(this).Build();
                mDialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                Intent intent = null;
                switch (v.Id)
                {
                    case Resource.Id.tv_my_profile:
                        intent = new Intent(this, typeof(MyProfileActivity));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_popular_posts:
                        intent = new Intent(this, typeof(PopularPostsActivity));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_pages:
                        intent = new Intent(this, typeof(PagesActivity));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_live_streaming:
                        try
                        {
                            new LiveUtil(this).GoLiveOnClick();
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                        break;
                    case Resource.Id.tv_upgrade_now:
                        intent = new Intent(this, typeof(GoProActivity));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_browser_users:
                        intent = new Intent(this, typeof(BrowserUserActivity));
                        //intent.PutExtra("Key", "Random");
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_donate:
                        intent = new Intent(this, typeof(LocalWebViewActivity));
                        intent.PutExtra("URL", InitializeWoWonder.WebsiteUrl + "/site-pages/donations");
                        intent.PutExtra("Type", this.GetString(Resource.String.Lbl_Donate));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_patriot_tv:
                        intent = new Intent(this, typeof(LocalWebViewActivity));
                        intent.PutExtra("URL", "https://www.americafirstpatriot.org/patriot-tv");
                        intent.PutExtra("Type", this.GetString(Resource.String.Lbl_PatriotTV));
                        StartActivity(intent);
                        break;
                    case Resource.Id.tv_groups:
                        StartActivity(new Intent(this, typeof(GroupsActivity)));
                        break;
                    case Resource.Id.tv_events:
                        StartActivity(new Intent(this, typeof(EventMainActivity)));
                        break;
                    case Resource.Id.tv_general_account:
                        StartActivity(new Intent(this, typeof(GeneralAccountActivity)));
                        break;
                    case Resource.Id.tv_privacy:
                        StartActivity(new Intent(this, typeof(PrivacyActivity)));
                        break;
                    case Resource.Id.tv_notifications:
                        StartActivity(new Intent(this, typeof(MessegeNotificationActivity)));
                        break;
                    case Resource.Id.tv_help:
                        StartActivity(new Intent(this, typeof(SupportActivity)));
                        break;
                }
                if (mDialog != null)
                {
                    Console.WriteLine("===== mDialog is not null");
                    mDialog.Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    // Add image story
                    case 500 when resultCode == Result.Ok:
                        {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                        break;
                    }
                    // Add video story
                    case 501 when resultCode == Result.Ok:
                        {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            switch (type)
                            {
                                case "Video":
                                {
                                    var fileName = filepath.Split('/').Last();
                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                    var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                    var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                    switch (videoPlaceHolderImage)
                                    {
                                        case "File Dont Exists":
                                        {
                                            var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                            break;
                                        }
                                    }

                                    if (AppSettings.EnableVideoEditor)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", fullPathFile.Path);
                                        intent.PutExtra("Type", "Story");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", fullPathFile.Path);
                                        intent.PutExtra("Type", "video");
                                        StartActivity(intent);
                                    }     
                                              
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Uri uri = data.Data;
                            var filepath2 = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath2);
                        }

                        break;
                    }
                    // Add video camera story
                    case 513 when resultCode == Result.Ok:
                        {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            switch (type)
                            {
                                case "Video":
                                {
                                    var fileName = filepath.Split('/').Last();
                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                    var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                    var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                    switch (videoPlaceHolderImage)
                                    {
                                        case "File Dont Exists":
                                        {
                                            var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                            break;
                                        }
                                    }

                                    if (AppSettings.EnableVideoEditor)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", fullPathFile.Path);
                                        intent.PutExtra("Type", "Story");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", fullPathFile.Path);
                                        intent.PutExtra("Type", "video");
                                        StartActivity(intent);
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            var filepath2 = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            PickiTonCompleteListener(filepath2);
                        }

                        break;
                    }
                    //If its from Camera or Gallery
                    case CropImage.CropImageActivityRequestCode:
                    {
                        var result = CropImage.GetActivityResult(data);

                        switch (resultCode)
                        {
                            case Result.Ok when result.IsSuccessful:
                            {
                                var resultUri = result.Uri;

                                switch (string.IsNullOrEmpty(resultUri.Path))
                                {
                                    case false:
                                    {
                                        //Do something with your Uri
                                        Intent intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", resultUri.Path);
                                        intent.PutExtra("Type", "image");
                                        StartActivity(intent);
                                        break;
                                    }
                                    default:
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong),ToastLength.Long);
                                        break;
                                }

                                break;
                            }
                            case Result.Ok:
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                break;
                        }

                        break;
                    } 
                    //Edit post
                    case 3950 when resultCode == Result.Ok:
                    {
                        var postId = data.GetStringExtra("PostId") ?? "";
                        var postText = data.GetStringExtra("PostText") ?? "";
                        var diff = NewsFeedTab.PostFeedAdapter?.ListDiffer;
                        List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                        switch (dataGlobal.Count)
                        {
                            case > 0:
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    postData.PostData.Orginaltext = postText;
                                    var index = diff.IndexOf(postData);
                                    switch (index)
                                    {
                                        case > -1:
                                            NewsFeedTab.PostFeedAdapter?.NotifyItemChanged(index);
                                            break;
                                    }
                                }

                                var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                                switch (checkTextSection)
                                {
                                    case null:
                                    {
                                        var collection = dataGlobal.FirstOrDefault()?.PostData;
                                        var item = new AdapterModelsClass
                                        {
                                            TypeView = PostModelType.TextSectionPostPart,
                                            Id = Convert.ToInt32((int)PostModelType.TextSectionPostPart + collection?.Id),
                                            PostData = collection,
                                            IsDefaultFeedPost = true
                                        };

                                        var headerPostIndex = diff.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                        switch (headerPostIndex)
                                        {
                                            case > -1:
                                                diff.Insert(headerPostIndex + 1, item);
                                                NewsFeedTab.PostFeedAdapter?.NotifyItemInserted(headerPostIndex + 1);
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
                    //Edit post product 
                    case 3500 when resultCode == Result.Ok:
                    {
                        if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                        var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData"));
                        if (item != null)
                        {
                            var diff = NewsFeedTab.PostFeedAdapter?.ListDiffer;
                            var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                            switch (dataGlobal.Count)
                            {
                                case > 0:
                                {
                                    foreach (var postData in dataGlobal)
                                    {
                                        var index = diff.IndexOf(postData);
                                        switch (index)
                                        {
                                            case > -1:
                                            {
                                                var productUnion = postData.PostData.Product?.ProductClass;
                                                if (productUnion != null) productUnion.Id = item.Id;
                                                productUnion = item;
                                                Console.WriteLine(productUnion);
                                     
                                                NewsFeedTab.PostFeedAdapter?.NotifyItemChanged(index);
                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }
                            } 
                        }

                        break;
                    }
                    case 4711:
                        switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                        {
                            case Result.Ok:
                                switch (UpdateManagerApp.AppUpdateTypeSupported)
                                {
                                    // In app update success
                                    case AppUpdateType.Immediate:
                                        ToastUtils.ShowToast(this, "App updated", ToastLength.Short);
                                        break;
                                }
                                break;
                            case Result.Canceled:
                                ToastUtils.ShowToast(this, "In app update cancelled", ToastLength.Short);
                                break;
                            case (Result)ActivityResult.ResultInAppUpdateFailed:
                                ToastUtils.ShowToast(this, "In app update failed", ToastLength.Short);
                                break;
                        } 
                        break; 
                    // => NiceArtEditor add story text
                    case 2200 when resultCode == Result.Ok:
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                                if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                                {
                                    //Do something with your Uri
                                    Intent intent = new Intent(this, typeof(AddStoryActivity));
                                    intent.PutExtra("Uri", imagePath);
                                    intent.PutExtra("Type", "image");
                                    StartActivity(intent);
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        break;
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
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults); 
                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        switch (ImageType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image":
                                OpenDialogGallery("Image");
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "VideoCamera":
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                        }

                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        CheckAndGetLocation();
                        break; 
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 111 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new LiveUtil(this).OpenDialogLive();
                        break;
                    case 111:
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
             
        #region Set Tab

        private void AddFragmentsTabs()
        {
            try
            {
                TabAdapter.ClaerFragment();

                NewsFeedTab = new NewsFeedNative();
                NotificationsTab = new NotificationsFragment();
                TrendingTab = new TrendingFragment();
                MoreTab = new MoreFragment();

                if (TabAdapter != null && TabAdapter.ItemCount <= 0)
                {
                    TabAdapter.AddFragment(NewsFeedTab, GetText(Resource.String.Lbl_News_Feed));
                    TabAdapter.AddFragment(NotificationsTab, GetText(Resource.String.Lbl_Notifications));

                    switch (AppSettings.ShowTrendingPage)
                    {
                        case true:
                            TabAdapter.AddFragment(TrendingTab, GetText(Resource.String.Lbl_Trending));
                            break;
                    }

                    TabAdapter.AddFragment(MoreTab, GetText(Resource.String.Lbl_More));

                    ViewPager.UserInputEnabled = false;
                    ViewPager.CurrentItem = TabAdapter.ItemCount;
                    ViewPager.OffscreenPageLimit = TabAdapter.ItemCount;

                    ViewPager.Orientation = ViewPager2.OrientationHorizontal;
                    ViewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                    ViewPager.Adapter = TabAdapter;
                    ViewPager.Adapter.NotifyDataSetChanged();

                    if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                        MeoNavigationTabBar.SetZ(5);
                }

                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                    MeoNavigationTabBar.Show(0, false);
                else
                    BottomNavigationTab.SelectItem(0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly TabbedMainActivity Activity;

            public MyOnPageChangeCallback(TabbedMainActivity activity)
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

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    switch (position)
                    {
                        case < 0:
                            return;
                        // News_Feed_Tab
                        case 0:
                            {
                                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                {
                                    if (Activity.MeoNavigationTabBar.GetSelectedId() != position)
                                        Activity.MeoNavigationTabBar.Show(0, true);
                                }
                                else
                                    Activity.BottomNavigationTab.SelectItem(0);

                                if (AppSettings.ShowFilterPost)
                                    Activity.RlFilter.Visibility = ViewStates.Visible;

                                break;
                            }
                        // Notifications_Tab
                        case 1:
                            {
                                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                {
                                    if (Activity.MeoNavigationTabBar.GetSelectedId() != position)
                                        Activity.MeoNavigationTabBar.Show(1, true);
                                }
                                else
                                    Activity.BottomNavigationTab.SelectItem(1);

                                Activity.RlFilter.Visibility = ViewStates.Gone;
                                
                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                        // Trending_Tab
                        case 2 when AppSettings.ShowTrendingPage:
                            {
                                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                {
                                    if (Activity.MeoNavigationTabBar.GetSelectedId() != position)
                                        Activity.MeoNavigationTabBar.Show(2, true);
                                }
                                else
                                    Activity.BottomNavigationTab.SelectItem(2);

                                Activity.RlFilter.Visibility = ViewStates.Gone;

                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                        // More_Tab
                        case 3:
                            {
                                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                {
                                    if (Activity.MeoNavigationTabBar.GetSelectedId() != position)
                                        Activity.MeoNavigationTabBar.Show(3, true);
                                }
                                else
                                    Activity.BottomNavigationTab.SelectItem(3);

                                Activity.RlFilter.Visibility = ViewStates.Gone;

                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }
         
        #endregion

        #region MaterialDialog

        private void ShowDialogAddStory()
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.image));
                if (WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                    arrayAdapter.Add(GetText(Resource.String.video));

                arrayAdapter.Add(GetText(Resource.String.text));

                dialogList.Title(GetText(Resource.String.Lbl_Addnewstory)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogVideo()
        {
            try
            { 
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.Title(GetText(Resource.String.Lbl_SelectVideoFrom)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
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
                if (itemString == GetText(Resource.String.image))
                {
                    OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                }
                else if (itemString == GetText(Resource.String.video))
                {
                    OpenDialogVideo(); 
                } 
                else if (itemString == GetText(Resource.String.Lbl_VideoGallery))
                {
                    ImageType = "Video";

                    switch ((int)Build.VERSION.SdkInt)
                    {
                        // Check if we're running on Android 5.0 or higher
                        case < 23:
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                            break;
                        default:
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                            {
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }

                            break;
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera))
                {
                    ImageType = "VideoCamera";

                    switch ((int)Build.VERSION.SdkInt)
                    {
                        // Check if we're running on Android 5.0 or higher
                        case < 23:
                            //requestCode >> 513 => video Camera
                            new IntentController(this).OpenIntentVideoCamera();
                            break;
                        default:
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                            {
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }

                            break;
                        }
                    }
                }  
                else if (itemString == GetText(Resource.String.Lbl_Create_Ad))
                {
                    StartActivity(new Intent(this, typeof(CreateAdvertiseActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Create_Events))
                {
                    StartActivity(new Intent(this, typeof(CreateEventActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_CreateNewProduct))
                {
                    StartActivity(new Intent(this, typeof(CreateProductActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Create_New_Page))
                {
                    StartActivity(new Intent(this, typeof(CreatePageActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Create_New_Group))
                {
                    StartActivity(new Intent(this, typeof(CreateGroupActivity)));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        #endregion

        #region Stories

        public void StoryAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                var diff = NewsFeedTab?.PostFeedAdapter?.ListDiffer;
                var checkSection = diff?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    //Open View Story Or Create New Story
                    var item = NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.GetItem(e.Position);
                    if (item != null)
                    {
                        //var circleIndicator = e.View.FindViewById<CircleImageView>(Resource.Id.profile_indicator); 
                        //circleIndicator.BorderColor = Color.ParseColor(Settings.StoryReadColor);

                        switch (item.Type)
                        {
                            case "Your":
                                ShowDialogAddStory();
                                break;
                            case "Live":
                            { 
                                if (item.DataLivePost?.LiveTime != null && item.DataLivePost?.LiveTime.Value > 0 && string.IsNullOrEmpty(item.DataLivePost?.AgoraResourceId) && string.IsNullOrEmpty(item.DataLivePost?.PostFile))
                                {
                                    //Live
                                    //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                                    Intent intent = new Intent(this, typeof(LiveStreamingActivity));
                                    intent.PutExtra(Constants.KeyClientRole, DT.Xamarin.Agora.Constants.ClientRoleAudience);
                                    intent.PutExtra("PostId", item.DataLivePost.PostId);
                                    intent.PutExtra("StreamName", item.DataLivePost.StreamName);
                                    intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(item.DataLivePost));
                                    StartActivity(intent);
                                }
                                break;
                            } 
                            default:
                            {
                                if (NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.Count  > 0)
                                {
                                    List<StoryDataObject> storyList = new List<StoryDataObject>(NewsFeedTab.PostFeedAdapter?.HolderStory.StoryAdapter.StoryList);
                                    storyList.RemoveAll(o => o.Type == "Your" || o.Type == "Live");

                                    var indexItem = storyList.IndexOf(item);

                                    Intent intent = new Intent(this, typeof(StoryDetailsActivity));
                                    intent.PutExtra("UserId", item.UserId);
                                    intent.PutExtra("IndexItem", indexItem);
                                    intent.PutExtra("StoriesCount", storyList.Count);
                                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(new ObservableCollection<StoryDataObject>(storyList)));
                                    StartActivity(intent);

                                    //item.ProfileIndicator = AppSettings.StoryReadColor;
                                    //NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.NotifyItemChanged(e.Position);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Back Pressed 

        public override void OnBackPressed()
        {
            try
            {
                switch (RecentlyBackPressed)
                {
                    case true:
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                        Finish();
                        break;
                    default:
                        RecentlyBackPressed = true; 
                        ToastUtils.ShowToast(this, GetString(Resource.String.press_again_exit), ToastLength.Long);
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                        break;
                }
            }
            catch (Exception exception)
            {
                base.OnBackPressed();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    case < 23:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    default:
                    {
                        if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                        {
                            Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        }
                        else
                        {
                            //request Code 110
                            new PermissionsController(this).RequestPermission(110);
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

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenDim, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service

        private void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this);
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region General App Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                if (string.IsNullOrEmpty(Current.AccessToken) || string.IsNullOrEmpty(UserDetails.UserId))
                   sqlEntity.Get_data_Login_Credentials();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }
                 
                var dataUser = sqlEntity.Get_MyProfile();

                switch (string.IsNullOrEmpty(UserDetails.Avatar))
                {
                    case false:
                        Glide.With(this).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
                        break;
                }

                Task.Run(() =>
                {
                    ListUtils.StickersList = sqlEntity.Get_From_StickersTb();

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this), ApiRequest.GetMyGroups, ApiRequest.GetMyPages, ApiRequest.GetLastArticles, ApiRequest.LoadSuggestedUser, ApiRequest.LoadSuggestedGroup, ApiRequest.LoadSuggestedPage, ApiPostAsync.GetAllPostVideo });
                }).ConfigureAwait(false);

                switch (dataUser?.ShareMyLocation)
                {
                    // Check if we're running on Android 5.0 or higher
                    case "1" when (int)Build.VERSION.SdkInt >= 23:
                    {
                        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                            CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                        {
                            CheckAndGetLocation();
                        }
                        else
                        {
                            // 100 >> Storage , 103 >> Camera , 105 >> Location
                            new PermissionsController(this).RequestPermission(105);
                        }
                        break;
                    }
                    case "1":
                        CheckAndGetLocation();
                        break;
                }
                 
                LoadConfigSettings();

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                if (ListUtils.SettingsSiteList?.PostColors?.PostColorsList != null)
                {
                    var fullGlideRequestBuilder = Glide.With(this).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(200);

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            foreach (var item in ListUtils.SettingsSiteList?.PostColors?.PostColorsList.Values)
                            {
                                switch (string.IsNullOrEmpty(item.Image))
                                {
                                    case false:
                                        fullGlideRequestBuilder.Load(item.Image).Preload();
                                        break;
                                    default:
                                    {
                                        var colorsList = new List<int>();

                                        switch (string.IsNullOrEmpty(item.Color1))
                                        {
                                            case false:
                                                colorsList.Add(Color.ParseColor(item.Color1));
                                                break;
                                        }

                                        switch (string.IsNullOrEmpty(item.Color2))
                                        {
                                            case false:
                                                colorsList.Add(Color.ParseColor(item.Color2));
                                                break;
                                        }

                                        GradientDrawable gd = new GradientDrawable(GradientDrawable.Orientation.TopBottom, colorsList.ToArray());
                                        gd.SetCornerRadius(0f);

                                        fullGlideRequestBuilder.Load(gd).Preload();
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void CheckAndGetLocation()
        {
            try
            {
                var locationManager = (LocationManager)GetSystemService(LocationService);
                if (!locationManager!.IsProviderEnabled(LocationManager.GpsProvider))
                {

                }
                else
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                    Console.WriteLine("Position Status: {0}", position.Timestamp);
                    Console.WriteLine("Position Latitude: {0}", position.Latitude);
                    Console.WriteLine("Position Longitude: {0}", position.Longitude);

                    UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                    UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);

                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (Methods.CheckConnectivity())
                            {
                                Dictionary<string, string> dictionaryProfile = new Dictionary<string, string>();

                                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dictionaryProfile = new Dictionary<string, string>();

                                    dataUser.Lat = UserDetails.Lat;
                                    dataUser.Lat = UserDetails.Lat;

                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                }

                                dictionaryProfile.Add("lat", UserDetails.Lat);
                                dictionaryProfile.Add("lng", UserDetails.Lng);

                                //if (Methods.CheckConnectivity())
                                //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dictionaryProfile) });
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void GetOneSignalNotification()
        {
            try
            {
                string userId = Intent?.GetStringExtra("userId") ?? "";
                string postId = Intent?.GetStringExtra("PostId") ?? "";
                string pageId = Intent?.GetStringExtra("PageId") ?? "";
                string groupId = Intent?.GetStringExtra("GroupId") ?? "";
                string eventId = Intent?.GetStringExtra("EventId") ?? "";
                string type = Intent?.GetStringExtra("type") ?? "";
                string notifier = Intent?.GetStringExtra("Notifier") ?? "";
                if (notifier == "Chat" || notifier == "Notifier")
                {
                    //PageId, GroupId,EventId
                    NotificationsTab.EventClickNotification(this, new NotificationObject
                    {
                        NotifierId = userId,
                        Notifier = new UserDataObject
                        {
                            UserId = userId,
                        },
                        PostId = postId,
                        PageId = pageId,
                        GroupId = groupId,
                        EventId = eventId,
                        Type = type,
                    });

                    //Old 
                    //if (!string.IsNullOrEmpty(userId))
                    //{
                    //    MeoNavigationTabBar.Show(1, true);
                    //    ViewPager.SetCurrentItem(1, true);
                    //}
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public async Task Get_Notifications()
        {
            try
            {
                var (countNotifications, countFriend, countMessages) = await NotificationsTab.LoadGeneralData(false).ConfigureAwait(false); 
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(countNotifications) == false && (countNotifications != "0" && countNotifications != CountNotificationsStatic))
                            {
                                if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                    FragmentBottomNavigator.ShowBadge(1, countNotifications, true);
                                else
                                    BottomNavigationTab.ShowBadge(1, Convert.ToInt32(countNotifications), true);

                                CountNotificationsStatic = countNotifications;
                            }

                            if (string.IsNullOrEmpty(countFriend) == false && (countFriend != "0" && countFriend != CountFriendStatic))
                            {
                                switch (AppSettings.ShowTrendingPage)
                                {
                                    case true:
                                        if (AppSettings.NavigationBottom == BottomNavigationSystem.MeowBottomNavigation)
                                            FragmentBottomNavigator.ShowBadge(2, countFriend, true);
                                        else
                                            BottomNavigationTab.ShowBadge(2, Convert.ToInt32(countFriend), true);

                                        CountFriendStatic = countFriend;
                                        break;
                                }
                            }

                            if (AppSettings.MessengerIntegration && (!string.IsNullOrEmpty(countMessages) && countMessages != "0" && countMessages != CountMessagesStatic))
                            {
                                var listMore = MoreTab.MoreSectionAdapter1.SectionList;
                                switch (listMore?.Count)
                                {
                                    case > 0:
                                    {
                                        var dataTab = listMore.FirstOrDefault(a => a.Id == 2);
                                        if (dataTab != null)
                                        {
                                            CountMessagesStatic = countMessages;
                                            dataTab.BadgeCount = Convert.ToInt32(countMessages);
                                            dataTab.Badgevisibilty = true;

                                            MoreTab.MoreSectionAdapter1.NotifyItemChanged(listMore.IndexOf(dataTab), "WithoutBlobBadge");
                                        }

                                        break;
                                    }
                                }
                            }
                            else if (AppSettings.MessengerIntegration && countMessages == "0")
                            {
                                var listMore = MoreTab.MoreSectionAdapter1?.SectionList;
                                switch (listMore?.Count)
                                {
                                    case > 0:
                                    {
                                        var dataTab = listMore.FirstOrDefault(a => a.Id == 2);
                                        if (dataTab != null)
                                        {
                                            CountMessagesStatic = "0";
                                            dataTab.BadgeCount = 0;
                                            dataTab.Badgevisibilty = false;
                                            dataTab.IconColor = Color.ParseColor("#03a9f4");

                                            MoreTab.MoreSectionAdapter1.NotifyItemChanged(listMore.IndexOf(dataTab), "WithoutBlobBadge");
                                        }

                                        break;
                                    }
                                }
                            } 
                        }
                        catch (Exception e)
                        {
                            //Methods.DisplayReportResultTrack(e);
                        }
                    });
            }
            catch (Exception e)
            {
                //Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                if (!WoWonderTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_AllowedFileUpload), GetText(Resource.String.Lbl_Ok));
                    return;
                }
                
                ImageType = typeImage;
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
                            .SetGuidelinesColor(Resource.Color.accent)
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
                                .SetGuidelinesColor(Resource.Color.accent)
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
         
        private void InAppUpdate()
        {
            try
            {
                switch (AppSettings.ShowSettingsUpdateManagerApp)
                {
                    case true:
                        UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview?.GetBoolean(MainSettings.PrefKeyInAppReview, false) ?? false;
                switch (inAppReview)
                {
                    case false when AppSettings.ShowSettingsRateApp:
                    {
                        if (CountRateApp == AppSettings.ShowRateAppCount)
                        {
                            var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                            dialog.Title(GetText(Resource.String.Lbl_RateOurApp)).TitleColorRes(Resource.Color.primary);
                            dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                            dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                            {
                                try
                                {
                                    StoreReviewApp store = new StoreReviewApp();
                                    store.OpenStoreReviewPage(PackageName);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();

                            MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                        }
                        else
                        {
                            CountRateApp++;
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
         
        #region PickiT >> Gert path file

        public async void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile) => "Drive file was selected" 
                //else if (wasUnknownProvider)  => "File was selected from unknown provider" 
                //else => "Local file was selected"

                //  Chick if it was successful
                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(path);
                if (!check)
                {
                    if (info == "AdultImages")
                    {
                        //this file not allowed 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);

                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Content(GetText(Resource.String.Lbl_Error_AdultImages));
                        dialog.PositiveText(GetText(Resource.String.Lbl_IgnoreAndSend)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(path);
                                switch (type)
                                {
                                    case "Image":
                                        {
                                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                                            intent.PutExtra("Uri", path);
                                            intent.PutExtra("Type", "image");
                                            StartActivity(intent);
                                            break;
                                        }
                                    case "Video":
                                        {
                                            var fileName = path.Split('/').Last();
                                            var fileNameWithoutExtension = fileName.Split('.').First();
                                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                            switch (videoPlaceHolderImage)
                                            {
                                                case "File Dont Exists":
                                                    {
                                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, path);
                                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                                        break;
                                                    }
                                            }

                                            if (AppSettings.EnableVideoEditor)
                                            {
                                                Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                                intent.PutExtra("Uri", path);
                                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                                intent.PutExtra("Type", "Story");
                                                StartActivity(intent);
                                            }
                                            else
                                            {
                                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                                intent.PutExtra("Uri", path);
                                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                                intent.PutExtra("Type", "video");
                                                StartActivity(intent);
                                            }

                                            break;
                                        }
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new WoWonderTools.MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                    else
                    {
                        //this file not supported on the server , please select another file 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                        return;
                    } 
                }
                else
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    switch (type)
                    {
                        case "Image":
                        {
                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                            intent.PutExtra("Uri", path);
                            intent.PutExtra("Type", "image");
                            StartActivity(intent);
                            break;
                        }
                        case "Video":
                        {
                            var fileName = path.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            switch (videoPlaceHolderImage)
                            {
                                case "File Dont Exists":
                                {
                                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, path);
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                    break;
                                }
                            }

                            if (AppSettings.EnableVideoEditor)
                            {
                                Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                intent.PutExtra("Uri", path);
                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                intent.PutExtra("Type", "Story");
                                StartActivity(intent);
                            }
                            else
                            {
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", path);
                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                intent.PutExtra("Type", "video");
                                StartActivity(intent);
                            }
                             
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
    }
}