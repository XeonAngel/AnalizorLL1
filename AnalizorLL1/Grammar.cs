using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalizorLL1
{
    class Grammar
    {
        public string _StartSymbol { get; set; }
        public List<string> _Nonterminals { get; set; }
        public List<string> _Terminals { get; set; }
        public int _NumberOfProductionRules { get; set; }
        public List<ProductionRule> _ProductionRules { get; set; }

        public Grammar()
        {
            _Nonterminals = new List<string>();
            _Terminals = new List<string>();
            _ProductionRules = new List<ProductionRule>();
        }

        public string PrintProductionRules()
        {
            StringBuilder Grammar = new StringBuilder();
            foreach(var NonTer in _ProductionRules)
            {
                foreach(var Term in NonTer._RHS)
                {
                    Grammar.Append($"{NonTer._LHS} -> {Term}");
                    Grammar.AppendLine();
                }

            }
            return Grammar.ToString();
        }

        public void AddProductionRule(string[] ProductionVector)
        {
            foreach (var ProdRule in _ProductionRules)
            {
                if (ProdRule._LHS == ProductionVector[0])
                {
                    ProdRule._RHS.Add(ProductionVector[1]);
                    return;
                }
            }
            ProductionRule NewRule = new ProductionRule(ProductionVector);
            _ProductionRules.Add(NewRule);
        }

        public string CreateNewNonterminal(string OldNonterminal)
        {
            string NontermPrefix = Regex.Replace(OldNonterminal, @"\d", "");

            var ListOfOldNontermials = from OldTerm in _Nonterminals
                                       where OldTerm.StartsWith(NontermPrefix)
                                       select OldTerm;

            ListOfOldNontermials.OrderByDescending(q => q).ToList();

            string Number = Regex.Replace(ListOfOldNontermials.Last(), @"\D", "");
            int Max;
            if (Number == "")
            {
                Max = 1;
            }
            else
            {
                Max = int.Parse(Number) + 1;
            }
            string NewNonTerm = NontermPrefix + Max.ToString();

            return NewNonTerm;
        }
    }
}
