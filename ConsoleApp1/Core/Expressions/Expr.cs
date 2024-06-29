using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions;

public abstract class Expr
{
    /// Args = arguments/parametres de l'expression (thermes d'une addition, facteur d'un produit)
    public Expr[] Args { get; protected set; }

    protected Expr(params Expr[] args)
    {
        Args = args;
    }
    
    # region <-- Conversion -->

    public Fonction AsFonction(string variable)
    {
        return new Fonction(this, variable);
    }
    
    // TODO
    /// Retourne l'expression sous la forme a * x où a est un nombre et x une expression
    public virtual (double, Expr?) AsMulCoef()
    {
        return (1, this);
    }
    
    // 2x -> 2, x / 2 -> 2, 1
    public (int, Expr) AsMulInt()
    {
        if (this is not Multiplication mul)
            return (1, this);
        
        
        var coef = 1;
        List<Expr> rest = [];
        var isChange = false;
        foreach (var factor in mul.Factors)
        {
            if (factor is Number num && num.IsEntier())
            {
                isChange = true;
                coef *= (int) num.Num;
            }
            else
            {
                rest.Add(factor);
            }
        }

        return isChange ? (coef, Mul(rest.ToArray())) : (coef, mul);
    }
    
    
    // ax -> a, x / ae^x -> a, e^x
    // (constant, not contant)
    public (Expr, Expr) AsMulCsteNCste(string variable)
    {
        if (this is not Multiplication mul)
            return Constant(variable) ? (this, Un) : (Un, this);
        
        
        List<Expr> constantPart = [];
        List<Expr> variablePart = [];
        foreach (var factor in mul.Factors)
        {
            if (factor.Constant(variable))
                constantPart.Add(factor);
            else
                variablePart.Add(factor);
        }

        if (constantPart.Count == 0)
            return (Un, this);
        
        if (variablePart.Count == 0)
            return (this, Un);
        
        return (Mul(constantPart.ToArray()), Mul(variablePart.ToArray()));
    }

    // e^(2x) -> (e^x)^2
    public (int, Expr) AsPowInt()
    {
        if (this is not Power pow)
            return (1, this);
        
        var (n, newExp) = pow.Exp.AsMulInt();
        return (n, Pow(pow.Base, newExp));
    }


    public virtual (Expr, Expr) AsComplex()
    {
        throw new Exception($"Cannot convert {GetType()} as complex");
    }

    # endregion

    # region <-- String Méthodes -->

    /// Génère un string représentant l'expression en utilisant le Latex
    public abstract string ToLatex();

    public string ParenthesisLatexIfNeeded(Expr expr)
    {
        return expr.GetOrderOfOperation() < GetOrderOfOperation() ? LatexUtils.Parenthesis(expr.ToLatex()) : expr.ToLatex();
    }
    
    # endregion

    # region <-- Outils Mathématiques -->

    public enum OrderOfOperation : int
    {
        Always = 0,             // Tous le temps des parenthèses
        Addition = 1,           // Addition, Soustraction
        Multiplication = 2,     // Multiplication, Division (scalaires et matriciels)
        Power = 3,              // Puissance, Racine
        Atom = int.MaxValue,    // Nombre, Variable, Fonction
    }
    public virtual OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Always;
    }
    
    public bool ExprNeedParenthesis(Expr expr)
    {
        return expr.GetOrderOfOperation() < GetOrderOfOperation();
    }
    
    public abstract double N();
    
    public Expr Substitue(string variable, Expr value)
    {
        return Map<Variable>(var => var.Name == variable ? value : var);
    }

    public Expr Gcd(Expr b)
    {
        var a = this;
        if (a is Number a_num && a_num.IsEntier() && b is Number b_num && b_num.IsEntier())
        {
            return Number.Gcd((int) a_num.Num, (int) b_num.Num).Expr();
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
    
    public virtual Expr Derivee(string variable, int n)
    {
        var result = this;
        for (var i = 0; i < n; i++) 
            result = result.Derivee(variable);

        return result;
    }
    
    # endregion

    # region <-- Outils -->

    public IEnumerable<Expr> GetEnumerableTherms()
    {
        if (this is Addition add)
        {
            return add.Therms.SelectMany(factor => factor.GetEnumerableTherms());
        }
        return [ this ];
    }
    
    public IEnumerable<Expr> GetEnumerableFactors()
    {
        if (this is Multiplication mul)
        {
            return mul.Factors.SelectMany(factor => factor.GetEnumerableFactors());
        }
        return [ this ];
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

    /// Test si un il y a un type
    /// <typeparamref name="T" />
    /// dans l'expression
    public bool Has<T>() where T : Expr
    {
        if (this is T)
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

        foreach (var arg in Args)
            count += arg.Count<T>();

        if (this is T) count++;

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

    protected void SortArgs()
    {
        Args = Args.Order().ToArray();
    }
    
    # endregion
    
    # region <-- Math Operator -->
    
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
    
    # endregion
    
    # region <-- Comparaison -->
    
    protected long TypeId()
    {
        return GetType().TypeHandle.Value.ToInt64();
    }
    
    public override int GetHashCode()
    {
        return Args.GetHashCode();
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
        
        return 0;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Expr expr) return false;

        return CompareTo(expr) == 0;
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
    
    # endregion
    
    # region <-- Comparaison Spéciales -->

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
    
    # endregion
}