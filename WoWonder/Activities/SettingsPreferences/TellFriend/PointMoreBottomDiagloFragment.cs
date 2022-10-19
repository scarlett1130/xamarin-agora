using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Google.Android.Material.BottomSheet;
using System;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.SettingsPreferences.TellFriend
{
    public class PointMoreBottomDiagloFragment : BottomSheetDialogFragment, IDialogInterfaceOnShowListener
    {
        private Activity MainContext;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                MainContext = Activity;
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.point_more_bottom, container, false);
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

                Dialog.SetOnShowListener(this);

                InitComponent(view);
                AddOrRemoveEvent(true);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitComponent(View view)
        {
            try
            {
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
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnShow(IDialogInterface dialog)
        {
            /*var d = dialog as BottomSheetDialog;
            var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
            var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
            bottomSheetBehavior.State = BottomSheetBehavior.StateCollapsed;
            bottomSheetBehavior.PeekHeight = 400;*/
        }
    }
}