//##############################################
//Cᴏᴘʏʀɪɢʜᴛ 2020 DᴏᴜɢʜᴏᴜᴢLɪɢʜᴛ Codecanyon Item 19703216
//Elin Doughouz >> https://www.facebook.com/Elindoughouz
//====================================================

//For the accuracy of the icon and logo, please use this website " https://appicon.co " and add images according to size in folders " mipmap " 

using System.Collections.Generic;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Helpers.Model;

namespace WoWonder
{
    internal static class AppSettings
    {
        /// <summary>
        /// Deep Links To App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">demo.wowonder.com</string>
        /// </summary>
        public static string TripleDesAppServiceProvider = "o/VzrGFt3Tss5DDtv6lyC6Wh9FgcSD5ptBpgvHSiTA0DUQe0+jAokeP1FZiKK63wyNuRNQ1J7bCzdoNBoDznLbUWGfNVh2NArm4bEBS6wiQwGevuQue6ZDSDrl04VrJGgX/6yvmXMfysguIcN0Fq+Ej4o37OfyQC6UUohnVq0E7Tnhjw4kQhoRvg/Ky5VMqj2H2oX9AO6nvALLaSvmfKiQzvBK4vDrFZjyLvxjvfH6dY3S2nX/aLtWt/ZxYU09GFEnOz8bnxILTeRSlWc1K+0rtiUeb3Ws+68I9amrVh6DTwRdKOHG0QvWOy3b8cPPSZjppaBCP5YiAjAJLm0+IAiKf2ZX8JNYjY1F19HhRg1MoHe8HPiGKyarkJmwiAM5NKoRn7o8Vdb1i8jizYLmi+MXOWPZi4VJEnxxAYRsk4IZoQHVjQKkFb+RYcYcVnqlco3i34NqXK6suO+HeD28hM/rLJhegcxPxTP1yCHwzccCkVRofqjXwmn2W3dU65XbHVLK9YzJsRhMtNp4Q7zUuQnVFvdsZe7A/SrxhFXxmUqxaQRiZJXRQbRafRK+NDomN2EQ6I4hVtCGuiCme79WXrvlIwH7KO2+O7136uhxnlV9ZOL40We8eMkNZ8MuxSJF8yEWuufmas9hU3fsRqIpSB74L7A+FYZL5/H4ndzT28BExY7j/TVox7/UfTUtj/fGo6U39wO4ucRzHEk1RaWQNDHfmANKiTnuMBxgcDTJ5NVxdPUWawxq389HPobjqGQb18VgagAE/WvQslbbCVDvAHXrxsz6KMt1+Sz/QWd2WMMlmsNHmOGqhZpjzQVok/PtKpFBXb9OW+1/uT4VGHAAc1OHwHUMyRo6K32tdEAVgxp2ZnBy3y4d/qLlyQCLBVy34QPPaCmMJQNs82k6hYHraane0w3WGHCidPEIZAVWlcVB3XuHOzOzEq3WIM4G/r/wbbBiU8t8v6Glz9QceRh+x41Bo5ebicMPfFQSIWzWdUAfYstarfCvtMZsC2HIr+bkTZTpZDB3PquLtQhut5hrZxxAzYOFe3LHX9Sh0+VmATnr4tl+bmVU8OMisOZoTcRfjZZs0B2JcffWU13cG4YEO6HrSvswFPzz3ftqcVvMTiIpAeUSNPt9PHtIB+5RLw5cidmAJ1qAOP9bjBmJIuHzT/GjeUBIhCWWCkRaladcszABHP57w24VnkJ9devytvDtITG8nUgF1Rxkc5noo2bt3M3fJV1myG1TUbGiHeKVuu2WJ9zKMKm/KlDfdV3bxZWsjKbAXcV6TkQqmg1suBE2zdl1R/2xe5q2UsNNlkthDsjTWQ6XpuNJUjy4bKoCGRZysklcFjUGwp1FfEqe7Ra3PEKH43f9HIV1hh+L8KN2iwBQyqZRkLmgl4SQ/ogI8yraTLaO57iKprxEZzUzWb7Ne9EM4Es5fULaaEpR7MjNUBor6blPB5q8xG2bBCbivfS36/q3HQlJCEFVC1hf/6Ci/+DzQl+3CqODdShRp7ODMsR7xC88sl0EHAS8H6O+nkcQf9Vwjh1wbTVIrxbJbcfK94z7s5qvYI2B1iNPWWMOCUGaua44NKq4vaWMcVDXKVvMRB4jw/rskxDQVIJGZtdoVrZCKEfdXH+Tbzevqp45JjAtBgoKECmHWSKfLt8HQPMnLMkwkRIapelTAzHqzXRGVIT5mW4k/Iw1ds6bCe0WWokU7uW7ZHAddWpn/YrTEddPmV7nUQUeHueVZ3uBxl2n5pYB4czMoF5rh6P02DKYWpj/DSPtF3E++jtJ/9N2DeQf77EB8m/aKrTs1O4oFKLCpuLR8o0zpSoAbxyNR7hjKUN1YksoiOy7N+gBlFKEuOM16TJwrvqlk8d3Mmz0KlWaTOMHGG2YQcpZwfHfo6SFIRtnAsWxWTYDrxIZSbyph6RSZlZlzehLAubD3zUTjvQvN0PRusVTcTyysz6PRBHH5oxwQhHIfzntEQ2p+8WE1z2gBstvEFn8CUtKoeKlL0/AVlc2+s71DTNX1qlnTYT9Hm/OVg/Rv8uqchquXeygZE985Rfu+KS4bwlyA5LRDnt1eyKhTdvcGeOnWY9mISRzj15cBOod8q0x874aRfWw98UR7gQi+33V7+EDH6I/ttLJHgokKNjCDkNuSg5r4MUa9DzKuidXsQYrICilm0Ss5FdiDQrN2UiJszVPA6ENgAPFkk1vr1jud/7ESP5700uv9ZUJRtQgjzXqOi0FWyiIQV/Fb0StOZhwErWByMZkArsUQlGaMPzwFxqXVCbbAW3ZBHzBtKPDtrDGeM7CYBVj+tWORTBl8WUqXBe/rBLj0kOmF8Vv/uZ0d6cwJmxWLjH2vf6etYiNgdAIKIjpmKzaFslX6bTUVctdx40CYXmwTbtrNDS6H90k2mifeUT3J/V4Y5KAaICHoSyztYXtSKLc2VK+fZz7gorbYTuO6DsNFdqNGuJi/WkyMKrSt4eZr2yViS/wv8hEBSBkdOSaVvHj/bS/DHvRbQmYqqOOhGN5amRAjL9VAMhcTmW6DL+ya4VPoN1CUNrGBbnMg059XFmsgmbKbbA3BIgkMw2BNuzZBKCl4bRISxtrkyTYhyL3vhE2ajykcTpM6YT33q4JUECOpvay0xv+ze8tb0TuZMc9dV5Rt9hsfJ6Ho7ElZ0QOP55NC89Zm7m+2L/2ihHRcyJfGPS8h65mEpVY9hHo1wcoFulXzVfQMEtk4V4D3yTM9NyXRGVst97uNQORps93iRSrucFVSIadge8bpwXGwJdQ2w+t1pY2PzYUZmEVyqIWUVdmfUXPUWUDApbnSTqAgLMs5KWAJmfoWUTDkjCeewOQAKWRw8OMvluVp5uDy3jmfWIhnyGmP9GnXIY9yXc+CmzaIe0L/a9Frk/n7o6sFEetrTDZW6rxyqGH1Z/rqNt+E0YJjHh512rCZywrXtOnMq+om5fXpwArqFL5zbpWNwFdls/n+P3j0GQSXCVNCBi+j8G7DASdHlOIPH+gFFgnMYgSGWvztCrRpvvB8AxlCUH4/gIuLJI7A9xJ8cVzBir4j7atDT0Sc5nvHQMpi0bp7mscHN9bBV7QAvz7ErxgUGQ+doGZixBAi6whl2iyd/lDNZRgMe7FFrBFA7bNJ5caQKUrkMZloQNHRcMp1QMkqU/jtEGkMWUgkF7RwRHyyre8Uy+5RiiMyPHniFo4i4mO7bnWVhm6fcFWO2txMEfPpUL4dMhtudI5JWe7dDOyctgT9weROIJxOr9NoNIJ28iKfQYkAcOuG+HQdApOxq0U6Fa1+metYUxwZbbwGLdDA52HsjyvIDUZbbG1iVTGC7tk56Em+OGIBCX46SKrgxXl4RMBG2lD5yXf/NFyRJtqfetq86kIiO6Vs7ipjR1cRKeuQ98KLXkDsyN4IYDxOl0Igzb/CXKRJ+yub6vIrjNuEOvZ2e0ek3izhdPxO+AtsynDd3qsAv3dIPZ3AtLvJzEn4+aHcXEiW+A+/7h5KRbvnH2OhU18O7A7ILk/00534E5u/sIKHr01sLrdxMMCnzqwrrVoQx69oRD6PvqlhSpFlHLN26TZJRo3TurtvM59c5FjrGc/zAHZhZ6rdi/+E82RatBvpxfZNNTdDN96px9SKJXE0ICTHExhhz1R1QvpIZNzAH8YsBYboPkCvflSWSc6KacTZV5IC7947+VcsNtNZsmKR0b3IxccflZp/kboUTCC2nq6gz742cnb3w9FP1mSWRmMSuw7mgA1dSoAD4ftHo0oihtTz+qg3BfnCPL64CW4ds7/HUQsLvjipUaE6je+bl4+5c+Ifd+8697gnJoQI+iFa12QnK30v5ODnH2oHYDp8Q8WzYWx3sqrZiKJfLfl6ZYLQPVTaBpSnErfpUMyByPerA47X3yV1YN32s/7PP/Ld3pGgv35FV58lOa25s6KJYWc0b2zR7KfdueKM/8m3znbhManeW+jQ8qz+wfzbypg1PhzL9hjfCKsTjl++owAx/ZdSfC9bqzcKvtalPlF3+1eyzVOKRcWyRdle+ys6U5kHhjPg79qsHbgSfzveUOwHr/Yj04cfrPw==";

