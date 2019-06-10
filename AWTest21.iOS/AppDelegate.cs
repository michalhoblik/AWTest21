using System.Diagnostics;
using AirWatchSDK;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace AWTest21.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            SetupAirWatchThings();
            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void SetupAirWatchThings()
        {
            if (Runtime.Arch == Arch.SIMULATOR)
            {
                Debug.WriteLine("Running in Simulator, skipping initialization of the AirWatch SDK!");
                return;
            }
            else
            {
                Debug.WriteLine("Running on Device, beginning initialization of the AirWatch SDK.");

                // Configure Controller:
                var sdkController = AWController.ClientInstance();
                // 1> defining the callback scheme so the app can get callback back
                sdkController.CallbackScheme = "awtest21scheme";
                // 2> set the delegate to know when the initialization has been completed
                sdkController.Delegate = AirWatchSDKManager.Instance;
            }
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            AWController.ClientInstance().Start();
        }

        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            return AWController.ClientInstance().HandleOpenURL(url, "");
        }

        [Export("window")]
        public UIWindow GetWindow()
        {
            return UIApplication.SharedApplication.Windows[0];
        }
    }
}
