using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MaterialDialogsCore;
using Android.App;
using Android.Content.Res;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Movies;
using Exception = System.Exception;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Movies.Adapters
{
    public class MoviesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        public event EventHandler<MoviesAdapterClickEventArgs> ItemClick;
        public event EventHandler<MoviesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<MoviesDataObject> MoviesList = new ObservableCollection<MoviesDataObject>();

        public MoviesAdapter(Activity context)
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

        public override int ItemCount => MoviesList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Video_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Video_View, parent, false);
                var vh = new MoviesAdapterViewHolder(itemView, Click, LongClick);
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
                    case MoviesAdapterViewHolder holder:
                    {
                        var item = MoviesList[position];
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

        private MoviesDataObject MovieDataMenue; 
        private void Initialize(MoviesAdapterViewHolder holder, MoviesDataObject movie)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, movie.Cover, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                //GlideImageLoader.LoadImage(ActivityContext, movie., holder.AvatarImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                string name = Methods.FunString.DecodeString(movie.Name);
                holder.TxtTitle.Text = name;
                holder.TxtSubname.Text = "@" + AppSettings.ApplicationName;

                var millis = Convert.ToInt32(movie.Duration);  
                int hours = millis / 60; //since both are ints, you get an int
                int minutes = millis % 60; 
                holder.TxtDuration.Text = hours + ":" + minutes;
                 
                holder.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(movie.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

                holder.TxtTime.Text = Methods.FunString.DecodeString(movie.Release);

                //FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.MenueView, IonIconsFonts.More);

                //Video Type
                //ShowGlobalBadgeSystem(holder.VideoType, movie);

                switch (holder.MoreImage.HasOnClickListeners)
                {
                    case false:
                        holder.MoreImage.Click +=  (sender, args) => 
                        {
                            try
                            {
                                MovieDataMenue = movie;

                                var arrayAdapter = new List<string>();
                                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CopeLink));
                                arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_Share));

                                dialogList.Title(ActivityContext.GetString(Resource.String.Lbl_More)).TitleColorRes(Resource.Color.primary);
                                dialogList.Items(arrayAdapter);
                                dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                                dialogList.AlwaysCallSingleChoiceCallback();
                                dialogList.ItemsCallback(this).Build().Show(); 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowGlobalBadgeSystem(TextView videoTypeIcon, MoviesDataObject item)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Iframe) && item.Iframe.Contains("Youtube") || item.Iframe.Contains("youtu"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoTypeIcon, IonIconsFonts.LogoYoutube);
                    videoTypeIcon.Visibility = ViewStates.Visible;
                    videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#FF0000"));
                }
                else 
                {
                    videoTypeIcon.Text = Methods.FunString.GetoLettersfromString(AppSettings.ApplicationName);
                    videoTypeIcon.Visibility = ViewStates.Visible;
                    videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Cope Link
        private void OnCopeLink_Button_Click(MoviesDataObject movie)
        {
            try
            { 
                Methods.CopyToClipboard(ActivityContext, movie.Url); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click(MoviesDataObject movie)
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    //Share Plugin same as video
                    case false:
                        return;
                    default:
                        await CrossShare.Current.Share(new ShareMessage
                        {
                            Title = movie.Name,
                            Text = movie.Description,
                            Url = movie.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                    case MoviesAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.VideoImage);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public MoviesDataObject GetItem(int position)
        {
            return MoviesList[position];
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

        private void Click(MoviesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MoviesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MoviesList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.Cover != "")
                {
                    d.Add(item.Cover);
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

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == ActivityContext.GetString(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLink_Button_Click(MovieDataMenue);
                }
                else if (text == ActivityContext.GetString(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click(MovieDataMenue);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }

    public class MoviesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoviesAdapterViewHolder(View itemView, Action<MoviesAdapterClickEventArgs> clickListener,Action<MoviesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                VideoImage = (ImageView) MainView.FindViewById(Resource.Id.Imagevideo);
                AvatarImage = MainView.FindViewById<ImageView>(Resource.Id.iv_avatar);
                TxtDuration = MainView.FindViewById<TextView>(Resource.Id.duration);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.Title);
                TxtViewsCount = MainView.FindViewById<TextView>(Resource.Id.Views_Count);
                TxtSubname = MainView.FindViewById<TextView>(Resource.Id.tv_subname);
                TxtTime = MainView.FindViewById<TextView>(Resource.Id.tv_time);
                MoreImage = MainView.FindViewById<ImageView>(Resource.Id.iv_more);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new MoviesAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new MoviesAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});

             
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView VideoImage { get; private set; }
        public ImageView AvatarImage { get; private set; }
        public TextView TxtDuration { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtTime { get; private set; }
        public TextView TxtViewsCount { get; private set; }
        public TextView TxtSubname { get; private set; }
        public ImageView MoreImage { get; private set; }
        #endregion
    }

    public class MoviesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}