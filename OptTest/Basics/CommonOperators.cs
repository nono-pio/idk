using System.Runtime.InteropServices.JavaScript;
using OptTest.Utils;

namespace OptTest.Basics;

using NonNegativeInteger = uint;
using Symbol = string;
using OutputForm = string;
using SingleInteger = int;

public sealed class BasicOperator
{
    public Symbol OpName;
    public SingleInteger NArg;
    public Dictionary<Symbol, object> Props;

    public BasicOperator(Symbol name, SingleInteger n, Dictionary<Symbol, object> props)
    {
        OpName = name;
        NArg = n;
        Props = props;
    }

    public Symbol Name() => OpName;
    public Dictionary<Symbol, object> Properties() => Props;

    public BasicOperator Copy() => Oper(OpName, NArg, Props.AsEnumerable().ToDictionary());
    public static BasicOperator Operator(Symbol name) => Oper(name, -1, []);
    public static BasicOperator Operator(Symbol name, NonNegativeInteger nArgs) => Oper(name, (SingleInteger)nArgs, []);

    public MayFail<NonNegativeInteger> Arity()
    {
        var n = NArg;
        if (n < 0)
            return new MayFail<NonNegativeInteger>();
        return new MayFail<NonNegativeInteger>((NonNegativeInteger)n);
    }

    public Boolean IsNullary() => NArg == 0;
    public Boolean Unary() => NArg == 1;
    public Boolean Nary() => NArg < 0;

    public NonNegativeInteger Weight()
    {
        var w = Property(WEIGHT);
        if (w.IsFailed)
            return 1;
        return (NonNegativeInteger)w.Value;
    }

    public BasicOperator Weight(NonNegativeInteger n) => SetProperty(WEIGHT, n);

    public BasicOperator Equality(Func<BasicOperator, BasicOperator, Boolean> func) =>
        SetProperty(EQUAL, func);

    public BasicOperator Comparison(Func<BasicOperator, BasicOperator, Boolean> func) =>
        SetProperty(LESS, func);

    public MayFail<Func<OutputForm[], OutputForm>> Display()
    {
        var u = Property(DISPLAY);
        if (u.IsFailed)
            return new MayFail<Func<OutputForm[], OutputForm>>();
        return new MayFail<Func<OutputForm[], OutputForm>>((Func<OutputForm[], OutputForm>)u.Value);
    }

    public BasicOperator Display(Func<OutputForm[], OutputForm> f) => SetProperty(DISPLAY, f);
    public BasicOperator Display(Func<OutputForm, OutputForm> f) => Display(l1 => f(l1[0]));
    public BasicOperator Input(Func<SEX[], SEX> f) => SetProperty(SEXPR, f);

    public MayFail<Func<SEX[], SEX>> Input()
    {
        var u = Property(SEXPR);
        if (u.IsFailed)
            return new MayFail<Func<SEX[], SEX>>();
        return new MayFail<Func<SEX[], SEX>>((Func<SEX[], SEX>)u.Value);
    }

    public Boolean Is(Symbol name) => Name() == name;
    public Boolean Has(Symbol name) => Props.ContainsKey(name);
    public BasicOperator Assert(Symbol s) => SetProperty(s, null);

    public BasicOperator DeleteProperty(Symbol name)
    {
        Properties().Remove(name);
        return this;
    }

    public MayFail<object> Property(Symbol name)
    {
        if (Props.TryGetValue(name, out var prop))
        {
            return new MayFail<object>(prop);
        }

        return new MayFail<object>();
    }

    public BasicOperator SetProperty(Symbol name, object value)
    {
        Props.Add(name, value);
        return this;
    }

    public BasicOperator SetProperties(Dictionary<Symbol, object> l)
    {
        Props = l;
        return this;
    }

    public static BasicOperator Oper(Symbol name, SingleInteger nArg, Dictionary<Symbol, object> props) =>
        new BasicOperator(name, nArg, props);

    public static bool operator ==(BasicOperator op1, BasicOperator op2)
    {
        if (ReferenceEquals(op1, op2)) return true;
        if (op1.OpName != op2.OpName) return false;
        if (op1.NArg != op2.NArg) return false;
        if (!op1.Props.Keys.SequenceEqual(op2.Props.Keys)) return false;
        var func = op1.Property(EQUAL);
        if (!func.IsFailed)
            return ((Func<BasicOperator, BasicOperator, Boolean>)func.Value)(op1, op2);
        return true;
    }

