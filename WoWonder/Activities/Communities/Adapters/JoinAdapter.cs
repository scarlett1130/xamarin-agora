using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;

using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Group;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Communities.Adapters
{
    public class JoinAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<JoinAdapterClickEventArgs> ItemClick;
        public event EventHandler<JoinAdapterClickEventArgs> ItemLongClick;

        public event EventHandler<JoinAdapterClickEventArgs> DeleteButtonItemClick;
        public event EventHandler<JoinAdapterClickEventArgs> AddButtonItemClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<JoinRequestsObject> JoinList = new ObservableCollection<JoinRequestsObject>();

        public JoinAdapter(Activity activity)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => JoinList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                //var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContact_view, parent, false);
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_JoinRequestsView, parent, false);
                var vh = new JoinAdapterViewHolder(itemView, Click, LongClick, DeleteButtonClick, AddButtonClick);
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
                    case JoinAdapterViewHolder holder:
                    {
                        var item = JoinList[position];
                        if (item != null)
                        {
                            Initialize(holder, item); 
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

        private void Initialize(JoinAdapterViewHolder holder, JoinRequestsObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop,ImagePlaceholders.Drawable);

                holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item.UserData), 25);

                switch (item.UserData.Verified)
                {
                    case "1":
                        holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);
                        break;
                }

                holder.About.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.UserData.LastseenUnixTime), false);
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
                     case JoinAdapterViewHolder viewHolder:
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

        public JoinRequestsObject GetItem(int position)
        {
            return JoinList[position];
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
         
        private void Click(JoinAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(JoinAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        private void AddButtonClick(JoinAdapterClickEventArgs args)
        {
            AddButtonItemClick?.Invoke(this, args);
        }
        private void DeleteButtonClick(JoinAdapterClickEventArgs args)
        {
            DeleteButtonItemClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = JoinList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.UserData.Avatar != "")
                {
                    d.Add(item.UserData.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        } 
    }

    public class JoinAdapterViewHolder : RecyclerView.ViewHolder
    {
        public JoinAdapterViewHolder(View itemView, Action<JoinAdapterClickEventArgs> clickListener,
            Action<JoinAdapterClickEventArgs> longClickListener, Action<JoinAdapterClickEventArgs> deleteButtonClickListener, Action<JoinAdapterClickEventArgs> addButtonClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                AddButton = MainView.FindViewById<AppCompatButton>(Resource.Id.Add_button);
                DeleteButton = MainView.FindViewById<AppCompatButton>(Resource.Id.delete_button);
                //ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);

                //Event
                itemView.Click += (sender, e) => clickListener(new JoinAdapterClickEventArgs {View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new JoinAdapterClickEventArgs {View = itemView, Position = BindingAdapterPosition});
                AddButton.Click += (sender, e) => addButtonClickListener(new JoinAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                DeleteButton.Click += (sender, e) => deleteButtonClickListener(new JoinAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //Button.Visibility = ViewStates.Gone;
                //ImageLastSeen.Visibility = ViewStates.Gone;

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
        public TextView About { get; private set; }
        public AppCompatButton AddButton { get; private set; }
        public AppCompatButton DeleteButton { get; private set; }
        //public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class JoinAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}