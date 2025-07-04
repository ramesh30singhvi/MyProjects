using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Indexing.v3;
using Google.Apis.Indexing.v3.Data;
using CPReservationApi.DAL;

namespace CPReservationApi.WebApi.Services
{
    public class GoogleIndexer
    {
        static string _credenitalsJSON = "{  \"type\": \"service_account\",  \"project_id\": \"lithe-bonito-230821\",  \"private_key_id\": \"cfbead2ca49f258b27ac3661811efa4dba89d531\",  \"private_key\": \"-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCM+xb2aMvkSc5I\n9VVo/VPfBvgZzqCKw6GrkTB7NEKWZACaWo0AjlHEHUfnuP2WpqaIfg4ezehxmowe\newT6eWwB55jPE2Z1rYZsTtXRKd22lWLFjWm38FrN+mj8kWzIpDBUgvmGMWFTuI59\n655PeulYdByXdr4t6nHLm+29Aq5i6PqM1RIC0k062J8J5m5Rki9k1akLSv8Zbi//\n27ojgDrvGQnjri3LVRFJ8uzOF1hTUTdVxGrPUurL+K3QlqxSJKAU08EZC+avyAue\nXk3m4s9PUGpQkUcQlzws302Di1I8JBYSKxsn0uIjAULdFm0XctcO1xagDtEOebOW\naWFcB22BAgMBAAECggEAAYnN2HBDshQYfNUcePhNEK5uFgEz4QyKBSnts8wNFY01\n/axf3hhip2qp+bFowI34XDeDGbslpNoXjekwU54jW3cpxJZE56M2Li4pooFVirDH\nmQZv5F9qUwjYwDf2Z89CB1hxUF48KyJk5glxOuAvwqGfT3HIWNtP7lvsGxD37Frz\nsDKidWb4Anl1p3nn882aRdyFq2PO3Bbq1PF9HS3lmj4aveVAA+WE4zTlqQtoqJEk\nWjBiQVAV5hDv4zz+P6da/aTwBUewi2/SKlZ2zhm2HpdouyEMLNe1pOK6M2m64Ijs\nmmvi2gy2nbDc4q05cROpQzRNdPC9q+s5ELrCzHMFfwKBgQDGUsmnL+5DzVsZ6Q1V\nT+xnI3CLTa3LIdScJj9No03Y4FwVvqYG/DmLs84GdJZlFcibybrX5MKFg+6Y3PoK\nqbB4kYzPop3IXBsHpgMtZlDpMxK6QsnfSU8k8+sAF170tbonJD8mZXduoRyO0IWR\n1wSBjdXLU8s1uuoL5dXGcXx3OwKBgQC1+yMhx79WoY2nu7rD+uXMvkOrLdEYFJsA\nepe2sCnw4dfqg8JpMydpsJHG5ywRttI0LwSnUrB3sxDvnzRIElilBl0UZXUpfJtL\nTAIUPF3W91uJRQZWvdYKDNL6bPMpaf0gPouYIkaXsuj4ziDn42x1V32RKZx96fSd\nSgnN0ae6cwKBgBBsPJymoHFm6PtdTChbZPUpfyFZ/mFK3ONNW4KfWkvyUE3Qqsj7\nS/ygmBf6wZV/V0xnSRylqeIKidCIw//sC1wAmYt8KNq1ndtlSPASf/K/svZPn43o\n8S/bLwOj684R2mdxXfVdA6Xam5XL+LOZ/ZqI10JuHu7FGqp/jtNPBIKPAoGASWD8\np5BdrNbzwaDHDGNM6iQGMS4GVAlq/uRv7HBYWRMX9y4t5DGGQO27ulJYyMPJOtWC\nsN183qdLuvOdJzqOl+xa9/B5L8NY8yGw7Ovygs23EUDN74gYmHDXWbc4zYg5Uxkl\nPLEFegHgQwMK7bYcc6xVq4T13yfJROsnzOAvG9UCgYAIfvo17lBjG0Jd7gUGv6Al\noo0zAs2h0R1aCNGLY8b+8KQwrDD4J6QHh/ug3x+r0A76IFRNBlx7uQ16YyDbwfuO\ndF+PtUjgPoyMQLo21HATJ3hAN5uGuvivdnk/9gj76gAk735S9clkoxboMd0UYhDD\nZOXaBQFGJdZlgRYbtOoNHg==\n-----END PRIVATE KEY-----\n\",  \"client_email\": \"google-indexing-api@lithe-bonito-230821.iam.gserviceaccount.com\",  \"client_id\": \"118266651856906196948\",  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",  \"token_uri\": \"https://oauth2.googleapis.com/token\",  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/google-indexing-api%40lithe-bonito-230821.iam.gserviceaccount.com\"}";
        public static bool AddUrl(string url,string URL_Type = "URL_UPDATED")
        {
            bool isSuccess = false;

            try
            {
                GoogleCredential credential = GoogleCredential.FromJson(_credenitalsJSON).CreateScoped(new[] { IndexingService.Scope.Indexing });

                var indexingService = new IndexingService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google Indexing Integration"
                });

                var requestBody = new UrlNotification
                {
                    Url = url,
                    Type = URL_Type
                };

                var publishRequest = new UrlNotificationsResource.PublishRequest(indexingService, requestBody);

                var resp = publishRequest.Execute();

                if (resp != null)
                    isSuccess = true;
            }
            catch (Exception ex) { 
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            logDAL.InsertLog("WebApi", "CreateGoogleIndexer:  Url-" + url + ",Error:" + ex.Message.ToString(), "",1,0);
            }

            return isSuccess;
        }

        public static bool CheckUrl(string url)
        {
            bool isSuccess = false;

            GoogleCredential credential = GoogleCredential.FromJson(_credenitalsJSON).CreateScoped(new[] { IndexingService.Scope.Indexing });


            var indexingService = new IndexingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Indexing Integration"
            });

            var GetMetadataRequest = new UrlNotificationsResource.GetMetadataRequest(indexingService);
            GetMetadataRequest.Url = url;

            var resp = GetMetadataRequest.Execute();

            if (resp != null)
                isSuccess = true;

            return isSuccess;
        }
    }
}
