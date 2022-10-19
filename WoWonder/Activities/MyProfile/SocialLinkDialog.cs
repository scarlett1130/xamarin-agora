using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.MyProfile.Adapters;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.MyProfile
{
    public class SocialLinkDialog : DialogFragment
    {
        private readonly SocialItem SItem;
        private readonly IOnSocialClick Listener;
        private TextView SocialIcon, SocialName;
        private EditText SocialLink;
        private AppCompatButton BtnAdd;

        public interface IOnSocialClick
        {
            void OnSocialClick(string inputType);
        }

        public SocialLinkDialog(SocialItem item, IOnSocialClick listner)
        {
            SItem = item;
            Listener = listner;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.SocialLink_Dialog, container, false);
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

                SocialIcon = view.FindViewById<TextView>(Resource.Id.IconSocial);
                SocialName = view.FindViewById<TextView>(Resource.Id.TxtSocialName);
                SocialLink = view.FindViewById<EditText>(Resource.Id.et_content);
                BtnAdd = view.FindViewById<AppCompatButton>(Resource.Id.btn_close);

                FontUtils.SetTextViewIcon(SItem.Id == 4 ? FontsIconFrameWork.FontAwesomeBrands : FontsIconFrameWork.IonIcons, SocialIcon, SItem.SocialIcon);
                SocialIcon.SetTextColor(SItem.IconColor);

                SocialName.Text = SItem.SocialName;
                SocialLink.Text = SItem.SocialLinkName;

                BtnAdd.Click += BtnClose_Click;
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
                if (SocialLink.Text.Length > 0)
                {
                    Listener?.OnSocialClick(SocialLink.Text);
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