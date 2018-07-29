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
using RX.PositionTracker.Droid.Services;

namespace RX.PositionTracker.Droid
{
    [Application]
    public class AppWrapper : Application
    {
        public static Intent ServiceIntent { get; set; }

        public static MotionService Service { get; set; }

        public AppWrapper(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}