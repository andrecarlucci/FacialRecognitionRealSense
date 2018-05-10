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

            
            SelfieStateMachine.FirstMessageDelay = Int32.Parse(Config["Selfie:FirstMessageDelay"]);
            SelfieStateMachine.CountDownDelay = Int32.Parse(Config["Selfie:CountDownDelay"]);
            SelfieStateMachine.ClickDelay = Int32.Parse(Config["Selfie:ClickDelay"]);
            SelfieStateMachine.FinalMessageDelay = Int32.Parse(Config["Selfie:FinalMessageDelay"]);
            SelfieStateMachine.CoolDownDelay = Int32.Parse(Config["Selfie:CoolDownDelay"]);
            SelfieStateMachine.PostToTwitter = Convert.ToBoolean(Config["Selfie:PostToTwitter"]);
            SelfieStateMachine.PathToSave = Config["Selfie:PathToSave"] ?? "";

            MirrorStateMachine.SELFIE = Config["Selfie:Trigger"];
            MirrorClient.Address = Config["SmartMirror:Address"];

            int GetInt(string key) {
                return Int32.Parse(Config["SmartMirror:" + key]);
            }
        }
        
        private void OnException(object sender, UnhandledExceptionEventArgs e) {
            Log.Logger.Error("Global Unhandled exeption: " + e.ExceptionObject);
        }
    }
}