        //Main Settings >>>>>
        //*********************************************************
        public static string Version = "5.0";
        public static string ApplicationName = "Social Network For Patriots";
        public static string DatabaseName = "SocialNetworkForPatriots"; 

        // Friend system = 0 , follow system = 1
        public static int ConnectivitySystem = 0;
         
        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#002868";
         
        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar

        //Set Language User on site from phone 
        public static bool SetLangUser = true; 

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "64974c58-9993-40ed-b782-0814edc401ea";

        // WalkThrough Settings >>
        //*********************************************************
        public static bool ShowWalkTroutPage = true;

        //Main Messenger settings
        //*********************************************************
        public static bool MessengerIntegration = true;
        public static bool ShowDialogAskOpenMessenger = false; 
        public static string MessengerPackageName = "com.messengerforpatriot.app"; //APK name on Google Play

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static ShowAds ShowAds = ShowAds.AllUsers;

        //Three times after entering the ad is displayed
        public static int ShowAdInterstitialCount = 3;
        public static int ShowAdRewardedVideoCount = 3;
        public static int ShowAdNativeCount = 40;
        public static int ShowAdAppOpenCount = 2;
         
        public static bool ShowAdMobBanner = false;
        public static bool ShowAdMobInterstitial = false;
        public static bool ShowAdMobRewardVideo = false;
        public static bool ShowAdMobNative = false;
        public static bool ShowAdMobNativePost = false;
        public static bool ShowAdMobAppOpen = false;  
        public static bool ShowAdMobRewardedInterstitial = false;  

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/3584502890";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/2518408206";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2280543246";
        public static string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/2813560515";  
        public static string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/7842669101";  
         
        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false;  
        public static bool ShowFbRewardVideoAds = false; 
        public static bool ShowFbNativeAds = false; 
         
        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";  
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492"; 
        public static string AdsFbNativeKey = "250485588986218_554706301897477"; 
         
        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static bool ShowColonyBannerAds = false; 
        public static bool ShowColonyInterstitialAds = false; 
        public static bool ShowColonyRewardAds = false;  

