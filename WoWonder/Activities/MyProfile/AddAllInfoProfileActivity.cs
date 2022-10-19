using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Base;
using WoWonder.Activities.Suggested.User;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AddAllInfoProfileActivity : BaseActivity, View.IOnFocusChangeListener 
    {
        #region Variables Basic

        private AppCompatButton BtnSave;
        private TextView TxtTitle,TxtStep;
        private LinearLayout Step1Layout, Step2Layout;
        private ImageView YourImage,  BtnAddImage;

        private EditText TxtFirstName, TxtLastName, TxtLocation, TxtWork, TxtSchool;
        private EditText TxtAbout,TxtMobile, TxtWebsite;

        private RecyclerView MRecycler;
        private GendersAdapter MAdapter;

        private PublisherAdView PublisherAdView;
        private string PathYourImage,IdRelationShip;

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
                SetContentView(Resource.Layout.AddAllInfoProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                SetStep(1); 
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

                PublisherAdView?.Resume();
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

                PublisherAdView?.Pause();
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
                TxtTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtStep = FindViewById<TextView>(Resource.Id.toolbar_step);

                Step1Layout = FindViewById<LinearLayout>(Resource.Id.step1);
                Step2Layout = FindViewById<LinearLayout>(Resource.Id.step2);

                YourImage = FindViewById<ImageView>(Resource.Id.Image);
                BtnAddImage = FindViewById<ImageView>(Resource.Id.btn_AddPhoto);
                 
                BtnSave = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                TxtFirstName = FindViewById<EditText>(Resource.Id.FirstNameEditText);
                TxtLastName = FindViewById<EditText>(Resource.Id.LastNameEditText);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                TxtWork = FindViewById<EditText>(Resource.Id.WorkStatusEditText);
                TxtSchool = FindViewById<EditText>(Resource.Id.SchoolEditText);

                TxtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);
                TxtMobile = FindViewById<EditText>(Resource.Id.PhoneEditText);
                TxtWebsite = FindViewById<EditText>(Resource.Id.WebsiteEditText);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.RelationshipRecycler);
                 
                Methods.SetColorEditText(TxtFirstName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLastName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMobile, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWebsite, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWork, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSchool, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                Methods.SetColorEditText(TxtAbout, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                 
                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    //toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(false);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MRecycler.HasFixedSize = true;
                MRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                MAdapter = new GendersAdapter(this)
                {
                    GenderList = new ObservableCollection<Classes.Gender>()
                };
                MRecycler.SetAdapter(MAdapter);
                MRecycler.NestedScrollingEnabled = false;
                MAdapter.NotifyDataSetChanged();
                MRecycler.Visibility = ViewStates.Visible;

                string[] relationshipArray = Application.Context.Resources?.GetStringArray(Resource.Array.RelationShipArray); 
                for (int i = 0; i < relationshipArray?.Length; i++)
                {
                    MAdapter.GenderList.Add(new Classes.Gender
                    {
                        GenderId = i.ToString(),
                        GenderName = relationshipArray[i],
                        GenderColor = AppSettings.MainColor,
                        GenderSelect = false
                    });
                }
                 
                MAdapter.NotifyDataSetChanged();
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
                        BtnSave.Click += TxtSaveOnClick;
                        TxtLocation.OnFocusChangeListener = this;
                        BtnAddImage.Click += BtnAddImageOnClick;
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        break;
                    default:
                        BtnSave.Click -= TxtSaveOnClick;
                        TxtLocation.OnFocusChangeListener = null!;
                        BtnAddImage.Click += BtnAddImageOnClick;
                        MAdapter.ItemClick -= MAdapterOnItemClick;
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
                PublisherAdView?.Destroy();
                BtnSave = null!;
                TxtFirstName = null!;
                TxtLastName = null!;
                TxtLocation = null!;
                TxtMobile = null!;
                TxtWebsite = null!;
                TxtWork = null!;
                TxtSchool = null!;  
                PublisherAdView = null!;
                IdRelationShip = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnAddImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, GendersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                    {
                        var item = MAdapter.GetItem(position);
                        if (item != null)
                        {
                            var check = MAdapter.GenderList.Where(a => a.GenderSelect).ToList();
                            switch (check.Count)
                            {
                                case > 0:
                                {
                                    foreach (var all in check)
                                        all.GenderSelect = false;
                                    break;
                                }
                            }

                            item.GenderSelect = true;
                            MAdapter.NotifyDataSetChanged();

                            IdRelationShip = item.GenderId;
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }
                   
                if (BtnSave.Tag?.ToString() == "Next")
                {
                    if (string.IsNullOrEmpty(PathYourImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(PathYourImage) });
                    SetStep(2);

                    return;
                }

                if (!string.IsNullOrEmpty(TxtMobile.Text) && !Methods.FunString.IsPhoneNumber(TxtMobile.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PhoneNumberIsWrong), ToastLength.Short);
                    return;
                }

                if (!string.IsNullOrEmpty(TxtWebsite.Text) && Methods.FunString.Check_Regex(TxtWebsite.Text) != "Website")
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Website), ToastLength.Short);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                var dictionary = new Dictionary<string, string>
                {
                    {"first_name", TxtFirstName.Text},
                    {"last_name", TxtLastName.Text},
                    {"address", TxtLocation.Text},
                    {"phone_number", TxtMobile.Text},
                    {"website", TxtWebsite.Text},
                    {"working", TxtWork.Text},
                    {"school", TxtSchool.Text},
                    {"about", TxtAbout.Text},
                    {"relationship", IdRelationShip},
                };

                var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dictionary);
                if (apiStatus == 200)
                {
                    if (respond is MessageObject result)
                    { 
                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.FirstName = TxtFirstName.Text;
                            dataUser.LastName = TxtLastName.Text;
                            dataUser.Address = TxtLocation.Text;
                            dataUser.PhoneNumber = TxtMobile.Text;
                            dataUser.Website = TxtWebsite.Text;
                            dataUser.Working = TxtWork.Text;
                            dataUser.About = TxtAbout.Text;
                            dataUser.School = TxtSchool.Text;
                            dataUser.RelationshipId = IdRelationShip;

                            dataUser.Avatar = PathYourImage;
                            UserDetails.Avatar = PathYourImage;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                        }

                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);
                        AndHUD.Shared.Dismiss(this);

                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent); 

                        Finish();
                    }
                }
                else
                    Methods.DisplayAndHudErrorResult(this, respond);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnClick()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        //Open intent Camera when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    default:
                        {
                            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Camera when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
                            }

                            break;
                        }
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
                    case 502 when resultCode == Result.Ok:
                        GetPlaceFromPicker(data);
                        break;
                    //If its from Camera or Gallery  
                    case CropImage.CropImageActivityRequestCode:
                        {
                            CropImage.ActivityResult result = CropImage.GetActivityResult(data);

                            switch (resultCode)
                            {
                                case Result.Ok when result.IsSuccessful:
                                    {
                                        Uri resultUri = result.Uri;

                                        switch (string.IsNullOrEmpty(resultUri.Path))
                                        {
                                            case false:
                                                PathYourImage = resultUri.Path;
                                                File file2 = new File(resultUri.Path);
                                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(YourImage);
                                                break;
                                            default:
                                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Short);
                                                break;
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
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open intent Camera when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    //Image Picker
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open Image 
                        OpenGallery();
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

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                TxtLocation.Text = string.IsNullOrEmpty(placeAddress) switch
                {
                    //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    false => placeAddress,
                    _ => TxtLocation.Text
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Update Image Avatar
         
        private void OpenGallery()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task Update_Image_Api(string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(path);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result:
                                        {
                                            Console.WriteLine(result.Message);
                                           
                                             
                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
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

        private void SetStep(int step)
        {
            try
            {
                if (step == 1)
                {
                    TxtTitle.Text = GetText(Resource.String.Lbl_Iam);
                    TxtStep.Text = "1/2";

                    Step1Layout.Visibility = ViewStates.Visible;
                    Step2Layout.Visibility = ViewStates.Gone;

                    BtnSave.Text = GetText(Resource.String.Lbl_Next);
                    BtnSave.Tag = "Next";

                    var local = ListUtils.MyProfileList?.FirstOrDefault();
                    if (local != null)
                    {
                        TxtFirstName.Text = Methods.FunString.DecodeString(local.FirstName);
                        TxtLastName.Text = Methods.FunString.DecodeString(local.LastName);
                    }
                }
                else if (step == 2)
                {
                    TxtTitle.Text = GetText(Resource.String.Lbl_WhatAboutYou);
                    TxtStep.Text = "2/2";

                    Step1Layout.Visibility = ViewStates.Gone;
                    Step2Layout.Visibility = ViewStates.Visible;

                    BtnSave.Text = GetText(Resource.String.Lbl_Save);
                    BtnSave.Tag = "Save";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnFocusChange(View v, bool hasFocus)
        {
            if (v?.Id == TxtLocation.Id && hasFocus)
            {
                TxtLocationOnClick();
            }
        }

    }
}