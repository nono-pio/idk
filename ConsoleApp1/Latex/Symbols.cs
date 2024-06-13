using System.Diagnostics.CodeAnalysis;

namespace ConsoleApp1.Latex;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Symbols
{
    // Greek letters
    public static string Alpha = @"\Alpha";
    public static string alpha = @"\alpha";
    public static string Beta = @"\Beta";
    public static string beta = @"\beta";
    public static string Gamma = @"\Gamma";
    public static string gamma = @"\gamma";
    public static string Delta = @"\Delta";
    public static string delta = @"\delta";
    public static string Epsilon = @"\Epsilon";
    public static string epsilon = @"\epsilon";
    public static string varepsilon = @"\varepsilon";
    public static string Zeta = @"\Zeta";
    public static string zeta = @"\zeta";
    public static string Eta = @"\Eta";
    public static string eta = @"\eta";
    public static string Theta = @"\Theta";
    public static string theta = @"\theta";
    public static string vartheta = @"\vartheta";
    public static string Iota = @"\Iota";
    public static string iota = @"\iota";
    public static string Kappa = @"\Kappa";
    public static string kappa = @"\kappa";
    public static string varkappa = @"\varkappa";
    public static string Lambda = @"\Lambda";
    public static string lambda = @"\lambda";
    public static string Mu = @"\Mu";
    public static string mu = @"\mu";
    public static string Nu = @"\Nu";
    public static string nu = @"\nu";
    public static string Xi = @"\Xi";
    public static string xi = @"\xi";
    public static string Omicron = @"\Omicron";
    public static string omicron = @"\omicron";
    public static string Pi = @"\Pi";
    public static string pi = @"\pi";
    public static string varpi = @"\varpi";
    public static string Rho = @"\Rho";
    public static string rho = @"\rho";
    public static string varrho = @"\varrho";
    public static string Sigma = @"\Sigma";
    public static string sigma = @"\sigma";
    public static string varsigma = @"\varsigma";
    public static string Tau = @"\Tau";
    public static string tau = @"\tau";
    public static string Upsilon = @"\Upsilon";
    public static string upsilon = @"\upsilon";
    public static string Phi = @"\Phi";
    public static string phi = @"\phi";
    public static string varphi = @"\varphi";
    public static string Chi = @"\Chi";
    public static string chi = @"\chi";
    public static string Psi = @"\Psi";
    public static string psi = @"\psi";
    public static string Omega = @"\Omega";
    public static string omega = @"\omega";
    
    // Value
    public static string True = @"\top";
    public static string False = @"\bot";
    
    // Unary operators
    public static string Not = @"\neg";
    public static string Factorial = "!";
    
    // Binary operators
    public static string And = @"\land";
    public static string Or = @"\lor";
    
    // Equalities
    public static string Equal = "=";
    public static string NotEqual = @"\neq";
    public static string Equivalent = @"\equiv";
    public static string ApproximatelyEqual = @"\approx";
    
    // Inequalities
    public static string LessThan = "<";
    public static string LessThanOrEqual = @"\leq";
    public static string GreaterThan = ">";
    public static string GreaterThanOrEqual = @"\geq";
    
    // Set membership
    public static string ElementOf = @"\in";
    public static string NotElementOf = @"\notin";
    
    // Set relations
    public static string Subset = @"\subset";
    public static string SubsetEqual = @"\subseteq";
    public static string Superset = @"\supset";
    public static string SupersetEqual = @"\supseteq";
    public static string Union = @"\cup";
    public static string Intersection = @"\cap";
    public static string SetMinus = @"\setminus";
    
    // Set
    public static string EmptySet = @"\emptyset";
    public static string UniversalSet = @"\mathbb{U}";
    public static string NaturalNumbers = @"\N";
    public static string Integers = @"\Z";
    public static string RationalNumbers = @"\Q";
    public static string IrrationalNumbers = @"\I";
    public static string RealNumbers = @"\R";
    public static string ComplexNumbers = @"\C";
    
    // Delimiters
    public static string OpenParenthesis = @"\left(";
    public static string CloseParenthesis = @"\right)";
    public static string OpenSquareBracket = @"\left[";
    public static string CloseSquareBracket = @"\right]";
    public static string OpenCurlyBracket = @"\left{";
    public static string CloseCurlyBracket = @"\right}";
    public static string OpenFloor = @"\lfloor";
    public static string CloseFloor = @"\rfloor";
    public static string OpenCeil = @"\lceil";
    public static string CloseCeil = @"\rceil";
    
    // Complex
    public static string RealPart = @"\Re";
    public static string ImaginaryPart = @"\Im";
    public static string ComplexUnit = @"\i";
    
    // Calculus
    public static string PartialDerivative = @"\partial";
    public static string Gradient = @"\nabla";
    
    // Others
    public static string Infinity = @"\infty";
    
    // Physics
    public static string HBar = @"\hbar";
    
}