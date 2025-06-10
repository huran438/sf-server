using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;

namespace SFServer.API.Services
{
    public class S3Service
    {
        private readonly IConfiguration _config;

        public S3Service(IConfiguration config)
        {
            _config = config;
        }

        private IAmazonS3 CreateClient()
        {
            var region = _config["AWS_REGION"] ?? "us-east-1";
            var accessKey = _config["AWS_ACCESS_KEY_ID"];
            var secret = _config["AWS_SECRET_ACCESS_KEY"];

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secret))
            {
                var creds = new BasicAWSCredentials(accessKey, secret);
                return new AmazonS3Client(creds, RegionEndpoint.GetBySystemName(region));
            }

            return new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
        }

        private string GetBucket() => _config["S3:Bucket"] ?? string.Empty;

        public async Task UploadJsonAsync(string key, string json)
        {
            using var client = CreateClient();
            var request = new PutObjectRequest
            {
                BucketName = GetBucket(),
                Key = key,
                ContentBody = json,
                ContentType = "application/json"
            };
            await client.PutObjectAsync(request);
        }

        public async Task<string> DownloadJsonAsync(string key)
        {
            using var client = CreateClient();
            var response = await client.GetObjectAsync(GetBucket(), key);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }

        public async Task DeleteAsync(string key)
        {
            using var client = CreateClient();
            await client.DeleteObjectAsync(GetBucket(), key);
        }
    }
}
