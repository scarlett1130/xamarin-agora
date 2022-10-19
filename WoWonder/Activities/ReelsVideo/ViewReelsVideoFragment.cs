using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Drm;
using Com.Google.Android.Exoplayer2.Extractor.TS;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream; 
using Newtonsoft.Json;
using WoWonder.Activities.Gift;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers;
using WoWonder.SQLite;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using YouTubePlayerAndroidX.Player;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;
using Uri = Android.Net.Uri;
using Util = Com.Google.Android.Exoplayer2.Util.Util;

namespace WoWonder.Activities.ReelsVideo
{ 
    public class ViewReelsVideoFragment : AndroidX.Fragment.App.Fragment, IYouTubePlayerInitListener, IPlayerEventListener, StTools.IXAutoLinkOnClickListener
    {
        #region Variables Basic

        private ReelsVideoDetailsActivity GlobalContext;
        private static ViewReelsVideoFragment Instance;

        private StReadMoreOption ReadMoreOption;
        private PostDataObject DataVideos;
        private PostModelType PostFeedType;

        private ImageView IconBack;

        private View MainView;
        private FrameLayout Root;

        private SimpleExoPlayer VideoPlayer;
        private PlayerView PlayerView;

        public YouTubePlayerView TubePlayerView;
        private IYouTubePlayer YoutubePlayer { get; set; }
        private YouTubePlayerEvents YouTubePlayerEvents;
        private string VideoIdYoutube;
        private PostClickListener ClickListener;

        public LinearLayout LikeLayout;
        private LinearLayout UserLayout , GiftLayout, CommentLayout, ShareLayout;
        public ImageView  ImgLike;
        private ImageView UserImageView , ImgSendGift, ImgComment, ImgShare;
        public TextView TxtLikeCount;
        private TextView TxtUsername , TxtCommentCount, TxtShareCount;

        private SuperTextView TxtDescription;

