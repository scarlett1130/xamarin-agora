using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using Com.Aghajari.Emojiview.Listener;
using Com.Aghajari.Emojiview.Search;
using Com.Aghajari.Emojiview.View;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Utils;

namespace WoWonder.StickersView
{
    public class EmojisViewActions : SimplePopupAdapter, View.IOnClickListener 
    {
        private readonly Activity ActivityContext;

        private readonly CommentActivity CommentActivity;
        private readonly StoryReplyActivity StoryReplyActivity;

        public readonly AXEmojiPopup Popup;
        private readonly AXEmojiEditText AxEmojiEditText;
        private readonly ImageView EmojisViewImage;

        private readonly string TypePage;

        private bool IsShowing = false;

        public EmojisViewActions(Activity activity, string typePage, AXEmojiPager emojiPager, AXEmojiEditText editText, ImageView image)
        {
            try
            {
                ActivityContext = activity;
                TypePage = typePage;

                switch (typePage)
                {
                    // Create your fragment here
                    case "CommentActivity":
                        CommentActivity = CommentActivity.GetInstance();
                        break;
                    case "StoryReplyActivity":
                        StoryReplyActivity = StoryReplyActivity.GetInstance();
                        break;
                }

                Popup = new AXEmojiPopup(emojiPager);
                AxEmojiEditText = editText;
                EmojisViewImage = image;

                EmojisViewImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));

                AxEmojiEditText.SetOnClickListener(this);
                EmojisViewImage.SetOnClickListener(this);
                Popup.SetPopupListener(this);
                Popup.SearchView = new AXEmojiSearchView(activity, emojiPager.GetPage(0));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateButton(bool emoji)
        {
            try
            {
                if (IsShowing == emoji) return;
                IsShowing = emoji;

                if (emoji)
                {
                    Drawable dr = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_keyboard);
                    DrawableCompat.SetTint(DrawableCompat.Wrap(dr), Color.Black);
                    EmojisViewImage.SetImageDrawable(dr);
                }
                else
                {
                    Drawable dr = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_smile_vector);
                    DrawableCompat.SetTint(DrawableCompat.Wrap(dr), Color.Black);
                    EmojisViewImage.SetImageDrawable(dr);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (v?.Id == AxEmojiEditText?.Id)
                {
                    if (Popup.IsShowing)
                    {
                        Popup.Toggle();

                        switch (TypePage)
                        {
                            // Create your fragment here
                            //case "ChatWindowActivity":
                            //    ChatWindow?.RemoveButtonFragment();
                            //    break;
                            //case "PageChatWindowActivity":
                            //    PageActivityView?.RemoveButtonFragment();
                            //    break;
                            //case "GroupChatWindowActivity":
                            //    GroupActivityView?.RemoveButtonFragment();
                            //    break;
                            case "StoryReplyActivity":
                                //StoryReplyActivity?.RemoveButtonFragment();
                                break;
                        }
                    }
                }
                else if (v?.Id == EmojisViewImage?.Id)
                {
                    Popup.Toggle();

                    switch (TypePage)
                    {
                        // Create your fragment here
                        //case "ChatWindowActivity":
                        //    ChatWindow?.RemoveButtonFragment();
                        //    break;
                        //case "PageChatWindowActivity":
                        //    PageActivityView?.RemoveButtonFragment();
                        //    break;
                        //case "GroupChatWindowActivity":
                        //    GroupActivityView?.RemoveButtonFragment();
                        //    break;
                        case "StoryReplyActivity":
                            //StoryReplyActivity?.RemoveButtonFragment();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnShow()
        {
            try
            {
                base.OnShow();
                UpdateButton(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDismiss()
        {
            try
            {
                base.OnDismiss();
                UpdateButton(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnKeyboardOpened(int height)
        {
            try
            {
                base.OnKeyboardOpened(height);
                UpdateButton(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnKeyboardClosed()
        {
            try
            {
                base.OnKeyboardClosed();
                UpdateButton(Popup.IsShowing);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}