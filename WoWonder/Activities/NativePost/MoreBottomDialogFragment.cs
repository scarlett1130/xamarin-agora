using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Activities.NativePost
{
    public class MoreBottomDialogFragment : BottomSheetDialogFragment ,View.IOnClickListener
    {
        #region Variables Basic

        private LinearLayout LlSavePost, LlCopyText, LlCopyLink, LlHidePost, LlReportPost, LlEditPost, LlBoostPost, LlDisableComments, LlDeletePost;
        private TextView TvSavePost, TvCopyText, TvCopyLink, TvHidePost, TvReportPost, TvEditPost, TvBoostPost, TvDisableComments, TvDeletePost;

        private PostDataObject DataPost;
        private Activity MainContext;
        private PostClickListener Listener;

        #endregion

        #region General

        public MoreBottomDialogFragment(PostClickListener clickListener)
        {
            Listener = clickListener;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                MainContext = Activity;
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.post_more_bottom, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                LoadData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDetach()
        {
            try
            {
                base.OnDetach();
                Listener = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                LlSavePost = view.FindViewById<LinearLayout>(Resource.Id.more_save_post);
                LlCopyText = view.FindViewById<LinearLayout>(Resource.Id.more_copy_text);
                LlCopyLink = view.FindViewById<LinearLayout>(Resource.Id.more_copy_link);
                LlHidePost = view.FindViewById<LinearLayout>(Resource.Id.more_hide_post);
                LlReportPost = view.FindViewById<LinearLayout>(Resource.Id.more_report_post);
                LlEditPost = view.FindViewById<LinearLayout>(Resource.Id.more_edit_post);
                LlBoostPost = view.FindViewById<LinearLayout>(Resource.Id.more_boost_post);
                LlDisableComments = view.FindViewById<LinearLayout>(Resource.Id.more_disable_comment);
                LlDeletePost = view.FindViewById<LinearLayout>(Resource.Id.more_delete_post);

                TvSavePost = view.FindViewById<TextView>(Resource.Id.tv_save_post);
                TvCopyText = view.FindViewById<TextView>(Resource.Id.tv_copy_text);
                TvCopyLink = view.FindViewById<TextView>(Resource.Id.tv_copy_link);
                TvHidePost = view.FindViewById<TextView>(Resource.Id.tv_hide_post);
                TvReportPost = view.FindViewById<TextView>(Resource.Id.tv_report_post);
                TvEditPost = view.FindViewById<TextView>(Resource.Id.tv_edit_post);
                TvBoostPost = view.FindViewById<TextView>(Resource.Id.tv_boost_post);
                TvDisableComments = view.FindViewById<TextView>(Resource.Id.tv_disable_comment);
                TvDeletePost = view.FindViewById<TextView>(Resource.Id.tv_delete_post);

                LlSavePost?.SetOnClickListener(this);
                LlCopyText?.SetOnClickListener(this);
                LlCopyLink?.SetOnClickListener(this);
                LlHidePost?.SetOnClickListener(this);
                LlReportPost?.SetOnClickListener(this);
                LlEditPost?.SetOnClickListener(this);
                LlBoostPost?.SetOnClickListener(this);
                LlDisableComments?.SetOnClickListener(this);
                LlDeletePost?.SetOnClickListener(this);
                
                if (!AppSettings.ShowBoostedPosts)
                    LlBoostPost.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        public void OnClick(View v)
        {
            try
            {
                string item;
                if (v.Id == LlSavePost.Id)
                {
                    item = TvSavePost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlCopyText.Id)
                {
                    item = TvCopyText.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlCopyLink.Id)
                {
                    item = TvCopyLink.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlHidePost.Id)
                {
                    item = TvHidePost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlReportPost.Id)
                {
                    item = TvReportPost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlEditPost.Id)
                {
                    item = TvEditPost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlBoostPost.Id)
                {
                    item = TvBoostPost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlDisableComments.Id)
                {
                    item = TvDisableComments.Text;
                    Listener.OnItemClick(item, DataPost);
                }
                else if (v.Id == LlDeletePost.Id)
                {
                    item = TvDeletePost.Text;
                    Listener.OnItemClick(item, DataPost);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void LoadData()
        {
            try
            { 
                DataPost = JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("ItemData") ?? "");
                if (DataPost != null)
                {
                    var postType = PostFunctions.GetAdapterType(DataPost);
                      
                    LlCopyText.Visibility = !string.IsNullOrEmpty(DataPost.Orginaltext) ? ViewStates.Visible : ViewStates.Gone;
                    LlReportPost.Visibility = !Convert.ToBoolean(DataPost.IsPostReported) ? ViewStates.Visible : ViewStates.Gone;
                    LlHidePost.Visibility = DataPost.Publisher.UserId != UserDetails.UserId ? ViewStates.Visible : ViewStates.Gone;

                    TvSavePost.Text = DataPost.IsPostSaved != null && DataPost.IsPostSaved.Value ? GetText(Resource.String.Lbl_UnSavePost) : GetText(Resource.String.Lbl_SavePost);

                    if ((DataPost.UserId != "0" || DataPost.PageId != "0" || DataPost.GroupId != "0") && DataPost.Publisher.UserId == UserDetails.UserId)
                    {
                        switch (postType)
                        {
                            case PostModelType.ProductPost:
                                TvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditProduct);
                                break;
                            case PostModelType.OfferPost:
                                TvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditOffers);
                                break;
                            default:
                                TvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditPost);
                                break;
                        }

                        if (AppSettings.ShowAdvertisingPost)
                        {
                            var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                            if (dataUser?.IsPro == "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                            {
                                switch (DataPost?.Boosted)
                                {
                                    case "0":
                                        TvBoostPost.Text = MainContext.GetString(Resource.String.Lbl_BoostPost);
                                        break;
                                    case "1":
                                        TvBoostPost.Text = MainContext.GetString(Resource.String.Lbl_UnBoostPost);
                                        break;
                                }
                            } 
                        }

                        switch (DataPost?.CommentsStatus)
                        {
                            case "0":
                                TvDisableComments.Text = MainContext.GetString(Resource.String.Lbl_EnableComments);
                                break;
                            case "1":
                                TvDisableComments.Text = MainContext.GetString(Resource.String.Lbl_DisableComments);
                                break;
                        } 
                    }
                    else
                    {
                        LlEditPost.Visibility = ViewStates.Gone;
                        LlBoostPost.Visibility = ViewStates.Gone;
                        LlDisableComments.Visibility = ViewStates.Gone;
                        LlDeletePost.Visibility = ViewStates.Gone;
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