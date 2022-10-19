using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.AppBar;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Base;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.SearchForPosts;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.Upgrade;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;
using WoWonder.Activities.Tabbes;

namespace WoWonder.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyProfileActivity : BaseActivity, MaterialDialog.IListCallback, AppBarLayout.IOnOffsetChangedListener 
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private ImageView ImageCover; 
        private ImageButton BtnMore, ImageBack;
        private TextView TxtSearchForPost;
        
        private SwipeRefreshLayout SwipeRefreshLayout;
        public WRecyclerView MainRecyclerView;

        private AdView MAdView;

        private FeedCombiner Combiner; 
        private static MyProfileActivity Instance;
        private UserDataObject UserData;

        public NativePostAdapter PostFeedAdapter;
        private string ImageType;

        private string Caller = "";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.Overlap_Dark : Resource.Style.Overlap_Light);

                Methods.App.FullScreenApp(this);

                Overlap();

                // Create your application here
                SetContentView(Resource.Layout.MyProfile_Layout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetMyInfoData();
                PostClickListener.OpenMyProfile = true;

                AdsGoogle.Ad_Interstitial(this);
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
                MainRecyclerView?.StopVideo();
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
                MainRecyclerView?.StopVideo();
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
                MainRecyclerView?.ReleasePlayer();
                PostClickListener.OpenMyProfile = false;
                MAdView?.Destroy();
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void Overlap()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Window.SetStatusBarColor(Color.Transparent);
#pragma warning disable 618
                    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LayoutFullscreen | (StatusBarVisibility)SystemUiFlags.LayoutStable;
#pragma warning restore 618
                }

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
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this); 

                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                ImageCover = FindViewById<ImageView>(Resource.Id.cover_image);
                ImageBack = FindViewById<ImageButton>(Resource.Id.back); 
                BtnMore = FindViewById<ImageButton>(Resource.Id.BtnMore); 
               
                TxtSearchForPost = FindViewById<TextView>(Resource.Id.tv_SearchForPost);
                TxtSearchForPost.Visibility = ViewStates.Invisible;
                 

                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);
                  
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
                    toolBar.Title = "";
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                PostFeedAdapter = new NativePostAdapter(this, UserDetails.UserId, MainRecyclerView, NativeFeedType.User);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                Combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, this);
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
                        ImageCover.Click += ImageCoverOnClick;
                        ImageBack.Click += ImageBackOnClick;
                        BtnMore.Click += BtnMoreOnClick; 
                        //ImageAvatar.Click += ImageAvatarOnClick;
                        //BtnEdit.Click += BtnEditOnClick;
                        //BtnWallet.Click += BtnWalletOnClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        TxtSearchForPost.Click += TxtSearchForPostOnClick;
                        break;
                    default:
                        ImageCover.Click -= ImageCoverOnClick;
                        ImageBack.Click -= ImageBackOnClick;
                        BtnMore.Click -= BtnMoreOnClick;
                        //ImageAvatar.Click -= ImageAvatarOnClick;
                        //BtnEdit.Click -= BtnEditOnClick;
                        //BtnWallet.Click -= BtnWalletOnClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        TxtSearchForPost.Click -= TxtSearchForPostOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static MyProfileActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private void DestroyBasic()
        {
            try
            {
                ImageCover = null!;
                Instance = null!;
                //ImageAvatar = null!;
                //ImageBack = null!;
                //BtnMore = null!;
                //TxtName = null!;
                //TxtUsername = null!;
                //BtnEdit = null!; 
                SwipeRefreshLayout = null!;
                MainRecyclerView = null!;
                MAdView = null!;
                Combiner = null!;
                UserData = null!;
                PostFeedAdapter = null!; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void TxtSearchForPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchForPostsActivity));
                intent.PutExtra("TypeSearch", "user");
                intent.PutExtra("IdSearch", UserDetails.UserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }
                 
                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                MainRecyclerView.MainScrollEvent.IsLoading = false;
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                arrayAdapter.Add(GetText(Resource.String.Lbl_EditAvatar));
                arrayAdapter.Add(GetText(Resource.String.Lbl_EditCover));
                
                arrayAdapter.Add(GetText(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Activities));
                arrayAdapter.Add(GetText(Resource.String.Lbl_ViewPrivacy));
                arrayAdapter.Add(GetText(Resource.String.Lbl_SettingsAccount));

                switch (ListUtils.SettingsSiteList?.Pro)
                {
                    case "1" when AppSettings.ShowGoPro && UserData.ProType != "4":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_upgrade_now));
                        break;
                }

                dialogList.Title(Resource.String.Lbl_More).TitleColorRes(Resource.Color.primary);
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

        private void ImageBackOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Caller.Contains("free"))
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageCoverOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserData.Cover.Contains("d-cover"))
                    return;

                if (!string.IsNullOrEmpty(UserData.CoverPostId) && UserData.CoverPostId != "0")
                {
                    var intent = new Intent(this, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", UserData.CoverPostId);
                    //intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    StartActivity(intent);
                }
                else
                {
                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Cover.Split('/').Last(), UserData.Cover);
                    if (media.Contains("http"))
                    {
                        var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                        StartActivity(intent);
                    }
                    else
                    {
                        var file2 = new File(media);
                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                        var intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Get Profile

        private void GetMyInfoData()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    LoadPassedDate(dataUser);

                    switch (ListUtils.MyFollowersList.Count)
                    {
                        case > 0 when dataUser.Details.DetailsClass != null:
                            LoadFriendsLayout(new List<UserDataObject>(ListUtils.MyFollowersList), Methods.FunString.FormatPriceValue(Convert.ToInt32(dataUser.Details.DetailsClass.FollowingCount)));
                            break;
                    }

                    PostFeedAdapter?.NotifyDataSetChanged();
                }

                PostFeedAdapter?.SetLoading(); 
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data,following");

            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                LoadPassedDate(result.UserData);
                 
                switch (result.Following.Count)
                {
                    //if (SPrivacyFriend == "0" || result.UserProfileObject?.IsFollowing == "1" && SPrivacyFriend == "1" || SPrivacyFriend == "2")
                    case > 0 when result.UserData.Details.DetailsClass != null:
                        RunOnUiThread(() => { LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(Convert.ToInt32(result.UserData.Details.DetailsClass.FollowingCount))); });
                        break;
                }

                //##Set the AddBox place on Main RecyclerView
                //------------------------------------------------------------------------
                var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                if (check != null)
                {
                    Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check) + 2);
                    //switch (AppSettings.ShowSearchForPosts)
                    //{
                    //    case true:
                    //        Combiner.SearchForPostsView("user");
                    //        break;
                    //}
                }
                
                //------------------------------------------------------------------------ 
                RunOnUiThread(() =>
                {
                    try
                    {
                        PostFeedAdapter?.NotifyDataSetChanged();
                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.Insert_Or_Update_To_MyProfileTable(result.UserData);

                        switch (result.Following?.Count)
                        {
                            case > 0:
                                sqlEntity.Insert_Or_Replace_MyContactTable(new ObservableCollection<UserDataObject>(result.Following));
                                break;
                        }

                        ListUtils.MyFollowersList = result.Followers?.Count switch
                        {
                            > 0 => new ObservableCollection<UserDataObject>(result.Followers),
                            _ => ListUtils.MyFollowersList
                        };
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
            }
        }
        
        private void LoadPassedDate(UserDataObject result)
        {
            try
            {
                UserData = result;
                 
                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                    default:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                }
                  
                var checkDetailsSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.ProfileDetailsSection);
                if (checkDetailsSection == null)
                {
                    Combiner.ProfileDetailsView(result, 0, "MyProfile");
                } 

                var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                switch (checkAboutBox)
                {
                    case null:
                        Combiner.AboutBoxPostView(WoWonderTools.GetAboutFinal(result), 2); 
                        break;
                    default:
                        checkAboutBox.AboutModel.Description = WoWonderTools.GetAboutFinal(result); 
                        break;
                }

                var checkInfoUserBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoUserBox);
                switch (checkInfoUserBox)
                {
                    case null:
                        Combiner.InfoUserBoxPostView(result, 3);
                        break;
                    default:
                        checkInfoUserBox.InfoUserModel.UserData = result; 
                        break;
                }


                if (!result.Cover.Contains("d-cover"))
                    WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Cover.Split('/').Last(), result.Cover);

                if (!result.Avatar.Contains("d-avatar"))
                    WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Avatar.Split('/').Last(), result.Avatar); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFriendsLayout(List<UserDataObject> followers, string friendsCounter)
        {
            try
            {
                if (followers?.Count > 0)
                {
                    BtnMore.Visibility = ViewStates.Visible;

                    var followersClass = new FollowersModelClass
                    {
                        TitleHead = GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends),
                        FollowersList = new List<UserDataObject>(followers.Take(12)),
                        More = friendsCounter
                    };

                    var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                    if (check != null)
                    {
                        check.FollowersModel = followersClass;
                    }
                    else
                    {
                        Combiner.FollowersBoxPostView(followersClass, 4); 
                    }
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
                var text = itemString;
                if (text == GetText(Resource.String.Lbl_EditAvatar))
                {
                    OpenDialogGallery("Avatar");
                }
                else if (text == GetText(Resource.String.Lbl_EditCover))
                {
                    OpenDialogGallery("Cover");
                }
                else if (text == GetText(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLinkToProfile_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_ViewPrivacy))
                {
                    OnViewPrivacy_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Activities))
                {
                    OnMyActivities_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_SettingsAccount))
                {
                    OnSettingsAccount_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_upgrade_now))
                {
                    UpgradeNow_Click();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Event Menu >> Cope Link To Profile
        private void OnCopeLinkToProfile_Button_Click()
        {
            try
            {
                Methods.CopyToClipboard(this, UserData.Url);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click()
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
                            Title = UserData.Name,
                            Text = "",
                            Url = UserData.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> View Privacy Shortcuts
        private void OnViewPrivacy_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(PrivacyActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> View Privacy Shortcuts
        private void OnMyActivities_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(MyActivitiesActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> General Account 
        private void OnSettingsAccount_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GeneralAccountActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpgradeNow_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GoProActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Update Image Avatar && Cover

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

        private async Task Update_Image_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    switch (type)
                    {
                        case "Avatar":
                            {
                                var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(path);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    {
                                                        Console.WriteLine(result.Message);
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                                        var file2 = new File(path);
                                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                        //  Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(ImageAvatar);
                                                        //Set image  
                                                        //GlideImageLoader.LoadImage(this, path, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                         
                                                        var data = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.MyProfileInfoHeaderSection);
                                                        if (data != null)
                                                        {
                                                            data.InfoUserModel.UserData.Avatar = photoUri.ToString(); 
                                                        }
                                                        
                                                        RunOnUiThread(() =>
                                                        {
                                                            try
                                                            {
                                                                var d = new Runnable(() => { PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(data)); });
                                                                d.Run();
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                Methods.DisplayReportResultTrack(e);
                                                            }
                                                        });
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
                                }

                                break;
                            }
                        case "Cover":
                            {
                                var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserCoverAsync(path);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    {
                                                        Console.WriteLine(result.Message);
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                                        //Set image 
                                                        var file2 = new File(path);
                                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageCover);

                                                        //GlideImageLoader.LoadImage(this, path, CoverImage, ImageStyle.FitCenter, ImagePlaceholders.Drawable);
                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
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

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    //If its from Camera or Gallery
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
                                                                string pathImg;
                                                                switch (ImageType)
                                                                {
                                                                    case "Cover":
                                                                        pathImg = resultUri.Path;
                                                                        UserDetails.Cover = pathImg;
                                                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, pathImg) });
                                                                        break;
                                                                    case "Avatar":
                                                                        pathImg = resultUri.Path;
                                                                        UserDetails.Avatar = pathImg;
                                                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, pathImg) });
                                                                        break;
                                                                }

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
                    //Edit post
                    case 3950 when resultCode == Result.Ok:
                        {
                            var postId = data.GetStringExtra("PostId") ?? "";
                            var postText = data.GetStringExtra("PostText") ?? "";
                            var diff = PostFeedAdapter?.ListDiffer;
                            var dataGlobal = diff?.Where(a => a.PostData?.Id == postId).ToList();
                            switch (dataGlobal?.Count)
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
                                                    PostFeedAdapter?.NotifyItemChanged(index);
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
                                                            PostFeedAdapter?.NotifyItemInserted(headerPostIndex + 1);
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
                            var item = JsonConvert.DeserializeObject<ProductDataObject>(data?.GetStringExtra("itemData") ?? "");
                            if (item != null)
                            {
                                var diff = PostFeedAdapter?.ListDiffer;
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

                                                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
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
                    //Edit profile 
                    case 5124 when resultCode == Result.Ok:
                        {
                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                            if (dataUser != null)
                            {
                                LoadPassedDate(dataUser);
                                PostFeedAdapter?.NotifyDataSetChanged();
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
                        OpenDialogGallery(ImageType);
                        break;
                    case 108:
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

        #region appBarLayout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                int minHeight = ViewCompat.GetMinimumHeight(CollapsingToolbar) * 2;
                float scale = (float)(minHeight + verticalOffset) / minHeight;

                if (scale >= 0)
                {
                    ImageBack.SetColorFilter(Color.White);
                    BtnMore.SetColorFilter(Color.White);
                    TxtSearchForPost.Visibility = ViewStates.Invisible;
                }
                else
                {
                    //ImageBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                    if (AppSettings.ShowSearchForPosts)
                    {
                        TxtSearchForPost.BackgroundTintList = ColorStateList.ValueOf(AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#ecedf1"));
                        TxtSearchForPost.Visibility = ViewStates.Visible;
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