        public static string AdsColonyAppId = "appff22269a7a0a4be8aa"; 
        public static string AdsColonyBannerId = "vz85ed7ae2d631414fbd"; 
        public static string AdsColonyInterstitialId = "vz39712462b8634df4a8";  
        public static string AdsColonyRewardedId = "vz32ceec7a84aa4d719a"; 
        //********************************************************* 

        public static bool EnableRegisterSystem = true;

        /// <summary>
        /// true => Only over 18 years old
        /// false => all 
        /// </summary>
        public static bool IsUserYearsOld = true;
        public static bool AddAllInfoPorfileAfterRegister = true; //#New

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;
            
        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static bool AutoCodeTimeZone = true;
        public static string CodeTimeZone = "UTC";

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file 
        //Facebook >> ../values/analytic.xml .. line 10-11 
        //Google >> ../values/analytic.xml .. line 15 
        //*********************************************************
        public static bool EnableSmartLockForPasswords = true; //#New

        public static bool ShowFacebookLogin = false;
        public static bool ShowGoogleLogin = false;

        public static readonly string ClientId = "430795656343-679a7fus3pfr1ani0nr0gosotgcvq2s8.apps.googleusercontent.com";

        //########################### 

        //Main Slider settings
        //*********************************************************
        public static PostButtonSystem PostButton = PostButtonSystem.ReactionDefault;
        public static ToastTheme ToastTheme = ToastTheme.Custom; 

