using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.NearbyShops;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Product;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Market.Adapters
{
    public class MarketAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MarketAdapterClickEventArgs> ItemClick;
        public event EventHandler<MarketAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.ProductClass> MarketList = new ObservableCollection<Classes.ProductClass>();
        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }
        private MarketNearbyAdapter MarketNearbyAdapter;

        public MarketAdapter(Activity context)
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

        public override int ItemCount => MarketList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int) Classes.ItemType.NearbyShops:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                        var vh = new TemplateRecyclerViewHolder(itemView, Click, LongClick);
                        RecycledViewPool = new RecyclerView.RecycledViewPool();
                        vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                        return vh;
                    }
                    case (int) Classes.ItemType.Product:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ProudctView, parent, false);
                        var vh = new ProductAdapterViewHolder(itemView, Click, LongClick);
                        return vh;
                    }
                    case (int) Classes.ItemType.MyProduct:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MyProductView, parent, false); 
                        var vh = new MyProductAdapterViewHolder(itemView, Click, LongClick);
                        return vh;
                    }
                    case (int)Classes.ItemType.Section:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Section, parent, false);
                        var vh = new AdapterHolders.SectionViewHolder(itemView);
                        return vh;
                    }
                    default:
                        return null!;
                }
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
                var item = MarketList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.NearbyShops:
                        {
                            if (viewHolder is TemplateRecyclerViewHolder holder)
                            {
                                if (MarketNearbyAdapter == null)
                                {
                                    MarketNearbyAdapter = new MarketNearbyAdapter(ActivityContext)
                                    {
                                        ProductList = new ObservableCollection<ProductDataObject>()
                                    };

                                    LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                    holder.MRecycler.SetLayoutManager(layoutManager);
                                    holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                    var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                    var preLoader = new RecyclerViewPreloader<ProductDataObject>(ActivityContext, MarketNearbyAdapter, sizeProvider, 10);
                                    holder.MRecycler.AddOnScrollListener(preLoader);
                                    holder.MRecycler.SetAdapter(MarketNearbyAdapter);
                                    MarketNearbyAdapter.ItemClick += MarketNearbyAdapterOnItemClick;

                                    holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_NearbyShops);
                                    holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_ViewAll);
                                    holder.MoreText.Visibility = ViewStates.Visible;
                                    holder.MoreText.Click+= MoreTextOnClick;
                                }

                                var countList = item.ProductList.Count;
                                if (item.ProductList.Count is > 0 && countList > 0)
                                {
                                    foreach (var user in from user in item.ProductList let check = MarketNearbyAdapter.ProductList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select user)
                                    {
                                        MarketNearbyAdapter.ProductList.Add(user);
                                    }

                                    MarketNearbyAdapter.NotifyItemRangeInserted(countList, MarketNearbyAdapter.ProductList.Count - countList);
                                }
                                else if (item.ProductList.Count is > 0)
                                {
                                    MarketNearbyAdapter.ProductList = new ObservableCollection<ProductDataObject>(item.ProductList);
                                    MarketNearbyAdapter.NotifyDataSetChanged();
                                }
                            }

                            break;
                        }
                        case Classes.ItemType.Product:
                        {
                            if (viewHolder is ProductAdapterViewHolder holder)
                            {
                                switch (item.Product?.Images?.Count)
                                {
                                    case > 0 when item.Product.Images[0].Image.Contains("http"):
                                        GlideImageLoader.LoadImage(ActivityContext, item.Product.Images?[0]?.Image, holder.Thumbnail, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                        break;
                                    case > 0:
                                        Glide.With(ActivityContext).Load(new File(item.Product.Images?[0]?.Image)).Apply(new RequestOptions().CircleCrop().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Thumbnail);
                                        break;
                                }

                                holder.Title.Text = Methods.FunString.DecodeString(item.Product.Name);

                                var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Product.Currency);
                                Console.WriteLine(currency);

                                holder.TxtPrice.Text = currencyIcon + item.Product.Price;
                                holder.LocationText.Text = !string.IsNullOrEmpty(item.Product.Location) ? Methods.FunString.DecodeString(item.Product.Location) : ActivityContext.GetText(Resource.String.Lbl_Unknown);
                            }

                            break;
                        }
                        case Classes.ItemType.MyProduct:
                        {
                            if (viewHolder is MyProductAdapterViewHolder holder)
                            {
                                switch (item.Product?.Images?.Count)
                                {
                                    case > 0 when item.Product != null && item.Product.Images[0].Image.Contains("http"):
                                        GlideImageLoader.LoadImage(ActivityContext, item.Product?.Images?[0]?.Image, holder.Thumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        break;
                                    case > 0:
                                        Glide.With(ActivityContext).Load(new File(item.Product?.Images?[0]?.Image)).Apply(new RequestOptions().CenterCrop().Placeholder(Resource.Drawable.ImagePlacholder).Error(Resource.Drawable.ImagePlacholder)).Into(holder.Thumbnail);
                                        break;
                                }

                                GlideImageLoader.LoadImage(ActivityContext, item.Product?.Seller?.Avatar, holder.Userprofilepic, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                                     
                                holder.Title.Text = Methods.FunString.DecodeString(item.Product?.Name);

                                holder.UserName.Text = WoWonderTools.GetNameFinal(item.Product?.Seller);
                                holder.Time.Text = item.Product?.TimeText;

                                var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Product?.Currency);
                                Console.WriteLine(currency);

                                holder.TxtPrice.Text = item.Product?.Price + " " + currencyIcon;
                                holder.LocationText.Text = !string.IsNullOrEmpty(item.Product?.Location) ? item.Product?.Location : ActivityContext.GetText(Resource.String.Lbl_Unknown);
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
                    }
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
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(NearbyShopsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MarketNearbyAdapterOnItemClick(object sender, MarketAdapterClickEventArgs e)
        {
            try
            {
                var item = MarketNearbyAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(ProductViewActivity));
                    intent.PutExtra("Id", item.PostId);
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case MyProductAdapterViewHolder viewHolder:
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
        public Classes.ProductClass GetItem(int position)
        {
            return MarketList[position];
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
                var item = MarketList[position];
                return item.Type switch
                {
                    Classes.ItemType.NearbyShops => (int) Classes.ItemType.NearbyShops,
                    Classes.ItemType.Product => (int) Classes.ItemType.Product,
                    Classes.ItemType.MyProduct => (int) Classes.ItemType.MyProduct,
                    Classes.ItemType.Section => (int)Classes.ItemType.Section,
                    _ => (int) Classes.ItemType.Product
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)Classes.ItemType.Product;
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

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MarketList[p0];
                if (item == null || item.Type == Classes.ItemType.NearbyShops)
                    return Collections.SingletonList(p0);

                switch (item.Product?.Images?.Count)
                {
                    case > 0:
                        d.Add(item.Product?.Images[0].Image);
                        d.Add(item.Product?.Seller.Avatar);
                        return d;
                    default:
                        d.Add(item.Product?.Seller.Avatar); 
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
     
    public class TemplateRecyclerViewHolder : RecyclerView.ViewHolder
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

        public TemplateRecyclerViewHolder(View itemView, Action<MarketAdapterClickEventArgs> clickListener, Action<MarketAdapterClickEventArgs> longClickListener) : base(itemView)
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
                itemView.Click += (sender, e) => clickListener(new MarketAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MarketAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
     
    public class ProductAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ProductAdapterViewHolder(View itemView, Action<MarketAdapterClickEventArgs> clickListener,Action<MarketAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            { 
                MainView = itemView;

                Thumbnail = MainView.FindViewById<ImageView>(Resource.Id.thumbnail);
                Title = MainView.FindViewById<TextView>(Resource.Id.titleTextView);

                LocationText = MainView.FindViewById<TextView>(Resource.Id.LocationText);
                TxtPrice = MainView.FindViewById<TextView>(Resource.Id.pricetext);


                //Event
                itemView.Click += (sender, e) => clickListener(new MarketAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new MarketAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});

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
    

    public class MyProductAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MyProductAdapterViewHolder(View itemView, Action<MarketAdapterClickEventArgs> clickListener,Action<MarketAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Thumbnail = MainView.FindViewById<ImageView>(Resource.Id.thumbnail);
                Title = MainView.FindViewById<TextView>(Resource.Id.titleTextView);

                LocationText = MainView.FindViewById<TextView>(Resource.Id.LocationText);
                Userprofilepic = MainView.FindViewById<ImageView>(Resource.Id.userprofile_pic);
                UserName = MainView.FindViewById<TextView>(Resource.Id.User_name);
                Time = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                TxtPrice = MainView.FindViewById<TextView>(Resource.Id.pricetext);
                
                //Event
                itemView.Click += (sender, e) => clickListener(new MarketAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new MarketAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
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

        public ImageView Userprofilepic { get; private set; }
        public TextView UserName { get; private set; }
        public TextView Time { get; private set; }
        public TextView LocationText { get; private set; }
        public TextView TxtPrice { get; private set; }
       

        #endregion
    }
     
    public class MarketAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    } 
}