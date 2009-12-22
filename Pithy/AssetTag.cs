namespace Pithy
{
    internal class AssetTag
    {
        public AssetTag(AssetType assetType, string name, AssetLocation[] assetLocations)
        {
            AssetType = assetType;
            Name = name;
            AssetLocations = assetLocations;
        }

        public AssetType AssetType { get; private set; }
        public string Name { get; private set; }
        public AssetLocation[] AssetLocations { get; private set; }
    }
}
