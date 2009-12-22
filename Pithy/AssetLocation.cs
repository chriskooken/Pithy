namespace Pithy
{
    internal class AssetLocation
    {
        public AssetLocation(string contentPath, string physicalPath)
        {
            ContentPath = contentPath;
            PhysicalPath = physicalPath;
        }

        public string ContentPath { get; private set; }
        public string PhysicalPath { get; private set; }
    }
}
