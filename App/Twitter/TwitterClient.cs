using System.Collections.Generic;
using System.IO;
using TweetSharp;

namespace App.Twitter {
    public class TwitterClient {

        public static string AccessKey;
        public static string AccessSecret;
        public static string ConsumerKey;
        public static string ConsumerSecret;

        public static string DefaultMessage = "";

        private TwitterService _client;

        public TwitterClient() {
            _client = new TwitterService(ConsumerKey, ConsumerSecret);
            _client.AuthenticateWith(AccessKey, AccessSecret);
        }

        public bool Send(byte[] picture, string message = null) {
            if(message == null) {
                message = DefaultMessage;
            }
            using (var stream = new MemoryStream(picture)) {
                var result = _client.SendTweetWithMedia(new SendTweetWithMediaOptions {
                    Status = message,
                    Images = new Dictionary<string, Stream> { { "", stream } }
                });
                return result != null;
            }
        }
    }
}
