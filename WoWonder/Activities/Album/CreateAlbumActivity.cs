using System;
using System.Collections.ObjectModel;
using System.Linq;
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
using AndroidX.RecyclerView.Widget;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using MaterialDialogsCore;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Album;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateAlbumActivity : BaseActivity
    {
        #region Variables Basic

        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private EditText TxtNameAlbum;
        private TextView TxtAdd;
        private PublisherAdView PublisherAdView;
        private LinearLayout llStep1;
        private RelativeLayout rlStep2;
        private ImageView EmptyPhotos;
        private AppCompatButton BtnCreateAlbum;
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
                SetContentView(Resource.Layout.CreateAlbumLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                SetStep();
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                llStep1 = FindViewById<LinearLayout>(Resource.Id.ll_step1);
                rlStep2 = FindViewById<RelativeLayout>(Resource.Id.rl_step2);
                EmptyPhotos = FindViewById<ImageView>(Resource.Id.imageView1);
                BtnCreateAlbum = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);
                TxtNameAlbum = (EditText)FindViewById(Resource.Id.NameEditText);

                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Create);
                TxtAdd.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtNameAlbum, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

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
                    toolBar.Title = GetString(Resource.String.Lbl_CreateAlbum);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                //var LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                //MRecycler.AddItemDecoration(new GridSpacingItemDecoration(2, 2, true));
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                //var attach = new Attachments
                //{
                //    Id = MAdapter.AttachmentList.Count + 1,
                //    TypeAttachment = "Default",
                //    FileSimple = "addImage",
                //    FileUrl = "addImage"
                //};

                //MAdapter.Add(attach);
                //MAdapter.NotifyDataSetChanged();
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
                        MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        TxtAdd.Click += TxtAddOnClick;
                        BtnCreateAlbum.Click += BtnCreateAlbum_Click;
                        break;
                    default:
                        MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick -= MAdapterOnItemClick;
                        TxtAdd.Click -= TxtAddOnClick;
                        BtnCreateAlbum.Click -= BtnCreateAlbum_Click;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStep()
        {
            try
            {
                switch (nStep)
                {
                    case 1:
                        llStep1.Visibility = ViewStates.Visible;
                        rlStep2.Visibility = ViewStates.Gone;
                        BtnCreateAlbum.Text = GetText(Resource.String.Lbl_CreateAlbum);
                        TxtAdd.Visibility = ViewStates.Gone;
                        break;
                    case 2:
                        llStep1.Visibility = ViewStates.Gone;
                        rlStep2.Visibility = ViewStates.Visible;
                        BtnCreateAlbum.Text = GetText(Resource.String.Lbl_AddPhotos);
                        TxtAdd.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnCreateAlbum_Click(object sender, EventArgs e)
        {
            try
            {
                switch (nStep)
                {
                    case 1:
                        nStep += 1;
                        SetStep();
                        break;
                    case 2:
                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher 
                            case < 23:
                                OpenDialogGallery();
                                break;
                            default:
                                {
                                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                                    {
                                        OpenDialogGallery();
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
                                }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                PublisherAdView?.Destroy();
                MAdapter = null!;
                MRecycler = null!;
                TxtNameAlbum = null!;
                TxtAdd = null!;

                PublisherAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
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
                                MAdapter.Remove(item);
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
                    if (string.IsNullOrEmpty(TxtNameAlbum.Text.Replace(" ", "")))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    switch (MAdapter.AttachmentList.Count)
                    {
                        case <= 1:
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                            break;
                        default:
                            {
                                //Show a progress
                                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                                var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                                var (apiStatus, respond) = await RequestsAsync.Album.CreateAlbumAsync(TxtNameAlbum.Text.Replace(" ", ""), new ObservableCollection<Attachments>(list));
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case CreateAlbumObject result:
                                                    {
                                                        switch (result.Data.PhotoAlbum.Count)
                                                        {
                                                            case > 0:
                                                                {
                                                                    AndHUD.Shared.Dismiss(this);
                                                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                                                    //AlbumItem >> PostDataObject  
                                                                    Intent returnIntent = new Intent();
                                                                    returnIntent?.PutExtra("AlbumItem", JsonConvert.SerializeObject(result.Data));
                                                                    SetResult(Result.Ok, returnIntent);
                                                                    Finish();
                                                                    break;
                                                                }
                                                        }

                                                        break;
                                                    }
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayAndHudErrorResult(this, respond);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            switch (item)
                            {
                                case null:
                                    return;
                            }
                            if (item.TypeAttachment != "Default") return;
                            switch ((int)Build.VERSION.SdkInt)
                            {
                                // Check if we're running on Android 5.0 or higher 
                                case < 23:
                                    OpenDialogGallery();
                                    break;
                                default:
                                    {
                                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                                        {
                                            OpenDialogGallery();
                                        }
                                        else
                                        {
                                            new PermissionsController(this).RequestPermission(108);
                                        }

                                        break;
                                    }
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
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    // Add image 
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                        {
                            var result = CropImage.GetActivityResult(data);

                            if (result.IsSuccessful)
                            {
                                var resultUri = result.Uri;

                                if (string.IsNullOrEmpty(resultUri.Path) == false)
                                {
                                    //  Chick if it was successful
                                    var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(resultUri.Path);
                                    if (check is false)
                                    {
                                        if (info == "AdultImages")
                                        {
                                            //this file not allowed 
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);

                                            var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                                            dialog.Content(GetText(Resource.String.Lbl_Error_AdultImages));
                                            dialog.PositiveText(GetText(Resource.String.Lbl_IgnoreAndSend)).OnPositive((materialDialog, action) =>
                                            {
                                                try
                                                {
                                                    var attach = new Attachments
                                                    {
                                                        Id = MAdapter.AttachmentList.Count + 1,
                                                        TypeAttachment = "postPhotos[]",
                                                        FileSimple = resultUri.Path,
                                                        FileUrl = resultUri.Path
                                                    };

                                                    MAdapter.Add(attach);
                                                    // 
                                                    EmptyPhotos.Visibility = ViewStates.Gone;
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }
                                            });
                                            dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new WoWonderTools.MyMaterialDialog());
                                            dialog.AlwaysCallSingleChoiceCallback();
                                            dialog.Build().Show();
                                        }
                                        else
                                        {
                                            //this file not supported on the server , please select another file 
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                                        }
                                    }
                                    else
                                    {
                                        var attach = new Attachments
                                        {
                                            Id = MAdapter.AttachmentList.Count + 1,
                                            TypeAttachment = "postPhotos[]",
                                            FileSimple = resultUri.Path,
                                            FileUrl = resultUri.Path
                                        };

                                        MAdapter.Add(attach);
                                        // 
                                        EmptyPhotos.Visibility = ViewStates.Gone;
                                    } 
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
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
    }
}