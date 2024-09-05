namespace ConsoleApp1.Core.Expressions.FunctionMethods;

// true/T -> has attribute
// false/null -> undefine 
public class FuncAttributes
{
    public bool Positive { get; set; } = false; // f(x)>0
    public bool Negative { get; set; } = false; // f(x)<0

    public bool YInteger { get; set; } = false; // f(x) in ZZ

    public bool YInterceptZero { get; set; } = false; // f(0)=0 
    
    public bool Increasing { get; set; } = false; // x>y => f(x)>f(y)
    public bool Decreasing { get; set; } = false; // x>y => f(x)<f(y)
    public bool Monotonic => Increasing || Decreasing; 
    
    public Expr? Periodic { get; set; } = null; // f(x+nT)=f(x) n in N
    
    public bool Odd { get; set; } = false; // f(-x)=-f(x)
    public bool Even { get; set; } = false; // f(-x)=f(x)
    
    public bool Bounded { get; set; } = false; // inf < f(x) < sup inf, sup in R 
    public bool Unbounded { get; set; } = false; // sup or inf = oo
    
    public bool Continuous { get; set; } = false; 
    public bool Differentiable { get; set; } = false;
    public bool Integrable { get; set; } = false;
    public bool Reciprocal { get; set; } = false;
}

