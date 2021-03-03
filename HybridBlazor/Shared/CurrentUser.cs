using System.Collections.Generic;

namespace HybridBlazor.Shared
{
    public class CurrentUser
    {
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
}
