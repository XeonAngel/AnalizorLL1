using System.Collections.Generic;

namespace AnalizorLL1
{
    class First_Follow
    {
        public int _NonTerminalIndex { get; set; }
        public int _RuleIndex { get; set; }
        public List<string> _Elements { get; set; }

        public First_Follow()
        {
            _Elements = new List<string>();
        }

        public First_Follow(int NonTerminalIndex, int RuleIndex, List<string> Elements)
        {
            _NonTerminalIndex = NonTerminalIndex;
            _RuleIndex = RuleIndex;
            _Elements = Elements;
        }
    }
}
