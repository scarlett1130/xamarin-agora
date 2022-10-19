using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient.Classes.Games;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************

        public static GetSiteSettingsObject.ConfigObject SettingsSiteList;

        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();

        public static ObservableCollection<UserDataObject> MyProfileList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> MyFollowersList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PageDataObject> MyPageList = new ObservableCollection<PageDataObject>();
        public static ObservableCollection<PageDataObject> InvitesPagesList = new ObservableCollection<PageDataObject>();
        public static ObservableCollection<GroupDataObject> MyGroupList = new ObservableCollection<GroupDataObject>();
       
        public static ObservableCollection<Classes.Family> FamilyList = new ObservableCollection<Classes.Family>();
         
        public static ObservableCollection<PostDataObject> ListCachedDataAlbum = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<ArticleDataObject> ListCachedDataArticle = new ObservableCollection<ArticleDataObject>();
        public static ObservableCollection<Classes.ProductClass> ListCachedDataMyProduct = new ObservableCollection<Classes.ProductClass>();
        public static ObservableCollection<MoviesDataObject> ListCachedDataMovie = new ObservableCollection<MoviesDataObject>();
        public static ObservableCollection<UserDataObject> ListCachedDataNearby = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GiftObject.DataGiftObject> GiftsList = new ObservableCollection<GiftObject.DataGiftObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyPhotos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyVideos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<Classes.GameClass> ListCachedDataGames = new ObservableCollection<Classes.GameClass>();
        public static ObservableCollection<GamesDataObject> ListCachedDataPopularGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GamesDataObject> ListCachedDataMyGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GroupDataObject> SuggestedGroupList = new ObservableCollection<GroupDataObject>();
        public static ObservableCollection<UserDataObject> SuggestedUserList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PageDataObject> SuggestedPageList = new ObservableCollection<PageDataObject>();

        public static ObservableCollection<UserDataObject> FriendRequestsList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<TrendingHashtag> HashTagList = new ObservableCollection<TrendingHashtag>();
        public static ObservableCollection<Classes.ShortCuts> ShortCutsList = new ObservableCollection<Classes.ShortCuts>();

        public static ObservableCollection<DataTables.StickersTb> StickersList = new ObservableCollection<DataTables.StickersTb>();
        public static ObservableCollection<PostDataObject> VideoReelsList = new ObservableCollection<PostDataObject>();
        public static List<PostDataObject> NewPostList = new List<PostDataObject>();

        public static void ClearAllList()
        {
            try
            {
                DataUserLoginList.Clear(); 
                MyProfileList.Clear();
                MyFollowersList.Clear();
                MyPageList.Clear();
                InvitesPagesList.Clear();
                MyGroupList.Clear(); 
                FamilyList.Clear();
                ListCachedDataAlbum.Clear();
                ListCachedDataArticle.Clear();
                ListCachedDataMyProduct.Clear();
                ListCachedDataMovie.Clear();
                ListCachedDataNearby.Clear();
                GiftsList.Clear();
                ListCachedDataMyPhotos.Clear();
                ListCachedDataMyVideos.Clear();
                ListCachedDataGames.Clear();
                ListCachedDataPopularGames.Clear();
                ListCachedDataMyGames.Clear();
                SuggestedGroupList.Clear();
                SuggestedUserList.Clear();
                FriendRequestsList.Clear();
                HashTagList.Clear();
                ShortCutsList.Clear();
                SuggestedPageList.Clear();
                StickersList.Clear();
                NewPostList.Clear();
                 
                StickersList.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        //{
        //    try
        //    {
        //        items.ToList().ForEach(collection.Add);
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}


        //public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        //{
        //    var list = new List<List<T>>();

        //    for (int i = 0; i < locations.Count; i += nSize)
        //    {
        //        list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
        //    }

        //    return list;
        //}

        //public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        //{
        //    var enumerable = source as T[] ?? source.ToArray();

        //    return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        //}


        //public static T Cast<T>(object myobj)
        //{
        //    Type objectType = myobj.GetType();
        //    Type target = typeof(UserDataObject);
        //    object x = Activator.CreateInstance(target, false);

        //    var z = from source in objectType.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    var d = from source in target.GetMembers().ToList()
        //            where source.MemberType == MemberTypes.Property
        //            select source;

        //    List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name)).ToList();
        //    PropertyInfo propertyInfo;

        //    object value;
        //    foreach (var memberInfo in members)
        //    {
        //        propertyInfo = typeof(T).GetProperty(memberInfo.Name);
        //        value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj, null);

        //        if (propertyInfo != null)
        //            propertyInfo.SetValue(x, value, null);
        //    }
        //    return (T)x;
        //}


    }
}