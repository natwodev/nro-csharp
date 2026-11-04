using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Nro.Tests.Unit.Core.Templates;

/// <summary>The result of one comparison run: the list of errors and the number of items checked (to prove nothing was skipped).</summary>
internal sealed class VerifyResult
{
    public List<string> Errors { get; } = [];

    public int Classes { get; set; }

    public int NestedTypes { get; set; }

    public int Fields { get; set; }

    public int Constructors { get; set; }

    public int Methods { get; set; }

    public int Instances { get; set; }

    public int Setters { get; set; }

    /// <summary>Array/list/object fields: the value cannot be flipped; already checked in CheckInstances (element by element).</summary>
    public int SettersSkipped { get; set; }

    public int Values { get; set; }

    public int NullDefaults { get; set; }

    public void Error(JavaClass jc, string message) => Errors.Add($"[{jc.Name}] {message}");
}

/// <summary>
/// Compares the C# classes with the description taken from the compiled Java classes THEMSELVES (golden): class shape, fields, constructors, methods,
/// nested classes, and the state of every field after calling the constructor with sample arguments.
/// </summary>
internal static class TemplatesVerifier
{
    private const BindingFlags DeclaredInstance =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private const BindingFlags DeclaredAny = DeclaredInstance | BindingFlags.Static;

    public static void CheckClass(JavaClass jc, VerifyResult r)
    {
        CheckShape(jc, r);
        CheckFields(jc, r);
        CheckConstructors(jc, r);
        CheckMethods(jc, r);
        CheckInstances(jc, r);
        CheckSetters(jc, r);
    }

    // ------------------------------------------------------------------------------------------------ class shape
    public static void CheckShape(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            r.Error(jc, $"missing C# class {JavaTypeMap.ExpectedClrName(jc.Name)}");
            return;
        }

        r.Classes++;
        if (jc.Kind != "class")
        {
            r.Error(jc, $"Java kind '{jc.Kind}' is not yet supported by the test");
        }

        if (!clr.IsClass)
        {
            r.Error(jc, "must be a class (not a struct, enum or interface)");
        }

        if (clr.IsGenericType)
        {
            r.Error(jc, "must not be a generic class");
        }

        bool javaAbstract = jc.Modifiers.Contains("abstract");
        if (clr.IsAbstract != javaAbstract)
        {
            r.Error(jc, $"abstract/static mismatch: Java abstract={javaAbstract}, C# IsAbstract={clr.IsAbstract} (a static class also counts as abstract)");
        }

        bool javaFinal = jc.Modifiers.Contains("final");
        if (clr.IsSealed != javaFinal)
        {
            r.Error(jc, $"sealed mismatch: Java final={javaFinal}, C# IsSealed={clr.IsSealed}");
        }

        bool javaPublic = jc.Modifiers.Contains("public");
        bool clrPublic = clr.IsNested ? clr.IsNestedPublic : clr.IsPublic;
        if (clrPublic != javaPublic)
        {
            r.Error(jc, $"class access level mismatch: Java public={javaPublic}, C# public={clrPublic}");
        }

        if (jc.Superclass == "java.lang.Object" && clr.BaseType != typeof(object))
        {
            r.Error(jc, $"base class must be object, actual {clr.BaseType}");
        }

        if (clr.GetInterfaces().Length != jc.Interfaces.Length)
        {
            r.Error(jc, $"interface count mismatch: Java {jc.Interfaces.Length}, C# {clr.GetInterfaces().Length} (a record or an automatically added IEquatable is wrong)");
        }

        if (jc.Enclosing is null)
        {
            if (clr.DeclaringType is not null)
            {
                r.Error(jc, "Java has a top-level class but C# has a nested class");
            }

            if (clr.Namespace != JavaTypeMap.ClrNamespace)
            {
                r.Error(jc, $"namespace must be {JavaTypeMap.ClrNamespace}, actual {clr.Namespace}");
            }
        }
        else if (clr.DeclaringType != JavaTypeMap.FindClrClass(jc.Enclosing))
        {
            r.Error(jc, $"nested class must be inside {jc.Enclosing}");
        }

