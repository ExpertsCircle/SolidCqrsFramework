using System.Collections.Generic;

namespace SolidCqrsFramework.Aws.PreSignedUrlGenerator
{
    public interface IPreSignedUrlGenerator
    {
        IList<PreSignedFile> Generate(string auth0OptionsBucketName, IList<string> objectKeys, int expireFileInDays, string pathPrefix);
    }
}
