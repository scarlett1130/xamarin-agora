using System;
using System.Linq;
using MaterialDialogsCore;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Activities.Album;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Boosted;
using WoWonder.Activities.CommonThings;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.Games;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Market;
using WoWonder.Activities.Memories;
using WoWonder.Activities.Movies;
using WoWonder.Activities.MyPhoto;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.MyVideo;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.Offers;
using WoWonder.Activities.Pokes;
using WoWonder.Activities.PopularPosts;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.InvitationLinks;
using WoWonder.Activities.SettingsPreferences.MyInformation;
using WoWonder.Activities.SettingsPreferences.Notification;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.Support;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using WoWonder.Activities.Search;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class MoreFragment : AndroidX.Fragment.App.Fragment,  MaterialDialog.ISingleButtonCallback 
    {
        #region  Variables Basic

        public MoreSectionAdapter MoreSectionAdapter1,MoreSectionAdapter2;
        private RecyclerView MRecycler1,MRecycler2;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.Tab_More_Layout, container, false); 
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                SetRecyclerViewAdapters(view); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                MRecycler1 = (RecyclerView)view.FindViewById(Resource.Id.recyler1);
                MRecycler1.NestedScrollingEnabled = true;

                if (AppSettings.MoreTheme == MoreTheme.Grid)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Grid);

                    var layoutManager = new GridLayoutManager(Activity, 3);
                    var countListFirstRow = MoreSectionAdapter1.SectionList.Where(q => q.StyleRow == StyleRowMore.Grid).ToList().Count;

                    layoutManager.SetSpanSizeLookup(new MySpanSizeLookup2(countListFirstRow, 1, 3));//20, 1, 4
                    MRecycler1.SetLayoutManager(layoutManager);
                }
                else if (AppSettings.MoreTheme == MoreTheme.Card)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Card);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.20f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    }
                    else
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.12f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    } 
                }

                MoreSectionAdapter1.ItemClick += MoreSectionAdapter1OnItemClick;
                MRecycler1.SetAdapter(MoreSectionAdapter1);
                MRecycler1.HasFixedSize = true;
                MRecycler1.SetItemViewCacheSize(50);
                MRecycler1.GetLayoutManager().ItemPrefetchEnabled = true;
                 
                //=================================

                MRecycler2 = (RecyclerView)view.FindViewById(Resource.Id.recyler2);
                MRecycler2.NestedScrollingEnabled = true;

                MoreSectionAdapter2 = new MoreSectionAdapter(Activity , StyleRowMore.Row);
                MoreSectionAdapter2.ItemClick += MoreSectionAdapter2OnItemClick;
                 
                MRecycler2.SetLayoutManager(new LinearLayoutManager(Activity));
                MRecycler2.SetAdapter(MoreSectionAdapter2);
                MRecycler2.HasFixedSize = true;
                MRecycler2.SetItemViewCacheSize(50);
                MRecycler2.GetLayoutManager().ItemPrefetchEnabled = true; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private class MySpannedGridLayoutManager : SpannedGridLayoutManager.IGridSpanLookup
        {
            private bool Big1 = true;
            private bool Big2 = true;

            public SpannedGridLayoutManager.SpanInfo GetSpanInfo(int position)
            {
                try
                {
                    // Conditions for 2x2 items 
                    if (position == 0)
                    {
                        return new SpannedGridLayoutManager.SpanInfo(3, 1);
                    }

                    if (position % 2 == 0)
                    {
                        if (Big1)
                        {
                            Big1 = false;
                            return new SpannedGridLayoutManager.SpanInfo(2, 1);
                        }
                        else
                        {
                            Big1 = true;
                            return new SpannedGridLayoutManager.SpanInfo(1, 1);
                        }
                    }
                    else
                    {
                        if (Big2)
                        {
                            Big2 = false;
                            return new SpannedGridLayoutManager.SpanInfo(1, 1);
                        }
                        else
                        {
                            Big2 = true;
                            return new SpannedGridLayoutManager.SpanInfo(2, 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return new SpannedGridLayoutManager.SpanInfo(2, 1);
                }
            }
        }

        #endregion

        #region Event
         
        private void MoreSectionAdapter1OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter1?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    {
                        // My Profile
                        case 1:
                            {
                                var intent = new Intent(Context, typeof(MyProfileActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Messages
                        case 2:
                        {
                            Methods.App.OpenAppByPackageName(Context, AppSettings.MessengerPackageName, "OpenChatApp");
                            break;
                        }
                        // Contacts
                        case 3:
                            {
                                var intent = new Intent(Context, typeof(MyContactsActivity));
                                intent.PutExtra("ContactsType", "Following");
                                intent.PutExtra("UserId", UserDetails.UserId);
                                StartActivity(intent);
                                break;
                            }
                        // Pokes
                        case 27:
                            {
                                var intent = new Intent(Context, typeof(BrowserUserActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Album
                        case 5:
                            {
                                var intent = new Intent(Context, typeof(MyAlbumActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyImages
                        case 6:
                            {
                                var intent = new Intent(Context, typeof(MyPhotosActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyVideos
                        case 7:
                            {
                                var intent = new Intent(Context, typeof(MyVideoActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Saved Posts
                        case 8:
                            {
                                var intent = new Intent(Context, typeof(SavedPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Groups
                        case 9:
                            {
                                var intent = new Intent(Context, typeof(GroupsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Pages
                        case 10:
                            {
                                var intent = new Intent(Context, typeof(PagesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Blogs
                        case 11:
                            StartActivity(new Intent(Context, typeof(ArticlesActivity)));
                            break;
                        // Market
                        case 12:
                            StartActivity(new Intent(Context, typeof(TabbedMarketActivity)));
                            break;
                        // Boosted Posts & Pages
                        case 13:
                            {
                                var intent = new Intent(Context, typeof(BoostedActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Popular Posts
                        case 14:
                            {
                                var intent = new Intent(Context, typeof(PopularPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Events
                        case 15:
                            {
                                var intent = new Intent(Context, typeof(EventMainActivity));
                                StartActivity(intent);
                                break;
                            }
                        // NearBy Find Friends
                        case 16:
                            {
                                var intent = new Intent(Context, typeof(PeopleNearByActivity));
                                StartActivity(intent);
                                break;
                            }
                        //Offers
                        case 17:
                        {
                            var intent = new Intent(Context, typeof(OffersActivity));
                            StartActivity(intent);
                            break;
                        }
                        // Movies
                        case 18:
                            {
                                var intent = new Intent(Context, typeof(MoviesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Patriot TV
                        case 19:
                            {
                                var intent = new Intent(Context, typeof(LocalWebViewActivity));
                                intent.PutExtra("URL", "https://www.americafirstpatriot.org/patriot-tv");
                                intent.PutExtra("Type", this.GetString(Resource.String.Lbl_PatriotTV));
                                StartActivity(intent);
                                break;
                            }
                        // common things
                        case 20:
                            {
                                var intent = new Intent(Context, typeof(CommonThingsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Memories
                        case 21:
                        {
                            var intent = new Intent(Context, typeof(MemoriesActivity));
                            StartActivity(intent);
                            break;
                        }
                        // Funding
                        case 22:
                            {
                                //var intent = new Intent(Context, typeof(LocalWebViewActivity));
                                //intent.PutExtra("URL", "https://www.americafirstpatriot.org/browse-fundraisers");
                                //intent.PutExtra("Type", this.GetString(Resource.String.Lbl_Funding));
                                //StartActivity(intent);
                                var intent = new Intent(Context, typeof(MainFundraiserActicity));
                                //var intent = new Intent(Context, typeof(FundingActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Games
                        case 23:
                            {
                                var intent = new Intent(Context, typeof(GamesActivity));
                                StartActivity(intent);
                                break;
                            }
                       
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreSectionAdapter2OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter2?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    { 
                        // General Account
                        case 100:
                            {
                                var intent = new Intent(Context, typeof(GeneralAccountActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Privacy
                        case 101:
                            {
                                var intent = new Intent(Context, typeof(PrivacyActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Notification
                        case 102:
                            {
                                var intent = new Intent(Context, typeof(MessegeNotificationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // InvitationLinks
                        case 103:
                            {
                                var intent = new Intent(Context, typeof(InvitationLinksActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyInformation
                        case 104:
                            {
                                var intent = new Intent(Context, typeof(MyInformationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Tell Friends
                        case 105:
                            {
                                var intent = new Intent(Context, typeof(TellFriendActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Help & Support
                        case 106:
                            {
                                var intent = new Intent(Context, typeof(SupportActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Logout
                        case 107:
                            {
                                var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                dialog.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                                dialog.Content(Context.GetText(Resource.String.Lbl_Are_you_logout));
                                dialog.PositiveText(Context.GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                                dialog.NegativeText(Context.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                                dialog.AlwaysCallSingleChoiceCallback();
                                dialog.Build().Show();
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long);
                    ApiRequest.Logout(Activity);
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
       
        #endregion
          
    }
}