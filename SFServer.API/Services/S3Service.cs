using Amazon.S3;
using Amazon.S3.Model;

namespace SFServer.API.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;

        public S3Service(IAmazonS3 s3Client, IConfiguration config)
        {
            _s3Client = s3Client;
            _bucket = config["S3:Bucket"] ?? throw new ArgumentException("S3 bucket is not configured");
        }

        public async Task UploadJsonAsync(string key, string json)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                ContentBody = json,
                ContentType = "application/json"
            };
            await _s3Client.PutObjectAsync(request);
        }

        public async Task<string> DownloadJsonAsync(string key)
        {
            var response = await _s3Client.GetObjectAsync(_bucket, key);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }

        public async Task DeleteAsync(string key)
        {
            await _s3Client.DeleteObjectAsync(_bucket, key);
        }
    }
}
