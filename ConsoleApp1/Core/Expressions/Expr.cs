﻿using System.Diagnostics;
using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;
using Sdcb.Arithmetic.Mpfr;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions;

public abstract class Expr
{
    /// Args = arguments/parametres de l'expression (thermes d'une addition, facteur d'un produit)
    public readonly Expr[] Args;
    public virtual object[] GetArgs() => null;

    # region Constructors
    
    protected Expr(params Expr[] args)
    {
        Args = args;
    }

    public static implicit operator Expr(double value) => new Number(value);
    public static implicit operator Expr(int value) => new Number(value);
    public static implicit operator Expr(long value) => new Number(value);
    public static implicit operator Expr(string variable) => new Variable(variable);

    # endregion
    
    public static Number Inf => new(double.PositiveInfinity);
    public static Number NegInf => new(double.NegativeInfinity);
    
    # region Condition

    public virtual bool IsNatural => false;
    public virtual bool IsInteger => IsNatural;
    public virtual bool IsRational => IsInteger;
    public virtual bool IsReal => IsRational;
    public virtual bool IsComplex => IsReal;
    public bool IsExtendedReal => IsReal || IsInfinity || IsNegativeInfinity;

    public bool IsInfinite => IsInfinity || IsNegativeInfinity;
    public virtual bool IsInfinity => false;
    public virtual bool IsNegativeInfinity => false;

    public bool? Positivity => IsPositive ? true : IsNegative ? false : null;
    
    public virtual bool IsPositive
    {
        get
        {
            if (Constant())
                return N() > 0;
            
            return false;
        }
    }

    public virtual bool IsNegative
    {
        get
        {
            if (Constant())
                return N() < 0;
            
            return false;
        }
    }

    public virtual Boolean IsContinue(Variable variable, Set set)
    {
        return this switch
        {
            Number => true,
            Variable var => var != variable || var.Domain is null ? true : set.IsSubset(var.Domain),
            Addition add => And.Eval(add.Therms.Select(therm => therm.IsContinue(variable, set))),
            Multiplication mul => And.Eval(mul.Factors.Select(factor => factor.IsContinue(variable, set))),
            _ => throw new NotImplementedException()
        };
    }
    
    public bool IsNumZero => this is Number num && num.Num.IsZero;

    public virtual bool IsZero
    {
        get
        {
            Debug.WriteLine($"The {GetType()} hasn't define the fonction IsZero");
            return false;
        }
    }
    
    public bool IsNumOne => this is Number num && num.Num.IsOne;

    public virtual bool IsOne
    {
        get
        {
            Debug.WriteLine($"The {GetType()} hasn't define the fonction IsOne");
            return false;
        }
    }

    public bool Is(double n) => this is Number num && num.Num == n;
    
    public virtual bool IsNaN => this is Number { Num.IsNan: true };
    
    public bool IsVar(Variable variable) => this is Variable var && var.Equal(variable);

    public bool IsNumberInt() => this is Number num && num.IsInteger;
    public bool IsNumberIntPositif() => this is Number num && num.IsNatural;
    
    public int ToInt()
    {
        if (this is Number num && num.IsInteger)
            return num.Num.ToInt();
        
        throw new Exception("Cannot convert to int");
    }
    
    # endregion
    
    # region Evaluate

    public abstract Expr Eval(Expr[] exprs, object[]? objects = null);
    public abstract Expr NotEval(Expr[] exprs, object[]? objects = null);
    
    # endregion
    
    public virtual Expr Develop()
    {
        return this;
    }

    public bool IsEqualStrong(Expr expr)
    {
        return Develop() == expr.Develop();
    }
    
    # region Conversion

    public virtual Set AsSet() => ArraySet(this);

    public virtual (Expr Num, Expr Den) AsFraction(bool expNumber = true) => (this, 1);

    // af(x) -> a, f(x)
    public virtual (Expr Constant, Expr Variate) SeparateConstant(Variable var) => Constant(var) ? (this, 1) : (1, this);
    public virtual (Expr Constant, Expr Variate) SeparateConstant() => Constant() ? (this, 1) : (1, this);
    
    public Fonction AsFonction(Variable variable)
    {
        return new Fonction(this, variable);
    }
    
