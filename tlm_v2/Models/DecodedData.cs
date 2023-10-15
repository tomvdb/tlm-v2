using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tlm_v2.Models
{
    public class DecodedData
    {
        private Hashtable _data;
        private int _id;
        private int _type;
        private int _group;

        public int Type { get => _type; }
        public int Id { get => _id; }
        public Hashtable Data { get => _data; }
        public int Group { get => _group; }

        public DecodedData(int type, int group, int id, Hashtable data) 
        { 
            _data = data;
            _id = id;
            _type = type;
        }
    }
}
