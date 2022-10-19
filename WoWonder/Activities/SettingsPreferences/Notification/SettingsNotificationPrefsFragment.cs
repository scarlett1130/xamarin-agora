using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using WoWonder.Activities.SettingsPreferences.Custom;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
 
namespace WoWonder.Activities.SettingsPreferences.Notification
{
    public class SettingsNotificationPrefsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region  Variables Basic

        private CustomCheckBoxPreference  LikedPref, CommentedPref, SharedPref, FollowedPref, LikedPagePref, VisitedPref, MentionedPref, JoinedGroupPref, AcceptedPref, ProfileWallPostPref, MemoryPref;
        private CustomSwitchPreference PlaySoundPref, PopupPref;
        private readonly Activity ActivityContext; 

        #endregion

        #region General

        public SettingsNotificationPrefsFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
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
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Notification);
                
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                InitComponent();
                LoadDataUser();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                PopupPref = (CustomSwitchPreference)FindPreference("notifications_key");
                PlaySoundPref = (CustomSwitchPreference)FindPreference("checkBox_PlaySound_key");

                LikedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_liked_key");
                CommentedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_commented_key");
                SharedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_shared_key");
                FollowedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_followed_key");
                LikedPagePref = (CustomCheckBoxPreference)FindPreference("checkBox_e_liked_page_key");
                VisitedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_visited_key");
                MentionedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_mentioned_key");
                JoinedGroupPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_joined_group_key");
                AcceptedPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_accepted_key");
                ProfileWallPostPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_profile_wall_post_key");
                MemoryPref = (CustomCheckBoxPreference)FindPreference("checkBox_e_memory_key");

                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "notifications_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_PlaySound_key");

                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_liked_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_commented_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_shared_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_followed_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_liked_page_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_visited_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_mentioned_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_joined_group_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_accepted_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_profile_wall_post_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_e_memory_key");

                PopupPref.IconSpaceReserved = false;

                LikedPref.IconSpaceReserved = false;
                CommentedPref.IconSpaceReserved = false;
                SharedPref.IconSpaceReserved = false;
                FollowedPref.IconSpaceReserved = false;
                LikedPagePref.IconSpaceReserved = false;
                VisitedPref.IconSpaceReserved = false;
                MentionedPref.IconSpaceReserved = false;
                JoinedGroupPref.IconSpaceReserved = false;
                AcceptedPref.IconSpaceReserved = false;
                ProfileWallPostPref.IconSpaceReserved = false;
                MemoryPref.IconSpaceReserved = false;
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        PopupPref.PreferenceChange += NotificationPopupPrefOnPreferenceChange;
                        PlaySoundPref.PreferenceChange += NotificationPlaySoundPrefOnPreferenceChange;
                        LikedPref.PreferenceChange += LikedPrefOnPreferenceChange;
                        CommentedPref.PreferenceChange += CommentedPrefOnPreferenceChange;
                        SharedPref.PreferenceChange += SharedPrefOnPreferenceChange;
                        FollowedPref.PreferenceChange += FollowedPrefOnPreferenceChange;
                        LikedPagePref.PreferenceChange += LikedPagePrefOnPreferenceChange;
                        VisitedPref.PreferenceChange += VisitedPrefOnPreferenceChange;
                        MentionedPref.PreferenceChange += MentionedPrefOnPreferenceChange;
                        JoinedGroupPref.PreferenceChange += JoinedGroupPrefOnPreferenceChange;
                        AcceptedPref.PreferenceChange += AcceptedPrefOnPreferenceChange;
                        ProfileWallPostPref.PreferenceChange += ProfileWallPostPrefOnPreferenceChange;
                        MemoryPref.PreferenceChange += MemoryPrefOnPreferenceChange;

                        break;
                    default:
                        PopupPref.PreferenceChange -= NotificationPopupPrefOnPreferenceChange;
                        PlaySoundPref.PreferenceChange -= NotificationPlaySoundPrefOnPreferenceChange;
                        LikedPref.PreferenceChange -= LikedPrefOnPreferenceChange;
                        CommentedPref.PreferenceChange -= CommentedPrefOnPreferenceChange;
                        SharedPref.PreferenceChange -= SharedPrefOnPreferenceChange;
                        FollowedPref.PreferenceChange -= FollowedPrefOnPreferenceChange;
                        LikedPagePref.PreferenceChange -= LikedPagePrefOnPreferenceChange;
                        VisitedPref.PreferenceChange -= VisitedPrefOnPreferenceChange;
                        MentionedPref.PreferenceChange -= MentionedPrefOnPreferenceChange;
                        JoinedGroupPref.PreferenceChange -= JoinedGroupPrefOnPreferenceChange;
                        AcceptedPref.PreferenceChange -= AcceptedPrefOnPreferenceChange;
                        ProfileWallPostPref.PreferenceChange -= ProfileWallPostPrefOnPreferenceChange;
                        MemoryPref.PreferenceChange -= MemoryPrefOnPreferenceChange;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void NotificationPlaySoundPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                switch (eventArgs.Handled)
                {
                    case true:
                    {
                        var etp = (CustomCheckBoxPreference)sender;
                        var value = eventArgs.NewValue.ToString();
                        etp.Checked = bool.Parse(value);
                        UserDetails.SoundControl = etp.Checked switch
                        {
                            true => true,
                            _ => false
                        };

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Notification >> Popup 
        private void NotificationPopupPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                switch (eventArgs.Handled)
                {
                    case true:
                    {
                        var etp = (CustomSwitchPreference)sender;
                        var value = eventArgs.NewValue.ToString();
                        etp.Checked = bool.Parse(value);
                        switch (etp.Checked)
                        {
                            case true:
                                UserDetails.NotificationPopup = true;
                                OneSignalNotification.Instance.RegisterNotificationDevice(Context);
                                break;
                            default:
                                UserDetails.NotificationPopup = false;
                                OneSignalNotification.Instance.UnRegisterNotificationDevice();
                                break;
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MemoryPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                { 
                    string memoryPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference) sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMemory = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMemory = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                memoryPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMemory = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMemory = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                memoryPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_memory", memoryPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProfileWallPostPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string profileWallPostPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EProfileWallPost = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EProfileWallPost = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                profileWallPostPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EProfileWallPost = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EProfileWallPost = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                profileWallPostPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_profile_wall_post", profileWallPostPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AcceptedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string acceptedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EAccepted = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EAccepted = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                acceptedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EAccepted = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EAccepted = "0"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                acceptedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_accepted", acceptedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void JoinedGroupPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string joinedGroupPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EJoinedGroup = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EJoinedGroup = "1"
                                        }
                                    };
                                }

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                joinedGroupPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EJoinedGroup = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EJoinedGroup = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                joinedGroupPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_joined_group", joinedGroupPref},

                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MentionedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string mentionedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMentioned = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMentioned = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                mentionedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EMentioned = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EMentioned = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                mentionedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_mentioned", mentionedPref},

                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VisitedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string visitedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EVisited = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EVisited = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                visitedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EVisited = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EVisited = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                visitedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_visited", visitedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikedPagePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string likedPagePref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELikedPage = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELikedPage = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);


                                likedPagePref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELikedPage = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELikedPage = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);


                                likedPagePref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_liked_page", likedPagePref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FollowedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string followedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EFollowed = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EFollowed = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                followedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EFollowed = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EFollowed = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                followedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_followed", followedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SharedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string sharedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EShared = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EShared = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                sharedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.EShared = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            EShared = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                sharedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_shared", sharedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string commentedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ECommented = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ECommented = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                commentedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ECommented = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ECommented = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                commentedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_commented", commentedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikedPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    string likedPref;
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (CustomCheckBoxPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    switch (etp.Checked)
                    {
                        case true:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELiked = "1";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELiked = "1"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                likedPref = "1";
                                break;
                            }
                        default:
                            {
                                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                                {
                                    dataUser.ApiNotificationSettings.NotificationSettingsClass.ELiked = "0";
                                }
                                else
                                {
                                    dataUser.ApiNotificationSettings = new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = new NotificationSettings
                                        {
                                            ELiked = "0"
                                        }
                                    };
                                }
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                likedPref = "0";
                                break;
                            }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataNotification = new Dictionary<string, string>
                        {
                            {"e_liked", likedPref},
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataNotification) });
                    }
                    else
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                switch (key)
                {
                    case "notifications_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("notifications_key", true) ?? true;
                        PopupPref.Checked = getvalue;
                        UserDetails.NotificationPopup = getvalue;
                        break;
                    }
                    case "checkBox_PlaySound_key":
                    {
                        bool getvalue = MainSettings.SharedData?.GetBoolean("checkBox_PlaySound_key", true) ?? true;
                        PlaySoundPref.Checked = getvalue;
                        UserDetails.SoundControl = getvalue;
                        break;
                    }
                    case "checkBox_e_liked_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_liked_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.ELiked == "1") ?? true;
                        LikedPref.Checked = getvalue;
                        break;
                    } 
                    case "checkBox_e_commented_key":
                    { 
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_commented_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.ECommented == "1") ?? true;
                        CommentedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_shared_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_shared_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EShared == "1") ?? true;
                        SharedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_followed_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_followed_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EFollowed == "1") ?? true;
                        FollowedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_liked_page_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_liked_page_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.ELikedPage == "1") ?? true;
                        LikedPagePref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_visited_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_visited_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EVisited == "1") ?? true;
                        VisitedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_mentioned_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_mentioned_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EMentioned == "1") ?? true;
                        MentionedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_joined_group_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_joined_group_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EJoinedGroup == "1") ?? true;
                        JoinedGroupPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_accepted_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_accepted_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EAccepted == "1") ?? true;
                        AcceptedPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_profile_wall_post_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_profile_wall_post_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EProfileWallPost == "1") ?? true;
                        ProfileWallPostPref.Checked = getvalue;
                        break;
                    }
                    case "checkBox_e_memory_key":
                    {
                        var getvalue = MainSettings.SharedData?.GetBoolean("checkBox_e_memory_key", dataUser?.ApiNotificationSettings.NotificationSettingsClass?.EMemory == "1") ?? true;
                        MemoryPref.Checked = getvalue;
                        break;
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void LoadDataUser()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.ApiNotificationSettings.NotificationSettingsClass != null)
                {
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_liked_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.ELiked == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_commented_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.ECommented == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_shared_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EShared == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_followed_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EFollowed == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_liked_page_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.ELikedPage == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_visited_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EVisited == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_mentioned_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EMentioned == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_joined_group_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EJoinedGroup == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_accepted_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EAccepted == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_profile_wall_post_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EProfileWallPost == "1")?.Commit();
                    MainSettings.SharedData?.Edit()?.PutBoolean("checkBox_e_memory_key", dataUser.ApiNotificationSettings.NotificationSettingsClass?.EMemory == "1")?.Commit();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
} 