using System;
using AirWatchSDK;
using CoreGraphics;
using Foundation;
using UIKit;

namespace AWTest21.iOS
{
    public sealed class AirWatchSDKManager : AWSDKDelegate
    {
        private const string LogCategory = nameof(AirWatchSDKManager);

        private static readonly Lazy<AirWatchSDKManager>
            _airWatchSDKManager =
            new Lazy<AirWatchSDKManager>(() => new AirWatchSDKManager());

        public static AirWatchSDKManager Instance => _airWatchSDKManager.Value;

        private AirWatchSDKManager()
        { }

        #region Properties

        public AWProfile SdkProfile { get; private set; }
        public bool InitialCheckDone { get; private set; }
        public bool RecievedProfiles { get; private set; }

        #endregion

        #region Overloads

        public override void InitialCheckDoneWithError(NSError error)
        {
            LogMessage($"AWXamarin InitialCheckDoneWithError received {error}, SDK initialized without error");

            //Post notification if error occurs, to remove the loader. In case of no error, loader will be removed after profiles are recieved.
            if (error != null)
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(AWConstants.InitialCheckDoneWithErrorNotification, null);
            }
            else
            {
                InitialCheckDone = true;
            }
        }

        public override void Lock()
        {
            // Add any SDK Customization here
            LogMessage("AWXamarin Lock command received");
        }

        public override void ReceivedProfiles(AWProfile[] profiles)
        {
            RecievedProfiles = true;
            LogMessage($"AWXamarin ReceivedProfiles received {profiles}");

            NSNotificationCenter.DefaultCenter.PostNotificationName(AWConstants.ReceivedProfilesNotification, null);

            foreach (var profile in profiles)
            {
                if (profile.ProfileType == AWProfileType.SDKProfile)
                {
                    SdkProfile = profile;
                }
            }
        }

        public override void ResumeNetworkActivity()
        {
            // Add any SDK Customization here
            LogMessage("AWXamarin ResumeNetworkActivity received");
        }

        public override void StopNetworkActivity(AWNetworkActivityStatus networkActivityStatus)
        {
            // Add any SDK Customization here
            LogMessage($"AWXamarin StopNetworkActivity received {networkActivityStatus}");
        }

        public override void Unlock()
        {
            // Add any SDK Customization here
            LogMessage("AWXamarin Unlock command received");
        }

        public override void Wipe()
        {
            // Add any SDK Customization here
            LogMessage("AWXamarin Wipe command received");
        }

        #endregion

        #region Methods

        public bool DLPEnabled()
        {
            bool DLPPersmission = false;
            AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
            if (SdkProfile != null && restrictionsPayload != null)
            {
                DLPPersmission = restrictionsPayload.EnableDataLossPrevention;
            }
            return DLPPersmission;

        }

        // Returns boolean signifying if copy paste operation is allowed or not. Can be configured under DLP Settings in Settings
        public bool AllowCopyPaste()
        {
            bool copyPastePermission = false;
            if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
            {
                AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
                copyPastePermission = !restrictionsPayload.PreventCopyAndCut;
            }
            return copyPastePermission;
        }

        // Returns boolean signifying if document opening is allowed in specific apps or not. If checked then list of allowed apps can be obtained from allowedApplicationsList()
        public bool RestrictDocumentToApps()
        {
            bool restrictApps = false;
            if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
            {
                AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
                restrictApps = restrictionsPayload.RestrictDocumentToApps;
            }
            return restrictApps;
        }

        // Returns a list of allowed application list for document opening. Can be configured under DLP Settings in Settings.
        public NSArray AllowedApplicationsList()
        {
            NSArray allowedApps = null;
            if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
            {
                AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
                allowedApps = restrictionsPayload.AllowedApplications;
            }
            return allowedApps;
        }

        // Returns boolean signifying if camera access is allowed or not. Can be configured under DLP Settings in Settings.
        public bool AllowCamera()
        {
            bool cameraAccess = false;
            if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
            {
                AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
                cameraAccess = restrictionsPayload.EnableCameraAccess;
            }
            return cameraAccess;
        }

