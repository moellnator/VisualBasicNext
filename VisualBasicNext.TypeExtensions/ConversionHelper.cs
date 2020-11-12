﻿using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CSharpBinder = Microsoft.CSharp.RuntimeBinder.Binder;

/// <summary>
/// Code extracted from Github MedallionUtilities by madelson
/// https://github.com/madelson/MedallionUtilities/tree/master/MedallionReflection
/// Published under MIT License:
/// https://github.com/madelson/MedallionUtilities/blob/master/License.txt
/// </summary>

namespace VisualBasicNext.TypeExtensions
{
    public static partial class Reflect
    {
        public static bool IsCastableTo(this Type from, Type to)
        {
            if (from == null || to == null) { return false; }

            return ConversionHelper.CanExplicitCast(from, to);
        }

        public static bool IsImplicitlyCastableTo(this Type from, Type to)
        {
            if (from == null) { throw new ArgumentNullException(nameof(from)); }
            if (to == null) { throw new ArgumentNullException(nameof(to)); }

            return ConversionHelper.CanImplicitCast(from, to);
        }

        internal static class ConversionHelper
        {
            #region ---- Explicit casts ----
            public static bool CanExplicitCast(Type from, Type to)
            {
                if (from == null || to == null) { return false; }
                // explicit conversion always works if there's implicit conversion
                if (CanImplicitCast(from, to))
                {
                    return true;
                }

                var key = new KeyValuePair<Type, Type>(from, to);
                if (TryGetCachedValue(CastCache, key, out bool cachedValue))
                {
                    return cachedValue;
                }

                // for nullable types, we can simply strip off the nullability and evaluate the underyling types
                var underlyingFrom = Nullable.GetUnderlyingType(from);
                var underlyingTo = Nullable.GetUnderlyingType(to);
                if (underlyingFrom != null || underlyingTo != null)
                {
                    return (underlyingFrom ?? from).IsCastableTo(underlyingTo ?? to);
                }

                bool result;

                if ((from == typeof(string) && to.IsValueType && to.IsPrimitive) || (to == typeof(string) && from.IsValueType && from.IsPrimitive)) return true;

                if (from.IsValueType)
                {
                    if (to.IsValueType && to.IsPrimitive) return true;
                    result = (bool) GetMethod(() => AttemptExplicitCast<object, object>())
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(from, to)
                            .Invoke(null, new object[0]);
                }
                else
                {
                    // if the from type is null, the dynamic logic above won't be of any help because 
                    // either both types are nullable and thus a runtime cast of null => null will 
                    // succeed OR we get a runtime failure related to the inability to cast null to 
                    // the desired type, which may or may not indicate an actual issue. thus, we do 
                    // the work manually
                    result = CanExplicitCastNonValueType(from, to);
                }

                UpdateCache(CastCache, key, result);
                return result;
            }

            private static bool CanExplicitCastNonValueType(Type from, Type to)
            {
                if ((to.IsInterface && !from.IsSealed)
                    || (from.IsInterface && !to.IsSealed))
                {
                    // any non-sealed type can be cast to any interface since the runtime type MIGHT implement
                    // that interface. The reverse is also true; we can cast to any non-sealed type from any interface
                    // since the runtime type that implements the interface might be a derived type of to.
                    return true;
                }

                // arrays are complex because of array covariance 
                // (see http://msmvps.com/blogs/jon_skeet/archive/2013/06/22/array-covariance-not-just-ugly-but-slow-too.aspx).
                // Thus, we have to allow for things like var x = (IEnumerable<string>)new object[0];
                // and var x = (object[])default(IEnumerable<string>);
                var arrayType = from.IsArray && !from.GetElementType().IsValueType ? from
                    : to.IsArray && !to.GetElementType().IsValueType ? to
                    : null;
                if (arrayType != null)
                {
                    var genericInterfaceType = from.IsInterface && from.IsGenericType ? from
                        : to.IsInterface && to.IsGenericType ? to
                        : null;
                    if (genericInterfaceType != null)
                    {
                        return arrayType.GetInterfaces()
                            .Any(i => i.IsGenericType
                                && i.GetGenericTypeDefinition() == genericInterfaceType.GetGenericTypeDefinition()
                                && i.GetGenericArguments().Zip(to.GetGenericArguments(), (ia, ta) => ta.IsAssignableFrom(ia) || ia.IsAssignableFrom(ta)).All(b => b));
                    }
                }

                // look for conversion operators. Even though we already checked for implicit conversions, we have to look
                // for operators of both types because, for example, if a class defines an implicit conversion to int then it can be explicitly
                // cast to uint
                const BindingFlags conversionFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                var conversionMethods = from.GetMethods(conversionFlags)
                    .Concat(to.GetMethods(conversionFlags))
                    .Where(m => (m.Name == "op_Explicit" || m.Name == "op_Implicit")
                        && m.Attributes.HasFlag(MethodAttributes.SpecialName)
                        && m.GetParameters().Length == 1
                        && (
                            // the from argument of the conversion function can be an indirect match to from in
                            // either direction. For example, if we have A : B and Foo defines a conversion from B => Foo,
                            // then C# allows A to be cast to Foo
                            m.GetParameters()[0].ParameterType.IsAssignableFrom(from)
                            || from.IsAssignableFrom(m.GetParameters()[0].ParameterType)
                        )
                    );

                if (to.IsPrimitive && typeof(IConvertible).IsAssignableFrom(to))
                {
                    // as mentioned above, primitive convertible types (i. e. not IntPtr) get special 
                    // treatment in the sense that if you can convert from Foo => int, you can convert
                    // from Foo => double as well
                    return conversionMethods.Any(m => m.ReturnType.IsCastableTo(to));
                }

                return conversionMethods.Any(m => m.ReturnType == to);
            }

