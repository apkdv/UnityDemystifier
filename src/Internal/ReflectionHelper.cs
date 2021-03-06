// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Apkd.Internal
{
    /// <summary>
    /// A helper class that contains utilities methods for dealing with reflection.
    /// </summary>
    internal static class ReflectionHelper
    {
        static PropertyInfo tranformerNamesLazyPropertyInfo;
        static readonly Type isReadOnlyAttribute = Type.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute", false);
        static readonly Type tupleElementNamesAttribute = Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute", false);

        /// <summary>
        /// Returns true if <paramref name="type"/> is <code>System.Runtime.CompilerServices.IsReadOnlyAttribute</code>.
        /// </summary>
        internal static bool IsIsReadOnlyAttribute(this Type type)
            => type == isReadOnlyAttribute;

        /// <summary>
        /// Returns true if the <paramref name="type"/> is a value tuple type.
        /// </summary>
        internal static bool IsValueTuple(this Type type)
            => type.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);

        /// <summary>
        /// Returns true if the given <paramref name="attribute"/> is of type <code>TupleElementNameAttribute</code>.
        /// </summary>
        /// <remarks>
        /// To avoid compile-time depencency hell with System.ValueTuple, this method uses reflection and not checks statically that 
        /// the given <paramref name="attribute"/> is <code>TupleElementNameAttribute</code>.
        /// </remarks>
        internal static bool IsTupleElementNameAttribute(this Attribute attribute)
            => attribute.GetType() == tupleElementNamesAttribute;

        /// <summary>
        /// Returns 'TransformNames' property value from a given <paramref name="attribute"/>.
        /// </summary>
        /// <remarks>
        /// To avoid compile-time depencency hell with System.ValueTuple, this method uses reflection 
        /// instead of casting the attribute to a specific type.
        /// </remarks>
        internal static IList<string> GetTransformNames(this Attribute attribute)
        {
            System.Diagnostics.Debug.Assert(attribute.IsTupleElementNameAttribute());
            var propertyInfo = GetTransformNamesPropertyInfo(attribute.GetType());
            return (IList<string>)propertyInfo.GetValue(attribute);
        }

        static PropertyInfo GetTransformNamesPropertyInfo(Type attributeType)
            => LazyInitializer.EnsureInitialized(ref tranformerNamesLazyPropertyInfo,
                () => attributeType.GetProperty("TransformNames", BindingFlags.Instance | BindingFlags.Public));
    }
}
