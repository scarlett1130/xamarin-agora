using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.UserProfile.Adapters
{
    public class UserGroupsSecondAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<UserGroupsAdapterClickEventArgs> ItemClick;
        public event EventHandler<UserGroupsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GroupDataObject> GroupList = new ObservableCollection<GroupDataObject>();

        public UserGroupsSecondAdapter(Activity context)
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

        public override int ItemCount => GroupList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is not UserGroupsSecondAdapterViewHolder holder)
                    return;

                var item = GroupList[position];
                switch (item)
                {
                    case null:
                        return;
                }

                if (item.Avatar.Contains("http"))
                    GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.GroupImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                else
                    Glide.With(ActivityContext).Load(new File(item.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.GroupImage);

                holder.GroupName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Name), 20);

                holder.GroupCategory.Text = Methods.FunString.DecodeString(item.Category);

                holder.CivBackground.SetColorFilter(Color.ParseColor("#FCA65C"));
                holder.SmallIcon.SetImageResource(Resource.Drawable.ic_small_group);
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
                //Setup your layout here >> Style_Group_Simple
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MyPage_View, parent, false);
                var vh = new UserGroupsSecondAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case UserGroupsSecondAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.GroupImage);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public GroupDataObject GetItem(int position)
        {
            return GroupList[position];
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

        private void Click(UserGroupsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(UserGroupsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public System.Collections.IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GroupList[p0];
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
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class UserGroupsSecondAdapterViewHolder: RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public ImageView GroupImage { get; set; }
        public TextView GroupName { get; set; }
        public TextView GroupCategory { get; set; }
        public ImageView SmallIcon { get; set; }
        public CircleImageView CivBackground { get; set; }

        public UserGroupsSecondAdapterViewHolder(View itemView, Action<UserGroupsAdapterClickEventArgs> clickListener,
            Action<UserGroupsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                GroupImage = itemView.FindViewById<ImageView>(Resource.Id.PageImage);
                GroupName = itemView.FindViewById<TextView>(Resource.Id.Page_Name);
                GroupCategory = itemView.FindViewById<TextView>(Resource.Id.Page_Notifications);
                CivBackground = itemView.FindViewById<CircleImageView>(Resource.Id.civ_bg);
                SmallIcon = itemView.FindViewById<ImageView>(Resource.Id.iv_small_icon);

                //Event
                itemView.Click += (sender, e) => clickListener(new UserGroupsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserGroupsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}