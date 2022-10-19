using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using Android.Text;
using Android.Text.Style;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Request;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Refractored.Controls;

namespace WoWonder.Activities.Tabbes.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<NotificationObject> NotificationsList = new ObservableCollection<NotificationObject>();

        public NotificationsAdapter(Activity context)
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

        public override int ItemCount => NotificationsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Notifications_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Notifications_view, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, Click, LongClick);
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
                    case NotificationsAdapterViewHolder holder:
                        {
                            var item = NotificationsList[position];
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

        private void Initialize(NotificationsAdapterViewHolder holder, NotificationObject notify)
        {
            try
            {
                switch (notify.Type)
                {
                    case "memory":
                        Glide.With(ActivityContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                        holder.UserNameNotfy.Text = AppSettings.ApplicationName;
                        holder.TextNotfy.Text = Methods.Time.TimeAgo(Convert.ToInt32(notify.Time), false);
                        break;
                    case "Announcement":
                        Glide.With(ActivityContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                        holder.UserNameNotfy.Text = ActivityContext.GetText(Resource.String.Lbl_Announcement);
                        holder.UserNameNotfy.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                        holder.TextNotfy.Text = ActivityContext.GetText(Resource.String.Lbl_Announcement_SubText);
                        break;
                    case "admin_notification":
                        Glide.With(ActivityContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                        holder.UserNameNotfy.Text = AppSettings.ApplicationName + " " + notify.Text;
                        holder.TextNotfy.Text = Methods.Time.TimeAgo(Convert.ToInt32(notify.Time), false);
                        break;
                    default:
                        {
                            GlideImageLoader.LoadImage(ActivityContext, notify.Notifier?.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            var name = WoWonderTools.GetNameFinal(notify.Notifier);
                            string tempString;

                            if (AppSettings.FlowDirectionRightToLeft)
                            {
                                tempString = notify.Type == "share_post" || notify.Type == "shared_your_post"
                                    ? ActivityContext.GetText(Resource.String.Lbl_sharedYourPost) + " " + name 
                                    : notify.TypeText + " " + name;
                            }
                            else
                            {
                                tempString = notify.Type == "share_post" || notify.Type == "shared_your_post"
                                    ? name + " " + ActivityContext.GetText(Resource.String.Lbl_sharedYourPost)
                                    : name + " " + notify.TypeText;
                            }
                           
                            try
                            {
                                SpannableString spanString = new SpannableString(tempString);
                                spanString.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, name.Length, 0);

                                holder.UserNameNotfy.SetText(spanString, TextView.BufferType.Spannable);
                            }
                            catch
                            {
                                holder.UserNameNotfy.Text = tempString;
                            }

                            holder.TextNotfy.Text = Methods.Time.TimeAgo(Convert.ToInt32(notify.Time), false);
                            break;
                        }
                }
                 
                AddIconFonts(holder, notify.Type, notify.Icon);

                //var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound("", Color.ParseColor(GetColorFonts(notify.Type, notify.Icon)));
                //holder.Image.SetImageDrawable(drawable);
                holder.Image.SetColorFilter(Color.ParseColor("#FFAE35"));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddIconFonts(NotificationsAdapterViewHolder holder, string type, string icon)
        {
            try
            {
                switch (type)
                {
                    case "following":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_add);
                        return;
                    case "memory":
                        return;
                    case "Announcement":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.icon_announcement_vector);
                        return;
                    case "comment":
                    case "comment_reply":
                    case "also_replied":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_point_comments);
                        return;
                    case "liked_post":
                    case "liked_comment":
                    case "liked_reply_comment":
                    case "liked_page":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_point_like);
                        return;
                    case "wondered_post":
                    case "wondered_comment":
                    case "wondered_reply_comment":
                    case "exclamation-circle":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_information_circled);
                        return;
                    case "comment_mention":
                    case "comment_reply_mention":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_pricetag);
                        return;
                    case "post_mention":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_at);
                        return;
                    case "share_post":
                    case "shared_your_post":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_share);
                        return;
                    case "profile_wall_post":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_image);
                        return;
                    case "visited_profile":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_ios_eye);
                        return;
                    case "joined_group":
                    case "accepted_invite":
                    case "accepted_request":
                    case "accepted_join_request":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_checkmark_circled);
                        return;
                    case "invited_page":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_flag);
                        return;
                    case "added_you_to_group":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_add_circle);
                        return;
                    case "requested_to_join_group":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_time);
                        return;
                    case "thumbs-down":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_thumbsdown);
                        return;
                    case "going_event":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_calendar);
                        return;
                    case "viewed_story":
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_aperture);
                        return;
                    case "reaction":
                        {
                            holder.IconNotfy.SetImageResource(Resource.Drawable.ic_point_like);
                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == icon).Value?.Id ?? "";
                            switch (react)
                            {
                                case "like":
                                case "1":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_thumbsup);
                                    break;
                                case "haha":
                                case "3":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_happy);
                                    break;
                                case "love":
                                case "2":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_heart);
                                    break;
                                case "wow":
                                case "4":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_information_circled);
                                    break;
                                case "sad":
                                case "5":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_sad);
                                    break;
                                case "angry":
                                case "6":
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_social_freebsd_devil);
                                    break;
                                default:
                                    holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_notifications);
                                    break;
                            }

                            break;
                        }
                    default:
                        holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_notifications);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                holder.IconNotfy.SetImageResource(Resource.Drawable.ic_android_notifications);
            }
        }

        private string GetColorFonts(string type, string icon)
        {
            try
            {
                string iconColorFo;

                switch (type)
                {
                    case "following":
                        iconColorFo = "#F50057";
                        return iconColorFo;
                    case "memory":
                        iconColorFo = "#00695C";
                        return iconColorFo;
                    case "comment":
                    case "comment_reply":
                    case "also_replied":
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    case "liked_post":
                    case "liked_comment":
                    case "liked_reply_comment":
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    case "wondered_post":
                    case "wondered_comment":
                    case "wondered_reply_comment":
                    case "exclamation-circle":
                        iconColorFo = "#FFA500";
                        return iconColorFo;
                    case "comment_mention":
                    case "comment_reply_mention":
                        iconColorFo = "#B20000";

                        return iconColorFo;
                    case "post_mention":
                        iconColorFo = "#B20000";
                        return iconColorFo;
                    case "share_post":
                        iconColorFo = "#2F2FFF";
                        return iconColorFo;
                    case "profile_wall_post":
                        iconColorFo = "#006064";
                        return iconColorFo;
                    case "visited_profile":
                        iconColorFo = "#328432";
                        return iconColorFo;
                    case "liked_page":
                        iconColorFo = "#2F2FFF";
                        return iconColorFo;
                    case "joined_group":
                    case "accepted_invite":
                    case "accepted_request":
                        iconColorFo = "#2F2FFF";
                        return iconColorFo;
                    case "invited_page":
                        iconColorFo = "#B20000";
                        return iconColorFo;
                    case "accepted_join_request":
                        iconColorFo = "#2F2FFF";
                        return iconColorFo;
                    case "added_you_to_group":
                        iconColorFo = "#311B92";
                        return iconColorFo;
                    case "requested_to_join_group":
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    case "thumbs-down":
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    case "going_event":
                        iconColorFo = "#33691E";
                        return iconColorFo;
                    case "viewed_story":
                        iconColorFo = "#D81B60";
                        return iconColorFo;
                    case "reaction" when icon == "like":
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    case "reaction" when icon == "haha":
                        iconColorFo = "#0277BD";
                        return iconColorFo;
                    case "reaction" when icon == "love":
                        iconColorFo = "#d50000";
                        return iconColorFo;
                    case "reaction" when icon == "wow":
                        iconColorFo = "#FBC02D";
                        return iconColorFo;
                    case "reaction" when icon == "sad":
                        iconColorFo = "#455A64";
                        return iconColorFo;
                    case "reaction" when icon == "angry":
                        iconColorFo = "#BF360C";
                        return iconColorFo;
                    case "reaction":
                        iconColorFo = "#424242";
                        return iconColorFo;
                    default:
                        iconColorFo = "#424242";
                        return iconColorFo;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "#424242";
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
                    case NotificationsAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.ImageUser);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public NotificationObject GetItem(int position)
        {
            return NotificationsList[position];
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

        private void Click(NotificationsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(NotificationsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                        {
                            if (item.Type == "Announcement")
                                return d;

                            if (!string.IsNullOrEmpty(item.Notifier?.Avatar)) 
                                d.Add(item.Notifier.Avatar);

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

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener, Action<NotificationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                 
                //Get values
                ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.image_view);
                IconNotfy = MainView.FindViewById<ImageView>(Resource.Id.smallIcon);
                UserNameNotfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
                TextNotfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }
          
        public ImageView ImageUser { get; private set; }
        public CircleImageView Image { get; private set; }
        public ImageView IconNotfy { get; private set; }
        public TextView UserNameNotfy { get; private set; }
        public TextView TextNotfy { get; private set; }

        #endregion
    }
     
    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}