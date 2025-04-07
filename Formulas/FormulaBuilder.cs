using System.Reflection;
using TextExcel3.Formulas.Components;

namespace TextExcel3.Formulas;

/// <summary>
/// Builds a formula based on a string input.
/// </summary>
public class FormulaBuilder
{
    /// <summary>
    /// Translates from traditional infix notation to RPN using the Shunting Yard algorithm.
    /// Includes support for non-primitives including function calls and cell references/ranges.
    /// </summary>
    /// <param name="infix">Arithmetic in infix notation</param>
    /// <returns>The RPN equivalent of the arithmetic in `infix`</returns>
    public static string ShuntingYard(string infix)
    {
        List<string> tokens = TokenizeMath(infix);
        
        Queue<string> output = [];
        Stack<string> operators = [];

        foreach (string t in tokens)
        {
            if (t is "+" or "-" or "*" or "/" or "^")
            {
                while (operators.TryPeek(out string? prev) && CheckPrecedence(t, prev))
                {
                    output.Enqueue(operators.Pop());
                }
                
                operators.Push(t);
                continue;
            }
            
            output.Enqueue(t);
        }
        
        while (operators.TryPop(out string? op))
        {
            output.Enqueue(op);
        }
        
        return string.Join(" ", output);
    }

    private static List<string> TokenizeMath(string infix)
    {
        List<string> tokens = [""];
        int parens = 0;
        foreach (char c in infix)
        {
            if (c == ')')
            {
                tokens[^1] += c;
                parens--;
            }
            else if (parens > 0) tokens[^1] += c;
            else if (c == '(')
            {
                tokens[^1] += c;
                parens++;
            }
            else if (c == ' ')
            {
                tokens.Add("");
            }
            else
            {
                tokens[^1] += c;
            }
        }

        return tokens;
    }

    /// <summary>
    /// check if operator 2 has higher precedence than operator 1
    /// </summary>
    /// <param name="op1"></param>
    /// <param name="op2"></param>
    /// <returns></returns>
    private static bool CheckPrecedence(string op1, string op2) =>
        // checking if op2 is HIGHER THAN or EQUAL TO op1 in PEMDAS
        op2 switch
        {
            "^" => true, // ^ is the highest precedence
            "*" or "/" => op1 is not "^", // * / must be higher or equal unless the other one is ^
            "+" or "-" => op1 is "+" or "-", // + - can only be equal, and only if the other is also + or -
            _ => throw new FormulaException("Invalid operator " + op2, "#NAME")
        };

    /// <summary>
    /// Converts a reverse polish notation string into a formula tree.
    /// </summary>
    /// <param name="rpn">Arithmetic in RPN</param>
    /// <returns>A formula term node tree equivalent to the given RPN</returns>
    public static string RpnToFormulaTerm(string rpn)
    {
        Stack<string> stack = [];
        List<string> tokens = TokenizeMath(rpn);

        foreach (string t in tokens)
        {
            stack.Push(t);
            if (stack.Peek() is "*" or "/" or "+" or "-" or "^")
            {
                string op = stack.Pop();
                string arg2 = stack.Pop();
                string arg1 = stack.Pop();
                string functionName = GetFunctionNameForOperator(op);
                stack.Push(functionName + "(" + arg1 + ", " + arg2 + ")");
            }
        }
        
        return stack.Pop();
    }

    private static string GetFunctionNameForOperator(string op) =>
        op switch
        {
            "*" => "Multiply",
            "/" => "Divide",
            "+" => "Add",
            "-" => "Subtract",
            "^" => "Power",
            _ => throw new FormulaException("Invalid operator " + op, "#NAME")
        };

