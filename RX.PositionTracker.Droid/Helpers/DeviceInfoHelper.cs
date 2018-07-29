using System;
using Java.Util;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;

namespace RX.PositionTracker.Droid.Helpers
{
    public class DeviceInfoHelper
    {
        public static long GetDeviceUniqID()
        {
            string telephonyDeviceId = string.Empty;
            string androidDeviceId = string.Empty;
            string m_szDevIDShort = string.Empty;

            try
            {
                androidDeviceId = Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            }
            catch (Exception e)
            {
            }
            try
            {
                TelephonyManager tm = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);
                telephonyDeviceId = tm.DeviceId;
                if (telephonyDeviceId == null)
                {
                    telephonyDeviceId = "";
                }
                else
                {
                    long result;
                    if (long.TryParse(telephonyDeviceId, out result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                //CUSTOMCODE: add your logger
            }

            if (m_szDevIDShort == null || m_szDevIDShort == "" || m_szDevIDShort.Equals("000000000000000") || m_szDevIDShort.Equals("0000000000000"))
            {
                m_szDevIDShort = "35" + //we make this look like a valid IMEI
                 Build.Board.Length % 10 + Build.Brand.Length % 10 +
                 Build.CpuAbi.Length % 10 + Build.Device.Length % 10 +
                 Build.Display.Length % 10 + Build.Host.Length % 10 +
                 Build.Id.Length % 10 + Build.Manufacturer.Length % 10 +
                 Build.Model.Length % 10 + Build.Product.Length % 10 +
                 Build.Tags.Length % 10 + Build.Type.Length % 10 +
                 Build.User.Length % 10; //13 digits
            }

            long resultDevIDShort;
            if (long.TryParse(m_szDevIDShort, out resultDevIDShort))
            {
                return resultDevIDShort;
            }

            return 000000000000000;
        }

        public static string GetStringIntegerHexBlocks(int value)
        {
            string result = "";
            string hexString = value.ToString("X");

            int remain = 8 - hexString.Length;
            char[] chars = new char[remain];
            Arrays.Fill(chars, '0');

            hexString = new String(chars) + hexString;

            int count = 0;
            for (int i = hexString.Length - 1; i >= 0; i--)
            {
                count++;
                try
                {
                    result = hexString.Substring(i, 1) + result;
                }
                catch (Exception ex)
                {

                }
                if (count == 4)
                {
                    result = "-" + result;
                    count = 0;
                }
            }

            if (result.StartsWith("-"))
            {
                result = result.Substring(1, result.Length - 1);
            }

            return result;
        }
    }
}