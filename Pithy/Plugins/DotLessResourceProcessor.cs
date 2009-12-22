using dotless.Core;
using dotless.Core.configuration;

namespace Pithy.Plugins
{
    public class DotLessResourceProcessor : IResourceProcessor
    {
        public string ProcessFile(string content, AssetType assetType, string physicalFilePath, string contentPath)
        {
            if (assetType == AssetType.CSS && physicalFilePath.ToUpperInvariant().EndsWith(".LESS"))
            {
                var factory = new EngineFactory();
                var engine = factory.GetEngine(DotlessConfiguration.Default);
                return engine.TransformToCss(physicalFilePath);
            }
            return content;
        }
    }
}
