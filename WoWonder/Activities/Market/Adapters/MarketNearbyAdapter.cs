using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Product;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Market.Adapters
{
    public class MarketNearbyAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MarketAdapterClickEventArgs> ItemClick;
        public event EventHandler<MarketAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<ProductDataObject> ProductList = new ObservableCollection<ProductDataObject>();

        public MarketNearbyAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Market_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Market_view, parent, false);
                var vh = new MarketNearbyAdpaterViewHolder(itemView, Click, LongClick);

                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case MarketNearbyAdpaterViewHolder holder:
                        {
                            var item = ProductList[position];
                            if (item != null) Initialize(holder, item);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => ProductList?.Count ?? 0;

        private void Initialize(MarketNearbyAdpaterViewHolder holder, ProductDataObject item)
        {
            try
            {
                switch (item.Images?.Count)
                {
                    case > 0 when item.Images[0].Image.Contains("http"):
                        GlideImageLoader.LoadImage(ActivityContext, item.Images?[0]?.Image, holder.Thumbnail, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        break;
                    case > 0:
                        Glide.With(ActivityContext).Load(new File(item.Images?[0]?.Image)).Apply(new RequestOptions().CircleCrop().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Thumbnail);
                        break;
                }

                holder.Title.Text = Methods.FunString.DecodeString(item.Name);

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Currency);
                System.Console.WriteLine(currency);

                holder.TxtPrice.Text = currencyIcon + item.Price;
                holder.LocationText.Text = !string.IsNullOrEmpty(item.Location) ? Methods.FunString.DecodeString(item.Location) : ActivityContext.GetText(Resource.String.Lbl_Unknown);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case MarketNearbyAdpaterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Thumbnail);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public ProductDataObject GetItem(int position)
        {
            return ProductList[position];
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
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(MarketAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MarketAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public System.Collections.IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ProductList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                switch (item.Images?.Count)
                {
                    case > 0:
                        d.Add(item.Images[0].Image);
                        d.Add(item.Seller.Avatar);
                        return d;
                    default:
                        d.Add(item.Seller.Avatar);

                        return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CenterCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class MarketNearbyAdpaterViewHolder : RecyclerView.ViewHolder
    {
        public MarketNearbyAdpaterViewHolder(View itemView, Action<MarketAdapterClickEventArgs> clickListener, Action<MarketAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Thumbnail = MainView.FindViewById<ImageView>(Resource.Id.thumbnail);
                Title = MainView.FindViewById<TextView>(Resource.Id.titleTextView);

                LocationText = MainView.FindViewById<TextView>(Resource.Id.LocationText);
                TxtPrice = MainView.FindViewById<TextView>(Resource.Id.pricetext);

                //Event
                itemView.Click += (sender, e) => clickListener(new MarketAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MarketAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Thumbnail { get; private set; }
        public TextView Title { get; private set; }

        public TextView LocationText { get; private set; }
        public TextView TxtPrice { get; private set; }
        #endregion
    }
}