    public static bool operator !=(BasicOperator op1, BasicOperator op2) => !(op1 == op2);

    public static bool operator <(BasicOperator op1, BasicOperator op2)
    {

        var w1 = op1.Weight();
        var w2 = op2.Weight();
        if (w1 != w2) return w1 < w2;
        if (op1.NArg != op2.NArg) return op1.NArg < op2.NArg;
        if (op1.Name() != op2.Name()) return op1.Name().CompareTo(op2.Name()) < 0;

        var k1 = op1.Props.Keys;
        var k2 = op2.Props.Keys;
        var n1 = k1.Count;
        var n2 = k2.Count;
        if (n1 != n2) return n1 < n2;
        var d1 = k1.Except(k2).ToArray();
        if  ((n1 = d1.Length) != 0)
        {
            var d2 = k2.Except(d1).ToArray();
            if (n1 != (n2 = d2.Length)) return n1 < n2;
            return inspect(d1) < inspect(d2);
        }

        var func = op1.Property(LESS);
        if (!func.IsFailed) return ((Func<BasicOperator, BasicOperator, Boolean>)func.Value)(op1, op2);
        func = op1.Property(EQUAL);
        if (!func.IsFailed) return !((Func<BasicOperator, BasicOperator, Boolean>)func.Value)(op1, op2);
        return false;
    }