    public static IFormulaTerm BuildFormula(string input, Spreadsheet sheet)
    {
        
        // for now, only implement tolerance for strict functions
        // no infix notation; cell refs will come soon
        // therefore, a formula is just a tree of function calls
        // the outermost is always the root of the tree, and there
        // is only one root
        
        // this function will recursively generate the tree
        // and return the root of the tree
        
        // before anything, check if this is a primitive type
        // that's the base case
        if (CheckPrimitiveType(input, out IFormulaTerm? term) && term != null)
        {
            return term;
        }
        
        // check for a single cell reference
        if (CheckSingleCellReference(input, sheet, out IFormulaTerm? result) && result != null)
        {
            return result;
        }
        
        // check if this is infix arithmetic
        bool infix = false;
        int parens = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '(') parens++;
            if (input[i] == ')') parens--;
            if (parens > 0) continue;
            if (input[i] is '+' or  '*' or '/' or '^' || input[i] == '-' && input[i+1] == ' ')
            {
                infix = true;
                break;
            }
        }

        // if it is, convert it to RPN and then to function calls
        if (infix)
        {
            string rpn = ShuntingYard(input);
            return BuildFormula(RpnToFormulaTerm(rpn), sheet);
        }
        
        // first, get the outermost function
        string functionName = GetFunctionName(input);
        
        // if the function's name is "", it's just parens with no function
        // we can just return the inner formula
        if (functionName == "")
        {
            return BuildFormula(input[1..^1], sheet);
        }
        
        // then, get the function's arguments
        //string[] args = input[(input.IndexOf("(") + 1)..^1].Split(',');
        //args = args.Select(a => a.Trim()).ToArray();
        List<string> args = [""];
        parens = 0; // only consider the outermost parens-- ignore anything that we see when parens != 0
        foreach (char c in input[(input.IndexOf("(") + 1)..^1])
        {
            if (c == '(') parens++;
            if (c == ')') parens--;
            if (c == ',' && parens == 0) args.Add(string.Empty);
            else args[^1] += c;
        }
        if (parens != 0) throw new FormulaException("Unmatched parentheses", "#SYNTAX");
        
        args = args.Select(a => a.Trim()).ToList();
        
        // check for cell ranges and replace them with cell references
        for (int i = 0; i < args.Count; i++)
        {
            if (CheckReferenceRange(args[i], out string[]? cellRefs) && cellRefs != null)
            {
                args.RemoveAt(i);
                args.AddRange(cellRefs);
                i--;
            }
        }
        
        // get an IFormulaTerm from each argument
        List<IFormulaTerm> formulaArgs = [];
        for (int i = 0; i < args.Count; i++)
        {
            formulaArgs.Add(BuildFormula(args[i], sheet));
        }
        
        // now, use reflection to get a Func<IFormulaTerm[], decimal> from the function name
        // this is a bit of a hack, but it works
        Type type = typeof(FormulaFunction);
        MethodInfo? method = type.GetMethod(functionName, BindingFlags.Static | BindingFlags.Public);
        if (method == null)
        {
            throw new FormulaException("Function " + functionName + " not found", "#NAME");
        }
        
        // create a delegate from the method
        Func<IFormulaTerm[], decimal> func = (Func<IFormulaTerm[], decimal>)Delegate.CreateDelegate(typeof(Func<IFormulaTerm[], decimal>), method);
        
        // return a new FormulaFunction
        return new FormulaFunction(func, formulaArgs.ToArray());
    }

    /// <summary>
    /// gets the name of the formula function from the input string, normalized based on case
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string GetFunctionName(string input)
    {
        try
        {
            int parens = input.IndexOf('(');
            if (parens == -1) return "";
            string result = input[..parens].Trim().ToLower();
            if (result.Length == 0) return "";
            return char.ToUpper(result[0]) + result[1..];
        }
        catch (Exception e)
        {
            throw new FormulaException("Malformed formula near " + input, "#SYNTAX", e);
        }
    }

    /// <summary>
    /// Attempts to parse the given string into a primitive type.
    /// </summary>
    /// <param name="input">The string to attempt to record as a primitive type</param>
    /// <param name="result">An output variable which will be set to a PrimitiveType instance if successful, or null otherwise.</param>
    /// <returns>true if the string is a primitive type, false otherwise.</returns>
    private static bool CheckPrimitiveType(string input, out IFormulaTerm? result)
    {
        input = input.ToLower();
        if (input == "true") 
            result = new PrimitiveType<bool>(true);
        else if (input == "false")
            result = new PrimitiveType<bool>(false);
        else if (decimal.TryParse(input, out decimal val)) 
            result = new PrimitiveType<decimal>(val);
        
        // DateTime and TimeSpan are not supported yet
        else result = null;
        
        return result != null;
    }
    
    /// <summary>
    /// Checks for a cell reference or reference range. If a range is found, the result will be a list of cell references computed from the range.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool CheckCellReference(string input, Spreadsheet sheet, out IFormulaTerm[]? result)
    {
        if (CheckReferenceRange(input, sheet, out result))
        {
            return true;
        }

        if (CheckSingleCellReference(input, sheet, out IFormulaTerm singleResult) && singleResult != null)
        {
            result = [singleResult];
            return true;
        }

        result = null;
        return false;
    }

    private static bool CheckSingleCellReference(string input, Spreadsheet sheet, out IFormulaTerm? result)
    {
        try
        {
            result = new CellReference(new SpreadsheetLocation(input), sheet);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static bool CheckReferenceRange(string input, Spreadsheet sheet, out IFormulaTerm[]? result)
    {
        if (input.Contains('('))
        {
            result = null;
            return false;
        }
        
        if (input.Contains(':'))
        {
            string[] parts = input.Split(':');
            if (parts.Length != 2)
            {
                result = null;
                return false;
            }

            try
            {

                SpreadsheetLocation loc1 = new(parts[0]);
                SpreadsheetLocation loc2 = new(parts[1]);
                List<IFormulaTerm> refs = new();

                for (int r = loc1.Row; r <= loc2.Row; r++)
                {
                    for (int c = loc1.Column; c <= loc2.Column; c++)
                    {
                        refs.Add(new CellReference(new SpreadsheetLocation(c, r), sheet));
                    }
                }

                result = refs.ToArray();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        result = null;
        return false;
    }
    
    private static bool CheckReferenceRange(string input, out string[]? result)
    {
        if (input.Contains('('))
        {
            result = null;
            return false;
        }
        
        if (input.Contains(':'))
        {
            string[] parts = input.Split(':');
            if (parts.Length != 2)
            {
                result = null;
                return false;
            }

            try
            {
                SpreadsheetLocation loc1 = new(parts[0]);
                SpreadsheetLocation loc2 = new(parts[1]);
                List<SpreadsheetLocation> refs = [];

                for (int r = loc1.Row; r <= loc2.Row; r++)
                {
                    for (int c = loc1.Column; c <= loc2.Column; c++)
                    {
                        refs.Add(new SpreadsheetLocation(c, r));
                    }
                }

                result = refs.Select(loc => loc.ToString()).ToArray()!;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        result = null;
        return false;
    }
}