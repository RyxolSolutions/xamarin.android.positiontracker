using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RX.PositionTracker.Droid.ViewModels
{
    public class AccelerometerDataViewModel
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public DateTime Time { get; set; }

        public float Feet { get; set; }
    }
}