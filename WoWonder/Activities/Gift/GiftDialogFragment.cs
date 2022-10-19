using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.Gift.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Gift
{
    public class GiftDialogFragment : BottomSheetDialogFragment 
    {
        #region Variables Basic

        private GiftAdapter MAdapter; 
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private string UserId;
        private RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.GiftDialogtLayout, container, false); 
                return view;
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

                UserId = Arguments.GetString("UserId");

                InitComponent(view);
                SetRecyclerViewAdapters(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
         
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
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
                MRecycler.NestedScrollingEnabled = false;
                MAdapter = new GiftAdapter(Activity)
                {
                    GiftsList = new ObservableCollection<GiftObject.DataGiftObject>()
                };
                MAdapter.OnItemClick += GiftAdapterOnItemClick; 
                LayoutManager = new GridLayoutManager(Activity, 2);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetItemViewCacheSize(20);
                MRecycler.HasFixedSize = true;
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<GiftObject.DataGiftObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                 
                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
                 
                if (ListUtils.GiftsList?.Count > 0)
                {
                    MAdapter.GiftsList = new ObservableCollection<GiftObject.DataGiftObject>(ListUtils.GiftsList);
                    MAdapter.NotifyDataSetChanged();

                    var item = MAdapter.GiftsList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                        StartApiService(item.Id);
                }
                else
                {
                    StartApiService("0");
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.GiftsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void GiftAdapterOnItemClick(object sender, GiftAdapterClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                int position = e.Position;
                switch (position)
                {
                    case > -1:
                    {
                        var item = MAdapter.GetItem(position);
                        if (item != null)
                        { 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.SendGiftAsync(UserId, item.Id) });

                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Sent_successfully), ToastLength.Short);
                            //Close Fragment 
                            Dismiss();
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

        #endregion
         
        #region Load Gifts 

        private void StartApiService(string offset)
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGifts(offset) });
        }

        private async Task LoadGifts(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapter.GiftsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Global.FetchGiftAsync("10", offset);
                if (apiStatus != 200 || respond is not GiftObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    switch (respondList)
                    {
                        case > 0:
                            {
                                switch (countList)
                                {
                                    case > 0:
                                        {
                                            foreach (var item in from item in result.Data let check = MAdapter.GiftsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                            {
                                                MAdapter.GiftsList.Add(item);
                                            }
                                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.GiftsList.Count - countList); });
                                            break;
                                        }
                                    default:
                                        MAdapter.GiftsList = new ObservableCollection<GiftObject.DataGiftObject>(result.Data);
                                        Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                        break;
                                }

                                break;
                            }
                        default:
                            {
                                switch (MAdapter.GiftsList.Count)
                                {
                                    case > 10 when !MRecycler.CanScrollVertically(1):
                                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreVideo), ToastLength.Short);
                                        break;
                                }

                                break;
                            }
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            } 
            else
            {
                MainScrollEvent.IsLoading = false;
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                switch (MAdapter.GiftsList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;

                        ListUtils.GiftsList = new ObservableCollection<GiftObject.DataGiftObject>(MAdapter.GiftsList);
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone; 
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion 
    }
}