        string[] javaNested = jc.Nested.Select(n => n[(n.LastIndexOf('$') + 1)..]).Order(StringComparer.Ordinal).ToArray();
        string[] clrNested = clr.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Select(t => t.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (javaNested.SequenceEqual(clrNested))
        {
            r.NestedTypes += javaNested.Length;
        }
        else
        {
            r.Error(jc, $"nested classes mismatch: Java [{string.Join(", ", javaNested)}], C# [{string.Join(", ", clrNested)}]");
        }
    }

    // ------------------------------------------------------------------------------------------------ fields
    public static void CheckFields(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        var expectedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (JavaField f in jc.Fields)
        {
            if (f.IsStatic || f.IsFinal)
            {
                r.Error(jc, $"field '{f.Name}' is static/final: not yet supported by the test (needs a const/static readonly/getter-only mapping)");
                continue;
            }

            string name = JavaTypeMap.ToClrName(f.Name);
            expectedNames.Add(name);

            PropertyInfo? prop = clr.GetProperty(name, DeclaredInstance);
            if (prop is null)
            {
                r.Error(jc, $"missing property '{name}' (Java field '{f.Name}' of type {f.Type})");
                continue;
            }

            Type expectedType;
            try
            {
                expectedType = JavaTypeMap.ToClr(f.Type);
            }
            catch (MappingException e)
            {
                r.Error(jc, $"field '{f.Name}': {e.Message}");
                continue;
            }

            if (prop.PropertyType != expectedType)
            {
                r.Error(jc, $"'{name}' has the wrong type: Java {f.Type} needs {JavaTypeMap.Pretty(expectedType)}, actual {JavaTypeMap.Pretty(prop.PropertyType)}");
            }

            string expectedAccess = JavaTypeMap.ExpectedAccess(f.Modifiers);
            MethodInfo? getter = prop.GetMethod;
            MethodInfo? setter = prop.SetMethod;
            if (getter is null || JavaTypeMap.Access(getter) != expectedAccess)
            {
                r.Error(jc, $"'{name}' must have a {expectedAccess} getter");
            }

            if (setter is null)
            {
                r.Error(jc, $"'{name}' is missing a setter (the game modifies these fields at runtime)");
            }
            else
            {
                if (JavaTypeMap.Access(setter) != expectedAccess)
                {
                    r.Error(jc, $"'{name}' must have a {expectedAccess} setter, actual {JavaTypeMap.Access(setter)}");
                }

                if (setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
                {
                    r.Error(jc, $"'{name}' has an init-only setter (it must be reassignable after creation)");
                }
            }

            if (prop.GetCustomAttribute<RequiredMemberAttribute>() is not null)
            {
                r.Error(jc, $"'{name}' uses 'required' (code ported from Java assigns fields one by one after new, so this cannot be used)");
            }

            r.Fields++;
        }

        foreach (PropertyInfo p in clr.GetProperties(DeclaredAny))
        {
            if (!expectedNames.Contains(p.Name))
            {
                r.Error(jc, $"extra property '{p.Name}' has no corresponding Java field");
            }
        }

        foreach (FieldInfo fi in clr.GetFields(DeclaredAny))
        {
            if (fi.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            {
                r.Error(jc, $"field '{fi.Name}' is not a property (rule 3.5: Java fields become {{ get; set; }} properties)");
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ constructors
    public static void CheckConstructors(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        ConstructorInfo[] clrCtors = clr.GetConstructors(DeclaredInstance);
        var matched = new HashSet<ConstructorInfo>();
        foreach (JavaCtor jctor in jc.Ctors)
        {
            string sig = string.Join(", ", jctor.Parameters.Select(p => p.Type + " " + p.Name));
            Type[] expected;
            try
            {
                expected = jctor.Parameters.Select(p => JavaTypeMap.ToClr(p.Type)).ToArray();
            }
            catch (MappingException e)
            {
                r.Error(jc, $"constructor ({sig}): {e.Message}");
                continue;
            }

            ConstructorInfo? found = clrCtors.FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(expected));
            if (found is null)
            {
                r.Error(jc, $"missing C# constructor corresponding to Java ({sig})");
                continue;
            }

            matched.Add(found);
            string expectedAccess = JavaTypeMap.ExpectedAccess(jctor.Modifiers);
            if (JavaTypeMap.Access(found) != expectedAccess)
            {
                r.Error(jc, $"constructor ({sig}) must be {expectedAccess}, actual {JavaTypeMap.Access(found)}");
            }

            ParameterInfo[] ps = found.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].Name != jctor.Parameters[i].Name)
                {
                    r.Error(jc, $"constructor ({sig}): parameter {i} must be named '{jctor.Parameters[i].Name}' (rule 3.5: parameter names are kept as is), actual '{ps[i].Name}'");
                }
            }

            r.Constructors++;
        }

        foreach (ConstructorInfo c in clrCtors)
        {
            if (!matched.Contains(c))
            {
                string sig = string.Join(", ", c.GetParameters().Select(p => JavaTypeMap.Pretty(p.ParameterType) + " " + p.Name));
                r.Error(jc, $"extra C# constructor ({sig}) not present in Java");
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ methods
    public static void CheckMethods(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        // IsSpecialName excludes the property get_/set_ methods; every remaining method must exist in Java.
        MethodInfo[] clrMethods = clr.GetMethods(DeclaredAny).Where(m => !m.IsSpecialName).ToArray();
        var matched = new HashSet<MethodInfo>();
        foreach (JavaMethod jm in jc.Methods)
        {
            string name = JavaTypeMap.ToClrName(jm.Name);
            Type[] expected;
            Type expectedReturn;
            try
            {
                expected = jm.Parameters.Select(p => JavaTypeMap.ToClr(p.Type)).ToArray();
                expectedReturn = jm.ReturnType == "void" ? typeof(void) : JavaTypeMap.ToClr(jm.ReturnType);
            }
            catch (MappingException e)
            {
                r.Error(jc, $"method {jm.Name}: {e.Message}");
                continue;
            }

            MethodInfo? found = clrMethods.FirstOrDefault(m => m.Name == name
                && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(expected));
            if (found is null)
            {
                r.Error(jc, $"missing C# method '{name}' ({string.Join(", ", expected.Select(JavaTypeMap.Pretty))})");
                continue;
            }

            matched.Add(found);
            if (found.ReturnType != expectedReturn)
            {
                r.Error(jc, $"method '{name}' has the wrong return type: needs {JavaTypeMap.Pretty(expectedReturn)}, actual {JavaTypeMap.Pretty(found.ReturnType)}");
            }

            if (found.IsStatic != jm.IsStatic)
            {
                r.Error(jc, $"method '{name}': static mismatch (Java {jm.IsStatic})");
            }

            r.Methods++;
        }

        foreach (MethodInfo m in clrMethods)
        {
            if (!matched.Contains(m))
            {
                r.Error(jc, $"extra C# method '{m.Name}' not present in Java (this class must be a plain POJO with no logic)");
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ sample instantiation
    public static void CheckInstances(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        Dictionary<string, JavaField> fieldByName = jc.InstanceFields.ToDictionary(f => f.Name, StringComparer.Ordinal);
        foreach (JavaInstance inst in jc.Instances)
        {
            string label = $"{inst.Sample} ({string.Join(", ", inst.CtorTypes)})";
            try
            {
                object? obj = CreateFromInstance(clr, inst, out string? problem);
                if (obj is null)
                {
                    r.Error(jc, $"sample instantiation {label}: {problem}");
                    continue;
                }

                int stateCount = 0;
                foreach (JsonProperty p in inst.State.EnumerateObject())
                {
                    stateCount++;
                    if (!fieldByName.TryGetValue(p.Name, out JavaField? jf))
                    {
                        r.Error(jc, $"sample instantiation {label}: golden has an unknown field '{p.Name}'");
                        continue;
                    }

                    PropertyInfo? prop = obj.GetType().GetProperty(JavaTypeMap.ToClrName(jf.Name), DeclaredInstance);
                    if (prop is null)
                    {
                        r.Error(jc, $"sample instantiation {label}: missing property for field '{jf.Name}'");
                        continue;
                    }

                    NullabilityState nullability = new NullabilityInfoContext().Create(prop).ReadState;
                    CompareValue(jc, r, $"sample instantiation {label} .{prop.Name}", prop.GetValue(obj), p.Value, jf.Type, nullability);
                }

                if (stateCount != fieldByName.Count)
                {
                    r.Error(jc, $"sample instantiation {label}: golden has {stateCount} fields, the Java class has {fieldByName.Count}");
                }

                r.Instances++;
            }
            catch (Exception e)
            {
                r.Error(jc, $"sample instantiation {label}: error while running ({e.GetType().Name}: {e.Message})");
            }
        }
    }

    /// <summary>
    /// Primitive, bool and string fields: assigning a new value through the setter and reading it back must give exactly that value, and the other properties
    /// must not change (this catches empty setters or setters that assign the wrong field). Arrays/lists/objects are already checked in CheckInstances.
    /// </summary>
    public static void CheckSetters(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        object? obj = CreateFromInstance(clr, jc.Instances[0], out string? problem);
        if (obj is null)
        {
            r.Error(jc, $"setter check: could not build the object ({problem})");
            return;
        }

        var props = new List<PropertyInfo>();
        foreach (JavaField f in jc.InstanceFields)
        {
            PropertyInfo? p = clr.GetProperty(JavaTypeMap.ToClrName(f.Name), DeclaredInstance);
            if (p is null)
            {
                r.Error(jc, $"setter check: missing property '{JavaTypeMap.ToClrName(f.Name)}' (Java field '{f.Name}')");
            }
            else
            {
                props.Add(p);
            }
        }

        foreach (PropertyInfo target in props)
        {
            object? current = target.GetValue(obj);
            object? probe = ProbeValue(target.PropertyType, current);
            if (probe is null)
            {
                if (IsSimple(target.PropertyType))
                {
                    r.Error(jc, $"setter check '{target.Name}': the current value {Show(current)} cannot be flipped (simple types must be flippable)");
                }
                else
                {
                    r.SettersSkipped++;
                }

                continue;
            }

            object?[] before = props.Select(p => p.GetValue(obj)).ToArray();
            target.SetValue(obj, probe);
            for (int i = 0; i < props.Count; i++)
            {
                object? after = props[i].GetValue(obj);
                object? expected = props[i] == target ? probe : before[i];
                if (!Equals(after, expected))
                {
                    r.Error(jc, $"assigning '{target.Name}' = {Show(probe)}: '{props[i].Name}' expected {Show(expected)}, actual {Show(after)}");
                }
            }

            r.Setters++;
        }
    }

    private static bool IsSimple(Type type)
    {
        return type == typeof(bool) || type == typeof(sbyte) || type == typeof(short) || type == typeof(int)
            || type == typeof(long) || type == typeof(string);
    }

    /// <summary>A value that is guaranteed to differ from the current value (flipped bit / flipped bool / appended suffix); null for complex types.</summary>
    private static object? ProbeValue(Type type, object? current)
    {
        if (current is null)
        {
            return null;
        }

        if (type == typeof(bool))
        {
            return !(bool)current;
        }

        if (type == typeof(sbyte))
        {
            return unchecked((sbyte)~(sbyte)current);
        }

        if (type == typeof(short))
        {
            return unchecked((short)~(short)current);
        }

        if (type == typeof(int))
        {
            return ~(int)current;
        }

        if (type == typeof(long))
        {
            return ~(long)current;
        }

        return type == typeof(string) ? (string)current + "!" : null;
    }

    /// <summary>Builds the C# object from one sample instantiation in golden (choosing the constructor by the mapped Java parameter types).</summary>
    private static object? CreateFromInstance(Type clr, JavaInstance inst, out string? problem)
    {
        problem = null;
        try
        {
            Type[] paramTypes = inst.CtorTypes.Select(t => JavaTypeMap.ToClr(t)).ToArray();
            ConstructorInfo? ctor = clr.GetConstructors(DeclaredInstance)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramTypes));
            if (ctor is null)
            {
                problem = "no corresponding C# constructor";
                return null;
            }

            JsonElement[] argJson = inst.Args.EnumerateArray().ToArray();
            if (argJson.Length != paramTypes.Length)
            {
                problem = $"golden has {argJson.Length} arguments, the constructor needs {paramTypes.Length}";
                return null;
            }

            var args = new object?[paramTypes.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Build(paramTypes[i], argJson[i]);
            }

            return ctor.Invoke(args);
        }
        catch (Exception e)
        {
            problem = $"error while running ({e.GetType().Name}: {e.Message})";
            return null;
        }
    }

    /// <summary>Every reference-type property must be declared non-null (the null decision of A-04).</summary>
    public static void CheckNullability(JavaClass jc, VerifyResult r)
    {
        Type? clr = JavaTypeMap.FindClrClass(jc.Name);
        if (clr is null)
        {
            return;
        }

        var context = new NullabilityInfoContext();
        foreach (PropertyInfo p in clr.GetProperties(DeclaredInstance))
        {
            if (p.PropertyType.IsValueType)
            {
                continue;
            }

            NullabilityInfo info = context.Create(p);
            if (info.ReadState != NullabilityState.NotNull || info.WriteState != NullabilityState.NotNull)
            {
                r.Error(jc, $"'{p.Name}' of reference type must be declared non-null (read={info.ReadState}, write={info.WriteState})");
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ building arguments from golden
    private static object? Build(Type clrType, JsonElement json)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.String:
                return json.GetString();
            case JsonValueKind.Number:
                // GetSByte/GetInt16... throw if the value is outside the type's range: this also catches a mismatch between golden and the C# type.
                if (clrType == typeof(sbyte))
                {
                    return json.GetSByte();
                }

                if (clrType == typeof(short))
                {
                    return json.GetInt16();
                }

                if (clrType == typeof(int))
                {
                    return json.GetInt32();
                }

                if (clrType == typeof(long))
                {
                    return json.GetInt64();
                }

                throw new MappingException($"cannot build a number for type {JavaTypeMap.Pretty(clrType)}");
            case JsonValueKind.Array:
                return BuildCollection(clrType, json.EnumerateArray().ToArray());
            case JsonValueKind.Object:
                return BuildObject(json);
            default:
                throw new MappingException($"unsupported golden value: {json.ValueKind}");
        }
    }

    private static object BuildCollection(Type clrType, JsonElement[] items)
    {
        if (clrType.IsArray)
        {
            Type elem = clrType.GetElementType() ?? throw new MappingException("array has no element type");
            Array arr = Array.CreateInstance(elem, items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                arr.SetValue(Build(elem, items[i]), i);
            }

            return arr;
        }

        if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type elem = clrType.GetGenericArguments()[0];
            var list = (IList)(Activator.CreateInstance(clrType) ?? throw new MappingException("cannot create the list"));
            foreach (JsonElement item in items)
            {
                list.Add(Build(elem, item));
            }

            return list;
        }

        throw new MappingException($"cannot build a collection for type {JavaTypeMap.Pretty(clrType)}");
    }

    private static object BuildObject(JsonElement json)
    {
        string className = json.GetProperty("new").GetString() ?? throw new MappingException("missing class name in the build recipe");
        Type type = JavaTypeMap.FindClrClass(className) ?? throw new MappingException($"missing C# class for {className}");
        Type[] paramTypes = json.GetProperty("ctor").EnumerateArray()
            .Select(x => JavaTypeMap.ToClr(x.GetString() ?? "")).ToArray();
        ConstructorInfo ctor = type.GetConstructor(DeclaredInstance, binder: null, types: paramTypes, modifiers: null)
            ?? throw new MappingException($"missing C# constructor for {className}");
        JsonElement[] argJson = json.GetProperty("args").EnumerateArray().ToArray();
        var args = new object?[paramTypes.Length];
        for (int i = 0; i < args.Length; i++)
        {
            args[i] = Build(paramTypes[i], argJson[i]);
        }

        return ctor.Invoke(args);
    }

    // ------------------------------------------------------------------------------------------------ value comparison
    private static void CompareValue(JavaClass jc, VerifyResult r, string path, object? actual, JsonElement expected, string javaType, NullabilityState nullability)
    {
        switch (expected.ValueKind)
        {
            case JsonValueKind.Null:
                CompareNull(jc, r, path, actual, nullability);
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (actual is bool b && b == expected.GetBoolean())
                {
                    r.Values++;
                }
                else
                {
                    r.Error(jc, $"{path}: expected bool {expected.GetBoolean()}, actual {Show(actual)}");
                }

                break;
            case JsonValueKind.Number:
                CompareNumber(jc, r, path, actual, expected, javaType);
                break;
            case JsonValueKind.String:
                if (actual is string s && string.Equals(s, expected.GetString(), StringComparison.Ordinal))
                {
                    r.Values++;
                }
                else
                {
                    r.Error(jc, $"{path}: expected string \"{expected.GetString()}\", actual {Show(actual)}");
                }

                break;
            case JsonValueKind.Array:
                CompareCollection(jc, r, path, actual, expected, javaType);
                break;
            case JsonValueKind.Object:
                CompareObject(jc, r, path, actual, expected);
                break;
            default:
                r.Error(jc, $"{path}: unsupported golden value kind {expected.ValueKind}");
                break;
        }
    }

    private static void CompareNull(JavaClass jc, VerifyResult r, string path, object? actual, NullabilityState nullability)
    {
        // Java leaves it null. Decision of A-04: non-null properties use a neutral value (empty string, empty array);
        // properties declared nullable must be exactly null.
        if (nullability == NullabilityState.Nullable)
        {
            if (actual is null)
            {
                r.Values++;
                r.NullDefaults++;
            }
            else
            {
                r.Error(jc, $"{path}: Java null, property declared nullable but actual {Show(actual)}");
            }

            return;
        }

        if ((actual is string s && s.Length == 0) || (actual is Array a && a.Length == 0))
        {
            r.Values++;
            r.NullDefaults++;
        }
        else
        {
            r.Error(jc, $"{path}: Java null, a non-null property must have a neutral value (empty string or empty array), actual {Show(actual)}");
        }
    }

    private static void CompareNumber(JavaClass jc, VerifyResult r, string path, object? actual, JsonElement expected, string javaType)
    {
        Type clrType;
        try
        {
            clrType = JavaTypeMap.ToClr(javaType, genericArgument: true);
        }
        catch (MappingException e)
        {
            r.Error(jc, $"{path}: {e.Message}");
            return;
        }

        if (actual is null || actual.GetType() != clrType)
        {
            r.Error(jc, $"{path}: expected type {JavaTypeMap.Pretty(clrType)}, actual {Show(actual)}");
        }
        else if (Convert.ToInt64(actual, CultureInfo.InvariantCulture) != expected.GetInt64())
        {
            r.Error(jc, $"{path}: expected {expected.GetInt64()}, actual {Show(actual)}");
        }
        else
        {
            r.Values++;
        }
    }

    private static void CompareCollection(JavaClass jc, VerifyResult r, string path, object? actual, JsonElement expected, string javaType)
    {
        string elementType;
        if (javaType.EndsWith("[]", StringComparison.Ordinal))
        {
            elementType = javaType[..^2];
        }
        else if (javaType.StartsWith("java.util.List<", StringComparison.Ordinal) && javaType.EndsWith('>'))
        {
            elementType = javaType["java.util.List<".Length..^1];
        }
        else
        {
            r.Error(jc, $"{path}: golden is an array/list but the Java type is {javaType}");
            return;
        }

        if (actual is not IList list)
        {
            r.Error(jc, $"{path}: expected an array/list, actual {Show(actual)}");
            return;
        }

        JsonElement[] items = expected.EnumerateArray().ToArray();
        if (list.Count != items.Length)
        {
            r.Error(jc, $"{path}: expected {items.Length} elements, actual {list.Count}");
            return;
        }

        r.Values++;
        for (int i = 0; i < items.Length; i++)
        {
            CompareValue(jc, r, $"{path}[{i}]", list[i], items[i], elementType, NullabilityState.NotNull);
        }
    }

    private static void CompareObject(JavaClass jc, VerifyResult r, string path, object? actual, JsonElement expected)
    {
        string className = expected.GetProperty("class").GetString() ?? "";
        Type? clrType = JavaTypeMap.FindClrClass(className);
        if (actual is null || clrType is null || actual.GetType() != clrType)
        {
            r.Error(jc, $"{path}: expected object {className}, actual {Show(actual)}");
            return;
        }

        JavaClass nested = JavaGolden.Instance.ByName[className];
        foreach (JsonProperty fp in expected.GetProperty("fields").EnumerateObject())
        {
            JavaField? jf = nested.Fields.FirstOrDefault(f => f.Name == fp.Name);
            PropertyInfo? prop = clrType.GetProperty(JavaTypeMap.ToClrName(fp.Name), DeclaredInstance);
            if (jf is null || prop is null)
            {
                r.Error(jc, $"{path}: missing field/property '{fp.Name}' of {className}");
                continue;
            }

            CompareValue(jc, r, $"{path}.{prop.Name}", prop.GetValue(actual), fp.Value, jf.Type, NullabilityState.NotNull);
        }
    }

    private static string Show(object? value)
    {
        return value is null ? "null" : $"{JavaTypeMap.Pretty(value.GetType())} {value}";
    }
}
