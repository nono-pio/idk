namespace ConsoleApp1.Core.Polynomials.Rings;

public class IntegersMod : Ring<int>
{

    public int Mod;
    public IntegersMod(int mod)
    {
        Mod = mod;
    }

    public override int Zero => 0;
    public override int One => 1;
    public override int ValueOf(int value)
    {
        return value % Mod;
    }

    public override bool IsInt(int e, int value) => e == value % Mod;
    public override bool IsZero(int e) => e == 0;
    public override bool IsOne(int e) => e == 1;

    public override int Add(int a, int b)
    {
        return (a + b) % Mod;
    }

    public override int Sub(int a, int b)
    {
        return b > a ? a - b + Mod : a - b;
    }

    public override int Neg(int e)
    {
        return e == 0 ? 0 : Mod - e;
    }

    public override int Mul(int a, int b)
    {
        return (a * b) % Mod;
    }

    public override int Div(int a, int b)
    {
        return a * ModInv(b);
    }
    
    public int ModInv(int a)
    {
        var ex = NumberUtils.ExtendedGcd(a, Mod); // O(log^2 Mod)
        if (ex.Gcd != 1)
            throw new Exception("Modular inverse does not exist");
        
        return ex.s;
        // O(Mod)
        // for (int i = 1; i < Mod; i++)
        // {
        //     if ((a * i) % Mod == 1)
        //         return i;
        // }
        //
        // return 0;
    }

    public override int Rem(int a, int b)
    {
        return 0;
    }

    public override (int Quotient, int Remainder) DivRem(int a, int b)
    {
        return (Div(a, b), 0);
    }
}