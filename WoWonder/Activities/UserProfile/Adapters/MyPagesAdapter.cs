using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.UserProfile.Adapters
{
    public class MyPagesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MyPagesAdapterClickEventArgs> ItemClick;
        public event EventHandler<MyPagesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PageDataObject> PageList = new ObservableCollection<PageDataObject>();

        public MyPagesAdapter(Activity context)
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

        public override int ItemCount => PageList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case MyPagesAdapterViewHolder holder:
                        {
                            var item = PageList[position];
                            if (item != null)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.PageImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                string name = Methods.FunString.DecodeString(item.PageTitle);
                                holder.PageName.Text = name;

                                CategoriesController cat = new CategoriesController();
                                holder.PageNotification.Text = cat.Get_Translate_Categories_Communities(item.PageCategory, item.Category, "Page");

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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MyPage_View, parent, false);
                var vh = new MyPagesAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
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
                    case MyPagesAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.PageImage);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public PageDataObject GetItem(int position)
        {
            return PageList[position];
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

        private void Click(MyPagesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MyPagesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public System.Collections.IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PageList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                        {
                            switch (string.IsNullOrEmpty(item.Avatar))
                            {
                                case false:
                                    d.Add(item.Avatar);
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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class MyPagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public ImageView PageImage { get; set; }
        public TextView PageName { get; set; }
        public TextView PageNotification { get; set; }

        public MyPagesAdapterViewHolder(View itemView, Action<MyPagesAdapterClickEventArgs> clickListener,
            Action<MyPagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                PageImage = itemView.FindViewById<ImageView>(Resource.Id.PageImage);
                PageName = itemView.FindViewById<TextView>(Resource.Id.Page_Name);
                PageNotification = itemView.FindViewById<TextView>(Resource.Id.Page_Notifications);

                //Event
                itemView.Click += (sender, e) => clickListener(new MyPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MyPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class MyPagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}