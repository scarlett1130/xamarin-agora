using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Android.Content.PM;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Articles
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FilterCategoriesActivity : BaseActivity, FilterCategoryAdapter.IOnClickListener
    {
        private RecyclerView RvCategory;
        private FilterCategoryAdapter CategoryAdapter;
        private List<string> Categories;
        private RelativeLayout RlClose;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.filter_articles_activity);

                //
                RlClose = FindViewById<RelativeLayout>(Resource.Id.rl_close);
                RlClose.Click += RlClose_Click;

                Categories = JsonConvert.DeserializeObject<List<string>>(Intent.GetStringExtra("filter_category"));
                // 
                RvCategory = FindViewById<RecyclerView>(Resource.Id.rv_filter_category);
                CategoryAdapter = new FilterCategoryAdapter(Categories, this);
                RvCategory.SetLayoutManager(new LinearLayoutManager(this, RecyclerView.Vertical, false));
                RvCategory.SetAdapter(CategoryAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RlClose_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public void OnItemClick(string item)
        {
            Intent intent = new Intent();
            intent.PutExtra("category_item", item);
            SetResult(Result.Ok, intent);
            Finish();
        }

    }

    class FilterCategoryAdapter : RecyclerView.Adapter
    {
        private readonly List<string> Categories;
        private Context Context;
        private readonly IOnClickListener Listener;

        public interface IOnClickListener
        {
            void OnItemClick(string item);
        }

        public FilterCategoryAdapter(List<string> categories, IOnClickListener listener)
        {
            Categories = categories;
            Listener = listener;
        }
        public override int ItemCount
        {
            get { return Categories.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CategoryHolder vh = holder as CategoryHolder;
            vh.Bind(Categories[position], Listener);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Context = parent.Context;
            View view = LayoutInflater.From(Context)?.Inflate(Resource.Layout.filter_category_row, parent, false);
            CategoryHolder holder = new CategoryHolder(view);
            return holder;
        }

        class CategoryHolder : RecyclerView.ViewHolder
        {
            private readonly TextView TvCategory;
            private IOnClickListener Listener;

            public CategoryHolder(View itemView) : base(itemView)
            {
                TvCategory = itemView.FindViewById<TextView>(Resource.Id.tv_filter_category);
            }

            public void Bind(string category, IOnClickListener listener)
            {
                TvCategory.Text = category;

                Listener = listener;
                ItemView.Click += ItemView_Click;
            }

            private void ItemView_Click(object sender, EventArgs e)
            {
                Listener.OnItemClick(TvCategory.Text);
            }
        }
    }
}