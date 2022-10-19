using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Utils;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace WoWonder.Activities.Comment.Fragment
{
    public class OptionCommentDialog : DialogFragment
    {
        #region Variables Basic

        private readonly CommentActivity CommentActivity;
        private LinearLayout LlGallery, LlGif;
        private AppCompatButton BtnClose;

        #endregion

        #region General

        public OptionCommentDialog(CommentActivity activity)
        {
            try
            {
                CommentActivity = activity;
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

                View view = localInflater?.Inflate(Resource.Layout.OptionCommentDialogLayout, container, false);
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

                LlGallery = view.FindViewById<LinearLayout>(Resource.Id.llGallery);
                LlGallery.Click += LlGalleryOnClick;

                LlGif = view.FindViewById<LinearLayout>(Resource.Id.llGif);
                LlGif.Click += LlGifOnClick;

                BtnClose = view.FindViewById<AppCompatButton>(Resource.Id.btn_close);
                  
                BtnClose.Click += BtnCloseOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        #endregion

        #region Event
         
        private void LlGifOnClick(object sender, EventArgs e)
        {
            try
            {
                CommentActivity?.OpenGifActivity();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LlGalleryOnClick(object sender, EventArgs e)
        {
            try
            {
                CommentActivity?.OpenDialogGallery();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void BtnCloseOnClick(object sender, EventArgs e)
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

    }
}