using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Album.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using MaterialDialogsCore;
using System.Threading;
using WoWonderClient.Classes.Album;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using WoWonderClient;
using AT.Markushi.UI;

namespace WoWonder.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyAlbumActivity : BaseActivity
    {
        #region Variables Basic

        private AlbumsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;
        private TextView CreateAlbum;
        private int position;
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

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadAlbums();

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
                ListUtils.ListCachedDataAlbum = MAdapter.AlbumList.Count switch
                {
                    > 0 => MAdapter.AlbumList,
                    _ => ListUtils.ListCachedDataAlbum
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                CreateAlbum = FindViewById<TextView>(Resource.Id.toolbar_title);
                CreateAlbum.Visibility = ViewStates.Visible;
                CreateAlbum.Text = GetString(Resource.String.Lbl_Create);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

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
                    toolBar.Title = GetText(Resource.String.Lbl_Albums);
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
                MAdapter = new AlbumsAdapter(this)
                {
                    AlbumList = new ObservableCollection<PostDataObject>()
                };
                LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2 
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PostDataObject>(this, MAdapter, sizeProvider, 10);
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
                        MAdapter.OnItemClick += MAdapterOnOnItemClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        CreateAlbum.Click += ActionButtonOnClick;
                        MAdapter.CloseItemClick += MAdapterCloseItemClick;
                        ivSelfAd.Click += SelfAdOnClick;
                        cbCloseAd.Click += CbCloseAdOnClick;
                        break;
                    default:
                        MAdapter.OnItemClick -= MAdapterOnOnItemClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        CreateAlbum.Click -= ActionButtonOnClick;
                        MAdapter.CloseItemClick -= MAdapterCloseItemClick;
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
                MainScrollEvent = null!;
                CreateAlbum = null!;
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
                MAdapter.AlbumList.Clear();
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
        private async void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.AlbumList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    await LoadAlbum(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnOnItemClick(object sender, AlbumsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    position = e.Position;

                    var intent = new Intent(this, typeof(ImageByAlbumActivity));
                    intent.PutExtra("ItemData", JsonConvert.SerializeObject(item));
                    StartActivityForResult(intent, 2030);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(CreateAlbumActivity));
                StartActivityForResult(intent, 2020);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterCloseItemClick(object sender, AlbumsAdapterClickEventArgs e)
        {
            try
            {
                PostDataObject item = MAdapter.GetItem(e.Position);
                //Console.WriteLine("===== Album name " + item.AlbumName + " =====");
                if (item != null)
                {
                    DeletePostEvent(item);

                }
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

        private void DeletePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(GetText(Resource.String.Lbl_DeleteAlbum)).TitleColorRes(Resource.Color.primary);
                    dialog.Content(GetText(Resource.String.Lbl_AreYouSureDeleteAlbum));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(item.Id, "delete") });

                            Thread.Sleep(1000);

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);

                            //
                            MAdapter.AlbumList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            MainScrollEvent.IsLoading = false;

                            StartApiService();

                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new WoWonderTools.MyMaterialDialog()).NegativeColorRes(Resource.Color.colorIcon);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
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
                    case 2020 when resultCode == Result.Ok:
                        {
                            //
                            MAdapter.AlbumList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            MainScrollEvent.IsLoading = false;

                            StartApiService();

                            /*string result = data.GetStringExtra("AlbumItem");
                            var item = JsonConvert.DeserializeObject<PostDataObject>(result);
                            if (item != null)
                            {
                                MAdapter.AlbumList.Add(item);
                                MAdapter.NotifyDataSetChanged();

                                RunOnUiThread(ShowEmptyPage);
                            }*/

                            break;
                        }
                    case 2030 when resultCode == Result.Ok:
                        {
                            string result = data.GetStringExtra("DeleteAlbumPhoto");
                            var item = JsonConvert.DeserializeObject<ObservableCollection<PhotoAlbumObject>>(result);
                            if (item != null)
                            {
                                MAdapter.AlbumList[position].PhotoAlbum.Clear();
                                IEnumerable<PhotoAlbumObject> obsCollection = (IEnumerable<PhotoAlbumObject>)item;
                                var list = new List<PhotoAlbumObject>(obsCollection);
                                foreach (var i in list)
                                {
                                    MAdapter.AlbumList[position].PhotoAlbum.Add(i);
                                }
                                MAdapter.NotifyDataSetChanged();

                                RunOnUiThread(ShowEmptyPage);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Album 

        private void LoadAlbums()
        {
            try
            {
                switch (ListUtils.ListCachedDataAlbum.Count)
                {
                    case > 0:
                        {
                            MAdapter.AlbumList = ListUtils.ListCachedDataAlbum;
                            MAdapter.NotifyDataSetChanged();

                            var item = MAdapter.AlbumList.LastOrDefault()?.Id ?? "0";
                            StartApiService(item);
                            break;
                        }
                    default:
                        StartApiService();
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
            ListUtils.ListCachedDataAlbum = MAdapter.AlbumList.Count switch
            {
                > 0 => MAdapter.AlbumList,
                _ => ListUtils.ListCachedDataAlbum
            };

            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadAlbum(offset) });
        }

        private async Task LoadAlbum(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.AlbumList.Count;
                var (apiStatus, respond) = await RequestsAsync.Album.GetAlbumsAsync(UserDetails.UserId, "15", offset);
                if (apiStatus != 200 || respond is not GetAlbumsObject result || result.Albums == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Albums?.Count;
                    switch (respondList)
                    {
                        case > 0 when countList > 0:
                            {
                                foreach (var item in from item in result.Albums let check = MAdapter.AlbumList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.AlbumList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.AlbumList.Count - countList); });
                                break;
                            }
                        case > 0:
                            MAdapter.AlbumList = new ObservableCollection<PostDataObject>(result.Albums);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            break;
                        default:
                            {
                                switch (MAdapter.AlbumList.Count)
                                {
                                    case > 10 when !MRecycler.CanScrollVertically(1):
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short);
                                        break;
                                }

                                break;
                            }
                    }
                }

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
                MainScrollEvent.IsLoading = false;
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.AlbumList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoAlbum);
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
                MainScrollEvent.IsLoading = false;
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