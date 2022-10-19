using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.AddPost
{
    public class PostDoingDialog : DialogFragment, View.IOnClickListener
    {
        private LinearLayout LlFeeling, LlListening, LlPlaying, LlWatching, LlTraveling;
        private AppCompatButton BtnClose;
        private readonly IOnDoingListener Listener;

        public interface IOnDoingListener
        {
            void OnDoingClick(string type);
        }

        public PostDoingDialog(IOnDoingListener listener)
        {
            Listener = listener;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.addpost_doing, container, false);
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

                LlFeeling = view.FindViewById<LinearLayout>(Resource.Id.llFeeling);
                LlListening = view.FindViewById<LinearLayout>(Resource.Id.llListening);
                LlPlaying = view.FindViewById<LinearLayout>(Resource.Id.llPlaying);
                LlWatching = view.FindViewById<LinearLayout>(Resource.Id.llWatching);
                LlTraveling = view.FindViewById<LinearLayout>(Resource.Id.llTraveling);
                BtnClose = view.FindViewById<AppCompatButton>(Resource.Id.btn_close);
                 
                if (!AppSettings.ShowFeeling)
                    LlFeeling.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowListening)
                    LlListening.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowPlaying)
                    LlPlaying.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowWatching)
                    LlWatching.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowTraveling)
                    LlTraveling.Visibility = ViewStates.Gone;
                 
                LlFeeling.SetOnClickListener(this);
                LlListening.SetOnClickListener(this);
                LlPlaying.SetOnClickListener(this);
                LlWatching.SetOnClickListener(this);
                LlTraveling.SetOnClickListener(this);

                BtnClose.Click += BtnClose_Click;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

        private void BtnClose_Click(object sender, EventArgs e)
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

        public void OnClick(View v)
        {
            try
            {
                switch (v.Id)
                {
                    case Resource.Id.llFeeling:
                        Listener.OnDoingClick("Feeling");
                        break;
                    case Resource.Id.llListening:
                        Listener.OnDoingClick("Listening");
                        break;
                    case Resource.Id.llPlaying:
                        Listener.OnDoingClick("Playing");
                        break;
                    case Resource.Id.llWatching:
                        Listener.OnDoingClick("Watching");
                        break;
                    case Resource.Id.llTraveling:
                        Listener.OnDoingClick("Traveling");
                        break;
                }
                Dismiss();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}