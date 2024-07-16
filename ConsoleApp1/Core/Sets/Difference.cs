// namespace ConsoleApp1.Core.Sets;
//
// public class Difference : Set
// {
//
//     public Set A;
//     public Set B;
//     
//     public Difference(Set a, Set b)
//     {
//         A = a;
//         B = b;
//     }
//
//
//     public override long? Length()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override bool IsEnumerable()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override IEnumerable<double> GetEnumerable()
//     {
//         return A.GetEnumerable().Where(element => !B.Contain(element));
//     }
//
//     public override double Max()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override double Min()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override double PrincipalValue()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override bool Contain(double x)
//     {
//         return A.Contain(x) && !B.Contain(x);
//     }
// }