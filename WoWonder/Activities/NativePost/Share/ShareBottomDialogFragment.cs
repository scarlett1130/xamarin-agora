using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.Content;
using Android.OS; 
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Exception = System.Exception;

namespace WoWonder.Activities.NativePost.Share
{
    public class ShareBottomDialogFragment : BottomSheetDialogFragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private LinearLayout ShareTimelineLayout, ShareGroupLayout, ShareOptionsLayout, SharePageLayout;
        private PostDataObject DataPost;
        private PostModelType TypePost;
        private string TypeDialog;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.NativeShareBottomDialog, container, false); 
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

                DataPost = JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("ItemData") ?? ""); 
                TypePost = JsonConvert.DeserializeObject<PostModelType>(Arguments?.GetString("TypePost") ?? "");

                InitComponent(view);
                AddOrRemoveEvent(true);
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

        private void InitComponent(View view)
        {
            try
            {
                ShareTimelineLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareTimelineLayout);
                ShareGroupLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareGroupLayout);
                ShareOptionsLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareOptionsLayout);
                SharePageLayout = view.FindViewById<LinearLayout>(Resource.Id.SharePageLayout);

                if (TypePost == PostModelType.AdsPost)
                {
                    ShareTimelineLayout.Visibility = ViewStates.Gone;
                    ShareGroupLayout.Visibility = ViewStates.Gone;
                    SharePageLayout.Visibility = ViewStates.Gone;
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
                        ShareTimelineLayout.Click += ShareTimelineLayoutOnClick;
                        ShareGroupLayout.Click += ShareGroupLayoutOnClick;
                        ShareOptionsLayout.Click += ShareOptionsLayoutOnClick;
                        SharePageLayout.Click += SharePageLayoutOnClick;
                        break;
                    default:
                        ShareTimelineLayout.Click -= ShareTimelineLayoutOnClick;
                        ShareGroupLayout.Click -= ShareGroupLayoutOnClick;
                        ShareOptionsLayout.Click -= ShareOptionsLayoutOnClick;
                        SharePageLayout.Click -= SharePageLayoutOnClick;
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

        //ShareToPage
        private void SharePageLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    List<PageDataObject> listPageClass = ListUtils.MyPageList.ToList();

                    if (listPageClass.Count > 0)
                    {
                        Intent intent = new Intent(Context, typeof(SharePageActivity));
                        intent.PutExtra("Pages", JsonConvert.SerializeObject(listPageClass));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost));
                        StartActivity(intent);
                    }
                    else
                        ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_NoPageManaged), ToastLength.Short);

                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 

        private async void ShareOptionsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    case false:
                        return;
                }
                 
                switch (TypePost)
                {
                    case PostModelType.EventPost:
                    {
                        if (DataPost.Event?.EventClass != null)
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = Methods.FunString.DecodeString(DataPost.Event.Value.EventClass.Name),
                                Text = Methods.FunString.DecodeString(DataPost.Event.Value.EventClass.Description),
                                Url = DataPost.Event.Value.EventClass.Url,
                            });
                        break;
                    }
                    case PostModelType.ImagePost:
                    case PostModelType.StickerPost:
                    case PostModelType.MapPost:
                    case PostModelType.MultiImage2:
                    case PostModelType.MultiImage3:
                    case PostModelType.MultiImage4:
                    { 
                        var imagesList = DataPost.PhotoMulti ?? DataPost.PhotoAlbum;
                        string urlImage = imagesList?.Count > 0 ? imagesList[0].Image : !string.IsNullOrEmpty(DataPost.PostSticker) ? DataPost.PostSticker : DataPost.PostFileFull;
                        var fileName = urlImage?.Split('/').Last();
                             
                        switch (AppSettings.AllowDownloadMedia)
                        {
                            case true:
                                await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url, urlImage, fileName, Context.GetText(Resource.String.Lbl_Send_to));
                                break;
                            default:
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = "",
                                    Text = DataPost.Url,
                                    Url = DataPost.Url
                                });
                                break;
                        } 
                        break;
                    }
                    case PostModelType.LinkPost:
                    case PostModelType.YoutubePost:
                    {
                        var linkUrl = DataPost.Url;
                        ShareFileImplementation.ShareText(Activity, linkUrl, Context.GetText(Resource.String.Lbl_Send_to));
                        break;
                    }
                    case PostModelType.VideoPost:
                    {
                        var linkUrl = DataPost.PostFileFull;
                        var fileName = linkUrl?.Split('/').Last();

                        switch (AppSettings.AllowDownloadMedia)
                        {
                            case true:
                                await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url,linkUrl, fileName, Context.GetText(Resource.String.Lbl_Send_to));
                                break;
                            default:
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = "",
                                    Text = DataPost.Url,
                                    Url = DataPost.Url
                                });
                                break;
                        }
                        break; 
                    }
                    case PostModelType.FilePost:
                    {
                        var linkUrl = DataPost.PostFileFull;
                        var fileName = linkUrl?.Split('/').Last();
                             
                        switch (AppSettings.AllowDownloadMedia)
                        {
                            case true:
                                await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url, linkUrl, fileName, Context.GetText(Resource.String.Lbl_Send_to));
                                break;
                            default:
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = "",
                                    Text = DataPost.Url,
                                    Url = DataPost.Url
                                });
                                break;
                        }
                        break;
                    }
                    case PostModelType.ProductPost:
                    {
                        if (DataPost.Product != null)
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = Methods.FunString.DecodeString(DataPost.Product.Value.ProductClass.Name),
                                Text = Methods.FunString.DecodeString(DataPost.Product.Value.ProductClass.Description),
                                Url = DataPost.Product.Value.ProductClass.Url,
                            });
                        break;
                    }
                    case PostModelType.BlogPost:
                        if (DataPost.Blog != null)
                        {
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = Methods.FunString.DecodeString(DataPost.Blog.Title),
                                Text = Methods.FunString.DecodeString(DataPost.Blog.Description),
                                Url = DataPost.Blog.Url,
                            });
                        }
                        break;
                    case PostModelType.AdsPost:
                        if (DataPost.Blog != null)
                        {
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = "",
                                Text = DataPost.Url,
                                Url = DataPost.Url,
                            });
                        }
                        break;
                    default:
                    {
                        if (DataPost.Blog != null)
                        {
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = Methods.FunString.DecodeString(DataPost.Blog.Title),
                                Text = Methods.FunString.DecodeString(DataPost.Blog.Description),
                                Url = DataPost.Blog.Url,
                            });
                        }
                        else switch (string.IsNullOrEmpty(DataPost.PostSticker))
                        {
                            case false:
                            {
                                var linkUrl = DataPost.PostSticker; 
                                var fileName = linkUrl?.Split('/').Last();
                                 
                                switch (AppSettings.AllowDownloadMedia)
                                {
                                    case true:
                                        await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url, linkUrl, fileName, Context.GetText(Resource.String.Lbl_Send_to));
                                        break;
                                    default:
                                        await CrossShare.Current.Share(new ShareMessage
                                        {
                                            Title = "",
                                            Text = DataPost.Url,
                                            Url = DataPost.Url
                                        });
                                        break;
                                }

                                break;
                            }
                            default:
                            {
                                switch (string.IsNullOrEmpty(DataPost.PostFileFull))
                                {
                                    case false:
                                    {
                                        var linkUrl = DataPost.PostFileFull;
                                        var fileName = linkUrl?.Split('/').Last();
                                 
                                        var type = Methods.AttachmentFiles.Check_FileExtension(linkUrl);
                                        switch (type)
                                        {
                                            case "Image":
                                            case "File":
                                                await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url, linkUrl, fileName, Context.GetText(Resource.String.Lbl_Send_to));
                                                break;
                                            default:
                                                ShareFileImplementation.ShareText(Activity, linkUrl, Context.GetText(Resource.String.Lbl_Send_to));
                                                break;
                                        }

                                        break;
                                    }
                                    default:
                                        await CrossShare.Current.Share(new ShareMessage
                                        {
                                            Title = "",
                                            Text = Methods.FunString.DecodeString(DataPost.PostText),
                                            Url = DataPost.Url
                                        });
                                        break;
                                }

                                break;
                            }
                        }

                        break;
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //ShareToGroup
        private void ShareGroupLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    List<GroupDataObject> listGroupClass = ListUtils.MyGroupList.ToList();

                    if (listGroupClass.Count > 0)
                    {
                        Intent intent = new Intent(Context, typeof(ShareGroupActivity));
                        intent.PutExtra("Groups", JsonConvert.SerializeObject(listGroupClass));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost));
                        StartActivity(intent);
                    } 
                    else
                        ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_NoGroupManaged), ToastLength.Short);
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }   
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void ShareTimelineLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "ShareToMyTimeline";

                    var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    dialog.Title(Resource.String.Lbl_Share).TitleColorRes(Resource.Color.primary);
                    dialog.Content(Context.GetText(Resource.String.Lbl_ShareToMyTimeline));
                    dialog.PositiveText(Context.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(Context.GetText(Resource.String.Lbl_No)).OnNegative(this).NegativeColorRes(Resource.Color.colorIcon);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
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
                switch (TypeDialog)
                {
                    case "ShareToGroup":
                    {
                        GroupDataObject dataGroup = ListUtils.MyGroupList[position];
                        if (dataGroup != null)
                        {
                            Intent intent = new Intent(Context,typeof(SharePostActivity));
                            intent.PutExtra("ShareToType", "Group"); 
                            intent.PutExtra("ShareToGroup", JsonConvert.SerializeObject(dataGroup)); //GroupClass
                            intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost)); //PostDataObject
                            Context.StartActivity(intent); 

                            Dismiss();
                        }

                        break;
                    }
                    case "ShareToPage":
                    {
                        PageDataObject dataPage = ListUtils.MyPageList[position];
                        if (dataPage != null)
                        {
                            Intent intent = new Intent(Context, typeof(SharePostActivity));
                            intent.PutExtra("ShareToType", "Page"); 
                            intent.PutExtra("ShareToPage", JsonConvert.SerializeObject(dataPage)); //PageClass
                            intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost)); //PostDataObject
                            Context.StartActivity(intent);

                            Dismiss();
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

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    switch (TypeDialog)
                    {
                        case "ShareToMyTimeline":
                        {
                            Intent intent = new Intent(Context, typeof(SharePostActivity));
                            intent.PutExtra("ShareToType", "MyTimeline");
                            //intent.PutExtra("ShareToMyTimeline", "");  
                            intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost)); //PostDataObject
                            Context.StartActivity(intent);

                            Dismiss();
                            break;
                        }
                    }
                }
                else if (p1 == DialogAction.Neutral)
                {
                    switch (TypeDialog)
                    {
                        case "ShareToGroup":
                            Context.StartActivity(new Intent(Context, typeof(CreateGroupActivity)));
                            break;
                        case "ShareToPage":
                            Context.StartActivity(new Intent(Context, typeof(CreatePageActivity)));
                            break;
                    }
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