// See https://aka.ms/new-console-template for more information

using Sdcb.Arithmetic.Mpfr;


var x = MpfrFloat.ConstPi(precision: 1000);

Console.WriteLine(x);

x.Dispose();
