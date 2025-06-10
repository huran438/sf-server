using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;

namespace SFServer.API.Services
{
    public class S3Service
    {
        public S3Service()
        {
        }

        private IAmazonS3 CreateClient()
        {
            var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secret = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var url = Environment.GetEnvironmentVariable("S3__Url");

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };

            if (!string.IsNullOrEmpty(url))
            {
                config.ServiceURL = url;
                config.ForcePathStyle = true;
            }

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secret))
            {
                var creds = new BasicAWSCredentials(accessKey, secret);
                return new AmazonS3Client(creds, config);
            }

            return new AmazonS3Client(config);
        }

        private string GetBucket() => Environment.GetEnvironmentVariable("S3__Bucket") ?? string.Empty;

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