        public static BottomNavigationSystem NavigationBottom = BottomNavigationSystem.Default; 

        public static bool ShowBottomAddOnTab = true; 
        public static bool ShowFilterPost = true; //#New

        public static bool ShowAlbum = true;
        public static bool ShowArticles = true;
        public static bool ShowPokes = true;
        public static bool ShowCommunitiesGroups = true;
        public static bool ShowCommunitiesPages = true;
        public static bool ShowMarket = true;
        public static bool ShowPopularPosts = true;
        /// <summary>
        /// if selected false will remove boost post and get list Boosted Posts
        /// </summary>
        public static bool ShowBoostedPosts = true;  
        public static bool ShowBoostedPages = true;  
        public static bool ShowMovies = true;
        public static bool ShowNearBy = true;
        public static bool ShowStory = true;
        public static bool ShowSavedPost = true;
        public static bool ShowUserContacts = true; 
        public static bool ShowJobs = true; 
        public static bool ShowCommonThings = true; 
        public static bool ShowFundings = true;
        public static bool ShowMyPhoto = true; 
        public static bool ShowMyVideo = true; 
        public static bool ShowGames = true;
        public static bool ShowMemories = true;  
        public static bool ShowOffers = true;  
        public static bool ShowNearbyShops = true;   

        public static bool ShowSuggestedPage = true; 
        public static bool ShowSuggestedGroup = true;
        public static bool ShowSuggestedUser = true;

        public static bool ShowBrowserUser = true;
        public static bool ShowPatriotTV = true;
         
        //count times after entering the Suggestion is displayed
        public static int ShowSuggestedPageCount = 90;  
        public static int ShowSuggestedGroupCount = 70; 
        public static int ShowSuggestedUserCount = 50;

        //allow download or not when share
        public static bool AllowDownloadMedia = true; 

        public static bool ShowAdvertise = true;  
         
        /// <summary>
        /// https://rapidapi.com/api-sports/api/covid-193
        /// you can get api key and host from here https://prnt.sc/wngxfc 
        /// </summary>
        public static bool ShowInfoCoronaVirus = false;  
        public static string KeyCoronaVirus = "164300ef98msh0911b69bed3814bp131c76jsneaca9364e61f"; 
        public static string HostCoronaVirus = "covid-193.p.rapidapi.com"; 
         
        public static bool ShowLive = true;
        public static string AppIdAgoraLive = "fd2506f784e9465dbbe15883d313da86";

        //Events settings
        //*********************************************************  
        public static bool ShowEvents = true; 
        public static bool ShowEventGoing = true; 
        public static bool ShowEventInvited = true;  
        public static bool ShowEventInterested = true;  
        public static bool ShowEventPast = true;

        // Story >>
        //*********************************************************
        //Set a story duration >> Sec
        public static long StoryImageDuration = 7; 
        public static long StoryVideoDuration = 30; //#New

        /// <summary>
        /// If it is false, it will appear only for the specified time in the value of the StoryVideoDuration
        /// </summary>
        public static bool ShowFullVideo = false; //#New

        public static bool EnableStorySeenList = true; 
        public static bool EnableReplyStory = true;
         
        /// <summary>
        /// https://dashboard.stipop.io/
        /// you can get api key from here https://prnt.sc/26ofmq9
        /// </summary>
        public static string StickersApikey = "0a441b19287cad752e87f6072bb914c0";//#New
         
        //*********************************************************

        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;
        public static readonly string CurrencyIconStatic = "$";
        public static readonly string CurrencyCodeStatic = "USD";
        public static readonly string CurrencyFundingPriceStatic = "$";

        //Profile settings
        //*********************************************************
        public static bool ShowGift = false;
        public static bool ShowWallet = true; 
        public static bool ShowGoPro = true;  
        public static bool ShowAddToFamily = true;

