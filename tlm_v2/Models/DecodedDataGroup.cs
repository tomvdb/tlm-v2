using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlm_v2.Models
{
    public class DecodedDataGroup
    {
        private int _groupId;
        private string _groupName;
        private int _groupType;

        public string GroupName { get => _groupName; }
        public int GroupType { get => _groupType; }

        public DecodedDataGroup(int GroupId, string GroupName, int GroupType) 
        { 
            _groupName = GroupName;
            _groupType = GroupType;
            _groupId = GroupId;
        }
    }
}
