using System.Reflection;

namespace Nro.Tests.Templates;

/// <summary>A Java type or name cannot be mapped to C# according to the project's rules.</summary>
internal sealed class MappingException(string message) : Exception(message);

/// <summary>
/// Java-to-C# mapping rules (PC/01-ANALYSIS-MAPPING.md sections 3.3 and 3.5), shared by every verification step of A-04.
/// This type table is "pinned down" by hand-written canary tests in <c>TemplatesParityTests</c>, so that if both the C# code and this table
/// are wrong together (for example both map byte to unsigned byte), a test still fails.
/// </summary>
internal static class JavaTypeMap
{
    public const string ClrNamespace = "Nro.Core.Templates";

    private const string JavaPackagePrefix = "nro.template.";

    private static readonly Assembly CoreAssembly = typeof(Nro.Core.AssemblyMarker).Assembly;

    private static readonly Dictionary<Type, string> Aliases = new()
    {
        [typeof(sbyte)] = "sbyte",
        [typeof(byte)] = "byte",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(bool)] = "bool",
        [typeof(char)] = "char",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(string)] = "string",
        [typeof(object)] = "object",
    };

    /// <summary>Rule 3.5: capitalize only the first letter, do not split words (<c>iconID</c> becomes <c>IconID</c>).</summary>
    public static string ToClrName(string javaName)
    {
        if (javaName.Length == 0)
        {
            throw new MappingException("Java name is empty");
        }

        return char.ToUpperInvariant(javaName[0]) + javaName[1..];
    }

    /// <summary>The expected full name of the C# class (nested classes use a plus sign, as in .NET reflection).</summary>
    public static string ExpectedClrName(string javaBinaryName)
    {
        if (!javaBinaryName.StartsWith(JavaPackagePrefix, StringComparison.Ordinal))
        {
            throw new MappingException($"Java class outside the nro.template package: {javaBinaryName}");
        }

        return ClrNamespace + "." + javaBinaryName[JavaPackagePrefix.Length..].Replace('$', '+');
    }

    public static Type? FindClrClass(string javaBinaryName)
    {
        return CoreAssembly.GetType(ExpectedClrName(javaBinaryName), throwOnError: false);
    }

    /// <summary>Java type (the string from <c>Type.getTypeName()</c>) to C# type. <paramref name="genericArgument"/> allows wrapper types (Integer) when inside a generic.</summary>
    public static Type ToClr(string javaType, bool genericArgument = false)
    {
        string t = javaType.Trim();
        if (t.EndsWith("[]", StringComparison.Ordinal))
        {
            return ToClr(t[..^2], genericArgument).MakeArrayType();
        }

        int lt = t.IndexOf('<', StringComparison.Ordinal);
        if (lt >= 0)
        {
            if (!t.EndsWith('>'))
            {
                throw new MappingException($"malformed generic type: {javaType}");
            }

            Type[] args = SplitTopLevel(t[(lt + 1)..^1]).Select(a => ToClr(a, genericArgument: true)).ToArray();
            Type? definition = t[..lt] switch
            {
                "java.util.List" => typeof(List<>),
                "java.util.Map" => typeof(Dictionary<,>),
                _ => null,
            };
            if (definition is null || definition.GetGenericArguments().Length != args.Length)
            {
                throw new MappingException($"no mapping rule for {javaType}");
            }

            return definition.MakeGenericType(args);
        }

        switch (t)
        {
            case "byte":
                return typeof(sbyte);
            case "short":
                return typeof(short);
            case "int":
                return typeof(int);
            case "long":
                return typeof(long);
            case "boolean":
                return typeof(bool);
            case "char":
                return typeof(char);
            case "float":
                return typeof(float);
            case "double":
                return typeof(double);
            case "java.lang.String":
                return typeof(string);
            case "java.lang.Byte" when genericArgument:
                return typeof(sbyte);
            case "java.lang.Short" when genericArgument:
                return typeof(short);
            case "java.lang.Integer" when genericArgument:
                return typeof(int);
            case "java.lang.Long" when genericArgument:
                return typeof(long);
            case "java.lang.Boolean" when genericArgument:
                return typeof(bool);
        }

        if (t.StartsWith(JavaPackagePrefix, StringComparison.Ordinal))
        {
            return FindClrClass(t) ?? throw new MappingException($"missing C# class {ExpectedClrName(t)} for Java type {t}");
        }

        throw new MappingException($"no mapping rule for Java type {javaType}");
    }

    /// <summary>The expected C# access level from the Java modifiers: no modifier (package-private) means internal.</summary>
    public static string ExpectedAccess(IEnumerable<string> javaModifiers)
    {
        string[] mods = javaModifiers.ToArray();
        if (mods.Contains("public"))
        {
            return "public";
        }

        if (mods.Contains("protected"))
        {
            return "protected";
        }

        return mods.Contains("private") ? "private" : "internal";
    }

    public static string Access(MethodBase m)
    {
        if (m.IsPublic)
        {
            return "public";
        }

        if (m.IsFamily)
        {
            return "protected";
        }

        if (m.IsPrivate)
        {
            return "private";
        }

        if (m.IsAssembly)
        {
            return "internal";
        }

        return m.IsFamilyOrAssembly ? "protected internal" : "private protected";
    }

    /// <summary>A short C# type name (sbyte, short, List&lt;int&gt;...) to keep error messages readable.</summary>
    public static string Pretty(Type t)
    {
        if (t.IsArray)
        {
            return Pretty(t.GetElementType() ?? typeof(object)) + "[]";
        }

        if (t.IsGenericType)
        {
            string name = t.Name[..t.Name.IndexOf('`', StringComparison.Ordinal)];
            return name + "<" + string.Join(", ", t.GetGenericArguments().Select(Pretty)) + ">";
        }

        return Aliases.TryGetValue(t, out string? alias) ? alias : t.Name;
    }

    private static IEnumerable<string> SplitTopLevel(string s)
    {
        int depth = 0;
        int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            switch (s[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    yield return s[start..i];
                    start = i + 1;
                    break;
            }
        }

        yield return s[start..];
    }
}
