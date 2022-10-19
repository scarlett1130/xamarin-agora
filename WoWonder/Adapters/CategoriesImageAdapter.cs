using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Adapters
{
    public class CategoriesImageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CategoriesImageAdapterClickEventArgs> ItemClick;
        public event EventHandler<CategoriesImageAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.Categories> CategoriesList = new ObservableCollection<Classes.Categories>();

        private int[] Ids = {Resource.Drawable.category_car, Resource.Drawable.category_comedy, Resource.Drawable.category_economics, Resource.Drawable.category_education,
                             Resource.Drawable.category_entertainment, Resource.Drawable.category_movies, Resource.Drawable.category_gaming, Resource.Drawable.category_history,
                             Resource.Drawable.category_live_style, Resource.Drawable.category_natural, Resource.Drawable.category_news, Resource.Drawable.category_people,
                             Resource.Drawable.category_pet, Resource.Drawable.category_place, Resource.Drawable.category_science, Resource.Drawable.category_sport, Resource.Drawable.category_travel,
                             Resource.Drawable.category_other};

        public CategoriesImageAdapter(Activity context)
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

        public override int ItemCount => CategoriesList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_CategoryImageView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CategoryImageView, parent, false);
                var vh = new CategoriesImageAdapterViewHolder(itemView, Click, LongClick);
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
                    case CategoriesImageAdapterViewHolder holder:
                    {
                        var item = CategoriesList[position];
                        if (item != null)
                        { 
                            holder.Name.Text = Methods.FunString.DecodeString(item.CategoriesName);
                            holder.Image.SetImageResource(Ids[position]);
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

        public Classes.Categories GetItem(int position)
        {
            return CategoriesList[position];
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

        private void Click(CategoriesImageAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(CategoriesImageAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CategoriesList[p0];
                return item switch
                {
                    null => d,
                    _ => d
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class CategoriesImageAdapterViewHolder : RecyclerView.ViewHolder
    {
        public CategoriesImageAdapterViewHolder(View itemView, Action<CategoriesImageAdapterClickEventArgs> clickListener,Action<CategoriesImageAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                Name = MainView.FindViewById<TextView>(Resource.Id.cat);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new CategoriesImageAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CategoriesImageAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

           
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }
         
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }

        #endregion
    }

    public class CategoriesImageAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}