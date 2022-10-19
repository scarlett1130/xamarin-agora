using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Tabs;
using MaterialDialogsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainFundraiserActicity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private Toolbar ToolBar;
        private AppCompatButton btnCreateFundraiser, btnHowWorks;
        private RelativeLayout rlMore;

        #region General
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.FundraiserMainLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveFunding(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveFunding(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

#endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                btnCreateFundraiser = FindViewById<AppCompatButton>(Resource.Id.btn_create_fraiser);
                btnHowWorks = FindViewById<AppCompatButton>(Resource.Id.btn_fund_how_works);
                rlMore = FindViewById<RelativeLayout>(Resource.Id.rlMore);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Fundraiser);
                    ToolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveFunding(bool addFunding)
        {
            try
            {
                switch (addFunding)
                {
                    // true +=  // false -=
                    case true:
                        btnCreateFundraiser.Click += BtnCreateFundraiserOnClick;
                        btnHowWorks.Click += BtnHowWorksOnClick;
                        rlMore.Click += RlMoreOnClick;
                        break;
                    default:
                        btnCreateFundraiser.Click -= BtnCreateFundraiserOnClick;
                        btnHowWorks.Click -= BtnHowWorksOnClick;
                        rlMore.Click -= RlMoreOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                btnCreateFundraiser = null!;
                btnHowWorks = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnHowWorksOnClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(FundingActivity));
            intent.PutExtra("fundraiser", 2);
            StartActivity(intent);

        }

        private void BtnCreateFundraiserOnClick(object sender, EventArgs e)
        {
            //Intent intent = new Intent(this, typeof(FundingActivity));
            //intent.PutExtra("fundraiser", 0);
            Intent intent = new Intent(this, typeof(CreateFundingActivity));
            StartActivityForResult(intent, 6350);
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string text)
        {
            try
            {
                //Intent intent = new Intent(this, typeof(CreateFundingActivity));
                //Intent intent = new Intent(this, typeof(FundrasingHowWorksActivity));
                //Intent intent = new Intent(this, typeof(FundraisingFaqActivity));
                //StartActivity(intent);
                //TxtAmount.Text = itemString;
                if (position == 0)
                {
                    Intent intent = new Intent(this, typeof(FundingActivity));
                    StartActivity(intent);
                }
                else if (position == 1)
                {
                    Intent intent = new Intent(this, typeof(FundraisingFaqActivity));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog dialog, DialogAction which)
        {
            try
            {
                if (which == DialogAction.Positive)
                {
                }
                else if (which == DialogAction.Negative)
                {
                    dialog.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RlMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                arrayAdapter.Add("Fundraisers");
                arrayAdapter.Add("FAQ");

                switch (arrayAdapter?.Count)
                {
                    case > 0:
                        {
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Fraisers category");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    case 6350 when resultCode == Result.Ok:
                        {
                            // Show fundraisers when done create new fundraiser
                            /*Intent returnIntent = new Intent();
                            SetResult(Result.Ok, returnIntent);
                            Finish();*/
                            Intent intent = new Intent(this, typeof(FundingActivity));
                            intent.PutExtra("fundraiser", 0);
                            StartActivity(intent);

                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion


    }
}