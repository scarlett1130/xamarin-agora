using Android.App;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.ObjectModel;
using AndroidX.AppCompat.Widget;
using WoWonder.Adapters;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Events.Adapters
{
    public class EventCategoryAdapter : RecyclerView.Adapter
    {
        public event EventHandler<CategoriesAdapterClickEventArgs> ItemClick;
        public event EventHandler<CategoriesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<string> MCategoriesList = new ObservableCollection<string>();
        private AppCompatButton FirstItem;

        public override int ItemCount => MCategoriesList?.Count ?? 0;

        public EventCategoryAdapter(Activity context)
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
                    case CategoriesAdapterViewHolder holder:
                        {
                            var item = MCategoriesList[position];
                            if (item != null)
                            {
                                holder.Button.Text = item;

                                if (position == 0)
                                {
                                    holder.Button.SetBackgroundResource(Resource.Drawable.Categories_button_press);
                                    holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                                    FirstItem = holder.Button;
                                }
                                else
                                    holder.Button.SetBackgroundResource(Resource.Drawable.Categories_button);
                                //holder.Button.SetTextColor(Color.ParseColor(item.CategoriesColor)); 
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Console.WriteLine(ActivityContext);
            }
        }

        public AppCompatButton GetFirstItem()
        {
            return FirstItem;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Categories_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Categories_View, parent, false);
                var vh = new CategoriesAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public string GetItem(int position)
        {
            return MCategoriesList[position];
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

        private void Click(CategoriesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(CategoriesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

}