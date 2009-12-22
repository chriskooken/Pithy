using Pithy.CacheBuster;

namespace Pithy.Plugins
{
    public class CacheBusterEmbeddedUrlProcessor : IResourceProcessor
    {
        public string ProcessFile(string content, AssetType assetType, string physicalFilePath, string contentPath)
        {
            return content.Replace("?r=0", "?r=" + CachedResourceId.Key);
        }
    }
}
