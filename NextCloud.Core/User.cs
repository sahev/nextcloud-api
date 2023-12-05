using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NextCloud {

	public class UserInfo : ApiEntryBase {
		public string userid { get; set; }
		public string password { get; set; }
        public string displayName { get; set; }
        public string email { get; set; }
        public string [] groups { get; set; }
        public string [] subadmin { get; set; }
        public string quota { get; set; }
        public string language { get; set; }
    }
	public class Quota : ApiEntryBase {
		public long free { get; set; }
        public long used { get; set; }
        public long total { get; set; }
        public double relative { get; set; }
        public long quota { get; set; }
    }
	public class User : ApiEntryBase {
		public bool enabled { get; set; }
		public string storageLocation { get; set; }
        public string id { get; set; }
		public DateTime lastLogin { get; set; }
		public string backend { get; set; }
		public string[] groups { get; set; }
		public string[] subadmin { get; set; }
		public Quota quota { get; set; }
		public string email { get; set; }
		public string displayname { get; set; }
		public string phone { get; set; }
		public string address { get; set; }
		public string website { get; set; }
		public string twitter { get; set; }
		public string language { get; set; }
		public string locale { get; set; }
		public JObject backendCapabilities { get; set; }

		static public async Task<ApiList<string>> List(NextCloudService api, ListRequest request = null) {
			return await api.GetListAsync<string>("ocs/v1.php/cloud/users", "ocs.data.users", request);
		}

		static public async Task<User> Get(NextCloudService api, string userid = null) {
			if (string.IsNullOrEmpty(userid))
				userid = api.Settings.User;
			if (string.IsNullOrEmpty(userid))
				userid = api.Settings.Username;
			OcsEntry entry = await api.GetAsync<OcsEntry>(NextCloudService.Combine("ocs/v1.php/cloud/users", userid));
			return entry.ocs.data.ConvertToObject<User>();
		}

		static public async Task Create(NextCloudService api, UserInfo info) {
			await api.PostAsync("ocs/v1.php/cloud/users", null, info);
		}

		public async Task Update(NextCloudService api, string password = null) {
			await api.PutAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", id), null, new {
				email,
				quota.quota,
				displayname,
				phone,
				address,
				website,
				twitter,
				password
			});
		}

		static public async Task Disable(NextCloudService api, string userid) {
			await api.PutAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "disable"));
		}

		static public async Task Enable(NextCloudService api, string userid) {
			await api.PutAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "enable"));
		}

		static public async Task Delete(NextCloudService api, string userid) {
			await api.DeleteAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid));
		}

		static public async Task<ApiList<string>> GetGroups(NextCloudService api, string userid, ListRequest request = null) {
			return await api.GetListAsync<string>(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "groups"), "ocs.data.groups", request);
		}

		static public async Task AddToGroup(NextCloudService api, string userid, string groupid) {
			await api.PostAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "groups"), null, new {
				groupid
			});
		}

		static public async Task RemoveFromGroup(NextCloudService api, string userid, string groupid) {
			await api.DeleteAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "groups"), null, new {
				groupid
			});
		}

		static public async Task PromoteToSubadminOfGroup(NextCloudService api, string userid, string groupid) {
			await api.PostAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "subadmins"), null, new {
				groupid
			});
		}

		static public async Task DemoteFromSubadminOfGroup(NextCloudService api, string userid, string groupid) {
			await api.DeleteAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "subadmins"), null, new {
				groupid
			});
		}

		static public async Task<List<string>> GetSubadminGroups(NextCloudService api, string userid) {
			JObject j = await api.GetAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "subadmins"));
			return j.SelectToken("ocs.data").ConvertToObject<List<string>>();
		}

		static public async Task ResendWelcomeEmail(NextCloudService api, string userid) {
			await api.PostAsync(NextCloudService.Combine("ocs/v1.php/cloud/users", userid, "welcome"));
		}

	}
}
