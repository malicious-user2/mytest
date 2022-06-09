// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2.Responses;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("==================================================");
        Console.WriteLine("This is a test");
        Console.WriteLine(Environment.GetEnvironmentVariable("TEMP_SECRET"));
        Console.WriteLine(Environment.GetEnvironmentVariable("AUTO_SECRET").Substring(2));
        Task.WaitAll(ExecuteAsync());
        Console.WriteLine("This was a test");
    }

    public static async Task ExecuteAsync()
    {


        string authJson = @"{
                ""installed"": {
                ""client_id"": ""573861238622-2qjkd0bq0n8d4ii3gpj1ipun3sk2s2ra.apps.googleusercontent.com"",
                ""project_id"": ""iconic-apricot-351114"",
                ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                ""token_uri"": ""https://oauth2.googleapis.com/token"",
                ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                ""client_secret"": ""{CLIENT_SECRET}"",
                ""redirect_uris"": [ ""http://localhost"" ]
              }
            }";

        authJson = authJson.Replace("{CLIENT_SECRET}", Environment.GetEnvironmentVariable("CLIENT_SECRET"));
        byte[] jsonArray = Encoding.ASCII.GetBytes(authJson);


        FileDataStore credStore = new FileDataStore("cred");
        MemoryStream jsonStream = new MemoryStream(jsonArray);


        GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = GoogleClientSecrets.FromStream(jsonStream).Secrets,
            Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
        });

        Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp webapp = new Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp(flow, "https://localhost", "");
        AuthResult auth = await webapp.AuthorizeAsync("opensource@aswglobal.com", CancellationToken.None);

        Console.WriteLine(auth.RedirectUri);
        Thread.Sleep(100);

        if (Environment.GetEnvironmentVariable("TOKEN_RESPONSE_CODE") != string.Empty)
        {
            Console.WriteLine("Response code found.");
            TokenResponse tokenRes = await flow.ExchangeCodeForTokenAsync("opensource@aswglobal.com", Environment.GetEnvironmentVariable("TOKEN_RESPONSE_CODE"), "https://localhost", CancellationToken.None);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com");

            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("TestApp", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", Environment.GetEnvironmentVariable("ACCESS_TOKEN"));

            Task<HttpResponseMessage> response = client.GetAsync("/repos/cantest-nospam/mytest/actions/secrets/public-key");
            var resource = Newtonsoft.Json.Linq.JObject.Parse(response.Result.Content.ReadAsStringAsync().Result);
            string key = (string)resource["key"];
            string key_id = (string)resource["key_id"];
            byte[] publicKey = Convert.FromBase64String(key);

            HttpResponseMessage deleteResponse = client.DeleteAsync("/repos/cantest-nospam/mytest/actions/secrets/TOKEN_RESPONSE_CODE").Result;
            Console.WriteLine(deleteResponse.StatusCode);

            byte[] accessTokenArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.AccessToken);
            byte[] expiresInSecondsArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.ExpiresInSeconds.Value.ToString());
            byte[] idTokenArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.IdToken == null ? String.Empty : tokenRes.IdToken);
            byte[] issuedArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.Issued.ToString());
            byte[] issuedUtcArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.IssuedUtc.ToString());
            byte[] refreshTokenArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.RefreshToken);
            byte[] scopeArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.Scope);
            byte[] tokenTypeArray = System.Text.Encoding.UTF8.GetBytes(tokenRes.TokenType);

            byte[] accessTokenBox = Sodium.SealedPublicKeyBox.Create(accessTokenArray, publicKey);
            byte[] expiresInSecondsBox = Sodium.SealedPublicKeyBox.Create(expiresInSecondsArray, publicKey);
            byte[] idTokenBox = Sodium.SealedPublicKeyBox.Create(idTokenArray, publicKey);
            byte[] issuedBox = Sodium.SealedPublicKeyBox.Create(issuedArray, publicKey);
            byte[] issuedUtcBox = Sodium.SealedPublicKeyBox.Create(issuedUtcArray, publicKey);
            byte[] refreshTokenBox = Sodium.SealedPublicKeyBox.Create(refreshTokenArray, publicKey);
            byte[] scopeBox = Sodium.SealedPublicKeyBox.Create(scopeArray, publicKey);
            byte[] tokenTypeBox = Sodium.SealedPublicKeyBox.Create(tokenTypeArray, publicKey);

            dynamic accessTokenSecret = new JObject();
            dynamic expiresInSecondsSecret = new JObject();
            dynamic idTokenSecret = new JObject();
            dynamic issuedSecret = new JObject();
            dynamic issuedUtcSecret = new JObject();
            dynamic refreshTokenSecret = new JObject();
            dynamic scopeSecret = new JObject();
            dynamic tokenTypeSecret = new JObject();

            accessTokenSecret.encrypted_value = Convert.ToBase64String(accessTokenBox);
            expiresInSecondsSecret.encrypted_value = Convert.ToBase64String(expiresInSecondsBox);
            idTokenSecret.encrypted_value = Convert.ToBase64String(idTokenBox);
            issuedSecret.encrypted_value = Convert.ToBase64String(issuedBox);
            issuedUtcSecret.encrypted_value = Convert.ToBase64String(issuedUtcBox);
            refreshTokenSecret.encrypted_value = Convert.ToBase64String(refreshTokenBox);
            scopeSecret.encrypted_value = Convert.ToBase64String(scopeBox);
            tokenTypeSecret.encrypted_value = Convert.ToBase64String(tokenTypeBox);

            accessTokenSecret.key_id = key_id;
            expiresInSecondsSecret.key_id = key_id;
            idTokenSecret.key_id = key_id;
            issuedSecret.key_id = key_id;
            issuedUtcSecret.key_id = key_id;
            refreshTokenSecret.key_id = key_id;
            scopeSecret.key_id = key_id;
            tokenTypeSecret.key_id = key_id;

            using (HttpContent httpContent = new StringContent(accessTokenSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/GOOGLE_ACCESS_TOKEN", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(expiresInSecondsSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/EXPIRES_IN_SECONDS", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(idTokenSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/ID_TOKEN", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(issuedSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/ISSUED", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(issuedUtcSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/ISSUED_UTC", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(refreshTokenSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/REFRESH_TOKEN", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(scopeSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/SCOPE", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }
            using (HttpContent httpContent = new StringContent(tokenTypeSecret.ToString(Formatting.None)))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response2 = client.PutAsync("/repos/cantest-nospam/mytest/actions/secrets/TOKEN_TYPE", httpContent).Result;
                Console.WriteLine(response2.StatusCode);
            }

            UserCredential cred1 = new UserCredential(flow, "opensource@aswglobal.com", tokenRes);

            Console.WriteLine(cred1.UserId);
        }
        else if (Environment.GetEnvironmentVariable("GOOGLE_ACCESS_TOKEN") != string.Empty)
        {
            Console.WriteLine("Saved token response found.");
            TokenResponse tokenRes = new TokenResponse();
            tokenRes.AccessToken = (string)Environment.GetEnvironmentVariable("GOOGLE_ACCESS_TOKEN");
            tokenRes.ExpiresInSeconds = long.Parse(Environment.GetEnvironmentVariable("EXPIRES_IN_SECONDS"));
            tokenRes.IdToken = (string)Environment.GetEnvironmentVariable("ID_TOKEN");
            tokenRes.Issued = DateTime.Parse(Environment.GetEnvironmentVariable("ISSUED"));
            tokenRes.IssuedUtc = DateTime.Parse(Environment.GetEnvironmentVariable("ISSUED_UTC"));
            tokenRes.RefreshToken = (string)Environment.GetEnvironmentVariable("REFRESH_TOKEN");
            tokenRes.Scope = (string)Environment.GetEnvironmentVariable("SCOPE");
            tokenRes.TokenType = (string)Environment.GetEnvironmentVariable("TOKEN_TYPE");

            UserCredential cred1 = new UserCredential(flow, "opensource@aswglobal.com", tokenRes);
            Console.WriteLine(cred1.UserId);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(DateTime.Now.AddSeconds((double)tokenRes.ExpiresInSeconds));
            Console.WriteLine(tokenRes.IsExpired(Google.Apis.Util.SystemClock.Default));

            YouTubeService yt = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = (string)Environment.GetEnvironmentVariable("YOUTUBE_API_KEY"),
                ApplicationName = "My App",
                HttpClientInitializer = cred1
            });

            Video vid = new Video();
            vid.Id = "jnzNNTKBglc";
            vid.Snippet = new VideoSnippet();
            vid.Snippet.Description = "GitHub Updated This!";
            vid.Snippet.Title = "Test title from GitHub";
            vid.Snippet.CategoryId = "1";
            VideosResource.UpdateRequest req = yt.Videos.Update(vid, "snippet");
            req.Execute();


        }
    }

}
