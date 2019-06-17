using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultApp.Objects
{
    class Domain
    {
        public string Name;
        public List<Entry> Entries = new List<Entry>();

        public Domain(String Name)
        {
            this.Name = Name;
        }
    }
}
