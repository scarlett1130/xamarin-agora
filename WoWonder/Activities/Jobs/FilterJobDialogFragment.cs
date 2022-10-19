using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Flexbox;
using Google.Android.Material.BottomSheet;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Jobs
{
    public class FilterJobDialogFragment : BottomSheetDialogFragment, SeekBar.IOnSeekBarChangeListener, IDialogInterfaceOnShowListener
    {
        #region Variables Basic

        private JobsActivity ContextJobs;
        private ImageButton CloseButton;
        private AppCompatButton BtnApply;
        private AppCompatButton BtnJobType1, BtnJobType2, BtnJobType3, BtnJobType4, BtnJobType5;
        private FlexboxLayout CategoryLayout;
        private TextView TxtDistanceCount, Clearbutton;
        private SeekBar DistanceBar;                                                     
        private int DistanceCount;
        private string JobType, Category, CategoryId;
        private AppCompatButton BtnPrev;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            ContextJobs = (JobsActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                //    View view = inflater.Inflate(Resource.Layout.ButtomSheetJobhFilter, container, false);
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.ButtomSheetJobhFilter, container, false); 
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);

                Dialog.SetOnShowListener(this);

                Clearbutton.Click += ClearbuttonOnClick;
                CloseButton.Click += IconBackOnClick;
                BtnApply.Click += BtnApplyOnClick;

                BtnJobType1.Click += BtnJobType1_Click;
                BtnJobType2.Click += BtnJobType2_Click;
                BtnJobType3.Click += BtnJobType3_Click;
                BtnJobType4.Click += BtnJobType4_Click;
                BtnJobType5.Click += BtnJobType5_Click; 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                TxtDistanceCount = view.FindViewById<TextView>(Resource.Id.Distancenumber);

                DistanceBar = view.FindViewById<SeekBar>(Resource.Id.distanceSeeker);
                DistanceBar.Max = 300;
                DistanceBar.SetOnSeekBarChangeListener(this);

                Clearbutton = view.FindViewById<TextView>(Resource.Id.Clearbutton);
                BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ShowResultbutton);

                //
                CloseButton = view.FindViewById<ImageButton>(Resource.Id.ib_back);

                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.N:
                        DistanceBar.SetProgress(string.IsNullOrEmpty(UserDetails.FilterJobLocation) ? 300 : Convert.ToInt32(UserDetails.FilterJobLocation), true);
                        break;
                    // For API < 24 
                    default:
                        DistanceBar.Progress = string.IsNullOrEmpty(UserDetails.FilterJobLocation) ? 300 : Convert.ToInt32(UserDetails.FilterJobLocation);
                        break;
                }

                // Job type
                BtnJobType1 = view.FindViewById<AppCompatButton>(Resource.Id.btn_job_fulltime);
                BtnJobType2 = view.FindViewById<AppCompatButton>(Resource.Id.btn_job_parttime);
                BtnJobType3 = view.FindViewById<AppCompatButton>(Resource.Id.btn_job_intern);
                BtnJobType4 = view.FindViewById<AppCompatButton>(Resource.Id.btn_job_contract);
                BtnJobType5 = view.FindViewById<AppCompatButton>(Resource.Id.btn_job_volunteer);

                // Categories
                CategoryLayout = view.FindViewById<FlexboxLayout>(Resource.Id.categoryLayout);
                CreateCategoryButtons();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void ClearbuttonOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.FilterJobType = "";
                UserDetails.FilterJobLocation = "";
                UserDetails.FilterJobCategories = "";

                JobType = "";
                DistanceCount = 0;
                CategoryId = "";

                if (BtnPrev != null)
                {
                    BtnPrev.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnPrev.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));
                }
                 
                BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_gray);
                BtnJobType1.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_gray);
                BtnJobType2.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_gray);
                BtnJobType3.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_gray);
                BtnJobType5.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));
                 
                BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_gray);
                BtnJobType4.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.FilterJobType = JobType; 
                UserDetails.FilterJobLocation = DistanceCount.ToString();
                UserDetails.FilterJobCategories = CategoryId;

                ContextJobs.MAdapter.JobList.Clear();
                ContextJobs.MAdapter.NotifyDataSetChanged();

                ContextJobs.SwipeRefreshLayout.Refreshing = true;
                ContextJobs.MainScrollEvent.IsLoading = false;

                ContextJobs.StartApiService();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
           
        #endregion
         
        #region MaterialDialog

        #endregion

        #region SeekBar

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            try
            {
                TxtDistanceCount.Text = progress + " " + GetText(Resource.String.Lbl_km);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {

        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                DistanceCount = seekBar.Progress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void SetJobType()
        {
            try
            {
                switch (JobType)
                {
                    case "full_time":
                        BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        BtnJobType1.SetTextColor(Color.ParseColor("#ffffff"));

                        BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType2.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType3.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType4.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType5.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        JobType = "full_time";
                        break;
                    case "part_time":
                        BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        BtnJobType2.SetTextColor(Color.ParseColor("#ffffff"));

                        BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType1.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType3.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType4.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType5.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        JobType = "part_time";
                        break;
                    case "internship":
                        BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        BtnJobType3.SetTextColor(Color.ParseColor("#ffffff"));

                        BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType1.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType2.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType4.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType5.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        JobType = "internship";
                        break;
                    case "volunteer":
                        BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        BtnJobType5.SetTextColor(Color.ParseColor("#ffffff"));

                        BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType1.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType2.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType3.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType4.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        JobType = "volunteer";
                        break;
                    case "contract":
                        BtnJobType4.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        BtnJobType4.SetTextColor(Color.ParseColor("#ffffff"));

                        BtnJobType1.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType1.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType2.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType2.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType3.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType3.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        BtnJobType5.SetBackgroundResource(Resource.Drawable.round_button_gray);
                        BtnJobType5.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));

                        JobType = "full_time";
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnJobType5_Click(object sender, EventArgs e)
        {
            JobType = "volunteer";
            SetJobType();
        }

        private void BtnJobType4_Click(object sender, EventArgs e)
        {
            JobType = "contract";
            SetJobType();
        }

        private void BtnJobType3_Click(object sender, EventArgs e)
        {
            JobType = "internship";
            SetJobType();
        }

        private void BtnJobType2_Click(object sender, EventArgs e)
        {
            JobType = "part_time";
            SetJobType();
        }

        private void BtnJobType1_Click(object sender, EventArgs e)
        {
            JobType = "full_time";
            SetJobType();
        }

        private void CreateCategoryButtons()
        {
            try
            {
                int count = CategoriesController.ListCategoriesJob.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(Activity, "Not have List Categories Job");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesJob)
                {
                    AppCompatButton button = new AppCompatButton(Activity);
                    var ll = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    ll.SetMargins(10, 10, 10, 10);
                    button.LayoutParameters = ll;

                    button.Text = category.CategoriesName;
                    button.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    button.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));
                    button.TextSize = 14;
                    button.SetAllCaps(false);
                    button.SetPadding(10, 0, 10, 0);
                    button.Click += ButtonOnClick;
                    CategoryLayout.AddView(button);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

      
        private void ButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                AppCompatButton BtnCurrent = sender as AppCompatButton;

                if (BtnPrev != null)
                {
                    BtnPrev.SetBackgroundResource(Resource.Drawable.round_button_gray);
                    BtnPrev.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#ffffff") : Color.ParseColor("#B0B6C3"));
                }

                BtnCurrent.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                BtnCurrent.SetTextColor(Color.ParseColor("#ffffff"));
                 
                Category = BtnCurrent.Text;
                CategoryId = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesName == Category)?.CategoriesId;

                BtnPrev = BtnCurrent;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void OnShow(IDialogInterface dialog)
        {
            try
            {
                var d = dialog as BottomSheetDialog;
                var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
                var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
                var layoutParams = bottomSheet.LayoutParameters;

                if (layoutParams != null)
                    layoutParams.Height = Resources.DisplayMetrics.HeightPixels;
                bottomSheet.LayoutParameters = layoutParams;
                bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
    }
}