using System.Diagnostics.CodeAnalysis;

namespace ConsoleApp1.utils;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class Latex
{
    // greek letters
    public const string alpha = "alpha";
    public const string beta = "beta";
    public const string gamma = "gamma";
    public const string Gamma = "Gamma";
    public const string delta = "delta";
    public const string Delta = "Delta";
    public const string epsilon = "epsilon";
    public const string varepsilon = "varepsilon";
    public const string zeta = "zeta";
    public const string eta = "eta";
    public const string theta = "theta";
    public const string vartheta = "vartheta";
    public const string Theta = "Theta";
    public const string iota = "iota";
    public const string kappa = "kappa";
    public const string lambda = "lambda";
    public const string Lambda = "Lambda";
    public const string mu = "mu";
    public const string nu = "nu";
    public const string xi = "xi";
    public const string Xi = "Xi";
    public const string pi = "pi";
    public const string Pi = "Pi";
    public const string rho = "rho";
    public const string varrho = "varrho";
    public const string sigma = "sigma";
    public const string Sigma = "Sigma";
    public const string tau = "tau";
    public const string upsilon = "upsilon";
    public const string Upsilon = "Upsilon";
    public const string phi = "phi";
    public const string varphi = "varphi";
    public const string Phi = "Phi";
    public const string chi = "chi";
    public const string psi = "psi";
    public const string Psi = "Psi";
    public const string omega = "omega";
    public const string Omega = "Omega";

    public static readonly string[] GreekLetters =
    {
        alpha, beta, gamma, Gamma, delta, Delta, epsilon, varepsilon, zeta, eta, theta, vartheta,
        Theta, iota, kappa, lambda, Lambda, mu, nu, xi, Xi, pi, Pi, rho, varrho, sigma, Sigma,
        tau, upsilon, Upsilon, phi, varphi, Phi, chi, psi, Psi, omega, Omega
    };

    //
    public static string Parenthesis(string expr)
    {
        return $@"\left({expr}\right)";
    }

    public static string CurlyBrackets(string expr)
    {
        return "{" + expr + "}";
    }

    // Primary Operations
    public static string Add(string x, string y)
    {
        return $"{x}+{y}";
    }

    public static string Sub(string x, string y)
    {
        return $"{x}-{y}";
    }

    public static string Mul(string x, string y)
    {
        return $"{x}*{y}";
    }

    public static string MulDot(string x, string y)
    {
        return $@"{x}\cdot{y}";
    }

    public static string MulCross(string x, string y)
    {
        return $@"{x}\times{y}";
    }

    public static string Frac(string num, string den)
    {
        return @"\frac{" + num + "}{" + den + "}";
    }

    public static string FracInline(string num, string den)
    {
        return $"{num}/{den}";
    }

    public static string Pow(string value, string exp)
    {
        return "{" + value + "}^{" + exp + "}";
    }

    public static string Sqrt(string value, string n)
    {
        return $@"\sqrt[{n}]" + "{" + value + "}";
    }

    // Functions
    // Trig
    public static string Sin(string x)
    {
        return @"\sin" + Parenthesis(x);
    }

    public static string Cos(string x)
    {
        return @"\cos" + Parenthesis(x);
    }

    public static string Tan(string x)
    {
        return @"\tan" + Parenthesis(x);
    }

    public static string Sec(string x)
    {
        return @"\sec" + Parenthesis(x);
    }

    public static string Csc(string x)
    {
        return @"\csc" + Parenthesis(x);
    }

    public static string Cot(string x)
    {
        return @"\cot" + Parenthesis(x);
    }

    public static string Asin(string x)
    {
        return @"\sin^{-1}" + Parenthesis(x);
    }

    public static string Acos(string x)
    {
        return @"\cos^{-1}" + Parenthesis(x);
    }

    public static string Atan(string x)
    {
        return @"\tan^{-1}" + Parenthesis(x);
    }

    // Hyper Trig
    public static string Sinh(string x)
    {
        return @"\sinh" + Parenthesis(x);
    }

    public static string Cosh(string x)
    {
        return @"\cosh" + Parenthesis(x);
    }

    public static string Tanh(string x)
    {
        return @"\tanh" + Parenthesis(x);
    }

    // Exponential
    public static string Exp(string x)
    {
        return @"\exp" + Parenthesis(x);
    }

    public static string Ln(string x)
    {
        return @"\ln" + Parenthesis(x);
    }

    public static string Log(string x)
    {
        return @"\log" + Parenthesis(x);
    }

    public static string Log(string x, string n)
    {
        return @"\log_{" + n + "}" + Parenthesis(x);
    }

    // Misc
    public static string Arg(string x)
    {
        return @"\arg" + Parenthesis(x);
    }

    public static string Min(string x)
    {
        return @"\min" + Parenthesis(x);
    }

    public static string Max(string x)
    {
        return @"\max" + Parenthesis(x);
    }
}