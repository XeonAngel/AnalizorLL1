using System.Collections.Generic;

namespace AnalizorLL1
{
    internal class ProductionRule
    {
        public string _LHS { get; set; }
        public List<string> _RHS { get; set; }

        public ProductionRule(string[] ProductionVector)
        {
            _LHS = ProductionVector[0];
            _RHS = new List<string> { ProductionVector[1] };
        }

        public ProductionRule(string lhs, List<string> rhs)
        {
            _LHS = lhs;
            _RHS = rhs;
        }
    }
}
