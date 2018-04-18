using App.Selfie;
using App.Twitter;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Windows;

namespace App {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class TheApp : Application {

        public static IConfigurationRoot Config { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.RollingFile("log.txt");

            Log.Logger = loggerConfig.CreateLogger();
            
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnException);

            SetConfig();
        }

        protected static void SetConfig() {
            App.MainWindow.UseRealSense = Config["UseRealSense"].ToLower() == "true";
            App.MainWindow.UseOcr = Config["UseOcr"].ToLower() == "true";
            App.MainWindow.UseMotionDetection = Config["UseMotionDetection"].ToLower() == "true";
            App.MainWindow.CameraIndex = Int32.Parse(Config["CameraIndex"]);

            App.MainWindow.WordForPicture = Config["Ocr:WordForPicture"];

            TwitterClient.AccessKey = Config["Twitter:AccessKey"];
            TwitterClient.AccessSecret = Config["Twitter:AccessSecret"];
            TwitterClient.ConsumerKey = Config["Twitter:ConsumerKey"];
            TwitterClient.ConsumerSecret = Config["Twitter:ConsumerSecret"];
            TwitterClient.DefaultMessage = Config["Twitter:DefaultMessage"];

            SelfieStateMachine.DelayBetweenMessages = Int32.Parse(Config["Selfie:DelayBetweenMessages"]);

            MirrorClient.Address = Config["SmartMirror:Address"];

            MirrorStateMachine.IDENTIFIEDUSER_TO_IDENTIFIEDUSER = GetInt("IDENTIFIEDUSER_TO_IDENTIFIEDUSER");
            MirrorStateMachine.IDENTIFIEDUSER_TO_NOBODY = GetInt("IDENTIFIEDUSER_TO_NOBODY");
            MirrorStateMachine.IDENTIFIEDUSER_TO_SOMEONE = GetInt("IDENTIFIEDUSER_TO_SOMEONE");
            MirrorStateMachine.NODOBY_TO_IDENTIFIEDUSER = GetInt("NODOBY_TO_IDENTIFIEDUSER");
            MirrorStateMachine.NODOBY_TO_SOMEONE = GetInt("NODOBY_TO_SOMEONE");
            MirrorStateMachine.SOMEONE_TO_IDENTIFIEDUSER = GetInt("SOMEONE_TO_IDENTIFIEDUSER");
            MirrorStateMachine.SOMEONE_TO_NODOBY = GetInt("SOMEONE_TO_NODOBY");

            int GetInt(string key) {
                return Int32.Parse(Config["SmartMirror:" + key]);
            }
        }
        
        private void OnException(object sender, UnhandledExceptionEventArgs e) {
            Log.Logger.Error("Global Unhandled exeption: " + e.ExceptionObject);
        }
    }
}
