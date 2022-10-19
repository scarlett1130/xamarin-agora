using Android.OS;
using Android.Runtime;
using System;
using System.Collections.ObjectModel;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using Newtonsoft.Json;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace WoWonder.Activities.ReelsVideo.Adapters
{
    public class ReelsVideoPagerAdapter : FragmentStateAdapter
    {
        private int CountVideo;
        private ObservableCollection<PostDataObject> DataVideos;
         
        public ReelsVideoPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ReelsVideoPagerAdapter(Fragment fragment) : base(fragment)
        {
        }

        public ReelsVideoPagerAdapter(FragmentActivity fragmentActivity) : base(fragmentActivity)
        {
        }

        public ReelsVideoPagerAdapter(FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
        {
        }

        public ReelsVideoPagerAdapter(FragmentActivity fragmentActivity, int size, ObservableCollection<PostDataObject> dataVideos) : base(fragmentActivity)
        {
            try
            {
                CountVideo = size;
                DataVideos = dataVideos; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        public void UpdateReelsVideoPager(int size, ObservableCollection<PostDataObject> dataVideos) 
        {
            try
            {
                CountVideo = size;
                DataVideos = dataVideos; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        public override int ItemCount => CountVideo;

        public override Fragment CreateFragment(int position)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutInt("position", position);

                PostDataObject dataItem = DataVideos[position];
                if (dataItem != null)
                    bundle.PutString("DataItem", JsonConvert.SerializeObject(dataItem));

                ViewReelsVideoFragment viewReelsVideoFragment = new ViewReelsVideoFragment { Arguments = bundle };
                return viewReelsVideoFragment;
            }
            catch (Exception a)
            {
                Methods.DisplayReportResultTrack(a);
                return null!;
            }
        } 
    }
}