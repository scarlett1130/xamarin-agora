using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using AndroidX.Core.Content;
using WoWonder.Library.Anjo.SuperTextLibrary; 
using WoWonder.Activities.AddPost;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Controller; 
using WoWonder.SQLite;

namespace WoWonder.Helpers.Utils
{
    public class TextSanitizer : Java.Lang.Object, StTools.IXAutoLinkOnClickListener, MaterialDialog.ISingleButtonCallback
    {
        private readonly SuperTextView SuperTextView;
        private readonly Activity Activity;
        private readonly string TypePage;

        public TextSanitizer(SuperTextView linkTextView, Activity activity, string typePage = "normal")
        {
            try
            {
                SuperTextView = linkTextView;
                Activity = activity;
                TypePage = typePage;
                SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>()); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Load(string text)
        {
            try
            {
                SuperTextView.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention, StTools.XAutoLinkMode.ModeCustom });
                SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModePhone_color)));
                SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeEmail_color)));
                SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeHashtag_color)));
                SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeMention_color)));
                SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));

                var textt = text.Split('/');
                if (textt.Length > 1)
                {
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                    SuperTextView.SetCustomRegex(@"\b(" + textt.LastOrDefault() + @")\b");
                }

                string laststring = text.Replace(" /", " ");
                if (!string.IsNullOrEmpty(laststring))
                    SuperTextView.SetText(laststring, TextView.BufferType.Spannable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
                if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
                }
                else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    string url = matchedText.Replace(" ", "").Replace("\n", "");
                    if (!matchedText.Contains("http"))
                    {
                        url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
                    }

                    //var intent = new Intent(Activity, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //Activity.StartActivity(intent);
                    new IntentController(Activity).OpenBrowserFromApp(url);
                }
                else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {
                    var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                    intent.PutExtra("Id", matchedText.Replace(" ", ""));
                    intent.PutExtra("Tag", matchedText.Replace(" ", ""));
                    Activity.StartActivity(intent);
                }
                else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = matchedText.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);
                     
                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(Activity, user.UserId, user);
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            switch (PostClickListener.OpenMyProfile)
                            {
                                case true:
                                    return;
                                default:
                                {
                                    var intent = new Intent(Activity, typeof(MyProfileActivity));
                                    Activity.StartActivity(intent);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var intent = new Intent(Activity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            Activity.StartActivity(intent);
                        }
                    }
                }
                else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
                }
                else if (autoLinkMode == StTools.XAutoLinkMode.ModeCustom && TypePage == "AddPost")
                {
                    var dialog = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    dialog.Title(Activity.GetText(Resource.String.Lbl_Location)).TitleColorRes(Resource.Color.primary);
                    dialog.PositiveText(Activity.GetText(Resource.String.Lbl_RemoveLocation)).OnPositive(this);
                    dialog.NeutralText(Activity.GetText(Resource.String.Lbl_ChangeLocation)).OnNeutral(this);
                    dialog.NegativeText(Activity.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    //dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    ((AddPostActivity)Activity)?.RemoveLocation();
                    ((PostSharingActivity)Activity)?.RemoveLocation();
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
                else if (p1 == DialogAction.Neutral)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(Activity).OpenIntentLocation();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}