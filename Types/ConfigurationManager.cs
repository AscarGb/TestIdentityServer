using System;
using System.Collections.Generic;
using System.Text;

namespace Types
{
    public class ConfigurationManager
    {
        public string Hash { get; set; }
        public string ngAuthSecret { get; set; }
        public string AppSecret { get; set; }
        public string defaultAdminPsw { get; set; }
    }
}