            private static bool AttemptExplicitCast<TFrom, TTo>()
            {
                // based on the IL generated from
                // var x = (TTo)(dynamic)default(TFrom);
                try
                {

                    var binder = CSharpBinder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(TTo), typeof(ConversionHelper));
                        var callSite = CallSite<Func<CallSite, TFrom, TTo>>.Create(binder);
                        callSite.Target(callSite, default(TFrom));
                    return true;
                }
                catch (RuntimeBinderException ex)
                {
                    return !ex.Message.EndsWith(" konvertiert werden.");
                }
            }   
            #endregion

            #region ---- Implicit casts ----
            public static bool CanImplicitCast(Type from, Type to)
            {
                // not strictly necessary, but speeds things up and avoids polluting the cache
                if (to.IsAssignableFrom(from))
                {
                    return true;
                }

                var key = new KeyValuePair<Type, Type>(from, to);
                if (TryGetCachedValue(ImplicitCastCache, key, out bool cachedValue))
                {
                    return cachedValue;
                }

                bool result = false;

                result =(bool) GetMethod(() => AttemptImplicitCast<object, object>())
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(from, to)
                    .Invoke(null, new object[0]);
                

                UpdateCache(ImplicitCastCache, key, result);
                return result;
            }

            private static bool AttemptImplicitCast<TFrom, TTo>()
            {
                // based on the IL produced by:
                // dynamic list = new List<TTo>();
                // list.Add(Get<TFrom>());
                // We can't use the above code because it will mimic a cast in a generic method
                // which doesn't have the same semantics as a cast in a non-generic method

                var list = new List<TTo>(capacity: 0);
                var binder = CSharpBinder.InvokeMember(
                    flags: CSharpBinderFlags.ResultDiscarded,
                    name: "Add",
                    typeArguments: null,
                    context: typeof(ConversionHelper),
                    argumentInfo: new[]
                    {
                        CSharpArgumentInfo.Create(flags: CSharpArgumentInfoFlags.None, name: null),
                        CSharpArgumentInfo.Create(
                            flags: CSharpArgumentInfoFlags.UseCompileTimeType,
                            name: null
                        ),
                    }
                );
                var callSite = CallSite<Action<CallSite, object, TFrom>>.Create(binder);
                try
                {
                    callSite.Target.Invoke(callSite, list, default(TFrom));
                    return true;
                }
                catch (RuntimeBinderException ex)
                {
                    return !ex.Message.StartsWith("Die beste Übereinstimmung");
                }
            }
            #endregion

            #region ---- Caching ----
            private const int MaxCacheSize = 5000;
            private static readonly Dictionary<KeyValuePair<Type, Type>, bool> CastCache = new Dictionary<KeyValuePair<Type, Type>, bool>(),
                ImplicitCastCache = new Dictionary<KeyValuePair<Type, Type>, bool>();

            private static bool TryGetCachedValue<TKey, TValue>(Dictionary<TKey, TValue> cache, TKey key, out TValue value)
            {
                ICollection collection = cache;
                lock (collection.SyncRoot)
                {
                    return cache.TryGetValue(key, out value);
                }
            }

            private static void UpdateCache<TKey, TValue>(Dictionary<TKey, TValue> cache, TKey key, TValue value)
            {
                ICollection collection = cache;
                lock (collection.SyncRoot)
                {
                    if (cache.Count > MaxCacheSize)
                    {
                        cache.Clear();
                    }
                    cache[key] = value;
                }
            }
            #endregion
        }
    }
}
