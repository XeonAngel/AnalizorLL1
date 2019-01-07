using System.Collections.Generic;

namespace AnalizorLL1
{
    internal class TabelNonTerminal
    {
        public string _Nonterminal { get; set; }
        public List<TabelElement> _Terminals { get; set; }

        public TabelNonTerminal()
        {
            _Terminals = new List<TabelElement>();
        }
    }
}
