// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Apkd.Internal
{
    internal class ResolvedParameter
    {
        internal string Name { get; set; }

        internal System.Type ResolvedType { get; set; }

        internal string Prefix { get; set; }
        internal string Prefix2 { get; set; }

        public override string ToString() => Append(new StringBuilder()).ToString();

        internal StringBuilder Append(StringBuilder sb)
        {
            sb.AppendFormattingChar('‹');

            if (!string.IsNullOrEmpty(Prefix2))
                sb.Append(Prefix2).Append(' ');

            if (!string.IsNullOrEmpty(Prefix))
                sb.Append(Prefix).Append(' ');

            if (ResolvedType != null)
                AppendTypeName(sb);
            else
                sb.Append('?');

            sb.AppendFormattingChar('›');

            if (!string.IsNullOrEmpty(Name))
                sb.Append(' ').Append(Name);

            return sb;
        }

        internal virtual void AppendTypeName(StringBuilder sb) 
        {
            sb.AppendTypeDisplayName(ResolvedType, fullName: false, includeGenericParameterNames: true);
        }
    }
}
