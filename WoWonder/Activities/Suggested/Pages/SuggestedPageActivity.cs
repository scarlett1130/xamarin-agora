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
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Search.Adapters;
using WoWonder.Activities.Suggested.Adapters;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Suggested.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SuggestedPageActivity : BaseActivity
    {
        #region Variables Basic

        private SuggestedPageAdapter MAdapter;
        private SearchPageAdapter RandomAdapter;
        private CategoriesImageAdapter CategoriesAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;

        private ViewStub EmptyStateLayout, SuggestedPageViewStub, CatPageViewStub, RandomPageViewStub;
        private View Inflated, SuggestedPageInflated, CatPageInflated, RandomPageInflated;
        private TemplateRecyclerInflater RecyclerInflaterSuggestedPage, RecyclerInflaterCatPage, RecyclerInflaterRandomPage;
        private RecyclerViewOnScrollListener SuggestedPageScrollEvent;

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
                ListUtils.SuggestedPageList = MAdapter.PageList.Count switch
                {
                    > 0 => MAdapter.PageList,
                    _ => ListUtils.SuggestedPageList
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

                SuggestedPageViewStub = FindViewById<ViewStub>(Resource.Id.viewStubSuggestedGroup);
                CatPageViewStub = FindViewById<ViewStub>(Resource.Id.viewStubCatGroup);
                RandomPageViewStub = FindViewById<ViewStub>(Resource.Id.viewStubRandomGroup);

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
                MAdapter = new SuggestedPageAdapter(this) { PageList = new ObservableCollection<PageDataObject>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.LikeButtonItemClick += MAdapterOnLikeButtonItemClick;

                RandomAdapter = new SearchPageAdapter(this) { PageList = new ObservableCollection<PageDataObject>() };
                RandomAdapter.ItemClick += RandomAdapterOnItemClick;
                RandomAdapter.LikeButtonItemClick += MAdapterRandomOnLikeButtonItemClick;

                switch (CategoriesController.ListCategoriesPage.Count)
                {
                    case > 0:
                        {
                            CategoriesAdapter = new CategoriesImageAdapter(this) { CategoriesList = CategoriesController.ListCategoriesPage };
                            CategoriesAdapter.ItemClick += CategoriesAdapterOnItemClick;

                            CatPageInflated = CatPageInflated switch
                            {
                                null => CatPageViewStub.Inflate(),
                                _ => CatPageInflated
                            };

                            RecyclerInflaterCatPage = new TemplateRecyclerInflater();
                            RecyclerInflaterCatPage.InflateLayout<Classes.Categories>(this, CatPageInflated, CategoriesAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_Categories), GetString(Resource.String.Lbl_FindPageByCategories));

                            RecyclerInflaterCatPage.Recyler.Visibility = ViewStates.Visible;

                            CategoriesAdapter.NotifyDataSetChanged();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Page");
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
                SuggestedPageViewStub = null!;
                CatPageViewStub = null!;
                RandomPageViewStub = null!;
                Inflated = null!;
                SuggestedPageInflated = null!;
                CatPageInflated = null!;
                RandomPageInflated = null!;
                RecyclerInflaterSuggestedPage = null!;
                RecyclerInflaterCatPage = null!;
                RecyclerInflaterRandomPage = null!;
                SuggestedPageScrollEvent = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Get Page By Categories
        private void CategoriesAdapterOnItemClick(object sender, CategoriesImageAdapterClickEventArgs e)
        {
            try
            {
                var item = CategoriesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(PageByCategoriesActivity));
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

        //See all SuggestedPage
        private void MainLinearSuggestedPageOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AllSuggestedPageActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Offset Suggested Page
        private void SuggestedPageScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.PageList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.PageId))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPage(item.PageId) });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile Suggested Page
        private void MAdapterOnItemClick(object sender, SuggestedPageAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(PageProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterRandomOnLikeButtonItemClick(object sender, SearchPageAdapterClickEventArgs e)
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

                var (apiStatus, respond) = await RequestsAsync.Page.LikePageAsync(item.PageId);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            { 
                                case LikePageObject result:
                                    {
                                        var isLiked = result.LikeStatus == "unliked" ? "false" : "true";
                                        e.Button.Text = GetText(isLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                                         
                                        switch (isLiked)
                                        {
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

        private void MAdapterOnLikeButtonItemClick(object sender, SuggestedPageAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetLikePage(this, item.PageId, e.LikeButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Open Profile Random Page
        private void RandomAdapterOnItemClick(object sender, SearchPageAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(PageProfileActivity), item);
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPage("0"), () => LoadRandomPage("0") });
        }

        private async Task LoadPage(string offset)
        {
            if (SuggestedPageScrollEvent != null && SuggestedPageScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                if (SuggestedPageScrollEvent != null) SuggestedPageScrollEvent.IsLoading = true;
                var countList = MAdapter.PageList.Count;

                var (respondCode, respondString) = await RequestsAsync.Page.GetRecommendedPagesAsync("10", offset);
                switch (respondCode)
                {
                    case 200:
                        {
                            switch (respondString)
                            {
                                case ListPagesObject result:
                                    {
                                        var respondList = result.Data.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Data let check = MAdapter.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                                    {
                                                        MAdapter.PageList.Add(item);
                                                    }

                                                    RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.PageList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                MAdapter.PageList = new ObservableCollection<PageDataObject>(result.Data);

                                                RunOnUiThread(() =>
                                                {
                                                    SuggestedPageInflated ??= SuggestedPageViewStub.Inflate();

                                                    RecyclerInflaterSuggestedPage = new TemplateRecyclerInflater();
                                                    RecyclerInflaterSuggestedPage.InflateLayout<PageDataObject>(this, SuggestedPageInflated, MAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_SuggestedForYou), "", true);

                                                    RecyclerInflaterSuggestedPage.MainLinear.Click += MainLinearSuggestedPageOnClick;

                                                    switch (SuggestedPageScrollEvent)
                                                    {
                                                        case null:
                                                            {
                                                                RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterSuggestedPage.LayoutManager);
                                                                SuggestedPageScrollEvent = playlistRecyclerViewOnScrollListener;
                                                                SuggestedPageScrollEvent.LoadMoreEvent += SuggestedPageScrollEventOnLoadMoreEvent;
                                                                RecyclerInflaterSuggestedPage.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                                                SuggestedPageScrollEvent.IsLoading = false;
                                                                break;
                                                            }
                                                    }
                                                });
                                                break;
                                            default:
                                                {
                                                    if (RecyclerInflaterSuggestedPage?.Recyler != null && MAdapter.PageList.Count > 10 && !RecyclerInflaterSuggestedPage.Recyler.CanScrollVertically(1))
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMorePages), ToastLength.Short);
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
                if (SuggestedPageScrollEvent != null) SuggestedPageScrollEvent.IsLoading = false;
            }
        }

        private async Task LoadRandomPage(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = RandomAdapter.PageList.Count;

                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "30"},
                    {"Page_offset", offset},
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
                                        var respondList = result.Pages.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Pages let check = RandomAdapter.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                                    {
                                                        RandomAdapter.PageList.Add(item);
                                                    }

                                                    RunOnUiThread(() => { RandomAdapter.NotifyItemRangeInserted(countList, RandomAdapter.PageList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                RandomAdapter.PageList = new ObservableCollection<PageDataObject>(result.Pages);

                                                RunOnUiThread(() =>
                                                {
                                                    RandomPageInflated = RandomPageInflated switch
                                                    {
                                                        null => RandomPageViewStub.Inflate(),
                                                        _ => RandomPageInflated
                                                    };

                                                    RecyclerInflaterRandomPage = new TemplateRecyclerInflater();
                                                    RecyclerInflaterRandomPage.InflateLayout<PageDataObject>(this, RandomPageInflated, RandomAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, GetString(Resource.String.Lbl_RandomPages));
                                                });
                                                break;
                                            default:
                                                {
                                                    if (RecyclerInflaterRandomPage?.Recyler != null && RandomAdapter.PageList.Count > 10 && !RecyclerInflaterRandomPage.Recyler.CanScrollVertically(1))
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMorePages), ToastLength.Short);
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
                if (SuggestedPageScrollEvent != null) SuggestedPageScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                switch (MAdapter.PageList.Count)
                {
                    case > 0:
                        {
                            if (RecyclerInflaterSuggestedPage?.Recyler != null)
                                RecyclerInflaterSuggestedPage.Recyler.Visibility = ViewStates.Visible;

                            EmptyStateLayout.Visibility = ViewStates.Gone;
                            break;
                        }
                }

                switch (RandomAdapter.PageList.Count)
                {
                    case > 0:
                        {
                            if (RecyclerInflaterRandomPage?.Recyler != null)
                                RecyclerInflaterRandomPage.Recyler.Visibility = ViewStates.Visible;

                            EmptyStateLayout.Visibility = ViewStates.Gone;
                            break;
                        }
                }

                switch (MAdapter.PageList.Count)
                {
                    case 0 when RandomAdapter.PageList.Count == 0:
                        {
                            if (RecyclerInflaterSuggestedPage?.Recyler != null)
                                RecyclerInflaterSuggestedPage.Recyler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPage);
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
                if (SuggestedPageScrollEvent != null) SuggestedPageScrollEvent.IsLoading = false;
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