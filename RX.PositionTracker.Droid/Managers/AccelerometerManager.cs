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
    public delegate void AccelerometerDataChangedHandler();

    public class AccelerometerManager : Java.Lang.Object, ISensorEventListener
    {
        static readonly object _syncLock = new object();

        public SensorManager SensorManager { get; set; }
        public AccelerometerDataViewModel CurrentSensorData { get; private set; }

        public static event AccelerometerDataChangedHandler AccelerometerDataChanged;

        #region Singletone

        private static AccelerometerManager instance;

        private AccelerometerManager()
        {
        }

        public static AccelerometerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccelerometerManager();
                }
                return instance;
            }
        }

        #endregion

        #region Public Methods

        public void StartAccelerometerTracking()
        {
            currentInterval = -1;

            SensorManager.RegisterListener(this, SensorManager.GetDefaultSensor(SensorType.LinearAcceleration), SensorDelay.Normal);
        }

        public void StopAccelerometerTracking()
        {
            if (SensorManager != null)
            {
                SensorManager.UnregisterListener(this);
            }
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

                if (CurrentSensorData != null)
                {
                    if (currentInterval != DateTime.UtcNow.Second)
                    {
                        currentInterval = DateTime.UtcNow.Second;

                        if (packetData.Count > 0)
                        {
                            packetData = new List<AccelerometerDataViewModel>();
                        }
                    }
                    else
                    {
                        packetData.Add(model);
                    }
                }
                else
                {
                    currentInterval = DateTime.UtcNow.Second;
                    packetData.Add(model);
                }

                CurrentSensorData = model;

                AccelerometerDataChanged?.Invoke();
            }
        }

        #endregion

        int currentInterval = -1;

        List<AccelerometerDataViewModel> packetData = new List<AccelerometerDataViewModel>();

        #region Calculation

        void SaveSensorDataToDB()
        { 
            AccelerometerDataChanged?.Invoke();
        }

        #endregion
    }
}