        private bool MIsVisibleToUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (ReelsVideoDetailsActivity)Activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                MainView = inflater.Inflate(Resource.Layout.ReelsVideoSwipeLayout, container, false);
                return MainView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            { 
                base.OnViewCreated(view, savedInstanceState);
                MainView = view;
                 
                Instance = this;
                InitComponent(view);
                SetPlayer();

                ClickListener = new PostClickListener(GlobalContext, NativeFeedType.Global);

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(Activity.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(Activity.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                  
                if (IsResumed && MIsVisibleToUser)
                {
                    //var position = Arguments.GetInt("position", 0); 
                    DataVideos = JsonConvert.DeserializeObject<PostDataObject>(Arguments.GetString("DataItem") ?? "");
                    if (DataVideos != null)
                    {
                        LoadData(DataVideos);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();

                if (MIsVisibleToUser)
                    StopVideo();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroyView()
        {
            try
            {
                ReleaseVideo();
                
                base.OnDestroyView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                ReleaseVideo();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<ImageView>(Resource.Id.back);
                Root = view.FindViewById<FrameLayout>(Resource.Id.root);
                PlayerView = view.FindViewById<PlayerView>(Resource.Id.player_view);
                  
                UserLayout = view.FindViewById<LinearLayout>(Resource.Id.userLayout);
                UserImageView = view.FindViewById<ImageView>(Resource.Id.imageAvatar);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.username);
                 
                TxtDescription = view.FindViewById<SuperTextView>(Resource.Id.tv_descreption);

                GiftLayout = view.FindViewById<LinearLayout>(Resource.Id.GiftLayout);
                ImgSendGift = view.FindViewById<ImageView>(Resource.Id.img_sendGift);

                LikeLayout = view.FindViewById<LinearLayout>(Resource.Id.likeLayout);
                ImgLike = view.FindViewById<ImageView>(Resource.Id.img_like);
                TxtLikeCount = view.FindViewById<TextView>(Resource.Id.tv_likeCount);
                LikeLayout.Tag = "Like";

                CommentLayout = view.FindViewById<LinearLayout>(Resource.Id.commentLayout);
                ImgComment = view.FindViewById<ImageView>(Resource.Id.img_comment);
                TxtCommentCount = view.FindViewById<TextView>(Resource.Id.tv_comment_count);
                
                ShareLayout = view.FindViewById<LinearLayout>(Resource.Id.shareLayout);
                ImgShare = view.FindViewById<ImageView>(Resource.Id.img_share);
                TxtShareCount = view.FindViewById<TextView>(Resource.Id.tv_share_count);

                TubePlayerView = view.FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
                if (TubePlayerView != null)
                {
                    TubePlayerView.Visibility = ViewStates.Gone;

                    // The player will automatically release itself when the activity is destroyed.
                    // The player will automatically pause when the activity is paused
                    // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                    Lifecycle.AddObserver(TubePlayerView);

                    TubePlayerView.PlayerUiController.ShowMenuButton(false);

                    TubePlayerView.PlayerUiController.ShowCustomActionLeft1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionLeft2(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight2(false);

                    TubePlayerView.PlayerUiController.ShowFullscreenButton(false);

                    //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                }

                if (!AppSettings.ShowGift)
                {
                    GiftLayout.Visibility = ViewStates.Gone;
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
                // true +=  // false -=
                if (addEvent)
                {
                    IconBack.Click += IconBackOnClick;
                    UserLayout.Click += UserLayoutOnClick;
                    GiftLayout.Click += GiftLayoutOnClick;
                    LikeLayout.Click += LikeLayoutOnClick;
                    CommentLayout.Click += CommentLayoutOnClick;
                    ShareLayout.Click += ShareLayoutOnClick;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    UserLayout.Click -= UserLayoutOnClick;
                    GiftLayout.Click -= GiftLayoutOnClick;
                    LikeLayout.Click -= LikeLayoutOnClick;
                    CommentLayout.Click -= CommentLayoutOnClick;
                    ShareLayout.Click -= ShareLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewReelsVideoFragment GetInstance()
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

        #region Event

        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShareLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = DataVideos, }, PostFeedType); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = DataVideos,
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (LikeLayout.Tag?.ToString() == "Liked")
                {
                    DataVideos.Reaction ??= new Reaction();

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                            {
                                if (DataVideos.Reaction != null)
                                {
                                    switch (DataVideos.Reaction.Count)
                                    {
                                        case > 0:
                                            DataVideos.Reaction.Count--;
                                            break;
                                        default:
                                            DataVideos.Reaction.Count = 0;
                                            break;
                                    }

                                    DataVideos.Reaction.Type = "";
                                    DataVideos.Reaction.IsReacted = false;
                                }
                                TxtLikeCount.Text = Methods.FunString.FormatPriceValue(DataVideos.Reaction.Count);
                                break;
                            }
                        default:
                            {
                                var x = Convert.ToInt32(DataVideos.PostLikes);
                                switch (x)
                                {
                                    case > 0:
                                        x--;
                                        break;
                                    default:
                                        x = 0;
                                        break;
                                }

                                DataVideos.IsLiked = false;
                                DataVideos.PostLikes = Convert.ToString(x, CultureInfo.InvariantCulture);
                                TxtLikeCount.Text = DataVideos.PostLikes;
                                break;
                            }
                    }
                     
                    ImgLike.SetImageResource(Resource.Drawable.icon_post_like_vector);
                    ImgLike.SetColorFilter(Color.White);
                    LikeLayout.Tag = "Like";

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataVideos.PostId, "reaction") });
                            break;
                        default:
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataVideos.PostId, "like") });
                            break;
                    }
                }
                else
                {
                    new ReactionReelsVideo(Activity)?.ClickDialog(new GlobalClickEventArgs
                    {
                        NewsFeedClass = DataVideos
                    });
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GiftLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", DataVideos.UserId);

                GiftDialogFragment mGiftFragment = new GiftDialogFragment
                {
                    Arguments = bundle
                };

                mGiftFragment.Show(ChildFragmentManager, mGiftFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(Activity, DataVideos.UserId, DataVideos.Publisher);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
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
                        Methods.App.SendEmail(Activity, p1);
                        break;
                    case "Website":
                        {
                            string url = p1.Contains("http") switch
                            {
                                false => "http://" + p1,
                                _ => p1
                            };

                            //var intent = new Intent(MainContext, typeof(LocalWebViewActivity));
                            //intent.PutExtra("URL", url);
                            //intent.PutExtra("Type", url);
                            //MainContext.StartActivity(intent);
                            new IntentController(Activity).OpenBrowserFromApp(url);
                            break;
                        }
                    case "Hashtag":
                        {
                            var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                            intent.PutExtra("Id", p1);
                            intent.PutExtra("Tag", p1);
                            Activity.StartActivity(intent);
                            break;
                        }
                    case "Mention":
                        {
                            var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                            string name = p1.Replace("@", "");

                            var sqlEntity = new SqLiteDatabase();
                            var user = sqlEntity.Get_DataOneUser(name);


                            if (user != null)
                            {
                                WoWonderTools.OpenProfile(Activity, user.UserId, user);
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
                                                            var intent = new Intent(Activity, typeof(MyProfileActivity));
                                                            Activity.StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else if (data.Key != null)
                                            {
                                                var intent = new Intent(Activity, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("UserId", data.Key);
                                                Activity.StartActivity(intent);
                                            }
                                            else
                                            {
                                                if (name == dataUSer?.Name || name == dataUSer?.Username)
                                                {
                                                    switch (PostClickListener.OpenMyProfile)
                                                    {
                                                        case true:
                                                            return;
                                                        default:
                                                            {
                                                                var intent = new Intent(Activity, typeof(MyProfileActivity));
                                                                Activity.StartActivity(intent);
                                                                break;
                                                            }
                                                    }
                                                }
                                                else
                                                {
                                                    var intent = new Intent(Activity, typeof(UserProfileActivity));
                                                    //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                    intent.PutExtra("name", name);
                                                    Activity.StartActivity(intent);
                                                }
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
                                                            var intent = new Intent(Activity, typeof(MyProfileActivity));
                                                            Activity.StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                var intent = new Intent(Activity, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("name", name);
                                                Activity.StartActivity(intent);
                                            }

                                            break;
                                        }
                            }

                            break;
                        }
                    case "Number":
                        Methods.App.SaveContacts(Activity, p1, "", "2");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region YouTube Player
         
        public void OnInitSuccess(IYouTubePlayer player)
        {
            try
            {
                YoutubePlayer = player;
                YouTubePlayerEvents = new YouTubePlayerEvents(player, VideoIdYoutube, "ReelsVideo");
                YoutubePlayer.AddListener(YouTubePlayerEvents);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        #endregion

        #region Exo Player

        private void SetPlayer()
        {
            try
            {
                DefaultTrackSelector trackSelector = new DefaultTrackSelector(Context);
                trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder(Context));

                VideoPlayer = new SimpleExoPlayer.Builder(Context).SetTrackSelector(trackSelector).Build();

                PlayerView.UseController = true;
                PlayerView.Player = VideoPlayer;

                var controlView = PlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                if (controlView != null)
                {
                    var mFullScreenIcon = controlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    var mFullScreenButton = controlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    mFullScreenIcon.Visibility = ViewStates.Gone;
                    mFullScreenButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string extension, string tag)
        {
            var bandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(Context);
            var buildHttpDataSourceFactory = new DefaultDataSourceFactory(Context, bandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(Context, AppSettings.ApplicationName)));
            var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(Context, bandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(Context, AppSettings.ApplicationName)));
            int type = Util.InferContentType(uri, extension);
            try
            {

                IMediaSource src;
                switch (type)
                {
                    case C.TypeSs:
                        src = new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).SetDrmSessionManager(IDrmSessionManager.DummyDrmSessionManager).CreateMediaSource(uri);
                        break;
                    case C.TypeDash:
                        src = new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).SetDrmSessionManager(IDrmSessionManager.DummyDrmSessionManager).CreateMediaSource(uri);
                        break;
                    case C.TypeHls:
                        DefaultHlsExtractorFactory defaultHlsExtractorFactory = new DefaultHlsExtractorFactory(DefaultTsPayloadReaderFactory.FlagAllowNonIdrKeyframes, true);
                        src = new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).SetExtractorFactory(defaultHlsExtractorFactory).CreateMediaSource(uri);
                        break;
                    case C.TypeOther:
                        src = new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                    default:
                        src = new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                }

                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                try
                {
                    return new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                    return null!;
                }
            }
        }

        public void StopVideo()
        {
            try
            {
                if (PostFeedType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();
                }
                else
                {
                    if (PlayerView.Player != null && PlayerView.Player.PlaybackState == IPlayer.StateReady)
                        PlayerView.Player.PlayWhenReady = false;
                }

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                //GC Collect
                //GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                if (PostFeedType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();
                }
                else
                {
                    StopVideo();
                    PlayerView?.Player?.Stop();

                    if (VideoPlayer != null)
                    {
                        VideoPlayer.Release();
                        VideoPlayer = null!;
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLoadingChanged(bool p0)
        {
        }

        public void OnPlaybackParametersChanged(PlaybackParameters p0)
        {
        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {

        }

        public void OnPlayerError(ExoPlaybackException p0)
        {
        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                switch (playbackState)
                {
                    case IPlayer.StateBuffering:
                        break;
                    case IPlayer.StateReady:
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnIsPlayingChanged(bool p0)
        {

        }

        public void OnPositionDiscontinuity(int p0)
        {
        }

        public void OnRepeatModeChanged(int p0)
        {
        }

        public void OnSeekProcessed()
        {
        }

        public void OnShuffleModeEnabledChanged(bool p0)
        {
        }

        public void OnTimelineChanged(Timeline p0, int p2)
        {
        }

        public void OnTracksChanged(TrackGroupArray p0, TrackSelectionArray p1)
        {
        }


        #endregion

        private void LoadData(PostDataObject dataObject)
        {
            try
            { 
                PostFeedType = PostFunctions.GetAdapterType(dataObject);
                 
                if (PostFeedType == PostModelType.VideoPost)
                {
                    if (YouTubePlayerEvents != null && TubePlayerView != null && YouTubePlayerEvents.IsPlaying)
                    {
                        TubePlayerView?.Release();
                        TubePlayerView.Visibility = ViewStates.Gone;
                    }
                     
                    // Uri
                    Uri uri = Uri.Parse(dataObject.PostFileFull);
                    var videoSource = GetMediaSourceFromUrl(uri, uri?.Path?.Split('.').Last(), "normal");

                    VideoPlayer.Prepare(videoSource);
                    VideoPlayer.PlayWhenReady = true;
                }
                else if (PostFeedType == PostModelType.YoutubePost)
                {
                    VideoIdYoutube = dataObject.PostYoutube;

                    if (TubePlayerView != null)
                        TubePlayerView.Visibility = ViewStates.Visible;

                    PlayerView.Visibility = ViewStates.Gone;
                    ReleaseVideo();

                    TubePlayerView.Initialize(this, true); 
                }

                GlideImageLoader.LoadImage(Activity, dataObject.PostPrivacy == "4" ? "user_anonymous" : dataObject.Publisher.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                TxtUsername.Text = dataObject.PostPrivacy == "4" ? Activity.GetText(Resource.String.Lbl_Anonymous) : WoWonderTools.GetNameFinal(dataObject.Publisher);

                TxtCommentCount.Text = dataObject.PostComments;

                if (AppSettings.ShowCountSharePost)
                {
                    TxtShareCount.Text = dataObject.DatumPostShare;
                }
                else
                {
                    TxtShareCount.Visibility = ViewStates.Gone;
                }

                if (dataObject.UserId == UserDetails.UserId)
                {
                    GiftLayout.Visibility = ViewStates.Gone;
                }

                if (string.IsNullOrEmpty(dataObject.Orginaltext) || string.IsNullOrWhiteSpace(dataObject.Orginaltext))
                {
                    TxtDescription.Visibility = ViewStates.Invisible;
                }
                else
                {
                    switch (dataObject.RegexFilterList != null & dataObject.RegexFilterList?.Count > 0)
                    {
                        case true:
                            TxtDescription.SetAutoLinkOnClickListener(this, dataObject.RegexFilterList);
                            break;
                        default:
                            TxtDescription.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                            break;
                    }

                    ReadMoreOption.AddReadMoreTo(TxtDescription, new String(dataObject.Orginaltext)); 
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                        {
                            dataObject.Reaction ??= new Reaction();

                            TxtLikeCount.Text = Methods.FunString.FormatPriceValue(dataObject.Reaction.Count);

                            if (dataObject.Reaction.IsReacted != null && dataObject.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(dataObject.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == dataObject.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    if (dataObject.Reaction.Count > 0)
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                            }
                                            LikeLayout.Tag = "Liked";
                                        }
                                        ImgLike.ClearColorFilter();
                                        break;
                                }
                            }
                            else
                            {
                                ImgLike.SetImageResource(Resource.Drawable.icon_post_like_vector);
                                ImgLike.SetColorFilter(Color.White);
                                LikeLayout.Tag = "Like";
                            }
                        } 
                        break; 
                    default:
                        {
                            if (dataObject.Reaction.IsReacted != null && !dataObject.Reaction.IsReacted.Value)
                            {
                                ImgLike.SetImageResource(Resource.Drawable.icon_post_like_vector);
                                ImgLike.SetColorFilter(Color.White);
                                LikeLayout.Tag = "Like";
                            }

                            if (dataObject.IsLiked != null && dataObject.IsLiked.Value)
                            {
                                ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                ImgLike.ClearColorFilter();
                                LikeLayout.Tag = "Liked";
                            }

                            TxtLikeCount.Text = dataObject.PostLikes;

                            break;
                        } 
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}