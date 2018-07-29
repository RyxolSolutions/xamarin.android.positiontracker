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

namespace RX.PositionTracker.Droid.Enums
{
    public enum PartialType : int
    {
        Unknown = 0,
        CurrentValues = 1,
        DeviceInfo = 2
    }
}