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
    public class DeviceInfoFragment : BaseFragment
    {
        TextView textIMEI;
        TextView textGetLinearAcceleration;
        TextView textGetGravity;
        TextView textGetRotationVector;

        string getTitle = "GET";
        string noTitle = "NO";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            View partial = base.OnCreateView(inflater, container, savedInstanceState);

            textIMEI = partial.FindViewById<TextView>(Resource.Id.textIMEI);
            textGetLinearAcceleration = partial.FindViewById<TextView>(Resource.Id.textGetLinearAcceleration);
            textGetGravity = partial.FindViewById<TextView>(Resource.Id.textGetGravity);
            textGetRotationVector = partial.FindViewById<TextView>(Resource.Id.textGetRotationVector);

            InitControls();

            return partial;
        }

        void InitControls()
        {
            textIMEI.Text = SessionManager.IMEI?.ToString() ?? "NOT IDENTIFIED";

            CheckDeviceSensors();
        }

        void CheckDeviceSensors()
        {
            var sensor = (SensorManager)this.Activity.GetSystemService(Context.SensorService);
            if (sensor.GetDefaultSensor(SensorType.LinearAcceleration) != null)
            {
                textGetLinearAcceleration.Text = getTitle;
            }
            else
            {
                textGetLinearAcceleration.Text = noTitle;
            }
            if (sensor.GetDefaultSensor(SensorType.Gravity) != null)
            {
                textGetGravity.Text = getTitle;
            }
            else
            {
                textGetGravity.Text = noTitle;
            }
            if (sensor.GetDefaultSensor(SensorType.RotationVector) != null)
            {
                textGetRotationVector.Text = getTitle;
            }
            else
            {
                textGetRotationVector.Text = noTitle;
            }
        }

        #region Override Methods

        protected override int GetLayoutId()
        {
            return Resource.Layout.fragment_device_info;
        }

        #endregion
    }
}