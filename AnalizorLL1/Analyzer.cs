using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnalizorLL1
{
    class Analyzer
    {
        public Grammar _Grammar = new Grammar();
        public List<First_Follow> _First_Follow = new List<First_Follow>();
        public List<TabelNonTerminal> _Tabel = new List<TabelNonTerminal>();
        public bool IsOkToCompile = false;


        public string PrintFirst_Follow()
        {
            StringBuilder First_FollowStringB = new StringBuilder();

            foreach (var Item in _First_Follow)
            {
                First_FollowStringB.Append($"{_Grammar._ProductionRules[Item._NonTerminalIndex]._LHS}:");
                foreach(var Element in Item._Elements)
                {
                    First_FollowStringB.Append($" {Element} ");
                }
                First_FollowStringB.AppendLine();
            }

            return First_FollowStringB.ToString(); 
        }

        public void SetNewGrammar(string FilePath)
        {
            //Citire Fisier
            var FileContent = File.ReadLines(FilePath);

            //Citim simbolul de start
            _Grammar._StartSymbol = FileContent.ElementAt(0).ToString();

            //Citim Neterminalele
            string NontermString = FileContent.ElementAt(1).ToString();
            var NontermVector = NontermString.Split(' ');
            _Grammar._Nonterminals.AddRange(NontermVector);

            //Citim Terminalele
            string TermString = FileContent.ElementAt(2).ToString();
            if (TermString.Contains("   "))
            {
                TermString = TermString.Replace("   ", " ");
                _Grammar._Terminals.Add(" ");
            }
            else if (TermString.Contains("  "))
            {
                TermString = TermString.Replace("  ", "");
                _Grammar._Terminals.Add(" ");
            }
            var TermVector = TermString.Split(' ');
            _Grammar._Terminals.AddRange(TermVector);

            //Citim Numarul de reguli de productie
            _Grammar._NumberOfProductionRules = int.Parse(FileContent.ElementAt(3).ToString());

            //Citim Regulile de productie
            for (int i = 4; i < 4 + _Grammar._NumberOfProductionRules; i++)
            {
                string Rules = FileContent.ElementAt(i);
                var RulesVector = Rules.Split(':');
                _Grammar.AddProductionRule(RulesVector);
            }
        }

        public void CheckAndModifyForDescendingAnalysis()
        {
            RemoveRulesOnSameLine();
            RemoveCommonStart();
            RemoveLeftRecursiveDerivations();
            //_Grammar._ProductionRules = _Grammar._ProductionRules.OrderBy(s => s._LHS).ToList();
            //_Grammar._Nonterminals = _Grammar._Nonterminals.OrderBy(s => s).ToList();
        }

        private void RemoveRulesOnSameLine()
        {
            foreach (var ProdRule in _Grammar._ProductionRules)
            {
                for (int i = 0; i < ProdRule._RHS.Count(); i++)
                {
                    var Count = ProdRule._RHS[i].Count(x => x == '|');
                    var NewRules = ProdRule._RHS[i].Split('|').ToList();

                    ProdRule._RHS.RemoveAt(i);
                    ProdRule._RHS.AddRange(NewRules);

                    _Grammar._NumberOfProductionRules += Count;
                }
            }

        }

        private void RemoveCommonStart()
        {
            int NonTerminalToAnalyze = 0;

            for (int i = NonTerminalToAnalyze; i < _Grammar._ProductionRules.Count(); i++, NonTerminalToAnalyze++)
            {
                //Sparg partea dreapta a regulilor de productie
                List<string> CopiedList = new List<string>();
                _Grammar._ProductionRules[i]._RHS.ForEach(s => CopiedList.Add(s));

                //Verific daca sunt elemente cu inceput comun pana raman cu lista goala
                while (CopiedList.Count() != 0)
                {
                    //Caut cel mai lung inceput comun pentru cel putin 2 reguli
                    string LookedUpString = LongestCommonStart(CopiedList);

                    if (LookedUpString == null)
                    {
                        CopiedList.Clear();
                        continue;
                    }

                    var FiltredList = from RHS in CopiedList
                                      where RHS.StartsWith(LookedUpString)
                                      select RHS;

                    //Cream Nonterminul care trebuie adaugat
                    string ToAddNonTerminal = _Grammar.CreateNewNonterminal(_Grammar._ProductionRules[i]._LHS);

                    //Cream regula care trebuie adaugata pentru Nonterminalul curent
                    string ModifiedRule = LookedUpString + " " + ToAddNonTerminal;

                    //Cream regulile care trebuie adaugate noului terminal si facem adaugarea
                    List<string> NewRules = new List<string>();
                    foreach (var Rule in FiltredList)
                    {
                        int CommonStartEndIndex = LookedUpString.Length + 1;
                        int UncommonStringStartIndex = Rule.Length - CommonStartEndIndex;

                        if (UncommonStringStartIndex > 0)
                        {
                            NewRules.Add(Rule.Substring(CommonStartEndIndex, UncommonStringStartIndex));
                        }
                        else
                        {
                            NewRules.Add("ε");
                        }
                    }
                    _Grammar._ProductionRules.Add(new ProductionRule(ToAddNonTerminal, NewRules));

                    //Adaugarea Nonterminaului in lista de Nonterminale
                    _Grammar._Nonterminals.Add(ToAddNonTerminal);

                    //Stergem regulile cu acelasi inceput
                    _Grammar._ProductionRules[i]._RHS.RemoveAll(s => s.StartsWith(LookedUpString));
                    CopiedList.RemoveAll(s => s.StartsWith(LookedUpString));

                    //Adaugam regula Nonterminalului curent
                    _Grammar._ProductionRules[i]._RHS.Add(string.Copy(ModifiedRule));

                    CopiedList.Clear();
                    _Grammar._ProductionRules[i]._RHS.ForEach(s => CopiedList.Add(s));
                }

            }

            //Setam noul numar de reguli de productie
            _Grammar._NumberOfProductionRules = 0;
            foreach (var ProdRules in _Grammar._ProductionRules)
            {
                _Grammar._NumberOfProductionRules += ProdRules._RHS.Count();
            }

        }

        private string LongestCommonStart(List<string> MyList)
        {
            List<List<string>> SplitedStrings = new List<List<string>>();
            MyList.ForEach(s => SplitedStrings.Add(s.Split(' ').ToList()));

            int MaxCounter = 0;
            int IndexOfMax = 0;

            for (int i = 0; i < SplitedStrings.Count(); i++)
            {
                for (int j = 0; j < SplitedStrings.Count(); j++)
                {
                    if (SplitedStrings[i] != SplitedStrings[j])
                    {
                        int Counter = MaxString(SplitedStrings[i], SplitedStrings[j]);
                        if (MaxCounter < Counter)
                        {
                            IndexOfMax = j;
                            MaxCounter = Counter;
                        }
                    }
                }
            }

            if (MaxCounter == 0)
            {
                return null;
            }

            string LongestString = SplitedStrings[IndexOfMax][0];
            for (int i = 1; i < MaxCounter; i++)
            {
                LongestString += " " + SplitedStrings[IndexOfMax][i];
            }
            return LongestString;
        }

        private int MaxString(List<string> List1, List<string> List2)
        {
            int MinLength = Math.Min(List1.Count(), List2.Count());
            int NumberOfCommonElements = 0;

            for (int i = 0; i < MinLength; i++)
            {
                if (List1[i] == List2[i])
                {
                    NumberOfCommonElements++;
                }
                else
                {
                    break;
                }
            }
            return NumberOfCommonElements;
        }

        private void RemoveLeftRecursiveDerivations()
        {
            int RemainingIndex = 1;
            int ProdRulesNumber = _Grammar._ProductionRules.Count();

            RemoveLeftRecursion(_Grammar._StartSymbol);
            for (int i = 1; i < ProdRulesNumber; i++, RemainingIndex++)
            {
                RemoveNonTerminalFromRHS(RemainingIndex);
                RemoveLeftRecursion(_Grammar._ProductionRules[i]._LHS);
            }

            _Grammar._NumberOfProductionRules = 0;
            foreach (var Nonter in _Grammar._ProductionRules)
            {
                _Grammar._NumberOfProductionRules += Nonter._RHS.Count();
            }
        }

        private void RemoveLeftRecursion(string NonTermianl)
        {
            //facem o copie a listei de reguli de productie pentru Nonterminalul dat ca parametru
            List<string> ProdRules = new List<string>();
            foreach (var Rules in _Grammar._ProductionRules)
            {
                if (Rules._LHS == NonTermianl)
                {
                    Rules._RHS.ForEach(s => ProdRules.Add(s));
                    break;
                }
            }

            //Cream lista cu regulile de productie ce au recursivitate stanga
            var LeftRec = (from Rule in ProdRules
                           where Rule.StartsWith(NonTermianl)
                           select Rule).ToList();

            if (LeftRec.Count() == 0)
            {
                return;
            }

            //Cream lista cu regulile de productie ce nu au recursivitate stanga
            var NoLeftRec = ProdRules.Except(LeftRec).ToList();

            //Cream Nonterminul care trebuie adaugat
            string ToAddNonTerminal = _Grammar.CreateNewNonterminal(NonTermianl);
            _Grammar._Nonterminals.Add(ToAddNonTerminal);

            //Scoatem din regulile de productie Nonterminalul care creeaza problema si adaug noul nonterminal
            for (int i = 0; i < LeftRec.Count(); i++)
            {
                LeftRec[i] = LeftRec[i].Substring(NonTermianl.Length + 1) + " " + ToAddNonTerminal;
            }

            //Creez Regulile de productie pentru noul terminal si le adaug
            LeftRec.Add("ε");
            ProductionRule NewRules = new ProductionRule(ToAddNonTerminal, LeftRec);
            _Grammar._ProductionRules.Add(NewRules);

            //Golesc lista de reguli de productie pentru Nonterminalul dat ca parametru
            int IndexOfNonterm = _Grammar._ProductionRules.FindIndex(a => a._LHS == NonTermianl);
            _Grammar._ProductionRules.ElementAt(IndexOfNonterm)._RHS.Clear();

            if (NoLeftRec == null)
            {
                _Grammar._ProductionRules.ElementAt(IndexOfNonterm)._RHS.Add(ToAddNonTerminal);
            }
            else
            {
                for (int i = 0; i < NoLeftRec.Count(); i++)
                {
                    if (NoLeftRec[i] == "ε")
                    {
                        NoLeftRec[i] = ToAddNonTerminal;
                    }
                    else
                    {
                        NoLeftRec[i] += " " + ToAddNonTerminal;
                    }
                }
                _Grammar._ProductionRules.ElementAt(IndexOfNonterm)._RHS = NoLeftRec;
            }
            return;
        }

        private void RemoveNonTerminalFromRHS(int RemainingIndex)
        {
            for (int NonterminalIndex = 0; NonterminalIndex < RemainingIndex; NonterminalIndex++)
            {
                for (int RuleIndex = 0; RuleIndex < _Grammar._ProductionRules[RemainingIndex]._RHS.Count(); RuleIndex++)
                {
                    List<string> NewRules = new List<string>();
                    if (_Grammar._ProductionRules[RemainingIndex]._RHS[RuleIndex].StartsWith(_Grammar._ProductionRules[NonterminalIndex]._LHS))
                    {
                        foreach (var Rule in _Grammar._ProductionRules[NonterminalIndex]._RHS)
                        {
                            NewRules.Add(_Grammar._ProductionRules[RemainingIndex]._RHS[RuleIndex].Replace(_Grammar._ProductionRules[NonterminalIndex]._LHS, Rule));
                        }
                        _Grammar._ProductionRules[RemainingIndex]._RHS.RemoveAt(RuleIndex);
                        _Grammar._ProductionRules[RemainingIndex]._RHS.AddRange(NewRules);
                    }
                }
            }
        }

        public void CalculateFirstAndFollow()
        {
            var MyList = SplitIt();
            int Counter = 0;
            var IWasHere = new List<string>();

            for (int NontermIndex = 0; NontermIndex < _Grammar._ProductionRules.Count(); NontermIndex++)
            {
                for (int RuleIndex = 0; RuleIndex < _Grammar._ProductionRules[NontermIndex]._RHS.Count(); RuleIndex++)
                {
                    if (MyList[Counter][1] == "ε")
                    {
                        List<string> results = CalculateFollow(MyList[Counter][0], MyList, IWasHere);
                        _First_Follow.Add(new First_Follow(NontermIndex, RuleIndex, results));
                    }
                    else
                    {
                        List<string> results = CalculateFirst(MyList[Counter][1], MyList);
                        _First_Follow.Add(new First_Follow(NontermIndex, RuleIndex, results));
                    }
                    Counter++;
                }
            }
        }

        private List<List<string>> SplitIt()
        {
            List<List<string>> MyList = new List<List<string>>();

            for (int NontermIndex = 0; NontermIndex < _Grammar._ProductionRules.Count(); NontermIndex++)
            {
                for (int RuleIndex = 0; RuleIndex < _Grammar._ProductionRules[NontermIndex]._RHS.Count(); RuleIndex++)
                {
                    string Rule = _Grammar._ProductionRules[NontermIndex]._RHS[RuleIndex];
                    List<string> SplitedRule = new List<string>();

                    SplitedRule.Add(_Grammar._ProductionRules[NontermIndex]._LHS);

                    if (Rule.Contains("   "))
                    {
                        Rule = Rule.Replace("   ", " ");
                        SplitedRule.Add(" ");
                    }
                    else if (Rule.Contains("  "))
                    {
                        Rule = Rule.Replace("  ", "");
                        SplitedRule.Add(" ");
                    }
                    var TermVector = Rule.Split(' ');
                    SplitedRule.AddRange(TermVector);
                    MyList.Add(SplitedRule);
                }
            }
            return MyList;
        }

        private List<string> CalculateFirst(string Element, List<List<string>> ProdRulesList)
        {
            List<string> MyList = new List<string>();

            if (IsATerminal(Element) || Element == "ε")
            {
                MyList.Add(Element);
                return MyList;
            }
            var ProdRulesOfElem = (from Rule in ProdRulesList
                                   where Rule[0] == Element
                                   select Rule).ToList();

            foreach (var Rule in ProdRulesOfElem)
            {
                if (Rule[1] == " ")
                {
                    MyList.Add(" ");
                    return MyList;
                }

                MyList.AddRange(CalculateFirst(Rule[1], ProdRulesList));
                if (MyList.Contains("ε") && Rule.Count() > 2)
                {
                    MyList.AddRange(CalculateFirst(Rule[2], ProdRulesList));
                }
            }

            return MyList;
        }

        private List<string> CalculateFollow(string Element, List<List<string>> ProdRulesList, List<string> IWasHere)
        {
            List<string> MyList = new List<string>();
            IWasHere.Add(Element);

            if (Element == _Grammar._StartSymbol)
            {
                MyList.Add("$");
            }

            var ProdRulesOfElem = (from Rule in ProdRulesList
                                   where (Rule.Contains(Element))
                                   select Rule).ToList();

            foreach (var Rule in ProdRulesOfElem)
            {
                var ElementIndexes = Enumerable.Range(1, Rule.Count()-1).Where(s => Rule[s] == Element).ToList();

                foreach(var ElementInd in ElementIndexes)
                {
                    if(Rule[0] != Element && ElementInd == (Rule.Count()-1))
                    {
                        if (!IWasHere.Contains(Rule[0]))
                        {
                            MyList.AddRange(CalculateFollow(Rule[0], ProdRulesList, IWasHere));
                        }
                    }
                    else 
                    {
                        if (ElementInd != (Rule.Count() - 1))
                        {
                            MyList.AddRange(CalculateFirst(Rule.ElementAt(ElementInd + 1), ProdRulesList));
                            if (MyList.Contains("ε"))
                            {
                                MyList.RemoveAll(s => s == "ε");
                                if (!IWasHere.Contains(Rule[0]))
                                {
                                    MyList.AddRange(CalculateFollow(Rule[0], ProdRulesList, IWasHere));
                                }
                            }
                        }
                    }
                }
            }

            MyList = MyList.Distinct().ToList();
            MyList.RemoveAll(s => s == "ε");

            IWasHere.Remove(Element);

            return MyList;
        }

        private bool IsATerminal(string Element)
        {
            foreach (var Term in _Grammar._Terminals)
            {
                if (Term == Element)
                    return true;
            }
            return false;
        }

        public bool IsGrammarLL1()
        {
            for (int i = 0; i < _First_Follow.Count(); i++)
            {
                for (int j = i + 1; j < _First_Follow.Count(); j++)
                {
                    if (_First_Follow[i]._NonTerminalIndex == _First_Follow[j]._NonTerminalIndex)
                    {
                        foreach (var Element in _First_Follow[i]._Elements)
                        {
                            if (_First_Follow[j]._Elements.Contains(Element))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool IsInAList(string Nonterm, List<string> List)
        {
            foreach (var Nonterminal in List)
            {

            }
            return false;
        }

        public void ConstructTheTabel()
        {
            foreach (var Nonterm in _Grammar._Nonterminals)
            {
                var First_FollowForNonterm = (from First_Follow in _First_Follow
                                              where _Grammar._ProductionRules[First_Follow._NonTerminalIndex]._LHS == Nonterm
                                              select First_Follow).ToList();
                TabelNonTerminal tabelNonTerminal = new TabelNonTerminal();
                tabelNonTerminal._Nonterminal = Nonterm;

                foreach (var First_Follow in First_FollowForNonterm)
                {
                    string Rule = _Grammar._ProductionRules[First_Follow._NonTerminalIndex]._RHS[First_Follow._RuleIndex];

                    List<string> SplitedRule = Rule.Split(' ').ToList();

                    foreach (var Term in First_Follow._Elements)
                    {
                        TabelElement tabelElement = new TabelElement();
                        tabelElement._Terminal = Term;
                        tabelElement._Rule = SplitedRule;
                        tabelNonTerminal._Terminals.Add(tabelElement);
                    }
                }
                _Tabel.Add(tabelNonTerminal);
            }

            foreach (var First_Follow in _First_Follow)
            {
                string Rule = _Grammar._ProductionRules[First_Follow._NonTerminalIndex]._RHS[First_Follow._RuleIndex];
                var SplitedRule = Rule.Split(' ');
            }
        }

        public string GenerateGrammarCode()
        {
            StringBuilder GrammarCode = new StringBuilder();
            GrammarCode.Append(@"
namespace AnalizorLL1
{
    using System;
    class Analizor
    {
        static string[] Input;   
        static int Index;

        static void Main(string[] args)
        {
            var Inp = Console.ReadLine();
            Inp += "" $"";
            Input = Inp.Split(' ');
            try
            {");

            //Adaug Functia cu numele Simbolului de start
            GrammarCode.Append($"\n\t\t{_Grammar._StartSymbol}();");

            GrammarCode.Append(@"
            }
            catch (Exception Excep)
            {
                Console.WriteLine(Excep.Message);
            }

            if (Input[Index] == ""$"")
            {
                Console.WriteLine(""Propozitia este corecta"");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine(""Propozitia este incorecta"");
                Console.ReadKey();
            }
        }");

            foreach (var TabelNonterminal in _Tabel)
            {
                GrammarCode.AppendLine();
                GrammarCode.Append($"\n\tstatic void {TabelNonterminal._Nonterminal}()\n\t{{");

                GrammarCode.Append(GenerateFunctionCode(TabelNonterminal._Terminals));

                GrammarCode.Append("\n\t}");
            }

            GrammarCode.Append(@"
    } 
}
                                ");

            return GrammarCode.ToString();
        }

        private string GenerateFunctionCode(List<TabelElement> Elements)
        {
            StringBuilder Code = new StringBuilder();
            StringBuilder Terminals = new StringBuilder();

            Code.AppendLine();
            foreach (var Rule in Elements)
            {
                string Term = "\"" + Rule._Terminal + "\"";
                Code.Append($"\t\tif (Input[Index] == {Term})\n\t\t{{");

                Code.Append(GenerateCodeForThisElement(0, Rule._Rule));

                Code.Append("\n\t\t}\n\t\telse");
                Terminals.Append($" {Rule._Terminal} ");
            }

            Code.Append($"\n\t\t{{\n\t\t\tstring error = \"Lipseste unul dintre atomii {Terminals.ToString()} de la indexul \";");
            Code.Append($"\n\t\t\terror += Index.ToString();");
            Code.Append($"\n\t\t\tthrow new Exception(error);");
            Code.Append("\n\t\t}");
            
            return Code.ToString();
        }

        private string GenerateCodeForThisElement(int ElemenIndex, List<string> ElementList)
        {
            if (ElemenIndex == ElementList.Count())
                return "";

            var RuleElement = ElementList[ElemenIndex];
            StringBuilder Code = new StringBuilder();

            if (ElemenIndex == 0 && IsATerminal(RuleElement))
            {
                Code.Append($"\n\t\t\tIndex++;");
                Code.Append(GenerateCodeForThisElement(ElemenIndex + 1, ElementList));
                return Code.ToString();
            }

            if (_Grammar._Nonterminals.Contains(RuleElement))
            {
                Code.Append($"\n\t\t\t{RuleElement}();");
                Code.Append(GenerateCodeForThisElement(ElemenIndex + 1, ElementList));
            }
            else if (RuleElement == "ε")
            {
                Code.Append("\n\t\t\treturn;");
            }
            else
            {
                string Term = "\"" + RuleElement + "\"";
                Code.Append($"\n\t\t\tif (Input[Index] == {Term})\n\t\t\t{{");
                Code.Append($"\n\t\t\tIndex++;");

                Code.Append(GenerateCodeForThisElement(ElemenIndex + 1, ElementList));

                Code.Append("\n\t\t\t}");
                
                Code.Append($"\n\t\t\telse\n\t\t\t{{\n\t\t\t\tstring error = \"Lipseste atomul {RuleElement} de la indexul \";");
                Code.Append($"\n\t\t\t\terror += Index.ToString();");
                Code.Append($"\n\t\t\t\tthrow new Exception(error);");
                Code.Append("\n\t\t\t}");
            }
            return Code.ToString();
        }
    }
}
