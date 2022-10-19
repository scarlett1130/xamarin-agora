using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using Refractored.Controls;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UserProfile;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers;
using WoWonder.SQLite;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using YouTubePlayerAndroidX.Player;
using String = Java.Lang.String;

namespace WoWonder.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/TransparentBlack", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ResizeableActivity = true)]
    public class YoutubePlayerActivity : BaseActivity, IYouTubePlayerInitListener, IYouTubePlayerFullScreenListener, StTools.IXAutoLinkOnClickListener, View.IOnClickListener, View.IOnLongClickListener
    {
        #region Variables Basic

        private CoordinatorLayout MainView;
         
        private CommentAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;

        private SwipeRefreshLayout SwipeRefreshLayout;

        public YouTubePlayerView TubePlayerView;
        public IYouTubePlayer YoutubePlayer { get; private set; }
        public YouTubePlayerEvents YouTubePlayerEvents;

        private string PostId;
        private PostDataObject PostObject;

        private CircleImageView UserAvatar;
        private SuperTextView Description;
        private TextViewWithImages Username;
        private AppCompatTextView TimeText, PrivacyPostIcon;
        private LinearLayout ShareLinearLayout, CommentLinearLayout, SecondReactionLinearLayout;
        private ImageView MoreIcon;
        private TextView SecondReactionButton, CommentCount, LikeCount;
        private LinearLayout MainSectionButton;
        private ReactButton LikeButton;
        private RelativeLayout PostExtrasLayout;
        private PostClickListener PostClickListener;
        private string TypeYouTubePlayerFullScreen = "RequestedOrientation";

        public static YoutubePlayerActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.TransparentBlack_Dark : Resource.Style.TransparentBlack);

                SetContentView(Resource.Layout.YoutubePlayerActivityLayout);

                Instance = this;

                PostId = Intent?.GetStringExtra("PostId") ?? string.Empty;
                PostObject = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostObject") ?? "");

                PostClickListener = new PostClickListener(this , NativeFeedType.Global);
                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                LoadPost();
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
         
        protected override void OnStop()
        {
            try
            {
                if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                    YoutubePlayer.Pause();

                base.OnStop();
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
                TubePlayerView?.Release();

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
                MainView = FindViewById<CoordinatorLayout>(Resource.Id.main_content);

                SecondReactionButton = FindViewById<TextView>(Resource.Id.SecondReactionText);
                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);

                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);

                CommentCount = FindViewById<TextView>(Resource.Id.Commentcount);
                LikeCount = FindViewById<TextView>(Resource.Id.Likecount);

                TimeText = FindViewById<AppCompatTextView>(Resource.Id.time_text);
                PrivacyPostIcon = FindViewById<AppCompatTextView>(Resource.Id.privacyPost);
                MoreIcon = FindViewById<ImageView>(Resource.Id.moreicon);

                Username = FindViewById<TextViewWithImages>(Resource.Id.username);
                UserAvatar = FindViewById<CircleImageView>(Resource.Id.userAvatar);
                Description = FindViewById<SuperTextView>(Resource.Id.description);

                PostExtrasLayout = FindViewById<RelativeLayout>(Resource.Id.postExtras);

                ShareLinearLayout = FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                CommentLinearLayout = FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                SecondReactionLinearLayout = FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                 
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycler_view);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                if (SecondReactionButton != null)
                {
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                        case PostButtonSystem.Like:
                            MainSectionButton.WeightSum = 3;
                            SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                            break;
                        case PostButtonSystem.Wonder:
                            MainSectionButton.WeightSum = 4;
                            SecondReactionLinearLayout.Visibility = ViewStates.Visible;

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                            SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                            break;
                        case PostButtonSystem.DisLike:
                            MainSectionButton.WeightSum = 4;
                            SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_action_dislike, 0, 0, 0);
                            SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                            break;
                    }
                }

                LikeButton.SetTextColor(Color.White);
                //if (LikeButton?.GetCurrentReaction()?.GetReactType() == ReactConstants.Default)
                //{
                //    LikeButton.CompoundDrawableTintList = ColorStateList.ValueOf(Color.White);
                //}

                TubePlayerView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
                if (TubePlayerView != null)
                {
                    //TubePlayerView.Visibility = ViewStates.Gone;

                    // The player will automatically release itself when the activity is destroyed.
                    // The player will automatically pause when the activity is paused
                    // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                    Lifecycle.AddObserver(TubePlayerView);

                    TubePlayerView.PlayerUiController.ShowMenuButton(false);

                    TubePlayerView.PlayerUiController.ShowCustomActionLeft1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionLeft2(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight2(false);
                     
                    //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                    TubePlayerView.Initialize(this, true);
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
                MAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                        UserAvatar?.SetOnClickListener(this);
                        Username?.SetOnClickListener(this);
                        CommentLinearLayout?.SetOnClickListener(this);
                        CommentCount?.SetOnClickListener(this);
                        ShareLinearLayout?.SetOnClickListener(this);
                        LikeButton?.SetOnClickListener(this);
                        LikeButton?.SetOnLongClickListener(this);
                        MoreIcon?.SetOnClickListener(this);
                        LikeCount?.SetOnClickListener(this);
                        SecondReactionButton?.SetOnClickListener(this);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 

        #endregion

        private void LoadPost()
        {
            try
            { 
                if (PostObject != null)
                {
                    var readMoreOption = new StReadMoreOption.Builder()
                        .TextLength(200, StReadMoreOption.TypeCharacter)
                        .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                        .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                        .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LabelUnderLine(true)
                        .Build();

                    if (SecondReactionButton != null)
                    {
                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.ReactionDefault:
                            case PostButtonSystem.ReactionSubShine:
                            case PostButtonSystem.Like:
                                MainSectionButton.WeightSum = 3;
                                SecondReactionButton.Visibility = ViewStates.Gone;
                                break;
                            case PostButtonSystem.Wonder:
                                MainSectionButton.WeightSum = 4;
                                SecondReactionButton.Visibility = ViewStates.Visible;

                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_action_wowonder, 0, 0, 0);
                                SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                                break;
                            case PostButtonSystem.DisLike:
                                MainSectionButton.WeightSum = 4;
                                SecondReactionButton.Visibility = ViewStates.Visible;

                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_action_dislike, 0, 0, 0);
                                SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                                break;
                            default:
                                MainSectionButton.WeightSum = 3;
                                SecondReactionButton.Visibility = ViewStates.Gone;
                                break;
                        }
                    }

                    var publisher = PostObject.Publisher ?? PostObject.UserData;
                    if (publisher != null)
                    {
                        switch (PostObject.PostPrivacy)
                        {
                            case "4":
                                Username.Text = GetText(Resource.String.Lbl_Anonymous);
                                GlideImageLoader.LoadImage(this, "user_anonymous", UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                break;
                            default:
                            {
                                GlideImageLoader.LoadImage(this, publisher.Avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                var postDataDecoratedContent = new WoTextDecorator().SetupStrings(PostObject, this);
                                Username.SetText(postDataDecoratedContent, TextView.BufferType.Spannable);
                                break;
                            }
                        }
                         
                        if (PostExtrasLayout != null)
                            PostExtrasLayout.Visibility = PostObject.IsPostBoosted == "0" ? ViewStates.Gone : ViewStates.Visible;

                        if (string.IsNullOrEmpty(PostObject.Orginaltext))
                        {
                            if (Description.Visibility != ViewStates.Gone)
                                Description.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            if (Description.Visibility != ViewStates.Visible)
                                Description.Visibility = ViewStates.Visible;

                            if (!Description.Text.Contains(GetText(Resource.String.Lbl_ReadMore)) && !Description.Text.Contains(GetText(Resource.String.Lbl_ReadLess)))
                            {
                                switch (PostObject.RegexFilterList != null & PostObject.RegexFilterList?.Count > 0)
                                {
                                    case true:
                                        Description.SetAutoLinkOnClickListener(this, PostObject.RegexFilterList);
                                        break;
                                    default:
                                        Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                                        break;
                                }

                                readMoreOption.AddReadMoreTo(Description, new String(PostObject.Orginaltext));
                            }
                            else if (Description.Text.Contains(GetText(Resource.String.Lbl_ReadLess)))
                            {
                                readMoreOption.AddReadLess(Description, new String(PostObject.Orginaltext));
                            }
                            else
                            {
                                Description.Text = PostObject.Orginaltext;
                            }
                        }

                        bool success = int.TryParse(PostObject.Time, out var number);
                        TimeText.Text = success ? Methods.Time.TimeAgo(number, false) : PostObject.Time;

                        if (PrivacyPostIcon != null && !string.IsNullOrEmpty(PostObject.PostPrivacy) && publisher.UserId == UserDetails.UserId)
                        {
                            switch (PostObject.PostPrivacy)
                            {
                                //Everyone
                                case "0":
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PrivacyPostIcon, FontAwesomeIcon.Globe);
                                    break;
                                default:
                                {
                                    if (PostObject.PostPrivacy.Contains("ifollow") || PostObject.PostPrivacy == "2") //People_i_Follow
                                    {
                                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PrivacyPostIcon, FontAwesomeIcon.User);
                                    }
                                    else if (PostObject.PostPrivacy.Contains("me") || PostObject.PostPrivacy == "1") //People_Follow_Me
                                    {
                                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PrivacyPostIcon, FontAwesomeIcon.UserFriends);
                                    }
                                    else switch (PostObject.PostPrivacy)
                                    {
                                        //Anonymous
                                        case "4":
                                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, PrivacyPostIcon, FontAwesomeIcon.UserSecret);
                                            break;
                                        //No_body) 
                                        default:
                                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PrivacyPostIcon, FontAwesomeIcon.Lock);
                                            break;
                                    }

                                    break;
                                }
                            }

                            PrivacyPostIcon.Visibility = ViewStates.Visible;
                        }

                        if (CommentCount != null)
                            CommentCount.Text = PostObject.PostComments; 
                    }

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                        {
                            PostObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                            if (LikeCount != null)
                                LikeCount.Text = PostObject?.Reaction?.Count + " " + GetString(Resource.String.Btn_Likes);

                            if (PostObject.Reaction.IsReacted != null && PostObject.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(PostObject.Reaction.Type))
                                {
                                    case false:
                                    {
                                        var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostObject.Reaction.Type).Value?.Id ?? "";
                                        switch (react)
                                        {
                                            case "1":
                                                LikeButton.SetReactionPack(ReactConstants.Like);
                                                break;
                                            case "2":
                                                LikeButton.SetReactionPack(ReactConstants.Love);
                                                break;
                                            case "3":
                                                LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                break;
                                            case "4":
                                                LikeButton.SetReactionPack(ReactConstants.Wow);
                                                break;
                                            case "5":
                                                LikeButton.SetReactionPack(ReactConstants.Sad);
                                                break;
                                            case "6":
                                                LikeButton.SetReactionPack(ReactConstants.Angry);
                                                break;
                                            default:
                                                LikeButton.SetReactionPack(ReactConstants.Default);
                                                break;
                                        }

                                        break;
                                    }
                                }
                            }
                            else
                                LikeButton.SetReactionPack(ReactConstants.Default);

                            break;
                        }
                        default:
                        {
                            if (PostObject.IsLiked != null && PostObject.IsLiked.Value)
                                LikeButton.SetReactionPack(ReactConstants.Like);

                            if (LikeCount != null)
                                LikeCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostObject.PostLikes)) + " " + GetString(Resource.String.Btn_Likes);

                            if (SecondReactionButton != null)
                            {
                                switch (AppSettings.PostButton)
                                {
                                    case PostButtonSystem.Wonder when PostObject.IsWondered != null && PostObject.IsWondered.Value:
                                    {
                                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                        switch (Build.VERSION.SdkInt)
                                        {
                                            case <= BuildVersionCodes.Lollipop:
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                break;
                                            default:
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                break;
                                        }

                                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                        SecondReactionButton.Text = GetString(Resource.String.Lbl_wondered);
                                        SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                        break;
                                    }
                                    case PostButtonSystem.Wonder:
                                    {
                                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                        switch (Build.VERSION.SdkInt)
                                        {
                                            case <= BuildVersionCodes.Lollipop:
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                break;
                                            default:
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                break;
                                        }
                                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                        SecondReactionButton.Text = GetString(Resource.String.Btn_Wonder);
                                        SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                        break;
                                    }
                                    case PostButtonSystem.DisLike when PostObject.IsWondered != null && PostObject.IsWondered.Value:
                                    {
                                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                        switch (Build.VERSION.SdkInt)
                                        {
                                            case <= BuildVersionCodes.Lollipop:
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                break;
                                            default:
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                break;
                                        }

                                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                        SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                                        SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                        break;
                                    }
                                    case PostButtonSystem.DisLike:
                                    {
                                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                        switch (Build.VERSION.SdkInt)
                                        {
                                            case <= BuildVersionCodes.Lollipop:
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                break;
                                            default:
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                break;
                                        }

                                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                        SecondReactionButton.Text = GetString(Resource.String.Btn_Dislike);
                                        SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    }

                    StartApiService();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }

        #region YouTube

        public void OnInitSuccess(IYouTubePlayer player)
        {
            try
            {
                if (YoutubePlayer == null)
                {
                    YoutubePlayer = player;
                    YouTubePlayerEvents = new YouTubePlayerEvents(player, PostObject.PostYoutube , "YoutubePlayerActivity");
                    YoutubePlayer.AddListener(YouTubePlayerEvents);
                    TubePlayerView.AddFullScreenListener(this); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerEnterFullScreen()
        {
            try
            {
                Intent intent = new Intent(this, typeof(YouTubePlayerFullScreenActivity));
                intent.PutExtra("type", TypeYouTubePlayerFullScreen);
                intent.PutExtra("VideoIdYoutube", PostObject.PostYoutube);
                intent.PutExtra("CurrentSecond", YouTubePlayerEvents.CurrentSecond);
                StartActivityForResult(intent, 2100);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerExitFullScreen()
        {
            try
            {
                TypeYouTubePlayerFullScreen = "RequestedOrientation";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        public void OnClick(View v)
        {
            try
            {
                var postType = PostFunctions.GetAdapterType(PostObject);

                if (v.Id == Username.Id)
                    PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = PostObject, View = MainView }, "NewsFeedClass", "Username");
                else if (v.Id == UserAvatar.Id)
                    PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = PostObject, View = MainView }, "NewsFeedClass", "UserAvatar");
                else if (v.Id == MoreIcon.Id)
                    PostClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView }, "YoutubePlayer");
                else if (v.Id == LikeButton.Id)
                    LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView }, null);
                else if (v.Id == CommentLinearLayout.Id)
                    PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView });
                else if (v.Id == ShareLinearLayout.Id)
                    PostClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView }, postType);
                else if (v.Id == SecondReactionButton.Id)
                    PostClickListener.SecondReactionButtonClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView });
                else if (v.Id == LikeCount.Id)
                    PostClickListener.DataItemPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView });
                else if (v.Id == CommentCount.Id)
                    PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public bool OnLongClick(View v)
        {
            switch (AppSettings.PostButton)
            {
                //add event if System = ReactButton 
                case PostButtonSystem.ReactionDefault:
                case PostButtonSystem.ReactionSubShine:
                {
                    if (LikeButton.Id == v.Id)
                        LikeButton.LongClickDialog(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView }, null);
                    break;
                }
            }

            return true;
        }
         
        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                p1 = p1.Replace(" ", "").Replace("\n", "");
                var typeText = Methods.FunString.Check_Regex(p1);
                switch (typeText)
                {
                    case "Email":
                        Methods.App.SendEmail(this, p1);
                        break;
                    case "Website":
                    {
                        string url = p1.Contains("http") switch
                        {
                            false => "http://" + p1,
                            _ => p1
                        };

                        //var intent = new Intent(this, typeof(LocalWebViewActivity));
                        //intent.PutExtra("URL", url.Replace(" ", ""));
                        //intent.PutExtra("Type", url.Replace(" ", ""));
                        //this.StartActivity(intent);
                        new IntentController(this).OpenBrowserFromApp(url);
                        break;
                    }
                    case "Hashtag":
                    {
                        var intent = new Intent(this, typeof(HashTagPostsActivity));
                        intent.PutExtra("Id", p1);
                        intent.PutExtra("Tag", p1);
                        StartActivity(intent);
                        break;
                    }
                    case "Mention":
                    {
                        var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                        string name = p1.Replace("@", "").Replace(" ", "");

                        var sqlEntity = new SqLiteDatabase();
                        var user = sqlEntity.Get_DataOneUser(name);
                    

                        if (user != null)
                        {
                            WoWonderTools.OpenProfile(this, user.UserId, user);
                        }
                        else switch (userData?.Count)
                        {
                            case > 0:
                            {
                                var data = userData.FirstOrDefault(a => a.Value == name);
                                if (data.Key != null && data.Key == UserDetails.UserId)
                                {
                                    switch (PostClickListener.OpenMyProfile)
                                    {
                                        case true:
                                            return;
                                        default:
                                        {
                                            var intent = new Intent(this, typeof(MyProfileActivity));
                                            StartActivity(intent);
                                            break;
                                        }
                                    }
                                }
                                else if (data.Key != null)
                                {
                                    var intent = new Intent(this, typeof(UserProfileActivity));
                                    //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                    intent.PutExtra("UserId", data.Key);
                                    StartActivity(intent);
                                }

                                break;
                            }
                            default:
                            {
                                if (name == dataUSer?.Name || name == dataUSer?.Username)
                                {
                                    switch (PostClickListener.OpenMyProfile)
                                    {
                                        case true:
                                            return;
                                        default:
                                        {
                                            var intent = new Intent(this, typeof(MyProfileActivity));
                                            StartActivity(intent);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    var intent = new Intent(this, typeof(UserProfileActivity));
                                    //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                    intent.PutExtra("name", name);
                                    StartActivity(intent);
                                }

                                break;
                            }
                        }

                        break;
                    }
                    case "Number":
                        Methods.App.SaveContacts(this, p1, "", "2");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Result

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
               
                switch (requestCode)
                { 
                    case 2100 when resultCode == Result.Ok:
                        {
                            TubePlayerView?.ExitFullScreen();
                            break;
                        } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Comment

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() =>  LoadDataComment(offset) });
        }

        private async Task LoadDataComment(string offset = "0")
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var (apiStatus, respond) = await RequestsAsync.Comment.GetPostCommentsAsync(PostId, "10", offset);
                if (apiStatus != 200 || respond is not CommentObject result || result.CommentList == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.CommentList?.Count;
                    switch (respondList)
                    {
                        case > 0:
                            {
                                foreach (var item in result.CommentList)
                                {
                                    CommentObjectExtra check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id);
                                    switch (check)
                                    {
                                        case null:
                                            {
                                                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                                if (db != null) MAdapter.CommentList.Add(db);
                                                break;
                                            }
                                        default:
                                            check = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                            check.Replies = item.Replies;
                                            check.RepliesCount = item.RepliesCount;
                                            break;
                                    }
                                }

                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                break;
                            }
                    }
                }
            }
        }

        #endregion

    }
}