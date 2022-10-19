using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Aghajari.Emojiview.View;
using Newtonsoft.Json;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Stories.DragView;
using WoWonder.StickersView;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Exception = System.Exception; 

namespace WoWonder.Activities.Story
{ 
    [Activity(Icon = "@mipmap/icon", Theme = "@style/DragTransparentBlack", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class StoryReplyActivity : AppCompatActivity, DragToClose.IDragListener 
    {
        #region Variables Basic

        private DragToClose DragToClose;
        private ImageView ChatEmojImage;
        private LinearLayout RootView;
        private AXEmojiEditText EmojIconEditTextView;
        private ImageView SendMessageButton;

        public string StoryId, UserId; // to_id  
        private StoryDataObject.Story DataStories;

        private static StoryReplyActivity Instance;
        private LinearLayout RepliedMessageView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.StoryReplyLayout);

                Instance = this;

                UserId = Intent?.GetStringExtra("recipientId") ?? "";
                StoryId = Intent?.GetStringExtra("StoryId") ?? "";
                DataStories = JsonConvert.DeserializeObject<StoryDataObject.Story>(Intent?.GetStringExtra("DataNowStory") ?? "");

                //Get Value And Set Toolbar
                InitComponent();
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

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            { 
                DragToClose = FindViewById<DragToClose>(Resource.Id.drag_to_close);
                DragToClose.SetCloseOnClick(true);
                DragToClose.SetDragListener(this);

                RootView = FindViewById<LinearLayout>(Resource.Id.reply_story);
                RepliedMessageView = FindViewById<LinearLayout>(Resource.Id.send_message_panel);
                ChatEmojImage = FindViewById<ImageView>(Resource.Id.sendEmojisIconButton);
                EmojIconEditTextView = FindViewById<AXEmojiEditText>(Resource.Id.MessageWrapper);
                SendMessageButton = FindViewById<ImageView>(Resource.Id.sendMessageButton);
                  
                if (AppSettings.SetTabDarkTheme)
                    EmojisViewTools.LoadDarkTheme();
                else
                    EmojisViewTools.LoadTheme(AppSettings.MainColor);

                EmojisViewTools.MStickerView = false;
                EmojisViewTools.LoadView(this, EmojIconEditTextView, "StoryReplyActivity", ChatEmojImage);

                EmojIconEditTextView.SetTextColor(Color.White);
                EmojIconEditTextView.SetHintTextColor(Color.ParseColor("#888888"));
                  
                EmojIconEditTextView.PerformClick();
                EmojIconEditTextView.RequestFocus();

                ChatEmojImage.SetColorFilter(Color.White);
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
                    SendMessageButton.Click += SendMessageButtonOnClick;
                }
                else
                {
                    SendMessageButton.Click -= SendMessageButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static StoryReplyActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void SendMessageButtonOnClick(object sender, EventArgs e)
        {
            OnClick_OfSendButton();
        }

        #endregion

        #region Events
          
        //Send Message type => "right_audio" Or "right_text"
        private void OnClick_OfSendButton()
        {
            try
            {
                if (string.IsNullOrEmpty(EmojIconEditTextView.Text))
                {

                }
                else
                {
                    //remove \n in a string
                    string replacement = Regex.Replace(EmojIconEditTextView.Text, @"\t|\n|\r", "");

                    if (Methods.CheckConnectivity())
                    {
                        Task.Factory.StartNew(() =>
                        {
                            SendMess(UserId, replacement).ConfigureAwait(false);
                        });
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }

                    EmojIconEditTextView.Text = "";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
          
        #region Drag

        public void OnStartDraggingView()
        {

        }

        public void OnDraggingView(float offset)
        {
            try
            {
                RepliedMessageView.Alpha = offset;
                RootView.Alpha = offset;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnViewClosed()
        {
            try
            {
                HideKeyboard();

                Intent resultIntent = new Intent();
                resultIntent.PutExtra("isReply", true);
                SetResult(Result.Ok, resultIntent);

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override void OnBackPressed()
        {
            try
            {
                HideKeyboard();

                Intent resultIntent = new Intent();
                resultIntent.PutExtra("isReply", true);
                SetResult(Result.Ok, resultIntent);

                base.OnBackPressed();
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task SendMess(string userId = "", string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    //Here on This function will send Selected audio file to the user 
                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(userId, time2, text, contact, pathFile, imageUrl, stickerId, gifUrl, "", lat, lng, StoryId);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            //MessageController.UpdateLastIdMessage(result);

                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    Intent resultIntent = new Intent();
                                    resultIntent.PutExtra("isReply", true);
                                    SetResult(Result.Ok, resultIntent);

                                    Finish();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}