using System.Collections.Generic;

namespace AnalizorLL1
{
    internal class TabelElement
    {
        public string _Terminal { get; set; }
        public List<string> _Rule { get; set; }

        public TabelElement()
        {
            _Rule = new List<string>();
        }
    }
}
