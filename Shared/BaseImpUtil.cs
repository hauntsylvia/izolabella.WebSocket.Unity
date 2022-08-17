using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace izolabella.WebSocket.Unity.Shared
{
    public static class BaseImpUtil
    {
        /// <summary>
        /// Gets all items that implement a particular class and have a parameterless constructor, initializes them, and returns them.
        /// </summary>
        /// <typeparam name="T">The base class.</typeparam>
        /// <param name="From">The assembly to load classes from.</param>
        /// <returns>A <see cref="List{T}"/> of initialized items.</returns>
        public static List<T> GetItems<T>(Assembly? From = null)
        {
            List<T> R = new();
            foreach (Type Ty in (From ?? Assembly.GetCallingAssembly()).GetTypes().Where(Ty => typeof(T).IsAssignableFrom(Ty) && !Ty.IsInterface && !Ty.IsAbstract && Ty.GetConstructor(Type.EmptyTypes) != null))
            {
                object? O = Activator.CreateInstance(Ty);
                if (O is not null and T M)
                {
                    R.Add(M);
                }
            }
            return R;
        }

        /// <summary>
        /// Gets all items that implement a particular class and have a parameterless constructor, initializes them, and returns them.
        /// </summary>
        /// <typeparam name="T">The base class.</typeparam>
        /// <param name="From">The assemblies to load classes from.</param>
        /// <returns>A <see cref="List{T}"/> of initialized items.</returns>
        public static List<T> GetItems<T>(Assembly?[] Assemblies)
        {
            List<T> R = new();
            foreach (Assembly? From in Assemblies)
            {
                R.AddRange(GetItems<T>(From));
            }
            return R;
        }
    }
}
