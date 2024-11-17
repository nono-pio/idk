namespace ConsoleApp1.Core.Evaluators;

public abstract class ExprEvaluator
{
    /// Create an evaluate exprs and objects and return the evaluation 
    public abstract Expr Eval(Expr[] exprs, object[]? objects = null);
    /// Return the expr without evaluation
    public abstract Expr NotEval(Expr[] exprs, object[]? objects = null);
}