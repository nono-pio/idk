using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Limits;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Series;
using ConsoleApp1.Core.Solvers;
using ConsoleApp1.Parser;
using ConsoleApp1.Utils;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.apis;

public class Api
{
    
    private static T TryCatch<T>(Func<T> func, T defaultValue = default(T))
    {
        try
        {
            return func();
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }
    
    public static void APIRoutes(RouteGroupBuilder routes)
    {
        routes.MapPost("/eval", ([FromBody] EvalRequest request) =>
        {
            var input = Parser.Parse(request.Expr);
            if (input is null)
                return Results.BadRequest();

            return Results.Ok(new EvalResponse(input.ToLatex()));
        });

        routes.MapPost("/derivative", ([FromBody] DerivativeRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            var x = new Variable(request.Var);

            if (func is null)
                return Results.BadRequest();

            var derivative = func.Derivee(x);
            return Results.Ok(new DerivativeResponse(derivative.ToLatex()));
        });

        routes.MapPost("/integral", ([FromBody] IntegralRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            var x = new Variable(request.Var);

            if (func is null)
                return Results.BadRequest();

            var integral = Integral.Integrate(func, x); //func.Integrate(request.Var);
            return Results.Ok(
                new IntegralResponse(integral?.ToLatex() ?? "\\text{Cannot do integral}"));
        });

        routes.MapPost("/limit", ([FromBody] LimitRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            var x = new Variable(request.Var);
            var x0 = Parser.Parse(request.To);

            if (func is null || x0 is null)
                return Results.BadRequest();

            var limit = Limit.LimitOf(func, x, x0);
            return Results.Ok(new LimitResponse(limit?.ToLatex() ?? "Cannot do limit"));
        });

        routes.MapPost("/simplify", ([FromBody] SimplifyRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            if (func is null)
                return Results.BadRequest();

            Expr simplified = double.NaN; //func.Simplify();
            return Results.Ok(new SimplifyResponse(simplified.ToLatex()));
        });

        routes.MapPost("/analyse", ([FromBody] AnalyzeFunctionRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            if (func is null)
                return Results.BadRequest();
            
            var variable = new Variable(request.Var);

            var domain = TryCatch(() => Inequalities.FindDomain(func, variable));
            var range = TryCatch(() => Inequalities.FindRange(func, variable));
            var derivative = func.Derivee(variable);
            var integral = Integral.Integrate(func, variable) ?? null;
            var reciprocal = TryCatch(() => Reciprocal.GetReciprocal(func, variable));
            var seriesExpansion = TryCatch(() => TaylorSeries.TaylorSeriesOf(func, variable, 6).Eval(variable));
            Expr? factorization = null; //func.Factorization();

            return Results.Ok(
                new AnalyzeFunctionResponse(
                    func.ToLatex(), 
                    domain?.ToLatex(), 
                    range?.ToLatex(),
                    derivative.ToLatex(), 
                    integral?.ToLatex(), 
                    reciprocal?.ToLatex(), 
                    seriesExpansion?.ToLatex(),
                    factorization?.ToLatex()
                    )
                );
        });

        routes.MapPost("/equation", ([FromBody] EquationRequest request) =>
        {
            var func1 = Parser.Parse(request.LHS);
            var func2 = Parser.Parse(request.RHS);
            var x = new Variable(request.Var);

            if (func1 is null || func2 is null || !Parser.IsLetter(request.Var))
                return Results.BadRequest();

            var sol = Solve.SolveFor(func1, func2, x);

            return  Results.Ok(new EquationResponse(sol is null ? double.NaN.Expr().ToLatex() : sol.ToLatex()));
        });

        routes.MapPost("/inequality", ([FromBody] InequalityRequest request) =>
        {
            var lhs = Parser.Parse(request.LHS);
            var rhs = Parser.Parse(request.RHS);
            var x = new Variable(request.Var);
            if (lhs is null || rhs is null)
                return Results.BadRequest();

            InequationType? sign = request.Sign switch
            {
                "<" => InequationType.LessThan,
                "<=" => InequationType.LessThanOrEqual,
                ">" => InequationType.GreaterThan,
                ">=" => InequationType.GreaterThanOrEqual,
                _ => null
            };

            if (sign is null)
                return Results.BadRequest();

            var sol = Inequalities.SolveFor(lhs, rhs, sign.Value, x);

            return Results.Ok(new InequalityResponse(sol.ToLatex()));
        });

        routes.MapPost("/graph", ([FromBody] GraphRequest request) =>
        {
            var func = Parser.Parse(request.Expr);
            if (func is null)
                return Results.BadRequest();

            var f = new Fonction(func, (Variable)request.Var);

            var n = 1_000;
            if (request.NumPoints is not null && request.NumPoints > 0)
                n = 1_000;

            // TODO: Improve
            if (request.XStart is null || request.XEnd is null || request.YStart is null || request.YEnd is null)
                return Results.BadRequest();

            var dx = (request.XEnd.Value - request.XStart.Value) / n;
            var points = new double?[n];
            for (int i = 0; i < n; i++)
            {
                var x = request.XStart.Value + i * dx;
                var y = f.N(x);
                points[i] = double.IsNaN(y) ? null : y;
            }

            return Results.Ok(new GraphResponse(points));
        });
        
        routes.MapFallback(() => "No API endpoint found");
    }
}

/*
All things that you can do with the API are:
- Eval (Expr) -> Expr, Numerical Value
- Derivative (Expr) -> Expr
- Integral (Expr) -> Expr, Numerical Value
- Limit (Expr) -> Expr
- Simplify (Expr) -> Eval, Simplify : Expr
- Analyze Function (Expr, Var = x) -> Func Eval, Find Domain, Range, Derivative, Integral, Reciprocal, Series Expansion, ..., Factorization (Poly)
- Equation (Expr, Expr, Var = x) : Set of solutions
- Inequality (Expr, Expr, sign, Var = x) : Set of solutions
- Graph (Expr, Var = x, x_start, x_end, y_start, y_end) : Graph of the function
*/

// Eval
record EvalRequest(string Expr);
record EvalResponse(string Expr);

// Derivative
record DerivativeRequest(string Expr, string Var);
record DerivativeResponse(string Expr);

// Integral
record IntegralRequest(string Expr, string Var);
record IntegralResponse(string Expr);

// Limit
record LimitRequest(string Expr, string Var, string To);
record LimitResponse(string Expr);

// Simplify
record SimplifyRequest(string Expr);
record SimplifyResponse(string Expr);

// Analyze Function
record AnalyzeFunctionRequest(string Expr, string Var);
record AnalyzeFunctionResponse(string? EvalFunc, string? Domain, string? Range, string? Derivative, string? Integral, string? Reciprocal, string? SeriesExpansion, string? Factorization);

// Equation
record EquationRequest(string LHS, string RHS, string Var);
record EquationResponse(string Solutions);

// Inequality
record InequalityRequest(string LHS, string RHS, string Sign, string Var);
record InequalityResponse(string Solutions);

// Graph
record GraphRequest(string Expr, string Var, double? XStart, double? XEnd, double? YStart, double? YEnd, int? NumPoints);
record GraphResponse(double?[] Points);