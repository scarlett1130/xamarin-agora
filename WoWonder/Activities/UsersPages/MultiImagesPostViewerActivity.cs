using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.ViewPager.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.PostData;
using WoWonder.Activities.UsersPages.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.UsersPages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MultiImagesPostViewerActivity : AppCompatActivity
    {
        #region Variables Basic

        private ViewPager ViewPager;
        private ImageView ImgLike, ImgWoWonder;
        private SuperTextView TxtDescription;
        private TextView TxtCountLike, TxtCountWoWonder, CommentCount, ShareCount;
        private LinearLayout MainSectionButton, BtnCountLike, BtnCountWoWonder, ShareLinearLayout, CommentLinearLayout, SecondReactionLinearLayout, ReactLinearLayout, InfoImageLiner;
        private RelativeLayout MainLayout;
        private PostDataObject PostData;
        private ReactButton LikeButton;
        private PostClickListener ClickListener;
        private TextView SecondReactionButton;
        private int IndexImage;
        private string TypePost;
        private ObservableCollection<string> photos;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            { 
                
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.MultiImagesPostViewerLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_DataImage();

                ClickListener = new PostClickListener(this , NativeFeedType.Global);

                AdsGoogle.Ad_RewardedVideo(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ImagePost, menu); 

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.download:
                    Download_OnClick();
                    break;

                /*case Resource.Id.ic_action_comment:
                    Copy_OnClick();
                    break;*/

                case Resource.Id.action_More:
                    More_OnClick();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Event Download Image  
        private void Download_OnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                { 
                    IndexImage = ViewPager.CurrentItem;

                    Methods.CapturePhotoUtils.InsertImage(ContentResolver, photos[IndexImage]);
                     
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ImageSaved), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Copy link image 
        private void Copy_OnClick()
        {
            try
            {
                Methods.CopyToClipboard(this, PostData.Url); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event More 
        private void More_OnClick()
        {
            try
            {
                ClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = PostData });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = (ViewPager)FindViewById(Resource.Id.view_pager);

                TxtDescription = FindViewById<SuperTextView>(Resource.Id.tv_description);
                TxtDescription?.SetTextInfo(TxtDescription);

                ImgLike = FindViewById<ImageView>(Resource.Id.image_like1);
                ImgWoWonder = FindViewById<ImageView>(Resource.Id.image_wowonder);

                TxtCountLike = FindViewById<TextView>(Resource.Id.LikeText1);
                TxtCountWoWonder = FindViewById<TextView>(Resource.Id.WoWonderTextCount);

                ShareCount = FindViewById<TextView>(Resource.Id.Sharecount);
                CommentCount = FindViewById<TextView>(Resource.Id.Commentcount);

                MainLayout = FindViewById<RelativeLayout>(Resource.Id.main);
                InfoImageLiner = FindViewById<LinearLayout>(Resource.Id.infoImageLiner);
                InfoImageLiner.Visibility = ViewStates.Visible;

                BtnCountLike = FindViewById<LinearLayout>(Resource.Id.linerlikeCount);
                BtnCountWoWonder = FindViewById<LinearLayout>(Resource.Id.linerwowonderCount);

                ShareLinearLayout = FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                CommentLinearLayout = FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                SecondReactionLinearLayout = FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                ReactLinearLayout = FindViewById<LinearLayout>(Resource.Id.ReactLinearLayout);

                // set the default image display type
                // PageImage.SetDisplayType(ImageViewTouchBase.DisplayType.FitIfBigger); 

                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);
                LikeButton.SetTextColor(Color.ParseColor("#C3C7D0"));

                SecondReactionButton = FindViewById<TextView>(Resource.Id.SecondReactionText);

                ShareLinearLayout.Visibility = AppSettings.ShowShareButton switch
                {
                    false => ViewStates.Gone,
                    _ => ShareLinearLayout.Visibility
                };

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                    case PostButtonSystem.Like:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 3 : 2;

                        SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                        TxtCountWoWonder.Visibility = ViewStates.Gone;
                        ImgWoWonder.Visibility = ViewStates.Gone;
                        break;
                    case PostButtonSystem.Wonder:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(
                            Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);

                        TxtCountWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);

                        break;
                    case PostButtonSystem.DisLike:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(
                            Resource.Drawable.ic_action_dislike, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);

                        TxtCountWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_dislike);

                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = " ";
                    toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    toolBar.SetNavigationIcon(Resource.Drawable.ic_action_left_arrow2);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    
                }
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
                    {
                        CommentLinearLayout.Click += BtnCommentOnClick;
                        ShareLinearLayout.Click += BtnShareOnClick;
                        BtnCountLike.Click += BtnCountLikeOnClick;
                        BtnCountWoWonder.Click += BtnCountWoWonderOnClick;
                        InfoImageLiner.Click += MainLayoutOnClick;
                        MainLayout.Click += MainLayoutOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionButton.Click += SecondReactionButtonOnClick;
                                break;
                        }

                        LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs
                        {
                            NewsFeedClass = PostData,
                            View = MainLayout,
                        }, null, "MultiImagesPostViewerActivity");

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.ReactionDefault:
                            case PostButtonSystem.ReactionSubShine:
                                LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs
                                {
                                    NewsFeedClass = PostData,
                                    View = MainLayout,
                                }, null, "MultiImagesPostViewerActivity");
                                break;
                        }
                        break;
                    }
                    default:
                    {
                        CommentLinearLayout.Click -= BtnCommentOnClick;
                        ShareLinearLayout.Click -= BtnShareOnClick;
                        BtnCountLike.Click -= BtnCountLikeOnClick;
                        BtnCountWoWonder.Click -= BtnCountWoWonderOnClick;
                        InfoImageLiner.Click -= MainLayoutOnClick;
                        MainLayout.Click -= MainLayoutOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionButton.Click -= SecondReactionButtonOnClick;
                                break;
                        }

                        LikeButton.Click += null!;
                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.ReactionDefault:
                            case PostButtonSystem.ReactionSubShine:
                                LikeButton.LongClick -= null!;
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
        private void DestroyBasic()
        {
            try
            {
                ViewPager = null!;
                TxtDescription = null!;
                ImgLike = null!;
                ImgWoWonder = null!;
                TxtCountLike = null!;
                TxtCountWoWonder = null!;
                MainLayout = null!;
                InfoImageLiner = null!;
                BtnCountLike = null!;
                BtnCountWoWonder = null!;
                MainSectionButton = null!;
                SecondReactionButton = null!;
                LikeButton = null!;
                ClickListener = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Events

        private void MainLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoImageLiner.Visibility = InfoImageLiner.Visibility != ViewStates.Visible
                    ? ViewStates.Visible
                    : ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Add Wonder
        private void SecondReactionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("select.mp3");
                        break;
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder when PostData.IsWondered != null && (bool)PostData.IsWondered:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            PostData.IsWondered = false;
                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Btn_Wonder);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#666666"));
                            break;
                        }
                    case PostButtonSystem.Wonder:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            x++;

                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            PostData.IsWondered = true;

                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Lbl_wondered);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));

                            PostData.Reaction ??= new Reaction();
                            if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                            {
                                PostData.Reaction.IsReacted = false;
                            }

                            break;
                        }
                    case PostButtonSystem.DisLike when PostData.IsWondered != null && PostData.IsWondered.Value:
                        {
                            var unwrappedDrawable =
                                AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"),
                                        PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Btn_Dislike);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#666666"));

                            var x = Convert.ToInt32(PostData.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            PostData.IsWondered = false;
                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            break;
                        }
                    case PostButtonSystem.DisLike:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            x++;

                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            PostData.IsWondered = true;

                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"),
                                        PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));

                            PostData.Reaction ??= new Reaction();
                            if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                            {
                                PostData.Reaction.IsReacted = false;
                            }

                            break;
                        }
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;

                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == PostData.Id).ToList();
                switch (dataGlobal?.Count)
                {
                    case > 0:
                        {
                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData = PostData;
                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "reaction");
                            }

                            break;
                        }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "wonder")});
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "dislike")});
                        break;
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show all users Wowonder >> Open Post PostData_Activity
        private void BtnCountWoWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PostDataActivity));
                intent.PutExtra("PostId", PostData.PostId);
                intent.PutExtra("PostType", "post_wonders");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show all users liked >> Open Post PostData_Activity
        private void BtnCountLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                        {
                            switch (PostData.Reaction.Count)
                            {
                                case > 0:
                                    {
                                        var intent = new Intent(this, typeof(ReactionPostTabbedActivity));
                                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostData));
                                        StartActivity(intent);
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        {
                            var intent = new Intent(this, typeof(PostDataActivity));
                            intent.PutExtra("PostId", PostData.PostId);
                            intent.PutExtra("PostType", "post_likes");
                            StartActivity(intent);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostData, },
                    PostModelType.ImagePost);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Add Comment
        private void BtnCommentOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = PostData,
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                IndexImage = Convert.ToInt32(Intent?.GetStringExtra("indexImage") ?? "0");
                TypePost = Intent?.GetStringExtra("TypePost") ?? ""; 
                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("AlbumObject") ?? "");
                if (PostData != null)
                {
                    photos = new ObservableCollection<string>();

                    switch (TypePost)
                    {
                        case "Product" when PostData.Product != null:
                        {
                            foreach (var item in PostData.Product.Value.ProductClass.Images)
                            {
                                photos.Add(item.Image);
                            }

                            break;
                        }
                        default:
                        {
                            foreach (var item in PostData.PhotoMulti ?? PostData.PhotoAlbum)
                            {
                                photos.Add(item.Image);
                            }

                            break;
                        }
                    }
                    ViewPager.Adapter = new TouchImageAdapter(this, photos);
                    ViewPager.CurrentItem = IndexImage;
                    ViewPager.Adapter.NotifyDataSetChanged();

                    if (string.IsNullOrEmpty(PostData.Orginaltext) || string.IsNullOrWhiteSpace(PostData.Orginaltext))
                    {
                        TxtDescription.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        var description = Methods.FunString.DecodeString(PostData.Orginaltext);
                        var readMoreOption = new StReadMoreOption.Builder()
                            .TextLength(250, StReadMoreOption.TypeCharacter)
                            .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                            .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                            .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LabelUnderLine(true)
                            .Build();
                        readMoreOption.AddReadMoreTo(TxtDescription, new Java.Lang.String(description)); 
                    }

                    CommentCount.Text = PostData.PostComments + " " + GetString(Resource.String.Lbl_Comments);
                    ShareCount.Text = PostData.PostShares + " " + GetString(Resource.String.Lbl_Shares);

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.ReactionDefault:
                        case PostButtonSystem.ReactionSubShine:
                            {
                                PostData.Reaction ??= new Reaction();

                                TxtCountLike.Text = Methods.FunString.FormatPriceValue(PostData.Reaction.Count);

                                if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                                {
                                    switch (string.IsNullOrEmpty(PostData.Reaction.Type))
                                    {
                                        case false:
                                            {
                                                var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostData.Reaction.Type).Value?.Id ?? "";
                                                switch (react)
                                                {
                                                    case "1":
                                                        LikeButton.SetReactionPack(ReactConstants.Like);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                                        break;
                                                    case "2":
                                                        LikeButton.SetReactionPack(ReactConstants.Love);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_love);
                                                        break;
                                                    case "3":
                                                        LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                        break;
                                                    case "4":
                                                        LikeButton.SetReactionPack(ReactConstants.Wow);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                        break;
                                                    case "5":
                                                        LikeButton.SetReactionPack(ReactConstants.Sad);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                        break;
                                                    case "6":
                                                        LikeButton.SetReactionPack(ReactConstants.Angry);
                                                        ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                        break;
                                                    default:
                                                        LikeButton.SetReactionPack(ReactConstants.Default);
                                                        ImgLike.SetImageResource(PostData.Reaction.Count > 0
                                                            ? Resource.Drawable.emoji_like
                                                            : Resource.Drawable.icon_post_like_vector);
                                                        break;
                                                }

                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    LikeButton.SetReactionPack(ReactConstants.Default);
                                    LikeButton.SetTextColor(Color.ParseColor("#C3C7D0"));

                                    ImgLike.SetImageResource(PostData.Reaction.Count > 0
                                        ? Resource.Drawable.emoji_like
                                        : Resource.Drawable.icon_post_like_vector);
                                }

                                break;
                            }
                        default:
                        {
                            ImgLike.SetImageResource(Resource.Drawable.icon_post_like_vector);

                            TxtCountLike.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostData.PostLikes));

                            switch (AppSettings.PostButton)
                            {
                                case PostButtonSystem.Wonder:
                                case PostButtonSystem.DisLike:
                                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostData.PostWonders));
                                    break;
                            }

                            if (PostData.Reaction.IsReacted != null && !PostData.Reaction.IsReacted.Value)
                                LikeButton.SetReactionPack(ReactConstants.Default);

                            if (PostData.IsLiked != null && PostData.IsLiked.Value)
                                LikeButton.SetReactionPack(ReactConstants.Like);

                                if (SecondReactionButton != null)
                                {
                                    switch (AppSettings.PostButton)
                                    {
                                        case PostButtonSystem.Wonder when PostData.IsWondered != null && PostData.IsWondered.Value:
                                            {
                                                Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                                                Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                switch (Build.VERSION.SdkInt)
                                                {
                                                    case <= BuildVersionCodes.Lollipop:
                                                        DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                        break;
                                                    default:
                                                        wrappedDrawable = wrappedDrawable.Mutate();
                                                        wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                        break;
                                                }

                                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                                SecondReactionButton.Text = GetString(Resource.String.Lbl_wondered);
                                                SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                break;
                                            }
                                        case PostButtonSystem.Wonder:
                                            {
                                                Drawable unwrappedDrawable =
                                                    AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                                                Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                switch (Build.VERSION.SdkInt)
                                                {
                                                    case <= BuildVersionCodes.Lollipop:
                                                        DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                        break;
                                                    default:
                                                        wrappedDrawable = wrappedDrawable.Mutate();
                                                        wrappedDrawable.SetColorFilter(
                                                            new PorterDuffColorFilter(Color.ParseColor("#666666"),
                                                                PorterDuff.Mode.SrcAtop));
                                                        break;
                                                }

                                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable,
                                                    null, null, null);

                                                SecondReactionButton.Text = GetString(Resource.String.Btn_Wonder);
                                                SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                                break;
                                            }
                                        case PostButtonSystem.DisLike
                                            when PostData.IsWondered != null && PostData.IsWondered.Value:
                                            {
                                                Drawable unwrappedDrawable =
                                                    AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                                                Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                                switch (Build.VERSION.SdkInt)
                                                {
                                                    case <= BuildVersionCodes.Lollipop:
                                                        DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                        break;
                                                    default:
                                                        wrappedDrawable = wrappedDrawable.Mutate();
                                                        wrappedDrawable.SetColorFilter(
                                                            new PorterDuffColorFilter(Color.ParseColor("#f89823"),
                                                                PorterDuff.Mode.SrcAtop));
                                                        break;
                                                }

                                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable,
                                                    null, null, null);

                                                SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                                                SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                                break;
                                            }
                                        case PostButtonSystem.DisLike:
                                            {
                                                Drawable unwrappedDrawable =
                                                    AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                                                Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                switch (Build.VERSION.SdkInt)
                                                {
                                                    case <= BuildVersionCodes.Lollipop:
                                                        DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                        break;
                                                    default:
                                                        wrappedDrawable = wrappedDrawable.Mutate();
                                                        wrappedDrawable.SetColorFilter(
                                                            new PorterDuffColorFilter(Color.ParseColor("#666666"),
                                                                PorterDuff.Mode.SrcAtop));
                                                        break;
                                                }

                                                SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable,
                                                    null, null, null);

                                                SecondReactionButton.Text = GetString(Resource.String.Btn_Dislike);
                                                SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                                break;
                                            }
                                    }
                                }

                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}