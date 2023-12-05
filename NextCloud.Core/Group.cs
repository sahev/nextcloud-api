using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NextCloud {
	public class Group {
		static public async Task<ApiList<string>> List(NextCloudService api, ListRequest request = null) {
			return await api.GetListAsync<string>("ocs/v1.php/cloud/groups", "ocs.data.groups", request);
		}

		static public async Task Create(NextCloudService api, string groupid) {
			await api.PostAsync("ocs/v1.php/cloud/groups", null, new { groupid });
		}

		static public async Task<PlainList<string>> GetMembers(NextCloudService api, string groupid) {
			return await api.GetPlainListAsync<string>(NextCloudService.Combine("ocs/v1.php/cloud/groups", groupid), "ocs.data.users");
		}

		static public async Task<ApiList<string>> GetSubadmins(NextCloudService api, string groupid, ListRequest request = null) {
			return await api.GetListAsync<string>(NextCloudService.Combine("ocs/v1.php/cloud/groups", groupid, "subadmins"), "ocs.data", request);
		}

		static public async Task Delete(NextCloudService api, string groupid) {
			await api.DeleteAsync(NextCloudService.Combine("ocs/v1.php/cloud/groups", groupid));
		}

	}
}
