using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace TimelyBackEnd.Services.Implementations
{
    public class FcmService
    {
        public FcmService(IConfiguration configuration)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = configuration["Firebase:CredentialsPath"];
                if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });
                }
                // If no file, we might be using GOOGLE_APPLICATION_CREDENTIALS env var
                else if (FirebaseApp.DefaultInstance == null)
                {
                     FirebaseApp.Create(new AppOptions()
                     {
                         Credential = GoogleCredential.GetApplicationDefault()
                     });
                }
            }
        }

        public async Task SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrEmpty(token)) return;

            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task SendGroupNotificationAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null)
        {
            var tokenList = tokens.Where(t => !string.IsNullOrEmpty(t)).ToList();
            if (!tokenList.Any()) return;

            var message = new MulticastMessage()
            {
                Tokens = tokenList,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        }
    }
}