    public static bool operator >(BasicOperator op1, BasicOperator op2) => !(op1 < op2) && !(op1 == op2);

}
// CommonOperators() : Exports == Implementation where
//   Exports ==> with
//     operator : Symbol -> OP
//         ++ operator(s) returns an operator with name s, with the
//         ++ appropriate semantics if s is known. If s is not known,
//         ++ the result has no semantics.
//
//   Implementation ==> add
//     dpi        : List O -> O
//     dbeta      : List O -> O
//     dgamma     : List O -> O
//     dquote     : List O -> O
//     dexp       : O -> O
//     dfact      : O -> O
//     startUp    : Boolean -> Void
//     setDummyVar : (OP, NonNegativeInteger) -> OP
//
//     brandNew? : Boolean := true
//
//     opalg   := operator('rootOf, 2)$OP
//     oproot  := operator('nthRoot, 2)
//     oppi    := operator('pi, 0)
//     oplog   := operator('log, 1)
//     opexp   := operator('exp, 1)
//     opabs   := operator('abs, 1)
//     opsin   := operator('sin, 1)
//     opcos   := operator('cos, 1)
//     optan   := operator('tan, 1)
//     opcot   := operator('cot, 1)
//     opsec   := operator('sec, 1)
//     opcsc   := operator('csc, 1)
//     opasin  := operator('asin, 1)
//     opacos  := operator('acos, 1)
//     opatan  := operator('atan, 1)
//     opacot  := operator('acot, 1)
//     opasec  := operator('asec, 1)
//     opacsc  := operator('acsc, 1)
//     opsinh  := operator('sinh, 1)
//     opcosh  := operator('cosh, 1)
//     optanh  := operator('tanh, 1)
//     opcoth  := operator('coth, 1)
//     opsech  := operator('sech, 1)
//     opcsch  := operator('csch, 1)
//     opasinh := operator('asinh, 1)
//     opacosh := operator('acosh, 1)
//     opatanh := operator('atanh, 1)
//     opacoth := operator('acoth, 1)
//     opasech := operator('asech, 1)
//     opacsch := operator('acsch, 1)
//     opbox   := operator('%box, 1)$OP
//     oppren  := operator('%paren, 1)$OP
//     opquote := operator('%quote)$OP
//     opdiff  := operator('%diff, 3)
//     opsi    := operator('Si, 1)
//     opci    := operator('Ci, 1)
//     opshi   := operator('Shi, 1)
//     opchi   := operator('Chi, 1)
//     opei    := operator('Ei, 1)
//     opli    := operator('li, 1)
//     operf   := operator('erf, 1)
//     operfi  := operator('erfi, 1)
//     opli2   := operator('dilog, 1)
//     opfis   := operator('fresnelS, 1)
//     opfic   := operator('fresnelC, 1)
//     opGamma     := operator('Gamma, 1)
//     opGamma2    := operator('Gamma2, 2)
//     opBeta      := operator('Beta, 2)
//     opBeta3     := operator('Beta3, 3)
//     opdigamma   := operator('digamma, 1)
//     oppolygamma := operator('polygamma, 2)
//     opBesselJ   := operator('besselJ, 2)
//     opBesselY   := operator('besselY, 2)
//     opBesselI   := operator('besselI, 2)
//     opBesselK   := operator('besselK, 2)
//     opAiryAi    := operator('airyAi,  1)
//     opAiryAiPrime := operator('airyAiPrime,  1)
//     opAiryBi    := operator('airyBi , 1)
//     opAiryBiPrime := operator('airyBiPrime,  1)
//     opLambertW := operator('lambertW,  1)
//     opPolylog := operator('polylog, 2)
//     opWeierstrassP := operator('weierstrassP, 3)
//     opWeierstrassPPrime := operator('weierstrassPPrime, 3)
//     opWeierstrassSigma := operator('weierstrassSigma, 3)
//     opWeierstrassZeta := operator('weierstrassZeta, 3)
//     -- arbitrary arity
//     opHypergeometricF := operator('hypergeometricF)$BasicOperator
//     opMeijerG := operator('meijerG)$BasicOperator
//
//     opWhittakerM := operator('whittakerM, 3)$OP
//     opWhittakerW := operator('whittakerW, 3)$OP
//     opAngerJ := operator('angerJ, 2)$OP
//     opWeberE := operator('weberE, 2)$OP
//     opStruveH := operator('struveH, 2)$OP
//     opStruveL := operator('struveL, 2)$OP
//     opHankelH1 := operator('hankelH1, 2)$OP
//     opHankelH2 := operator('hankelH2, 2)$OP
//     opLommelS1 := operator('lommelS1, 3)$OP
//     opLommelS2 := operator('lommelS2, 3)$OP
//     opKummerM := operator('kummerM, 3)$OP
//     opKummerU := operator('kummerU, 3)$OP
//     opLegendreP := operator('legendreP, 3)$OP
//     opLegendreQ := operator('legendreQ, 3)$OP
//     opKelvinBei := operator('kelvinBei, 2)$OP
//     opKelvinBer := operator('kelvinBer, 2)$OP
//     opKelvinKei := operator('kelvinKei, 2)$OP
//     opKelvinKer := operator('kelvinKer, 2)$OP
//     opEllipticK := operator('ellipticK, 1)$OP
//     opEllipticE := operator('ellipticE, 1)$OP
//     opEllipticE2 := operator('ellipticE2, 2)$OP
//     opEllipticF := operator('ellipticF, 2)$OP
//     opEllipticPi := operator('ellipticPi, 3)$OP
//     opJacobiSn := operator('jacobiSn, 2)$OP
//     opJacobiCn := operator('jacobiCn, 2)$OP
//     opJacobiDn := operator('jacobiDn, 2)$OP
//     opJacobiZeta := operator('jacobiZeta, 2)$OP
//     opJacobiTheta := operator('jacobiTheta, 2)$OP
//     opWeierstrassPInverse := operator('weierstrassPInverse, 3)$OP
//     opLerchPhi := operator('lerchPhi, 3)$OP
//     opRiemannZeta := operator('riemannZeta, 1)$OP
//
//     -- orthogonal polynomials
//     opCharlierC := operator('charlierC, 3)$OP
//     op_hahn_p := operator('hahn_p, 5)$OP
//     op_hahnQ := operator('hahnQ, 5)$OP
//     op_hahnR := operator('hahnR, 5)$OP
//     op_hahnS := operator('hahnS, 5)$OP
//     opHermiteH := operator('hermiteH, 2)$OP
//     opJacobiP := operator('jacobiP, 4)$OP
//     op_krawtchoukK := operator('krawtchoukK, 4)$OP
//     opLaguerreL := operator('laguerreL, 3)$OP
//     opMeixnerM := operator('meixnerM, 4)$OP
//     op_meixnerP := operator('meixnerP, 4)$OP
//     op_racahR := operator('racahR, 6)$OP
//     op_wilsonW := operator('wilsonW, 6)$OP
//
//     op_log_gamma := operator('%logGamma, 1)$OP
//     op_eis := operator('%eis, 1)$OP
//     op_erfs := operator('%erfs, 1)$OP
//     op_erfis := operator('%erfis, 1)$OP
//
//     opint   := operator('integral, 3)
//     -- arbitrary arity
//     opiint  := operator('%iint)$BasicOperator
//     opdint  := operator('%defint, 5)
//     opfact  := operator('factorial, 1)
//     opperm  := operator('permutation, 2)
//     opbinom := operator('binomial, 2)
//     oppow   := operator(POWER, 2)
//     opsum   := operator('summation, 3)
//     opdsum  := operator('%defsum, 5)
//     opprod  := operator('product, 3)
//     opdprod := operator('%defprod, 5)
//
//     oprootsum := operator('%root_sum, 3)
//     opfloor := operator('floor, 1)
//     opceil := operator('ceil, 1)
//     op_fractionPart := operator('fractionPart, 1)
//     opreal := operator('real, 1)
//     opimag := operator('imag, 1)
//     opconjugate := operator('conjugate, 1)
//     oparg := operator('arg, 1)
//     opsign := operator('sign, 1)
//     op_unitStep := operator('unitStep, 1)
//     op_diracDelta := operator('diracDelta, 1)
//     -- arbitrary arity
//     opmax := operator('max)$BasicOperator
//     opmin := operator('min)$BasicOperator
//
//     algop   := [oproot, opalg]$List(OP)
//     rtrigop := [opsin, opcos, optan, opcot, opsec, opcsc,
//                          opasin, opacos, opatan, opacot, opasec, opacsc]
//     htrigop := [opsinh, opcosh, optanh, opcoth, opsech, opcsch,
//                    opasinh, opacosh, opatanh, opacoth, opasech, opacsch]
//     trigop  := concat(rtrigop, htrigop)
//     elemop  := concat(trigop, [oppi, oplog, opexp])
//     primop  := [opei, opli, opsi, opci, opshi, opchi, operf, operfi, opli2,
//                    opint, opdint, opfis, opfic, opiint]
//     combop  := [opfact, opperm, opbinom, oppow,
//                                          opsum, opdsum, opprod, opdprod]
//     specop  := [opGamma, opGamma2, opBeta, opBeta3, opdigamma, oppolygamma,
//                opfloor, opceil, opreal, opimag, opsign, opabs, opmax, opmin,
//                    op_fractionPart, op_unitStep,
//                  op_diracDelta, oparg, opconjugate, op_log_gamma,
//                    op_eis, op_erfs, op_erfis,
//                 opBesselJ, opBesselY, opBesselI, opBesselK, opAiryAi, opAiryBi,
//                  opAiryAiPrime, opAiryBiPrime, opLambertW, opPolylog,
//                   opWeierstrassP, opWeierstrassPPrime, opWeierstrassZeta,
//                    opWeierstrassSigma, opHypergeometricF, opMeijerG, _
//     opWhittakerM, _
//     opWhittakerW, _
//     opAngerJ, _
//     opWeberE, _
//     opStruveH, _
//     opStruveL, _
//     opHankelH1, _
//     opHankelH2, _
//     opLommelS1, _
//     opLommelS2, _
//     opKummerM, _
//     opKummerU, _
//     opLegendreP, _
//     opLegendreQ, _
//     opKelvinBei, _
//     opKelvinBer, _
//     opKelvinKei, _
//     opKelvinKer, _
//     opEllipticK, _
//     opEllipticE, _
//     opEllipticE2, _
//     opEllipticF, _
//     opEllipticPi, _
//     opJacobiSn, _
//     opJacobiCn, _
//     opJacobiDn, _
//     opJacobiZeta, _
//     opJacobiTheta, _
//     opLerchPhi, _
//     opRiemannZeta, _
//     opCharlierC, _
//     op_hahn_p, op_hahnQ, op_hahnR, op_hahnS, _
//     opHermiteH, _
//     opJacobiP, _
//     op_krawtchoukK, _
//     opLaguerreL, _
//     opMeixnerM, op_meixnerP, _
//     op_racahR, _
//     op_wilsonW, _
//     opWeierstrassPInverse _
//     ]
//
//     anyop   := [oppren, opdiff, opbox, opquote]
//     allop   := concat(concat(concat(concat(concat(
//                             algop, elemop), primop), combop), specop), anyop)
//
//     -- odd and even single argument operators, must be maintained current!
//     evenop := [opcos, opsec, opcosh, opsech, opabs, op_diracDelta]
//     oddop  := [opsin, opcsc, optan, opcot, opasin, opacsc, opatan,
//                opsinh, opcsch, optanh, opcoth, opasinh, opacsch, opatanh,
//                 opacoth, opsi, opshi, operf, operfi, opfis, opfic,
//                  opsign, opreal, opimag]
//
// -- operators whose second argument is a dummy variable
//     dummyvarop1 := [opdiff, opalg, opint, oprootsum, opsum, opprod]
// -- operators whose second and third arguments are dummy variables
//     dummyvarop2 := [opdint, opdsum, opdprod]
//
//     operator s ==
//       if brandNew? then startUp false
//       for op in allop repeat
//         is?(op, s) => return copy op
//       operator(s)$OP
//
//     dpi l    == '%pi::O
//     dfact x  == postfix("!"::Symbol::O, (ATOM(x)$Lisp => x; paren x))
//     dquote l == prefix(quote(first(l)::O), rest l)
//     dgamma l == prefix('Gamma::O, l)
//     dbeta(l) == prefix('Beta::O, l)
//     dEllipticE2(l : List O) : O == prefix('ellipticE::O, l)
//
//     setDummyVar(op, n) == setProperty(op, DUMMYVAR, n pretend None)
//
//     dexp x ==
//       e := '%e::O
//       x = 1::O => e
//       e ^ x
//
//     inputdefsum(a: List InputForm): InputForm ==
//         seg: InputForm := convert([convert('SEGMENT)$InputForm, a.4, a.5])$InputForm
//         eq: InputForm := convert([convert('equation)$InputForm, a.2, seg])
//         convert([convert('sum)$InputForm, a.1, eq])
//
//     inputdefprod(a: List InputForm): InputForm ==
//         seg: InputForm := convert([convert('SEGMENT)$InputForm, a.4, a.5])$InputForm
//         eq: InputForm := convert([convert('equation)$InputForm, a.2, seg])
//         convert([convert('product)$InputForm, a.1, eq])
//
//     startUp b ==
//       brandNew? := b
//       display(oppren, paren)
//       display(opbox, commaSeparate)
//       display(oppi, dpi)
//       display(opexp, dexp)
//       display(opBeta3, dbeta)
//       setProperty(opBeta3, 'disp_name, ('Beta::O) pretend None)
//       display(opGamma2, dgamma)
//       setProperty(opGamma2, 'disp_name, ('Gamma::O) pretend None)
//       display(opEllipticE2, dEllipticE2)
//       display(opfact, dfact)
//       display(opquote, dquote)
//       display(opperm, (z1 : List O) : O +-> supersub('A::O, z1))
//       display(opbinom, (z1 : List O) : O +-> binomial(first z1, second z1))
//       display(oppow, (z1 : List O) : O +-> first(z1) ^ second(z1))
//       display(opsum, (z1 : List O) : O +-> sum(first z1, second z1, third z1))
//       display(opprod, (z1 : List O) : O +-> prod(first z1, second z1, third z1))
//       display(opint, (z1 : List O) : O +-> int(first z1 * hconcat('d::O, second z1),
//                                                    empty(), third z1))
//       input(oppren, (z1 : List InputForm) : InputForm +-> convert concat(convert("("::Symbol)@InputForm,
//                             concat(z1, convert(")"::Symbol)@InputForm)))
//       input(oppow, (z1 : List InputForm) : InputForm +-> convert concat(convert("^"::Symbol)@InputForm, z1))
//       input(oproot,
//             (z1 : List InputForm) : InputForm +-> convert [convert("^"::Symbol)@InputForm, first z1, 1 / second z1])
//       input(opdsum, inputdefsum)
//       input(opdprod, inputdefprod)
//
//       for op in algop   repeat assert(op, ALGOP)
//       for op in rtrigop repeat assert(op, 'rtrig)
//       for op in htrigop repeat assert(op, 'htrig)
//       for op in trigop  repeat assert(op, 'trig)
//       for op in elemop  repeat assert(op, 'elem)
//       for op in primop  repeat assert(op, 'prim)
//       for op in combop  repeat assert(op, 'comb)
//       for op in specop  repeat assert(op, 'special)
//       for op in anyop   repeat assert(op, 'any)
//       for op in evenop  repeat assert(op, EVEN)
//       for op in oddop   repeat assert(op, ODD)
//       for op in dummyvarop1 repeat setDummyVar(op, 1)
//       for op in dummyvarop2 repeat setDummyVar(op, 2)
//       assert(oppren, 'linear)
//       void