using System.Linq;
using System.Text;

namespace Pithy
{
    internal class AssetKey
    {
        public AssetKey(AssetType assetType, params string[] tags)
        {
            AssetType = assetType;
            Tags = tags.Select(x => x.ToUpperInvariant()).OrderBy(x => x).ToArray();
            var objects = Tags.Concat(new string[] { AssetType.ToString() }).ToArray();
            compoundKey = new CompoundKey(objects);
        }

        private CompoundKey compoundKey;
        public string[] Tags { get; private set; }
        public AssetType AssetType { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssetKey;
            if (other == null)
                return false;
            return compoundKey.Equals(other.compoundKey);
        }

        public override int GetHashCode()
        {
            return compoundKey.GetHashCode();
        }

        internal string ToCompiledName()
        {
            var sb = new StringBuilder();
            sb.Append(AssetType.ToString() + "_");
            foreach (var item in Tags)
                sb.Append(item + "_");
            return sb.ToString();
        }
    }
}
