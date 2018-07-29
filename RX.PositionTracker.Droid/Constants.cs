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

namespace RX.PositionTracker.Droid
{
    public static class Constants
    {
        public const string APPTITLE = "Position Tracker";
        public const double MILE_TO_KM_MULTIPLIER = 0.621371;
        public const double METERS_TO_KM_MULTIPLIER = 1000;
        public const double TIME_MULTIPLIER = 3600;
        public const double GRAVITATIONAL_ACCELERATION = 9.8066;
        public const int DEFAULT_SYNC_PERIOD = 60;
    }
}