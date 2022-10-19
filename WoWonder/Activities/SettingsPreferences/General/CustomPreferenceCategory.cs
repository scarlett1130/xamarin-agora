using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.SettingsPreferences.General
{
    public class CustomPreferenceCategory : PreferenceCategory
    {
        public CustomPreferenceCategory(Context context) : base(context)
        {
            try
            {
                //ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomPreferenceCategory(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                //ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CustomPreferenceCategory(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            try
            {
                //ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBindViewHolder(PreferenceViewHolder holder)
        {
            try
            {
                base.OnBindViewHolder(holder);

                var title = holder.ItemView.FindViewById<TextView>(Resource.Id.title);
                title.Text = Title;
                title.SetTextColor(Color.White);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

}