using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Fonts;
using AndroidX.RecyclerView.Widget;
using Q.Rorbin.Badgeview;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Tabbes.Adapters
{
    public class SectionItem
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string Icon { get; set; }
        public Color BackIcon { get; set; }

        public int IconAsImage { get; set; }
        public StyleRowMore StyleRow { get; set; }
        public Color IconColor { get; set; }
        public int BadgeCount { get; set; }
        public bool Badgevisibilty { get; set; }
    }

    public class MoreSectionAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<SectionItem> SectionList = new ObservableCollection<SectionItem>();
        private readonly Activity ActivityContext;

        public MoreSectionAdapter(Activity activityContext , StyleRowMore styleRowMore)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activityContext;
                 
                switch (styleRowMore)
                {
                    case StyleRowMore.Card:
                    case StyleRowMore.Grid:
                        SectionList.Add(new SectionItem
                        {
                            Id = 1,
                            SectionName = activityContext.GetText(Resource.String.Lbl_MyProfile),
                            BadgeCount = 0,
                            Badgevisibilty = false,
                            StyleRow = styleRowMore,
                            IconAsImage = Resource.Drawable.icon_more_my_profile,
                            Icon = IonIconsFonts.Happy,
                            BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.White,
                            IconColor = Color.ParseColor("#047cac")
                        });
                        switch (AppSettings.ShowPatriotTV)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 19,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_PatriotTV),
                                    BadgeCount = 0,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#fffefa"),
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_movies,
                                    Icon = IonIconsFonts.IosBriefcase,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }
                        switch (AppSettings.MessengerIntegration)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 2,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Messages),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_messages,
                                    Icon = IonIconsFonts.Chatbubbles,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#f8f8ff"),
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowUserContacts)
                        {
                            case true:
                            {
                                string name = activityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends);
                                SectionList.Add(new SectionItem
                                {
                                    Id = 3,
                                    SectionName = name,
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_following,
                                    Icon = IonIconsFonts.People,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#fbf1ff"),
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                            }
                        }

                        switch (AppSettings.ShowBrowserUser)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 27,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Browser_Users),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_find_friends,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#f9fffa"),
                                    Icon = IonIconsFonts.Aperture,
                                    IconColor = Color.ParseColor("#009688")
                                });
                                break;
                        }

                        /*switch (AppSettings.ShowPokes)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 4,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Pokes),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_pokes,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#f9fffa"),
                                    Icon = IonIconsFonts.Aperture,
                                    IconColor = Color.ParseColor("#009688")
                                });
                                break;
                        }*/

                        switch (AppSettings.ShowAlbum)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 5,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Albums),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#f5fffd"),
                                    Icon = IonIconsFonts.Images,
                                    IconAsImage = Resource.Drawable.ic_more_albums,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowMyPhoto)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 6,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_MyImages),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_my_images,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#fff9f9"),
                                    Icon = IonIconsFonts.Camera,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowMyVideo)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 7,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_MyVideos),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_my_videos,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#fff9f5"),
                                    Icon = IonIconsFonts.Film,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSavedPost)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 8,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Saved_Posts),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_saved_posts,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#fffef8"),
                                    Icon = IonIconsFonts.Bookmark,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowCommunitiesGroups)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 9,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Groups),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    IconAsImage = Resource.Drawable.ic_more_groups,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#fffef8"),
                                    Badgevisibilty = false,
                                    Icon = IonIconsFonts.Apps,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowCommunitiesPages)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 10,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Pages),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#fff9f5"),
                                    IconAsImage = Resource.Drawable.ic_more_pages,
                                    Icon = IonIconsFonts.Flag,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowArticles)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 11,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Blogs),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#fff7f6"),
                                    IconAsImage = Resource.Drawable.ic_more_blogs,
                                    Icon = IonIconsFonts.IosBook,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowMarket)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 12,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Marketplace),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#f8f5ff"),
                                    IconAsImage = Resource.Drawable.ic_more_market_place,
                                    Icon = IonIconsFonts.IosBriefcase,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        if (AppSettings.ShowBoostedPosts || AppSettings.ShowBoostedPages)
                            SectionList.Add(new SectionItem
                            {
                                Id = 13,
                                SectionName = activityContext.GetText(Resource.String.Lbl_Boosted),
                                BadgeCount = 0,
                                StyleRow = styleRowMore,
                                Badgevisibilty = false,
                                BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#f2f3f7"),
                                IconAsImage = Resource.Drawable.ic_more_rocket_vector,
                                Icon = IonIconsFonts.Rocket,
                                IconColor = Color.ParseColor("#03a9f4")
                            });

                        switch (AppSettings.ShowPopularPosts)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 14,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Popular_Posts),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#fefaff"),
                                    IconAsImage = Resource.Drawable.ic_more_popular_posts,
                                    Icon = IonIconsFonts.Clipboard,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowEvents)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 15,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Events),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#f6fff6"),
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_events,
                                    Icon = IonIconsFonts.Calendar,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        /*switch (AppSettings.ShowNearBy)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 16,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_FindFriends),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#f8fffe"),
                                    IconAsImage = Resource.Drawable.ic_more_find_friends,
                                    Icon = IonIconsFonts.Pin,
                                    IconColor = Color.ParseColor("#b2c17c")
                                });
                                break;
                        }

                        switch (AppSettings.ShowOffers)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 17,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Offers),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#fff8f8"),
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_offers,
                                    Icon = IonIconsFonts.Pricetag,
                                    IconColor = Color.ParseColor("#673AB7")
                                });
                                break;
                        }

                        switch (AppSettings.ShowMovies)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 18,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Movies),
                                    BadgeCount = 0,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#fffbf8"),
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_movies,
                                    Icon = IonIconsFonts.Film,
                                    IconColor = Color.ParseColor("#8d73cc")
                                });
                                break;
                        }*/

                        /*switch (AppSettings.ShowCommonThings)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 20,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_common_things),
                                    BadgeCount = 0,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#f8fcff"),
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_common_things,
                                    Icon = IonIconsFonts.CheckmarkCircle,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }*/

                        switch (AppSettings.ShowMemories)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 21,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Memories),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#212121") : Color.ParseColor("#fff7fd"),
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_memories,
                                    Icon = IonIconsFonts.Timer,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        switch (AppSettings.ShowFundings)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 22,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Fundraiser),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#262626") : Color.ParseColor("#fff4f2"),
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_funding,
                                    Icon = IonIconsFonts.LogoUsd,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }

                        /*switch (AppSettings.ShowGames)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 23,
                                    BackIcon = AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#faf7ff"),
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Games),
                                    BadgeCount = 0,
                                    StyleRow = styleRowMore,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_more_games,
                                    Icon = IonIconsFonts.LogoGameControllerB,
                                    IconColor = Color.ParseColor("#03a9f4")
                                });
                                break;
                        }*/

                        break;
                    case StyleRowMore.Row:
                        switch (AppSettings.ShowSettingsGeneralAccount)
                        {
                            //Settings Page
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 100,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_GeneralAccount),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_general_account,
                                    Icon = IonIconsFonts.Settings,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsPrivacy)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 101,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Privacy),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_privacy,
                                    Icon = IonIconsFonts.Eye,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsNotification)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 102,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Notifications),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_notifications,
                                    Icon = IonIconsFonts.Notifications,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsInvitationLinks)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 103,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_InvitationLinks),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_invitation_links,
                                    Icon = IonIconsFonts.Link,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsMyInformation)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 104,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_MyInformation),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_info,
                                    Icon = IonIconsFonts.IosPaper,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsInviteFriends)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 105,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Earnings),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settings_earnings,
                                    Icon = IonIconsFonts.IosHome,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        switch (AppSettings.ShowSettingsHelpSupport)
                        {
                            case true:
                                SectionList.Add(new SectionItem
                                {
                                    Id = 106,
                                    SectionName = activityContext.GetText(Resource.String.Lbl_Help_Support),
                                    BadgeCount = 0,
                                    StyleRow = StyleRowMore.Row,
                                    Badgevisibilty = false,
                                    IconAsImage = Resource.Drawable.ic_settingss_help_support,
                                    Icon = IonIconsFonts.Help,
                                    IconColor = Color.ParseColor("#757575")
                                });
                                break;
                        }

                        SectionList.Add(new SectionItem
                        {
                            Id = 107,
                            SectionName = activityContext.GetText(Resource.String.Lbl_Logout),
                            BadgeCount = 0,
                            StyleRow = StyleRowMore.Row,
                            Badgevisibilty = false,
                            IconAsImage = Resource.Drawable.ic_settings_logout,
                            Icon = IonIconsFonts.LogOut,
                            IconColor = Color.ParseColor("#d50000")
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override int ItemCount => SectionList?.Count ?? 0;
 
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemClick;
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemLongClick;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView; 
                switch (viewType)
                {
                    case (int)StyleRowMore.Card:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSectionCard_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolder(itemView, Click); 
                        return vh;
                    }
                    case (int)StyleRowMore.Grid:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSectionGrid_view, parent, false);
                        var vh = new MoreSectionGridAdapterViewHolder(itemView, Click); 
                        return vh;
                    }
                    case (int)StyleRowMore.Row:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolder(itemView, Click);
                        return vh;
                    } 
                    default:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolder(itemView, Click);
                        return vh;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = SectionList[position];
                    switch (payloads[0].ToString())
                    { 
                        case "WithoutBlobBadge":
                            {
                                if (item.StyleRow == StyleRowMore.Card)
                                {
                                    if (viewHolder is MoreSectionAdapterViewHolder holder)
                                    {
                                        if (item.BadgeCount != 0 && item.Id == 2 && item.Badgevisibilty)
                                        {
                                            holder.Badge.Text = item.BadgeCount + " " + ActivityContext.GetText(Resource.String.Lbl_NewMessages);
                                            holder.Badge.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                            holder.Badge.Visibility = ViewStates.Visible;
                                        }
                                        else if (item.Id == 2)
                                        {
                                            holder.Badge.Visibility = ViewStates.Invisible;
                                        }
                                    }
                                }
                                else if (item?.StyleRow == StyleRowMore.Grid)
                                {
                                    if (viewHolder is MoreSectionGridAdapterViewHolder holder)
                                    {
                                        if (item.BadgeCount != 0 && item.Id == 2 && item.Badgevisibilty)
                                        {
                                            ShowOrHideBadgeViewIcon(holder, item.BadgeCount, true);
                                            holder.Name.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                        }
                                        else if (item.Id == 2)
                                        {
                                            ShowOrHideBadgeViewIcon(holder);
                                        }
                                    }
                                }
                                    
                                //NotifyItemChanged(position);
                                break;

                            }
                        default:
                            base.OnBindViewHolder(viewHolder, position, payloads);
                            break;
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = SectionList[position];
                if (item?.StyleRow == StyleRowMore.Card || item?.StyleRow == StyleRowMore.Row)
                {
                    if (viewHolder is MoreSectionAdapterViewHolder holder)
                    {
                        holder.Name.Text = item.SectionName;

                        if (item.StyleRow == StyleRowMore.Card)
                        {
                            holder.LinearLayoutMain.BackgroundTintList = ColorStateList.ValueOf(item.BackIcon);

                            if (item.Id == 1)
                            {
                                var myProfile = ListUtils.MyProfileList?.FirstOrDefault();
                                GlideImageLoader.LoadImage(ActivityContext, myProfile != null ? myProfile.Avatar : UserDetails.Avatar, holder.Icon, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                holder.Badge.Text = ActivityContext.GetText(Resource.String.Lbl_SeeYourProfile);
                                holder.Badge.Visibility = ViewStates.Visible;

                                if (myProfile != null)
                                {
                                    string text = "";
                                    if (AppSettings.ShowUserPoint)
                                        text += Methods.FunString.FormatPriceValue(Convert.ToInt32(myProfile.Points)) + " " + ActivityContext.GetText(Resource.String.Btn_Points) + "\n";

                                    text += Methods.FunString.FormatPriceValue(Convert.ToInt32(myProfile.Details.DetailsClass.PostCount)) + " " + ActivityContext.GetText(Resource.String.Lbl_Posts);

                                    holder.TextCount.Text = text;
                                    holder.TextCount.Visibility = ViewStates.Visible;
                                } 
                            }

                            if (item.BadgeCount != 0 && item.Id == 2 && item.Badgevisibilty)
                            {
                                holder.Badge.Text = item.BadgeCount + " " + ActivityContext.GetText(Resource.String.Lbl_NewMessages);
                                holder.Badge.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.Badge.Visibility = ViewStates.Visible;
                            }
                            else if (item.Id == 2)
                            {
                                holder.Badge.Visibility = ViewStates.Invisible;
                            }
                        }

                        if (item.IconAsImage != 0 && item.Id != 1)
                            holder.Icon.SetImageResource(item.IconAsImage);
                    } 
                }
                else if (item?.StyleRow == StyleRowMore.Grid)
                {
                    if (viewHolder is MoreSectionGridAdapterViewHolder holder)
                    {
                        switch (AppSettings.FlowDirectionRightToLeft)
                        {
                            case true:
                                holder.LinearLayoutImage.LayoutDirection = LayoutDirection.Rtl;
                                holder.LinearLayoutMain.LayoutDirection = LayoutDirection.Rtl;
                                holder.Name.LayoutDirection = LayoutDirection.Rtl;
                                break;
                        }
                         
                        holder.Name.Text = item.SectionName;
                        holder.BackIcon.BackgroundTintList = ColorStateList.ValueOf(item.BackIcon);  

                        if (item.IconAsImage != 0)
                            holder.Icon.SetImageResource(item.IconAsImage);

                        if (item.BadgeCount != 0 && item.Id == 2 && item.Badgevisibilty)
                        {
                            ShowOrHideBadgeViewIcon(holder, item.BadgeCount, true);
                            holder.Name.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        }
                        else if (item.Id == 2)
                        {
                            ShowOrHideBadgeViewIcon(holder);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowOrHideBadgeViewIcon(MoreSectionGridAdapterViewHolder viewHolderTheme2, int countMessages = 0, bool show = false)
        {
            try
            {
                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        switch (show)
                        {
                            case true:
                            {
                                if (viewHolderTheme2?.LinearLayoutImage != null)
                                {
                                    int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                    QBadgeView badge = new QBadgeView(ActivityContext);
                                    badge.BindTarget(viewHolderTheme2.LinearLayoutImage);
                                    badge.SetBadgeNumber(countMessages);
                                    badge.SetBadgeGravity(gravity);
                                    badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                    badge.SetGravityOffset(10, true);
                                }

                                break;
                            }
                            default:
                                new QBadgeView(ActivityContext).BindTarget(viewHolderTheme2?.LinearLayoutImage).Hide(true);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public SectionItem GetItem(int position)
        {
            return SectionList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return SectionList[position].StyleRow switch
                {
                    StyleRowMore.Row => (int)StyleRowMore.Row,
                    StyleRowMore.Card => (int)StyleRowMore.Card,
                    StyleRowMore.Grid => (int)StyleRowMore.Grid,
                    _ => (int)StyleRowMore.Row
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)StyleRowMore.Row;
            }
        }

        private void Click(MoreSectionAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MoreSectionAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class MoreSectionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoreSectionAdapterViewHolder(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = MainView.FindViewById<LinearLayout>(Resource.Id.main);

                Name = MainView.FindViewById<TextView>(Resource.Id.section_name);
                Icon = MainView.FindViewById<ImageView>(Resource.Id.Icon);

                IconArrow = MainView.FindViewById<TextView>(Resource.Id.icon_arrow);
                if (IconArrow != null)
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconArrow, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowBack : IonIconsFonts.IosArrowForward);
                
                Badge = MainView.FindViewById<TextView>(Resource.Id.badge);
                if (Badge != null) Badge.Visibility = ViewStates.Invisible;

                TextCount = MainView.FindViewById<TextView>(Resource.Id.textCount);
                if (TextCount != null) TextCount.Visibility = ViewStates.Gone;

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public ImageView Icon { get; private set; }
        public TextView Name { get; private set; }
        public TextView Badge { get; private set; }
        public TextView IconArrow { get; private set; }
        public TextView TextCount { get; private set; }
        
    }
     
    public class MoreSectionGridAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoreSectionGridAdapterViewHolder(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = MainView.FindViewById<LinearLayout>(Resource.Id.main);
                LinearLayoutImage = MainView.FindViewById<RelativeLayout>(Resource.Id.imagecontainer);

                BackIcon = MainView.FindViewById<LinearLayout>(Resource.Id.backIcon);
                Name = MainView.FindViewById<TextView>(Resource.Id.section_name);
                Icon = MainView.FindViewById<ImageView>(Resource.Id.Icon);

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public RelativeLayout LinearLayoutImage { get; private set; }
        public LinearLayout BackIcon { get; private set; }
        public TextView Name { get; private set; }
        public ImageView Icon { get; private set; }

    }
     
    public class MoreSectionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}