using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextCloud {
	public class CloudInfo : ApiEntryBase {
		[Flags]
		public enum Properties {
			LastModified = 1,
			Tag = 2,
			Type = 4,
			Length = 8,
			Id = 16,
			FileId = 32,
			Favorite = 64,
			CommentsHref = 128,
			CommentsCount = 256,
			CommentsUnread = 512,
			OwnerId = 1024,
			OwnerDisplayName = 2048,
			ShareTypes = 4096,
			Checksums = 8192,
			HasPreview = 16384,
			Size = 32768,
			QuotaUsed = 65536,
			QuotaAvailable = 131072,
			Basic = LastModified|Tag|Type|Length|QuotaUsed|QuotaAvailable,
			All = 262143
		}

		[JsonProperty("href")]
		public string Path { get; set; }
		[JsonProperty("getlastmodified")]
		public DateTime? LastModified { get; set; }
        [JsonProperty("getetag")]
		public string Tag { get; set; }
        [JsonProperty("id")]
		public string Id { get; set; }
        [JsonProperty("fileid")]
		public string FileId { get; set; }
        [JsonProperty("favorite")]
		public int? Favorite { get; set; }
        [JsonProperty("comments-href")]
		public string CommentsHref { get; set; }
        [JsonProperty("comments-count")]
		public int? CommentsCount { get; set; }
        [JsonProperty("comments-unread")]
		public int? CommentsUnread { get; set; }
        [JsonProperty("owner-id")]
		public string OwnerId { get; set; }
        [JsonProperty("owner-display-name")]
		public string OwnerDisplayName { get; set; }
        [JsonProperty("share-types")]
		public string ShareTypes { get; set; }
        [JsonProperty("checksums")]
		public string Checksums { get; set; }
        [JsonProperty("size")]
		public long? Size { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }

        static public CloudInfo Parse(XElement data) {
			JObject props = new JObject();
			NextCloudService.FillJObject(props, data);
			if (props["collection"] == null)
				return props.ToObject<CloudFile>();
			return props.ToObject<CloudFolder>();
		}

		static public string ConvertFilePathToUriPath(string path) {
			path = path.Replace("\\", "/");
			while (path.StartsWith("/"))
				path = path.Substring(1);
			if (Regex.IsMatch(path, @"/\.*/"))
				throw new ApplicationException("Invalid path:" + path);
			return NextCloudService.Combine("remote.php/dav/files", path);
		}

		static public async Task<CloudInfo> GetProperties(NextCloudService api, string path, Properties properties = Properties.Basic) {
			XDocument postParams = EncodeProperties(properties);
			XElement data = await api.SendMessageAsyncAndGetXmlResponse(NextCloudService.PROPFIND, ConvertFilePathToUriPath(path), postParams,
				new {
					Depth = 0
				});
			return CloudInfo.Parse(data.Elements().First());
		}

		protected static XDocument EncodeProperties(Properties properties) {
			XDocument postParams = null;
			if (properties != Properties.Basic) {
				postParams = new XDocument();
				XElement p = new XElement("{DAV:}propfind");
				postParams.Add(p);
				XElement prop = new XElement("{DAV:}prop");
				p.Add(prop);
				prop.Add(new XElement("{DAV:}resourcetype"));   // Always ask for this, it's how we tell folder from file
				if (properties.HasFlag(Properties.LastModified))
					prop.Add(new XElement("{DAV:}getlastmodified"));
				if (properties.HasFlag(Properties.Tag))
					prop.Add(new XElement("{DAV:}getetag"));
				if (properties.HasFlag(Properties.Type))
					prop.Add(new XElement("{DAV:}getcontenttype"));
				if (properties.HasFlag(Properties.Length))
					prop.Add(new XElement("{DAV:}getcontentlength"));
				if (properties.HasFlag(Properties.Id))
					prop.Add(new XElement("{http://owncloud.org/ns}id"));
				if (properties.HasFlag(Properties.FileId))
					prop.Add(new XElement("{http://owncloud.org/ns}fileid"));
				if (properties.HasFlag(Properties.Favorite))
					prop.Add(new XElement("{http://owncloud.org/ns}favorite"));
				if (properties.HasFlag(Properties.CommentsHref))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-href"));
				if (properties.HasFlag(Properties.CommentsCount))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-count"));
				if (properties.HasFlag(Properties.CommentsUnread))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-unread"));
				if (properties.HasFlag(Properties.OwnerId))
					prop.Add(new XElement("{http://owncloud.org/ns}owner-id"));
				if (properties.HasFlag(Properties.OwnerDisplayName))
					prop.Add(new XElement("{http://owncloud.org/ns}owner-display-name"));
				if (properties.HasFlag(Properties.ShareTypes))
					prop.Add(new XElement("{http://owncloud.org/ns}share-types"));
				if (properties.HasFlag(Properties.Checksums))
					prop.Add(new XElement("{http://owncloud.org/ns}checksums"));
				if (properties.HasFlag(Properties.HasPreview))
					prop.Add(new XElement("{http://owncloud.org/ns}has-preview"));
				if (properties.HasFlag(Properties.Size))
					prop.Add(new XElement("{http://owncloud.org/ns}size"));
				if (properties.HasFlag(Properties.QuotaUsed))
					prop.Add(new XElement("{DAV:}quota-used-bytes"));
				if (properties.HasFlag(Properties.QuotaAvailable))
					prop.Add(new XElement("{DAV:}quota-available-bytes"));
			}
			return postParams;
		}

	}

	public class CloudFile : CloudInfo {
		[JsonProperty("has-preview")]
		public string HasPreview { get; set; }
        [JsonProperty("getcontentlength")]
		public long Length { get; set; }
        [JsonProperty("getcontenttype")]
		public string Type { get; set; }

        static public async Task<string> Upload(NextCloudService api, string path, Stream file) {
			XElement result = await api.SendMessageAsyncAndGetXmlResponse(HttpMethod.Put, ConvertFilePathToUriPath(path), file);
			return result.Element("OC-FileId").Value;
		}
	}

	public class CloudFolder : CloudInfo {
		[JsonProperty("quota-used")]
		public string QuotaUsed { get; set; }
        [JsonProperty("quota-available")]
		public string QuotaAvailable { get; set; }

        static public async Task<List<CloudInfo>> List(NextCloudService api, string path, Properties properties = Properties.Basic, int maxDepth = -1) {
			XDocument postParams = EncodeProperties(properties);
			object headers = null;
			if (maxDepth >= 0)
				headers = new {
					Depth = maxDepth
				};
			XElement result = await api.SendMessageAsyncAndGetXmlResponse(NextCloudService.PROPFIND, ConvertFilePathToUriPath(path), postParams, headers);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

		static public async Task Create(NextCloudService api, string path) {
			await api.SendMessageAsyncAndGetStringResponse(NextCloudService.MKCOL, ConvertFilePathToUriPath(path));
		}

		static public async Task Delete(NextCloudService api, string path) {
			await api.SendMessageAsyncAndGetStringResponse(HttpMethod.Delete, ConvertFilePathToUriPath(path));
		}

		static public async Task Move(NextCloudService api, string source, string dest) {
			await api.SendMessageAsyncAndGetStringResponse(NextCloudService.MOVE, source, "remote.php/dav", new { Destination = api.MakeUri(NextCloudService.Combine("remote.php/dav/files", dest)) });
		}

		static public async Task SetFavorite(NextCloudService api, string path, bool favorite) {
			XDocument postParams = new XDocument();
			XElement p = new XElement("{DAV:}propertyupdate");
			postParams.Add(p);
			XElement set = new XElement("{DAV:}set");
			p.Add(set);
			XElement prop = new XElement("{DAV:}prop");
			set.Add(prop);
			prop.Add(new XElement("{http://owncloud.org/ns}favorite", favorite ? "1" : "0"));
			await api.SendMessageAsyncAndGetStringResponse(NextCloudService.PROPPATCH, ConvertFilePathToUriPath(path), postParams);
		}

		static public async Task<List<CloudInfo>> GetFavorites(NextCloudService api, string path, Properties properties = Properties.Basic) {
			XDocument postParams = new XDocument();
			XElement p = new XElement("{http://owncloud.org/ns}filter-files");
			postParams.Add(p);
			XElement rules = new XElement("{http://owncloud.org/ns}filter-rules");
			p.Add(rules);
			rules.Add(new XElement("{http://owncloud.org/ns}favorite", "1"));
			XDocument propParams = EncodeProperties(properties);
			if(propParams != null)
				p.Add(propParams.Element("{DAV:}propfind").Element("{DAV:}prop"));
			XElement result = await api.SendMessageAsyncAndGetXmlResponse(NextCloudService.REPORT, ConvertFilePathToUriPath(path), postParams);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

		static public async Task<List<CloudInfo>> Search(NextCloudService api, XDocument query) {
			XElement result = await api.SendMessageAsyncAndGetXmlResponse(NextCloudService.REPORT, "remote.php/dav", query);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

	}
}
