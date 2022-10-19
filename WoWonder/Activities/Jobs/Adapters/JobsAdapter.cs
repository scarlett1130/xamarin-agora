using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.NearbyBusiness;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient;
using WoWonderClient.Classes.Jobs;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;

namespace WoWonder.Activities.Jobs.Adapters
{
    public class JobsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        public event EventHandler<JobsAdapterClickEventArgs> ItemClick;
        public event EventHandler<JobsAdapterClickEventArgs> ItemLongClick;

        public readonly Activity ActivityContext; 
        public ObservableCollection<Classes.JobClass> JobList = new ObservableCollection<Classes.JobClass>();
        public JobInfoObject DataInfoObject;
        public string DialogType;
        private RecyclerView.RecycledViewPool RecycledViewPool;
        private JobNearbyAdapter JobNearbyAdapter;

        public JobsAdapter(Activity context)
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
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)Classes.ItemType.NearbyJob:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                        var vh = new TemplateRecyclerViewHolder(itemView, Click, LongClick);
                        RecycledViewPool = new RecyclerView.RecycledViewPool();
                        vh.MRecycler.SetRecycledViewPool(RecycledViewPool);
                        return vh;
                    }
                    case (int)Classes.ItemType.JobRecent:
                    {
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_JobRecent_View, parent, false);
                        var vh = new JobRecentAdapterViewHolder(itemView, Click, LongClick);
                        return vh;
                    }
                    case (int)Classes.ItemType.Job:
                    {
                        //Setup your layout here >> Style_JobView
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_JobView, parent, false);
                        var vh = new JobsAdapterViewHolder(itemView, Click, LongClick, this);
                        return vh;
                    }
                    case (int)Classes.ItemType.Section:
                    {
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Section, parent, false);
                        var vh = new AdapterHolders.SectionViewHolder(itemView);
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
                var item = JobList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case Classes.ItemType.NearbyJob:
                            {
                                if (viewHolder is TemplateRecyclerViewHolder holder)
                                {
                                    if (JobNearbyAdapter == null)
                                    {
                                        JobNearbyAdapter = new JobNearbyAdapter(ActivityContext)
                                        {
                                            JobList = new ObservableCollection<JobInfoObject>()
                                        };

                                        LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                        holder.MRecycler.SetLayoutManager(layoutManager);
                                        holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                        var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                        var preLoader = new RecyclerViewPreloader<JobInfoObject>(ActivityContext, JobNearbyAdapter, sizeProvider, 10);
                                        holder.MRecycler.AddOnScrollListener(preLoader);
                                        holder.MRecycler.SetAdapter(JobNearbyAdapter);
                                        JobNearbyAdapter.ItemClick += JobNearbyAdapterOnItemClick;

                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_NearbyBusiness);
                                        holder.MoreText.Text = ActivityContext.GetText(Resource.String.Lbl_ViewAll);
                                        holder.MoreText.Visibility = ViewStates.Visible;
                                        holder.MoreText.Click += MoreTextOnClick;
                                    }

                                    var countList = item.JobList.Count;
                                    if (item.JobList.Count is > 0 && countList > 0)
                                    {
                                        foreach (var user in from user in item.JobList let check = JobNearbyAdapter.JobList.FirstOrDefault(a => a.UserId == user.UserId) where check == null select user)
                                        {
                                            JobNearbyAdapter.JobList.Add(user);
                                        }

                                        JobNearbyAdapter.NotifyItemRangeInserted(countList, JobNearbyAdapter.JobList.Count - countList);
                                    }
                                    else if (item.JobList.Count is > 0)
                                    {
                                        JobNearbyAdapter.JobList = new ObservableCollection<JobInfoObject>(item.JobList);
                                        JobNearbyAdapter.NotifyDataSetChanged();
                                    }
                                }

                                break;
                            }
                        case Classes.ItemType.Job:
                            {
                                if (viewHolder is JobsAdapterViewHolder holder)
                                {
                                    if (item.Job.Image.Contains("http"))
                                    {
                                        var image = item.Job.Image.Replace(InitializeWoWonder.WebsiteUrl + "/", "");
                                        item.Job.Image = image.Contains("http") switch
                                        {
                                            false => InitializeWoWonder.WebsiteUrl + "/" + image,
                                            _ => image
                                        };

                                        GlideImageLoader.LoadImage(ActivityContext, item.Job.Image, holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        File file2 = new File(item.Job.Image);
                                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.Image);
                                    }

                                    holder.Title.Text = Methods.FunString.DecodeString(item.Job.Title);

                                    var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Job.Currency);
                                    var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == item.Job.Category)?.CategoriesName;
                                    Console.WriteLine(currency);
                                    if (string.IsNullOrEmpty(categoryName))
                                        categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);

                                    holder.Salary.Text = item.Job.Minimum + " " + currencyIcon + " - " + item.Job.Maximum + " " + currencyIcon + " . " + categoryName;

                                    holder.Description.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Job.Description), 100);

                                    if (item.Job.IsOwner != null && item.Job.IsOwner.Value)
                                    {
                                        holder.IconMore.Visibility = ViewStates.Visible;
                                        holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_show_applies) + " (" + item.Job.ApplyCount + ")";
                                        holder.Button.Tag = "ShowApply";
                                    }
                                    else
                                    {
                                        holder.IconMore.Visibility = ViewStates.Gone;
                                    }

                                    if (item.Job.Apply == "true")
                                    {
                                        holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_already_applied);
                                        holder.Button.Enabled = false;
                                    }
                                    else
                                    {
                                        if (item.Job.Apply != "true" && item.Job.Page.IsPageOnwer != null && !item.Job.Page.IsPageOnwer.Value)
                                        {
                                            holder.Button.Text =
                                                ActivityContext.GetString(Resource.String.Lbl_apply_now);
                                            holder.Button.Tag = "Apply";
                                        }
                                    }
                                }
                                break;
                            }  
                        case Classes.ItemType.JobRecent:
                            {
                                if (viewHolder is JobRecentAdapterViewHolder holder)
                                {
                                    if (item.Job.Image.Contains("http"))
                                    {
                                        var image = item.Job.Image.Replace(InitializeWoWonder.WebsiteUrl + "/", "");
                                        item.Job.Image = image.Contains("http") switch
                                        {
                                            false => InitializeWoWonder.WebsiteUrl + "/" + image,
                                            _ => image
                                        };

                                        GlideImageLoader.LoadImage(ActivityContext, item.Job.Image, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        File file2 = new File(item.Job.Image);
                                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.Image);
                                    }

                                    holder.Title.Text = Methods.FunString.DecodeString(item.Job.Title);

                                    var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Job.Currency);
                                    Console.WriteLine(currency);
                                    
                                    string salary = "$" + item.Job.Minimum + " to " + "$" + item.Job.Maximum;
                                    string jobtype = item.Job.JobType switch
                                    {
                                        //Set job type
                                        "full_time" => ActivityContext.GetString(Resource.String.Lbl_full_time),
                                        "part_time" => ActivityContext.GetString(Resource.String.Lbl_part_time),
                                        "internship" => ActivityContext.GetString(Resource.String.Lbl_internship),
                                        "volunteer" => ActivityContext.GetString(Resource.String.Lbl_volunteer),
                                        "contract" => ActivityContext.GetString(Resource.String.Lbl_contract),
                                        _ => ActivityContext.GetString(Resource.String.Lbl_Unknown)

                                    };

                                    var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == item.Job.Category)?.CategoriesName;
                                    if (string.IsNullOrEmpty(categoryName))
                                        categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);

                                    holder.Content.Text = jobtype + " - " + salary + " - " + categoryName;
                                }

                                break;
                            }
                        case Classes.ItemType.Section:
                            {
                                switch (viewHolder)
                                {
                                    case AdapterHolders.SectionViewHolder holder:
                                        {
                                            holder.AboutHead.Text = item.Title;

                                            break;
                                        }
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private void MoreTextOnClick(object sender, EventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(NearbyBusinessActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void JobNearbyAdapterOnItemClick(object sender, JobsAdapterClickEventArgs e)
        {
            try
            {
                var item = JobNearbyAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(ActivityContext, typeof(JobsViewActivity));
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case JobsAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public Classes.JobClass GetItem(int position)
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
                var item = JobList[position];
                return item.Type switch
                {
                    Classes.ItemType.NearbyJob => (int)Classes.ItemType.NearbyJob,
                    Classes.ItemType.Job => (int)Classes.ItemType.Job,
                    Classes.ItemType.JobRecent => (int)Classes.ItemType.JobRecent,
                    Classes.ItemType.Section => (int)Classes.ItemType.Section,
                    _ => (int)Classes.ItemType.Job
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)Classes.ItemType.Job;
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
                        if (string.IsNullOrEmpty(item.Job?.Image)) return d;
                        if (item.Job.Image.Contains("http"))
                        {
                            var image = item.Job?.Image.Replace(InitializeWoWonder.WebsiteUrl + "/", "");
                            item.Job.Image = image.Contains("http") switch
                            {
                                false => InitializeWoWonder.WebsiteUrl + "/" + image,
                                _ => image
                            };

                            d.Add(item.Job?.Image);
                        }
                        else
                        {
                            d.Add(item.Job?.Image);
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

        public void OpenDialog()
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Edit));
                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Delete));

                dialogList.Title(ActivityContext.GetText(Resource.String.Lbl_More)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == ActivityContext.GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit Job
                    var intent = new Intent(ActivityContext, typeof(EditJobsActivity));
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(DataInfoObject));
                    ActivityContext.StartActivityForResult(intent, 246);
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialogBuilder = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                    dialogBuilder.Content(ActivityContext.GetText(Resource.String.Lbl_DeleteJobs));
                    dialogBuilder.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialogBuilder.NegativeText(ActivityContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.ItemsCallback(this).Build().Show();
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
                switch (DialogType)
                {
                    case "Delete" when p1 == DialogAction.Positive:
                    {
                        // Send Api delete 

                        if (Methods.CheckConnectivity())
                        {
                            var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                            var diff = adapterGlobal?.ListDiffer;
                            var dataGlobal = diff?.Where(a => a.PostData?.PostId == DataInfoObject?.PostId).ToList();
                            if (dataGlobal != null)
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                }
                            }

                            var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                            var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.PostId == DataInfoObject?.PostId).ToList();
                            if (dataGlobal2 != null)
                            {
                                foreach (var postData in dataGlobal2)
                                {
                                    recycler.RemoveByRowIndex(postData);
                                }
                            } 

                            var dataJob = JobList?.FirstOrDefault(a => a.Job?.Id == DataInfoObject.Id);
                            if (dataJob != null)
                            {
                                JobList.Remove(dataJob);
                                NotifyItemRemoved(JobsActivity.GetInstance().MAdapter.JobList.IndexOf(dataJob));
                            }

                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(DataInfoObject.PostId, "delete") });
                        }
                        else
                        {
                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        }

                        break;
                    }
                    case "Delete":
                    {
                        if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }

                        break;
                    }
                    default:
                    {
                        if (p1 == DialogAction.Positive)
                        {

                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }

    public class TemplateRecyclerViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public LinearLayout MainLinear { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView IconTitle { get; private set; }
        public TextView DescriptionText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public TemplateRecyclerViewHolder(View itemView, Action<JobsAdapterClickEventArgs> clickListener, Action<JobsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLinear = (LinearLayout)itemView.FindViewById(Resource.Id.mainLinear);
                TitleText = (TextView)itemView.FindViewById(Resource.Id.textTitle);
                IconTitle = (TextView)itemView.FindViewById(Resource.Id.iconTitle);
                DescriptionText = (TextView)itemView.FindViewById(Resource.Id.textSecondery);
                MoreText = (TextView)itemView.FindViewById(Resource.Id.textMore);
                MRecycler = (RecyclerView)itemView.FindViewById(Resource.Id.recyler);

                IconTitle.Visibility = ViewStates.Gone;
                DescriptionText.Visibility = ViewStates.Gone;

                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class JobRecentAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Content { get; private set; }
        #endregion

        public JobRecentAdapterViewHolder(View itemView, Action<JobsAdapterClickEventArgs> clickListener, Action<JobsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.JobCoverImage);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Content = MainView.FindViewById<TextView>(Resource.Id.tv_job_content);
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
     
    public class JobsAdapterViewHolder : RecyclerView.ViewHolder , View.IOnClickListener
    {
        #region Variables Basic

        private readonly JobsAdapter JobsAdapter;
        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Salary { get; private set; }
        public TextView IconMore { get; private set; }
        public AppCompatButton Button { get; private set; }
        public TextView Description { get; private set; }

        #endregion
         
        public JobsAdapterViewHolder(View itemView, Action<JobsAdapterClickEventArgs> clickListener, Action<JobsAdapterClickEventArgs> longClickListener , JobsAdapter jobsAdapter  ) : base(itemView)
        {
            try
            {
                JobsAdapter = jobsAdapter;
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.JobCoverImage);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Salary = MainView.FindViewById<TextView>(Resource.Id.salary);
                Description = MainView.FindViewById<TextView>(Resource.Id.description);
                IconMore = MainView.FindViewById<TextView>(Resource.Id.iconMore);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.applyButton);
                if (Button != null)
                {
                    Button.Tag = "Apply";
                    Button.SetOnClickListener(this);
                }
                 
                if (IconMore != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMore, IonIconsFonts.More);
                    IconMore.SetOnClickListener(this);
                }

                //Event  
                itemView.Click += (sender, e) => clickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new JobsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            if (BindingAdapterPosition != RecyclerView.NoPosition)
            {
                var item = JobsAdapter.JobList[BindingAdapterPosition];

                if (v?.Id == Button?.Id)
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(JobsAdapter.ActivityContext, JobsAdapter.ActivityContext?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    switch (Button?.Tag?.ToString())
                    {
                        // Open Apply Job Activity 
                        case "ShowApply":
                            {
                                switch (item.Job?.ApplyCount)
                                {
                                    case "0":
                                        ToastUtils.ShowToast(JobsAdapter.ActivityContext, JobsAdapter.ActivityContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short);
                                        return;
                                }

                                var intent = new Intent(JobsAdapter.ActivityContext, typeof(ShowApplyJobActivity));
                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item));
                                JobsAdapter.ActivityContext.StartActivity(intent);
                                break;
                            }
                        case "Apply":
                            {
                                var intent = new Intent(JobsAdapter.ActivityContext, typeof(ApplyJobActivity));
                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item));
                                JobsAdapter.ActivityContext.StartActivity(intent);
                                break;
                            }
                    }
                }
                else if (v?.Id == IconMore?.Id)
                {
                    try
                    {
                        JobsAdapter.DialogType = "More";
                        JobsAdapter.DataInfoObject = item.Job;
                        JobsAdapter.OpenDialog();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
        }
    }

    public class JobsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}