    /// Retourne l'expression sous la forme a * x où a est un nombre et x une expression
    public virtual (NumberStruct, Expr?) AsMulCoef()
    {
        return (1, this);
    }
    
    // 2x -> 2, x / 2 -> 2, 1
    public (int, Expr) AsMulInt()
    {
        if (this is Number n)
        {
            if (n.IsInteger)
                return (n.ToInt(), 1);

            return (1, this);
        }
        
        if (this is not Multiplication mul)
            return (1, this);
        
        
        var coef = 1;
        List<Expr> rest = [];
        var isChange = false;
        foreach (var factor in mul.Factors)
        {
            if (factor is Number num && num.IsInteger)
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
    public (Expr, Expr) AsMulCsteNCste(Variable variable)
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


    public virtual Complex AsComplex()
    {
        throw new NotImplementedException($"Complex not implemented for {GetType()}");
    }

    # endregion

    # region String Méthodes

    /// Génère un string représentant l'expression en utilisant le Latex
    public abstract string ToLatex();

    public string? ParenthesisIfNeeded(Expr expr)
    {
        return expr.GetOrderOfOperation() < GetOrderOfOperation() ? '(' + expr.ToString() + ')' : expr.ToString();
    }
    
    public string ParenthesisLatexIfNeeded(Expr expr)
    {
        return expr.GetOrderOfOperation() < GetOrderOfOperation() ? LatexUtils.Parenthesis(expr.ToLatex()) : expr.ToLatex();
    }
    
    # endregion

    # region Outils Mathématiques

    /// Represente the domain as a condition (ex: ln(x) -> x > 0)
    public virtual Boolean DomainCondition => true;

    /// If the Function has a period over Args[0] / null = no period else period
    public virtual Expr? BasePeriod => null;

    public bool IsNumInf => this is Number num && (num.Num.IsInfinity || num.Num.IsNegativeInfinity);

    public enum OrderOfOperation
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
    
    public double? SafeN()
    {
        try
        {
            var n = N();
            if (double.IsNaN(n) || double.IsInfinity(n))
                return null;
            return N();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
    public abstract double N();

    public virtual MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven) => MpfrFloat.From(double.NaN);

    public static Expr Gcd(IEnumerable<Expr> exprs)
    {
        return exprs.Aggregate(Gcd);
    }

    
    public static Expr Gcd(Expr a, Expr b)
    {
        if (a is Number a_num && a_num.Num.IsFraction && b is Number b_num && b_num.Num.IsFraction)
        {
            var gcd = NumberUtils.Gcd(
                b_num.Num.Numerator * a_num.Num.Denominator,
                a_num.Num.Numerator * b_num.Num.Denominator);
            
            return Num(gcd, a_num.Num.Denominator * b_num.Num.Denominator);
        }

        throw new NotImplementedException();
    }
    
    public static Expr Lcm(IEnumerable<Expr> exprs)
    {
        return exprs.Aggregate(Lcm);
    }


    public static Expr Lcm(Expr a, Expr b)
    {
        return a * b / Gcd(a, b);
    }
    
    public abstract Expr Reciprocal(Expr y, int argIndex);
    public virtual Expr[] AllReciprocal(Expr y, int argIndex) => [Reciprocal(y, argIndex)];

    # endregion
    
    # region Derivee
    
    public virtual Expr fDerivee(int argIndex) => throw new NotImplementedException("fDerivee not implemented for " + GetType());

    public virtual Expr Derivee(Variable variable)
    {
        Expr result = 0;
        for (int i = 0; i < Args.Length; i++)
        {
            if (Args[i].Constant(variable))
                continue;
            
            result += fDerivee(i) * Args[i].Derivee(variable);
        }

        return result;
    }
    
    public virtual Expr Derivee(Variable variable, int n)
    {
        var result = this;
        for (var i = 0; i < n; i++) 
            result = result.Derivee(variable);

        return result;
    }

    public Expr Derivee(Variable variable, Expr n)
    {
        return new Derivative(this, variable, n);
    }

    
    # endregion

    # region Outils
    
    public Expr Substitue(Variable variable, Expr value)
    {
        return MapAtoms<Variable>(var => var == variable ? value : var);
    }

    public Expr Substitue(Variable[] variable, Expr[] value)
    {
        var result = this;
        for (int i = 0; i < variable.Length; i++)
        {
            result = result.Substitue(variable[i], value[i]);
        }

        return result;
    }
    
    public virtual Expr Substitue(Expr expr, Expr value)
    {
        
        var objects = GetArgs();
        var newArgs = new Expr[Args.Length];
        
        for (var i = 0; i < Args.Length; i++) 
            newArgs[i] = Args[i].Substitue(expr, value);

        var result = Eval(newArgs, objects);
        
        if (result == expr)
            return value;
        
        return result;
    }

    public Expr Substitue(Dictionary<Variable,Expr> maps)
    {
        Expr expr = this;
        foreach (var (key, value) in maps)
        {
            expr = expr.Substitue(key, value);
        }

        return expr;
    }
    
    // return a where a*x=this
    public Expr? AsMulIndependent(Variable x)
    {
        if (this == x)
            return 1;
        if (this.IsNumZero)
            return 0;
        if (this is Number)
            return null;

        if (this is Multiplication mul)
            return mul.AsIndependent(x, exact: true).Constant;

        return null;
    }
    
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
    
    public bool Constant(Variable variable)
    {
        if (IsVar(variable)) 
            return false;

        foreach (var arg in Args)
            if (!arg.Constant(variable))
                return false;

        return true;
    }
    
    public bool Constant()
    {
        if (this is Variable && this is not Atoms.Constant) 
            return false;

        foreach (var arg in Args)
            if (!arg.Constant())
                return false;

        return true;
    }
    
    public Variable[] GetVariables()
    {
        var variables = new HashSet<Variable>(new ExprQuickComparer());
        ForEach<Variable>(var =>
        {
            if (var is not Atoms.Constant)
                variables.Add(var);
        });
        
        return variables.ToArray();
    }
    
    public bool Has<T>() where T : Expr
    {
        if (this is T)
            return true;

        foreach (var arg in Args)
            if (arg.Has<T>())
                return true;

        return false;
    }
    
    public int Count<T>() where T : Expr
    {
        var count = 0;

        foreach (var arg in Args)
            count += arg.Count<T>();

        if (this is T) 
            count++;

        return count;
    }
    
    public void ForEach<T>(Action<T> action) where T : Expr
    {
        if (this is T t)
            action(t);

        foreach (var arg in Args)
            arg.ForEach(action);
    }
    
    public Expr Map<T>(Func<T, Expr> func) where T : Expr
    {
        return MapBottomUp(expr => expr is T t ? func(t) : expr);
    }

    public Expr MapAtoms<T>(Func<T, Expr> func) where T : Atom
    {
        var objects = GetArgs();
        var newArgs = new Expr[Args.Length];

        if (this is T t)
            return func(t);
        
        for (var i = 0; i < Args.Length; i++) 
            newArgs[i] = Args[i].MapAtoms(func);

        return Eval(newArgs, objects);
    }
    
    public Expr MapArgs(Func<Expr, Expr> func)
    {
        var objects = GetArgs();
        var newArgs = new Expr[Args.Length];

        for (var i = 0; i < Args.Length; i++)
            newArgs[i] = func(Args[i]);

        return Eval(newArgs, objects);
    }

    public Expr MapBottomUp(Func<Expr, Expr> func)
    {

        var objects = GetArgs();
        var newArgs = new Expr[Args.Length];
        
        for (var i = 0; i < Args.Length; i++) 
            newArgs[i] = Args[i].MapBottomUp(func);

        return func(Eval(newArgs, objects));
    }
    
    # endregion
    
    # region Math Operator
    
    public static Expr operator +(Expr expr1, Expr expr2)
    {
        return Addition.AddOpti(expr1, expr2);
    }

    public static Expr operator -(Expr expr1, Expr expr2)
    {
        return expr1 + -expr2;
    }

    public static Expr operator -(Expr expr)
    {
        return -1 * expr;
    }

    public static Expr operator *(Expr expr1, Expr expr2)
    {
        return Multiplication.MulOpti(expr1, expr2);
    }


    public static Expr operator /(Expr expr1, Expr expr2)
    {
        return expr1 * Pow(expr2, -1);
    }
    
    # endregion
    
    # region Comparaison
    
    protected long TypeId()
    {
        return GetType().TypeHandle.Value.ToInt64();
    }
    
    public override int GetHashCode()
    {
        return Args.GetHashCode();
    }

    public int CompareType(Expr expr)
    {
        var compare = TypeId().CompareTo(expr.TypeId());
        if (compare == 0)
            return compare;

        if (this is Number)
            return -1;
        if (expr is Number)
            return 1;

        return compare;
    }
    
    public int CompareTo(Expr? expr)
    {
        if (expr is null) 
            return -1;

        // cmp type
        var cmpType = CompareType(expr);
        if (cmpType != 0) 
            return cmpType;

        if (this is Atom atom) 
            return atom.CompareAtom((Atom)expr);

        // cmp args length
        var cmpLength = Args.Length.CompareTo(expr.Args.Length);
        if (cmpLength != 0) 
            return cmpLength;

        // cmp args
        for (var i = 0; i < Args.Length; i++)
        {
            var cmpArg = Args[i].CompareTo(expr.Args[i]);
            if (cmpArg != 0) 
                return cmpArg;
        }
        
        return 0;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Expr expr) 
            return false;

        return CompareTo(expr) == 0;
    }

    public static bool operator ==(Expr lhs, Expr rhs)
    {
        return Equals(lhs, rhs);
    }

    public static bool operator !=(Expr lhs, Expr rhs)
    {
        return !Equals(lhs, rhs);
    }

    public static Boolean operator >(Expr lhs, Expr rhs)
    {
        return Boolean.GreaterThan(lhs, rhs);
    }
    
    public static Boolean operator >=(Expr lhs, Expr rhs)
    {
        return Boolean.GreaterThanOrEqual(lhs, rhs);
    }
    
    public static Boolean operator <(Expr lhs, Expr rhs)
    {
        return Boolean.LessThan(lhs, rhs);
    }
    
    public static Boolean operator <=(Expr lhs, Expr rhs)
    {
        return Boolean.LessThanOrEqual(lhs, rhs);
    }
    
    # endregion

    public class ExprComparer : IComparer<Expr>
    {
        public int Compare(Expr? x, Expr? y)
        {
            switch (x, y)
            {
                case (null, null):
                    return 0;
                case (null, _):
                    return -1;
                case (_, null):
                    return 1;
                case (_, _):
                    if (x > y)
                        return 1;
                    if (x < y)
                        return -1;
                    return 0;
            }
        }
    }
    
    public class ExprQuickComparer : IComparer<Expr>, IEqualityComparer<Expr>
    {
        public int Compare(Expr? x, Expr? y)
        {
            return (x, y) switch
            {
                (null, null) => 0,
                (null, _) => -1,
                (_, null) => 1,
                (_, _) => x.CompareTo(y)
            };
        }

        public bool Equals(Expr x, Expr y)
        {
            return (x, y) switch
            {
                (null, null) => true,
                (null, _) => false,
                (_, null) => false,
                (_, _) => x.CompareTo(y) == 0
            };
        }

        public int GetHashCode(Expr obj)
        {
            return obj.Args.GetHashCode();
        }
    }

    public bool HasNan()
    {
        var has = false;
        ForEach<Number>(num =>
        {
            if (num.IsNaN)
                has = true;
        });

        return has;
    }

    public virtual bool CanRemoveNegativeSign()
    {
        if (this is not Multiplication mul)
            return false;

        foreach (var fac in mul.Args)
        {
            if (fac.Is(-1))
                return true;
        }

        return false;
    }

    public (Expr AddCoef, Expr MulCoef)? AsLinear(Variable variable)
    {
        if (Constant(variable))
            return (this, 0);
        if (IsVar(variable))
            return (0, 1);
        
        Expr addCoef = 0;
        Expr mulCoef = 1;

        var e = this;

        if (e is Addition add)
        {
            (addCoef, e) = add.AsIndependent(variable);
        }

        if (e is Multiplication mul)
        {
            (mulCoef, e) = mul.AsIndependent(variable);
        }

        if (e.IsVar(variable))
            return (addCoef, mulCoef);

        return null;
    }
}