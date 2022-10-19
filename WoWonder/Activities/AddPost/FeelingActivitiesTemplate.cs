using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.AddPost
{
    public class FeelingActivitiesTemplate : DialogFragment
    {
        private ImageView IvFeeling;
        private TextView TvFeeling;
        private EditText EtContent;
        private AppCompatButton BtnClose;
        private readonly IOnFeelingClick Listener;

        private readonly int Type;

        public interface IOnFeelingClick
        {
            void OnFeelingClick(string inputType);
        }

        public FeelingActivitiesTemplate(int feelingsType, IOnFeelingClick listener)
        {
            Type = feelingsType;
            Listener = listener;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.feeling_template, container, false);
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

                IvFeeling = view.FindViewById<ImageView>(Resource.Id.iv_feeling);
                TvFeeling = view.FindViewById<TextView>(Resource.Id.tv_feeling);
                EtContent = view.FindViewById<EditText>(Resource.Id.et_content);
                BtnClose = view.FindViewById<AppCompatButton>(Resource.Id.btn_close);

                switch (Type)
                {
                    case 0:
                        IvFeeling.SetImageResource(Resource.Drawable.ic_listening);
                        TvFeeling.Text = GetString(Resource.String.Lbl_Listening);
                        EtContent.Hint = GetString(Resource.String.Lbl_Comment_Hint_Listening);
                        break;
                    case 1:
                        IvFeeling.SetImageResource(Resource.Drawable.ic_playing);
                        TvFeeling.Text = GetString(Resource.String.Lbl_Playing);
                        EtContent.Hint = GetString(Resource.String.Lbl_Comment_Hint_Playing);
                        break;
                    case 2:
                        IvFeeling.SetImageResource(Resource.Drawable.ic_watching);
                        TvFeeling.Text = GetString(Resource.String.Lbl_Watching);
                        EtContent.Hint = GetString(Resource.String.Lbl_Comment_Hint_Watching);
                        break;
                    case 3:
                        IvFeeling.SetImageResource(Resource.Drawable.ic_travelling);
                        TvFeeling.Text = GetString(Resource.String.Lbl_Traveling);
                        EtContent.Hint = GetString(Resource.String.Lbl_Comment_Hint_Traveling);
                        break;
                }

                BtnClose.Click += BtnClose_Click;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            try
            {
                if ( EtContent.Text.Length > 0 )
                {
                    Listener.OnFeelingClick(EtContent.Text);
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}