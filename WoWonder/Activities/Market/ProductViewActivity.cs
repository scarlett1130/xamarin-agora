using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;


using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Market.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UsersPages;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests; 
using Exception = System.Exception;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ProductViewActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView TxtProductName, TxtProductPrice, TxtProductNew, TxtProductInStock,  TxtProductLocation, TxtProductCardName, TxtProductTime;
        private SuperTextView TxtProductDescription;
        private ImageView ImageMore, UserImageAvatar, IconBack;
        private AppCompatButton BtnContact;
        private ProductDataObject ProductData;
        private ViewPager ViewPagerView;
        private CircleIndicator CircleIndicatorView;
        private string TypeDialog, PostId;
        private RecyclerView CommentsRecyclerView;
        private CommentAdapter MAdapter;

        private LinearLayout MainSectionButton;
        private LinearLayout ShareLinearLayout;
        private LinearLayout CommentLinearLayout;
        private LinearLayout SecondReactionLinearLayout;
        private LinearLayout ReactLinearLayout;
        private ReactButton LikeButton;
        private TextView SecondReactionButton;

        private PostClickListener ClickListener;
        private PostDataObject PostData;

        private LinearLayout LocationLayout; 

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.Overlap_Dark : Resource.Style.Overlap_Light);

                Methods.App.FullScreenApp(this);

                Overlap();
                
                // Create your application here
                SetContentView(Resource.Layout.ProductView_Layout);

                PostId = Intent?.GetStringExtra("Id") ?? string.Empty;

                ClickListener = new PostClickListener(this, NativeFeedType.Global);

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                ProductData = JsonConvert.DeserializeObject<ProductDataObject>(Intent?.GetStringExtra("ProductView") ?? "");
                switch (ProductData)
                {
                    case null:
                        return;
                    default:
                        Get_Data_Product(ProductData);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Overlap()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window?.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
                }
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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPagerView = FindViewById<ViewPager>(Resource.Id.pager);
                CircleIndicatorView = FindViewById<CircleIndicator>(Resource.Id.indicator);

                TxtProductName = FindViewById<TextView>(Resource.Id.tv_name);
                TxtProductPrice = (TextView)FindViewById(Resource.Id.tv_price);
                TxtProductNew = (TextView)FindViewById(Resource.Id.BoleanNew);
                TxtProductInStock = (TextView)FindViewById(Resource.Id.BoleanInStock);
                TxtProductDescription = (SuperTextView)FindViewById(Resource.Id.tv_description);
                LocationLayout = (LinearLayout)FindViewById(Resource.Id.locationLayout);
                TxtProductLocation = (TextView)FindViewById(Resource.Id.tv_Location);
                TxtProductCardName = (TextView)FindViewById(Resource.Id.card_name);
                TxtProductTime = (TextView)FindViewById(Resource.Id.card_dist);

                BtnContact = (AppCompatButton)FindViewById(Resource.Id.cont);

                UserImageAvatar = (ImageView)FindViewById(Resource.Id.card_pro_pic);
                ImageMore = (ImageView)FindViewById(Resource.Id.Image_more);
                IconBack = (ImageView)FindViewById(Resource.Id.iv_back);


                ShareLinearLayout = FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                CommentLinearLayout = FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                SecondReactionLinearLayout = FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                ReactLinearLayout = FindViewById<LinearLayout>(Resource.Id.ReactLinearLayout);
                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);

                SecondReactionButton = FindViewById<TextView>(Resource.Id.SecondReactionText);
                 
                ShareLinearLayout.Visibility = ViewStates.Gone;

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                    case PostButtonSystem.Like:
                        MainSectionButton.WeightSum = 2;

                        SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                        break;
                    case PostButtonSystem.Wonder:
                        MainSectionButton.WeightSum = 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;

                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                        break;
                    case PostButtonSystem.DisLike:
                        MainSectionButton.WeightSum = 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_action_dislike, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                        break;
                }

                switch (AppSettings.SetTabDarkTheme)
                {
                    case false:
                        ImageMore.SetColorFilter(Color.White);
                        break;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void ViewPagerViewOnClick()
        { 
            try
            {
                var intent = new Intent(this, typeof(MultiImagesPostViewerActivity));
                intent.PutExtra("indexImage", "0"); // Index Image Show
                intent.PutExtra("TypePost", "Product"); // Index Image Show
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(PostData)); // PostDataObject
                OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void SetRecyclerViewAdapters()
        {
            try
            { 
                CommentsRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                CommentsRecyclerView.NestedScrollingEnabled = false;
                MAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };

                StartApiService();
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
                        BtnContact.Click += BtnContactOnClick;
                        UserImageAvatar.Click += UserImageAvatarOnClick;
                        TxtProductCardName.Click += UserImageAvatarOnClick; 
                        ImageMore.Click += ImageMoreOnClick;
                        IconBack.Click += IconBackOnClick;
                        TxtProductDescription.LongClick += TxtProductDescriptionOnLongClick;
                        CommentLinearLayout.Click += BtnCommentOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionLinearLayout.Click += SecondReactionLinearLayoutOnClick;
                                break;
                        }

                        LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs
                        {
                            NewsFeedClass = PostData,
                        } , null, "ProductViewActivity");

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.ReactionDefault:
                            case PostButtonSystem.ReactionSubShine:
                                LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs
                                {
                                    NewsFeedClass = PostData,
                                }, null, "ProductViewActivity");
                                break;
                        }
                        break;
                    }
                    default:
                    {
                        BtnContact.Click -= BtnContactOnClick;
                        UserImageAvatar.Click -= UserImageAvatarOnClick;
                        TxtProductCardName.Click -= UserImageAvatarOnClick; 
                        ImageMore.Click -= ImageMoreOnClick;
                        IconBack.Click -= IconBackOnClick;
                        TxtProductDescription.LongClick -= TxtProductDescriptionOnLongClick;
                        CommentLinearLayout.Click -= BtnCommentOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionLinearLayout.Click -= SecondReactionLinearLayoutOnClick;
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
                ViewPagerView = null!;
                CircleIndicatorView = null!;
                TxtProductName = null!;
                TxtProductPrice = null!;
                TxtProductNew = null!;
                TxtProductInStock = null!;
                TxtProductDescription = null!;
                TxtProductLocation = null!;
                TxtProductCardName = null!;
                TxtProductTime = null!;
                BtnContact = null!;
                UserImageAvatar = null!;
                ImageMore = null!;
                IconBack  = null!;
                MainSectionButton = null!;
                SecondReactionLinearLayout = null!;
                LikeButton = null!;
                ClickListener = null!;
                PostData = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtProductDescriptionOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                if (Methods.FunString.StringNullRemover(ProductData.Description) != "Empty")
                {
                    Methods.CopyToClipboard(this, ProductData.Description); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Contact seller 
        private void BtnContactOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                switch (AppSettings.MessengerIntegration)
                {
                    case true when AppSettings.ShowDialogAskOpenMessenger:
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialog.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                        dialog.Content(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "SendMsgProduct", new ChatObject { UserId = ProductData.Seller.UserId, Avatar = ProductData.Seller.Avatar, Name = ProductData.Seller.Name, LastMessage = new LastMessageUnion { LastMessageClass = new MessageData { ProductId = ProductData.Id, Product = new ProductUnion { ProductClass = ProductData } } } });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                        break;
                    }
                    case true:
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "SendMsgProduct", new ChatObject { UserId = ProductData.Seller.UserId, Avatar = ProductData.Seller.Avatar, Name = ProductData.Seller.Name, LastMessage = new LastMessageUnion { LastMessageClass = new MessageData { ProductId = ProductData.Id, Product = new ProductUnion { ProductClass = ProductData } } } });
                        break;
                    default:
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            return;
                        }

                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var time = unixTimestamp.ToString();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SendMessageAsync(ProductData.Seller.UserId, time, "", "", "", "", "", "", ProductData.Id) });
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully), ToastLength.Short);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        // Event Open User Profile
        private void UserImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(this, ProductData.Seller.UserId, ProductData.Seller);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Event Back
        private void IconBackOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event More >> Show Menu (CopeLink , Share)
        private void ImageMoreOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ProductData.Seller.UserId == UserDetails.UserId)
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_EditProduct));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Delete));
                }
                
                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                 
                dialogList.Title(GetString(Resource.String.Lbl_More)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void OnCopyLink_Button_Click()
        {
            try
            {
                Methods.CopyToClipboard(this, ProductData.Url); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private void OnShare_Button_Click()
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostData, }, PostModelType.ProductPost);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        //Event Menu >> Edit Info Product if user == is_owner  
        private void EditInfoProduct_OnClick()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditProductActivity));
                intent.PutExtra("ProductView", JsonConvert.SerializeObject(ProductData));
                StartActivityForResult(intent, 3500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        //Event Menu >> Edit Info Product if user == is_owner  
        private void DeleteProduct_OnClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                   TypeDialog = "DeletePost";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(GetText(Resource.String.Lbl_DeletePost)).TitleColorRes(Resource.Color.primary);
                    dialog.Content(GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        //Event Add Wonder / Disliked
        private void SecondReactionLinearLayoutOnClick(object sender, EventArgs e)
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
                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
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
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
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
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "dislike") });
                        break;
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    OnCopyLink_Button_Click();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetString(Resource.String.Lbl_EditProduct))
                {
                    EditInfoProduct_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                     DeleteProduct_OnClick();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "DeletePost":
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }

                            var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                            var diff = adapterGlobal?.ListDiffer;
                            var dataGlobal = diff?.Where(a => a.PostData?.PostId == ProductData?.PostId).ToList();
                            if (dataGlobal != null)
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                }
                            }

                            var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                            var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.PostId == ProductData?.PostId).ToList();
                            if (dataGlobal2 != null)
                            {
                                foreach (var postData in dataGlobal2)
                                {
                                    recycler.RemoveByRowIndex(postData);
                                }
                            }

                            var instance = TabbedMarketActivity.GetInstance();

                            var checkMyProductsTab = instance?.MyProductsTab?.MAdapter?.MarketList?.FirstOrDefault(a => a.Product.Id == ProductData.Id && a.Type == Classes.ItemType.MyProduct);
                            if (checkMyProductsTab != null) 
                            {
                                instance.MyProductsTab.MAdapter.MarketList?.Remove(checkMyProductsTab);
                                instance.MyProductsTab.MAdapter.NotifyDataSetChanged();
                            }

                            var checkMarketTab = instance?.MarketTab?.MAdapter?.MarketList?.FirstOrDefault(a => a.Product.Id == ProductData.Id && a.Type == Classes.ItemType.Product);
                            if (checkMarketTab != null)
                            {
                                instance.MarketTab.MAdapter.MarketList?.Remove(checkMarketTab);
                                instance.MarketTab.MAdapter.NotifyDataSetChanged();
                            }
                             
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(ProductData.PostId, "delete") }); 

                            Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }

                        break;
                    default:
                    {
                        if (p1 == DialogAction.Positive)
                        {
                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
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

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 3500 when resultCode == Result.Ok:
                    {
                        if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                        var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData"));
                        if (item != null)
                        {
                            ProductData = item;
                            Get_Data_Product(item);
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

        #endregion

        private void Get_Data_Product(ProductDataObject productData)
        {
            try
            {
                ProductData = productData;

                PostData = new PostDataObject
                {
                    PostId = productData.PostId,
                    Product = new ProductUnion
                    {
                        ProductClass = productData,
                    },
                    ProductId = productData.Id,
                    UserId = productData.UserId,
                    UserData = productData.Seller,
                    Url = productData.Url,
                    PostUrl = productData.Url,
                };

                List<string> listImageUser = new List<string>();
                switch (productData.Images?.Count)
                {
                    case > 0:
                        listImageUser.AddRange(productData.Images.Select(t => t.Image));
                        break;
                    default:
                        listImageUser.Add(productData.Images?[0]?.Image);
                        break;
                }

                switch (ViewPagerView.Adapter)
                {
                    case null:
                        ViewPagerView.Adapter = new MultiImagePagerAdapter(this, listImageUser);
                        ViewPagerView.CurrentItem = 0;
                        CircleIndicatorView.SetViewPager(ViewPagerView);
                        break;
                }
                ViewPagerView.Adapter.NotifyDataSetChanged();

                GlideImageLoader.LoadImage(this, productData.Seller.Avatar, UserImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                     
                var (currency, currencyIcon) = WoWonderTools.GetCurrency(productData.Currency); 
                TxtProductPrice.Text = currencyIcon + productData.Price;

                TxtProductName.Text = productData.Name;
              
                Console.WriteLine(currency);
                var readMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                if (Methods.FunString.StringNullRemover(productData.Description) != "Empty")
                {
                    var description =Methods.FunString.DecodeString(productData.Description); 
                     readMoreOption.AddReadMoreTo(TxtProductDescription, new String(description));
                }
                else
                {
                    TxtProductDescription.Text = GetText(Resource.String.Lbl_Empty);
                }
                
                if (!string.IsNullOrEmpty(productData.Location))
                {
                    LocationLayout.Visibility = ViewStates.Visible;
                    TxtProductLocation.Text = Methods.FunString.DecodeString(productData.Location);
                }
                else
                {
                    LocationLayout.Visibility = ViewStates.Gone;
                }
                 
                TxtProductCardName.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(productData.Seller), 14);
                TxtProductTime.Text = productData.TimeText;

                if (productData.Seller.UserId == UserDetails.UserId)
                    BtnContact.Visibility = ViewStates.Gone;
                 
                //Type == "0" >>  New // Type != "0"  Used
                TxtProductNew.Visibility = productData.Type == "0" ? ViewStates.Visible : ViewStates.Gone;

                // Status InStock
                TxtProductInStock.Visibility = productData.Status == "0" ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else 
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPostDataAsync, () => LoadDataComment("0") });
        }

        private async Task LoadPostDataAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Posts.GetPostDataAsync(PostId, "post_data");
                if (apiStatus == 200)
                {
                    if (respond is GetPostDataObject result)
                    {
                        PostData = result.PostData;

                        if (LikeButton != null)
                            LikeButton.Text = PostData.PostLikes;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.ReactionDefault:
                            case PostButtonSystem.ReactionSubShine:
                                {
                                    PostData.Reaction ??= new Reaction();

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
                                                            break;
                                                        case "2":
                                                            LikeButton.SetReactionPack(ReactConstants.Love);
                                                            break;
                                                        case "3":
                                                            LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                            break;
                                                        case "4":
                                                            LikeButton.SetReactionPack(ReactConstants.Wow);
                                                            break;
                                                        case "5":
                                                            LikeButton.SetReactionPack(ReactConstants.Sad);
                                                            break;
                                                        case "6":
                                                            LikeButton.SetReactionPack(ReactConstants.Angry);
                                                            break;
                                                        default:
                                                            LikeButton.SetReactionPack(ReactConstants.Default);
                                                            break;
                                                    }

                                                    break;
                                                }
                                        }
                                    }
                                    else
                                        LikeButton.SetReactionPack(ReactConstants.Default);

                                    break;
                                }
                            default:
                                {
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
                                                    Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_wowonder);
                                                    Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
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
                                                    SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                                    break;
                                                }
                                            case PostButtonSystem.DisLike when PostData.IsWondered != null && PostData.IsWondered.Value:
                                                {
                                                    Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
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

                                                    SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                                                    SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                                    break;
                                                }
                                            case PostButtonSystem.DisLike:
                                                {
                                                    Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.ic_action_dislike);
                                                    Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
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
                else
                    Methods.DisplayReportResult(this, respond);
            }
            else
            { 
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }
         
        #region LoadDataComment

        private async Task LoadDataComment(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Comment.GetPostCommentsAsync(PostId, "10", offset);
                if (apiStatus != 200 || respond is not CommentObject result || result.CommentList == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.CommentList?.Count;
                    switch (respondList)
                    {
                        case > 0:
                        {
                            foreach (var item in from item in result.CommentList let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                if (db != null) MAdapter.CommentList.Add(db);
                            }

                            switch (countList)
                            {
                                case > 0:
                                    RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                                    break;
                                default:
                                    RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                    break;
                            }

                            break;
                        }
                    }
                }

                RunOnUiThread(ShowEmptyPage2);
            }
        }

        private void ShowEmptyPage2()
        {
            try
            {
                switch (MAdapter.CommentList.Count)
                {
                    case > 0:
                    {
                        CommentsRecyclerView.Visibility = ViewStates.Visible;

                        var emptyStateChecker = MAdapter.CommentList.FirstOrDefault(a => a.Text == MAdapter.EmptyState);
                        if (emptyStateChecker != null && MAdapter.CommentList.Count > 1)
                        {
                            MAdapter.CommentList.Remove(emptyStateChecker);
                            MAdapter.NotifyDataSetChanged();
                        }

                        break;
                    }
                    default:
                    {
                        CommentsRecyclerView.Visibility = ViewStates.Gone;

                        MAdapter.CommentList.Clear();
                        var d = new CommentObjectExtra { Text = MAdapter.EmptyState };
                        MAdapter.CommentList.Add(d);
                        MAdapter.NotifyDataSetChanged();
                        break;
                    }
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