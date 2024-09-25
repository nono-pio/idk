using System.Diagnostics.CodeAnalysis;
using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Alphabet
{
    public static Variable a = Var("a");
    public static Variable b = Var("b");
    public static Variable c = Var("c");
    public static Variable e = Var("e", value: Constant.E);
    public static Variable f = Var("f"); // TODO: default is func
    public static Variable g = Var("g"); // idem
    public static Variable h = Var("h"); // idem
    public static Variable i = Var("i", natural:true);
    public static Variable j = Var("j", natural:true);
    public static Variable k = Var("k");
    public static Variable l = Var("l");
    public static Variable m = Var("m");
    public static Variable n = Var("n", integer:true);
    public static Variable o = Var("o");
    public static Variable p = Var("p");
    public static Variable q = Var("q");
    public static Variable r = Var("r");
    public static Variable s = Var("s");
    public static Variable t = Var("t");
    public static Variable u = Var("u");
    public static Variable v = Var("v");
    public static Variable w = Var("w");
    public static Variable x = Var("x");
    public static Variable y = Var("y");
    public static Variable z = Var("z");
    
    
}