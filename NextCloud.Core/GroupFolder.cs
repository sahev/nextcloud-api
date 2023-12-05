using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextCloud {
	public class GroupFolder : ApiEntryBase {
		public int id;
		public string mount_point;
		public JToken groups;
		public string quota;
		public long size;
		public bool acl;

		static public async Task<PlainList<GroupFolder>> List(NextCloudService api) {
			return await api.GetPlainCollectionAsync<GroupFolder>("index.php/apps/groupfolders/folders", "ocs.data");
		}

		static public async Task<int> Create(NextCloudService api, string name) {
			OcsEntry r = await api.PostAsync<OcsEntry>("index.php/apps/groupfolders/folders", null, new { mountpoint = name });
			return r.ocs.data["id"].Value<int>();
		}

		static public async Task<GroupFolder> Get(NextCloudService api, int folderId) {
			OcsEntry result = await api.GetAsync<OcsEntry>(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId));
			return result.ocs.data.ConvertToObject<GroupFolder>();
		}

		static public async Task Delete(NextCloudService api, int folderId) {
			await api.DeleteAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId));
		}

		static public async Task AddGroup(NextCloudService api, int folderId, string group) {
			await api.PostAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId, "groups"), null, new { group });
		}

		public async Task AddGroup(NextCloudService api, string group) {
			await AddGroup(api, id, group);
		}

		static public async Task RemoveGroup(NextCloudService api, int folderId, string group) {
			await api.DeleteAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId, "groups", group));
		}

		public async Task RemoveGroup(NextCloudService api, string group) {
			await RemoveGroup(api, id, group);
		}

		[Flags]
		public enum Permissions {
			Read = 1,
			Update = 2,
			Create = 4,
			Delete = 8,
			Share = 16,
			All = 31
		}

		static public async Task SetPermissions(NextCloudService api, int folderId, string group, Permissions permissions) {
			await api.PostAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId, "groups", group), null, new {
				permissions = (int)permissions
			});
		}

		public async Task SetPermissions(NextCloudService api, string group, Permissions permissions) {
			await SetPermissions(api, id, group, permissions);
		}

		static public async Task SetQuota(NextCloudService api, int folderId, long quota) {
			await api.PostAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId, "quota"), null, new {
				quota
			});
		}

		public async Task SetQuota(NextCloudService api, long quota) {
			await SetQuota(api, id, quota);
		}

		static public async Task Rename(NextCloudService api, int folderId, string name) {
			await api.PostAsync(NextCloudService.Combine("index.php/apps/groupfolders/folders", folderId, "mountpoint"), null, new {
				mountpoint = name
			});
		}

		public async Task Rename(NextCloudService api, string name) {
			await Rename(api, id, name);
		}
	}
}
	