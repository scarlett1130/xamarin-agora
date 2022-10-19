using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Search;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using AT.Markushi.UI;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class GroupsActivity : BaseActivity
    {
        #region Variables Basic

        public SocialAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private TextView CreateButton;
        private AdView MAdView;
        private static GroupsActivity Instance;
        private ImageView ivSelfAd;
        private RelativeLayout rlAd;
        private CircleButton cbCloseAd;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                StartApiService();
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.White);
                MRecycler.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.White);

                CreateButton = FindViewById<TextView>(Resource.Id.toolbar_title);
                CreateButton.Text = GetString(Resource.String.Lbl_Create);
                CreateButton.Visibility = ViewStates.Visible;
                 
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);

                ivSelfAd = FindViewById<ImageView>(Resource.Id.iv_self_ad);
                rlAd = FindViewById<RelativeLayout>(Resource.Id.rl_ad);
                cbCloseAd = FindViewById<CircleButton>(Resource.Id.cb_close_ad);
                Glide.With(this).Load("https://www.americafirstpatriot.org/themes/wowonder/img/cotosen.jpg").Apply(new RequestOptions()).Into(ivSelfAd);
                rlAd.Visibility = ViewStates.Visible;
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
                    toolBar.Title = GetText(Resource.String.Lbl_ExploreGroup);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new SocialAdapter(this);
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(this);

                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SocialModelsClass>(this, MAdapter, sizeProvider, 8);
                MRecycler.AddOnScrollListener(preLoader);

                //RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                //MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                //MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                //MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                //MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MAdapterOnItemClick(object sender, SocialAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item?.TypeView == SocialModelType.MangedGroups || item?.TypeView == SocialModelType.JoinedGroups)
                    {
                        MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item.Group);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                        CreateButton.Click += CreateButtonOnClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        ivSelfAd.Click += SelfAdOnClick;
                        cbCloseAd.Click += CbCloseAdOnClick;
                        break;
                    default:
                        CreateButton.Click -= CreateButtonOnClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        ivSelfAd.Click -= SelfAdOnClick;
                        cbCloseAd.Click -= CbCloseAdOnClick;
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

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                Instance = null!; 
                CreateButton = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static GroupsActivity GetInstance()
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
         
        //Scroll
        //private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Code get last id where LoadMore >>
        //        var item = MAdapter.SocialList.LastOrDefault();
        //        if (item != null && !string.IsNullOrEmpty(item?.GroupData?.GroupId) && !MainScrollEvent.IsLoading)
        //            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetJoinedGroups(item?.GroupData?.GroupId), () => GetSuggestedGroup(item?.GroupData?.GroupId) });
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.SocialList.Clear();
                MAdapter.NotifyDataSetChanged();

                //MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Event Create New Group
        private void CreateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(CreateGroupActivity));
                StartActivityForResult(intent, 200);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SelfAdOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", "https://cotosen.sjv.io/c/3221882/1249240/14148");
                intent.PutExtra("Type", "");
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }

        }

        private void CbCloseAdOnClick(object sender, EventArgs e)
        {
            rlAd.Visibility = ViewStates.Gone;
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
                    case 200 when resultCode == Result.Ok:
                        {
                            string result = data?.GetStringExtra("groupItem") ?? "";
                            switch (string.IsNullOrEmpty(result))
                            {
                                case false:
                                    {
                                        var item = JsonConvert.DeserializeObject<GroupDataObject>(result);
                                        switch (item)
                                        {
                                            case null:
                                                return;
                                        }

                                        if (MAdapter.SocialList.Count > 0)
                                        {
                                            var check = MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedGroups);
                                            if (check != null)
                                            {
                                                MAdapter.SocialList?.Insert(1, new SocialModelsClass
                                                {
                                                    Id = Convert.ToInt32(item.GroupId),
                                                    Group = item,
                                                    TypeView = SocialModelType.MangedGroups
                                                });
                                                MAdapter?.NotifyDataSetChanged();
                                            }
                                            else
                                            {
                                                MAdapter.SocialList?.Insert(0, new SocialModelsClass
                                                {
                                                    Id = 0001111111,
                                                    TitleHead = GetString(Resource.String.Lbl_Manage_Groups),
                                                    TypeView = SocialModelType.Section
                                                });

                                                MAdapter.SocialList?.Insert(1, new SocialModelsClass
                                                {
                                                    Id = Convert.ToInt32(item.GroupId),
                                                    Group = item,
                                                    TypeView = SocialModelType.MangedGroups
                                                });

                                                MAdapter.SocialList?.Insert(2, new SocialModelsClass
                                                {
                                                    TypeView = SocialModelType.Divider
                                                });

                                                MAdapter?.NotifyDataSetChanged();
                                            }
                                        }
                                        else
                                        {
                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                Id = 0001111111,
                                                TitleHead = GetString(Resource.String.Lbl_Manage_Groups),
                                                TypeView = SocialModelType.Section
                                            });

                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                Id = Convert.ToInt32(item.GroupId),
                                                Group = item,
                                                TypeView = SocialModelType.MangedGroups
                                            });

                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                TypeView = SocialModelType.Divider
                                            });

                                            MAdapter.NotifyDataSetChanged();
                                        }

                                        ShowEmptyPage();
                                        break;
                                    }
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

        #endregion

        #region Get Group

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetMyGroups });
        }

        private async Task GetMyGroups()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Group.GetMyGroupsAsync("0", "7");
                if (apiStatus != 200 || respond is not ListGroupsObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data.Reverse();

                        var checkList = MAdapter.SocialList.FirstOrDefault(q => q.TypeView == SocialModelType.MangedGroups);
                        if (checkList == null)
                        {
                            MAdapter.SocialList.Insert(0, new SocialModelsClass
                            {
                                Id = 0001111111,
                                TitleHead = GetString(Resource.String.Lbl_Manage_Groups),
                                TypeView = SocialModelType.Section
                            });

                            foreach (var item in from item in result.Data let check = MAdapter.SocialList.FirstOrDefault(a => a.Group?.GroupId == item.GroupId) where check == null select item)
                            {
                                MAdapter.SocialList.Add(new SocialModelsClass
                                {
                                    Id = Convert.ToInt32(item.GroupId),
                                    Group = item,
                                    TypeView = SocialModelType.MangedGroups,
                                });

                                if (ListUtils.MyGroupList.FirstOrDefault(a => a.GroupId == item.GroupId) == null)
                                    ListUtils.MyGroupList.Add(item);
                            }

                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                TypeView = SocialModelType.Divider
                            });
                        }
                    }
                }

                await GetJoinedGroups(); 
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        private async Task GetJoinedGroups(string offset = "0")
        {
            //switch (MainScrollEvent.IsLoading)
            //{
            //    case true:
            //        return;
            //}

            if (Methods.CheckConnectivity())
            {
                //MainScrollEvent.IsLoading = true;

                var (apiStatus, respond) = await RequestsAsync.Group.GetJoinedGroupsAsync(UserDetails.UserId, offset, "10");
                if (apiStatus != 200 || respond is not ListGroupsObject result || result.Data == null)
                {
                    //MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.SocialList.FirstOrDefault(q => q.TypeView == SocialModelType.JoinedGroups);
                        if (checkList == null)
                        {
                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                Id = 0001111111,
                                TitleHead = GetString(Resource.String.Lbl_Joined_Groups),
                                TypeView = SocialModelType.Section
                            });

                            foreach (var item in from item in result.Data let check = MAdapter.SocialList.FirstOrDefault(a => a.Group?.GroupId == item.GroupId) where check == null select item)
                            {
                                item.IsGroupJoined = 2;

                                item.IsJoined = new IsJoined
                                {
                                    Bool = true,
                                    String = "yes"
                                };

                                MAdapter.SocialList.Add(new SocialModelsClass
                                {
                                    Id = Convert.ToInt32(item.GroupId),
                                    Group = item,
                                    TypeView = SocialModelType.JoinedGroups,
                                });

                                switch (ListUtils.MyGroupList.FirstOrDefault(a => a.GroupId == item.GroupId))
                                {
                                    case null:
                                        ListUtils.MyGroupList.Add(item);
                                        break;
                                }
                            }

                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                TypeView = SocialModelType.Divider
                            });
                        }
                    }
                    else
                    {
                        switch (MAdapter.SocialList.Count)
                        {
                            case > 10 when !MRecycler.CanScrollVertically(1):
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
                                break;
                        }
                    }
                }

                //MainScrollEvent.IsLoading = false;
                GetSuggestedGroup();
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                //MainScrollEvent.IsLoading = false;
            }
        }

        private void GetSuggestedGroup()
        {
            try
            {
                if (ListUtils.SuggestedGroupList.Count > 0)
                {
                    MAdapter.SocialList.Add(new SocialModelsClass
                    {
                        Id = 000001010101,
                        SuggestedGroupList = new List<GroupDataObject>(ListUtils.SuggestedGroupList),
                        TitleHead = GetString(Resource.String.Lbl_Discover),
                        TypeView = SocialModelType.SuggestedGroups
                    });

                    ShowEmptyPage();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                //MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.SocialList.Count)
                {
                    case > 0:
                        MAdapter.NotifyDataSetChanged();

                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    x.EmptyStateButton.Click += SearchButtonOnClick;
                                    break;
                            }
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                //MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Open Search And Get Group Random
        private void SearchButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "Random_Groups");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
    }
}