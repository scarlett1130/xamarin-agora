using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Com.Tuyenmonkey.Textdecorator;
using Java.Util;
using Refractored.Controls;
using WoWonder.Activities.Articles.Fragment;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Articles;
using IList = System.Collections.IList;
using Reaction = WoWonderClient.Classes.Posts.Reaction;

namespace WoWonder.Activities.Articles.Adapters
{
    public class ArticlesCommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        public readonly Activity ActivityContext;
        public readonly string Type;

        public ObservableCollection<CommentsArticlesObject> CommentList = new ObservableCollection<CommentsArticlesObject>();
        private readonly ArticlesCommentClickListener PostEventListener;
        private readonly StReadMoreOption ReadMoreOption;

        public ArticlesCommentAdapter(Activity context ,string type)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                Type = type;
                PostEventListener = new ArticlesCommentClickListener(ActivityContext, type);

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(250, StReadMoreOption.TypeCharacter)
                    .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CommentList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case 0:
                        return new ArticlesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                    case 666:
                        return new AdapterHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new ArticlesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public void LoadCommentData(CommentsArticlesObject item, ArticlesCommentAdapterViewHolder holder)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var text = Methods.FunString.DecodeString(item.Text);
                    ReadMoreOption.AddReadMoreTo(holder.CommentText, new Java.Lang.String(text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }

                holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Posted), true);
                holder.UserName.Text = item.UserData.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = item.UserData.Name;
                var textIsPro = string.Empty;

                switch (item.UserData.Verified)
                {
                    case "1":
                        textHighLighter += " " + IonIconsFonts.CheckmarkCircle;
                        break;
                }

                switch (item.UserData.IsPro)
                {
                    case "1":
                        textIsPro = " " + IonIconsFonts.Flash;
                        textHighLighter += textIsPro;
                        break;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, item.UserData.Name.Length);

                switch (item.UserData.Verified)
                {
                    case "1":
                        decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);
                        break;
                }

                switch (item.UserData.IsPro)
                {
                    case "1":
                        decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);
                        break;
                }

                decorator.Build();

                holder.ReplyTextView.Text = item.Replies?.Count switch
                {
                    > 0 => ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies.Count + ")",
                    _ => holder.ReplyTextView.Text
                };

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                        {
                            item.Reaction ??= new Reaction();

                            switch (item.Reaction.Count)
                            {
                                case > 0:
                                    holder.CountLikeSection.Visibility = ViewStates.Visible;
                                    holder.CountLike.Text = Methods.FunString.FormatPriceValue(item.Reaction.Count);
                                    break;
                                default:
                                    holder.CountLikeSection.Visibility = ViewStates.Gone;
                                    break;
                            }

                            if (item.Reaction.IsReacted != null && item.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(item.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == item.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Like);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Love);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.HaHa);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Wow);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Sad);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Angry);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                                    //holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                                    holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));
                                                    holder.LikeTextView.Tag = "Like";

                                                    switch (item.Reaction.Count)
                                                    {
                                                        case > 0:
                                                            holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                            break;
                                                    }

                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                //holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));
                                holder.LikeTextView.Tag = "Like";
                                switch (item.Reaction.Count)
                                {
                                    case > 0:
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                }
                            }

                            break;
                        }
                    case PostButtonSystem.Wonder:
                    case PostButtonSystem.DisLike:
                    {
                        if (item.Reaction?.IsReacted != null && !item.Reaction.IsReacted.Value)
                            ArticlesReactionComment.SetReactionPack(holder, ReactConstants.Default);

                        switch (item.IsCommentLiked)
                        {
                            case true:
                                holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                                holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.LikeTextView.Tag = "Liked";
                                break;
                        }

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_wondered);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                            case PostButtonSystem.Wonder:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Wonder);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                            case PostButtonSystem.DisLike when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_disliked);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor("#f89823"));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                            case PostButtonSystem.DisLike:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Dislike);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                        }

                        break;
                    }
                    default:
                    {
                        switch (item.IsCommentLiked)
                        {
                            case true:
                                holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                                holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.LikeTextView.Tag = "Liked";
                                break;
                            default:
                                holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                //holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));
                                holder.LikeTextView.Tag = "Like";
                                break;
                        }

                        break;
                    }
                }

                holder.TimeTextView.Tag = "true";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder.ItemViewType)
                {
                    case 666:
                    {
                        if (viewHolder is not AdapterHolders.EmptyStateAdapterViewHolder emptyHolder)
                            return;

                        emptyHolder.EmptyImage.SetImageResource(Resource.Drawable.comment_emptstate);
                        emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);

                        return;
                    }
                }

                if (viewHolder is not ArticlesCommentAdapterViewHolder holder)
                    return;

                var item = CommentList[position];
                switch (item)
                {
                    case null:
                        return;
                    default:
                        LoadCommentData(item, holder);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public CommentsArticlesObject GetItem(int position)
        {
            return CommentList[position];
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
                var item = CommentList[position];

                if (item.Text != EmptyState)
                    return 0;

                if (item.Text == EmptyState)
                    return 666;

                return 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                    {
                        if (item.Text != EmptyState)
                        {
                            switch (string.IsNullOrEmpty(item.UserData.Avatar))
                            {
                                case false:
                                    d.Add(item.UserData.Avatar);
                                    break;
                            }

                            return d;
                        }

                        return Collections.SingletonList(p0);
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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class ArticlesCommentAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
    {

        #region Variables Basic

        public View MainView { get; private set; }
        public ArticlesCommentAdapter CommentAdapter; 
        private readonly ArticlesCommentClickListener PostClickListener;
        private readonly string TypeClass;

        public RelativeLayout MainCommentLayout { get; private set; }
        public LinearLayout BubbleLayout { get; private set; }
        public CircleImageView Image { get; private set; }
        public SuperTextView CommentText { get; private set; }
        public TextView TimeTextView { get; private set; }
        public TextView UserName { get; private set; }
        public TextView ReplyTextView { get; private set; }
        public TextView LikeTextView { get; private set; }
        public TextView DislikeTextView { get; private set; }
         
        public LinearLayout CountLikeSection { get; private set; }
        public TextView CountLike { get; private set; }
        public ImageView ImageCountLike { get; private set; }

        #endregion
         
        //Comment Article
        public ArticlesCommentAdapterViewHolder(View itemView, ArticlesCommentAdapter commentAdapter, ArticlesCommentClickListener postClickListener, string typeClass = "Comment") : base(itemView)
        {
            try
            {
                MainView = itemView;

                CommentAdapter = commentAdapter;
                PostClickListener = postClickListener;
                TypeClass = typeClass;

                MainCommentLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.mainComment);
                BubbleLayout = MainView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<SuperTextView>(Resource.Id.active);
                CommentText?.SetTextInfo(CommentText);

                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);
                DislikeTextView = MainView.FindViewById<TextView>(Resource.Id.dislike); 
                CountLikeSection = MainView.FindViewById<LinearLayout>(Resource.Id.countLikeSection);
                CountLike = MainView.FindViewById<TextView>(Resource.Id.countLike);
                ImageCountLike = MainView.FindViewById<ImageView>(Resource.Id.ImagecountLike);
                CountLikeSection.Visibility = ViewStates.Gone;
               
                var font = Typeface.CreateFromAsset(MainView.Context.Resources?.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                switch (AppSettings.FlowDirectionRightToLeft)
                {
                    case true:
                        BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);
                        break;
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.DisLike:
                    case PostButtonSystem.Wonder:
                        DislikeTextView.Visibility = ViewStates.Visible;
                        break; 
                }

                /*ReplyTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);*/
                ReplyTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));
                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));
                DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));

                MainView.SetOnLongClickListener(this);
                Image.SetOnClickListener(this);
                LikeTextView.SetOnClickListener(this);
                DislikeTextView.SetOnClickListener(this);
                ReplyTextView.SetOnClickListener(this);
                CountLikeSection.SetOnClickListener(this);  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (BindingAdapterPosition != RecyclerView.NoPosition)
                {
                    CommentsArticlesObject item = TypeClass switch
                    {
                        "Comment" => CommentAdapter.CommentList[BindingAdapterPosition],
                        "Reply" => CommentAdapter.CommentList[BindingAdapterPosition],
                        _ => null!
                    };

                    if (v.Id == Image.Id)
                        PostClickListener.ProfileClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
                    else if (v.Id == LikeTextView.Id)
                        PostClickListener.LikeCommentReplyPostClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
                    else if (v.Id == DislikeTextView.Id)
                        PostClickListener.DislikeCommentReplyPostClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
                    else if (v.Id == ReplyTextView.Id)
                        PostClickListener.CommentReplyClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
                    else if (v.Id == CountLikeSection?.Id)//wael check this page
                        PostClickListener.CountLikeCommentReplyPostClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnLongClick(View v)
        {
            //add event if System = ReactButton 
            if (BindingAdapterPosition != RecyclerView.NoPosition)
            {
                CommentsArticlesObject item = TypeClass switch
                {
                    "Comment" => CommentAdapter.CommentList[BindingAdapterPosition],
                    "Reply" => CommentAdapter.CommentList[BindingAdapterPosition],
                    _ => null!
                };

                if (v.Id == MainView.Id)
                    PostClickListener.MoreCommentReplyPostClick(new CommentReplyArticlesClickEventArgs { Holder = this, CommentObject = item, Position = BindingAdapterPosition, View = MainView });
            }

            return true;
        }
    }

}