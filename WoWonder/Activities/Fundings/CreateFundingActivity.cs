using System;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Funding;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;
using ImageViews.Rounded;
using System.Collections.Generic;
using Android.Views.InputMethods;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateFundingActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImgUploadPhoto;
        private RoundedImageView ImgCover;
        private AppCompatButton BtnCreateFunding/*, BtnWelcome*/;
        private EditText TxtTitle, TxtAmount, TxtDescription;
        private string PathImage, TypeDialog;
        private TextView tvCategory;
        private LinearLayout llStep1, llStep2, llStep3, llStep4;
        //private RelativeLayout rlStep;
        private EditText TxtName, TxtAddress, TxtPhoneNumber;
        private int nStep = 1;

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
                SetContentView(Resource.Layout.CreateFundingLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                    if (nStep > 1)
                    {
                        nStep -= 1;
                        SetStepChild();
                    }
                    else
                    {
                        Intent returnIntent = new Intent();
                        SetResult(Result.Canceled, returnIntent);
                        Finish();
                    }
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
                ImgCover = FindViewById<RoundedImageView>(Resource.Id.fundingCover);
                ImgUploadPhoto = FindViewById<ImageView>(Resource.Id.btn_selectimage);

                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);

                TxtAmount = FindViewById<EditText>(Resource.Id.AmountEditText);

                TxtDescription  = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                BtnCreateFunding = FindViewById<AppCompatButton>(Resource.Id.btnCreateFunding);

                //tvStep = FindViewById<TextView>(Resource.Id.tv_step);
                tvCategory = FindViewById<TextView>(Resource.Id.tv_fundraise_category);

                //BtnWelcome = FindViewById<AppCompatButton>(Resource.Id.btn_create_fraiser);

                //rlStep = FindViewById<RelativeLayout>(Resource.Id.rl_step);
                llStep1 = FindViewById<LinearLayout>(Resource.Id.ll_step1);
                llStep2 = FindViewById<LinearLayout>(Resource.Id.ll_step2);
                llStep3 = FindViewById<LinearLayout>(Resource.Id.ll_step3);
                llStep4 = FindViewById<LinearLayout>(Resource.Id.ll_step4);

                TxtName = FindViewById<EditText>(Resource.Id.et_fraiser_name);
                TxtAddress = FindViewById<EditText>(Resource.Id.et_street_address);
                TxtPhoneNumber = FindViewById<EditText>(Resource.Id.et_phone_number);
                
                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAddress, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhoneNumber, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //Methods.SetFocusable(TxtAmount);
                SetStepChild();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStepChild()
        {
            //tvStep.Text = GetText(Resource.String.Lbl_Step) + " " + nStep + "/4";
            switch (nStep)
            {
                case 0:
                    //rlStep.Visibility = ViewStates.Visible;
                    llStep1.Visibility = ViewStates.Gone;
                    llStep2.Visibility = ViewStates.Gone;
                    llStep3.Visibility = ViewStates.Gone;
                    llStep4.Visibility = ViewStates.Gone;
                    HideKeyboard();
                    //BtnCreateFunding.Visibility = ViewStates.Gone;
                    BtnCreateFunding.Text = GetString(Resource.String.lb_onboarding_accessibility_next);
                    break;
                case 1:
                    //rlStep.Visibility = ViewStates.Gone;
                    llStep1.Visibility = ViewStates.Visible;
                    llStep2.Visibility = ViewStates.Gone;
                    llStep3.Visibility = ViewStates.Gone;
                    llStep4.Visibility = ViewStates.Gone;
                    HideKeyboard();
                    //BtnCreateFunding.Visibility = ViewStates.Visible;
                    //BtnCreateFunding.Text = GetString(Resource.String.lb_onboarding_accessibility_next);
                    break;
                case 2:
                    //rlStep.Visibility = ViewStates.Gone;
                    llStep1.Visibility = ViewStates.Gone;
                    llStep2.Visibility = ViewStates.Visible;
                    llStep3.Visibility = ViewStates.Gone;
                    llStep4.Visibility = ViewStates.Gone;
                    //BtnCreateFunding.Visibility = ViewStates.Visible;
                    BtnCreateFunding.Text = GetString(Resource.String.lb_onboarding_accessibility_next);
                    break;
                case 3:
                    //rlStep.Visibility = ViewStates.Gone;
                    llStep1.Visibility = ViewStates.Gone;
                    llStep2.Visibility = ViewStates.Gone;
                    llStep3.Visibility = ViewStates.Visible;
                    llStep4.Visibility = ViewStates.Gone;
                    HideKeyboard();
                    //BtnCreateFunding.Visibility = ViewStates.Visible;
                    BtnCreateFunding.Text = GetString(Resource.String.lb_onboarding_accessibility_next);
                    break;
                case 4:
                    //rlStep.Visibility = ViewStates.Gone;
                    llStep1.Visibility = ViewStates.Gone;
                    llStep2.Visibility = ViewStates.Gone;
                    llStep3.Visibility = ViewStates.Gone;
                    llStep4.Visibility = ViewStates.Visible;
                    BtnCreateFunding.Visibility = ViewStates.Visible;
                    BtnCreateFunding.Text = GetString(Resource.String.Lbl_Save);
                    break;
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_NewFunding);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        BtnCreateFunding.Click += TxtAddOnClick;
                        ImgUploadPhoto.Click += BtnSelectImageOnClick;
                        tvCategory.Touch += TvCategoryClick;
                        //BtnWelcome.Click += BtnWelcomeOnClick;
                        TxtAmount.AfterTextChanged += TxtAmountAfterTextChanged;
                        //TxtAmount.Touch += TxtAmountOnTouch;
                        break;
                    default:
                        BtnCreateFunding.Click -= TxtAddOnClick;
                        ImgUploadPhoto.Click -= BtnSelectImageOnClick;
                        tvCategory.Touch -= TvCategoryClick;
                        //BtnWelcome.Click -= BtnWelcomeOnClick;
                        TxtAmount.AfterTextChanged -= TxtAmountAfterTextChanged;
                        //TxtAmount.Touch -= TxtAmountOnTouch;
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
                BtnCreateFunding = null!;
                ImgCover = null!;
                ImgUploadPhoto = null!;
                TxtTitle = null!;
                TxtAmount = null!;
                TxtDescription = null!;
                PathImage = null!;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Add Image
        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery(); //requestCode >> 500 => Image Gallery 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtAmountAfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            String s = null;
            try
            {
                // The comma in the format specifier does the trick
                var text = e.Editable.ToString();
                TxtAmount.AfterTextChanged -= TxtAmountAfterTextChanged;
                text = text.Replace(",", "");
                s = String.Format("{0:n0}", long.Parse(text));
                System.Console.WriteLine("========== " + s + " ==========");
                TxtAmount.Text = s;
                TxtAmount.SetSelection(s.Length);

                TxtAmount.AfterTextChanged += TxtAmountAfterTextChanged;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Amount
        //private void TxtAmountOnTouch(object sender, View.TouchEventArgs e)
        //{
        //    try
        //    {
        //        if (e?.Event?.Action != MotionEventActions.Down) return;

        //        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

        //        var arrayAdapter = new List<string> { "5","10","15","20","25","30","35","40","45","50","55","60","65","70","75","80","85","90","95","100" };

        //        dialogList.Title(GetText(Resource.String.Lbl_Amount));
        //        dialogList.Items(arrayAdapter);
        //        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
        //        dialogList.AlwaysCallSingleChoiceCallback();
        //        dialogList.ItemsCallback(this).Build().Show();
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        //Save 
        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    switch (nStep)
                    {
                        case 1:
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                                return;
                            }
                            if (string.IsNullOrEmpty(TxtAddress.Text) || string.IsNullOrWhiteSpace(TxtAddress.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseEnterAddress), ToastLength.Short);
                                return;
                            }
                            if (string.IsNullOrEmpty(TxtPhoneNumber.Text) || string.IsNullOrWhiteSpace(TxtPhoneNumber.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseEnterPhone), ToastLength.Short);
                                return;
                            }
                            nStep += 1;
                            SetStepChild();
                            break;
                        case 2:
                            if (string.IsNullOrEmpty(tvCategory.Text) || string.IsNullOrWhiteSpace(tvCategory.Text) || (tvCategory.Text == GetText(Resource.String.Lbl_ChooseCategory)))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ChooseCategory), ToastLength.Short);
                                return;
                            }
                            if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short);
                                return;
                            }
                            nStep += 1;
                            SetStepChild();
                            break;
                        case 3:
                            if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtTitle.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_title), ToastLength.Short);
                                return;
                            }
                            if (string.IsNullOrEmpty(TxtDescription.Text))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short);
                                return;
                            }
                            nStep += 1;
                            SetStepChild();
                            break;
                        case 4:
                            if (string.IsNullOrEmpty(PathImage))
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                                return;
                            }
                            OnSave();
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void BtnWelcomeOnClick(object sender, EventArgs e)
        {
            nStep += 1;
            SetStepChild();
        }

        private async void OnSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtTitle.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var (apiStatus, respond) = await RequestsAsync.Funding.CreateFundingAsync(TxtTitle.Text, TxtDescription.Text, TxtAmount.Text, PathImage);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateFundingObject result:
                                        {
                                            AndHUD.Shared.Dismiss(this);
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                            //Add new item to list
                                            if (result.Data?.FundData != null)
                                            {
                                                result.Data.FundData.Value.FundDataClass.UserData ??= result.Data.Publisher;
                                                var instance = FundingActivity.GetInstance();
                                                if (instance?.FundingTab?.MAdapter != null)
                                                {
                                                    instance?.FundingTab?.MAdapter?.FundingList.Insert(0, result.Data.FundData.Value.FundDataClass);
                                                    RunOnUiThread(() => instance.FundingTab?.MAdapter?.NotifyItemInserted(0));
                                                }

                                                if (instance?.MyFundingTab?.MAdapter != null)
                                                {
                                                    instance?.MyFundingTab?.MAdapter?.FundingList.Insert(0, result.Data.FundData.Value.FundDataClass);
                                                    RunOnUiThread(() => instance.MyFundingTab?.MAdapter?.NotifyItemInserted(0));
                                                }
                                            }

                                            Intent returnIntent = new Intent();
                                            SetResult(Result.Ok, returnIntent);
                                            Finish();
                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayAndHudErrorResult(this, respond);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void TvCategoryClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                arrayAdapter.Add("Political Causes");
                arrayAdapter.Add("Medical Needs");
                arrayAdapter.Add("Business");
                arrayAdapter.Add("Personal Emergency");
                arrayAdapter.Add("Animals & Pets");
                arrayAdapter.Add("Funerals & Memorial Services");
                arrayAdapter.Add("Nonprofit Charities");

                switch (arrayAdapter?.Count)
                {
                    case > 0:
                        {
                            TypeDialog = "Category";

                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            dialogList.Title(GetText(Resource.String.Lbl_ChooseCategory)).TitleColorRes(Resource.Color.primary);
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

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    //If its from Camera or Gallery
                    case CropImage.CropImageActivityRequestCode:
                    {
                        var result = CropImage.GetActivityResult(data);

                        switch (resultCode)
                        {
                            case Result.Ok:
                            {
                                switch (result.IsSuccessful)
                                {
                                    case true:
                                    {
                                        var resultUri = result.Uri;

                                        switch (string.IsNullOrEmpty(resultUri.Path))
                                        {
                                            case false:
                                            {
                                                PathImage = resultUri.Path;
                                                File file2 = new File(resultUri.Path);
                                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgCover);


                                                //GlideImageLoader.LoadImage(this, resultUri.Path, ImgCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                                break;
                                            }
                                            default:
                                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                                break;
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
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

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        OpenDialogGallery();
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void OpenDialogGallery()
        {
            try
            {
                if (!WoWonderTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_AllowedFileUpload), GetText(Resource.String.Lbl_Ok));
                    return;
                }
                
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                        break;
                    }
                    default:
                    {
                        if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                        {
                            Methods.Path.Chack_MyFolder();

                            //Open Image 
                            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                            CropImage.Activity()
                                .SetInitialCropWindowPaddingRatio(0)
                                .SetAutoZoomEnabled(true)
                                .SetMaxZoom(4)
                                .SetGuidelines(CropImageView.Guidelines.On)
                                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                                .SetOutputUri(myUri).Start(this);
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
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

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                //TxtAmount.Text = itemString;
                if (TypeDialog == "Category")
                {
                    tvCategory.Text = itemString;
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
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
        #endregion

    }
} 