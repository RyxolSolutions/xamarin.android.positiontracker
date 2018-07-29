using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using RX.PositionTracker.Droid.Managers;
using static Android.OS.PowerManager;

namespace RX.PositionTracker.Droid.Services
{
    public delegate void SyncDataStartedHandler();
    public delegate void SyncDataStopedHandler();

    [Service(Icon = "@drawable/ic_stat_onesignal_default", Label = "@string/app_name")]
    public class MotionService : Service
    {
        private const int NotificationId = 1;

        WakeLock wl;
        Timer dataSyncTimer;
        long dataSyncDelay = (long)TimeSpan.FromSeconds(Constants.DEFAULT_SYNC_PERIOD).TotalMilliseconds;

        public MainActivity Activity;

        public bool SynchronizationApproved = true;
        public bool OnlyGPS = false;

        public static event SyncDataStartedHandler SyncDataStarted;
        public static event SyncDataStopedHandler SyncDataStoped;

        public override void OnCreate()
        {
            base.OnCreate();
            AppWrapper.Service = this;

            Init();
        }

        void Init()
        {
            dataSyncTimer = new System.Threading.Timer(new TimerCallback(DataSyncHandler), null, dataSyncDelay, Timeout.Infinite);

            StartForeground();
        }

        void DataSyncHandler(object o)
        {
            try
            {
                if (!SynchronizationApproved)
                    return;

                SyncDataStarted?.Invoke();

                //CUSTOMCODE: Add your sync request for data sending here

                SyncDataStoped?.Invoke();
            }
            catch (Exception ex)
            {
                //CUSTOMCODE: Add your logger here
            }

            ReactivateServiceSyncPeriod();

            return;
        }

        public void ReactivateServiceSyncPeriod()
        {
            dataSyncTimer.Change(dataSyncDelay, Timeout.Infinite);
        }


        private void StartForeground()
        {
            Notification notification = null;
            NotificationCompat.Builder notificationBuilder = null;

            Intent intentNotif = new Intent(this, typeof(MainActivity));
            intentNotif.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intentNotif, 0);

            int sdk = (int)Android.OS.Build.VERSION.SdkInt;
            if (sdk >= 26)
            {
                NotificationChannel chan1 = new NotificationChannel(
            "ryxolpositiontracking",
            "service",
            NotificationImportance.Default);

                chan1.LightColor = Color.Transparent;
                chan1.LockscreenVisibility = NotificationVisibility.Secret;
                chan1.Importance = NotificationImportance.Low;

                var notificationManager = NotificationManager.FromContext(Application.Context);
                notificationManager.CreateNotificationChannel(chan1);

                notificationBuilder = new NotificationCompat.Builder(Application.Context, "ryxolpositiontracking");
            }
            else
            {
                notificationBuilder = new NotificationCompat.Builder(Application.Context);
            }

            notificationBuilder.SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default)
                .SetContentTitle(Constants.APPTITLE)
                .SetContentText("GPS Location tracking is active.")
                .SetContentIntent(pendingIntent);

            if ((int)Android.OS.Build.VERSION.SdkInt >= 19)
            {
                notificationBuilder.SetLargeIcon(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_notification));
            }

            notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);

            if ((int)Android.OS.Build.VERSION.SdkInt >= 16)
            {
                notification = notificationBuilder.Build();
            }
            else
            {
                notification = notificationBuilder.Notification;
            }

            StartForeground(NotificationId, notification);

            PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
            wl = pm.NewWakeLock(WakeLockFlags.Partial, "PositionTrackingLock");
            wl.Acquire();
        }

        #region Android Service Members

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            //Set sticky as we are a long running operation
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            AppWrapper.Service = null;

            if (wl != null)
                wl.Release();

            if (dataSyncTimer != null)
                dataSyncTimer.Change(Timeout.Infinite, Timeout.Infinite);

            LocationDataManager.Instance.StopLocationRequest();

            StopForeground(true);
        }

        #endregion
    }
}