using System.Reflection;

namespace Polynomials.Utils;

using System;

public static class GenericHandler
{
    /// <summary>
    /// Appelle une méthode dédiée pour un type générique particulier si le type T correspond au modèle spécifié.
    /// </summary>
    /// <typeparam name="T">Le type générique actuel.</typeparam>
    /// <param name="genericDefinition">Le type générique attendu, par exemple typeof(Rational<>).</param>
    /// <param name="methodName">Le nom de la méthode spécifique à appeler.</param>
    /// <param name="targetType">La classe contenant la méthode.</param>
    /// <param name="arguments">Les arguments à transmettre à la méthode.</param>
    /// <returns>Le résultat de l'appel à la méthode.</returns>
    public static object? InvokeForGeneric<T>(Type genericDefinition, string methodName, Type targetType, params object[] arguments)
    {
        // Vérifie si T est un type générique correspondant au modèle
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == genericDefinition)
        {
            // Récupère le type interne générique
            var innerType = typeof(T).GetGenericArguments()[0];

            // Recherche la méthode avec réflexion et la spécialise
            var method = targetType.GetMethod(methodName,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | BindingFlags.Public)?
                .MakeGenericMethod(innerType);

            if (method != null)
            {
                return method.Invoke(null, arguments);
            }
        }

        throw new NotImplementedException($"No implementation found for type {typeof(T)} with generic base {genericDefinition}.");
    }
}
