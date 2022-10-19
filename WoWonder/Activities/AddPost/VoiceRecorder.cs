using System;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Com.Sothree.Slidinguppanel;
using Google.Android.Material.BottomSheet;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.AddPost
{
    public class VoiceRecorder : BottomSheetDialogFragment
    {
        #region Variables Basic

        private readonly AddPostActivity MainActivityContext;
        private readonly PostSharingActivity PostSharingActivity;

        private TextView IconClose, IconMicrophone;
        private CircleButton RecordPlayButton, RecordCloseButton, SendRecordButton, BtnVoice;
        private LinearLayout RecordLayout;
        private SeekBar VoiceSeekBar;
        private string RecordFilePath, TextRecorder;
        private Methods.AudioRecorderAndPlayer AudioPlayerClass;
        private Timer TimerSound;
        private bool IsRecording;
        private MediaPlayer MediaPlayer;
        private readonly string PageName;
        #endregion

        #region General

        public VoiceRecorder(Activity activityPage ,string page)
        {
            try
            {
                PageName = page;
                if (activityPage is AddPostActivity activity)
                {
                    MainActivityContext = activity;
                }
                else if (activityPage is PostSharingActivity sharingActivity)
                {
                    PostSharingActivity = sharingActivity;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }     
        }
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

                // clone the inflater using the ContextThemeWrapper 
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.DialogVoiceRecorder, container, false);

                InitComponent(view);
                 
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        { 
            try
            {
                StopAudioPlay();

                base.OnDestroy();
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
                IconClose = view.FindViewById<TextView>(Resource.Id.IconBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconClose, IonIconsFonts.Close);
                IconClose.Click += IconClose_Click;

                IconMicrophone = view.FindViewById<TextView>(Resource.Id.iconMicrophone);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMicrophone, IonIconsFonts.Microphone);

                RecordLayout = view.FindViewById<LinearLayout>(Resource.Id.recordLayout);
                
                RecordPlayButton = view.FindViewById<CircleButton>(Resource.Id.playButton);
                RecordPlayButton.Click += RecordPlayButton_Click;
                 
                RecordCloseButton = view.FindViewById<CircleButton>(Resource.Id.closeRecordButton);
                RecordCloseButton.Click += RecordCloseButtonClick;

                SendRecordButton = view.FindViewById<CircleButton>(Resource.Id.sendRecordButton);
                SendRecordButton.Visibility = ViewStates.Visible;
                SendRecordButton.Click += SendRecordButton_Click;

                VoiceSeekBar = view.FindViewById<SeekBar>(Resource.Id.voiceseekbar);
                VoiceSeekBar.Max = 10000;
                VoiceSeekBar.Progress = 0;
                 
                BtnVoice = view.FindViewById<CircleButton>(Resource.Id.startRecordButton);
                BtnVoice.LongClickable = true;
                BtnVoice.Tag = "Free";
                BtnVoice.LongClick += BtnVoiceOnLongClick;
                BtnVoice.Touch += BtnVoiceOnTouch;
                 
                AudioPlayerClass = new Methods.AudioRecorderAndPlayer("");
                TimerSound = new Timer();
                TextRecorder = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event
         
        private void SendRecordButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (PageName == "AddPost")
                {
                    MainActivityContext.NameAlbumButton.Visibility = ViewStates.Gone;

                    //remove file the type
                    MainActivityContext.AttachmentsAdapter.RemoveAll();

                    var attach = new Attachments
                    {
                        Id = MainActivityContext.AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postMusic",
                        FileSimple = "Record_File",
                        FileUrl = RecordFilePath
                    };
                     
                    MainActivityContext.AttachmentsAdapter.Add(attach);

                    MainActivityContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else
                {
                    PostSharingActivity.NameAlbumButton.Visibility = ViewStates.Gone;

                    //remove file the type
                    PostSharingActivity.AttachmentsAdapter.RemoveAll();

                    var attach = new Attachments
                    {
                        Id = PostSharingActivity.AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postMusic",
                        FileSimple = "Record_File",
                        FileUrl = RecordFilePath
                    };

                    PostSharingActivity.AttachmentsAdapter.Add(attach);

                    PostSharingActivity.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);

                }
                 
                Dismiss();
            }
            catch (Exception ez)
            {
                Console.WriteLine(ez);
            }
        }

        //Back
        private void IconClose_Click(object sender, EventArgs e)
        {
            try
            {
                StopAudioPlay();
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void RecordCloseButtonClick(object sender, EventArgs e)
        {
            try
            { 
                StopAudioPlay();

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                        break;
                }
                 
                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);

                RecordLayout.Visibility = ViewStates.Gone;

                BtnVoice.SetImageResource(0);
                BtnVoice.Tag = "Free";

                RecordFilePath = ""; 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecordPlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (MediaPlayer)
                {
                    case null:
                        {
                            MediaPlayer = new MediaPlayer();
                            MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());

                            MediaPlayer.Completion += (sender, e) =>
                            {
                                try
                                {
                                    RecordPlayButton.Tag = "Play";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                                     
                                    MediaPlayer.Stop();
                                    MediaPlayer.Reset();
                                    MediaPlayer = null!;

                                    TimerSound.Enabled = false;
                                    TimerSound.Stop();
                                    TimerSound = null!;

                                    VoiceSeekBar.Progress = 0;
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            };

                            MediaPlayer.Prepared += (s, ee) =>
                            {
                                try
                                {
                                    RecordPlayButton.Tag = "Pause";
                                    RecordPlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                    TimerSound ??= new Timer { Interval = 1000 };

                                    MediaPlayer.Start();
                                     
                                    TimerSound.Elapsed += (sender, eventArgs) =>
                                    {
                                        Activity?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                if (TimerSound != null && TimerSound.Enabled)
                                                {
                                                    if (MediaPlayer != null)
                                                    {
                                                        int totalDuration = MediaPlayer.Duration;
                                                        int currentDuration = MediaPlayer.CurrentPosition;

                                                        // Updating progress bar
                                                        int progress = GetProgressSeekBar(currentDuration, totalDuration);

                                                        switch (Build.VERSION.SdkInt)
                                                        {
                                                            case >= BuildVersionCodes.N:
                                                                VoiceSeekBar.SetProgress(progress, true);
                                                                break;
                                                            default:
                                                                // For API < 24 
                                                                VoiceSeekBar.Progress = progress;
                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                                RecordPlayButton.Tag = "Play";
                                            }
                                        });
                                    };
                                    TimerSound.Start();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };

                            if (RecordFilePath.Contains("http"))
                            {
                                MediaPlayer.SetDataSource(Activity, Uri.Parse(RecordFilePath));
                                MediaPlayer.PrepareAsync();
                            }
                            else
                            {
                                Java.IO.File file2 = new Java.IO.File(RecordFilePath);
                                var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                                MediaPlayer.SetDataSource(Activity, photoUri);
                                MediaPlayer.PrepareAsync();
                            }
                            
                            break;
                        }
                    default:
                        switch (RecordPlayButton?.Tag?.ToString())
                        {
                            case "Play":
                                {
                                    RecordPlayButton.Tag = "Pause";
                                    RecordPlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                    MediaPlayer?.Start();

                                    if (TimerSound != null)
                                    {
                                        TimerSound.Enabled = true;
                                        TimerSound.Start();
                                    }

                                    break;
                                }
                            case "Pause":
                                {
                                    RecordPlayButton.Tag = "Play";
                                    RecordPlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                                    MediaPlayer?.Pause();

                                    if (TimerSound != null)
                                    {
                                        TimerSound.Enabled = false;
                                        TimerSound.Stop();
                                    }

                                    break;
                                }
                        }

                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopAudioPlay()
        {
            try
            {
                RecordPlayButton.Tag = "Play";
                RecordPlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                if (MediaPlayer != null)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Reset();
                }
                MediaPlayer = null!;


                if (TimerSound != null)
                {
                    TimerSound.Enabled = false;
                    TimerSound.Stop();
                }

                TimerSound = null!;

                VoiceSeekBar.Progress = 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
          
        private int GetProgressSeekBar(int currentDuration, int totalDuration)
        {
            try
            {
                // calculating percentage
                double progress = (double)currentDuration / totalDuration * 10000;
                return progress switch
                {
                    >= 0 =>
                        // return percentage
                        Convert.ToInt32(progress),
                    _ => 0
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }
          
        //record voices ( Permissions is 102 )
        private void BtnVoiceOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    case < 23:
                        StartRecording();
                        break;
                    default:
                    {
                        //Check to see if any permission in our group is available, if one, then all are
                        if (Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                        {
                            StartRecording();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(102);
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

        private void BtnVoiceOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                var handled = false;

                switch (e?.Event?.Action)
                {
                    case MotionEventActions.Up:
                        try
                        {
                            switch (IsRecording)
                            {
                                case true:
                                {
                                    AudioPlayerClass.StopRecording();
                                    RecordFilePath = AudioPlayerClass.GetRecorded_Sound_Path();

                                    BtnVoice.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                                    BtnVoice.SetImageResource(0);
                                    BtnVoice.Tag = "tick";

                                    switch (TextRecorder)
                                    {
                                        case "Recording":
                                        {
                                            switch (string.IsNullOrEmpty(RecordFilePath))
                                            {
                                                case false:
                                                    Console.WriteLine("FilePath" + RecordFilePath);

                                                    RecordLayout.Visibility = ViewStates.Visible;
                                                    break;
                                            }

                                            TextRecorder = "";
                                            break;
                                        }
                                    }

                                    IsRecording = false;
                                    break;
                                }
                                default:
                                {
                                    switch (UserDetails.SoundControl)
                                    {
                                        case true:
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                                            break;
                                    }

                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_HoldToRecord), ToastLength.Short);
                                    break;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }

                        BtnVoice.Pressed = false;
                        handled = true;
                        break;
                }

                e.Handled = handled;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void StartRecording()
        {
            try
            {
                switch (BtnVoice.Tag?.ToString())
                {
                    case "Free":
                    {
                        //Set Record Style
                        IsRecording = true;

                        switch (UserDetails.SoundControl)
                        {
                            case true:
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");
                                break;
                        }

                        if (TextRecorder != null && TextRecorder != "Recording")
                            TextRecorder = "Recording";

                        BtnVoice.SetColorFilter(Color.White);
                        BtnVoice.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                        AudioPlayerClass = new Methods.AudioRecorderAndPlayer("");

                        //Start Audio record
                        await Task.Delay(600);
                        AudioPlayerClass.StartRecording();
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
}