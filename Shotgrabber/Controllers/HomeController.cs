﻿using System;
using System.Configuration;
using System.Web.Mvc;
using VideoOS.Mobile.Portable.MetaChannel;
using VideoOS.Mobile.Portable.Utilities;
using VideoOS.Mobile.Portable.VideoChannel.Params;
using VideoOS.Mobile.SDK.Portable.Server.Base.Connection;

namespace Shotgrabber.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Uri uri = new Uri(ConfigurationManager.AppSettings["milestoneUrl"]);

            if (String.IsNullOrWhiteSpace(Request.QueryString["cameraId"]))
                throw new Exception("Camera ID was not sent in");

            // Initialize the Mobile SDK
            VideoOS.Mobile.SDK.Environment.Instance.Initialize();
            var channelType = 0 == string.Compare(uri.Scheme, "http", StringComparison.InvariantCultureIgnoreCase)
                ? ChannelTypes.HTTP
                : ChannelTypes.HTTPSecure;
            var Connection = new Connection(channelType, uri.Host, (uint)uri.Port)
            {
                CommandsQueueing = CommandsQueueing.SingleThread
            };

            var connectResponse = Connection.Connect(null, TimeSpan.FromSeconds(15));
            if (connectResponse.ErrorCode != ErrorCodes.Ok)
                throw new Exception("Not connected to surveillance server");

            var loginResponse = Connection.LogIn(ConfigurationManager.AppSettings["username"], ConfigurationManager.AppSettings["password"], ClientTypes.MobileClient, TimeSpan.FromSeconds(15));

            if (loginResponse.ErrorCode != ErrorCodes.Ok)
                throw new Exception("Not logged in to the surveillance server");

            var cameras = Connection.Views.GetAllViewsAndCameras(TimeSpan.FromSeconds(30));

            var videoParams = new VideoParams()
            {
                // eea4f88b-dee6-42ad-ae03-ba9d358ab7ac
                CameraId = new Guid(Request.QueryString["cameraId"]),
                DestWidth = int.Parse(ConfigurationManager.AppSettings["width"]),
                DestHeight = int.Parse(ConfigurationManager.AppSettings["height"]),
                CompressionLvl = 0
            };
            var playbackParams = new PlaybackParams()
            {
                Time = !String.IsNullOrWhiteSpace(Request.QueryString["dateTime"]) ? DateTime.Parse(Request.QueryString["dateTime"]) : DateTime.Now
            };
            var thumbnail = Connection.Thumbnail.GetThumbnailByTime(videoParams, playbackParams, TimeSpan.FromSeconds(30));

            ViewBag.Title = "Shot";

            ViewData["Thumbnail"] = Convert.ToBase64String(thumbnail.Thumbnail);

            return View();
        }
    }
}
