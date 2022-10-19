using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Games;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Games.Adapters
{
    public class PopularGamesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GamesAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<GamesAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<GamesDataObject> GamesList = new ObservableCollection<GamesDataObject>();
        private readonly string WidthType;

        public PopularGamesAdapter(Activity context, string widthType)
        {
            try
            {
                ActivityContext = context;
                WidthType = widthType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => GamesList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = GamesList[position];
                if (item != null)
                {
                    if (viewHolder is GamesPopularAdapterViewHolder holder)
                    {
                        if (WidthType == "Match_Parent")
                        {
                            //var newLayout = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, holder.LytParent.LayoutParameters.Height);
                            //holder.LytParent.LayoutParameters.Width = LinearLayout.LayoutParams.MatchParent;
                            holder.LytParent.LayoutParameters.Width = ViewGroup.LayoutParams.MatchParent;
                        }
                        GlideImageLoader.LoadImage(ActivityContext, item.GameAvatar, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        holder.Title.Text = Methods.FunString.DecodeString(item.GameName);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView = LayoutInflater.From(parent.Context)
                                    .Inflate(Resource.Layout.Style_Popular_Game, parent, false);
                var vh = new GamesPopularAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        void OnClick(GamesAdapterViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);

        void OnLongClick(GamesAdapterViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public GamesDataObject GetItem(int position)
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
                            switch (string.IsNullOrEmpty(item.GameAvatar))
                            {
                                case false:
                                    d.Add(item.GameAvatar);
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

    public class GamesPopularAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public CardView LytParent { get; private set; }

        #endregion

        public GamesPopularAdapterViewHolder(View itemView, Action<GamesAdapterViewHolderClickEventArgs> clickListener, Action<GamesAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                Title = (TextView)MainView.FindViewById(Resource.Id.title);
                LytParent = MainView.FindViewById<CardView>(Resource.Id.lyt_parent);

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

}