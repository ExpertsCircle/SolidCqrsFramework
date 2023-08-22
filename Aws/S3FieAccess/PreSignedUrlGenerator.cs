using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace SolidCqrsFramework.Aws.S3FieAccess
{

    public class PreSignedUrlGenerator : IPreSignedUrlGenerator
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<PreSignedUrlGenerator> _logger;

        public PreSignedUrlGenerator(IAmazonS3 amazonS3Client, ILogger<PreSignedUrlGenerator> logger)
        {
            _s3Client = amazonS3Client;
            _logger = logger;
        }

        public IList<PreSignedFile> Generate(string bucketName, IList<string> objectKeys, int expireFileInDays, string pathPrefix)
        {
            _logger.LogTraceWithObject("Generating pre-signed URL", new { bucketName, pathPrefix, FilesToSign = objectKeys.ToList() });
            var preSignedFiles = new List<PreSignedFile>();

            foreach (var objectKey in objectKeys)
            {
                var keyWithFolder = !string.IsNullOrEmpty(pathPrefix) ? $"{pathPrefix}/{objectKey}" : objectKey;
                var validUntil = DateTime.UtcNow.AddHours(expireFileInDays);

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = keyWithFolder,
                    Expires = validUntil
                };

                try
                {
                    var urlString = _s3Client.GetPreSignedURL(request);
                    _logger.LogTraceWithObject("Generated pre-signed URL", new { urlString });

                    var preSignedFile = new PreSignedFile
                    {
                        Name = objectKey,
                        Url = urlString,
                        ValidUntil = validUntil
                    };

                    preSignedFiles.Add(preSignedFile);
                }
                catch (AmazonS3Exception ex)
                {
                    _logger.LogError(ex, $"Error encountered on server. Message:'{ex.Message}' when generating pre-signed URL.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unknown encountered on server. Message:'{ex.Message}' when generating pre-signed URL.");
                    throw;
                }
            }

            return preSignedFiles;
        }
    }

}
