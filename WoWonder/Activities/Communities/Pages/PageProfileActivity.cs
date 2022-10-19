using System;
using System.Collections.Generic;
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
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using TheArtOfDev.Edmodo.Cropper;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Pages.Settings;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using File = Java.IO.File;
using Uri = Android.Net.Uri;
using AndroidX.CardView.Widget;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Upgrade;
using WoWonderClient.Classes.Message;

namespace WoWonder.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PageProfileActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private SwipeRefreshLayout SwipeRefreshLayout;
        private ImageView ProfileImage, CoverImage;
        private TextView TxtSearchForPost, TxtPageName, TxtPageUsername;
        private ImageButton BtnMore, IconBack;
        private FloatingActionButton FloatingActionButtonView;
        private RelativeLayout EditAvatarImagePage, EditCoverImage;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private string PageId = "", UserId = "", ImageType = "";
        public static PageDataObject PageData;
        private FeedCombiner Combiner;
        private static PageProfileActivity Instance;
        private AdView MAdView;
        private CardView LikeCardView, ActionCardView, MessageCardView;
        private TextView LikeTxt, ActionTxt, MessageTxt;

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
                SetContentView(Resource.Layout.Page_Profile_Layout);

                Instance = this;
                PageId = Intent?.GetStringExtra("PageId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                GetDataPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
                MainRecyclerView?.ReleasePlayer();
                MAdView?.Destroy();
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
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this);

                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                TxtSearchForPost = FindViewById<TextView>(Resource.Id.tv_SearchForPost);
                TxtSearchForPost.Visibility = ViewStates.Invisible;

                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);
                IconBack = (ImageButton)FindViewById(Resource.Id.image_back);
                EditAvatarImagePage = (RelativeLayout)FindViewById(Resource.Id.LinearEdit);
                EditCoverImage = (RelativeLayout)FindViewById(Resource.Id.cover_layout);
                TxtPageName = (TextView)FindViewById(Resource.Id.Page_name);
                TxtPageUsername = (TextView)FindViewById(Resource.Id.Page_Username);

                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);

                // buttons with cardview
                LikeCardView = FindViewById<CardView>(Resource.Id.likeButton);
                ActionCardView = FindViewById<CardView>(Resource.Id.actionButton);
                MessageCardView = FindViewById<CardView>(Resource.Id.messagebutton);
                LikeCardView.Tag = "UserPage";

                LikeTxt = FindViewById<TextView>(Resource.Id.likeTxt);
                ActionTxt = FindViewById<TextView>(Resource.Id.actionTxt);
                MessageTxt = FindViewById<TextView>(Resource.Id.msgTxt);

                if (AppSettings.MessengerIntegration)
                    MessageCardView.Visibility = ViewStates.Gone;
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
                PostFeedAdapter = new NativePostAdapter(this, PageId, MainRecyclerView, NativeFeedType.Page);
                MainRecyclerView?.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
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
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        LikeCardView.Click += BtnLikeOnClick;
                        BtnMore.Click += BtnMoreOnClick;
                        EditCoverImage.Click += TxtEditPageInfoOnClick;
                        IconBack.Click += IconBackOnClick;
                        EditAvatarImagePage.Click += EditAvatarImagePageOnClick;
                        FloatingActionButtonView.Click += AddPostOnClick;
                        MessageCardView.Click += MessageButtonOnClick;
                        ProfileImage.Click += UserProfileImageOnClick;
                        CoverImage.Click += CoverImageOnClick;
                        break;
                    default:
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        LikeCardView.Click -= BtnLikeOnClick;
                        BtnMore.Click -= BtnMoreOnClick;
                        EditCoverImage.Click -= TxtEditPageInfoOnClick;
                        IconBack.Click -= IconBackOnClick;
                        EditAvatarImagePage.Click -= EditAvatarImagePageOnClick;
                        FloatingActionButtonView.Click -= AddPostOnClick;
                        MessageCardView.Click -= MessageButtonOnClick;
                        ProfileImage.Click -= UserProfileImageOnClick;
                        CoverImage.Click -= CoverImageOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static PageProfileActivity GetInstance()
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

        private void DestroyBasic()
        {
            try
            {
                SwipeRefreshLayout = null!;
                ProfileImage = null!;
                CoverImage = null!;
                IconBack = null!;
                TxtPageName = null!;
                TxtPageUsername = null!;
                LikeCardView = null!;
                ActionCardView = null!;
                BtnMore = null!;
                MessageCardView = null!;
                LikeTxt = null!;
                ActionTxt = null!;
                MessageTxt = null!;
                FloatingActionButtonView = null!;
                EditCoverImage = null!;
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                PageId = null!;
                UserId = null!;
                ImageType = null!;
                PageData = null!;
                EditAvatarImagePage = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Refresh
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

                if (PageData != null)
                    LoadPassedData(PageData);

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open image Cover
        private void CoverImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, PageData.Cover.Split('/').Last(), PageData.Cover);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open image Avatar
        private void UserProfileImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, PageData.Avatar.Split('/').Last(), PageData.Avatar);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Rating Page 
        public void RatingLinerOnClick()
        {
            try
            {
                if (PageData.IsRated != null && PageData.IsRated.Value)
                {
                    // You have already rated this page!
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouHaveAlReadyRatedThisPage), ToastLength.Short);
                }
                else
                {
                    var dialog = new DialogRatingBarFragment(this, PageId, PageData);
                    dialog.Show(SupportFragmentManager, dialog.Tag);
                    dialog.OnUpComplete += DialogOnOnUpComplete;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DialogOnOnUpComplete(object sender, DialogRatingBarFragment.RatingBarUpEventArgs e)
        {
            try
            {
                var th = new Thread(ActLikeARequest);
                th.Start();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void ActLikeARequest()
        {
            var x = Resource.Animation.slide_right;
            Console.WriteLine(x);
        }

        //Event Add New post  
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialPage");
                intent.PutExtra("PostId", PageId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(PageData));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Update Image Cover Page
        private void EditAvatarImagePageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtEditPageInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery("Cover");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Pages)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Reviews));
                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                {
                    switch (PageData?.Boosted)
                    {
                        case "0":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BoostPage));
                            break;
                        case "1":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_UnBoostPage));
                            break;
                    }
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Settings));
                }

                if (PageData.IsReported != null && PageData.IsReported.Value)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_CancelReport));
                else
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ReportThisPage));

                dialogList.Title(GetString(Resource.String.Lbl_More)).TitleColorRes(Resource.Color.primary);
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

        //Event Like => like , dislike 
        private async void BtnLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (LikeCardView?.Tag?.ToString())
                {
                    case "MyPage":
                        SettingsPage_OnClick();
                        break;
                    default:
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            else
                            {
                                var isLiked = LikeTxt?.Text == GetText(Resource.String.Btn_Liked) ? "false" : "true";
                                LikeCardView.BackgroundTintList = isLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor)) : AppSettings.SetTabDarkTheme ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                                LikeTxt.Text = GetText( isLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                                LikeTxt.SetTextColor( isLiked == "true" ? Color.White : Color.ParseColor(AppSettings.MainColor));

                                var (apiStatus, respond) = await RequestsAsync.Page.LikePageAsync(PageId);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case LikePageObject result:
                                                    isLiked = result.LikeStatus == "unliked" ? "false" : "true";
                                                    LikeCardView.BackgroundTintList = isLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor)) : AppSettings.SetTabDarkTheme ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                                                    LikeTxt.Text = GetText( isLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                                                    LikeTxt.SetTextColor( isLiked == "true" ? Color.White : Color.ParseColor(AppSettings.MainColor));
                                                    break;
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
                                }
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

        //Event Send Message to page  
        private void MessageButtonOnClick(object sender, EventArgs e)
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
                                    Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChatPage", new ChatObject { UserId = UserId, PageId = PageId, PageName = PageData.PageName, Avatar = PageData.Avatar });
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
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChatPage", new ChatObject { UserId = UserId, PageId = PageId, PageName = PageData.PageName, Avatar = PageData.Avatar });
                        break;
                    default:
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }

                            var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            dialog.Title(GetString(Resource.String.Lbl_SendMessageTo) + " " + Methods.FunString.DecodeString(PageData.Name)).TitleColorRes(Resource.Color.primary);
                            dialog.Input(Resource.String.Lbl_WriteMessage, 0, false, this);
                            dialog.InputType(InputTypes.TextFlagImeMultiLine);
                            dialog.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this);
                            dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                            dialog.Build().Show();
                            dialog.AlwaysCallSingleChoiceCallback();
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Invite friends to like this Page
        private void MembersLinerOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(InviteMembersPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent);
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
                                                    string pathImg;
                                                    switch (ImageType)
                                                    {
                                                        case "Cover":
                                                            {
                                                                pathImg = resultUri.Path;

                                                                //Set image
                                                                File file2 = new File(pathImg);
                                                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                                                switch (AppSettings.CoverImageStyle)
                                                                {
                                                                    case CoverImageStyle.CenterCrop:
                                                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                                                        break;
                                                                    case CoverImageStyle.FitCenter:
                                                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                                                        break;
                                                                    default:
                                                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                                                        break;
                                                                }
                                                                 
                                                                UpdateImagePage_Api(ImageType, pathImg);
                                                                break;
                                                            }
                                                        case "Avatar":
                                                            {
                                                                pathImg = resultUri.Path;

                                                                //Set image
                                                                File file2 = new File(pathImg);
                                                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                                Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(ProfileImage);
                                                                //GlideImageLoader.LoadImage(this, photoUri.ToString(), ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                                                var dataPage = PagesActivity.GetInstance()?.MAdapter?.SocialList.FirstOrDefault(a => a.Page?.PageId == PageId && a.TypeView == SocialModelType.MangedPages);
                                                                if (dataPage?.Page != null)
                                                                {
                                                                    dataPage.Page.Avatar = pathImg;
                                                                    PagesActivity.GetInstance()?.MAdapter?.NotifyDataSetChanged();

                                                                    var dataPage2 = ListUtils.MyPageList.FirstOrDefault(a => a.PageId == PageId);
                                                                    if (dataPage2 != null)
                                                                    {
                                                                        dataPage2.Avatar = pathImg;
                                                                    }
                                                                }

                                                                UpdateImagePage_Api(ImageType, pathImg);
                                                                break;
                                                            }
                                                    }

                                                    break;
                                                }
                                            default:
                                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
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
                            var diff = PostFeedAdapter?.ListDiffer;
                            var dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
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
                            var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
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
                    case 2005 when resultCode == Result.Ok:
                        {
                            var result = data.GetStringExtra("pageItem");
                            var item = JsonConvert.DeserializeObject<PageDataObject>(result ?? string.Empty);
                            if (item != null)
                                LoadPassedData(item);
                            break;
                        }
                    case 2019 when resultCode == Result.Ok:
                        { 
                            var manged = PagesActivity.GetInstance()?.MAdapter?.SocialList?.FirstOrDefault(a => a.Page?.PageId == PageId && a.TypeView == SocialModelType.MangedPages);
                            if (manged?.Page != null)
                            {
                                PagesActivity.GetInstance().MAdapter.SocialList.Remove(manged);
                                PagesActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                                ListUtils.MyPageList.Remove(manged.Page);
                                Finish();
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

        // Function Update Image Page : Avatar && Cover
        private async void UpdateImagePage_Api(string type, string path)
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
                                var (apiStatus, respond) = await RequestsAsync.Page.UpdatePageAvatarAsync(PageId, path).ConfigureAwait(false);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    ToastUtils.ShowToast(this,  GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                                    //GlideImageLoader.LoadImage(this, file.Path, ProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                                                    break;
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
                                var (apiStatus, respond) = await RequestsAsync.Page.UpdatePageCoverAsync(PageId, path).ConfigureAwait(false);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            if (respond is not MessageObject result)
                                                return;

                                            ToastUtils.ShowToast(this,  GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                            //GlideImageLoader.LoadImage(this, file.Path, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
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

        #region Get Data Page

        private void GetDataPage()
        {
            try
            {
                PageData = JsonConvert.DeserializeObject<PageDataObject>(Intent?.GetStringExtra("PageObject") ?? "");
                if (PageData != null)
                {
                    LoadPassedData(PageData);
                }

                PostFeedAdapter?.SetLoading();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            StartApiService();
        }

        private void LoadPassedData(PageDataObject pageData)
        {
            try
            {
                PageData = pageData;
                UserId = pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value ? UserDetails.UserId : pageData.UserId;

                GlideImageLoader.LoadImage(this, pageData.Avatar, ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    default:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                }
                 
                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value)
                {
                    LikeCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    LikeTxt.Text = GetText(Resource.String.Lbl_Edit);
                    LikeTxt.SetTextColor(Color.White);
                    LikeCardView.Tag = "MyPage";
                    //BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    LikeCardView.BackgroundTintList = WoWonderTools.IsLikedPage(pageData) ? ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor)) : AppSettings.SetTabDarkTheme ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                    LikeTxt.Text = GetText(WoWonderTools.IsLikedPage(pageData) ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                    LikeTxt.SetTextColor(WoWonderTools.IsLikedPage(pageData) ? Color.White : Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.BackgroundTintList = WoWonderTools.IsLikedPage(pageData) ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = WoWonderTools.IsLikedPage(pageData) ? ColorStateList.ValueOf(Color.Black) : ColorStateList.ValueOf(Color.White);
                    LikeCardView.Tag = "UserPage";
                }

                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value)
                {
                    EditAvatarImagePage.Visibility = ViewStates.Visible;
                    EditCoverImage.Visibility = ViewStates.Visible;
                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                    MessageCardView.Visibility = ViewStates.Gone;
                }
                else
                {
                    EditAvatarImagePage.Visibility = ViewStates.Gone;
                    EditCoverImage.Visibility = ViewStates.Gone;
                    FloatingActionButtonView.Visibility = ViewStates.Gone;
                    MessageCardView.Visibility = ViewStates.Visible;
                }

                var modelsClass = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoPageBox);
                switch (modelsClass)
                {
                    case null:
                        Combiner.InfoPageBox(new PageInfoModelClass { PageClass = pageData, PageId = pageData.PageId }, 0);
                        break;
                    default:
                        modelsClass.PageInfoModelClass = new PageInfoModelClass
                        {
                            PageClass = pageData,
                            PageId = pageData.PageId
                        };
                        PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(modelsClass));
                        break;
                }

                if (!string.IsNullOrEmpty(pageData.About))
                {
                    var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                    switch (checkAboutBox)
                    {
                        case null:
                            Combiner.AboutBoxPostView(Methods.FunString.DecodeString(pageData.About), 0);
                            break;
                        default:
                            checkAboutBox.AboutModel.Description = Methods.FunString.DecodeString(pageData.About);
                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkAboutBox));
                            break;
                    }
                }
                
                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value || pageData.UsersPost == "1")
                {
                    var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    switch (checkSection)
                    {
                        case null:
                            {
                                Combiner.AddPostBoxPostView("Page", -1, new PostDataObject { PageId = pageData.PageId, Publisher = new PublisherPost { PageName = pageData.PageName, Avatar = pageData.Avatar } });

                                //switch (AppSettings.ShowSearchForPosts)
                                //{
                                //    case true:
                                //        Combiner.SearchForPostsView("Page", new PostDataObject { PageId = pageData.PageId, Publisher = new PublisherPost { PageName = pageData.PageName, Avatar = pageData.Avatar } });
                                //        break;
                                //}

                                PostFeedAdapter?.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count - 1);
                                break;
                            }
                    }

                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                }

                TxtPageUsername.Text = "@" + pageData.Username;
                TxtPageName.Text = Methods.FunString.DecodeString(pageData.Name);

                TxtPageName.SetTextColor(Color.ParseColor(AppSettings.SetTabDarkTheme ? "#ffffff" : "#000000"));
                TxtPageUsername.SetTextColor(Color.ParseColor(AppSettings.SetTabDarkTheme ? "#ffffff" : "#C3C7D0"));

                SetCallActionsButtons(pageData);
                SetAdminInfo(pageData);

                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, pageData.Cover.Split('/').Last(), pageData.Cover);
                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, pageData.Avatar.Split('/').Last(), pageData.Avatar);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetCallActionsButtons(PageDataObject pageData)
        {
            try
            {

                if (pageData.CallActionType != "0" && !string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                {
                    var name = "Lbl_call_action_" + pageData.CallActionType;
                    var resourceId = Resources?.GetIdentifier(name, "string", ApplicationInfo?.PackageName) ?? 0;
                    ActionCardView.Visibility = ViewStates.Visible;
                    ActionTxt.Text = Resources?.GetString(resourceId);
                    switch (ActionCardView.HasOnClickListeners)
                    {
                        case false:
                            ActionCardView.Click += (sender, args) =>
                            {
                                try
                                {
                                    switch (string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                                    {
                                        case false:
                                            new IntentController(this).OpenBrowserFromApp(pageData.CallActionTypeUrl);
                                            break;
                                        default:
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_call_action_sorry), ToastLength.Short);
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };
                            break;
                    }
                }
                else
                {
                    ReplaceView(ActionCardView, LikeCardView);
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetAdminInfo(PageDataObject pageData)
        {
            try
            {
                if (pageData.AdminInfo?.AdminInfoClass != null && pageData.AdminInfo?.AdminInfoClass?.UserId == UserDetails.UserId)
                {
                    switch (pageData.AdminInfo?.AdminInfoClass.Avatar)
                    {
                        case "0":
                            EditCoverImage.Visibility = ViewStates.Gone;
                            EditAvatarImagePage.Visibility = ViewStates.Gone;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReplaceView(View currentView, View newView)
        {
            var parent = GetParent(currentView);
            switch (parent)
            {
                case null:
                    return;
            }
            var index = parent.IndexOfChild(currentView);
            RemoveView(currentView);
            RemoveView(newView);
            parent.AddView(newView, index);
        }

        private ViewGroup GetParent(View view)
        {
            return (ViewGroup)view.Parent;
        }

        private void RemoveView(View view)
        {
            var parent = GetParent(view);
            parent?.RemoveView(view);
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetPageDataApi, () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
        }

        private async Task GetPageDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Page.GetPageDataAsync(PageId);

            if (apiStatus != 200 || respond is not GetPageDataObject result || result.PageData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                PageData = result.PageData;
                LoadPassedData(PageData);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                var text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Settings))
                {
                    SettingsPage_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_BoostPage) || text == GetString(Resource.String.Lbl_UnBoostPage))
                {
                    BoostPageEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Reviews))
                {
                    ReviewsEvent();
                }
                else if (text == GetText(Resource.String.Lbl_ReportThisPage))
                {
                    OnReport_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_CancelReport))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                    {
                        PageData.IsReported = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.ReportPageAsync(PageId, "") });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Report
        private void OnReport_Button_Click()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(GetString(Resource.String.Lbl_ReportThisPage)).TitleColorRes(Resource.Color.primary);
                dialog.Input(Resource.String.text, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        switch (s.Length)
                        {
                            case <= 0:
                                return;
                        }
                        var text = s.ToString();

                        PageData.IsReported = true;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.ReportPageAsync(PageId, text) });
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_HasBeenReported), ToastLength.Short);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.InputType(InputTypes.TextFlagImeMultiLine);
                dialog.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        if (action == DialogAction.Positive)
                        {

                        }
                        else if (action == DialogAction.Negative)
                        {
                            materialDialog.Dismiss();
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

        public void OnInput(MaterialDialog dialog, string input)
        {
            //Send Message to page 
            try
            {
                if (input.Length > 0)
                {
                    var strName = input;

                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    if (!string.IsNullOrEmpty(strName) || !string.IsNullOrWhiteSpace(strName))
                    {
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var time = unixTimestamp.ToString();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.PageChat.SendMessageToPageChatAsync(PageId, UserId, time, strName) });
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully), ToastLength.Short);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_something_went_wrong), ToastLength.Short);
                    }
                }
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
                Methods.CopyToClipboard(this, PageData.Url);
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
                            Title = PageData.PageTitle,
                            Text = PageData.About,
                            Url = PageData.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Settings
        private void SettingsPage_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsPageActivity));
                intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                intent.PutExtra("PagesId", PageId);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void BoostPageEvent()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(this, typeof(GoProActivity));
                    StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    PageData.Boosted = "1";
                    //Sent Api 
                    var (apiStatus, respond) = await RequestsAsync.Page.BoostPageAsync(PageId);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject actionsObject when actionsObject.Message == "boosted":
                                        PageData.Boosted = "1";
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PageSuccessfullyBoosted), ToastLength.Short);
                                        break;
                                    case MessageObject actionsObject:
                                        PageData.Boosted = "0";
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
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReviewsEvent()
        {
            try
            {
                var intent = new Intent(this, typeof(ReviewsPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent);
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
                    //IconBack.SetColorFilter(Color.White);
                    //BtnMore.SetColorFilter(Color.White);
                    TxtSearchForPost.Visibility = ViewStates.Invisible;
                }
                else
                {
                    //IconBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
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