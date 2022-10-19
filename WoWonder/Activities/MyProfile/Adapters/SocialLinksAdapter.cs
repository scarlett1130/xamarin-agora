using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.MyProfile.Adapters
{
    public class SocialItem
    {
        public int Id { get; set; }
        public string SocialName { get; set; }
        public string SocialIcon { get; set; }
        public Color IconColor { get; set; }
        public string SocialLinkName { get; set; }
        public bool Checkvisibilty { get; set; }
    }

    public class SocialLinksAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<SocialItem> SocialList = new ObservableCollection<SocialItem>();
        private readonly Activity ActivityContext;
        public SocialLinksAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;

                switch (AppSettings.ShowSettingsSocialLinksFacebook)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 1,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_Facebook),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoFacebook,
                            IconColor = Color.ParseColor("#3b5999")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksTwitter)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 2,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_Twitter),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoTwitter,
                            IconColor = Color.ParseColor("#55acee")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksGoogle)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 3,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_GooglePlus) + "+",
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoGoogleplus,
                            IconColor = Color.ParseColor("#dd4b39")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksVkontakte)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 4,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_Vk),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = FontAwesomeIcon.Vk,
                            IconColor = Color.ParseColor("#4c75a3")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksLinkedin)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 5,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_Linkedin),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoLinkedin,
                            IconColor = Color.ParseColor("#0077B5")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksInstagram)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 6,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_Instagram),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoInstagram,
                            IconColor = Color.ParseColor("#e4405f")
                        });
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinksYouTube)
                {
                    case true:
                        SocialList.Add(new SocialItem
                        {
                            Id = 7,
                            SocialName = ActivityContext.GetText(Resource.String.Lbl_YouTube),
                            SocialLinkName = "",
                            Checkvisibilty = false,
                            SocialIcon = IonIconsFonts.LogoYoutube,
                            IconColor = Color.ParseColor("#cd201f")
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SocialList?.Count ?? 0;
 
        public event EventHandler<SocialLinksAdapterClickEventArgs> ItemClick;
        public event EventHandler<SocialLinksAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> ChannelSubscribed_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SocialLinks_View, parent, false);
                var vh = new SocialLinksAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case SocialLinksAdapterViewHolder holder:
                    {
                        var item = SocialList[position];
                        if (item != null)
                        {
                            string name = Methods.FunString.DecodeString(item.SocialName);
                            holder.NameSocial.Text = Methods.FunString.SubStringCutOf(name, 20);

                            holder.NameSocial.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#4E586E"));
                            holder.NameLink.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#DDDDDD"));

                            FontUtils.SetTextViewIcon(item.Id == 4 ? FontsIconFrameWork.FontAwesomeBrands : FontsIconFrameWork.IonIcons, holder.IconSocial, item.SocialIcon);

                            holder.IconSocial.SetTextColor(item.IconColor);

                            switch (item.Checkvisibilty)
                            {
                                case true:
                                    holder.NameLink.Text = item.SocialLinkName;
                                    holder.AddLink.Background.SetTint(Color.ParseColor("#F6F6F6"));
                                    holder.AddLink.Text = ActivityContext.GetText(Resource.String.Lbl_Edit);
                                    holder.AddLink.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#525252"));
                                    break;
                                default:
                                    holder.NameLink.Text = ActivityContext.GetText(Resource.String.Lbl_NoSocialLink);
                                    holder.AddLink.Text = ActivityContext.GetText(Resource.String.Lbl_AddLink);
                                    holder.AddLink.Background.SetTint(Color.ParseColor("#FFEFEF"));
                                    holder.AddLink.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    break;
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public void Update(SocialItem item, string linkName)
        {
            try
            {
                var data = SocialList.FirstOrDefault(a => a.Id == item.Id);
                if (data != null)
                {
                    switch (string.IsNullOrEmpty(linkName))
                    {
                        case false:
                            data.SocialLinkName = linkName;
                            data.Checkvisibilty = true;
                            break;
                        default:
                            data.Checkvisibilty = false;
                            break;
                    }

                    NotifyItemChanged(SocialList.IndexOf(data));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public SocialItem GetItem(int position)
        {
            return SocialList[position];
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
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(SocialLinksAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(SocialLinksAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class SocialLinksAdapterViewHolder : RecyclerView.ViewHolder
    {
        public SocialLinksAdapterViewHolder(View itemView, Action<SocialLinksAdapterClickEventArgs> clickListener,Action<SocialLinksAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                IconSocial = MainView.FindViewById<TextView>(Resource.Id.Social_Icon);
                NameSocial = MainView.FindViewById<TextView>(Resource.Id.Social_name);
                AddLink = MainView.FindViewById<TextView>(Resource.Id.AddLinkText);
                NameLink = MainView.FindViewById<TextView>(Resource.Id.Link_name);
                 
                itemView.Click += (sender, e) => clickListener(new SocialLinksAdapterClickEventArgs{ View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SocialLinksAdapterClickEventArgs{ View = itemView, Position = BindingAdapterPosition }); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }


        public TextView IconSocial { get; private set; } 
        public TextView NameSocial { get; private set; }
        public TextView NameLink { get; private set; }
        public TextView AddLink { get; private set; }

        #endregion
    }

    public class SocialLinksAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}