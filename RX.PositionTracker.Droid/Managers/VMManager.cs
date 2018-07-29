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
    public static class VMManager
    {
        public static AccelerometerDataViewModel ToAccelerometerDataViewModel(SensorEvent value)
        {
            var model = new AccelerometerDataViewModel();

            model.X = Convert.ToSingle(Math.Round(value.Values[0], 2));
            model.Y = Convert.ToSingle(Math.Round(value.Values[1], 2));
            model.Z = Convert.ToSingle(Math.Round(value.Values[2], 2));

            model.Time = DateTime.UtcNow;

            return model;
        }
    }
}