using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Util;
using AndroidX.Interpolator.View.Animation; 
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using TheArtOfDev.Edmodo.Cropper;
using Newtonsoft.Json;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Comment.Fragment;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Uri = Android.Net.Uri;
using SupportFragment = AndroidX.Fragment.App.Fragment;
using WoWonder.Activities.Tabbes;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Java.Util.Regex;
using MaterialDialogsCore;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Contacts.Adapters;
using WoWonder.Activities.PostData;
using WoWonder.StickersView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;
using RegexOptions = System.Text.RegularExpressions.RegexOptions;

namespace WoWonder.Activities.Comment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CommentActivity : BaseActivity
    {
        #region Variables Basic

        private static CommentActivity Instance;
        public CommentAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        public AXEmojiEditText TxtComment;
        private TextView LikeCountBox;
        public ImageView ImgSent, ImgGallery, ImgBack;
        public CircleButton BtnVoice;
        public PostDataObject PostObject;
        public string PostId, ImageUrl;
        private string Type, PathImage, PathVoice, TextRecorder = "";
        private FrameLayout TopFragment;
        private RecordSoundFragment RecordSoundFragment;
        private bool IsRecording;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private LinearLayout CommentLayout;
        private ImageView EmojisView;
        private LinearLayout RootView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Native_Comment_Layout);

                Instance = this;

                Type = Intent?.GetStringExtra("Type") ?? string.Empty;
                PostId = Intent?.GetStringExtra("PostId") ?? string.Empty;
                PostObject = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostObject")  ?? "");
                  
                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                LoadDataPost(); 

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
                ResetMediaPlayer();
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
                RootView = FindViewById<LinearLayout>(Resource.Id.main_content);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler_view);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                LikeCountBox = FindViewById<TextView>(Resource.Id.like_box);
                EmojisView = FindViewById<ImageView>(Resource.Id.emojiicon);
                TxtComment = FindViewById<AXEmojiEditText>(Resource.Id.commenttext);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);
                ImgGallery = FindViewById<ImageView>(Resource.Id.image);
                ImgBack = FindViewById<ImageView>(Resource.Id.back);
                CommentLayout = FindViewById<LinearLayout>(Resource.Id.commentLayout);

				// ar dev
                ImgSent.SetColorFilter(Color.Gray);
                BtnVoice = FindViewById<CircleButton>(Resource.Id.voiceButton);
                BtnVoice.LongClickable = true;
                BtnVoice.Tag = "Free";
                BtnVoice.SetImageResource(Resource.Drawable.microphone);

                TopFragment = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);

                TxtComment.Text = "";
                PathImage = "";
                ImageUrl = "";
                TextRecorder = "";

                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                RecordSoundFragment = new RecordSoundFragment();
                SupportFragmentManager.BeginTransaction().Add(TopFragment.Id, RecordSoundFragment, RecordSoundFragment.Tag);
                  
                ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));

                if (AppSettings.SetTabDarkTheme)
                    EmojisViewTools.LoadDarkTheme();
                else
                    EmojisViewTools.LoadTheme(AppSettings.MainColor);

                EmojisViewTools.MStickerView = true;
                EmojisViewTools.LoadView(this, TxtComment, "CommentActivity", EmojisView); 
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
                        ImgSent.Click += ImgSentOnClick;
                        ImgGallery.Click += ImgGalleryOnClick;
                        ImgBack.Click += ImgBackOnClick;
                        BtnVoice.LongClick += BtnVoiceOnLongClick;
                        BtnVoice.Touch += BtnVoiceOnTouch; 
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        LikeCountBox.Click += LikeCountBoxOnClick;
                        TxtComment.AfterTextChanged += TxtCommentOnAfterTextChanged;  
                        break;
                    default:
                        ImgSent.Click -= ImgSentOnClick;
                        ImgGallery.Click -= ImgGalleryOnClick;
                        ImgBack.Click -= ImgBackOnClick;
                        BtnVoice.LongClick -= BtnVoiceOnLongClick;
                        BtnVoice.Touch -= BtnVoiceOnTouch;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        LikeCountBox.Click -= LikeCountBoxOnClick;
                        TxtComment.AfterTextChanged -= TxtCommentOnAfterTextChanged; 
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static CommentActivity GetInstance()
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
                Instance = null!;
                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                TxtComment = null!;
                LikeCountBox = null!;
                ImgSent = null!; ImgGallery = null!; ImgBack = null!;
                BtnVoice = null!;
                PostObject = null!;
                PostId = null!;
                PathImage = null!; ImageUrl = null!; PathVoice = null!; TextRecorder = null!;
                TopFragment = null!;
                RecordSoundFragment = null!;
                RecorderService = null!;
                CommentLayout = null!;
                MentionList = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtCommentOnAfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                string mentionPattern = "(?:^|\\s|$|[.])@[\\p{L}0-9_]*";

                var pattern = Java.Util.Regex.Pattern.Compile(mentionPattern);
                Matcher matcher = pattern.Matcher(TxtComment.Text);

                while (matcher.Find())
                {
                    string searchText = matcher.Group().Replace(" ", "");
                    Console.WriteLine(searchText);

                    var check = MentionList.FirstOrDefault(a => a == searchText);
                    if (check == null)
                    {
                        ShowPopup(TxtComment, searchText);
                        MentionList.Add(searchText);
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeCountBoxOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                    {
                        switch (PostObject.Reaction.Count)
                        {
                            case > 0:
                            {
                                var intent = new Intent(this, typeof(ReactionPostTabbedActivity));
                                intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostObject));
                                StartActivity(intent);
                                break;
                            }
                        }

                        break;
                    }
                    default:
                    {
                        var intent = new Intent(this, typeof(PostDataActivity));
                        intent.PutExtra("PostId", PostObject.PostId);
                        intent.PutExtra("PostType", "post_likes");
                        StartActivity(intent);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnVoiceOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                var handled = false;

                switch (e.Event?.Action)
                {
                    case MotionEventActions.Up:
                        try
                        {
                            switch (IsRecording)
                            {
                                case true:
                                {
                                    RecorderService.StopRecording();
                                    PathVoice = RecorderService.GetRecorded_Sound_Path();

                                    BtnVoice.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                                    BtnVoice.SetImageResource(Resource.Drawable.microphone);

                                    switch (TextRecorder)
                                    {
                                        case "Recording":
                                        {
                                            switch (string.IsNullOrEmpty(PathVoice))
                                            {
                                                case false:
                                                {
                                                    Bundle bundle = new Bundle();
                                                    bundle.PutString("FilePath", PathVoice);
                                                    RecordSoundFragment.Arguments = bundle;
                                                    ReplaceTopFragment(RecordSoundFragment);
                                                    break;
                                                }
                                            }

                                            TextRecorder = "";
                                            break;
                                        }
                                    }

                                    IsRecording = false;
                                    break;
                                }
                                default:
                                {
                                    switch (UserDetails.SoundControl)
                                    {
                                        case true:
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                                            break;
                                    }

                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_HoldToRecord), ToastLength.Short);
                                    break;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }

                        BtnVoice.Pressed = false;
                        handled = true;
                        break;
                }

                e.Handled = handled;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //record voices ( Permissions is 102 )
        private void BtnVoiceOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    case < 23:
                        StartRecording();
                        break;
                    default:
                    {
                        //Check to see if any permission in our group is available, if one, then all are
                        if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                        {
                            StartRecording();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(102);
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

        private async void StartRecording()
        {
            try
            {
                switch (BtnVoice.Tag?.ToString())
                {
                    case "Free":
                    {
                        //Set Record Style
                        IsRecording = true;

                        switch (UserDetails.SoundControl)
                        {
                            case true:
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");
                                break;
                        }

                        if (TextRecorder != null && TextRecorder != "Recording")
                            TextRecorder = "Recording";

                        BtnVoice.SetColorFilter(Color.ParseColor("#FA3C4C"));
                        BtnVoice.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                        RecorderService = new Methods.AudioRecorderAndPlayer(PostId);
                        //Start Audio record
                        await Task.Delay(600);
                        RecorderService.StartRecording();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Back
        private void ImgBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Gallery
        private void ImgGalleryOnClick(object sender, EventArgs e)
        {
            try
            {
                OptionCommentDialog optionCommentDialog =  new OptionCommentDialog(this);
                optionCommentDialog.Show(SupportFragmentManager, optionCommentDialog.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Api sent Comment
        private async void ImgSentOnClick(object sender, EventArgs e)
        {
            try
            {
                IsRecording = false;

                switch (BtnVoice.Tag?.ToString())
                {
                    case "Audio":
                    {
                        var interTortola = new FastOutSlowInInterpolator();
                        TopFragment.Animate()?.SetInterpolator(interTortola)?.TranslationY(1200)?.SetDuration(300);
                        SupportFragmentManager.BeginTransaction().Remove(RecordSoundFragment)?.Commit();

                        PathVoice = RecorderService.GetRecorded_Sound_Path();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(TxtComment.Text) && string.IsNullOrWhiteSpace(TxtComment.Text) && string.IsNullOrEmpty(PathImage) && string.IsNullOrEmpty(PathVoice) && string.IsNullOrEmpty(ImageUrl))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                    var pathFileImage = "";
                    if (!string.IsNullOrEmpty(PathImage))
                    {
                        pathFileImage = PathImage;
                    }
                    else if (!string.IsNullOrEmpty(ImageUrl))
                    {
                        pathFileImage = ImageUrl;
                    }

                    //remove \n in a string
                    string replacement = Regex.Replace(TxtComment.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

                    CommentObjectExtra comment = new CommentObjectExtra
                    {
                        Id = unixTimestamp.ToString(),
                        PostId = PostObject.Id,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Time = time2,
                        CFile = pathFileImage,
                        Record = PathVoice,
                        Publisher = dataUser,
                        Url = dataUser?.Url,
                        Fullurl = PostObject?.PostUrl,
                        Orginaltext = replacement,
                        Owner = true,
                        CommentLikes = "0",
                        CommentWonders = "0",
                        IsCommentLiked = false,
                        Replies = "0",
                        RepliesCount = "0"
                    };

                    if (AppSettings.EnableFitchOgLink)
                    {
                        //Check if find website in text 
                        foreach (Match item in Regex.Matches(TxtComment.Text, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            Console.WriteLine(item.Value);
                            comment.FitchOgLink = await Methods.OgLink.FitchOgLink(item.Value);
                            break;
                        }
                    }
                   
                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapter.NotifyItemInserted(index);
                            break;
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapter.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapter.EmptyState)
                    {
                        MAdapter.CommentList.Remove(dd);
                        MAdapter.NotifyItemRemoved(MAdapter.CommentList.IndexOf(dd));
                    }

                    ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));
                    
                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Comment.CreatePostCommentsAsync(PostObject.PostId, replacement, PathImage, PathVoice, ImageUrl);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case CreateComments result:
                                {
                                    var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data.Id);
                                    if (date != null)
                                    {
                                        var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(result.Data);

                                        date = db;
                                        date.Id = result.Data.Id;

                                        index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                        MAdapter.CommentList[index] = index switch
                                        {
                                            > -1 => db,
                                            _ => MAdapter.CommentList[index]
                                        };

                                        var postFeedAdapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                        var dataGlobal = postFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == PostObject?.PostId).ToList();
                                        switch (dataGlobal?.Count)
                                        {
                                            case > 0:
                                            {
                                                foreach (var dataClass in from dataClass in dataGlobal let indexCom = postFeedAdapter?.ListDiffer?.IndexOf(dataClass) where indexCom > -1 select dataClass)
                                                {
                                                    dataClass.PostData.PostComments = MAdapter.CommentList.Count.ToString();

                                                    switch (dataClass.PostData.GetPostComments?.Count)
                                                    {
                                                        case > 0:
                                                        {
                                                            var dataComment = dataClass.PostData.GetPostComments.FirstOrDefault(a => a.Id == date.Id);
                                                            switch (dataComment)
                                                            {
                                                                case null:
                                                                    dataClass.PostData.GetPostComments.Add(date);
                                                                    break;
                                                            }

                                                            break;
                                                        }
                                                        default:
                                                            dataClass.PostData.GetPostComments = new List<GetCommentObject> { date };
                                                            break;
                                                    }

                                                    postFeedAdapter?.NotifyItemChanged(postFeedAdapter.ListDiffer.IndexOf(dataClass), "commentReplies");
                                                }

                                                break;
                                            }
                                        }

                                        var postFeedAdapter2 = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                        var dataGlobal2 = postFeedAdapter2?.ListDiffer?.Where(a => a.PostData?.Id == PostObject?.PostId).ToList();
                                        switch (dataGlobal2?.Count)
                                        {
                                            case > 0:
                                            {
                                                foreach (var dataClass in from dataClass in dataGlobal2 let indexCom = postFeedAdapter2.ListDiffer.IndexOf(dataClass) where indexCom > -1 select dataClass)
                                                {
                                                    dataClass.PostData.PostComments = MAdapter.CommentList.Count.ToString();

                                                    switch (dataClass.PostData.GetPostComments?.Count)
                                                    {
                                                        case > 0:
                                                        {
                                                            var dataComment = dataClass.PostData.GetPostComments.FirstOrDefault(a => a.Id == date.Id);
                                                            switch (dataComment)
                                                            {
                                                                case null:
                                                                    dataClass.PostData.GetPostComments.Add(date);
                                                                    break;
                                                            }

                                                            break;
                                                        }
                                                        default:
                                                            dataClass.PostData.GetPostComments = new List<GetCommentObject> { date };
                                                            break;
                                                    }

                                                    postFeedAdapter2.NotifyItemChanged(postFeedAdapter2.ListDiffer.IndexOf(dataClass), "commentReplies");
                                                }

                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    //else Methods.DisplayReportResult(this, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                    PathImage = "";
                    ImageUrl = "";
                    PathVoice = "";

                    BtnVoice.Tag = "Free";
                    BtnVoice.SetImageResource(Resource.Drawable.microphone);
                    BtnVoice.ClearColorFilter();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
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
                MAdapter.CommentList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                StartApiService();
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
          
        #endregion

        #region Load Comment 

        private void LoadDataPost()
        {
            try
            {
                if (PostObject != null)
                {
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                        {
                            PostObject.Reaction ??= new Reaction();

                            if (PostObject.Reaction != null)
                                LikeCountBox.Text = PostObject.Reaction.Count + " " + GetString(Resource.String.Lbl_Reactions);
                            else
                                LikeCountBox.Text = "0 " + GetString(Resource.String.Lbl_Reactions);
                            break;
                        }
                        default:
                        {
                            if (PostObject.PostLikes != null)
                                LikeCountBox.Text = PostObject.PostLikes + " " + GetString(Resource.String.Btn_Likes);
                            else
                                LikeCountBox.Text = "0 " + GetString(Resource.String.Btn_Likes);
                            break;
                        }
                    }

                    switch (PostObject.CommentsStatus)
                    {
                        case "0":
                            MAdapter.CommentList.Clear();

                            MAdapter.CommentList.Add(new CommentObjectExtra
                            {
                                Id = MAdapter.EmptyState,
                                Text = MAdapter.EmptyState,
                                Orginaltext = GetText(Resource.String.Lbl_CommentsAreDisabledBy) + " " + WoWonderTools.GetNameFinal(PostObject.Publisher),
                            });

                            MAdapter.NotifyDataSetChanged();

                            CommentLayout.Visibility = ViewStates.Gone;

                            MainScrollEvent.IsLoading = false;
                            SwipeRefreshLayout.Refreshing = false;


                            break;
                        default:
                            //if (PostObject?.GetPostComments?.Count > 0)
                            //{
                            //    var db = ClassMapper.Mapper?.Map<List<CommentObjectExtra>>(PostObject.GetPostComments);
                            //    MAdapter.CommentList = new ObservableCollection<CommentObjectExtra>(db);
                            //}
                            //else
                            //{
                            //    MAdapter.CommentList = new ObservableCollection<CommentObjectExtra>();
                            //}
                         
                            //if (MAdapter.CommentList.Count > 0)
                            //    MAdapter?.NotifyDataSetChanged();

                            StartApiService();
                            break;
                    }
                }

                switch (Type)
                {
                    case "Normal_Gallery":
                        OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        break;
                    case "Normal_EmojiIcon":
                        EmojisView.PerformClick();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService(string offset = "0")
        { 
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment(offset) });
        }

        private async Task LoadDataComment(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapter.CommentList.Count; 
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

                RunOnUiThread(ShowEmptyPage);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                 
                switch (MAdapter.CommentList.Count)
                {
                    case > 0:
                    {
                        var emptyStateChecker = MAdapter.CommentList.FirstOrDefault(a => a.Text == MAdapter.EmptyState);
                        if (emptyStateChecker != null && MAdapter.CommentList.Count > 1)
                        {
                            MAdapter.CommentList.Remove(emptyStateChecker);
                            MAdapter.NotifyDataSetChanged();
                        }

                        break;
                    }
                    default:
                    {
                        MAdapter.CommentList.Clear();
                        var d = new CommentObjectExtra { Text = MAdapter.EmptyState };
                        MAdapter.CommentList.Add(d);
                        MAdapter.NotifyDataSetChanged();
                        break;
                    }
                } 
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
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
                    case 500:
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                        break;
                    }
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
                                        PathImage = resultUri.Path;

                                        File file2 = new File(resultUri.Path);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgGallery);

                                        //GlideImageLoader.LoadImage(this, PathImage, ImgGallery, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
                    case 7 when resultCode == Result.Ok:
                    {
                        var giflink = data.GetStringExtra("gif") ?? "Data not available";
                        if (giflink != "Data not available" && !string.IsNullOrEmpty(giflink))
                        {
                            ImageUrl = giflink;
                                  
                            Glide.With(this).Load(ImageUrl).Apply(new RequestOptions()).Into(ImgGallery); 
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
                        OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 102 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        StartRecording();
                        break;
                    case 102:
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

        #region PickiT >> Gert path file

        private async void PickiTonCompleteListener(string path)
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
                if (check is false)
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
                                        PathImage = path;

                                        File file2 = new File(PathImage);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgGallery);

                                        //GlideImageLoader.LoadImage(this, PathImage, ImgGallery, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        break;
                                    }
                                    default:
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                                        break;
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
                    }
                }
                else
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    switch (type)
                    {
                        case "Image":
                        {
                            PathImage = path;

                            File file2 = new File(PathImage);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgGallery);

                            //GlideImageLoader.LoadImage(this, PathImage, ImgGallery, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            break;
                        }
                        default:
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
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

        private void ResetMediaPlayer()
        {
            try
            {
                var list = MAdapter.CommentList.Where(a => !string.IsNullOrEmpty(a.Record) && a.MediaPlayer != null).ToList();
                switch (list.Count)
                {
                    case > 0:
                    {
                        foreach (var item in list)
                        {
                            if (item.MediaPlayer != null)
                            {
                                item.MediaPlayer.Stop();
                                item.MediaPlayer.Reset();
                            }
                            item.MediaPlayer = null!;
                            item.MediaTimer = null!;

                            item.MediaPlayer?.Release();
                            item.MediaPlayer = null!;
                        }
                        MAdapter.NotifyDataSetChanged();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OpenGifActivity()
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(GifActivity)), 7);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        public void OpenDialogGallery()
        {
            try
            {
                if (!WoWonderTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_AllowedFileUpload), GetText(Resource.String.Lbl_Ok));
                    return;
                }
                
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

        private void ReplaceTopFragment(SupportFragment fragmentView)
        {
            try
            {
                switch (fragmentView.IsVisible)
                {
                    case true:
                        return;
                }

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragment.Id, fragmentView);

                switch (SupportFragmentManager.BackStackEntryCount)
                {
                    case 0:
                        trans.AddToBackStack(null);
                        break;
                }

                trans.Commit();

                TopFragment.TranslationY = 1200;
                TopFragment.Animate().SetInterpolator(new FastOutSlowInInterpolator()).TranslationYBy(-1200)
                    .SetDuration(500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region Popup Mention User
         
        private PopupWindow PopupWindow;
        private ProgressBar PopupProgressBar;
        private RecyclerView PopupRecycler;
        private ContactsAdapter PopupMAdapter;
        private string SearchText;
        private List<string> MentionList = new List<string>();

        private async void ShowPopup(AXEmojiEditText v, string searchText)
        {
            try
            {
                await Task.Delay(500);

                if (PopupWindow != null && PopupWindow.IsShowing)
                    return;
                 
                LayoutInflater layoutInflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
                View popupView = layoutInflater?.Inflate(Resource.Layout.PopupMentionLayout, null);
                //popupView?.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 300, Resources?.DisplayMetrics);
                PopupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent, true);

                InitializingPopupMention(popupView, searchText);
                 
                PopupWindow.SetBackgroundDrawable(new ColorDrawable());
                PopupWindow.AnimationStyle = Resource.Style.Animation;
                PopupWindow.Focusable = true;
                PopupWindow.ClippingEnabled = true;
                PopupWindow.OutsideTouchable = false;
                PopupWindow.DismissEvent += delegate (object sender, EventArgs args) {
                    try
                    {
                        PopupWindow?.Dismiss();

                        PopupWindow = null;
                        PopupProgressBar = null;
                        PopupRecycler = null;
                        PopupMAdapter = null;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                int[] location = new int[2];
                v.GetLocationInWindow(location);

                int offsetX = 0;
                int offsetY = -500;

                PopupWindow.ShowAtLocation(v, GravityFlags.NoGravity, location[0] + offsetX, location[1] + offsetY);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void InitializingPopupMention(View view , string searchText)
        {
            try
            {
                SearchText = searchText;
                 
                PopupProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
                PopupRecycler = view.FindViewById<RecyclerView>(Resource.Id.recyler);

                PopupProgressBar.Visibility = ViewStates.Visible;

                PopupMAdapter = new ContactsAdapter(this, false, ContactsAdapter.TypeTextSecondary.None)
                {
                    UserList = new ObservableCollection<UserDataObject>(),
                };
                PopupMAdapter.ItemClick += PopupMAdapterOnItemClick;
                PopupRecycler.SetLayoutManager(new LinearLayoutManager(this));
                PopupRecycler.HasFixedSize = true;
                PopupRecycler.SetItemViewCacheSize(50);
                PopupRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, PopupMAdapter, sizeProvider, 10);
                PopupRecycler.AddOnScrollListener(preLoader);
                PopupRecycler.SetAdapter(PopupMAdapter);
                  
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => StartSearchRequest(searchText) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopupMAdapterOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            {
                UserDataObject item = PopupMAdapter?.GetItem(e.Position);
                if (item != null)
                {
                    MentionList?.Add("@" + item.Username);

                    TxtComment.Text = TxtComment.Text?.Replace(SearchText, "@" + item.Username);
                     
                    PopupWindow?.Dismiss();

                    PopupWindow = null;
                    PopupProgressBar = null;
                    PopupRecycler = null;
                    PopupMAdapter = null;
                    SearchText = null;
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task StartSearchRequest(string searchText)
        {
            int countUserList = PopupMAdapter.UserList.Count;

            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "8"},
                {"user_offset", "0"},
                {"search_key", searchText.Replace("@" , "")}
            };

            var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            if (apiStatus == 200)
            {
                if (respond is GetSearchObject result)
                {
                    var respondUserList = result.Users?.Count;
                    if (respondUserList is > 0 && countUserList > 0)
                    {
                        foreach (var item in from item in result.Users let check = PopupMAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                        {
                            PopupMAdapter.UserList.Add(item);
                        }

                        RunOnUiThread(() =>
                        {
                            PopupMAdapter.NotifyItemRangeInserted(countUserList - 1, PopupMAdapter.UserList.Count - countUserList);
                        });
                    }
                    else if (respondUserList is > 0)
                    {
                        PopupMAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                        RunOnUiThread(() => { PopupMAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (PopupMAdapter.UserList.Count is > 10 && !PopupRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }
                }
            }
            else
                Methods.DisplayReportResult(this, respond);
            
            RunOnUiThread(() =>
            {
                try
                {
                    if (PopupMAdapter?.UserList?.Count > 0)
                    {
                        PopupProgressBar.Visibility = ViewStates.Gone;
                        PopupRecycler.Visibility = ViewStates.Visible; 
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        #endregion
         
    }
}  