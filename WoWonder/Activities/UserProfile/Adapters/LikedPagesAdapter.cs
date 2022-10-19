using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.UserProfile.Adapters
{
    public class LikedPagesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<LikedPagesAdapterClickEventArgs> LikeButtonItemClick;
        public event EventHandler<LikedPagesAdapterClickEventArgs> ItemClick;
        public event EventHandler<LikedPagesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PageDataObject> PageList = new ObservableCollection<PageDataObject>();

        public override int ItemCount => PageList?.Count ?? 0;

        public LikedPagesAdapter(Activity context)
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case LikedPagesAdapterViewHolder holder:
                        {
                            var item = PageList[position];
                            if (item != null)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.PageImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                if (!string.IsNullOrEmpty(item.PageTitle) || !string.IsNullOrWhiteSpace(item.PageTitle))
                                    holder.PageName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageTitle), 20);
                                else
                                    holder.PageName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageName), 20);

                                holder.PageNotification.Text = "@" + item.Username;

                                //Set style Btn Like page 
                                if (WoWonderTools.IsLikedPage(item))
                                {
                                    holder.BtnLike.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                                    holder.BtnLike.SetTextColor(Color.ParseColor("#ffffff"));
                                    holder.BtnLike.Text = ActivityContext.GetText(Resource.String.Btn_Unlike);
                                    holder.BtnLike.Tag = "true";
                                }
                                else
                                {
                                    holder.BtnLike.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                    holder.BtnLike.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    holder.BtnLike.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                    holder.BtnLike.Tag = "false";
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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_LikedPage, parent, false);
                var vh = new LikedPagesAdapterViewHolder(itemView, LikeButtonClick, Click, LongClick);
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
                    case LikedPagesAdapterViewHolder viewHolder:
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

        private void LikeButtonClick(LikedPagesAdapterClickEventArgs args)
        {
            LikeButtonItemClick?.Invoke(this, args);
        }

        private void Click(LikedPagesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(LikedPagesAdapterClickEventArgs args)
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

    public class LikedPagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public ImageView PageImage { get; private set; }
        public TextView PageName { get; private set; }
        public TextView PageNotification { get; private set; }
        public AppCompatButton BtnLike { get; private set; }

        public LikedPagesAdapterViewHolder(View itemView, Action<LikedPagesAdapterClickEventArgs> likeButtonClickListener, 
            Action<LikedPagesAdapterClickEventArgs> clickListener,
            Action<LikedPagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                PageImage = itemView.FindViewById<ImageView>(Resource.Id.PageImage);
                PageName = itemView.FindViewById<TextView>(Resource.Id.Page_Name);
                PageNotification = itemView.FindViewById<TextView>(Resource.Id.Page_Notifications);
                BtnLike = itemView.FindViewById<AppCompatButton>(Resource.Id.cont);

                //Event
                BtnLike.Click += (sender, e) => likeButtonClickListener(new LikedPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Button = BtnLike });
                itemView.Click += (sender, e) => clickListener(new LikedPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LikedPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LikedPagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton Button { get; set; }
    }
}