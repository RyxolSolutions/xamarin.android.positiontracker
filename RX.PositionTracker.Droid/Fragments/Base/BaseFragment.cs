using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace RX.PositionTracker.Droid.Fragments.Base
{
    public abstract class BaseFragment : Fragment
    {
        protected View _partial;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _partial = inflater.Inflate(GetLayoutId(), container, false);

            return _partial;
        }

        protected abstract int GetLayoutId();

        protected View GetPartialView()
        {
            return _partial;
        }
    }
}