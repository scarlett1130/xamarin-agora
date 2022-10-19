
using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using IList = System.Collections.IList;
using AT.Markushi.UI;

namespace WoWonder.Activities.MyPhoto.Adapters
{
    public class MyPhotosAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MyPhotosAdapterClickEventArgs> ItemClick;
        public event EventHandler<MyPhotosAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<MyPhotosAdapterClickEventArgs> CloseItemClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PostDataObject> MyPhotosList = new ObservableCollection<PostDataObject>();

        public MyPhotosAdapter(Activity context)
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

        public override int ItemCount => MyPhotosList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Image_Simple
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ImageAlbum_view, parent, false);
                var vh = new MyPhotosAdapterViewHolder(itemView, Click, LongClick, CloseClick);
                return vh;
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
                switch (viewHolder)
                {
                    case MyPhotosAdapterViewHolder holder:
                    {
                        var item = MyPhotosList[position];
                        if (item != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.PostFileFull, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                                if (item.Name != null)
                                {
                                    holder.tvTitle.Text = item.Name;
                                }
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
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                        return;

                switch (holder)
                {
                    case MyPhotosAdapterViewHolder viewHolder:
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
        public PostDataObject GetItem(int position)
        {
            return MyPhotosList[position];
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

        private void Click(MyPhotosAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MyPhotosAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        private void CloseClick(MyPhotosAdapterClickEventArgs args)
        {
            CloseItemClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MyPhotosList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                    {
                        switch (string.IsNullOrEmpty(item.PostFileFull))
                        {
                            case false:
                                d.Add(item.PostFileFull);
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

    public class MyPhotosAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MyPhotosAdapterViewHolder(View itemView, Action<MyPhotosAdapterClickEventArgs> clickListener, Action<MyPhotosAdapterClickEventArgs> longClickListener, Action<MyPhotosAdapterClickEventArgs> closeClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                CloseImage = MainView.FindViewById<CircleButton>(Resource.Id.iv_image_close);
                CloseImage.Visibility = ViewStates.Visible;
                tvTitle = MainView.FindViewById<TextView>(Resource.Id.tv_photo_title);

                //Event
                itemView.Click += (sender, e) => clickListener(new MyPhotosAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MyPhotosAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                CloseImage.Click += (sender, e) => closeClickListener(new MyPhotosAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }
         
        public ImageView Image { get; private set; }

        public CircleButton CloseImage { get; private set; }

        public TextView tvTitle { get; private set; }

        #endregion
    }

    public class MyPhotosAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}