using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NextCloud
{
    public class OcsShare
    {
        public Meta meta;
        public List<JObject> data;
    }

    public class OcsShareEntry : ApiEntry
    {
        public OcsShare ocs;
    }
}