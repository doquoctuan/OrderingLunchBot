using Azure.Storage.Blobs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;

namespace OrderRice.Helper
{
    public class GoogleSheetsHelper
    {
        public SheetsService Service { get; set; }
        const string APPLICATION_NAME = "OrderLunch";
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string AzBlobStorageConnectionStr;
        private readonly string ContainnerName;
        private readonly string FilePath;
        public GoogleSheetsHelper(IConfiguration configuration)
        {
            AzBlobStorageConnectionStr = configuration["AzureWebJobsStorage"];
            ContainnerName = configuration["AzBlobStorage_Container"];
            FilePath = configuration["AzBlobStorage_PathAuthentication"];
            InitializeService();
        }
        private void InitializeService()
        {
            var credential = GetCredentialsFromFile();
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = APPLICATION_NAME
            });
        }
        private GoogleCredential GetCredentialsFromFile()
        {
            GoogleCredential credential;

            BlobServiceClient blobServiceClient = new(AzBlobStorageConnectionStr);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainnerName);
            BlobClient blobClient = containerClient.GetBlobClient(FilePath);

            if (!blobClient.ExistsAsync().Result)
            {
                throw new Exception("The file does not exist");
            }

            var response = blobClient.DownloadAsync().Result;
            credential = GoogleCredential.FromStream(response.Value.Content).CreateScoped(Scopes);

            return credential;
        }
    }
}
