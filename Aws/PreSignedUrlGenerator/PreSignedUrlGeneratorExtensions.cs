using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

namespace SolidCqrsFramework.Aws.PreSignedUrlGenerator;

public static class PreSignedUrlGeneratorExtensions
{
    public static IServiceCollection AddPreSignedUrlGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IPreSignedUrlGenerator, PreSignedUrlGenerator>();
        services.AddAWSService<IAmazonS3>();
        return services;
    }
}
