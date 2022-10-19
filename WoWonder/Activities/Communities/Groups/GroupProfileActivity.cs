using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS; 
using Android.Views;
using Android.Widget;
using TheArtOfDev.Edmodo.Cropper;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.Text;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Groups.Settings;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
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
using WoWonder.Activities.SearchForPosts;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class GroupProfileActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;
         
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ImageButton BtnMore, IconBack;
        private ImageView UserProfileImage, CoverImage;
        private TextView TxtSearchForPost, TxtGroupName, TxtGroupUsername;
        private FloatingActionButton FloatingActionButtonView;
        private RelativeLayout EditAvatarImageGroup, EditCoverImageLayout;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private string GroupId, ImageType;
        public static GroupDataObject GroupDataClass;
        private ImageView JoinRequestImage1, JoinRequestImage2, JoinRequestImage3;
        private RelativeLayout LayoutJoinRequest;
        private FeedCombiner Combiner;
        private static GroupProfileActivity Instance;
        private AdView MAdView;
        private CardView JoinCardView;
        private TextView TxtJoin;

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
                SetContentView(Resource.Layout.Group_Profile_Layout);

                Instance = this;

                GroupId = Intent?.GetStringExtra("GroupId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                GetDataGroup();
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

                UserProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);

                IconBack = (ImageButton)FindViewById(Resource.Id.image_back);
                EditAvatarImageGroup = (RelativeLayout)FindViewById(Resource.Id.LinearEdit);
                EditCoverImageLayout = (RelativeLayout)FindViewById(Resource.Id.cover_layout);
                TxtGroupName = (TextView)FindViewById(Resource.Id.Group_name);
                TxtGroupUsername = (TextView)FindViewById(Resource.Id.Group_Username);
                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Gone;

                JoinRequestImage1 = (ImageView)FindViewById(Resource.Id.image_page_1);
                JoinRequestImage2 = (ImageView)FindViewById(Resource.Id.image_page_2);
                JoinRequestImage3 = (ImageView)FindViewById(Resource.Id.image_page_3);

                LayoutJoinRequest = (RelativeLayout)FindViewById(Resource.Id.layout_join_Request);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);

                // Join Button with cardview
                JoinCardView = FindViewById<CardView>(Resource.Id.joinButton);
                TxtJoin = FindViewById<TextView>(Resource.Id.joinTxt); 
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
                PostFeedAdapter = new NativePostAdapter(this, GroupId, MainRecyclerView, NativeFeedType.Group);
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
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        IconBack.Click += IconBackOnClick;
                        EditCoverImageLayout.Click += TxtEditGroupInfoOnClick;
                        EditAvatarImageGroup.Click += EditAvatarImageGroupOnClick;
                        JoinCardView.Click += BtnJoinOnClick;
                        BtnMore.Click += BtnMoreOnClick;
                        FloatingActionButtonView.Click += AddPostOnClick;
                        LayoutJoinRequest.Click += LayoutJoinRequestOnClick;
                        UserProfileImage.Click += UserProfileImageOnClick;
                        CoverImage.Click += CoverImageOnClick;
                        TxtSearchForPost.Click += TxtSearchForPostOnClick;
                        break;
                    default:
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        IconBack.Click -= IconBackOnClick;
                        EditCoverImageLayout.Click -= TxtEditGroupInfoOnClick;
                        EditAvatarImageGroup.Click -= EditAvatarImageGroupOnClick;
                        JoinCardView.Click -= BtnJoinOnClick;
                        BtnMore.Click -= BtnMoreOnClick;
                        FloatingActionButtonView.Click -= AddPostOnClick;
                        LayoutJoinRequest.Click -= LayoutJoinRequestOnClick;
                        UserProfileImage.Click -= UserProfileImageOnClick;
                        CoverImage.Click -= CoverImageOnClick;
                        TxtSearchForPost.Click -= TxtSearchForPostOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static GroupProfileActivity GetInstance()
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
                JoinCardView = null!;
                TxtJoin = null!;
                BtnMore = null!;
                UserProfileImage = null!;
                CoverImage = null!;
                IconBack = null!;
                TxtGroupName = null!;
                TxtGroupUsername = null!;
                EditCoverImageLayout = null!;
                FloatingActionButtonView = null!;
                EditAvatarImageGroup = null!;
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                GroupId = null!;
                ImageType = null!;
                GroupDataClass = null!;
                JoinRequestImage1 = null!;
                JoinRequestImage2 = null!;
                JoinRequestImage3 = null!;
                LayoutJoinRequest = null!;
                Combiner = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtSearchForPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchForPostsActivity));
                intent.PutExtra("TypeSearch", "group");
                intent.PutExtra("IdSearch", GroupId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

                GetDataGroup();
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
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, GroupDataClass.Cover.Split('/').Last(), GroupDataClass.Cover);
                if (media.Contains("http"))
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    File file2 = new File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    Intent intent = new Intent(Intent.ActionPick);
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
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, GroupDataClass.Avatar.Split('/').Last(), GroupDataClass.Avatar);
                if (media.Contains("http"))
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    File file2 = new File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    Intent intent = new Intent(Intent.ActionPick);
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

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Groups)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                if (GroupDataClass.IsOwner != null && GroupDataClass.IsOwner.Value)
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Members));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Settings));
                }

                if (GroupDataClass.IsReported != null && GroupDataClass.IsReported.Value)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_CancelReport));
                else
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ReportThisGroup));

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

        //Event Add New post
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialGroup");
                intent.PutExtra("PostId", GroupId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Join_Group => Joined , Join Group
        private async void BtnJoinOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (JoinCardView?.Tag?.ToString())
                {
                    case "MyGroup":
                        SettingGroup_OnClick();
                        break;
                    default:
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                            else
                            { 
                                var (apiStatus, respond) = await RequestsAsync.Group.JoinGroupAsync(GroupId);
                                if (apiStatus == 200)
                                {
                                    if (respond is JoinGroupObject result1)
                                    { 
                                        //Set style Btn Joined Group 
                                        if (result1.JoinStatus == "joined") //joined
                                        {
                                            JoinCardView.BackgroundTintList = AppSettings.SetTabDarkTheme ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE")); 
                                            TxtJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                            TxtJoin.Text = GetText(Resource.String.Btn_Joined);
                                            TxtJoin.Tag = "1";
                                        }
                                        else if (result1.JoinStatus == "requested") //requested
                                        {
                                            JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                            TxtJoin.SetTextColor(Color.White);
                                            TxtJoin.Text = GetText(Resource.String.Lbl_Request);
                                            JoinCardView.Tag = "2";
                                        }
                                        else //not joined
                                        {
                                            JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                            TxtJoin.SetTextColor(Color.White);
                                            TxtJoin.Text = GetText(Resource.String.Btn_Join_Group);
                                            JoinCardView.Tag = "0";
                                        } 
                                    } 
                                }
                                else
                                    Methods.DisplayReportResult(this, respond);
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

        //Event Update Image Cover Group
        private void TxtEditGroupInfoOnClick(object sender, EventArgs e)
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

        //Event Update Image avatar Group
        private void EditAvatarImageGroupOnClick(object sender, EventArgs e)
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

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        //Join Request
        private void LayoutJoinRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(JoinRequestActivity));
                intent.PutExtra("GroupId", GroupId);
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

                                                                            UpdateImageGroup_Api(ImageType, pathImg);
                                                                            break;
                                                                        }
                                                                    case "Avatar":
                                                                        {
                                                                            pathImg = resultUri.Path;

                                                                            //Set image
                                                                            File file2 = new File(pathImg);
                                                                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                                            Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(UserProfileImage);

                                                                            var dataGroup = GroupsActivity.GetInstance()?.MAdapter.SocialList?.FirstOrDefault(a => a.Group?.GroupId == GroupId && a.TypeView == SocialModelType.MangedGroups);
                                                                            if (dataGroup?.Group != null)
                                                                            {
                                                                                dataGroup.Group.Avatar = pathImg;
                                                                                GroupsActivity.GetInstance()?.MAdapter?.NotifyDataSetChanged();

                                                                                var dataGroup2 = ListUtils.MyGroupList.FirstOrDefault(a => a.GroupId == GroupId);
                                                                                if (dataGroup2 != null)
                                                                                {
                                                                                    dataGroup2.Avatar = pathImg;
                                                                                }
                                                                            }

                                                                            UpdateImageGroup_Api(ImageType, pathImg);
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
                            string result = data.GetStringExtra("groupItem") ?? "";
                            var item = JsonConvert.DeserializeObject<GroupDataObject>(result);
                            if (item != null)
                                LoadPassedData(item);
                            break;
                        }
                    case 2019 when resultCode == Result.Ok:
                        {
                            var manged = GroupsActivity.GetInstance()?.MAdapter?.SocialList?.FirstOrDefault(a => a.Group?.GroupId == GroupId && a.TypeView == SocialModelType.MangedGroups);
                            if (manged?.Group != null)
                            {
                                GroupsActivity.GetInstance().MAdapter.SocialList.Remove(manged);
                                GroupsActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                                ListUtils.MyGroupList.Remove(manged.Group);
                            }
                             
                            Finish();
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

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Members))
                {
                    MembersGroup_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_Settings))
                {
                    SettingGroup_OnClick();
                }
                else if (text == GetText(Resource.String.Lbl_ReportThisGroup))
                {
                    OnReport_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_CancelReport))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                    {
                        GroupDataClass.IsReported = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.ReportGroupAsync(GroupId, "") });
                    }
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

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, GroupDataClass.Url);
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
                            Title = GroupDataClass.GroupName,
                            Text = GroupDataClass.About,
                            Url = GroupDataClass.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Setting
        private void MembersGroup_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(GroupMembersActivity));
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Setting
        private void SettingGroup_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsGroupActivity));
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
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
                dialog.Title(GetString(Resource.String.Lbl_ReportThisGroup)).TitleColorRes(Resource.Color.primary);
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

                        GroupDataClass.IsReported = true;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.ReportGroupAsync(GroupId, text) });
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


        #endregion

        #region Get Data Group

        private void GetDataGroup()
        {
            try
            {
                GroupDataClass = JsonConvert.DeserializeObject<GroupDataObject>(Intent?.GetStringExtra("GroupObject") ?? "");
                if (GroupDataClass != null)
                {
                    LoadPassedData(GroupDataClass);
                }

                PostFeedAdapter?.SetLoading();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            StartApiService();
        }

        private void LoadPassedData(GroupDataObject result)
        {
            try
            {
                GlideImageLoader.LoadImage(this, result.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    default:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                }

                TxtGroupUsername.Text = "@" + Methods.FunString.DecodeString(result.Username);
                TxtGroupName.Text = Methods.FunString.DecodeString(result.Name);

                TxtGroupName.SetTextColor(Color.ParseColor(AppSettings.SetTabDarkTheme ? "#ffffff" : "#000000"));
                TxtGroupUsername.SetTextColor(Color.ParseColor(AppSettings.SetTabDarkTheme ? "#ffffff" : "#C3C7D0"));

                if (result.UserId == UserDetails.UserId)
                    result.IsOwner = true;

                if (result.IsOwner != null && result.IsOwner.Value)
                {
                    JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    TxtJoin.Text = GetText(Resource.String.Lbl_Edit);
                    TxtJoin.SetTextColor(Color.White);
                    JoinCardView.Tag = "MyGroup";
                    //BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    //Set style Btn Joined Group 
                    if (WoWonderTools.IsJoinedGroup(result) == "1") //joined
                    {
                        JoinCardView.BackgroundTintList = AppSettings.SetTabDarkTheme ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE")); 
                        TxtJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.Text = GetText(Resource.String.Btn_Joined);
                        TxtJoin.Tag = "1";
                    }
                    else if (WoWonderTools.IsJoinedGroup(result) == "2") //requested
                    {
                        JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.SetTextColor(Color.White);
                        TxtJoin.Text = GetText(Resource.String.Lbl_Request);
                        JoinCardView.Tag = "2";
                    }
                    else //not joined
                    {
                        JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.SetTextColor(Color.White);
                        TxtJoin.Text = GetText(Resource.String.Btn_Join_Group);
                        JoinCardView.Tag = "0";
                    }

                    //BtnMore.BackgroundTintList = WoWonderTools.IsJoinedGroup(result) ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = WoWonderTools.IsJoinedGroup(result) ? ColorStateList.ValueOf(Color.Black) : ColorStateList.ValueOf(Color.White);
                }

                var modelsClass = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoGroupBox);
                switch (modelsClass)
                {
                    case null:
                        Combiner.InfoGroupBox(new GroupPrivacyModelClass { GroupClass = result, GroupId = result.GroupId }, 0);
                        break;
                    default:
                        modelsClass.PrivacyModelClass = new GroupPrivacyModelClass
                        {
                            GroupClass = result,
                            GroupId = result.GroupId
                        };
                        PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(modelsClass));
                        break;
                }

                if (!string.IsNullOrEmpty(result.About))
                {
                    var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                    switch (checkAboutBox)
                    {
                        case null:
                            Combiner.AboutBoxPostView(Methods.FunString.DecodeString(result.About), 0);
                            break;
                        default:
                            checkAboutBox.AboutModel.Description = Methods.FunString.DecodeString(result.About);
                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkAboutBox));
                            break;
                    }
                }
                
                if (result.IsOwner != null && result.IsOwner.Value || WoWonderTools.IsJoinedGroup(result) == "1")
                {
                    var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    switch (checkSection)
                    {
                        case null:
                            {
                                Combiner.AddPostBoxPostView("Group", -1, new PostDataObject { GroupRecipient = result });

                                //switch (AppSettings.ShowSearchForPosts)
                                //{
                                //    case true:
                                //        Combiner.SearchForPostsView("Group", new PostDataObject { GroupRecipient = result });
                                //        break;
                                //}

                                PostFeedAdapter?.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count - 1);
                                break;
                            }
                    }
                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                }

                if (result.IsOwner != null && result.IsOwner.Value)
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Visible;
                    //TxtEditGroupInfo.Visibility = ViewStates.Visible;

                    EditCoverImageLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Gone;
                    //TxtEditGroupInfo.Visibility = ViewStates.Gone;

                    EditCoverImageLayout.Visibility = ViewStates.Gone;
                }

                if (WoWonderTools.IsJoinedGroup(result) == "1" || result.Privacy == "1" || result.IsOwner != null && result.IsOwner.Value)
                {
                    //Console.WriteLine("======= RunRetryPolicyFunction " + WoWonderTools.IsJoinedGroup(result) + " " + result.Privacy + " =========");

                    if (WoWonderTools.IsJoinedGroup(result) == "0" && result.Privacy == "1" && result.IsOwner != null && !result.IsOwner.Value)
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts(visible: false) });
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
                }
                else
                {
                    //Console.WriteLine("======= Not RunRetryPolicyFunction =========");

                    PostFeedAdapter?.SetLoaded();

                     var viewProgress = PostFeedAdapter?.ListDiffer?.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                     if (viewProgress != null)
                         MainRecyclerView.RemoveByRowIndex(viewProgress);

                     var emptyStateCheck = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox && a.TypeView != PostModelType.InfoGroupBox);
                     if (emptyStateCheck != null)
                     {
                         var emptyStateChecker = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                         if (emptyStateChecker != null && PostFeedAdapter?.ListDiffer?.Count > 1)
                             PostFeedAdapter?.ListDiffer?.Remove(emptyStateChecker);
                     }
                     else
                     {
                         var emptyStateChecker = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                         switch (emptyStateChecker)
                         {
                             case null:
                                 PostFeedAdapter?.ListDiffer?.Add(new AdapterModelsClass { TypeView = PostModelType.EmptyState, Id = 744747447 });
                                 break;
                         }
                     }
                     PostFeedAdapter?.NotifyDataSetChanged();
                }

                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Cover.Split('/').Last(), result.Cover);
                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Avatar.Split('/').Last(), result.Avatar);
                SwipeRefreshLayout.Refreshing = false;
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetGroupDataApi, GetJoin });
        }

        private async Task GetGroupDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Group.GetGroupDataAsync(GroupId);

            if (apiStatus != 200 || respond is not GetGroupDataObject result || result.GroupData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                GroupDataClass = result.GroupData;
                RunOnUiThread(() => { LoadPassedData(GroupDataClass); });
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

        // Function Update Image Group : Avatar && Cover
        private async void UpdateImageGroup_Api(string type, string path)
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
                                var (apiStatus, respond) = await RequestsAsync.Group.UpdateGroupAvatarAsync(GroupId, path).ConfigureAwait(false);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);
                                                    //GlideImageLoader.LoadImage(this, file.Path, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
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
                                var (apiStatus, respond) = await RequestsAsync.Group.UpdateGroupCoverAsync(GroupId, path).ConfigureAwait(false);
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

        private async Task GetJoin()
        {
            if (GroupDataClass.UserId == UserDetails.UserId)
            {
                var (apiStatus, respond) = await RequestsAsync.Group.GetGroupJoinRequestsAsync(GroupId, "5");
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case GetGroupJoinRequestsObject result:
                                    RunOnUiThread(() =>
                                    {
                                        var respondList = result.Data.Count;
                                        switch (respondList)
                                        {
                                            case > 0:
                                                LayoutJoinRequest.Visibility = ViewStates.Visible;
                                                try
                                                {
                                                    var list = result.Data.TakeLast(4).ToList();

                                                    for (var i = 0; i < list.Count; i++)
                                                    {
                                                        var item = list[i];
                                                        switch (item)
                                                        {
                                                            case null:
                                                                continue;
                                                            default:
                                                                switch (i)
                                                                {
                                                                    case 0:
                                                                        GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                                        break;
                                                                    case 1:
                                                                        GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                                        break;
                                                                    case 2:
                                                                        GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                                        break;
                                                                }

                                                                break;
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }

                                                break;
                                            default:
                                                LayoutJoinRequest.Visibility = ViewStates.Gone;
                                                break;
                                        }
                                    });
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
                RunOnUiThread(() => { LayoutJoinRequest.Visibility = ViewStates.Gone; });
            }
        }
    }
}