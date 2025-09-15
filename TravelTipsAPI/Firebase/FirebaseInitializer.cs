namespace TravelTipsAPI.Firebase
{
    using FirebaseAdmin;
    using Google.Apis.Auth.OAuth2;

    public class FirebaseInitializer
    {
        public static void InitFirebase(string json)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(
                    new AppOptions() { Credential = GoogleCredential.FromJson(json) }
                );
            }
        }
    }
}
