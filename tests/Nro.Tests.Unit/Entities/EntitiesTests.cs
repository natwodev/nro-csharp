using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;
using Nro.Data.Entities;

namespace Nro.Tests.Unit.Entities;

/// <summary>
/// Checks the generated entities (<c>tools/entities/gen_entities.py</c>) against <c>tests/golden/entities.json</c>: one class per table, the right table
/// and column names, types, nullability, lengths, keys, JSON markers and relationships. Both sides come from one source, so these tests catch
/// a hand-edited generated class or a forgotten re-run of the generator.
/// </summary>
public class EntitiesTests
{
    private sealed record GoldenForeignKey(string Navigation, string Target, string TargetTable, bool Optional);

    private sealed record GoldenColumn(
        bool ExplicitName, string Property, string Column, string CsharpType, bool Nullable, bool Json, int? MaxLength, string? SqlTypeName,
        bool PrimaryKey, bool Identity, GoldenForeignKey? ForeignKey);

    private sealed record GoldenCollection(string Name, string Element, string ViaTable, string ViaColumn);

    private sealed record GoldenEntity(
        string Table, string Entity, string Folder, bool ExplicitTable, string[] PrimaryKey, GoldenColumn[] Columns, GoldenCollection[] Collections);

    private sealed record GoldenFile(string Namespace, GoldenEntity[] Entities);

