using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Tests.Unit.Core.Utilities;

/// <summary>
/// Audit A-05 finding F1: <c>Util.addArray</c> keeps the run-time element type of the first array (commons-lang <c>ArrayUtils.addAll</c>),
/// because callers cast the result, for example <c>(int[][]) Util.addArray(a, b)</c> in the boss data classes.
/// </summary>
public class UtilAddArrayTypeTests
{
    [Fact]
    public void The_result_keeps_the_element_type_of_the_first_array()
    {
        int[][] first = [[1, 2], [3]];
        int[][] second = [[4]];

        object?[]? joined = Util.AddArray(first, second);

        int[][] cast = Assert.IsType<int[][]>(joined);
        Assert.Equal(3, cast.Length);
        Assert.Equal(new[] { 4 }, cast[2]);
    }

    [Fact]
    public void An_element_that_does_not_fit_the_first_array_type_fails_like_array_store_exception()
    {
        string[] first = ["a"];
        object[] second = [1];

        Assert.Throws<InvalidCastException>(() => Util.AddArray(first, second));
    }

    [Fact]
    public void Null_arrays_are_treated_as_empty()
    {
        int[][] second = [[9]];

        Assert.Equal(new[] { 9 }, Assert.IsType<int[][]>(Util.AddArray(null, second))[0]);
        Assert.Null(Util.AddArray(null, null));
    }
}
