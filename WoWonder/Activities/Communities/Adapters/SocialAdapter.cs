using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using Refractored.Controls;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Suggested.Adapters;
using WoWonder.Activities.Suggested.Groups;
using WoWonder.Activities.Suggested.Pages;
using WoWonder.Activities.UserProfile.Adapters;
using WoWonder.Activities.UsersPages;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Communities.Adapters
{
    public class SocialAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SocialAdapterClickEventArgs> ItemClick;
        public event EventHandler<SocialAdapterClickEventArgs> ItemLongClick;
        public event EventHandler<SocialAdapterClickEventArgs> ButtonItemClick;

        public readonly Activity ActivityContext;
        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }

        private LikedPagesAdapter LikedPagesAdapter;
        private SuggestedGroupAdapter SuggestedGroupAdapter;
        private SuggestedPageAdapter SuggestedPageAdapter;

        public ObservableCollection<SocialModelsClass> SocialList = new ObservableCollection<SocialModelsClass>();
         
        public SocialAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                RecycledViewPool = new RecyclerView.RecycledViewPool();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
       
        public override int ItemCount => SocialList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                { 
                    case (int)SocialModelType.MangedPages:
                    case (int)SocialModelType.JoinedGroups:
                    case (int)SocialModelType.MangedGroups:
                    {
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MyPage_View, parent, false);
                        var vh = new MangedSocialAdapterViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    } 
                    case (int)SocialModelType.LikedPages:
                    case (int)SocialModelType.SuggestedPages:
                    case (int)SocialModelType.SuggestedGroups:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                        var vh = new SocialAdapterViewHolder(itemView, OnClick, OnLongClick, this);
                        RecycledViewPool = new RecyclerView.RecycledViewPool();
                        vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                        return vh;
                    }
                    case (int)SocialModelType.Section:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Section, parent, false);
                        var vh = new SocialSectionViewHolder(itemView, OnClick, OnLongClick, this);
                        return vh;
                    }
                    case (int)SocialModelType.Divider:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Devider, parent, false);
                        var vh = new AdapterHolders.PostDividerSectionViewHolder(itemView);
                        return vh;
                    }
                    default:
                        return null!;
                }

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
                var item = SocialList[position];
                if (item != null)
                {
                    if (item.TypeView == SocialModelType.MangedPages)
                    {
                        if (viewHolder is MangedSocialAdapterViewHolder holder)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Page.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            string name = Methods.FunString.DecodeString(item.Page.PageTitle);
                            holder.Name.Text = name;

                            CategoriesController cat = new CategoriesController();
                            holder.Category.Text = cat.Get_Translate_Categories_Communities(item.Page.PageCategory, item.Page.Category, "Page"); 
                        }
                    }
                    else if (item.TypeView == SocialModelType.LikedPages)
                    {
                        if (viewHolder is SocialAdapterViewHolder holder)
                        {
                            if (LikedPagesAdapter == null)
                            {
                                LikedPagesAdapter = new LikedPagesAdapter(ActivityContext)
                                {
                                    PageList = new ObservableCollection<PageDataObject>()
                                };

                                LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                holder.MRecycler.NestedScrollingEnabled = false;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<PageDataObject>(ActivityContext, LikedPagesAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(LikedPagesAdapter);
                                LikedPagesAdapter.ItemClick += LikedPagesAdapterOnItemClick;

                                holder.TitleText.Text = item.TitleHead;

                                holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_SeeAll);
                                holder.MoreText.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.MoreText.Visibility = item.PageList.Count >= 1 ? ViewStates.Visible : ViewStates.Invisible;
                            }

                            var countList = LikedPagesAdapter.PageList.Count;
                            if (item.PageList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var page in from page in item.PageList let check = LikedPagesAdapter.PageList.FirstOrDefault(a => a.PageId == page.PageId) where check == null select page)
                                    {
                                        LikedPagesAdapter.PageList.Add(page);
                                    }

                                    LikedPagesAdapter.NotifyItemRangeInserted(countList, LikedPagesAdapter.PageList.Count - countList);
                                }
                                else
                                {
                                    LikedPagesAdapter.PageList = new ObservableCollection<PageDataObject>(item.PageList);
                                    LikedPagesAdapter.NotifyDataSetChanged();
                                }
                            }
                        }
                    }
                    else if (item.TypeView == SocialModelType.MangedGroups || item.TypeView == SocialModelType.JoinedGroups)
                    {
                        if (viewHolder is MangedSocialAdapterViewHolder holder)
                        {
                            if (item.Group.Avatar.Contains("http"))
                                GlideImageLoader.LoadImage(ActivityContext, item.Group.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            else
                                Glide.With(ActivityContext).Load(new File(item.Group.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder_circle).Error(Resource.Drawable.ImagePlacholder_circle)).Into(holder.Image);

                            holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Group.Name), 20);

                            holder.Category.Text = Methods.FunString.DecodeString(item.Group.Category);

                            holder.CivBackground.SetColorFilter(Color.ParseColor("#FCA65C"));
                            holder.SmallIcon.SetImageResource(Resource.Drawable.ic_small_group); 
                        }
                    }
                    else if (item.TypeView == SocialModelType.SuggestedGroups)
                    {
                        if (viewHolder is SocialAdapterViewHolder holder)
                        {
                            if (SuggestedGroupAdapter == null)
                            {
                                SuggestedGroupAdapter = new SuggestedGroupAdapter(ActivityContext)
                                {
                                    GroupList = new ObservableCollection<GroupDataObject>()
                                };

                                LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                holder.MRecycler.NestedScrollingEnabled = false;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<PageDataObject>(ActivityContext, SuggestedGroupAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(SuggestedGroupAdapter);
                                SuggestedGroupAdapter.ItemClick += SuggestedGroupAdapterOnItemClick;
                                SuggestedGroupAdapter.JoinButtonItemClick += SuggestedGroupAdapterOnJoinButtonItemClick;

                                holder.TitleText.Text = item.TitleHead;
                                holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_SeeAll);
                                holder.MoreText.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.MoreText.Visibility = item.SuggestedGroupList.Count >= 1 ? ViewStates.Visible : ViewStates.Invisible;
                            }

                            var countList = SuggestedGroupAdapter.GroupList.Count;
                            if (item.SuggestedGroupList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var page in from page in item.SuggestedGroupList let check = SuggestedGroupAdapter.GroupList.FirstOrDefault(a => a.GroupId == page.GroupId) where check == null select page)
                                    {
                                        SuggestedGroupAdapter.GroupList.Add(page);
                                    }

                                    SuggestedGroupAdapter.NotifyItemRangeInserted(countList, SuggestedGroupAdapter.GroupList.Count - countList);
                                }
                                else
                                {
                                    SuggestedGroupAdapter.GroupList = new ObservableCollection<GroupDataObject>(item.SuggestedGroupList);
                                    SuggestedGroupAdapter.NotifyDataSetChanged();
                                }
                            }
                        }
                    }
                    else if (item.TypeView == SocialModelType.SuggestedPages)
                    {
                        if (viewHolder is SocialAdapterViewHolder holder)
                        {
                            if (SuggestedPageAdapter == null)
                            {
                                SuggestedPageAdapter = new SuggestedPageAdapter(ActivityContext)
                                {
                                    PageList = new ObservableCollection<PageDataObject>()
                                };

                                LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                holder.MRecycler.SetLayoutManager(layoutManager);
                                holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                                holder.MRecycler.NestedScrollingEnabled = false;

                                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                var preLoader = new RecyclerViewPreloader<PageDataObject>(ActivityContext, SuggestedPageAdapter, sizeProvider, 10);
                                holder.MRecycler.AddOnScrollListener(preLoader);
                                holder.MRecycler.SetAdapter(SuggestedPageAdapter);
                                SuggestedPageAdapter.ItemClick += SuggestedPageAdapterOnItemClick;
                                SuggestedPageAdapter.LikeButtonItemClick += SuggestedPageAdapterOnLikeButtonItemClick;

                                holder.TitleText.Text = item.TitleHead;
                                holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_SeeAll);
                                holder.MoreText.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.MoreText.Visibility = item.SuggestedPageList.Count >= 1 ? ViewStates.Visible : ViewStates.Invisible;
                            }

                            var countList = SuggestedPageAdapter.PageList.Count;
                            if (item.SuggestedPageList.Count > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var page in from page in item.SuggestedPageList let check = SuggestedPageAdapter.PageList.FirstOrDefault(a => a.PageId == page.PageId) where check == null select page)
                                    {
                                        SuggestedPageAdapter.PageList.Add(page);
                                    }

                                    SuggestedPageAdapter.NotifyItemRangeInserted(countList, SuggestedPageAdapter.PageList.Count - countList);
                                }
                                else
                                {
                                    SuggestedPageAdapter.PageList = new ObservableCollection<PageDataObject>(item.SuggestedPageList);
                                    SuggestedPageAdapter.NotifyDataSetChanged();
                                }
                            }
                        }
                    }
                    else if (item.TypeView == SocialModelType.Section)
                    {
                        if (viewHolder is not SocialSectionViewHolder holder)
                            return;

                        holder.AboutHead.Text = item.TitleHead;
                    } 
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event

        private void LikedPagesAdapterOnItemClick(object sender, LikedPagesAdapterClickEventArgs e)
        {
            try
            {
                var item = LikedPagesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(ActivityContext, typeof(PageProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestedPageAdapterOnLikeButtonItemClick(object sender, SuggestedPageAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestedPageAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetLikePage(ActivityContext, item.PageId, e.LikeButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestedPageAdapterOnItemClick(object sender, SuggestedPageAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestedPageAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(ActivityContext, typeof(PageProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }



        private void SuggestedGroupAdapterOnJoinButtonItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestedGroupAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetJoinGroup(ActivityContext, item.GroupId, e.JoinButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestedGroupAdapterOnItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = SuggestedGroupAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(ActivityContext, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        #endregion

        public SocialModelsClass GetItem(int position)
        {
            return SocialList[position];
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
                var item = SocialList[position];
                if (item != null)
                {
                    return item.TypeView switch
                    {
                        SocialModelType.JoinedGroups => (int)SocialModelType.JoinedGroups,
                        SocialModelType.MangedGroups => (int)SocialModelType.MangedGroups,
                        SocialModelType.SuggestedGroups => (int)SocialModelType.SuggestedGroups,
                        SocialModelType.LikedPages => (int)SocialModelType.LikedPages,
                        SocialModelType.MangedPages => (int)SocialModelType.MangedPages,
                        SocialModelType.SuggestedPages => (int)SocialModelType.SuggestedPages,
                        SocialModelType.Section => (int)SocialModelType.Section,
                        SocialModelType.Divider => (int)SocialModelType.Divider,
                        _ => position
                    };
                }

                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }


        private void ButtonClick(SocialAdapterClickEventArgs args)
        {
            ButtonItemClick?.Invoke(this, args);
        }


        private void OnClick(SocialAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(SocialAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SocialList[p0];
                if (item == null)
                {
                    return d;
                }
                else
                {
                    //if (!string.IsNullOrEmpty(item.Avatar))
                    //    d.Add(item.Avatar);

                    return d;
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

    public class MangedSocialAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; }
        public ImageView Image { get; set; }
        public TextView Name { get; set; }
        public TextView Category { get; set; }
        public ImageView SmallIcon { get; set; }
        public CircleImageView CivBackground { get; set; }

        public MangedSocialAdapterViewHolder(View itemView, Action<SocialAdapterClickEventArgs> clickListener, Action<SocialAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = itemView.FindViewById<ImageView>(Resource.Id.PageImage);
                Name = itemView.FindViewById<TextView>(Resource.Id.Page_Name);
                Category = itemView.FindViewById<TextView>(Resource.Id.Page_Notifications);

                CivBackground = itemView.FindViewById<CircleImageView>(Resource.Id.civ_bg);
                SmallIcon = itemView.FindViewById<ImageView>(Resource.Id.iv_small_icon);


                //Event
                itemView.Click += (sender, e) => clickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
     
    public class SocialAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
    {
        #region Variables Basic

        private readonly SocialAdapter SocialAdapter;

        public View MainView { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public SocialAdapterViewHolder(View itemView, Action<SocialAdapterClickEventArgs> clickListener, Action<SocialAdapterClickEventArgs> longClickListener, SocialAdapter socialAdapter) : base(itemView)
        {
            try
            {
                SocialAdapter = socialAdapter;
                MainView = itemView;

                MainView = itemView;
                MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                TitleText = MainView.FindViewById<TextView>(Resource.Id.headText);
                MoreText = MainView.FindViewById<TextView>(Resource.Id.moreText);
                 
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10); 

                MoreText?.SetOnClickListener(this);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (BindingAdapterPosition != RecyclerView.NoPosition)
                {
                    SocialModelsClass item = SocialAdapter.SocialList[BindingAdapterPosition];

                    if (item.TypeView == SocialModelType.SuggestedGroups)
                    {
                        SocialAdapter.ActivityContext.StartActivity(new Intent(SocialAdapter.ActivityContext, typeof(SuggestedGroupActivity)));
                    }
                    else if (item.TypeView == SocialModelType.SuggestedPages)
                    {
                        SocialAdapter.ActivityContext.StartActivity(new Intent(SocialAdapter.ActivityContext, typeof(SuggestedPageActivity)));
                    } 
                    else if (item.TypeView == SocialModelType.LikedPages)
                    {
                        var intent = new Intent(SocialAdapter.ActivityContext, typeof(AllViewerActivity));
                        intent.PutExtra("Type", "LikedPagesModel");
                        intent.PutExtra("PassedId", UserDetails.UserId);
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                        SocialAdapter.ActivityContext.StartActivity(intent);
                    } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
  
    public class SocialSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
    {
        #region Variables Basic

        private readonly SocialAdapter SocialAdapter;

        public View MainView { get; private set; }
        public TextView AboutHead { get; private set; }
        public TextView AboutMore { get; private set; }

        #endregion

        public SocialSectionViewHolder(View itemView, Action<SocialAdapterClickEventArgs> clickListener, Action<SocialAdapterClickEventArgs> longClickListener, SocialAdapter socialAdapter) : base(itemView)
        {
            try
            {
                SocialAdapter = socialAdapter;
                MainView = itemView;

                MainView = itemView;
                AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);

                AboutMore.Text = SocialAdapter.ActivityContext.GetText(Resource.String.Lbl_SeeAll);
                AboutMore.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                AboutMore.Visibility = ViewStates.Visible;

                AboutMore?.SetOnClickListener(this);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SocialAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (BindingAdapterPosition != RecyclerView.NoPosition)
                {
                    List<SocialModelsClass> item = new List<SocialModelsClass>();
                    var intent = new Intent(SocialAdapter.ActivityContext, typeof(AllViewerActivity));

                    if (AboutHead.Text == SocialAdapter.ActivityContext.GetText(Resource.String.Lbl_Your_Pages))
                    {
                        intent.PutExtra("Type", "MangedPagesModel");
                        item = SocialAdapter.SocialList.Where(a => a.Page != null && a.TypeView == SocialModelType.MangedPages).ToList();
                    }
                    if (AboutHead.Text == SocialAdapter.ActivityContext.GetText(Resource.String.Lbl_Liked_Pages))  
                    {
                        intent.PutExtra("Type", "LikedPagesModel");
                        item = SocialAdapter.SocialList.Where(a => a.Page != null && a.TypeView == SocialModelType.LikedPages).ToList();
                    }
                    else if (AboutHead.Text == SocialAdapter.ActivityContext.GetText(Resource.String.Lbl_Manage_Groups))
                    {
                        intent.PutExtra("Type", "MangedGroupsModel");
                        item = SocialAdapter.SocialList.Where(a => a.Group != null && a.TypeView == SocialModelType.MangedGroups).ToList();
                    }
                    else if (AboutHead.Text == SocialAdapter.ActivityContext.GetText(Resource.String.Lbl_Joined_Groups))
                    {
                        intent.PutExtra("Type", "JoinedGroupsModel");
                        item = SocialAdapter.SocialList.Where(a => a.Group != null && a.TypeView == SocialModelType.JoinedGroups).ToList();
                    }

                    intent.PutExtra("PassedId", UserDetails.UserId);
                    intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                    SocialAdapter.ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
     
    public class SocialAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton Button { get; set; }
    }
}