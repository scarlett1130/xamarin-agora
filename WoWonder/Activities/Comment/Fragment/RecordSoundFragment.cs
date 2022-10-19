using System;
using System.Timers; 
using Android.Media;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using AT.Markushi.UI;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Comment.Fragment
{
    public class RecordSoundFragment : AndroidX.Fragment.App.Fragment
    { 
        private CircleButton RecordPlayButton, RecordCloseButton;
        private SeekBar VoiceSeekBar;
        private string RecordFilePath;
        private Methods.AudioRecorderAndPlayer AudioPlayerClass;
        private CommentActivity MainActivityView;
        private Timer TimerSound;
        private MediaPlayer MediaPlayer;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.RecourdSoundFragment, container, false); 
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
                RecordFilePath = Arguments.GetString("FilePath");

                MainActivityView = (CommentActivity)Activity;
                MainActivityView.BtnVoice.SetImageResource(Resource.Drawable.microphone);
                MainActivityView.BtnVoice.Tag = "Audio";

                RecordPlayButton = view.FindViewById<CircleButton>(Resource.Id.playButton);
                RecordPlayButton.Click += RecordPlayButton_Click;
              
                RecordCloseButton = view.FindViewById<CircleButton>(Resource.Id.closeRecordButton);
                RecordCloseButton.Click += RecordCloseButtonClick;

                VoiceSeekBar = view.FindViewById<SeekBar>(Resource.Id.voiceseekbar);

                VoiceSeekBar.Max = 10000;
                VoiceSeekBar.Progress = 0;

                AudioPlayerClass = new Methods.AudioRecorderAndPlayer(MainActivityView.PostId);
                TimerSound = new Timer();
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

                var fragmentHolder = Activity.FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);

                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);
                var interpolator = new FastOutSlowInInterpolator();
                fragmentHolder.Animate().SetInterpolator(interpolator).TranslationY(1200).SetDuration(300);
                Activity.SupportFragmentManager.BeginTransaction().Remove(this)?.Commit();

                MainActivityView.BtnVoice.SetImageResource(Resource.Drawable.microphone);
                MainActivityView.BtnVoice.Tag = "Free";
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
                                MediaPlayer.SetDataSource(Context, Uri.Parse(RecordFilePath));
                                MediaPlayer.PrepareAsync();
                            }
                            else
                            {
                                Java.IO.File file2 = new Java.IO.File(RecordFilePath);
                                var photoUri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", file2);

                                MediaPlayer.SetDataSource(Context, photoUri);
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

    }
}