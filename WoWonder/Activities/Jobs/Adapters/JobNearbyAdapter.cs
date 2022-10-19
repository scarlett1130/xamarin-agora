using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Jobs;

namespace WoWonder.Activities.Jobs.Adapters
{
    public class JobNearbyAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<JobsAdapterClickEventArgs> ItemClick;
        public event EventHandler<JobsAdapterClickEventArgs> ItemLongClick;

        public readonly Activity ActivityContext;
        public ObservableCollection<JobInfoObject> JobList = new ObservableCollection<JobInfoObject>();

        public JobNearbyAdapter(Activity context)
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

        public override int ItemCount => JobList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is JobNearbyAdapterViewHolder holder)
                {
                    var item = JobList[position];
                    if (item != null)
                    {
                        // Job Image
                        if (item.Image.Contains("http"))
                        {
                            var image = item.Image.Replace(InitializeWoWonder.WebsiteUrl + "/", "");
                            item.Image = image.Contains("http") switch
                            {
                                false => InitializeWoWonder.WebsiteUrl + "/" + image,
                                _ => image
                            };

                            GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.JobImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                        }
                        else
                        {
                            File file2 = new File(item.Image);
                            var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                            Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.JobImage);
                        }

                        // Job title
                        holder.JobTitle.Text = Methods.FunString.DecodeString(item.Title);

                        // Job salary
                        var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Currency);
                        holder.JobSalary.Text = "$" + item.Minimum + " " + ActivityContext.GetString(Resource.String.Lbl_To) + " " + "$" + item.Maximum;

                        // Job type
                        holder.JobType.Text = item.JobType switch
                        {
                            //Set job type
                            "full_time" => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_full_time),
                            "part_time" => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_part_time),
                            "internship" => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_internship),
                            "volunteer" => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_volunteer),
                            "contract" => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_contract),
                            _ => ActivityContext.GetString(Resource.String.Lbl_JobType) + " : " + ActivityContext.GetString(Resource.String.Lbl_Unknown)
                        };

                        // Job location
                        holder.JobLocation.Text = Methods.FunString.DecodeString(item.Location);

                        if (position == 0)
                        {
                            holder.JobCompany.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.JobTitle.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.JobType.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.JobSalary.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.JobLocation.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.JobLike.Drawable.SetTint(Color.ParseColor("#ffffff"));
                            holder.RootLayout.Background.SetTint(Color.ParseColor(AppSettings.MainColor));
                        }
                        else
                        {
                            holder.JobCompany.SetTextColor(Color.ParseColor("#25396F"));
                            holder.JobTitle.SetTextColor(Color.ParseColor("#25396F"));
                            holder.JobType.SetTextColor(Color.ParseColor("#25396F"));
                            holder.JobSalary.SetTextColor(Color.ParseColor("#25396F"));
                            holder.JobLocation.SetTextColor(Color.ParseColor("#25396F"));
                            holder.JobLike.Drawable.SetTint(Color.ParseColor("#25396F"));
                            holder.RootLayout.Background.SetTint(Color.ParseColor("#ffffff"));
                        }
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
                //Setup your layout here >> popular_job_item
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.popular_job_item, parent, false);
                var vh = new JobNearbyAdapterViewHolder(itemView, Click, LongClick, this);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case JobNearbyAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.JobImage);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public JobInfoObject GetItem(int position)
        {
            return JobList[position];
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

        private void Click(JobsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(JobsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

        System.Collections.IList ListPreloader.IPreloadModelProvider.GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = JobList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                        {
                            if (string.IsNullOrEmpty(item.Image)) return d;
                            if (item.Image.Contains("http"))
                            {
                                var image = item.Image.Replace(InitializeWoWonder.WebsiteUrl + "/", "");
                                item.Image = image.Contains("http") switch
                                {
                                    false => InitializeWoWonder.WebsiteUrl + "/" + image,
                                    _ => image
                                };

                                d.Add(item.Image);
                            }
                            else
                            {
                                d.Add(item.Image);
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
    }

    public class JobNearbyAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public ImageView JobImage { get; private set; }
        public ImageView JobLike { get; private set; }
        public TextView JobCompany { get; private set; }
        public TextView JobTitle { get; private set; }
        public TextView JobType { get; private set; }
        public TextView JobSalary { get; private set; }
        public TextView JobLocation { get; private set; }
        public RelativeLayout RootLayout { get; private set; }
        #endregion

        public JobNearbyAdapterViewHolder(View itemView, Action<JobsAdapterClickEventArgs> clickListener, Action<JobsAdapterClickEventArgs> longClickListener, JobNearbyAdapter jobsAdapter) : base(itemView)
        {
            try
            {
                MainView = itemView;

                RootLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.popular_job_layout);
                JobImage = MainView.FindViewById<ImageView>(Resource.Id.iv_job_image);
                JobLike = MainView.FindViewById<ImageView>(Resource.Id.iv_heart);
                JobCompany = MainView.FindViewById<TextView>(Resource.Id.tv_popular_job_company);
                JobTitle = MainView.FindViewById<TextView>(Resource.Id.tv_job_title);
                JobType = MainView.FindViewById<TextView>(Resource.Id.tv_job_type);
                JobSalary = MainView.FindViewById<TextView>(Resource.Id.tv_job_salary);
                JobLocation = MainView.FindViewById<TextView>(Resource.Id.tv_job_location);
                //Event  
                itemView.Click += (sender, e) => clickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}