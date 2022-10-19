using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class NewsFeedNative : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private FloatingActionButton PopupBubbleView;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        private Handler MainHandler = new Handler(Looper.MainLooper);
        private TabbedMainActivity GlobalContext; 

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        { 
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your fragment here 
                GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance(); 
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
                View view = inflater.Inflate(Resource.Layout.TestNewsFeed, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters();

                LoadPost(true);
                 
                GlobalContext?.GetOneSignalNotification(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        public override void OnStop()
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

        public override void OnDestroy()
        {
            try
            {
                MainRecyclerView?.ReleasePlayer();

                MainRecyclerView = null!;
                PostFeedAdapter = null!;
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
                MainRecyclerView = (WRecyclerView)view.FindViewById(Resource.Id.newsfeedRecyler);
                PopupBubbleView = (FloatingActionButton)view.FindViewById(Resource.Id.popup_bubble);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                if (SwipeRefreshLayout != null)
                {
                    SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                    SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                PostFeedAdapter = new NativePostAdapter(Activity, "", MainRecyclerView, NativeFeedType.Global);
                MainRecyclerView?.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                switch (AppSettings.ShowNewPostOnNewsFeed)
                {
                    case true:
                        MainRecyclerView?.SetXPopupBubble(PopupBubbleView);
                        break;
                    default:
                        PopupBubbleView.Visibility = ViewStates.Gone;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Refresh

        //Refresh 
        public void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                PopupBubbleView.Visibility = ViewStates.Gone;

                PostFeedAdapter?.ListDiffer?.Clear();  
                PostFeedAdapter?.NotifyDataSetChanged();

                PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.Clear();
                PostFeedAdapter?.HolderStory?.StoryAdapter?.NotifyDataSetChanged();

                MainRecyclerView?.StopVideo();

                var combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, Activity);

                combiner.AddPostBoxPostView("feed", -1);

                if (AppSettings.ShowStory)
                {
                    combiner.AddStoryPostView(new List<StoryDataObject>());
                }

                //combiner.AddPostBoxPostView("feed", -1);

                var checkSectionAlertBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                {
                    PostFeedAdapter?.ListDiffer?.Remove(checkSectionAlertBox);
                }

                var checkSectionAlertJoinBox = PostFeedAdapter?.ListDiffer?.Where(a => a.TypeView == PostModelType.AlertJoinBox).ToList();
                {
                    foreach (var adapterModelsClass in checkSectionAlertJoinBox)
                    {
                        PostFeedAdapter?.ListDiffer?.Remove(adapterModelsClass);
                    }
                }
                 
                PostFeedAdapter?.NotifyDataSetChanged();
                MainRecyclerView.MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Post Feed

        public void LoadPost(bool local)
        {
            try
            {
                var combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, Activity);

                //combiner.AddStoryPostView();
                combiner.AddPostBoxPostView("feed", -1);

                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                if (AppSettings.ShowStory)
                {
                    var list = dbDatabase.GetDataStory();
                    combiner.AddStoryPostView(!string.IsNullOrEmpty(list) ? JsonConvert.DeserializeObject<List<StoryDataObject>>(list) : new List<StoryDataObject>());
                }

                switch (local)
                {
                    case true:
                        combiner.AddGreetingAlertPostView();
                        break;
                }

                var json = dbDatabase.GetDataPost();

                if (!string.IsNullOrEmpty(json) && local)
                {
                    var postObject = JsonConvert.DeserializeObject<PostObject>(json);
                    if (postObject?.Data.Count > 0)
                    {
                        MainRecyclerView.ApiPostAsync.LoadDataApi(postObject.Status, postObject, "0");
                        MainRecyclerView.ScrollToPosition(0);
                    }

                    //Start Updating the news feed every few minus 
                    StartApiService("0" , "FirstInsert");
                    StartHandler();
                    return;
                }

                switch (PostFeedAdapter?.ListDiffer?.Count)
                {
                    case <= 5:
                        StartApiService();
                        break;
                    default:
                    {
                        var item = PostFeedAdapter?.ListDiffer?.LastOrDefault();

                        var lastItem = PostFeedAdapter.ListDiffer.IndexOf(item);

                        item = PostFeedAdapter?.ListDiffer[lastItem];

                        string offset;
                        switch (item.TypeView)
                        {
                            case PostModelType.Divider:
                            case PostModelType.ViewProgress:
                            case PostModelType.AdMob1:
                            case PostModelType.AdMob2:
                            case PostModelType.AdMob3:
                            case PostModelType.FbAdNative:
                            case PostModelType.AdsPost:
                            case PostModelType.SuggestedGroupsBox:
                            case PostModelType.SuggestedUsersBox:
                            case PostModelType.CommentSection:
                            case PostModelType.AddCommentSection:
                                item = PostFeedAdapter?.ListDiffer?.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                                offset = item?.PostData?.PostId ?? "0";
                                Console.WriteLine(offset);
                                break;
                            default:
                                offset = item.PostData?.PostId ?? "0";
                                break;
                        }

                        StartApiService(offset, "Insert");
                        break;
                    }
                }

                //Start Updating the news feed every few minus
                StartHandler();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0" ,string typeRun = "Add")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory, () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts(offset , typeRun) }); 
        }

        private static bool IsCanceledHandler;
        public void StartHandler()
        {
            try
            {
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ApiPostUpdaterHelper(Activity, new Handler(Looper.MainLooper)), AppSettings.RefreshPostSeconds);
                IsCanceledHandler = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void RemoveHandler()
        {
            try
            {
                MainHandler?.RemoveCallbacks(new ApiPostUpdaterHelper(Activity, new Handler(Looper.MainLooper)));
                MainHandler = null;
                IsCanceledHandler = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly Handler MainHandler;
            private readonly Activity Activity;

            public ApiPostUpdaterHelper(Activity activity, Handler mainHandler)
            {
                MainHandler = mainHandler;
                Activity = activity;
            }

            public async void Run()
            {
                try
                {
                    if (string.IsNullOrEmpty(Current.AccessToken) || !Methods.CheckConnectivity() || IsCanceledHandler)
                        return;
                     
                    var instance = TabbedMainActivity.GetInstance();
                    if (instance != null)
                    {
                        if (instance.NewsFeedTab != null) await instance.NewsFeedTab.LoadStory();
                        if (instance.NotificationsTab != null) await instance.Get_Notifications();
                    } 
                    //await ApiRequest.Get_MyProfileData_Api(Activity);
                    MainHandler?.PostDelayed(new ApiPostUpdaterHelper(Activity, MainHandler), AppSettings.RefreshPostSeconds);
                }
                catch (Exception e)
                {
                    //Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Get Story

        private async Task LoadStory()
        {
            switch (AppSettings.ShowStory)
            {
                case false:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    checkSection.StoryList ??= new ObservableCollection<StoryDataObject>();

                    var (apiStatus, respond) = await RequestsAsync.Story.GetUserStoriesAsync();
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case GetUserStoriesObject result:
                                    await Task.Factory.StartNew(() =>
                                    {
                                        try
                                        { 
                                            foreach (var item in result.Stories)
                                            {
                                                var check = checkSection.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                                if (check != null)
                                                {
                                                    foreach (var item2 in item.Stories)
                                                    {
                                                        item.DurationsList ??= new List<long>();

                                                        //image and video
                                                        var mediaFile = !item2.Thumbnail.Contains("avatar") && item2.Videos?.Count == 0 ? item2.Thumbnail : item2.Videos?.FirstOrDefault()?.Filename ?? "";

                                                        var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                        if (type != "Video")
                                                        {
                                                            Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                            item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                                        }
                                                        else
                                                        {
                                                            var fileName = mediaFile.Split('/').Last();
                                                            mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                            if (AppSettings.ShowFullVideo)
                                                            {
                                                                var duration = WoWonderTools.GetDuration(mediaFile);
                                                                item.DurationsList.Add(Long.ParseLong(duration));
                                                            }
                                                            else
                                                            { 
                                                                item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                            }
                                                          
                                                        }
                                                    }

                                                    check.Stories = item.Stories;
                                                }
                                                else
                                                {
                                                    foreach (var item1 in item.Stories)
                                                    {
                                                        item.DurationsList ??= new List<long>();

                                                        //image and video
                                                        string mediaFile = !item1.Thumbnail.Contains("avatar") && item1.Videos?.Count == 0 ? item1.Thumbnail : item1.Videos?.FirstOrDefault()?.Filename ?? "";

                                                        var type1 = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                        if (type1 != "Video")
                                                        {
                                                            Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                            item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                                        }
                                                        else
                                                        {
                                                            var fileName = mediaFile.Split('/').Last();
                                                            WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                            if (AppSettings.ShowFullVideo)
                                                            {
                                                                var duration = WoWonderTools.GetDuration(mediaFile);
                                                                item.DurationsList.Add(Long.ParseLong(duration));
                                                            }
                                                            else
                                                            {
                                                                item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                            } 
                                                        }
                                                    }

                                                    checkSection.StoryList.Add(item);
                                                }
                                            }
                                            Activity?.RunOnUiThread(() => 
                                            {
                                                try
                                                {
                                                    if (PostFeedAdapter?.HolderStory?.AboutMore != null)
                                                        PostFeedAdapter.HolderStory.AboutMore.Visibility = checkSection.StoryList.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
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
                                    }).ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }
                        default:
                            Methods.DisplayReportResult(Activity, respond);
                            break;
                    }
                    Activity?.RunOnUiThread(() =>
                    {
                        try
                        {
                            var d = new Runnable(() => { PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkSection)); }); d.Run();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    if (checkSection.StoryList.Count > 0)
                    {
                       SqLiteDatabase dbDatabase = new SqLiteDatabase();
                       dbDatabase.InsertOrUpdateStory(JsonConvert.SerializeObject(checkSection.StoryList)); 
                    }
                    else
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DeleteStory();
                    }
                }
            }
            else
            {
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        #endregion
         
    }
}