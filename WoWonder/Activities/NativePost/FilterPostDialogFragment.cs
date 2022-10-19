using Android.Content;
using Android.OS;
using Android.Views;
using System;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.NativePost
{
    public class FilterPostDialogFragment : BottomSheetDialogFragment 
    {
        #region Variables Basic

        private WRecyclerView Instances;

        private ImageButton IconBack;
        private RadioButton RadioAllPeople, RadioPeopleIFollow;
        private RadioButton RadioAllPost, RadioText, RadioImage, RadioVideo, RadioFile, RadioMusic, RadioMap;
        private TextView BtnClear;
        private AppCompatButton BtnShowResult;
        private int Type;
        private string Filter;

        #endregion

        #region General
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.FilterPostLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                Instances = WRecyclerView.GetInstance();
                InitComponent(view);
                LoadData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDetach()
        {
            try
            {
                base.OnDetach(); 
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
                IconBack = view.FindViewById<ImageButton>(Resource.Id.ib_back);

                RadioAllPeople = view.FindViewById<RadioButton>(Resource.Id.radioAllPeople);
                RadioPeopleIFollow = view.FindViewById<RadioButton>(Resource.Id.radioPeople_i_Follow);
                 
                RadioAllPost = view.FindViewById<RadioButton>(Resource.Id.radioAllPost);
                RadioText = view.FindViewById<RadioButton>(Resource.Id.radioText);
                RadioImage = view.FindViewById<RadioButton>(Resource.Id.radioImage);
                RadioVideo = view.FindViewById<RadioButton>(Resource.Id.radioVideo);
                RadioFile = view.FindViewById<RadioButton>(Resource.Id.radioFile);
                RadioMusic = view.FindViewById<RadioButton>(Resource.Id.radioMusic);
                RadioMap = view.FindViewById<RadioButton>(Resource.Id.radioMap);

                BtnClear = view.FindViewById<TextView>(Resource.Id.Clearbutton);
                BtnShowResult = view.FindViewById<AppCompatButton>(Resource.Id.ShowResultbutton);

                IconBack.Click += IconBackOnClick;
               
                RadioAllPeople.CheckedChange += RadioAllPeopleOnCheckedChange;
                RadioPeopleIFollow.CheckedChange += RadioPeople_i_FollowOnCheckedChange;
              
                RadioAllPost.CheckedChange += RadioAllPostOnCheckedChange;
                RadioText.CheckedChange += RadioTextOnCheckedChange;
                RadioImage.CheckedChange += RadioImageOnCheckedChange;
                RadioVideo.CheckedChange += RadioVideoOnCheckedChange;
                RadioFile.CheckedChange += RadioFileOnCheckedChange;
                RadioMusic.CheckedChange += RadioMusicOnCheckedChange;
                RadioMap.CheckedChange += RadioMapOnCheckedChange;

                BtnClear.Click += BtnClearOnClick;
                BtnShowResult.Click += BtnShowResultOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void BtnShowResultOnClick(object sender, EventArgs e)
        {
            try
            { 
                Instances?.SetPostAndFilterType(Type, Filter);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnClearOnClick(object sender, EventArgs e)
        {
            try
            {
                Type = 0;
                Filter = "0";
                Instances?.SetPostAndFilterType(Type, Filter);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioMapOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 6;

                    RadioAllPost.Checked = false;

                    RadioText.Checked = false;
                    RadioImage.Checked = false;
                    RadioVideo.Checked = false;
                    RadioFile.Checked = false;
                    RadioMusic.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioMusicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 4;

                    RadioAllPost.Checked = false;

                    RadioText.Checked = false;
                    RadioImage.Checked = false;
                    RadioVideo.Checked = false;
                    RadioFile.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioFileOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 5;

                    RadioAllPost.Checked = false;

                    RadioText.Checked = false;
                    RadioImage.Checked = false;
                    RadioVideo.Checked = false;
                    RadioMusic.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioVideoOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 3;

                    RadioAllPost.Checked = false;

                    RadioText.Checked = false;
                    RadioImage.Checked = false;
                    RadioFile.Checked = false;
                    RadioMusic.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioImageOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 2;

                    RadioAllPost.Checked = false;

                    RadioText.Checked = false;
                    RadioVideo.Checked = false;
                    RadioFile.Checked = false;
                    RadioMusic.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioTextOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 1;

                    RadioAllPost.Checked = false;

                    RadioImage.Checked = false;
                    RadioVideo.Checked = false;
                    RadioFile.Checked = false;
                    RadioMusic.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //All Post Types
        private void RadioAllPostOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Type = 0;

                    RadioAllPost.Checked = true;

                    RadioText.Checked = false;
                    RadioImage.Checked = false;
                    RadioVideo.Checked = false;
                    RadioFile.Checked = false;
                    RadioMusic.Checked = false;
                    RadioMap.Checked = false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //All Post for People I Follow 
        private void RadioPeople_i_FollowOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Filter = "1";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //All Post for All Users 
        private void RadioAllPeopleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (e.IsChecked)
                {
                    Filter = "0";
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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
         
        #endregion

        private void LoadData()
        {
            try
            {
                Filter = Instances.GetFilter();
                var postType =  Instances.GetPostType();

                switch (Filter)
                {
                    case "1":
                        RadioAllPeople.Checked = false;
                        RadioPeopleIFollow.Checked = true;
                        break;
                    default:
                        RadioAllPeople.Checked = true;
                        RadioPeopleIFollow.Checked = false;
                        break;
                } 
                
                switch (postType)
                {
                    //text
                    case "text":
                        Type = 1;

                        RadioAllPost.Checked = false;

                        RadioText.Checked = true;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = false;
                        break;
                    //image
                    case "photos":
                        Type = 2;
                        RadioAllPost.Checked = false;

                        RadioText.Checked = false;
                        RadioImage.Checked = true;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = false;
                        break;
                    //video
                    case "video":
                        Type = 3;

                        RadioAllPost.Checked = false;

                        RadioText.Checked = false;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = true;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = false;
                        break;
                    //Music
                    case "music":
                        Type = 4;

                        RadioAllPost.Checked = false;

                        RadioText.Checked = false;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = true;
                        RadioMap.Checked = false;
                        break;
                    //File
                    case "files":
                        Type = 5;

                        RadioAllPost.Checked = false;

                        RadioText.Checked = false;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = true;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = false;
                        break;
                    //Map
                    case "maps":
                        Type = 6;

                        RadioAllPost.Checked = false;

                        RadioText.Checked = false;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = true;
                        break;
                    //All
                    default:
                        Type = 0;

                        RadioAllPost.Checked = true;

                        RadioText.Checked = false;
                        RadioImage.Checked = false;
                        RadioVideo.Checked = false;
                        RadioFile.Checked = false;
                        RadioMusic.Checked = false;
                        RadioMap.Checked = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}