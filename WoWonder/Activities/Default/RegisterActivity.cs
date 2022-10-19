using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.EightbitLab.BlurViewBinding;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : AppCompatActivity, MaterialDialog.IListCallback, View.IOnClickListener
    {
        #region Variables Basic

        private RelativeLayout UsernameLayout, FirstNameLayout, LastNameLayout, EmailLayout, BirthdayLayout, GenderLayout, PhoneNumLayout, PasswordLayout, ConfirmPasswordLayout;
        private EditText TxtUsername, TxtFirstName, TxtLastName , TxtEmail, TxtBirthday, TxtGender, TxtPhoneNum, TxtPassword, TxtConfirmPassword;
        private TextView TxtUsernameRequired, TxtFirstNameRequired, TxtLastNameRequired, TxtEmailRequired, TxtBirthdayRequired, TxtGenderRequired, TxtPhoneNumRequired, TxtPasswordRequired, TxtConfirmPasswordRequired;
        private LinearLayout PhoneLayout;
        private CheckBox ChkTermsOfUse;
        private TextView TxtTermsOfService;
        private AppCompatButton BtnSignUp;
        private ProgressBar ProgressBar;
        private ImageView ImageShowPass, ImageShowConPass;
        private string GenderStatus = "male", Referral;
        private BlurView BlurView;
        private string Birthday = "";
        private EditText etYear, etMonth, etDay;
        private AppCompatButton btnOk, btnCancel;
        private MaterialDialog mDialog;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Register_Layout);

                Referral = Intent?.GetStringExtra("Referral") ?? "";

                //Get Value  
                InitComponent();
                InitToolbar();
                LoadConfigSettings();

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                UsernameLayout = FindViewById<RelativeLayout>(Resource.Id.rl_username); 
                TxtUsername = FindViewById<EditText>(Resource.Id.etUsername);
                TxtUsernameRequired = FindViewById<TextView>(Resource.Id.tv_username_required);

                FirstNameLayout = FindViewById<RelativeLayout>(Resource.Id.rl_firstname);
                TxtFirstName = FindViewById<EditText>(Resource.Id.etFirstName);
                TxtFirstNameRequired = FindViewById<TextView>(Resource.Id.tv_firstname_required);

                LastNameLayout = FindViewById<RelativeLayout>(Resource.Id.rl_lastname);
                TxtLastName = FindViewById<EditText>(Resource.Id.etLastName);
                TxtLastNameRequired = FindViewById<TextView>(Resource.Id.tv_lastname_required);
                 
                EmailLayout = FindViewById<RelativeLayout>(Resource.Id.rl_email);
                TxtEmail = FindViewById<EditText>(Resource.Id.etEmail);
                TxtEmailRequired = FindViewById<TextView>(Resource.Id.tv_email_required);

                PasswordLayout = FindViewById<RelativeLayout>(Resource.Id.rl_password);
                TxtPassword = FindViewById<EditText>(Resource.Id.etPassword);
                TxtPasswordRequired = FindViewById<TextView>(Resource.Id.tv_password_required);

                ConfirmPasswordLayout = FindViewById<RelativeLayout>(Resource.Id.rl_confirm_password);
                TxtConfirmPassword = FindViewById<EditText>(Resource.Id.etConfirmPassword);
                TxtConfirmPasswordRequired = FindViewById<TextView>(Resource.Id.tv_confirm_password_required);

                BirthdayLayout = FindViewById<RelativeLayout>(Resource.Id.rl_birthday);
                TxtBirthday = FindViewById<EditText>(Resource.Id.etBirthday);
                TxtBirthdayRequired = FindViewById<TextView>(Resource.Id.tv_birthday_required);
                
                GenderLayout = FindViewById<RelativeLayout>(Resource.Id.rl_gender);
                TxtGender = FindViewById<EditText>(Resource.Id.etGender);
                TxtGenderRequired = FindViewById<TextView>(Resource.Id.tv_gender_required);

                PhoneLayout = FindViewById<LinearLayout>(Resource.Id.phoneNumLayout);
                PhoneNumLayout = FindViewById<RelativeLayout>(Resource.Id.rl_phoneNum);
                TxtPhoneNum = FindViewById<EditText>(Resource.Id.etPhoneNum);
                TxtPhoneNumRequired = FindViewById<TextView>(Resource.Id.tv_phoneNum_required);
                  
                ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
                ImageShowConPass = FindViewById<ImageView>(Resource.Id.imageShowConPass);

                ChkTermsOfUse = FindViewById<CheckBox>(Resource.Id.terms_of_use);
                TxtTermsOfService = FindViewById<TextView>(Resource.Id.terms_of_service);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar); 
                BtnSignUp = FindViewById<AppCompatButton>(Resource.Id.btn_create_account);

                BlurView = FindViewById<BlurView>(Resource.Id.bv_create_account); 
                //BlurBackground(BlurView, 10f);

                ToggleVisibility(false);
                Methods.SetFocusable(TxtBirthday); 
                Methods.SetFocusable(TxtGender); 
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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = " ";
                    toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolBar);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnSignUp.Click += BtnSignUpOnClick;
                    ImageShowPass.Touch += ImageShowPassOnTouch;
                    ImageShowConPass.Touch += ImageShowConPassOnTouch;
                    TxtBirthday.Touch += TxtBirthdayOnTouch;
                    TxtGender.Touch += TxtGenderOnTouch;
                    TxtTermsOfService.Click += TxtTermsOfServiceOnClick;
                    TxtUsername.TextChanged += TxtUsernameOnTextChanged;
                    TxtFirstName.TextChanged += TxtFirstNameOnTextChanged;
                    TxtLastName.TextChanged += TxtLastNameOnTextChanged;
                    TxtEmail.TextChanged += TxtEmailOnTextChanged;
                    TxtBirthday.TextChanged += TxtBirthdayOnTextChanged;
                    TxtGender.TextChanged += TxtGenderOnTextChanged;
                    TxtPhoneNum.TextChanged += TxtPhoneNumOnTextChanged; 
                    TxtPassword.TextChanged += TxtPasswordOnTextChanged;  
                }
                else
                {
                    BtnSignUp.Click -= BtnSignUpOnClick;
                    ImageShowPass.Touch -= ImageShowPassOnTouch;
                    ImageShowConPass.Touch -= ImageShowConPassOnTouch;
                    TxtBirthday.Touch -= TxtBirthdayOnTouch;
                    TxtGender.Touch -= TxtGenderOnTouch;
                    TxtTermsOfService.Click -= TxtTermsOfServiceOnClick;
                    TxtUsername.TextChanged -= TxtUsernameOnTextChanged;
                    TxtFirstName.TextChanged -= TxtFirstNameOnTextChanged;
                    TxtLastName.TextChanged -= TxtLastNameOnTextChanged;
                    TxtEmail.TextChanged -= TxtEmailOnTextChanged;
                    TxtBirthday.TextChanged -= TxtBirthdayOnTextChanged;
                    TxtGender.TextChanged -= TxtGenderOnTextChanged;
                    TxtPhoneNum.TextChanged -= TxtPhoneNumOnTextChanged;
                    TxtPassword.TextChanged -= TxtPasswordOnTextChanged;
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
                TxtUsername = null!;
                TxtEmail = null!;
                TxtGender = null!;
                TxtPassword = null!;
                TxtConfirmPassword = null!;
                ChkTermsOfUse = null!;
                TxtTermsOfService = null!;
                BtnSignUp = null!;
                GenderStatus = "male";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        //Show Con Password 
        private void ImageShowConPassOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                switch (e.Event?.Action)
                {
                    case MotionEventActions.Up: // hide password
                        TxtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
                        ImageShowPass.SetImageResource(Resource.Drawable.ic_eye_hide);
                        break;
                    case MotionEventActions.Down: // show password
                        TxtPassword.TransformationMethod = HideReturnsTransformationMethod.Instance;
                        ImageShowPass.SetImageResource(Resource.Drawable.icon_eye);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show Password 
        private void ImageShowPassOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                switch (e.Event?.Action)
                {
                    case MotionEventActions.Up: // hide password
                        TxtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
                        ImageShowConPass.SetImageResource(Resource.Drawable.ic_eye_hide);
                        break;
                    case MotionEventActions.Down: // show password
                        TxtPassword.TransformationMethod = HideReturnsTransformationMethod.Instance;
                        ImageShowConPass.SetImageResource(Resource.Drawable.icon_eye);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //start Create account 
        private async void BtnSignUpOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                { 
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }
                  
                if (!ChkTermsOfUse.Checked)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (string.IsNullOrEmpty(TxtUsername.Text.Replace(" ", "")))
                {
                    SetHighLight(true, UsernameLayout, TxtUsername, TxtUsernameRequired);
                    return;
                }
                else
                    SetHighLight(false, UsernameLayout, TxtUsername, TxtUsernameRequired);
                 
                if (string.IsNullOrEmpty(TxtFirstName.Text.Replace(" ", "")))
                {
                    SetHighLight(true, FirstNameLayout, TxtFirstName, TxtFirstNameRequired);
                    return;
                }
                else
                    SetHighLight(false, FirstNameLayout, TxtFirstName, TxtFirstNameRequired);
                 
                if (string.IsNullOrEmpty(TxtLastName.Text.Replace(" ", "")))
                {
                    SetHighLight(true, LastNameLayout, TxtLastName, TxtLastNameRequired);
                    return;
                }
                else
                    SetHighLight(false, LastNameLayout, TxtLastName, TxtLastNameRequired);
                 
                if (string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")))
                {
                    SetHighLight(true, EmailLayout, TxtEmail, TxtEmailRequired);
                    return;
                }
                else
                    SetHighLight(false, EmailLayout, TxtEmail, TxtEmailRequired);
                 
                /*if (string.IsNullOrEmpty(TxtBirthday.Text))
                {
                    SetHighLight(true, BirthdayLayout, TxtBirthday, TxtBirthdayRequired);
                    return;
                }
                else
                    SetHighLight(false, BirthdayLayout, TxtBirthday, TxtBirthdayRequired);*/
                
                if (string.IsNullOrEmpty(GenderStatus))
                {
                    SetHighLight(true, GenderLayout, TxtGender, TxtGenderRequired);
                    return;
                }
                else
                    SetHighLight(false, GenderLayout, TxtGender, TxtGenderRequired);
                 
                if (string.IsNullOrEmpty(TxtPassword.Text))
                {
                    SetHighLight(true, PasswordLayout, TxtPassword, TxtPasswordRequired);
                    return;
                }
                else
                    SetHighLight(false, PasswordLayout, TxtPassword, TxtPasswordRequired);
                 
                if (string.IsNullOrEmpty(TxtConfirmPassword.Text))
                {
                    SetHighLight(true, ConfirmPasswordLayout, TxtConfirmPassword, TxtConfirmPasswordRequired);
                    return;
                }
                else
                    SetHighLight(false, ConfirmPasswordLayout, TxtConfirmPassword, TxtConfirmPasswordRequired);
                 
                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                if (smsOrEmail == "sms" && string.IsNullOrEmpty(TxtPhoneNum.Text))
                {
                    SetHighLight(true , PhoneNumLayout , TxtPhoneNum , TxtPhoneNumRequired);
                    return;
                }
                else
                    SetHighLight(false, PhoneNumLayout, TxtPhoneNum, TxtPhoneNumRequired);

                var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                if (!check)
                { 
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (TxtPassword.Text != TxtConfirmPassword.Text)
                { 
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Your_password_dont_match), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);

                var (apiStatus, respond) = await RequestsAsync.Auth.CreateAccountAsync(TxtUsername.Text.Replace(" ", ""), TxtPassword.Text, TxtConfirmPassword.Text, TxtEmail.Text.Replace(" ", ""), GenderStatus, TxtPhoneNum.Text, Referral, UserDetails.DeviceId);
                Console.WriteLine(apiStatus);
                if (apiStatus == 200 && respond is CreatAccountObject result)
                {
                    SetDataLogin(result);

                    var dataPrivacy = new Dictionary<string, string> {{"first_name", TxtFirstName.Text}, {"last_name", TxtLastName.Text}, { "birthday", Birthday }};
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy)});

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
                            if (AppSettings.AddAllInfoPorfileAfterRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            }
                        }
                    }
                     
                    ToggleVisibility(false);
                    FinishAffinity();
                }
                else if (apiStatus == 220)
                {
                    if (respond is AuthMessageObject message)
                    {
                        if (smsOrEmail == "sms")
                        {
                            UserDetails.Username = TxtUsername.Text;
                            UserDetails.FullName = TxtFirstName.Text + " " + TxtLastName.Text;
                            UserDetails.Password = TxtPassword.Text;
                            UserDetails.UserId = message.UserId;
                            UserDetails.Status = "Pending";
                            UserDetails.Email = TxtEmail.Text;
                             
                            //Insert user data to database
                            var user = new DataTables.LoginTb
                            {
                                UserId = UserDetails.UserId,
                                AccessToken = UserDetails.AccessToken,
                                Cookie = UserDetails.Cookie,
                                Username = UserDetails.Username,
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
                              
                            Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                            newIntent?.PutExtra("TypeCode", "AccountSms");
                            StartActivity(newIntent);
                        }
                        else if (smsOrEmail == "mail")
                        {
                            UserDetails.Email = TxtEmail.Text;

                            var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                            dialog.Title(GetText(Resource.String.Lbl_ActivationSent)).TitleColorRes(Resource.Color.primary);
                            dialog.Content(GetText(Resource.String.Lbl_ActivationDetails));
                            //dialog.Content(GetText(Resource.String.Lbl_ActivationDetails).Replace("@", TxtEmail.Text));
                            dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(new WoWonderTools.MyMaterialDialog());
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Invisible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), message.Message, GetText(Resource.String.Lbl_Ok));
                        } 
                        
                        ToggleVisibility(false); 
                    }
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        ToggleVisibility(false);
                        var errorText = error.Error.ErrorText;  
                        var errorId = error.Error.ErrorId;
                        switch (errorId)
                        {
                            case "3":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_3), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "4":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_4), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "5":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok)); break;
                            case "6":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_6), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "7":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_7), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "8":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_8), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "9":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_9), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "10":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_10), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "11":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_11), GetText(Resource.String.Lbl_Ok));
                                break;
                            default:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                break;
                        }
                    }
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Terms Of Service
        private void TxtTermsOfServiceOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = InitializeWoWonder.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtBirthdayOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                /*var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    try
                    {
                        if (AppSettings.IsUserYearsOld) // 18
                        {
                            if (!Methods.Time.HasAgeRequirement(time.Date)) // over 18 years
                            {
                                TxtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Error_IsUserYearsOld), ToastLength.Short);
                            }
                        }
                        else //All
                        {
                            Birthday = time.Date.ToString("dd-MM-yyyy");
							TxtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
				var dp = frag.Dialog.Window.DecorView;*/
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var viewGroup = FindViewById<ViewGroup>(Android.Resource.Id.Content);
                var dlgView = LayoutInflater.From(this).Inflate(Resource.Layout.birthday_dialog, viewGroup, false);

                etYear = dlgView.FindViewById<EditText>(Resource.Id.etYear);
                etMonth = dlgView.FindViewById<EditText>(Resource.Id.etMonth);
                etDay = dlgView.FindViewById<EditText>(Resource.Id.etDay);
                btnCancel = dlgView.FindViewById<AppCompatButton>(Resource.Id.acbCancel);
                btnOk = dlgView.FindViewById<AppCompatButton>(Resource.Id.acbOk);
                btnOk.SetOnClickListener(this);
                btnCancel.SetOnClickListener(this);

                dialogList.CustomView(dlgView, false);
                //dialogList.Items(arrayAdapter);
                //dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                mDialog = dialogList.ItemsCallback(this).Build();
                mDialog.Show();

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
                if (v.Id == btnOk.Id)
                {
                    DateTime dt = new DateTime(int.Parse(etYear.Text), int.Parse(etMonth.Text), int.Parse(etDay.Text));
                    if (AppSettings.IsUserYearsOld) // 18
                    {
                        if (!Methods.Time.HasAgeRequirement(dt.Date)) // over 18 years
                        {
                            Birthday = dt.Date.ToString("MM/dd/yyyy");
                            TxtBirthday.Text = dt.Date.ToString("MMM/dd/yyyy");
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Error_IsUserYearsOld), ToastLength.Short);
                        }
                    }
                    mDialog.Dismiss();
                }
                else if (v.Id == btnCancel.Id)
                {
                    mDialog.Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }

        private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                        arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                        break;
                    default:
                        arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                        arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                        break;
                }

                dialogList.Title(GetText(Resource.String.Lbl_Gender)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void TxtPasswordOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtPassword.Text.Length > 0)
                    SetHighLight(false, PasswordLayout, TxtPassword, TxtPasswordRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtPhoneNumOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtPhoneNum.Text.Length > 0)
                    SetHighLight(false, PhoneNumLayout, TxtPhoneNum, TxtPhoneNumRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void TxtBirthdayOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtBirthday.Text.Length > 0)
                    SetHighLight(false, BirthdayLayout, TxtBirthday, TxtBirthdayRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtGenderOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtGender.Text.Length > 0)
                    SetHighLight(false, GenderLayout, TxtGender, TxtGenderRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtEmailOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtEmail.Text.Length > 0)
                    SetHighLight(false, EmailLayout, TxtEmail, TxtEmailRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLastNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtLastName.Text.Length > 0)
                    SetHighLight(false, LastNameLayout, TxtLastName, TxtLastNameRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtFirstNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtFirstName.Text.Length > 0)
                    SetHighLight(false, FirstNameLayout, TxtFirstName, TxtFirstNameRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtUsernameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TxtUsername.Text.Length > 0)
                    SetHighLight(false, UsernameLayout, TxtUsername, TxtUsernameRequired);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                    {
                        TxtGender.Text = itemString;

                        var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                        GenderStatus = key ?? "male";
                        break;
                    }
                    default:
                    {
                        if (itemString == GetText(Resource.String.Radio_Male))
                        {
                            TxtGender.Text = GetText(Resource.String.Radio_Male);
                            GenderStatus = "male";
                        }
                        else if (itemString == GetText(Resource.String.Radio_Female))
                        {
                            TxtGender.Text = GetText(Resource.String.Radio_Female);
                            GenderStatus = "female";
                        }
                        else
                        {
                            TxtGender.Text = GetText(Resource.String.Radio_Male);
                            GenderStatus = "male";
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

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
                BtnSignUp.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetDataLogin(CreatAccountObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = TxtUsername.Text;
                UserDetails.FullName = TxtFirstName.Text + " " + TxtLastName.Text;
                UserDetails.Password = TxtPassword.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = TxtEmail.Text;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
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

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
                 
                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                PhoneLayout.Visibility = smsOrEmail == "sms" ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BlurBackground(BlurView blurView, float radius)
        {
            try
            {
                //float radius = 10f; 
                View decorView = Window.DecorView;
                //ViewGroup you want to start blur from. Choose root as close to BlurView in hierarchy as possible.
                ViewGroup rootView = decorView.FindViewById<ViewGroup>(Android.Resource.Id.Content);
                //Set drawable to draw in the beginning of each blurred frame (Optional). 
                //Can be used in case your layout has a lot of transparent space and your content
                //gets kinda lost after after blur is applied.
                Drawable windowBackground = decorView.Background;

                blurView.SetupWith(rootView)
                    .SetFrameClearDrawable(windowBackground)
                    .SetBlurAlgorithm(new RenderScriptBlur(this))
                    .SetBlurRadius(radius)
                    .SetBlurAutoUpdate(true)
                    .SetHasFixedTransformationMatrix(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetHighLight(bool state, RelativeLayout layout, EditText editText, TextView textView)
        {
            try
            {
                Color txtcolor, borderColor;
                if (state)
                {
                    textView.Visibility = ViewStates.Visible;
                    txtcolor = new Color(GetColor(Resource.Color.colorLoginHighlightText));
                    borderColor = new Color(GetColor(Resource.Color.colorLoginHighlightText));
                }
                else
                {
                    textView.Visibility = ViewStates.Gone;
                    txtcolor = new Color(GetColor(Resource.Color.gnt_white));
                    borderColor = new Color(GetColor(Resource.Color.transparent_border));
                }
                editText.SetHintTextColor(txtcolor);
                layout.Background.SetTint(borderColor);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
} 