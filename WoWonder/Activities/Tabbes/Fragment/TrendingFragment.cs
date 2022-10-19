using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Covid19;
using WoWonder.Activities.FriendRequest;
using WoWonder.Activities.FriendsBirthday;
using WoWonder.Activities.Hashtag;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class TrendingFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        private TabbedMainActivity GlobalContext;
        private RecyclerView MRecycler;
        public TrendingAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.Tab_Trending_Layout, container, false); 
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

                Task.Factory.StartNew(StartApiService);
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

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

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
                LayoutManager = new LinearLayoutManager(Activity);
                MAdapter = new TrendingAdapter(Activity)
                {
                    TrendingList = new ObservableCollection<Classes.TrendingClass>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;  
                MRecycler.SetAdapter(MAdapter); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Event
         
        private void MAdapterOnItemClick(object sender, TrendingAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                switch (item?.Type)
                {
                    case Classes.ItemType.FriendRequest:
                    {
                        var intent = new Intent(Activity, typeof(FriendRequestActivity));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.FriendsBirthday:
                    {
                        var dataUser = item.UserList?.FirstOrDefault();
                        if (dataUser != null)
                            WoWonderTools.OpenProfile(Activity, dataUser.UserId, dataUser);
                        break;
                    }
                    case Classes.ItemType.LastActivities when item.LastActivities.ActivityType == "following" || item.LastActivities.ActivityType == "friend":
                        WoWonderTools.OpenProfile(Activity, item.LastActivities.UserId, item.LastActivities.Activator);
                        break;
                    case Classes.ItemType.LastActivities:
                    {
                        var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                        intent.PutExtra("Id", item.LastActivities.PostId);
                        //intent.PutExtra("DataItem", JsonConvert.SerializeObject(item.PostData));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.HashTag:
                    {
                        string id = item.HashTags.Hash.Replace("#", "").Replace("_", " ");
                        string tag = item.HashTags?.Tag?.Replace("#", "");
                        var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                        intent.PutExtra("Id", id);
                        intent.PutExtra("Tag", tag);
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.HashTag:
                    {
                        var intent = new Intent(Activity, typeof(HashTagActivity));
                        Activity.StartActivity(intent);
                        break;
                    } 
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.LastActivities:
                    {
                        var intent = new Intent(Activity, typeof(LastActivitiesActivity));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.FriendsBirthday:
                    {
                        var intent = new Intent(Activity, typeof(FriendsBirthdayActivity));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.LastBlogs:
                    {
                        var intent = new Intent(Activity, typeof(ArticlesViewActivity));
                        intent.PutExtra("Id", item.LastBlogs.Id);
                        intent.PutExtra("ArticleObject", JsonConvert.SerializeObject(item.LastBlogs));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.LastBlogs:
                    {
                        var intent = new Intent(Activity, typeof(ArticlesActivity));
                        Activity.StartActivity(intent);
                        break;
                    }
                    case Classes.ItemType.CoronaVirus:
                    {
                        var intent = new Intent(Context, typeof(Covid19Activity));
                        Activity.StartActivity(intent);
                        break;
                    }
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
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Activities & Weather & Blogs & Friends Birthday

        private async Task StartSearchRequest()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "30"},
                {"user_offset", "0"},
                //{"group_offset", OffsetGroup},
                //{"page_offset", OffsetPage},
                {"gender", UserDetails.SearchGender},
                {"search_key", "a"},
                {"country", UserDetails.SearchCountry},
                {"status", "on"},
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
                                        case > 0 when AppSettings.ShowProUsersMembers:
                                            {
                                                var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ProUser);
                                                switch (checkList)
                                                {
                                                    case null:
                                                        {
                                                            var proUser = new Classes.TrendingClass
                                                            {
                                                                Id = 100,
                                                                UserList = new List<UserDataObject>(),
                                                                Type = Classes.ItemType.ProUser
                                                            };

                                                            foreach (var item in from item in result.Users let check = proUser.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                            {
                                                                proUser.UserList.Add(item);
                                                            }

                                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(0, proUser);
                                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(1, new Classes.TrendingClass
                                                            {
                                                                Type = Classes.ItemType.Divider
                                                            });
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            foreach (var item in from item in result.Users let check = checkList.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                            {
                                                                checkList.UserList.Add(item);
                                                            }

                                                            break;
                                                        }
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
                    Methods.DisplayReportResult(Activity, respond);
                    break;
            }
            SwipeRefreshLayout.Refreshing = false;
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync(), StartSearchRequest }); 
        }

        private async Task LoadActivitiesAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowLastActivities)
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetActivitiesAsync("6", offset);
                    if (apiStatus == 200)
                    {
                        if (respond is LastActivitiesObject result)
                        {
                            // LastActivities
                            var respondListLastActivities = result.Activities.Count;
                            if (respondListLastActivities > 0)
                            {
                                var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.LastActivities);
                                if (checkList == null)
                                {
                                    GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                    {
                                        Id = 900,
                                        Title = Activity.GetText(Resource.String.Lbl_LastActivities),
                                        SectionType = Classes.ItemType.LastActivities,
                                        Type = Classes.ItemType.Section,
                                    });

                                    var list = result.Activities.Take(5).ToList();

                                    foreach (var item in from item in list let check = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(a => a.LastActivities?.Id == item.Id && a.Type == Classes.ItemType.LastActivities) where check == null select item)
                                    {
                                        GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                        {
                                            Id = long.Parse(item.Id),
                                            LastActivities = item,
                                            Type = Classes.ItemType.LastActivities
                                        });
                                    }

                                    GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                    {
                                        Type = Classes.ItemType.Divider
                                    });
                                }
                            }
                        }
                    }
                    else
                        Methods.DisplayReportResult(Activity, respond);
                }

                await GetFriendsBirthdayApi();
                //await GetWeatherApi();
                await GetExchangeCurrencyApi();

                Activity?.RunOnUiThread(ShowEmptyPage);
            }  
        }
      
        private async Task GetWeatherApi()
        {
            if (AppSettings.ShowWeather && Methods.CheckConnectivity())
            {
                GetWeatherObject respond = await ApiRequest.GetWeather();
                if (respond != null)
                {
                    var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.Weather);
                    switch (checkList)
                    {
                        case null:
                        {
                            var weather = new Classes.TrendingClass
                            {
                                Id = 600,
                                Weather = respond,
                                Type = Classes.ItemType.Weather
                            };

                            MAdapter.TrendingList.Add(weather);
                            MAdapter.TrendingList.Add(new Classes.TrendingClass
                            {
                                Type = Classes.ItemType.Divider
                            });
                            break;
                        }
                        default:
                            checkList.Weather = respond;
                            break;
                    }
                }
            }
        }
        
        private async Task GetFriendsBirthdayApi()
        {
            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowFriendsBirthday)
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsBirthdayAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is ListUsersObject result)
                        {
                            // FriendsBirthday
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                var checkListFriendsBirthday = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.FriendsBirthday);
                                if (checkListFriendsBirthday == null)
                                {
                                    MAdapter.TrendingList.Add(new Classes.TrendingClass
                                    {
                                        Id = 950,
                                        Title = Activity.GetText(Resource.String.Lbl_FriendsBirthday),
                                        SectionType = Classes.ItemType.FriendsBirthday,
                                        Type = Classes.ItemType.Section,
                                    });

                                    MAdapter.TrendingList.Add(new Classes.TrendingClass
                                    {
                                        Id = 951,
                                        UserList = new List<UserDataObject>(result.Data),
                                        Type = Classes.ItemType.FriendsBirthday
                                    });

                                    MAdapter.TrendingList.Add(new Classes.TrendingClass
                                    {
                                        Type = Classes.ItemType.Divider
                                    });
                                }
                            }
                        }
                    }
                    else
                        Methods.DisplayReportResult(Activity, respond);
                }
            } 
        }

        private async Task GetExchangeCurrencyApi()
        {
            if (AppSettings.ShowExchangeCurrency && Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await ApiRequest.GetExchangeCurrencyAsync();
                if (apiStatus != 200 || respond is not Classes.ExchangeCurrencyObject result || result.Rates == null)
                {
                    switch (AppSettings.SetApisReportMode)
                    {
                        case true when apiStatus != 400 && respond is Classes.ExErrorObject error:
                            Methods.DialogPopup.InvokeAndShowDialog(Activity, "ReportMode", error?.Description, "Close");
                            break;
                    }
                }
                else
                {
                    var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ExchangeCurrency);
                    switch (checkList)
                    {
                        case null:
                        {
                            var exchangeCurrency = new Classes.TrendingClass
                            {
                                Id = 2013,
                                ExchangeCurrency = result,
                                Type = Classes.ItemType.ExchangeCurrency
                            };

                            MAdapter.TrendingList.Add(exchangeCurrency);
                            MAdapter.TrendingList.Add(new Classes.TrendingClass
                            {
                                Type = Classes.ItemType.Divider
                            });
                            break;
                        }
                        default:
                            checkList.ExchangeCurrency = result;
                            break;
                    }
                }
            }
        }

        private void ShowEmptyPage()
        {
            try
            {  
                var respondListShortcuts = ListUtils.ShortCutsList?.Count;
                switch (respondListShortcuts)
                {
                    case > 0 when AppSettings.ShowShortcuts:
                    { 
                        var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.Shortcuts);
                        switch (checkList)
                        {
                            case null:
                            {
                                var shortcuts = new Classes.TrendingClass
                                {
                                    Id = 700,
                                    ShortcutsList = new List<Classes.ShortCuts>(ListUtils.ShortCutsList),
                                    Type = Classes.ItemType.Shortcuts
                                };

                                //shortcuts.ShortcutsList = shortcuts.ShortcutsList.OrderBy(cuts => cuts.SocialId).ToList();
                              
                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(shortcuts);
                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Type = Classes.ItemType.Divider
                                });
                                break;
                            }
                        }

                        break;
                    }
                }

                var respondLastBlogs = ListUtils.ListCachedDataArticle?.Count;
                switch (respondLastBlogs)
                {
                    case > 0:
                    {
                        var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.LastBlogs);
                        if (checkList == null)
                        {
                            GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                            {
                                Id = 1200,
                                Title = Activity.GetText(Resource.String.Lbl_LastBlogs),
                                SectionType = Classes.ItemType.LastBlogs,
                                Type = Classes.ItemType.Section,
                            });

                            var list = ListUtils.ListCachedDataArticle.Take(3)?.ToList();

                            foreach (var item in from item in list let check = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(a => a.LastBlogs?.Id == item.Id && a.Type == Classes.ItemType.LastBlogs) where check == null select item)
                            {
                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Id = long.Parse(item.Id),
                                    LastBlogs = item,
                                    Type = Classes.ItemType.LastBlogs
                                });
                            }

                            GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                            {
                                Type = Classes.ItemType.Divider
                            });
                        }

                        break;
                    }
                }

                switch (MAdapter.TrendingList.Count)
                {
                    case > 0:
                    {
                        var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(a => a.Type == Classes.ItemType.EmptyPage);
                        if (emptyStateChecker != null)
                        {
                            MAdapter.TrendingList.Remove(emptyStateChecker);
                        }

                        var adMob = MAdapter.TrendingList.FirstOrDefault(a => a.Type == Classes.ItemType.AdMob);
                        switch (adMob)
                        {
                            case null when AppSettings.ShowAdMobBanner:
                                MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Type = Classes.ItemType.AdMob
                                });
                                break;
                        }
                     
                        MAdapter.NotifyDataSetChanged();
                        break;
                    }
                    default:
                    {
                        var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.EmptyPage);
                        switch (emptyStateChecker)
                        {
                            case null:
                                MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Id = 1000,
                                    Type = Classes.ItemType.EmptyPage
                                });
                                MAdapter.NotifyDataSetChanged();
                                break;
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
         
    }
}