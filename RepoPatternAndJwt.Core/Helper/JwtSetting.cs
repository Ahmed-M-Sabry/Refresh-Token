using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoPatternAndJwt.Core.Helper
{
    public class JwtSetting
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationInMiuntes { get; set; }
    }
}
