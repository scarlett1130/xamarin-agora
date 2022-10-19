using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Util;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Games;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Games.Adapters
{
    public class GamesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GamesAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<GamesAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }
        public ObservableCollection<Classes.GameClass> GamesList = new ObservableCollection<Classes.GameClass>();

        private PopularGamesAdapter PopularGamesAdapter;
        private MyGamesAdapter RecommendGamesAdapter, SearchGamesAdapter;
        private RecentGamesAdapter RecentGamesAdapter;

        public GamesAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)Classes.ItemType.RecommendGame:
                    case (int)Classes.ItemType.PopularGame:
                    case (int)Classes.ItemType.RecentGame:
                    case (int)Classes.ItemType.SearchGame:
                        {
                            View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                            var vh = new TemplateRecyclerViewHolder(itemView, OnClick, OnLongClick);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                            return vh;
                        }
                    case (int)Classes.ItemType.Section:
                        {
                            View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Section, parent, false);
                            var vh = new AdapterHolders.SectionViewHolder(itemView);
                            return vh;
                        }
                    case (int)Classes.ItemType.Divider:
                        {
                            View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Devider, parent, false);
                            var vh = new AdapterHolders.PostDividerSectionViewHolder(itemView);
                            return vh;
                        }
                    default:
                        {
                            return null;
                        }
                }
                //Setup your layout here >> Style_LastActivities_View
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = GamesList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.PopularGame:
                            {
                                if (viewHolder is TemplateRecyclerViewHolder holder)
                                {
                                    if (PopularGamesAdapter == null)
                                    {
                                        PopularGamesAdapter = new PopularGamesAdapter(ActivityContext, "Wrap_Content")
                                        {
                                            GamesList = new ObservableCollection<GamesDataObject>()
                                        };

                                        LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                        
                                        holder.MRecycler.SetLayoutManager(layoutManager);
                                        holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                        holder.MRecycler.NestedScrollingEnabled = false;

                                        var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                        var preLoader = new RecyclerViewPreloader<GamesDataObject>(ActivityContext, PopularGamesAdapter, sizeProvider, 10);
                                        holder.MRecycler.AddOnScrollListener(preLoader);
                                        holder.MRecycler.SetAdapter(PopularGamesAdapter);
                                        //holder.MRecycler.AddOnItemTouchListener(new RecyclerViewOnItemTouch(holder.MRecycler, GamesActivity.GetInstance()?.ViewPager));
                                        PopularGamesAdapter.ItemClick += PopularGamesAdapterOnItemClick;

                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_PopularGames);
                                        holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_ViewAll);
                                        holder.MoreText.Visibility = ViewStates.Visible;
                                        holder.MoreText.Click += MoreTextOnClick;
                                    }

                                    var countList = item.GameList.Count;
                                    if (item.GameList.Count is > 0 && countList > 0)
                                    {
                                        foreach (var user in from user in item.GameList let check = PopularGamesAdapter.GamesList.FirstOrDefault(a => a.Id == user.Id) where check == null select user)
                                        {
                                            PopularGamesAdapter.GamesList.Add(user);
                                        }

                                        PopularGamesAdapter.NotifyItemRangeInserted(countList, PopularGamesAdapter.GamesList.Count - countList);
                                    }
                                    else if (item.GameList.Count is > 0)
                                    {
                                        PopularGamesAdapter.GamesList = new ObservableCollection<GamesDataObject>(item.GameList);
                                        PopularGamesAdapter.NotifyDataSetChanged();
                                    }
                                }
                                break;
                            }
                        case Classes.ItemType.RecommendGame:
                            {
                                if (viewHolder is TemplateRecyclerViewHolder holder)
                                {
                                    if (RecommendGamesAdapter == null)
                                    {
                                        RecommendGamesAdapter = new MyGamesAdapter(ActivityContext)
                                        {
                                            GamesList = new ObservableCollection<GamesDataObject>()
                                        };

                                        GridLayoutManager layoutManager = new GridLayoutManager(ActivityContext, 2);
                                        layoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1));

                                        holder.MRecycler.SetLayoutManager(layoutManager);
                                        holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                        var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                        var preLoader = new RecyclerViewPreloader<GamesDataObject>(ActivityContext, RecommendGamesAdapter, sizeProvider, 10);
                                        holder.MRecycler.AddOnScrollListener(preLoader);
                                        holder.MRecycler.SetAdapter(RecommendGamesAdapter);
                                        //holder.MRecycler.AddOnItemTouchListener(new RecyclerViewOnItemTouch(holder.MRecycler, GamesActivity.GetInstance()?.ViewPager));
                                        RecommendGamesAdapter.ItemClick += RecommendGamesAdapterOnItemClick;

                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_GameRecommendations);
                                        holder.MoreText.Visibility = ViewStates.Gone;
                                    }

                                    var countList = item.GameList.Count;
                                    if (item.GameList.Count is > 0 && countList > 0)
                                    {
                                        foreach (var user in from user in item.GameList let check = RecommendGamesAdapter.GamesList.FirstOrDefault(a => a.Id == user.Id) where check == null select user)
                                        {
                                            RecommendGamesAdapter.GamesList.Add(user);
                                        }

                                        RecommendGamesAdapter.NotifyItemRangeInserted(countList, RecommendGamesAdapter.GamesList.Count - countList);
                                    }
                                    else if (item.GameList.Count is > 0)
                                    {
                                        RecommendGamesAdapter.GamesList = new ObservableCollection<GamesDataObject>(item.GameList);
                                        RecommendGamesAdapter.NotifyDataSetChanged();
                                    }
                                }
                                break;
                            }
                        case Classes.ItemType.RecentGame:
                            {
                                if (viewHolder is TemplateRecyclerViewHolder holder)
                                {
                                    if (RecentGamesAdapter == null)
                                    {
                                        RecentGamesAdapter = new RecentGamesAdapter(ActivityContext)
                                        {
                                            GamesList = new ObservableCollection<GamesDataObject>()
                                        };

                                        LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);

                                        holder.MRecycler.SetLayoutManager(layoutManager);
                                        holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                        holder.MRecycler.NestedScrollingEnabled = false;

                                        var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                        var preLoader = new RecyclerViewPreloader<GamesDataObject>(ActivityContext, RecentGamesAdapter, sizeProvider, 10);
                                        holder.MRecycler.AddOnScrollListener(preLoader);
                                        holder.MRecycler.SetAdapter(RecentGamesAdapter);
                                        //holder.MRecycler.AddOnItemTouchListener(new RecyclerViewOnItemTouch(holder.MRecycler, GamesActivity.GetInstance()?.ViewPager));
                                        RecentGamesAdapter.ItemClick += PopularGamesAdapterOnItemClick;

                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_RecentlyPlayed);
                                        holder.MoreText.Visibility = ViewStates.Gone;
                                    }

                                    var countList = item.GameList.Count;
                                    if (item.GameList.Count is > 0 && countList > 0)
                                    {
                                        foreach (var user in from user in item.GameList let check = RecentGamesAdapter.GamesList.FirstOrDefault(a => a.Id == user.Id) where check == null select user)
                                        {
                                            RecentGamesAdapter.GamesList.Add(user);
                                        }

                                        RecentGamesAdapter.NotifyItemRangeInserted(countList, RecentGamesAdapter.GamesList.Count - countList);
                                    }
                                    else if (item.GameList.Count is > 0)
                                    {
                                        RecentGamesAdapter.GamesList = new ObservableCollection<GamesDataObject>(item.GameList);
                                        RecentGamesAdapter.NotifyDataSetChanged();
                                    }
                                }
                                break;
                            }
                        case Classes.ItemType.SearchGame:
                            {
                                if (viewHolder is TemplateRecyclerViewHolder holder)
                                {
                                    if (SearchGamesAdapter == null)
                                    {
                                        SearchGamesAdapter = new MyGamesAdapter(ActivityContext)
                                        {
                                            GamesList = new ObservableCollection<GamesDataObject>()
                                        };

                                        GridLayoutManager layoutManager = new GridLayoutManager(ActivityContext, 2);
                                        layoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1));

                                        holder.MRecycler.SetLayoutManager(layoutManager);
                                        holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                        var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                        var preLoader = new RecyclerViewPreloader<GamesDataObject>(ActivityContext, SearchGamesAdapter, sizeProvider, 10);
                                        holder.MRecycler.AddOnScrollListener(preLoader);
                                        holder.MRecycler.SetAdapter(SearchGamesAdapter);
                                        //holder.MRecycler.AddOnItemTouchListener(new RecyclerViewOnItemTouch(holder.MRecycler, GamesActivity.GetInstance()?.ViewPager));
                                        SearchGamesAdapter.ItemClick += SearchGamesAdapterOnItemClick;

                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_SearchResults);   
                                        holder.MoreText.Visibility = ViewStates.Gone;
                                    }

                                    var countList = item.GameList.Count;
                                    if (item.GameList.Count is > 0 && countList > 0)
                                    {
                                        foreach (var user in from user in item.GameList let check = SearchGamesAdapter.GamesList.FirstOrDefault(a => a.Id == user.Id) where check == null select user)
                                        {
                                            SearchGamesAdapter.GamesList.Add(user);
                                        }

                                        SearchGamesAdapter.NotifyItemRangeInserted(countList, SearchGamesAdapter.GamesList.Count - countList);
                                    }
                                    else if (item.GameList.Count is > 0)
                                    {
                                        SearchGamesAdapter.GamesList = new ObservableCollection<GamesDataObject>(item.GameList);
                                        SearchGamesAdapter.NotifyDataSetChanged();
                                    }
                                }
                                break;
                            }
                        case Classes.ItemType.Section:
                            {
                                switch (viewHolder)
                                {
                                    case AdapterHolders.SectionViewHolder holder:
                                        {
                                            holder.AboutHead.Text = item.Title;

                                            break;
                                        }
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchGamesAdapterOnItemClick(object sender, MyGamesAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = SearchGamesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(GamesViewActivity));
                    intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecommendGamesAdapterOnItemClick(object sender, MyGamesAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = RecommendGamesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(GamesViewActivity));
                    intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreTextOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(PopularGamesActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopularGamesAdapterOnItemClick(object sender, GamesAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = PopularGamesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(GamesViewActivity));
                    intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                        return;

                switch (holder)
                {
                    case GamesAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public override int ItemCount => GamesList?.Count ?? 0;

        public Classes.GameClass GetItem(int position)
        {
            return GamesList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = GamesList[position];
                return item.Type switch
                {
                    Classes.ItemType.RecentGame => (int)Classes.ItemType.RecentGame,
                    Classes.ItemType.RecommendGame => (int)Classes.ItemType.RecommendGame,
                    Classes.ItemType.PopularGame => (int)Classes.ItemType.PopularGame,
                    Classes.ItemType.MyGame => (int)Classes.ItemType.MyGame,
                    Classes.ItemType.Section => (int)Classes.ItemType.Section,
                    Classes.ItemType.Divider => (int)Classes.ItemType.Divider,
                    _ => (int)Classes.ItemType.PopularGame
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnClick(GamesAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(GamesAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GamesList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                    {
                        switch (string.IsNullOrEmpty(item.Game?.GameAvatar))
                        {
                            case false:
                                d.Add(item.Game?.GameAvatar);
                                break;
                        }

                        return d;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class GamesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }

        #endregion

        public GamesAdapterViewHolder(View itemView, Action<GamesAdapterViewHolderClickEventArgs> clickListener, Action<GamesAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                Title = (TextView)MainView.FindViewById(Resource.Id.title);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new GamesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GamesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    class TemplateRecyclerViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public LinearLayout MainLinear { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView IconTitle { get; private set; }
        public TextView DescriptionText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public TemplateRecyclerViewHolder(View itemView, Action<GamesAdapterViewHolderClickEventArgs> clickListener, Action<GamesAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLinear = (LinearLayout)itemView.FindViewById(Resource.Id.mainLinear);
                TitleText = (TextView)itemView.FindViewById(Resource.Id.textTitle);
                IconTitle = (TextView)itemView.FindViewById(Resource.Id.iconTitle);
                DescriptionText = (TextView)itemView.FindViewById(Resource.Id.textSecondery);
                MoreText = (TextView)itemView.FindViewById(Resource.Id.textMore);
                MRecycler = (RecyclerView)itemView.FindViewById(Resource.Id.recyler);

                IconTitle.Visibility = ViewStates.Gone;
                DescriptionText.Visibility = ViewStates.Gone;

                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new GamesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GamesAdapterViewHolderClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }



    public class GamesAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}