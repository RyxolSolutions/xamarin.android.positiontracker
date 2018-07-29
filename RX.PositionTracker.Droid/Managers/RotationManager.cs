using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RX.PositionTracker.Droid.ViewModels;

namespace RX.PositionTracker.Droid.Managers
{
    public delegate void RotationDataChangedHandler();

    public class RotationManager : Java.Lang.Object, ISensorEventListener
    {
        static readonly object _syncLock = new object();

        public SensorManager SensorManager { get; set; }
        public static event RotationDataChangedHandler RotationDataChanged;
        public AccelerometerDataViewModel CurrentSensorData { get; private set; }

        #region Singletone

        private static RotationManager instance;

        private RotationManager()
        {
        }

        public static RotationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RotationManager();
                }
                return instance;
            }
        }

        #endregion

        #region Public Methods

        public void StartRotationTracking()
        {
            SensorManager.RegisterListener(this, SensorManager.GetDefaultSensor(SensorType.RotationVector), SensorDelay.Normal);
        }

        public void StopRotationTracking()
        {
            SensorManager.UnregisterListener(this);
        }

        #endregion

        #region ISensorEventListener Implementation

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // We don't want to do anything here.
        }

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                var model = VMManager.ToAccelerometerDataViewModel(e);

                CurrentSensorData = model;

                StopRotationTracking();

                RotationDataChanged?.Invoke();
            }
        }

        #endregion
    }
}