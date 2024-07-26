using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CAS API", Description = "CAS du turflu", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CAS API V1");
    });
}

app.UseHttpsRedirection();



app.MapGet("/", () => "Hello World!");

app.MapPost("/eval", ([FromBody] EvalRequest request) =>
{
    var input = Parser.Parse(request.Expr);
    if (input is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    return Results.Ok(new EvalResponse(input.ToLatex(), input.SafeN()));
});

app.MapPost("/derivative", ([FromBody] DerivativeRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    var derivative = func.Derivee(request.Var);
    return Results.Ok(new DerivativeResponse(derivative.ToLatex(), derivative.SafeN()));
});

app.MapPost("/integral", ([FromBody] IntegralRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    Expr integral = double.NaN;//func.Integrate(request.Var);
    return Results.Ok(new IntegralResponse(integral.ToLatex(), integral.SafeN()));
});

app.MapPost("/limit", ([FromBody] LimitRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    Expr limit = double.NaN; //func.Limit(request.Var, Parser.Parse(request.To));
    return Results.Ok(new LimitResponse(limit.ToLatex(), limit.SafeN()));
});

app.MapPost("/simplify", ([FromBody] SimplifyRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    Expr simplified = double.NaN; //func.Simplify();
    return Results.Ok(new SimplifyResponse(simplified.ToLatex(), simplified.SafeN()));
});

app.MapPost("/analyse", ([FromBody] AnalyzeFunctionRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    Expr domain = double.NaN; //func.Domain();
    Expr range = double.NaN; //func.Range();
    Expr derivative = func.Derivee(request.Var); //func.Derivee(request.Var);
    Expr integral = double.NaN; //func.Integrate(request.Var);
    Expr reciprocal = double.NaN; //func.Reciprocal();
    Expr seriesExpansion = double.NaN; //func.SeriesExpansion();
    Expr factorization = double.NaN; //func.Factorization();

    return Results.Ok(new AnalyzeFunctionResponse(func.ToLatex(), domain.ToLatex(), range.ToLatex(), derivative.ToLatex(), integral.ToLatex(), reciprocal.ToLatex(), seriesExpansion.ToLatex(), factorization.ToLatex()));
});

app.MapPost("/equation", ([FromBody] EquationRequest request) =>
{
    var func1 = Parser.Parse(request.LHS);
    var func2 = Parser.Parse(request.RHS);
    if (func1 is null || func2 is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    Equation equation = new(func1, func2);
    var sol = equation.SolveFor(request.Var);
    var numValue = double.NaN;//equation.SolveNumericallyFor(request.Var);
    
    return Results.Ok(new EquationResponse(sol.ToLatex(), double.IsNaN(numValue) ? null : numValue));
});

app.MapPost("/inequality", ([FromBody] InequalityRequest request) =>
{
    Console.WriteLine("no problem here");
    var func1 = Parser.Parse(request.LHS);
    var func2 = Parser.Parse(request.RHS);
    if (func1 is null || func2 is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);
    
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

    Inequation inequality = new(func1, func2, sign.Value);
    var sol = inequality.SolveFor(request.Var);
    
    return Results.Ok(new InequalityResponse(sol.ToLatex()));
});

app.MapPost("/graph", ([FromBody] GraphRequest request) =>
{
    var func = Parser.Parse(request.Expr);
    if (func is null)
        return Results.BadRequest();
    
    SetVariables(request.Variables);

    var f = new Fonction(func, request.Var);

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

app.Run();

void SetVariables(VariableModel[]? variables)
{
    if (variables is null)
        return;
    
    for (int i = 0; i < variables.Length; i++)
    {
        var variable = variables[i];
        
        var name = variable.Name;
        var dependencies = variable.Dependencies ?? Array.Empty<string>();
        var value = variable.Value is null ? null : Parser.Parse(variable.Value);
        var domain = variable.Domain is null ? null : Parser.ParseSet(variable.Domain);

        var data = VariableData.FromValue(name, value);
        data.Dependencies = dependencies.ToList();
        data.Domain = domain;
        
        Variable.Variables[name] = data;
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
record EvalRequest(string Expr, VariableModel[]? Variables);
record EvalResponse(string Expr, double? NumValue);

// Derivative
record DerivativeRequest(string Expr, string Var, VariableModel[]? Variables);
record DerivativeResponse(string Expr, double? NumValue);

// Integral
record IntegralRequest(string Expr, string Var, VariableModel[]? Variables);
record IntegralResponse(string Expr, double? NumValue);

// Limit
record LimitRequest(string Expr, string Var, string To, VariableModel[]? Variables);
record LimitResponse(string Expr, double? NumValue);

// Simplify
record SimplifyRequest(string Expr, VariableModel[]? Variables);
record SimplifyResponse(string Expr, double? NumValue);

// Analyze Function
record AnalyzeFunctionRequest(string Expr, string Var, VariableModel[]? Variables);
record AnalyzeFunctionResponse(string EvalFunc, string? Domain, string? Range, string? Derivative, string? Integral, string? Reciprocal, string? SeriesExpansion, string? Factorization);

// Equation
record EquationRequest(string LHS, string RHS, string Var, VariableModel[]? Variables);
record EquationResponse(string Solutions, double? NumValue);

// Inequality
record InequalityRequest(string LHS, string RHS, string Sign, string Var, VariableModel[]? Variables);
record InequalityResponse(string Solutions);

// Graph
record GraphRequest(string Expr, string Var, double? XStart, double? XEnd, double? YStart, double? YEnd, int? NumPoints, VariableModel[]? Variables);
record GraphResponse(double?[] Points);

record VariableModel(string Name, string[]? Dependencies, string? Domain, string? Value);