        public static bool ShowUserGroup = false; 
        public static bool ShowUserPage = false;  
        public static bool ShowUserImage = true;  
        public static bool ShowUserSocialLinks = false; 

        public static CoverImageStyle CoverImageStyle = CoverImageStyle.CenterCrop; 

        /// <summary>
        /// The default value comes from the site .. in case it is not available, it is taken from these values
        /// </summary>
        public static string WeeklyPrice = "4.999"; 
        public static string MonthlyPrice = "39"; 
        public static string YearlyPrice = "99"; 
        public static string LifetimePrice = "259";

        //Native Post settings
        //********************************************************* 
        public static bool ShowTextWithSpace = true; 

        public static bool ShowTextShareButton = false;
        public static bool ShowShareButton = true;
         
        public static int AvatarPostSize = 60;
        public static int ImagePostSize = 200;
        public static string PostApiLimitOnScroll = "22";

        //Get post in background >> 1 Min = 30 Sec
        public static long RefreshPostSeconds = 30000;  
        public static string PostApiLimitOnBackground = "12"; 

        public static bool AutoPlayVideo = true;
         
        public static bool EmbedPlayTubePostType = true;
        public static bool EmbedDeepSoundPostType = true;
        public static VideoPostTypeSystem EmbedFacebookVideoPostType = VideoPostTypeSystem.EmbedVideo; 
        public static VideoPostTypeSystem EmbedVimeoVideoPostType = VideoPostTypeSystem.EmbedVideo; 
        public static VideoPostTypeSystem EmbedPlayTubeVideoPostType = VideoPostTypeSystem.Link; 
        public static VideoPostTypeSystem EmbedTikTokVideoPostType = VideoPostTypeSystem.Link; 
        public static VideoPostTypeSystem EmbedTwitterPostType = VideoPostTypeSystem.Link; 
        public static bool ShowSearchForPosts = true; 
        public static bool EmbedLivePostType = true;
         
        //new posts users have to scroll back to top
        public static bool ShowNewPostOnNewsFeed = true; 
        public static bool ShowAddPostOnNewsFeed = false; 
        public static bool ShowCountSharePost = true;

        public static WRecyclerView.VolumeState DefaultVolumeVideoPost = WRecyclerView.VolumeState.On; 

        /// <summary>
        /// Post Privacy
        /// ShowPostPrivacyForAllUser = true : all posts user have icon Privacy 
        /// ShowPostPrivacyForAllUser = false : just my posts have icon Privacy (default)
        /// </summary>
        public static bool ShowPostPrivacyForAllUser = false; 

        public static bool ShowFullScreenVideoPost = true;

        public static bool EnableVideoCompress = false;
        public static bool EnableFitchOgLink = true; //#New

        //Trending page
        //*********************************************************   
        public static bool ShowTrendingPage = true;
         
        public static bool ShowProUsersMembers = true;
        public static bool ShowPromotedPages = true;
        public static bool ShowTrendingHashTags = true;
        public static bool ShowLastActivities = true;
        public static bool ShowShortcuts = true; 
        public static bool ShowFriendsBirthday = true; 
        public static bool ShowAnnouncement = true;

        /// <summary>
        /// https://www.weatherapi.com
        /// </summary>
        public static WeatherType WeatherType = WeatherType.Celsius; //#New
        public static bool ShowWeather = true;  
        public static string KeyWeatherApi = "e7cffc4d6a9a4a149a1113143201711";

        /// <summary>
        /// https://openexchangerates.org
        /// #Currency >> Your currency
        /// #Currencies >> you can use just 3 from those : USD,EUR,DKK,GBP,SEK,NOK,CAD,JPY,TRY,EGP,SAR,JOD,KWD,IQD,BHD,DZD,LYD,AED,QAR,LBP,OMR,AFN,ALL,ARS,AMD,AUD,BYN,BRL,BGN,CLP,CNY,MYR,MAD,ILS,TND,YER
        /// </summary>
        public static bool ShowExchangeCurrency = false; 
        public static string KeyCurrencyApi = "644761ef2ba94ea5aa84767109d6cf7b"; 
        public static string ExCurrency = "USD";  
        public static string ExCurrencies = "EUR,GBP,TRY"; 
        public static readonly List<string> ExCurrenciesIcons = new List<string>() {"€", "£", "₺"};

        //********************************************************* 

        /// <summary>
        /// you can edit video using FFMPEG 
        /// </summary>
        public static bool EnableVideoEditor = true; //#New


