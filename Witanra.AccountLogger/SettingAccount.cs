using System;
using System.Collections.Generic;

namespace Witanra.AccountLogger
{
    public class SettingAccount
    {
        public string name;
        public string description;
        public DateTime openDate;
        public DateTime closeDate;
        public List<SettingAccountStep> settingAccountSteps;

        public string Destination;
    }
}