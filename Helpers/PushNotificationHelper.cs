using WebPush;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;

namespace TaskManagement.Helpers
{
    public static class PushNotificationHelper
    {
        private static readonly string PublicKey;
        private static readonly string PrivateKey;
        private const string Subject = "mailto:your-email@example.com";

        // Static constructor to initialize the static fields
        static PushNotificationHelper()
        {
            // Load appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Assign keys from configuration
            PublicKey = configuration["VapidKeys:PublicKey"];
            PrivateKey = configuration["VapidKeys:PrivateKey"];

            // Validate the keys
            ValidateVapidKeys(PublicKey, PrivateKey);

            // Log the keys (for debugging, remove in production)
            Console.WriteLine($"PublicKey: {PublicKey}");
            Console.WriteLine($"PrivateKey: {PrivateKey}");
        }

        /// <summary>
        /// Validates the VAPID keys to ensure they are valid Base64 strings.
        /// </summary>
        /// <param name="publicKey">Public VAPID key</param>
        /// <param name="privateKey">Private VAPID key</param>
        private static void ValidateVapidKeys(string publicKey, string privateKey)
        {
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
            {
                throw new Exception("VAPID keys are not configured properly in appsettings.json. Keys are null or empty.");
            }

            if (!IsBase64String(publicKey) || !IsBase64String(privateKey))
            {
                throw new Exception("VAPID keys are not properly formatted Base64 strings.");
            }
        }

        /// <summary>
        /// Utility method to check if a string is a valid Base64 string.
        /// </summary>
        /// <param name="base64">The string to validate</param>
        /// <returns>True if valid Base64; otherwise, false</returns>
        private static bool IsBase64String(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        /// <summary>
        /// Sends a push notification to a user.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="message">The notification message</param>
        public static async Task SendPushAsync(string userId, string message, ApplicationDbContext dbContext)
        {
            try
            {
                // Retrieve the user's push subscription details from the database.
                var subscription = await GetUserSubscriptionAsync(userId, dbContext);
                if (subscription == null)
                {
                    Console.WriteLine($"No push subscription found for user: {userId}");
                    return;
                }

                // Create the PushSubscription object.
                var pushSubscription = new PushSubscription(
                    subscription.Endpoint,
                    subscription.P256DH,
                    subscription.Auth);

                // Set up VAPID details.
                var vapidDetails = new VapidDetails(Subject, PublicKey, PrivateKey);

                // Create a WebPush client.
                var webPushClient = new WebPushClient();

                // Send the push notification.
                await webPushClient.SendNotificationAsync(pushSubscription, message, vapidDetails);
                Console.WriteLine("Push notification sent successfully!");
            }
            catch (WebPushException ex)
            {
                Console.WriteLine($"Error sending push notification: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        /// <summary>
        /// Retrieves the push subscription details for a user.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>UserSubscription</returns>
        private static async Task<TaskManagement.Models.UserSubscription?> GetUserSubscriptionAsync(string userId, ApplicationDbContext dbContext)
        {
            try
            {
                // Query the UserSubscriptions table
                var subscription = await dbContext.UserSubscriptions
                    .FirstOrDefaultAsync(sub => sub.UserId == userId);

                return subscription;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subscription for user {userId}: {ex.Message}");
                return null;
            }
        }

    }

    /// <summary>
    /// Model class for user push subscription details.
    /// </summary>
    public class UserSubscription
    {
        public string Endpoint { get; set; } = string.Empty;
        public string P256DH { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }
}
