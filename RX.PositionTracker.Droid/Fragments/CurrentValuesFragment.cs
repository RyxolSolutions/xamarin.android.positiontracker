using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using RX.PositionTracker.Droid.Fragments.Base;
using RX.PositionTracker.Droid.Managers;

namespace RX.PositionTracker.Droid.Fragments
{
    public class CurrentValuesFragment : BaseFragment
    {
        TextView textCurrentSpeedTitle;
        TextView textCurrentSpeed;
        TextView textCurrentDistanceTitle;
        TextView textCurrentDistance;
        TextView textCoordinatesTitle;
        TextView textLatitudeTitle;
        TextView textLatitude;
        TextView textLongitudeTitle;
        TextView textLongitude;
        TextView textAccelerometerTitle;
        TextView textAccelerometerX;
        TextView textAccelerometerY;
        TextView textAccelerometerZ;
        TextView textAltitudeTitle;
        TextView textAltitude;
        Button buttonSubmit;
        Button buttonCalibrate;
        ProgressBar progressBarCalibration;

        bool IsStarted = false;
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View partial = base.OnCreateView(inflater, container, savedInstanceState);

            textCurrentSpeedTitle = partial.FindViewById<TextView>(Resource.Id.textCurrentSpeedTitle);
            textCurrentSpeed = partial.FindViewById<TextView>(Resource.Id.textCurrentSpeed);
            textCurrentDistanceTitle = partial.FindViewById<TextView>(Resource.Id.textCurrentDistanceTitle);
            textCurrentDistance = partial.FindViewById<TextView>(Resource.Id.textCurrentDistance);
            textCoordinatesTitle = partial.FindViewById<TextView>(Resource.Id.textCoordinatesTitle);
            textLatitudeTitle = partial.FindViewById<TextView>(Resource.Id.textLatitudeTitle);
            textLatitude = partial.FindViewById<TextView>(Resource.Id.textLatitude);
            textLongitudeTitle = partial.FindViewById<TextView>(Resource.Id.textLongitudeTitle);
            textLongitude = partial.FindViewById<TextView>(Resource.Id.textLongitude);
            textAccelerometerTitle = partial.FindViewById<TextView>(Resource.Id.textAccelerometerTitle);
            textAccelerometerX = partial.FindViewById<TextView>(Resource.Id.textAccelerometerX);
            textAccelerometerY = partial.FindViewById<TextView>(Resource.Id.textAccelerometerY);
            textAccelerometerZ = partial.FindViewById<TextView>(Resource.Id.textAccelerometerZ);
            textAltitudeTitle = partial.FindViewById<TextView>(Resource.Id.textAltitudeTitle);
            textAltitude = partial.FindViewById<TextView>(Resource.Id.textAltitude);
            buttonSubmit = partial.FindViewById<Button>(Resource.Id.buttonStart);
            buttonCalibrate = partial.FindViewById<Button>(Resource.Id.buttonCalibrate);
            progressBarCalibration = partial.FindViewById<ProgressBar>(Resource.Id.progressBarCalibration);

            InitControls();

            return partial;
        }

        protected void InitControls()
        {
            if (AccelerometerManager.Instance.CurrentSensorData != null)
            {
                textAccelerometerX.Text = AccelerometerManager.Instance.CurrentSensorData.X.ToString();
                textAccelerometerY.Text = AccelerometerManager.Instance.CurrentSensorData.Y.ToString();
                textAccelerometerZ.Text = AccelerometerManager.Instance.CurrentSensorData.Z.ToString();
            }

            if (LocationDataManager.Instance.CurrentLocation != null)
            {
                textLatitude.Text = LocationDataManager.Instance.CurrentLocation.Latitude.ToString();
                textLongitude.Text = LocationDataManager.Instance.CurrentLocation.Longitude.ToString();
                textAltitude.Text = LocationDataManager.Instance.Altitude.ToString();
                textCurrentSpeed.Text = LocationDataManager.Instance.Speed.ToString();

                if (LocationDataManager.Instance.CurrentDistance > 0)
                {
                    textCurrentDistance.Text = Math.Round(LocationDataManager.Instance.CurrentDistance, 2).ToString();
                }
            }

            RotationManager.Instance.SensorManager = (SensorManager)this.Activity.GetSystemService(Context.SensorService);
            if (RotationManager.Instance.SensorManager.GetDefaultSensor(SensorType.LinearAcceleration) != null
                   && RotationManager.Instance.SensorManager.GetDefaultSensor(SensorType.RotationVector) != null)
            {
                buttonCalibrate.Visibility = ViewStates.Visible;
            }
            else
            {
                buttonCalibrate.Visibility = ViewStates.Gone;
            }

            LocationDataManager.LocationAddressChanged += LocationDataManager_LocationAddressChanged;
            AccelerometerManager.AccelerometerDataChanged += AccelerometerManager_AccelerometerDataChanged;
            RotationManager.RotationDataChanged += RotationManager_CalibrationDataChanged;

            SetupGestures();
        }

        void SetupGestures()
        {
            buttonSubmit.Click += delegate
            {
                if (!IsStarted)
                {
                    (this.Activity as MainActivity).StartTracking();

                    buttonSubmit.Text = "STOP TRACKING";
                    IsStarted = true;
                }
                else
                {
                    (this.Activity as MainActivity).StopTracking();

                    buttonSubmit.Text = "START TRACKING";
                    textCurrentDistance.Text = "0";
                    IsStarted = false;
                }
            };

            buttonCalibrate.Click += async delegate
            {
                buttonCalibrate.Visibility = ViewStates.Gone;
                progressBarCalibration.Indeterminate = true;
                progressBarCalibration.Visibility = ViewStates.Visible;

                RotationManager.Instance.StartRotationTracking();
            };
        }

        void LocationDataManager_LocationAddressChanged()
        {
            if (this.Activity != null)
            {
                this.Activity.RunOnUiThread(() =>
                {
                    textLatitude.Text = LocationDataManager.Instance.CurrentLocation.Latitude.ToString();
                    textLongitude.Text = LocationDataManager.Instance.CurrentLocation.Longitude.ToString();
                    textAltitude.Text = LocationDataManager.Instance.Altitude.ToString();
                    textCurrentSpeed.Text = LocationDataManager.Instance.Speed.ToString();

                    if (LocationDataManager.Instance.CurrentDistance > 0)
                    {
                        textCurrentDistance.Text = Math.Round(LocationDataManager.Instance.CurrentDistance, 2).ToString();
                    }
                });
            }
        }

        void AccelerometerManager_AccelerometerDataChanged()
        {
            if (this.Activity != null)
            {
                this.Activity.RunOnUiThread(() =>
                {
                    textAccelerometerX.Text = AccelerometerManager.Instance.CurrentSensorData.X.ToString();
                    textAccelerometerY.Text = AccelerometerManager.Instance.CurrentSensorData.Y.ToString();
                    textAccelerometerZ.Text = AccelerometerManager.Instance.CurrentSensorData.Z.ToString();
                });
            }
        }

        void RotationManager_CalibrationDataChanged()
        {
            if (this.Activity != null)
            {
                this.Activity.RunOnUiThread(() =>
                {
                    Toast.MakeText(this.Activity,"Calibration Done", ToastLength.Short).Show();
                    progressBarCalibration.Visibility = ViewStates.Gone;
                    buttonCalibrate.Visibility = ViewStates.Visible;
                });
            }
        }

        #region Override Methods

        protected override int GetLayoutId()
        {
            return Resource.Layout.fragment_current_values;
        }

        #endregion
    }
}
