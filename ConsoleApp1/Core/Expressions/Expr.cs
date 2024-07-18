using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions;


public abstract class Expr
{
    /// Args = arguments/parametres de l'expression (thermes d'une addition, facteur d'un produit)
    public Expr[] Args { get; protected set; }

    protected Expr(params Expr[] args)
    {
        Args = args;
    }

    public static implicit operator Expr(double value) => new Number(value);
    public static implicit operator Expr(int value) => new Number(value);
    public static implicit operator Expr(string variable) => new Variable(variable);

    public static Number Inf => new(double.PositiveInfinity);
    public static Number NegInf => new(double.NegativeInfinity);
    
    public bool IsExtendedReal => IsReal || IsInfinity || IsNegativeInfinity;
    public bool IsNatural => this is Number; //TODO
    public bool IsInteger => this is Number; //TODO
    public bool IsRational => this is Number; //TODO
    public bool IsReal => this is Number; //TODO

    public bool IsInfinity => this is Number num && num.IsInfinity; //TODO
    public bool IsNegativeInfinity => this is Number num && num.IsNegativeInfinity; //TODO
    public bool IsPositive => this is Number num && num.IsPositive; //TODO
    public bool IsNegative => this is Number num && num.IsNegative; //TODO
    
    public virtual Boolean IsContinue(string variable, Set set)
    {
        return this switch
        {
            Number => true,
            Variable var => var.Name != variable || var.Data.Domain is null ? true : set.IsSubset(var.Data.Domain),
            Addition add => And.Eval(add.Therms.Select(therm => therm.IsContinue(variable, set))),
            Multiplication mul => And.Eval(mul.Factors.Select(factor => factor.IsContinue(variable, set))),
            _ => throw new NotImplementedException()
        };
    }
    
    public virtual Expr Develop()
    {
        return this;
    }

    public bool IsEqualStrong(Expr expr)
    {
        return Develop() == expr.Develop();
    }
    
    # region <-- Conversion -->

    public Fonction AsFonction(string variable)
    {
        return new Fonction(this, variable);
    }
    
    // TODO
    /// Retourne l'expression sous la forme a * x où a est un nombre et x une expression
    public virtual (NumberStruct, Expr?) AsMulCoef()
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
                coef *= num.ToInt();
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
            return Number.Gcd(a_num.ToInt(), b_num.ToInt()).Expr();
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

    public Expr Derivee(string variable, Expr n)
    {
        return new Derivative(this, variable, n);
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
    public static Expr operator +(Expr expr1, Expr expr2)
    {
        if (expr1 is Number num1 && expr2 is Number num2)
            return (Number) (num1.Num + num2.Num);

        return Add(expr1, expr2);
    }

    // Subtraction
    public static Expr operator -(Expr expr1, Expr expr2)
    {
        if (expr1 is Number num1 && expr2 is Number num2)
            return (Number) (num1.Num - num2.Num);
        
        return Sub(expr1, expr2);
    }

    // Negate
    public static Expr operator -(Expr expr)
    {
        if (expr is Number num)
            return (Number)(-num.Num);
        
        return Neg(expr);
    }

    // Multiplication
    public static Expr operator *(Expr expr1, Expr expr2)
    {
        if (expr1 is Number num1 && expr2 is Number num2)
            return (Number) (num1.Num * num2.Num);

        return Mul(expr1, expr2);
    }


    // Division
    public static Expr operator /(Expr expr1, Expr expr2)
    {
        if (expr1 is Number num1 && expr2 is Number num2)
            return (Number) (num1.Num / num2.Num);

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

    public bool IsZero() => this is Number { IsZero: true };
    public bool IsNotZero() => !IsZero();
    public bool IsOne() => this is Number { IsOne: true };
    public bool IsNegOne() => this is Number num && num.Is(-1);
    public bool Is(double n) => this is Number num && num.Is(n);
    
    public bool IsVar(string variable) => this is Variable var && var.Name == variable;

    public bool IsNumberInt() => this is Number num && num.IsEntier();
    public bool IsNumberIntPositif() => this is Number num && num.IsEntier() && num.IsPositif();


    public int ToInt()
    {
        if (this is not Number num || !num.IsEntier())
            throw new Exception("Cannot convert to int");
        
        return num.ToInt();
    }

    # endregion
}