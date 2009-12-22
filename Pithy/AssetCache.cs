using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Yahoo.Yui.Compressor;
using System.Globalization;

namespace Pithy
{
    public class AssetCache
    {
        private static string outputContentPath;
        private static string outputDirectoryPath;
        private static bool outputPathSet;
        private static bool compressAssets;
        private static bool compressAssetsSet;
        private static bool configured;
        private static bool debugMode;
        private static IDictionary<AssetKey, AssetTag> configuredTags;
        private static IDictionary<AssetKey, AssetLocation[]> processedTags;
        private static IList<IResourceProcessor> javaScriptProcessors;
        private static IList<IResourceProcessor> cssProcessors;

        private static object lockJavaScript = new object();
        private static object lockCss = new object();
        private static object lockFile = new object();

        static AssetCache()
        {
            configuredTags = new Dictionary<AssetKey, AssetTag>();
            processedTags = new Dictionary<AssetKey, AssetLocation[]>();
            javaScriptProcessors = new List<IResourceProcessor>();
            cssProcessors = new List<IResourceProcessor>();
            debugMode = false;
        }

        private static void AssertNotConfigured()
        {
            if (configured)
                throw new InvalidOperationException("Cannot change configuration after it has been verified");
        }

        public static string OutputDirectory
        {
            get { return outputDirectoryPath; }
            set
            {
                AssertNotConfigured();
                if (outputPathSet)
                    throw new InvalidOperationException("This variable can only be set once");
                outputContentPath = value;
                outputDirectoryPath = CurrentHttpContext.Server.MapPath(value);
                outputPathSet = true;
            }
        }

        public static bool CompressAssets
        {
            get { return compressAssets; }
            set
            {
                AssertNotConfigured();
                if (compressAssetsSet)
                    throw new InvalidOperationException("This variable can only be set once");
                compressAssets = value;
                compressAssetsSet = true;
            }
        }

        public static bool DebugMode
        {
            get { return debugMode; }
            set
            {
                AssertNotConfigured();
                debugMode = value;
            }
        }

        public static void AddJavaScriptProcessor(IResourceProcessor resourceProcessor)
        {
            if (!javaScriptProcessors.Contains(resourceProcessor))
                javaScriptProcessors.Add(resourceProcessor);
        }

        public static void AddCssProcessor(IResourceProcessor resourceProcessor)
        {
            if (!cssProcessors.Contains(resourceProcessor))
                cssProcessors.Add(resourceProcessor);
        }

        public static void AddJavaScript(string tag, params string[] contentPath)
        {
            AddTag(AssetType.JS, tag, contentPath);
        }

        public static void AddCss(string tag, params string[] contentPath)
        {
            AddTag(AssetType.CSS, tag, contentPath);
        }

        public static void AddFile(string tag, params string[] contentPath)
        {
            AddTag(AssetType.FILE, tag, contentPath);
        }

        private static void AddTag(AssetType assetType, string tag, params string[] contentPath)
        {
            AssertNotConfigured();
            var key = new AssetKey(assetType, tag);
            if (configuredTags.ContainsKey(key))
                throw new InvalidOperationException("Tag already exists: " + tag);
            var assetLocations = contentPath.Select(x => new AssetLocation(x, CurrentHttpContext.Server.MapPath(x))).ToArray();
            var assetTag = new AssetTag(assetType, tag, assetLocations);
            configuredTags.Add(key, assetTag);
        }

        public static void AssertConfigurationIsValid()
        {
            foreach (var tag in configuredTags.Values)
                foreach (var assetLocation in tag.AssetLocations)
                    if (!File.Exists(assetLocation.PhysicalPath))
                        throw new InvalidOperationException(string.Format("File path for tag [{0}] not found: {1}", tag.Name, assetLocation.PhysicalPath));
            if (!Directory.Exists(outputDirectoryPath))
                Directory.CreateDirectory(outputDirectoryPath);
            configured = true;
        }

        public static string[] GetJavaScriptFor(params string[] tags)
        {
            lock (lockJavaScript)
            {
                return GetResourcesFor(AssetType.JS, tags);
            }
        }

        public static string[] GetCssFor(params string[] tags)
        {
            lock (lockCss)
            {
                return GetResourcesFor(AssetType.CSS, tags);
            }
        }

        public static string[] GetFileFor(params string[] tags)
        {
            lock (lockFile)
            {
                return GetResourcesFor(AssetType.FILE, tags);
            }
        }

