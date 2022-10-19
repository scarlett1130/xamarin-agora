using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Suggested.Adapters
{
    public class SuggestedPageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestedPageAdapterClickEventArgs> LikeButtonItemClick;
        public event EventHandler<SuggestedPageAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestedPageAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PageDataObject> PageList = new ObservableCollection<PageDataObject>();

        public SuggestedPageAdapter(Activity context)
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

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SuggestedGroupView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SuggestedGroupView, parent, false);
                var vh = new SuggestedPageAdapterViewHolder(itemView, LikeButtonClick, Click, LongClick);
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
                    case SuggestedPageAdapterViewHolder holder:
                        {
                            var item = PageList[position];
                            if (item != null)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.Cover, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                holder.Name.Text = Methods.FunString.DecodeString(item.Name);

                                if (!string.IsNullOrEmpty(item.LikesCount))
                                    holder.CountMembers.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.LikesCount)) + " " + ActivityContext.GetString(Resource.String.Btn_Likes);
                                else
                                    holder.CountMembers.Text = "@" + Methods.FunString.DecodeString(item.Username);
                                 
                                if (item.Avatar.Contains("http"))
                                    GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                else
                                    Glide.With(ActivityContext).Load(new File(item.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Avatar);

                                if (item.IsPageOnwer != null && item.IsPageOnwer.Value || item.UserId == UserDetails.UserId)
                                {
                                    holder.LikeButton.Visibility = ViewStates.Gone;
                                }
                                else
                                {
                                    //Set style Btn Joined Page 
                                    if (WoWonderTools.IsLikedPage(item)) //Liked
                                    {
                                        holder.LikeButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                        holder.LikeButton.SetTextColor(Color.White);
                                        holder.LikeButton.Text = ActivityContext.GetText(Resource.String.Btn_Unlike);
                                        holder.LikeButton.Tag = "true";
                                    }
                                    else //not Liked
                                    {
                                        holder.LikeButton.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                        holder.LikeButton.SetTextColor(Color.White);
                                        holder.LikeButton.Text = ActivityContext.GetString(Resource.String.Btn_Like);
                                        holder.LikeButton.Tag = "false";
                                    }
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

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case SuggestedPageAdapterViewHolder viewHolder:
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
        public override int ItemCount => PageList?.Count ?? 0;

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

        void LikeButtonClick(SuggestedPageAdapterClickEventArgs args) => LikeButtonItemClick?.Invoke(this, args);
        void Click(SuggestedPageAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestedPageAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

    }

    public class SuggestedPageAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }

        public TextView Name { get; private set; }
        public TextView CountMembers { get; private set; }
        public AppCompatButton LikeButton { get; private set; }
        public ImageView Avatar { get; private set; }

        #endregion

        public SuggestedPageAdapterViewHolder(View itemView, Action<SuggestedPageAdapterClickEventArgs> likeButtonClickListener, Action<SuggestedPageAdapterClickEventArgs> clickListener, Action<SuggestedPageAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.coverGroup);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                CountMembers = MainView.FindViewById<TextView>(Resource.Id.countMembers);
                LikeButton = MainView.FindViewById<AppCompatButton>(Resource.Id.JoinButton);
                Avatar = MainView.FindViewById<ImageView>(Resource.Id.avatar);

                //Event
                LikeButton.Click += (sender, e) => likeButtonClickListener(new SuggestedPageAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, LikeButton = LikeButton });
                itemView.Click += (sender, e) => clickListener(new SuggestedPageAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                System.Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class SuggestedPageAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton LikeButton { get; set; }
    }
}