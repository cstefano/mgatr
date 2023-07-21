using System.Diagnostics;
using DbUp.Builder;

namespace Migratonator;

internal static class Extensions
{
    public static void EachWithIndex<T>(this IEnumerable<T> ie, Action<T, int> action)
    {
        var i = 0;
        foreach (var e in ie) action(e, i++);
    }
}