        // Returns boolean signifying if Watermark is allowed or not. 
        public bool AllowWatermark()
        {
            bool watermarkEnabled = false;
            if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
            {
                AWRestrictionsPayload restrictionsPayload = SdkProfile.RestrictionsPayload;
                watermarkEnabled = restrictionsPayload.EnableWatermark;
            }
            return watermarkEnabled;
        }

        //Returns a string containing Custom settings. Can be set while creating a custom profile which can be assigned as SDK profile.
        public string CustomSettings()
        {
            if (SdkProfile != null && SdkProfile.CustomPayload != null)
            {
                AWCustomPayload customPayload = SdkProfile.CustomPayload;
                return customPayload.Settings;
            }
            return string.Empty;
        }

        // Returns Boolean signifying if Integrated Authentication is allowed or not.
        public bool AllowIntegratedAuthentication()
        {
            bool iaAllowed = false;
            if (SdkProfile != null && SdkProfile.AuthenticationPayload != null)
            {
                AWAuthenticationPayload authPayload = SdkProfile.AuthenticationPayload;
                iaAllowed = authPayload.EnableIntegratedAuthentication;
            }
            return iaAllowed;
        }

        // Returns the list of allowed sites for Integrated Authentication. Can be configured under Integrated Authentication settings.
        public NSArray AllowedSitesList()
        {
            NSArray allowedSites = null;
            if (SdkProfile != null && SdkProfile.AuthenticationPayload != null)
            {
                allowedSites = SdkProfile.AuthenticationPayload.AllowedSites;
            }
            return allowedSites;
        }

        public void OpenDocumentFromUrl(NSUrl fileUrl, UIView view)
        {
            if (fileUrl != null && view != null)
            {
                AWDocumentInteractionController documentInteractionController = AWDocumentInteractionController.InteractionControllerWithURL(fileUrl);
                if (SdkProfile != null && SdkProfile.RestrictionsPayload != null)
                {
                    documentInteractionController.AllowedApps = Instance.AllowedApplicationsList();
                }
                CGRect frame = new CGRect(0, 0, 0, 0);
                documentInteractionController.PresentOpenInMenuFromRect(frame, view, true);
            }
            else
            {
                LogMessage("AWXamarin file opening URL or View is null");
            }
        }

        public void OpenDocumentFromFile(string fileName, string fileExtension)
        {
            if (fileName != null && fileExtension != null)
            {
                NSUrl fileURL = NSBundle.MainBundle.GetUrlForResource(fileName, fileExtension);
                if (UIDevice.CurrentDevice.AW_osVersionMajor() >= 11)
                {
                    fileURL = MoveItemToDocumentsDirectory(fileName, fileExtension);
                }

                var window = UIApplication.SharedApplication.KeyWindow;
                var view = window.RootViewController.View;
                OpenDocumentFromUrl(fileURL, view);
            }
            else
            {
                LogMessage("AWXamarin file name or extension not valid");
            }
        }

        public NSUrl MoveItemToDocumentsDirectory(string fileName, string fileExtension)
        {
            NSFileManager fileManager = NSFileManager.DefaultManager;

            string[] dataPath = { ApplicationDocumentsDirectory() + "/" + fileName + "." + fileExtension };

            NSUrl fileURLPrivate = NSBundle.MainBundle.GetUrlForResource(fileName, fileExtension);

            if (fileManager.FileExists(fileURLPrivate.Path))
            {
                //First run, if file is not copied then copy, else return the path if already copied
                if (fileManager.FileExists(dataPath[0]) == false)
                {
                    fileManager.Copy(fileURLPrivate.Path, dataPath[0], out NSError error);
                    if (error == null)
                    {
                        return NSUrl.CreateFileUrl(dataPath);
                    }
                    LogMessage("AWXamarin Error occured while copying");
                }
                else
                {
                    return NSUrl.CreateFileUrl(dataPath);
                }

                LogMessage("AWXamarin fileURLPrivate doesnt exist");
                return null;
            }
            return null;
        }

        public string ApplicationDocumentsDirectory()
        {
            // Get the documents directory
            string dirPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true)[0];
            return dirPath;
        }

        private void LogMessage(string message)
        {
            Console.WriteLine(message, LogCategory);
        }

        #endregion
    }
}
