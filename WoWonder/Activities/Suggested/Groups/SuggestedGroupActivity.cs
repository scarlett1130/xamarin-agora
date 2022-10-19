using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Search.Adapters;
using WoWonder.Activities.Suggested.Adapters;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Suggested.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SuggestedGroupActivity : BaseActivity
    {
        #region Variables Basic

        private SuggestedGroupAdapter MAdapter;
        private SearchGroupAdapter RandomAdapter;
        private CategoriesImageAdapter CategoriesAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;

        private ViewStub EmptyStateLayout, SuggestedGroupViewStub, CatGroupViewStub, RandomGroupViewStub;
        private View Inflated, SuggestedGroupInflated, CatGroupInflated, RandomGroupInflated;
        private TemplateRecyclerInflater RecyclerInflaterSuggestedGroup,RecyclerInflaterCatGroup,RecyclerInflaterRandomGroup;
        private RecyclerViewOnScrollListener SuggestedGroupScrollEvent;

        private LinearLayout Devider1, Devider2;

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
                SetContentView(Resource.Layout.SuggestedGroupLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                StartApiService();
                AdsGoogle.Ad_Interstitial(this);
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
                ListUtils.SuggestedGroupList = MAdapter.GroupList.Count switch
                {
                    > 0 => MAdapter.GroupList,
                    _ => ListUtils.SuggestedGroupList
                };

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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SuggestedGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubSuggestedGroup);
                CatGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubCatGroup);
                RandomGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubRandomGroup);

                Devider1 = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
                Devider2 = FindViewById<LinearLayout>(Resource.Id.linearLayout2);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                 
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
                    toolBar.Title = GetString(Resource.String.Lbl_Discover);
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
                MAdapter = new SuggestedGroupAdapter(this) { GroupList = new ObservableCollection<GroupDataObject>()  };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.JoinButtonItemClick += MAdapterOnJoinButtonItemClick;

                RandomAdapter = new SearchGroupAdapter(this) { GroupList = new ObservableCollection<GroupDataObject>() };
                RandomAdapter.ItemClick += RandomAdapterOnItemClick;
                RandomAdapter.JoinButtonItemClick += MAdapterRandomOnJoinButtonItemClick;

                switch (CategoriesController.ListCategoriesGroup.Count)
                {
                    case > 0:
                    {
                        CategoriesAdapter = new CategoriesImageAdapter(this) { CategoriesList = CategoriesController.ListCategoriesGroup };
                        CategoriesAdapter.ItemClick += CategoriesAdapterOnItemClick;

                        CatGroupInflated = CatGroupInflated switch
                        {
                            null => CatGroupViewStub.Inflate(),
                            _ => CatGroupInflated
                        };

                        RecyclerInflaterCatGroup = new TemplateRecyclerInflater();
                        RecyclerInflaterCatGroup.InflateLayout<Classes.Categories>(this, CatGroupInflated, CategoriesAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_Categories), GetString(Resource.String.Lbl_FindGroupByCategories));

                        RecyclerInflaterCatGroup.Recyler.Visibility = ViewStates.Visible;

                        CategoriesAdapter.NotifyDataSetChanged();
                        break;
                    }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Group");
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
                MAdapter = null!;
                RandomAdapter = null!;
                CategoriesAdapter = null!;
                SwipeRefreshLayout = null!;
                EmptyStateLayout = null!;
                SuggestedGroupViewStub = null!;
                CatGroupViewStub = null!;
                RandomGroupViewStub = null!;
                Inflated = null!;
                SuggestedGroupInflated = null!;
                CatGroupInflated = null!;
                RandomGroupInflated = null!;
                RecyclerInflaterSuggestedGroup = null!;
                RecyclerInflaterCatGroup = null!;
                RecyclerInflaterRandomGroup = null!;
                SuggestedGroupScrollEvent = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Get Group By Categories
        private void CategoriesAdapterOnItemClick(object sender, CategoriesImageAdapterClickEventArgs e)
        {
            try
            {
                var item = CategoriesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(GroupByCategoriesActivity));
                    intent.PutExtra("CategoryId", item.CategoriesId);
                    intent.PutExtra("CategoryName", Methods.FunString.DecodeString(item.CategoriesName));
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //See all SuggestedGroup
        private void MainLinearSuggestedGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AllSuggestedGroupActivity));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Offset Suggested Group
        private void SuggestedGroupScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.GroupList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.GroupId))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroup(item.GroupId) });   
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile Suggested Group
        private void MAdapterOnItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterRandomOnJoinButtonItemClick(object sender, SearchGroupAdapterClickEventArgs e)
        {
            try
            { 

                var item = MAdapter.GetItem(e.Position);
                switch (item)
                {
                    case null:
                        return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var (apiStatus, respond) = await RequestsAsync.Group.JoinGroupAsync(item.GroupId);
                switch (apiStatus)
                {
                    case 200:
                    {
                        switch (respond)
                        {
                            case JoinGroupObject result when result.JoinStatus == "requested":
                                e.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                e.Button.Text = Application.Context.GetText(Resource.String.Lbl_Request);
                                e.Button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                break;
                            case JoinGroupObject result:
                            {
                                var isJoined = result.JoinStatus == "left" ? "false" : "true";
                                e.Button.Text = GetText(isJoined == "yes" || isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);

                                switch (isJoined)
                                {
                                    case "yes":
                                    case "true":
                                        e.Button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                        e.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                        break;
                                    default:
                                        e.Button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                                        e.Button.SetTextColor(Color.White);
                                        break;
                                }

                                break;
                            }
                        }

                        break;
                    }
                    default:
                        Methods.DisplayReportResult(this, respond);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnJoinButtonItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetJoinGroup(this, item.GroupId, e.JoinButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Open Profile Random Group
        private void RandomAdapterOnItemClick(object sender, SearchGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroup("0") , () => LoadRandomGroup("0") });
        }

        private async Task LoadGroup(string offset)
        {
            if (SuggestedGroupScrollEvent != null && SuggestedGroupScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = true;
                var countList = MAdapter.GroupList.Count;

                var (respondCode, respondString) = await RequestsAsync.Group.GetRecommendedGroupsAsync("10", offset);
                switch (respondCode)
                {
                    case 200:
                    {
                        switch (respondString)
                        {
                            case ListGroupsObject result:
                            {
                                var respondList = result.Data.Count;
                                switch (respondList)
                                {
                                    case > 0 when countList > 0:
                                    {
                                        foreach (var item in from item in result.Data let check = MAdapter.GroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                                        {
                                            MAdapter.GroupList.Add(item);
                                        }

                                        RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.GroupList.Count - countList); });
                                        break;
                                    }
                                    case > 0:
                                        MAdapter.GroupList = new ObservableCollection<GroupDataObject>(result.Data);

                                        RunOnUiThread(() =>
                                        { 
                                            SuggestedGroupInflated ??= SuggestedGroupViewStub.Inflate();

                                            RecyclerInflaterSuggestedGroup = new TemplateRecyclerInflater();
                                            RecyclerInflaterSuggestedGroup.InflateLayout<GroupDataObject>(this, SuggestedGroupInflated, MAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_SuggestedForYou), "", true);

                                            RecyclerInflaterSuggestedGroup.MainLinear.Click += MainLinearSuggestedGroupOnClick;

                                            switch (SuggestedGroupScrollEvent)
                                            {
                                                case null:
                                                {
                                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterSuggestedGroup.LayoutManager);
                                                    SuggestedGroupScrollEvent = playlistRecyclerViewOnScrollListener;
                                                    SuggestedGroupScrollEvent.LoadMoreEvent += SuggestedGroupScrollEventOnLoadMoreEvent;
                                                    RecyclerInflaterSuggestedGroup.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                                    SuggestedGroupScrollEvent.IsLoading = false;
                                                    break;
                                                }
                                            } 
                                        });
                                        break;
                                    default:
                                    {
                                        if (RecyclerInflaterSuggestedGroup?.Recyler != null && MAdapter.GroupList.Count > 10 && !RecyclerInflaterSuggestedGroup.Recyler.CanScrollVertically(1))
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        break;
                    }
                    default:
                        Methods.DisplayReportResult(this, respondString);
                        break;
                }
                Devider1.Visibility = ViewStates.Visible;

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
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
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
            } 
        }

        private async Task LoadRandomGroup(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = RandomAdapter.GroupList.Count;

                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "30"},
                    {"group_offset", offset},
                    {"search_key", "a"},
                };

                var (respondCode, respondString) = await RequestsAsync.Global.SearchAsync(dictionary);
                switch (respondCode)
                {
                    case 200:
                    {
                        switch (respondString)
                        {
                            case GetSearchObject result:
                            {
                                var respondList = result.Groups.Count;
                                switch (respondList)
                                {
                                    case > 0 when countList > 0:
                                    {
                                        foreach (var item in from item in result.Groups let check = RandomAdapter.GroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                                        {
                                            RandomAdapter.GroupList.Add(item);
                                        }

                                        RunOnUiThread(() => { RandomAdapter.NotifyItemRangeInserted(countList, RandomAdapter.GroupList.Count - countList); });
                                        break;
                                    }
                                    case > 0:
                                        RandomAdapter.GroupList = new ObservableCollection<GroupDataObject>(result.Groups);

                                        RunOnUiThread(() =>
                                        {
                                            RandomGroupInflated = RandomGroupInflated switch
                                            {
                                                null => RandomGroupViewStub.Inflate(),
                                                _ => RandomGroupInflated
                                            };

                                            RecyclerInflaterRandomGroup = new TemplateRecyclerInflater();
                                            RecyclerInflaterRandomGroup.InflateLayout<GroupDataObject>(this, RandomGroupInflated, RandomAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, GetString(Resource.String.Lbl_RandomGroups));
                                        });
                                        break;
                                    default:
                                    {
                                        if (RecyclerInflaterRandomGroup?.Recyler != null && RandomAdapter.GroupList.Count > 10 && !RecyclerInflaterRandomGroup.Recyler.CanScrollVertically(1))
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        break;
                    }
                    default:
                        Methods.DisplayReportResult(this, respondString);
                        break;
                }
                Devider2.Visibility = ViewStates.Visible;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
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

        private void ShowEmptyPage()
        {
            try
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                switch (MAdapter.GroupList.Count)
                {
                    case > 0:
                    {
                        if (RecyclerInflaterSuggestedGroup?.Recyler != null)
                            RecyclerInflaterSuggestedGroup.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    }
                }

                switch (RandomAdapter.GroupList.Count)
                {
                    case > 0:
                    {
                        if (RecyclerInflaterRandomGroup?.Recyler != null)
                            RecyclerInflaterRandomGroup.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    }
                }
                
                switch (MAdapter.GroupList.Count)
                {
                    case 0 when RandomAdapter.GroupList.Count == 0:
                    {
                        if (RecyclerInflaterSuggestedGroup?.Recyler != null)
                            RecyclerInflaterSuggestedGroup.Recyler.Visibility = ViewStates.Gone;

                        Inflated ??= EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                        switch (x.EmptyStateButton.HasOnClickListeners)
                        {
                            case false:
                                x.EmptyStateButton.Click += null!;
                                break;
                        }
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
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