        public static bool ShowUserPoint = true;

        //Add Post
        public static AddPostSystem AddPostSystem = AddPostSystem.AllUsers;

        public static bool ShowGalleryImage = true;
        public static bool ShowGalleryVideo = true;
        public static bool ShowMention = true;
        public static bool ShowLocation = true;
        public static bool ShowFeelingActivity = true;
        public static bool ShowFeeling = true;
        public static bool ShowListening = true;
        public static bool ShowPlaying = true;
        public static bool ShowWatching = true;
        public static bool ShowTraveling = true;
        public static bool ShowGif = true;
        public static bool ShowFile = true;
        public static bool ShowMusic = true;
        public static bool ShowPolls = true;
        public static bool ShowColor = true;
        public static bool ShowVoiceRecord = true; 

        public static bool ShowAnonymousPrivacyPost = true;

        //Advertising 
        public static bool ShowAdvertisingPost = true;  

        //Settings Page >> General Account
        public static bool ShowSettingsGeneralAccount = true;
        public static bool ShowSettingsAccount = true;
        public static bool ShowSettingsSocialLinks = true;
        public static bool ShowSettingsPassword = true;
        public static bool ShowSettingsBlockedUsers = true;
        public static bool ShowSettingsDeleteAccount = true;
        public static bool ShowSettingsTwoFactor = true; 
        public static bool ShowSettingsManageSessions = true;  
        public static bool ShowSettingsVerification = true;
         
        public static bool ShowSettingsSocialLinksFacebook = true; 
        public static bool ShowSettingsSocialLinksTwitter = true; 
        public static bool ShowSettingsSocialLinksGoogle = true; 
        public static bool ShowSettingsSocialLinksVkontakte = true;  
        public static bool ShowSettingsSocialLinksLinkedin = true;  
        public static bool ShowSettingsSocialLinksInstagram = true;  
        public static bool ShowSettingsSocialLinksYouTube = true;  

        //Settings Page >> Privacy
        public static bool ShowSettingsPrivacy = true;
        public static bool ShowSettingsNotification = true;

        //Settings Page >> Tell a Friends (Earnings)
        public static bool ShowSettingsInviteFriends = true;

        public static bool ShowSettingsShare = true;
        public static bool ShowSettingsMyAffiliates = true;
        public static bool ShowWithdrawals = true;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// Just replace it with this 5 lines of code
        /// <uses-permission android:name="android.permission.READ_CONTACTS" />
        /// <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" />
        /// </summary>
        public static bool InvitationSystem = true; 

        //Settings Page >> Help && Support
        public static bool ShowSettingsHelpSupport = true;

        public static bool ShowSettingsHelp = true;
        public static bool ShowSettingsReportProblem = true;
        public static bool ShowSettingsAbout = true;
        public static bool ShowSettingsPrivacyPolicy = true;
        public static bool ShowSettingsTermsOfUse = true;

        public static bool ShowSettingsRateApp = true; 
        public static int ShowRateAppCount = 5; 
         
        public static bool ShowSettingsUpdateManagerApp = false; 

        public static bool ShowSettingsInvitationLinks = true; 
        public static bool ShowSettingsMyInformation = true; 

        public static bool ShowSuggestedUsersOnRegister = true;

        //Set Theme Tab
        //*********************************************************
        public static bool SetTabDarkTheme = false;
        public static MoreTheme MoreTheme = MoreTheme.Grid; 

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static bool RenderPriorityFastPostLoad = false;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false;

        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc & https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static bool ShowPaypal = true;
        public static string MerchantAccountId = "test"; //#New

        public static string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4c******"; //#New
        public static string ProductionTokenizationKey = "AdzE77ZVXeF_zfbeG3qNNqwHP-8EWE4mvoNS4PRguezMmpK9zOhEp3-DexnqCyMNKd9RSiE5M-HlRZvX"; //#New

        public static bool ShowBankTransfer = false; 
        public static bool ShowCreditCard = true;

        //********************************************************* 
        public static bool ShowCashFree = false;  

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static string CashFreeCurrency = "INR";  

        //********************************************************* 

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 24 
        /// </summary>
        public static bool ShowRazorPay = false; 

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static string RazorPayCurrency = "USD";  
         
        public static bool ShowPayStack = false;  
        public static bool ShowPaySera = false;  //#Next Version   
                                               
        //********************************************************* 

    }
}