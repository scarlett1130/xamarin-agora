using System;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Lang;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Message;
using Exception = System.Exception;

namespace WoWonder.StickersView
{
    public class StickerItemClickListener
    {
        private readonly string Type;
        private readonly CommentActivity CommentActivity;
        private readonly StoryReplyActivity StoryReplyActivity;
        private readonly string TimeNow = DateTime.Now.ToString("hh:mm");

        public StickerItemClickListener(string type)
        {
            try
            {
                Type = type;

                switch (Type)
                {
                    // Create your fragment here
                    case "CommentActivity":
                        CommentActivity = CommentActivity.GetInstance();
                        break;
                    case "StoryReplyActivity":
                        StoryReplyActivity = StoryReplyActivity.GetInstance();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StickerAdapterOnOnItemClick(string stickerUrl)
        {
            try
            {
                var position = "1";
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                switch (Type)
                {

                    case "CommentActivity":
                    {
                        CommentActivity.ImageUrl = stickerUrl;
                        Glide.With(CommentActivity).Load(stickerUrl).Apply(new RequestOptions()).Into(CommentActivity.ImgGallery);
                        break;
                    }
                    case "StoryReplyActivity":
                        {
                            if (Methods.CheckConnectivity())
                            {
                                //Sticker Send Function
                                StoryReplyActivity.SendMess(StoryReplyActivity.UserId, "", "", "", stickerUrl, "sticker" + position).ConfigureAwait(false);
                            }
                            else
                            {
                                ToastUtils.ShowToast(StoryReplyActivity, StoryReplyActivity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
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

    }
}