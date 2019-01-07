using Microsoft.CSharp;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace AnalizorLL1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Analyzer _LL1;
        private string _GrammarCode;

        public MainWindow()
        {
            InitializeComponent();
            _LL1 = new Analyzer();
        }

        private void Open_File_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select grammar";
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                if (openFileDialog.CheckFileExists == false)
                {
                    MessageBox.Show("File doesn't exist!/nPlease Try again!");
                }
                else
                {
                    if (_LL1._Grammar._StartSymbol != null)
                    {
                        _LL1 = new Analyzer();
                    }
                    _LL1.SetNewGrammar(openFileDialog.FileName);

                    Grammar_TextBox.Text = _LL1._Grammar.PrintProductionRules();

                }
            }
        }

        private void Modify_Grammar_Click(object sender, RoutedEventArgs e)
        {
            _LL1.CheckAndModifyForDescendingAnalysis();
            ModifiedGrammar_TextBox.Text = _LL1._Grammar.PrintProductionRules();
        }

        private void CheckIfLL1_Click(object sender, RoutedEventArgs e)
        {
            _LL1.CalculateFirstAndFollow();
            First_Follow_TextBox.Text = _LL1.PrintFirst_Follow();

            if (_LL1.IsGrammarLL1())
            {
                _LL1.IsOkToCompile = true;
                MessageBox.Show("E gramatica LL(1)!");
            }
            else
            {
                MessageBox.Show("Gramatica nu a putut fi transformata in gramatica LL(1)!");
            }

        }

        private void Complie_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_LL1.IsOkToCompile == true)
            {
                _LL1.ConstructTheTabel();
                _GrammarCode = _LL1.GenerateGrammarCode();

                GrammarCodeWindow CodeWindow = new GrammarCodeWindow();
                CodeWindow.CodeTextBox.Text = _GrammarCode;
                CodeWindow.Show();
            }
        }

        private void RunProgram_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_LL1.IsOkToCompile == true)
            {
                CodeDomProvider CodeProvider = new CSharpCodeProvider();
                CompilerParameters Parameters = new CompilerParameters(new[] { "System.dll" }, "AnalizorLL1Implementation.exe", true);
                Parameters.GenerateExecutable = true;
                CompilerResults Results = CodeProvider.CompileAssemblyFromSource(Parameters, _GrammarCode);

                if (Results.Errors.HasErrors)
                {
                    GrammarCodeWindow CodeWindow = new GrammarCodeWindow();
                    StringBuilder Errors = new StringBuilder();
                    foreach (CompilerError Error in Results.Errors)
                    {
                        Errors.Append(Error.ErrorText);
                        Errors.AppendLine();
                    }
                    CodeWindow.CodeTextBox.Text = Errors.ToString();
                    CodeWindow.Show();
                }
                else
                {
                    Process.Start(Results.PathToAssembly, null);
                    MessageBox.Show("Build succeeded!");
                }
            }
        }
    }
}
