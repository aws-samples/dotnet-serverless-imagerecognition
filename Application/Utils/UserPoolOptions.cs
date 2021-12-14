using System;
using System.Collections.Generic;
using System.Text;

namespace CognitoLogin
{
    public class UserPoolOptions
    {
        public string Region { get; set; }
        public string UserPoolId { get; set; }
        public string UserPoolClientId { get; set; }
        public string UserPoolClientSecret { get; set; }
    }
}
