using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using Android.Content.PM;
using WoWonder.Activities.Base;
using WoWonder.Helpers.CacheLoaders;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using static WoWonder.Activities.NativePost.Share.SharePageAdapter;

namespace WoWonder.Activities.NativePost.Share
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SharePageActivity : BaseActivity, IOnClickListener
    {
        public RecyclerView RvSharePage { get; private set; }
        private List<PageDataObject> Pages;
        private PostDataObject PostData;
        private SharePageAdapter SharePageAdapter;
        private TextView TvPageTitle;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.share_group_sheet);

            Pages = JsonConvert.DeserializeObject<List<PageDataObject>>(Intent.GetStringExtra("Pages"));
            PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("PostObject"));

            RvSharePage = FindViewById<RecyclerView>(Resource.Id.rv_share_group);
            SharePageAdapter = new SharePageAdapter(Pages, this);
            RvSharePage.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
            RvSharePage.SetAdapter(SharePageAdapter);

            //
            RelativeLayout rlClose = FindViewById<RelativeLayout>(Resource.Id.rl_close);
            rlClose.Click += RlClose_Click;

            //
            TvPageTitle = FindViewById<TextView>(Resource.Id.tv_shareTo);
            TvPageTitle.Text = GetText(Resource.String.Lbl_ShareToPage);
        }

        private void RlClose_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public void OnItemClick(PageDataObject item)
        {
            if (item != null)
            {
                Intent intent = new Intent(this, typeof(SharePostActivity));
                intent.PutExtra("ShareToType", "Page");
                intent.PutExtra("ShareToPage", JsonConvert.SerializeObject(item)); //PageClass
                intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostData)); //PostDataObject
                StartActivity(intent);
            }
        }
    }

    class SharePageAdapter : RecyclerView.Adapter
    {
        private readonly List<PageDataObject> PageClasses;
        private Context Context;
        private readonly IOnClickListener Listener;

        public interface IOnClickListener
        {
            void OnItemClick(PageDataObject item);
        }

        public SharePageAdapter(List<PageDataObject> pageClasses, IOnClickListener listener)
        {
            PageClasses = pageClasses;
            Listener = listener;
        }

        public override int ItemCount
        {
            get { return PageClasses.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            SharePageHolder vh = holder as SharePageHolder;

            PageDataObject item = PageClasses[position];
            GlideImageLoader.LoadImage((AppCompatActivity)Context, item.Avatar, vh.IvGroup, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            vh.TvGroupName.Text = item.PageName;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Context = parent.Context;
            View view = LayoutInflater.From(Context).Inflate(Resource.Layout.share_group_row, parent, false);

            return new SharePageHolder(view, Listener, PageClasses);
        }

        class SharePageHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public readonly CircleImageView IvGroup;
            public readonly TextView TvGroupName;
            private readonly IOnClickListener Listener;
            private readonly List<PageDataObject> Pages;

            public SharePageHolder(View itemView, IOnClickListener listener, List<PageDataObject> pages
                ) : base(itemView)
            {
                Pages = pages;
                Listener = listener;

                IvGroup = itemView.FindViewById<CircleImageView>(Resource.Id.civ_group);
                TvGroupName = itemView.FindViewById<TextView>(Resource.Id.tv_group_name);

                ItemView.SetOnClickListener(this);
            }


            public void OnClick(View v)
            {
                Listener.OnItemClick(Pages[LayoutPosition]);
            }
        }
    }
}