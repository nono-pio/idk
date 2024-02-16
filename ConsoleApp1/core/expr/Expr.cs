using ConsoleApp1.core.atoms;
using ConsoleApp1.core.expr.atoms;
using ConsoleApp1.core.expr.fonctions;
using ConsoleApp1.core.expr.fonctions.@base;

namespace ConsoleApp1.core.expr;

public abstract class Expr : IComparable<Expr>, IEqualityComparer<Expr>
{
    /// Args = arguments/parametres de l'expression (thermes d'une addition, facteur d'un produit)
    public Expr[] Args;

    protected Expr(params Expr[] args)
    {
        Args = args;
    }

    public int CompareTo(Expr? expr)
    {
        if (expr is null) return -1;

        // cmp type
        var cmpType = TypeId().CompareTo(expr.TypeId());
        if (cmpType != 0) return cmpType;

        if (this is Atom) return ((Atom)this).CompareAtom((Atom?)expr);

        // cmp args length
        var cmpLength = Args.Length.CompareTo(expr.Args.Length);
        if (cmpLength != 0) return cmpLength;

        // cmp args
        for (var i = 0; i < Args.Length; i++)
        {
            var cmpArg = Args[i].CompareTo(expr.Args[i]);
            if (cmpArg != 0) return cmpArg;
        }

        Console.WriteLine("cmp > " + this + "=" + expr);
        return 0;
    }

    // <-- Comparison -->

    public bool Equals(Expr? x, Expr? y)
    {
        if (x is null || y is null) return false;

        return x.Equals(y);
    }

    public int GetHashCode(Expr obj)
    {
        return obj.GetHashCode();
    }

    // <-- String Méthodes -->

    /// Génère un string représentant l'expression en utilisant le Latex
    public abstract string? ToLatex();

    // Génère un string représentant l'expression
    //public abstract new string? ToString();

    protected string? Join(string separator)
    {
        if (Args.Length == 0) return "";

        var result = ElementWithParenthesis(0);
        for (var i = 1; i < Args.Length; i++) result += separator + ElementWithParenthesis(i);

        return result;
    }

    protected string? ElementWithParenthesis(int i)
    {
        var str = Args[i].ToString();

        if (OpPriorite() < Args[i].OpPriorite()) return Parenthesis(str);

        return str;
    }

    protected string? Parenthesis(string? str)
    {
        return $"({str})";
    }

    protected string? ElementWithParenthesisLatex(int i)
    {
        var str = Args[i].ToLatex();

        if (OpPriorite() < Args[i].OpPriorite()) return ParenthesisLatex(str);

        return str;
    }

    protected string? ParenthesisLatex(string? str)
    {
        return $"\\right({str}\\left)";
    }

    protected string? JoinLatex(string separator)
    {
        if (Args.Length == 0) return "";

        var result = ElementWithParenthesisLatex(0);
        for (var i = 1; i < Args.Length; i++) result += separator + ElementWithParenthesisLatex(i);

        return result;
    }

    // <-- Outils Mathématiques -->


    public Expr Gcd(Expr b)
    {
        var a = this;
        if (a is Number a_num && a_num.IsEntier() && b is Number b_num && b_num.IsEntier())
        {
            return Number.Gcd((uint) a_num.Num, (uint) b_num.Num).Expr();
        }

        throw new NotImplementedException();
    }
    
    /// Retourne la réciproque en
    /// <paramref name="argIndex" />
    /// de l'expression sur
    /// <paramref name="y" />
    /// .
    /// <example>
    ///     x + 1 = y --> y - 1 avec <paramref name="argIndex" /> = 0
    ///     <para />
    ///     x + 1 = y --> y - x avec <paramref name="argIndex" /> = 1
    /// </example>
    public abstract Expr Inverse(Expr y, int argIndex);

    /// Retourne la dérivee de l'expression en la variable
    /// <paramref name="variable" />
    public abstract Expr Derivee(string variable);

    // <-- Comparaison Spéciales -->

    /// x == 0
    public bool IsZero()
    {
        return this is Number { Num: 0 };
    }
    
    public bool IsNotZero()
    {
        return !IsZero();
    }

    /// x == 1
    public bool IsOne()
    {
        return this is Number { Num: 1 };
    }

    /// x == -1
    public bool IsNegOne()
    {
        return this is Number { Num: -1 };
    }

    /// x == n avec n un nombre
    public bool Is(double n)
    {
        return this is Number number && Number.Equal(n, number.Num);
    }

    /// Test si l'expression est une variable nommé
    /// <paramref name="variable" />
    public bool IsVar(string variable)
    {
        return this is Variable var && var.Name == variable;
    }


    // <-- Outils -->

    /// Priorité des opérations
    /// <list type="table">
    ///     <item>
    ///         <term>0</term>
    ///         <description>atome - fonction</description>
    ///     </item>
    ///     <item>
    ///         <term>1</term>
    ///         <description>puissance</description>
    ///     </item>
    ///     <item>
    ///         <term>2</term>
    ///         <description>multiplication</description>
    ///     </item>
    ///     <item>
    ///         <term>3</term>
    ///         <description>addition</description>
    ///     </item>
    ///     <item>
    ///         <term>4</term>
    ///         <description>autre</description>
    ///     </item>
    /// </list>
    public int OpPriorite()
    {
        return this switch
        {
            Atom or Fonction => 0,
            Power => 1,
            Multiplication => 2,
            Addition => 3,
            _ => 4
        };
    }

