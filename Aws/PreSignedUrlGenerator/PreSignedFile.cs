using System;

namespace SolidCqrsFramework.Aws.PreSignedUrlGenerator;

public class PreSignedFile
{
    public string Name { get; set; }
    public string Url { get; set; }
    public DateTime ValidUntil { get; set; }
}
