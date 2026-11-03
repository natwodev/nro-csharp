namespace Nro.Data.Entities;

/// <summary>
/// Marks a text column that holds JSON or a nested array. The value is stored and read as the exact string: every character is preserved
/// (rule D8 of the port plan), so the entity layer never parses or reformats it. Metadata only, no behaviour.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class JsonTextAttribute : Attribute
{
}