    /// Test si l'expression dépend de la variable
    /// <paramref name="variable" />
    public bool Constant(string variable)
    {
        if (IsVar(variable)) return false;

        foreach (var arg in Args)
            if (!arg.Constant(variable))
                return false;

        return true;
    }

    // TODO
    /// Retourne l'expression sous la forme a * x où a est un nombre et x une expression
    public virtual (double, Expr?) HasMulCoef()
    {
        return (1, this);
    }

    /// Test si un il y a un type
    /// <typeparamref name="T" />
    /// dans l'expression
    public bool Has<T>() where T : Expr
    {
        if (IsType<T>())
            return true;

        foreach (var arg in Args)
            if (arg.Has<T>())
                return true;

        return false;
    }

    /// Compte le nombre de type
    /// <typeparamref name="T" />
    /// dans l'expression
    public int Count<T>() where T : Expr
    {
        var count = 0;

        for (var i = 0; i < Args.Length; i++) count += Args[i].Count<T>();

        if (IsType<T>()) count++;

        return count;
    }


    /// Map chaque type
    /// <typeparamref name="T" />
    /// avec la fonction
    /// <paramref name="func" />
    public Expr Map<T>(Func<T, Expr> func) where T : Expr
    {
        for (var i = 0; i < Args.Length; i++) Args[i] = Args[i].Map(func);


        if (GetType() == typeof(T)) return func((T)this);

        return this;
    }

    public Expr MapAtoms(Func<Atom, Expr> func)
    {
        if (this is Atom) return func((Atom)this);

        for (var i = 0; i < Args.Length; i++) Args[i] = Args[i].MapAtoms(func);

        return this;
    }

    public Expr MapBottomUp(Func<Expr, Expr> func)
    {
        for (var i = 0; i < Args.Length; i++) Args[i] = Args[i].MapBottomUp(func);

        return func(this);
    }

    public void SortArgs()
    {
        Args = Args.Order().ToArray();
    }

    public long TypeId()
    {
        return GetType().TypeHandle.Value.ToInt64();
    }

    public bool IsType<T>()
    {
        return GetType() == typeof(T);
    }

    public override bool Equals(object? obj)
    {
        Console.WriteLine("Equals > " + this + ";" + obj);
        if (obj is not Expr expr) return false;

        return this == expr;
    }

    public override int GetHashCode()
    {
        return Args.GetHashCode();
    }

    public static bool operator ==(Expr expr1, Expr expr2)
    {
        return expr1.CompareTo(expr2) == 0;
    }

    public static bool operator !=(Expr expr1, Expr expr2)
    {
        return expr1.CompareTo(expr2) != 0;
    }

    public static bool operator >(Expr expr1, Expr expr2)
    {
        return expr1.CompareTo(expr2) == 1;
    }

    public static bool operator <(Expr expr1, Expr expr2)
    {
        return expr1.CompareTo(expr2) == -1;
    }

    public static bool operator >=(Expr expr1, Expr expr2)
    {
        var cmp = expr1.CompareTo(expr2);
        return cmp == 0 || cmp == 1;
    }

    public static bool operator <=(Expr expr1, Expr expr2)
    {
        var cmp = expr1.CompareTo(expr2);
        return cmp == 0 || cmp == -1;
    }

    // <-- Operator -->

    // Addition
    public static Expr operator +(double num, Expr expr)
    {
        return Add(Num(num), expr);
    }

    public static Expr operator +(Expr expr, double num)
    {
        return Add(expr, Num(num));
    }

    public static Expr operator +(Expr expr1, Expr expr2)
    {
        return Add(expr1, expr2);
    }

    // Subtraction
    public static Expr operator -(double num, Expr expr)
    {
        return Sub(Num(num), expr);
    }

    public static Expr operator -(Expr expr, double num)
    {
        return Sub(expr, Num(num));
    }

    public static Expr operator -(Expr expr1, Expr expr2)
    {
        return Sub(expr1, expr2);
    }

    // Negate
    public static Expr operator -(Expr expr)
    {
        return Neg(expr);
    }

    // Multiplication
    public static Expr operator *(double num, Expr expr)
    {
        return Mul(Num(num), expr);
    }

    public static Expr operator *(Expr expr, double num)
    {
        return Mul(expr, Num(num));
    }

    public static Expr operator *(Expr expr1, Expr expr2)
    {
        return Mul(expr1, expr2);
    }


    // Division
    public static Expr operator /(double num, Expr expr)
    {
        return Div(Num(num), expr);
    }

    public static Expr operator /(Expr expr, double num)
    {
        return Div(expr, Num(num));
    }

    public static Expr operator /(Expr expr1, Expr expr2)
    {
        return Div(expr1, expr2);
    }
}