        private static string[] GetResourcesFor(AssetType assetType, params string[] tags)
        {
            var key = new AssetKey(assetType, tags);

            // In debug mode we don't cache the processed tags
            if (!debugMode && processedTags.ContainsKey(key))
                return processedTags[key].Select(x => x.ContentPath).ToArray();

            var assets = tags.Select(x =>
            {
                var assetKey = new AssetKey(assetType, x);
                if (!configuredTags.ContainsKey(assetKey))
                    throw new InvalidOperationException("Tag not found: " + x);
                return configuredTags[assetKey];
            });
            var allLocations = new List<AssetLocation>();
            foreach (var asset in assets)
                allLocations.AddRange(asset.AssetLocations);

            var resourceProcessors = assetType == AssetType.JS ? javaScriptProcessors : cssProcessors;
            if (assetType != AssetType.FILE && CompressAssets)
            {
                var processedFile = CompressAndCompile(key, allLocations);
                if (debugMode) // In debug mode we don't cache the processed tags
                    return new string[] { processedFile.ContentPath };
                processedTags.Add(key, new AssetLocation[] { processedFile });
            }
            else if (assetType != AssetType.FILE && resourceProcessors.Any())
            {
                var processedFiles = ProcessInfo(key, allLocations);
                if (debugMode) // In debug mode we don't cache the processed tags
                    return processedFiles.Select(x => x.ContentPath).ToArray();
                processedTags.Add(key, processedFiles);
            }
            else
            {
                if (debugMode) // In debug mode we don't cache the processed tags
                    return allLocations.Select(x => x.ContentPath).ToArray();
                processedTags.Add(key, allLocations.ToArray());
            }

            return processedTags[key].Select(x => x.ContentPath).ToArray();
        }

        private static AssetLocation[] ProcessInfo(AssetKey key, IEnumerable<AssetLocation> allLocations)
        {
            var locations = new List<AssetLocation>();
            foreach (var assetLocation in allLocations)
            {
                var fileContent = File.ReadAllText(assetLocation.PhysicalPath);
                var resourceProcessors = key.AssetType == AssetType.JS ? javaScriptProcessors : cssProcessors;
                foreach (var processor in resourceProcessors)
                    fileContent = processor.ProcessFile(fileContent, key.AssetType, assetLocation.PhysicalPath, assetLocation.ContentPath);
                var fileName = Path.GetFileName(assetLocation.PhysicalPath);
                var fileExtension = "." + key.AssetType.ToString().ToLower();
                if (!fileName.EndsWith(fileExtension))
                    fileName += fileExtension;
                var physicalPath = Path.Combine(outputDirectoryPath, fileName);
                File.WriteAllText(physicalPath, fileContent, Encoding.UTF8);
                var contentPath = outputContentPath + fileName;
                locations.Add(new AssetLocation(contentPath, physicalPath));
            }
            return locations.ToArray();
        }

        private static AssetLocation CompressAndCompile(AssetKey key, IEnumerable<AssetLocation> assetLocations)
        {
            var text = assetLocations.Select(x =>
                {
                    var fileContent = File.ReadAllText(x.PhysicalPath);
                    var resourceProcessors = key.AssetType == AssetType.JS ? javaScriptProcessors : cssProcessors;
                    foreach (var processor in resourceProcessors)
                        fileContent = processor.ProcessFile(fileContent, key.AssetType, x.PhysicalPath, x.ContentPath);
                    return fileContent;
                });
            var compiled = CompileText(text);
            var compressed = string.Empty;
            if (key.AssetType == AssetType.JS)
            {
                compressed = new JavaScriptCompressor(compiled, false, Encoding.UTF8, CultureInfo.CurrentCulture).Compress();
            }
            else if (key.AssetType == AssetType.CSS)
            {
                compressed = CssCompressor.Compress(compiled);
            }
            else
                throw new NotSupportedException();
            var fileName = key.ToCompiledName() + "." + key.AssetType.ToString().ToLower();
            var physicalPath = Path.Combine(outputDirectoryPath, fileName);
            File.WriteAllText(physicalPath, compressed, Encoding.UTF8);
            var contentPath = outputContentPath + fileName;
            return new AssetLocation(contentPath, physicalPath);
        }

        private static string CompileText(IEnumerable<string> text)
        {
            var sb = new StringBuilder();
            foreach (var item in text)
                sb.AppendLine(item + Environment.NewLine);
            return sb.ToString();
        }

        private static HttpContext CurrentHttpContext
        {
            get
            {
                return HttpContext.Current;
            }
        }
    }
}
