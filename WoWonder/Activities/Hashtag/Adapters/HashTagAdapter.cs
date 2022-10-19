using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.ObjectModel;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Activities.Hashtag.Adapters
{
    public class HashTagAdapter : RecyclerView.Adapter 
    {
        public event EventHandler<HashTagAdapterClickEventArgs> ItemClick;
        public event EventHandler<HashTagAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<TrendingHashtag> HashTagList = new ObservableCollection<TrendingHashtag>();

        public HashTagAdapter(Activity context)
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

        public override int ItemCount => HashTagList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_TrendingHashTagView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_TrendingHashTagView, parent, false);
                var vh = new HashTagAdapterViewHolder(itemView, Click, LongClick);
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
                    case HashTagAdapterViewHolder holder:
                        {
                            var item = HashTagList[position];
                            if (item != null)
                            {
                                holder.Text.Text = "#" + item.Tag;
                                holder.CountPosts.Text = item.TrendUseNum + " " + ActivityContext.GetText(Resource.String.Lbl_Post);
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

        public TrendingHashtag GetItem(int position)
        {
            return HashTagList[position];
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

        private void Click(HashTagAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(HashTagAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
    }

    public class HashTagAdapterViewHolder : RecyclerView.ViewHolder
    {
        public HashTagAdapterViewHolder(View itemView, Action<HashTagAdapterClickEventArgs> clickListener, Action<HashTagAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Text = MainView.FindViewById<TextView>(Resource.Id.text);
                CountPosts = MainView.FindViewById<TextView>(Resource.Id.countPosts);
                IconArrow = MainView.FindViewById<TextView>(Resource.Id.icon_arrow);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconArrow, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowBack : IonIconsFonts.IosArrowForward);
                 
                //Event
                itemView.Click += (sender, e) => clickListener(new HashTagAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new HashTagAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; private set; }
        public TextView Text { get; private set; }
        public TextView CountPosts { get; private set; }
        public TextView IconArrow { get; private set; }


        #endregion
    }

    public class HashTagAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}