    private static readonly Lazy<GoldenFile> Golden = new(() =>
        JsonSerializer.Deserialize<GoldenFile>(
            File.ReadAllText(TestPaths.Golden("entities.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("entities.json is empty"));

    public static TheoryData<string> Tables()
    {
        var data = new TheoryData<string>();
        foreach (GoldenEntity e in Golden.Value.Entities)
        {
            data.Add(e.Table);
        }

        return data;
    }

    private static Type TypeOf(GoldenEntity e) =>
        typeof(JsonTextAttribute).Assembly.GetType($"{Golden.Value.Namespace}.{e.Folder}.{e.Entity}")
        ?? throw new InvalidOperationException($"No class {e.Entity}");

    /// <summary>The naming convention the data layer applies: PascalCase to lower snake_case (the same rule as the generator).</summary>
    private static string Snake(string name)
    {
        string s = System.Text.RegularExpressions.Regex.Replace(name, "(?<=[a-z0-9])(?=[A-Z])", "_");
        s = System.Text.RegularExpressions.Regex.Replace(s, "(?<=[A-Z])(?=[A-Z][a-z])", "_");
        return System.Text.RegularExpressions.Regex.Replace(s, "_+", "_").ToLowerInvariant();
    }

    /// <summary>A scalar property maps to a column; navigation properties and collections do not.</summary>
    private static bool IsScalar(PropertyInfo p) =>
        !p.PropertyType.Namespace!.StartsWith("Nro.Data.Entities", StringComparison.Ordinal) && !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>));

    private static string CSharpName(Type t, NullabilityInfo info)
    {
        Type core = Nullable.GetUnderlyingType(t) ?? t;
        string name = core == typeof(int) ? "int"
            : core == typeof(long) ? "long"
            : core == typeof(short) ? "short"
            : core == typeof(bool) ? "bool"
            : core == typeof(double) ? "double"
            : core == typeof(decimal) ? "decimal"
            : core == typeof(string) ? "string"
            : core.Name;
        bool nullable = Nullable.GetUnderlyingType(t) is not null || (t == typeof(string) && info.WriteState == NullabilityState.Nullable);
        return nullable ? name + "?" : name;
    }

    [Fact]
    public void There_is_one_entity_per_table_and_every_column_is_covered()
    {
        GoldenEntity[] all = Golden.Value.Entities;
        Assert.Equal(66, all.Length);
        Assert.Equal(596, all.Sum(e => e.Columns.Length));
        Assert.Equal(all.Length, all.Select(e => e.Entity).Distinct().Count());
        Assert.Equal(all.Length, all.Select(e => e.Table).Distinct().Count());
    }

    [Fact]
    public void Every_entity_class_in_the_assembly_is_in_the_golden_and_lives_in_the_entities_namespace()
    {
        var generated = typeof(JsonTextAttribute).Assembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith(Golden.Value.Namespace + ".", StringComparison.Ordinal) && t.IsClass && t.IsPublic && !typeof(System.Attribute).IsAssignableFrom(t))
            .Select(t => t.Name)
            .Order(StringComparer.Ordinal);
        Assert.Equal(Golden.Value.Entities.Select(e => e.Entity).Order(StringComparer.Ordinal), generated);
    }

    [Fact]
    public void Static_data_entities_end_with_Row_so_they_never_clash_with_the_core_templates()
    {
        foreach (GoldenEntity e in Golden.Value.Entities)
        {
            Assert.Equal(e.Folder == "StaticData", e.Entity.EndsWith("Row", StringComparison.Ordinal));
        }

        var coreNames = typeof(Nro.Core.Templates.ItemTemplate).Assembly.GetTypes()
            .Where(t => t.Namespace == "Nro.Core.Templates").Select(t => t.Name).ToHashSet();
        foreach (GoldenEntity e in Golden.Value.Entities)
        {
            Assert.DoesNotContain(e.Entity, coreNames);
        }
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public void Table_columns_types_lengths_and_keys_match_the_golden(string table)
    {
        GoldenEntity e = Golden.Value.Entities.Single(x => x.Table == table);
        Type type = TypeOf(e);
        var nullability = new NullabilityInfoContext();

        TableAttribute? tableAttribute = type.GetCustomAttribute<TableAttribute>();
        Assert.Equal(e.ExplicitTable, tableAttribute is not null);
        Assert.Equal(table, tableAttribute?.Name ?? Snake(type.Name));

        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(IsScalar)
            .ToArray();
        Assert.Equal(e.Columns.Length, props.Length);

        foreach (GoldenColumn c in e.Columns)
        {
            string where = $"{e.Entity}.{c.Property}";
            PropertyInfo p = type.GetProperty(c.Property) ?? throw new InvalidOperationException($"{where} does not exist");
            ColumnAttribute? col = p.GetCustomAttribute<ColumnAttribute>();

            Assert.True(c.ExplicitName == (col?.Name is not null), $"{where}: [Column(name)] presence differs");
            Assert.True(c.Column == (col?.Name ?? Snake(p.Name)), $"{where}: column name differs");
            Assert.True(c.Column == c.Column.ToLowerInvariant() && !c.Column.Contains("__"), $"{where}: column is not lower-case snake_case");
            Assert.True(c.CsharpType == CSharpName(p.PropertyType, nullability.Create(p)), $"{where}: type differs");
            Assert.True(c.SqlTypeName == col?.TypeName, $"{where}: SQL type name differs");
            Assert.True(c.MaxLength == p.GetCustomAttribute<MaxLengthAttribute>()?.Length, $"{where}: max length differs");
            Assert.True(c.Json == (p.GetCustomAttribute<JsonTextAttribute>() is not null), $"{where}: JSON marker differs");
            Assert.True(c.PrimaryKey == (p.GetCustomAttribute<KeyAttribute>() is not null), $"{where}: key marker differs");
            Assert.True(c.CsharpType == "string" == (p.GetCustomAttribute<RequiredAttribute>() is not null), $"{where}: [Required] differs");

            DatabaseGeneratedAttribute? generated = p.GetCustomAttribute<DatabaseGeneratedAttribute>();
            if (c.PrimaryKey)
            {
                Assert.Equal(c.Identity ? DatabaseGeneratedOption.Identity : DatabaseGeneratedOption.None, generated?.DatabaseGeneratedOption);
            }
            else
            {
                Assert.Null(generated);
            }
        }

        Assert.Equal(
            e.PrimaryKey.Order(StringComparer.Ordinal),
            props.Where(p => p.GetCustomAttribute<KeyAttribute>() is not null)
                 .Select(p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? Snake(p.Name)).Order(StringComparer.Ordinal));

        if (e.PrimaryKey.Length > 1)
        {
            // A composite key needs an explicit order on every key column.
            int[] orders = props.Where(p => p.GetCustomAttribute<KeyAttribute>() is not null)
                .Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Order).Order().ToArray();
            Assert.Equal(Enumerable.Range(0, e.PrimaryKey.Length), orders);
        }
    }

    [Fact]
    public void Relationships_have_a_foreign_key_attribute_a_navigation_property_and_a_back_collection()
    {
        int relationships = 0;
        foreach (GoldenEntity e in Golden.Value.Entities)
        {
            Type type = TypeOf(e);
            foreach (GoldenColumn c in e.Columns.Where(c => c.ForeignKey is not null))
            {
                GoldenForeignKey fk = c.ForeignKey!;
                PropertyInfo scalar = type.GetProperty(c.Property)!;
                Assert.Equal(fk.Navigation, scalar.GetCustomAttribute<ForeignKeyAttribute>()!.Name);

                PropertyInfo nav = type.GetProperty(fk.Navigation) ?? throw new InvalidOperationException($"{e.Entity}.{fk.Navigation} is missing");
                Assert.Equal(fk.Target, nav.PropertyType.Name);
                Assert.Equal(fk.Optional, c.Nullable);

                GoldenEntity parent = Golden.Value.Entities.Single(x => x.Entity == fk.Target);
                Assert.Contains(parent.Collections, col => col.Element == e.Entity && col.ViaColumn == c.Column);
                relationships++;
            }

            foreach (GoldenCollection col in e.Collections)
            {
                PropertyInfo p = type.GetProperty(col.Name) ?? throw new InvalidOperationException($"{e.Entity}.{col.Name} is missing");
                Assert.Equal($"ICollection`1", p.PropertyType.Name);
                Assert.Equal(col.Element, p.PropertyType.GetGenericArguments()[0].Name);
                Assert.NotNull(p.GetValue(Activator.CreateInstance(type)));
            }
        }

        Assert.Equal(15, relationships);
    }

    [Fact]
    public void Account_is_keyed_by_id_alone_and_giftcode_save_id_is_the_account_id()
    {
        GoldenEntity account = Golden.Value.Entities.Single(x => x.Table == "account");
        Assert.Equal(["id"], account.PrimaryKey);

        GoldenEntity redemption = Golden.Value.Entities.Single(x => x.Table == "giftcode_save");
        Assert.Empty(redemption.PrimaryKey);
        Assert.Contains(redemption.Columns, c => c.Property == "AccountId" && c.Column == "id");
    }

    [Fact]
    public void Skill_template_has_a_composite_key_ordered_class_then_id()
    {
        GoldenEntity skill = Golden.Value.Entities.Single(x => x.Table == "skill_template");
        Assert.Equal(["nclass_id", "id"], skill.PrimaryKey);

        PropertyInfo[] keys = TypeOf(skill).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<ColumnAttribute>()!.Order).ToArray();
        Assert.Equal(["nclass_id", "id"], keys.Select(p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? Snake(p.Name)));
    }
}
