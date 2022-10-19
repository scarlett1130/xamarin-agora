using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using Com.Airbnb.Lottie;
using WoWonder.Activities.Base;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ValidationUserActivity : BaseActivity
    {
        #region Variables Basic

        private AppCompatButton EmptyStateButton;
        private TextView EmptyStateIcon;
        private TextView DescriptionText;
        private TextView TitleText;
        private LottieAnimationView AnimationView;
        private ImageView EmptyImage;

        private string Code, Email;
      
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.EmptyStateLayout);

                Code = Intent?.GetStringExtra("Code") ?? "";
                Email = Intent?.GetStringExtra("Email") ?? "";

                //Get Value 
                InitComponent();  
                StartApiService(); 
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
                AddOrRemoveEvent(true); 
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
                AddOrRemoveEvent(false);
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
                EmptyStateIcon = (TextView)FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (AppCompatButton)FindViewById(Resource.Id.button);
                AnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
                EmptyImage = FindViewById<ImageView>(Resource.Id.iv_empty);

                EmptyStateIcon.Text = "";
                TitleText.Text = "";
                DescriptionText.Text = "";
                EmptyStateButton.Visibility = ViewStates.Invisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                    default:
                        EmptyStateButton.Click -= EmptyStateButtonOnClick;
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
                EmptyStateButton = null!;
                EmptyStateIcon = null!;
                DescriptionText = null!;
                TitleText = null!;
                AnimationView = null!;
                EmptyImage = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                {
                    if (AppSettings.ShowWalkTroutPage)
                    {
                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                        {
                            var intent = new Intent(this, typeof(GoProActivity));
                            intent.PutExtra("class", "register");
                            StartActivity(intent);
                        }
                        else
                        {
                            if (AppSettings.ShowSuggestedUsersOnRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                                newIntent?.PutExtra("class", "register");
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            }
                        }
                    }
                }
                else
                    Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Validation User 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
            {
                if (!string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(Email))
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Processing));

                    ValidationUserAsync();
                }
                else
                {
                    AnimationView.Visibility = ViewStates.Gone;
                    EmptyStateIcon.Visibility = ViewStates.Visible;
                    EmptyStateButton.Visibility = ViewStates.Visible; 

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                    TitleText.Text = GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                    DescriptionText.Text = GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                    EmptyStateButton.Text = GetText(Resource.String.Lbl_SomThingWentWrong_Button);
                } 
            }
        }

        private async void ValidationUserAsync()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Auth.ValidationUserAsync(Email, Code);
                    if (apiStatus == 200)
                    {
                        if (respond is AuthObject result)
                        {
                            SetDataLogin(result);

                            AndHUD.Shared.Dismiss(this);

                            AnimationView.Visibility = ViewStates.Visible;
                            EmptyStateIcon.Visibility = ViewStates.Gone;
                            EmptyStateButton.Visibility = ViewStates.Visible;

                            AnimationView.SetAnimation("validation.json");
                            TitleText.Text = GetText(Resource.String.Lbl_AccountActivated_Title);
                            DescriptionText.Text = GetText(Resource.String.Lbl_AccountActivated_Description);
                            EmptyStateButton.Text = GetText(Resource.String.Btn_Continue);
                        }
                    }
                    else
                    {
                        AnimationView.Visibility = ViewStates.Gone;
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        EmptyStateButton.Visibility = ViewStates.Visible;

                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                        TitleText.Text = GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                        DescriptionText.Text = GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                        EmptyStateButton.Text = GetText(Resource.String.Lbl_SomThingWentWrong_Button);

                        Methods.DisplayAndHudErrorResult(this, respond); 
                    }
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        #endregion

        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = Email;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Email,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


    }
}