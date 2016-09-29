using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Lex.Util
{
    public class LexDfaProductionsCollection
    {
        public LexDfaProductionsCollection()
        {
        }

        public LexDfaProduction this[int index]
        {
            get { return null; }
        }

        public int Count { get; set; }

        public LexDfaProduction[] ToArray()
        {
            return null;
        }
        
        public bool Contains(LexDfaProduction lexDfaProduction)
        {
            return false;
        }

        public bool AddIfNotExists(LexDfaProduction lexDfaProduction)
        {
            //Add only if not exists!
            return false;
        }
    }
}
