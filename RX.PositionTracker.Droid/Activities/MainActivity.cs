using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using static Android.Support.V4.App.ActivityCompat;
using RX.PositionTracker.Droid.Fragments;
using Android.Graphics;
using Android.Support.V4.Content;
using RX.PositionTracker.Droid.Enums;
using RX.PositionTracker.Droid.Managers;
using Android.Hardware;
using Android.Content;
using Android.Gms.Location;
using RX.PositionTracker.Droid.Services;
using Android.Gms.Common;
using RX.PositionTracker.Droid.Helpers;

namespace RX.PositionTracker.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.SensorPortrait,
    LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = (ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Locale))]
    public class MainActivity : AppCompatActivity, IResultCallback, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, IOnRequestPermissionsResultCallback
    {
        CurrentValuesFragment currentValuesFragment;
        DeviceInfoFragment deviceInfoFragment;

        LinearLayout selectorCurrentValuesTab;
        LinearLayout selectorDeviceInfoTab;
        LinearLayout layoutTabSelectors;
        LinearLayout layoutTabs;
        Button buttonCurrentValuesTab;
        Button buttonDeviceInfoTab;
        FloatingActionButton fabInfo;

        Color selectedColor;
        Color notSelectedColor;

        IMenu Menu;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            layoutTabSelectors = FindViewById<LinearLayout>(Resource.Id.layoutTabSelectors);
            selectorCurrentValuesTab = FindViewById<LinearLayout>(Resource.Id.selectorCurrentValuesTab);
            selectorDeviceInfoTab = FindViewById<LinearLayout>(Resource.Id.selectorDeviceInfoTab);
            layoutTabs = FindViewById<LinearLayout>(Resource.Id.layoutTabs);
            buttonCurrentValuesTab = FindViewById<Button>(Resource.Id.buttonCurrentValuesTab);
            buttonDeviceInfoTab = FindViewById<Button>(Resource.Id.buttonDeviceInfoTab);
            fabInfo = FindViewById<FloatingActionButton>(Resource.Id.fabInfo);

            selectedColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            notSelectedColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorPrimary));

            currentValuesFragment = new CurrentValuesFragment();
            deviceInfoFragment = new DeviceInfoFragment();

            InitControls();
        }

        protected void InitControls()
        {
            if (!IsFinishing)
            {
                var partialSetup = SupportFragmentManager.BeginTransaction();
                partialSetup.Add(Resource.Id.fragmentContainer, deviceInfoFragment, "DeviceInfoFragment");
                partialSetup.Hide(deviceInfoFragment);
                partialSetup.Add(Resource.Id.fragmentContainer, currentValuesFragment, "CurrentValuesFragment");
                partialSetup.CommitAllowingStateLoss();
            }

            SetupGestures();

            RegisterDevice();
            StartMotionService();

            LocationDataManager.LocationAddressChanged += LocationDataManager_LocationChanged;
        }

        void SetupGestures()
        {
            fabInfo.Click += FabOnClick;

            buttonCurrentValuesTab.Click += delegate
            {
                selectorCurrentValuesTab.Visibility = ViewStates.Visible;
                selectorDeviceInfoTab.Visibility = ViewStates.Invisible;

                ShowFragment(PartialType.CurrentValues);
            };

            buttonDeviceInfoTab.Click += delegate
            {
                selectorCurrentValuesTab.Visibility = ViewStates.Invisible;
                selectorDeviceInfoTab.Visibility = ViewStates.Visible;

                ShowFragment(PartialType.DeviceInfo);
            };
        }

        private void ShowFragment(PartialType type)
        {
            var partialSetup = SupportFragmentManager.BeginTransaction();

            if (type == PartialType.CurrentValues)
            {
                partialSetup.Hide(deviceInfoFragment);
                partialSetup.AddToBackStack(null);

                currentValuesFragment = new CurrentValuesFragment();
                partialSetup.Replace(Resource.Id.fragmentContainer, currentValuesFragment, "CurrentValuesFragment");
                partialSetup.Show(currentValuesFragment);
            }
            else if (type == PartialType.DeviceInfo)
            {
                partialSetup.Hide(currentValuesFragment);
                partialSetup.AddToBackStack(null);

                deviceInfoFragment = new DeviceInfoFragment();
                partialSetup.Replace(Resource.Id.fragmentContainer, deviceInfoFragment, "DeviceInfoFragment");
                partialSetup.Show(deviceInfoFragment);
            }

            if (!IsFinishing)
            {
                partialSetup.CommitAllowingStateLoss();
            }
        }

        void RegisterDevice()
        {
            if (!SessionManager.IMEI.HasValue)
            {
                SessionManager.IMEI = DeviceInfoHelper.GetDeviceUniqID();
            }
        }

        #region Action Menu

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            if (menu != null)
            {
                Menu = menu;
                menu.FindItem(Resource.Id.action_exit).SetTitle("Exit");
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_exit:
                    ShowExitDialog();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        #endregion

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Contact us at dev@ryxol.com", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public void StartTracking()
        {
            if (IsGooglePlayServicesInstalled())
            {
                if (LocationDataManager.Instance.IsGoogleApiClientEmpty())
                {
                    ConnectToGooglePlayServicesClient();
                }
            }
            else
            {
                ShowInstallGooglePlayServicesDialog();
            }
        }

        public void StopTracking()
        {
            LocationDataManager.Instance.StopLocationRequest();
            AccelerometerManager.Instance.StopAccelerometerTracking();
            AccelerometerManager.Instance.SensorManager = null;

            LocationDataManager.Instance.DisconnectGoogleApiClient();
            LocationDataManager.Instance._apiClient = null;
        }

        void LocationDataManager_LocationChanged()
        {
            if (AccelerometerManager.Instance.SensorManager == null)
            {
                AccelerometerManager.Instance.SensorManager = (SensorManager)GetSystemService(Context.SensorService);

                if (AccelerometerManager.Instance.SensorManager.GetDefaultSensor(SensorType.LinearAcceleration) != null
                    && AccelerometerManager.Instance.SensorManager.GetDefaultSensor(SensorType.RotationVector) != null)
                {
                    AccelerometerManager.Instance.StartAccelerometerTracking();

                }
                else
                {
                    if (AppWrapper.Service != null)
                    {
                        AppWrapper.Service.OnlyGPS = true;
                    }

                    ShowNoSensorsDialog();
                }
            }
        }

        void SyncData_Start()
        {
            //CUSTOMCODE: make some job
        }

        void SyncData_Stop()
        {
            //CUSTOMCODE: make some job
        }

        #region Dialogs 

        protected void ShowExitDialog()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Exit");
            alert.SetMessage("Stop tracking and close application?");
            alert.SetPositiveButton("Yes", (senderAlert, args) =>
            {
                try
                {
                    if (AppWrapper.ServiceIntent != null)
                        StopService(AppWrapper.ServiceIntent);

                    AppWrapper.ServiceIntent = null;

                    System.Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Exit operation error: " + ex.ToString(), ToastLength.Short).Show();
                }
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
            });

            Android.App.Dialog dialog = alert.Create();
            dialog.Show();
        }

        protected void ShowNoSensorsDialog()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("No sensors");
            alert.SetMessage("Device sensors invalid");
            alert.SetPositiveButton("Accept", (senderAlert, args) =>
            {
            });

            Android.App.Dialog dialog = alert.Create();
            dialog.Show();
        }

        public void ShowInstallGooglePlayServicesDialog()
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Google Play Services");
            alert.SetMessage("Google Play Services not installed.");
            alert.SetPositiveButton("Cancel", (senderAlert, args) =>
            {
            });

            Android.App.Dialog dialog = alert.Create();
            dialog.Show();
        }

        #endregion

        #region Location Data

        GoogleApiClient _apiClient;
        LocationRequest _locRequest;

        void ConnectToGooglePlayServicesClient()
        {
            if (_apiClient == null)
                _apiClient = new GoogleApiClient.Builder(this)
                    .AddApi(LocationServices.API)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .Build();

            if (!_apiClient.IsConnected)
                _apiClient.Connect();
        }

        void BuildLocationRequest()
        {
            _locRequest = new LocationRequest();
            _locRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            _locRequest.SetFastestInterval(1 * 1000);
            _locRequest.SetInterval(2 * 1000);

            int sdk = (int)Android.OS.Build.VERSION.SdkInt;
            if (sdk < 23 ||
                (this.CheckCallingOrSelfPermission(Android.Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                    this.CheckCallingOrSelfPermission(Android.Manifest.Permission.AccessCoarseLocation) == Permission.Granted))
            {
                LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder().AddLocationRequest(_locRequest);
                builder.SetAlwaysShow(true);

                var result = LocationServices.SettingsApi.CheckLocationSettings(_apiClient, builder.Build());
                result.SetResultCallback(this);
            }
            else
            {
                Android.Support.V4.App.ActivityCompat.RequestPermissions(this, new System.String[] { Android.Manifest.Permission.AccessFineLocation, Android.Manifest.Permission.AccessCoarseLocation }, 1);
            }
        }

        void StartMotionService()
        {
            if (AppWrapper.ServiceIntent == null)
            {
                AppWrapper.ServiceIntent = new Intent(this, typeof(MotionService));
                StartService(AppWrapper.ServiceIntent);
            }
        }

        void RequestLocationUpdates()
        {
            LocationDataManager.Instance.SetGoogleApiClient(_apiClient, _locRequest);
            LocationDataManager.Instance.RequestLocation();
        }

        #region IOnRequestPermissionsResultCallback Implementation

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            if (CheckCallingOrSelfPermission(Android.Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                    CheckCallingOrSelfPermission(Android.Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
            {
                LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder().AddLocationRequest(_locRequest);
                var result = LocationServices.SettingsApi.CheckLocationSettings(_apiClient, builder.Build());
                result.SetResultCallback(this);
            }
        }

        #endregion

        #region GoogleApiClient Implementation

        public void OnConnected(Bundle connectionHint)
        {
            BuildLocationRequest();
        }

        public void OnConnectionFailed(ConnectionResult bundle)
        {
        }

        public void OnConnectionSuspended(int cause)
        {
        }

        #endregion

        #region IResultCallback Implementation

        const int requestCheckSettings = 2002;
        public void OnResult(Java.Lang.Object result)
        {
            var locationSettingsResult = result as LocationSettingsResult;

            Statuses status = locationSettingsResult.Status;
            switch (status.StatusCode)
            {
                case CommonStatusCodes.Success:
                    RequestLocationUpdates();
                    break;
                case CommonStatusCodes.ResolutionRequired:
                    try
                    {
                        status.StartResolutionForResult(this, requestCheckSettings);
                    }
                    catch (IntentSender.SendIntentException)
                    {
                    }
                    break;
                case LocationSettingsStatusCodes.SettingsChangeUnavailable:
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == requestCheckSettings)
            {
                if (resultCode == Result.Ok)
                {
                    RequestLocationUpdates();
                }
                else
                {
                    Toast.MakeText(this, "Please enable GPS", ToastLength.Long).Show();
                }
            }
        }

        #endregion

        #endregion

        bool IsGooglePlayServicesInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
            }
            return false;
        }
    }
}

