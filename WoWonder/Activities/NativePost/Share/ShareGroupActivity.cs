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
using static WoWonder.Activities.NativePost.Share.ShareGroupAdapter;

namespace WoWonder.Activities.NativePost.Share
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ShareGroupActivity : BaseActivity, IOnClickListener
    {
        public RecyclerView RvShareGroup { get; private set; }
        private List<GroupDataObject> Groups;
        private PostDataObject PostData;
        private ShareGroupAdapter ShareGroupAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.share_group_sheet);

            Groups = JsonConvert.DeserializeObject<List<GroupDataObject>>(Intent.GetStringExtra("Groups"));
            PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("PostObject"));

            RvShareGroup = FindViewById<RecyclerView>(Resource.Id.rv_share_group);
            ShareGroupAdapter = new ShareGroupAdapter(Groups, this);
            RvShareGroup.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
            RvShareGroup.SetAdapter(ShareGroupAdapter);

            //
            RelativeLayout rlClose = FindViewById<RelativeLayout>(Resource.Id.rl_close);
            rlClose.Click += RlClose_Click;
        }

        private void RlClose_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public void OnItemClick(GroupDataObject item)
        {
            if (item != null)
            {
                Intent intent = new Intent(this, typeof(SharePostActivity));
                intent.PutExtra("ShareToType", "Group");
                intent.PutExtra("ShareToGroup", JsonConvert.SerializeObject(item)); //GroupClass
                intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostData)); //PostDataObject
                StartActivity(intent);
            }
        }
    }

    class ShareGroupAdapter : RecyclerView.Adapter
    {
        private readonly List<GroupDataObject> GroupClasses;
        private Context Context;
        private readonly IOnClickListener Listener;

        public interface IOnClickListener
        {
            void OnItemClick(GroupDataObject item);
        }

        public ShareGroupAdapter(List<GroupDataObject> groupClasses, IOnClickListener listener)
        {
            GroupClasses = groupClasses;
            Listener = listener;
        }

        public override int ItemCount
        {
            get { return GroupClasses.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ShareGroupHolder vh = holder as ShareGroupHolder;

            GroupDataObject item = GroupClasses[position];
            GlideImageLoader.LoadImage((AppCompatActivity)Context, item.Avatar, vh.IvGroup, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            vh.TvGroupName.Text = item.GroupName;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Context = parent.Context;
            View view = LayoutInflater.From(Context).Inflate(Resource.Layout.share_group_row, parent, false);

            return new ShareGroupHolder(view, Listener, GroupClasses);
        }

        class ShareGroupHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public readonly CircleImageView IvGroup;
            public readonly TextView TvGroupName;
            private readonly IOnClickListener Listener;
            private readonly List<GroupDataObject> Groups;

            public ShareGroupHolder(View itemView, IOnClickListener listener, List<GroupDataObject> groups
                ) : base(itemView)
            {
                Groups = groups;
                Listener = listener;

                IvGroup = itemView.FindViewById<CircleImageView>(Resource.Id.civ_group);
                TvGroupName = itemView.FindViewById<TextView>(Resource.Id.tv_group_name);

                ItemView.SetOnClickListener(this);
            }


            public void OnClick(View v)
            {
                Listener.OnItemClick(Groups[LayoutPosition]);
            }
        }
    }

}