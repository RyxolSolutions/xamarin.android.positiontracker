using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RX.PositionTracker.Droid.Managers
{
    public delegate void LocationAddressChangedHandler();

    public class LocationDataManager : Java.Lang.Object, Android.Gms.Location.ILocationListener
    {
        #region Private Fields 

        private LocationRequest _locRequest;
        private Location lastLocation = null;

        #endregion

        #region Public Fields 

        public GoogleApiClient _apiClient;
        public bool NeedStopAfterSpecialTracking = false;
        public Location LastSpecialTrackLocation = null;
        public LatLng CurrentLocation { get; private set; }
        public float CurrentDistance { get; private set; }
        public float CurrentFeetDistanceForST { get; set; }
        public double Altitude { get; private set; }
        public float Speed { get; private set; }
        public DateTime? LastLocationDetectedOn { get; private set; }
        public bool IsGooglePlayServicesConnected { get { return (_apiClient != null && _apiClient.IsConnected); } }
        public static event LocationAddressChangedHandler LocationAddressChanged;

        #endregion

        #region Singletone

        private static LocationDataManager instance;

        private LocationDataManager()
        {
        }

        public static LocationDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LocationDataManager();
                }

                return instance;
            }
        }

        #endregion

        #region Public Methods

        public void SetGoogleApiClient(GoogleApiClient apiClient, LocationRequest locRequest)
        {
            _apiClient = apiClient;
            _locRequest = locRequest;
        }

        public bool IsGoogleApiClientEmpty()
        {
            if (_apiClient == null)
                return true;

            return false;
        }

        public void DisconnectGoogleApiClient()
        {
            if (_apiClient != null && _apiClient.IsConnected)
            {
                _apiClient.Disconnect();
            }
        }

        public void RequestLocation()
        {
            if (_apiClient != null && _apiClient.IsConnected && _locRequest != null)
            {
                LocationServices.FusedLocationApi.RequestLocationUpdates(_apiClient, _locRequest, this);
            }
        }

        public async void StopLocationRequest()
        {
            if (_apiClient != null && _apiClient.IsConnected)
            {
                await LocationServices.FusedLocationApi.RemoveLocationUpdates(_apiClient, this);

                CurrentDistance = 0;
            }
        }

        #endregion

        void ProceedAddressByLocation(Location location)
        {
            LastLocationDetectedOn = DateTime.Now;

            LocationAddressChanged?.Invoke();
        }

        #region ILocationListener Implementation

        float minAccurancy = 25;
        public void OnLocationChanged(Location location)
        {
            if (location != null)
            {
                if (CurrentLocation == null)
                {
                    CurrentLocation = new LatLng(location.Latitude, location.Longitude);
                    Altitude = Math.Round(location.Altitude, 0);

                    float formatedValue = Convert.ToSingle((location.Speed * Constants.TIME_MULTIPLIER) / Constants.METERS_TO_KM_MULTIPLIER);
                    Speed = Convert.ToSingle(Math.Round(formatedValue, 0));
                    CurrentDistance = 0;
                }
                else
                {
                    CurrentLocation = new LatLng(location.Latitude, location.Longitude);

                    if (location.HasAltitude)
                    {
                        Altitude = Math.Round(location.Altitude, 0);
                    }

                    if (location.HasSpeed)
                    {
                        float formatedValue = Convert.ToSingle((location.Speed * Constants.TIME_MULTIPLIER) / Constants.METERS_TO_KM_MULTIPLIER);
                        float value = Convert.ToSingle(Math.Round(formatedValue, 0));
                        if (value != Single.PositiveInfinity && value != Single.NegativeInfinity)
                        {
                            Speed = value;
                        }
                    }
                    else
                    {
                        if (lastLocation != null)
                        {
                            if (location.HasAccuracy && location.Accuracy < minAccurancy)
                            {
                                double elapsedTime = (location.Time - lastLocation.Time) / Constants.METERS_TO_KM_MULTIPLIER; // Convert milliseconds to seconds
                                var calculatedSpeed = lastLocation.DistanceTo(location) / elapsedTime;

                                float formatedValue = Convert.ToSingle((calculatedSpeed * Constants.TIME_MULTIPLIER) / Constants.METERS_TO_KM_MULTIPLIER);

                                if (formatedValue > 0)
                                {
                                    float value = Convert.ToSingle(Math.Round(formatedValue, 0));
                                    if (value != Single.PositiveInfinity && value != Single.NegativeInfinity)
                                    {
                                        Speed = value;
                                    }                 
                                }
                            }
                        }
                    }
                }

                if (location.Accuracy < minAccurancy)
                {
                    if (lastLocation != null)
                    {
                        CurrentDistance = Convert.ToSingle(CurrentDistance + Math.Round((lastLocation.DistanceTo(location) / Constants.METERS_TO_KM_MULTIPLIER), 6));
                    }

                    lastLocation = location;
                    LastSpecialTrackLocation = location;
                }

                ProceedAddressByLocation(location);
            }
        }

        #endregion
    }
}