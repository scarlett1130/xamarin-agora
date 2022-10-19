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
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using WoWonder.Activities.Base;
using WoWonder.Activities.Contacts.Adapters;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Search.Fragment;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;


namespace WoWonder.Activities.Search
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class BrowserUserActivity : BaseActivity, TextView.IOnEditorActionListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private AutoCompleteTextView SearchView;
        public string DataKey, SearchText = "";
        public string OffsetUser = "", OffsetPage = "", OffsetGroup = "";
        private ImageView ImgFilter;
        private Toolbar Toolbar;

        public ContactsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        public ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Window?.SetSoftInputMode(SoftInput.AdjustNothing);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.BrowserUser_Layout);

                DataKey = Intent?.GetStringExtra("Key") ?? "Data not available";
                if (DataKey != "Data not available" && !string.IsNullOrEmpty(DataKey))
                {
                    SearchText = DataKey;
                }

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();
                InitToolbar();

                //Console.WriteLine("===== SearchText is " + SearchText + " =====");
                switch (SearchText)
                {
                    case "Random":
                        SearchText = "a";
                        break;
                    default:
                        {
                            switch (string.IsNullOrEmpty(SearchText))
                            {
                                case false:
                                    Search(SearchText);
                                    break;
                                default:
                                    {
                                        switch (SearchView)
                                        {
                                            case null:
                                                return;
                                        }
                                        //SearchView.SetQuery(SearchText, false);
                                        SearchView.ClearFocus();
                                        //SearchView.OnActionViewCollapsed();
                                        break;
                                    }
                            }

                            break;
                        }
                }

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
                //SearchView.SetQuery("", false);
                SearchView?.ClearFocus();
                //SearchView.OnActionViewCollapsed();

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
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(false);

                ImgFilter = FindViewById<ImageView>(Resource.Id.filter_icon);
                ImgFilter.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                ProgressBarLoader = (ProgressBar)FindViewById(Resource.Id.sectionProgress);
                ProgressBarLoader.Visibility = ViewStates.Gone;

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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new ContactsAdapter(this, true, ContactsAdapter.TypeTextSecondary.LastSeen) { UserList = new ObservableCollection<UserDataObject>() };
                //MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.FollowButtonItemClick += MAdapter.OnFollowButtonItemClick;
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                //MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = " ";
                    Toolbar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));
                }

                SearchView = FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchView.SetOnEditorActionListener(this);
                //SearchView.ClearFocus();

                //Change text colors
                SearchView.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                SearchView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                        ImgFilter.Click += FloatingActionButtonViewOnClick;
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                        break;
                    default:
                        ImgFilter.Click -= FloatingActionButtonViewOnClick;
                        MAdapter.ItemClick -= MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        MainScrollEvent.LoadMoreEvent -= MainScrollEventOnLoadMoreEvent;
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
                AppBarLayout = null!;
                Toolbar = null!;
                SearchText = null!;
                OffsetUser = "";
                OffsetPage = "";
                OffsetGroup = "";
                DataKey = "";
                SearchText = "";
                ImgFilter = null!;

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                ProgressBarLoader = null!;
                LayoutManager = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                MainScrollEvent = null!;
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
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                {
                    OffsetUser = item.UserId;
                    StartApiService();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.OpenProfile(this, item.UserId, item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Filter
        private void FloatingActionButtonViewOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterUserDialogFragment mFragment = new FilterUserDialogFragment();
                mFragment.Show(SupportFragmentManager, mFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnQueryTextSubmit(string newText)
        {
            try
            {
                SearchText = newText;

                SearchView.ClearFocus();

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                OffsetUser = "0";
                OffsetPage = "0";
                OffsetGroup = "0";

                if (Methods.CheckConnectivity())
                {
                    if (ProgressBarLoader != null)
                        ProgressBarLoader.Visibility = ViewStates.Visible;

                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    StartApiService();
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                            x.EmptyStateButton.Click += null!;
                            break;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ProgressBarLoader.Visibility = ViewStates.Gone;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        public void Search(string text)
        {
            try
            {
                SearchText = text;

                switch (string.IsNullOrEmpty(SearchText))
                {
                    case false:
                        {
                            //Console.WriteLine("===== Search false =====");
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter?.UserList?.Clear();
                                MAdapter?.NotifyDataSetChanged();

                                if (ProgressBarLoader != null)
                                    ProgressBarLoader.Visibility = ViewStates.Visible;

                                EmptyStateLayout.Visibility = ViewStates.Gone;

                                StartApiService();
                            }

                            break;
                        }
                    default:
                        {
                            //Console.WriteLine("===== Search default =====");
                            Inflated ??= EmptyStateLayout?.Inflate();

                            EmptyStateInflater x1 = new EmptyStateInflater();
                            x1.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                            switch (x1.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x1.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                                    x1.EmptyStateButton.Click -= TryAgainButton_Click;
                                    x1.EmptyStateButton.Click += null!;
                                    break;
                            }

                            x1.EmptyStateButton.Click += TryAgainButton_Click;
                            if (EmptyStateLayout != null)
                            {
                                EmptyStateLayout.Visibility = ViewStates.Visible;
                            }

                            ProgressBarLoader.Visibility = ViewStates.Gone;

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
        }

        private async Task StartSearchRequest()
        {
            //Console.WriteLine("===== StartSearchRequest =====");
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            MainScrollEvent.IsLoading = true;
            int countUserList = MAdapter.UserList.Count;

            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "30"},
                {"user_offset", OffsetUser},
                //{"group_offset", OffsetGroup},
                //{"page_offset", OffsetPage},
                {"gender", UserDetails.SearchGender},
                {"search_key", SearchText},
                {"country", UserDetails.SearchCountry},
                {"status", UserDetails.SearchStatus},
                {"verified", UserDetails.SearchVerified},
                {"filterbyage", UserDetails.SearchFilterByAge},
                {"age_from", UserDetails.SearchAgeFrom},
                {"age_to", UserDetails.SearchAgeTo},
                {"image", UserDetails.SearchProfilePicture},
            };

            var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            switch (apiStatus)
            {
                case 200:
                    {
                        switch (respond)
                        {
                            case GetSearchObject result:
                                {
                                    var respondUserList = result.Users?.Count;
                                    switch (respondUserList)
                                    {
                                        case > 0 when countUserList > 0:
                                            {
                                                foreach (var item in from item in result.Users let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                {
                                                    MAdapter.UserList.Add(item);
                                                }

                                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countUserList, MAdapter.UserList.Count - countUserList); });
                                                break;
                                            }
                                        case > 0:
                                            MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                            break;
                                        default:
                                            {
                                                switch (MAdapter.UserList.Count)
                                                {
                                                    case > 10 when !MRecycler.CanScrollVertically(1):
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
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
                default:
                    Methods.DisplayReportResult(this, respond);
                    break;
            }

            RunOnUiThread(ShowEmptyPage);
            MainScrollEvent.IsLoading = false;

        }

        private void ShowEmptyPage()
        {
            try
            {
                ProgressBarLoader.Visibility = ViewStates.Gone;

                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.UserList.Count)
                {
                    case > 0:
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                                    x.EmptyStateButton.Click -= TryAgainButton_Click;
                                    break;
                            }

                            x.EmptyStateButton.Click += TryAgainButton_Click;
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }

            }
            catch (Exception e)
            {
                //SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (EmptyStateLayout != null) EmptyStateLayout.Visibility = ViewStates.Gone;

                //ViewPager.SetCurrentItem(0, true);

                Search("a");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();
                OffsetUser = "0";
                OffsetPage = "0";
                OffsetGroup = "0";

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                //ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                    ProgressBarLoader.Visibility = ViewStates.Visible;
                    StartApiService();
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                            x.EmptyStateButton.Click += null!;
                            break;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                    ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            switch (actionId)
            {
                case ImeAction.Search:
                    SearchText = v.Text;

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    SearchViewOnQueryTextSubmit(SearchText);

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    HideKeyboard();

                    return true;
                default:
                    return false;
            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}