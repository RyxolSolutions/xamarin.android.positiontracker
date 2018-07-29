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

namespace RX.PositionTracker.Droid.Managers
{
    public static class SessionManager
    {
        public static long? IMEI { get; set; }
    }
}