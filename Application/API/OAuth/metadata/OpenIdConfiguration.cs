using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metadata
{
    public class OpenIdConfiguration
    {
        public string authorization_endpoint { get; set; }
        public List<string> id_token_signing_alg_values_supported { get; set; }
        public string issuer { get; set; }
        public string jwks_uri { get; set; }
        public List<string> response_types_supported { get; set; }
        public List<string> scopes_supported { get; set; }
        public List<string> subject_types_supported { get; set; }
        public string token_endpoint { get; set; }
        public List<string> token_endpoint_auth_methods_supported { get; set; }
        public string userinfo_endpoint { get; set; }
    }
}
