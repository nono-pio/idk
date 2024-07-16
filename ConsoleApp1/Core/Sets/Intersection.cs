// namespace ConsoleApp1.Core.Sets;
//
// public class Intersection : Set
// {
//
//     public Set[] Sets;
//     public Intersection(params Set[] sets)
//     {
//         Sets = sets;
//     }
//
//     /*
//     Rules :
//     1. Empty set : A ∩ {} = {}
//     2. Intersection of Intersection : A ∩ (B ∩ C) = A ∩ B ∩ C
//     3. Universal set U : U ∩ A = A
//     4. FiniteSet Intersection : [1,2] ∩ [2,3] = [2]
//     5. Interval Intersection : [1,2] ∩ [2,4] = {2}
//     6. TODO ...
//     */
//     public static Set EvalIntersection(params Set[] sets)
//     {
//         
//         if (sets.Length == 0)
//             return EmptySet;
//         if (sets.Length == 1)
//             return sets[0];
//         
//         List<Set> newSets = new();
//         FiniteSet? concatFiniteSets = null;
//         foreach (var set in GetEnumerableIntersectionSets(sets)) // 2. Intersection of Intersection
//         {
//             if (set.IsEmpty) // 1. Empty set
//                 return EmptySet;
//
//             
//             switch (set)
//             {
//                 case UniversalSet: // 3. Universal set
//                     continue;
//                 
//                 case FiniteSet finiteSet: // 4. FiniteSet Intersection
//                     
//                     if (concatFiniteSets is null)
//                         concatFiniteSets = finiteSet.Copy();
//                     else 
//                         concatFiniteSets.IntersectionFiniteSet(finiteSet);
//                     
//                     break;
//                 
//                 case Interval interval: // 5. Interval Intersection
//                     throw new NotImplementedException();
//                     break;
//                 
//                 default: // 6. TODO ...
//                     newSets.Add(set);
//                     break;
//             }
//         }
//
//         if (concatFiniteSets is not null)
//         {
//             if (concatFiniteSets.IsEmpty)
//                 return EmptySet;
//             
//             newSets.Add(concatFiniteSets);
//         }
//
//         return newSets.Count switch
//         {
//             0 => EmptySet,
//             1 => newSets[0],
//             _ => new Intersection(newSets.ToArray())
//         };
//     }
//
//     public override long? Length() => throw new NotImplementedException();
//
//     public override bool IsEnumerable()
//     {
//         return Sets.All(set => set.IsEnumerable());
//     }
//     
//     public override IEnumerable<double> GetEnumerable()
//     {
//         return Sets[0].GetEnumerable().Where(Contain);
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
//         return Sets.All(set => set.Contain(x));
//     }
// }