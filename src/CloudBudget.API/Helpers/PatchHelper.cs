using System.Reflection;

namespace CloudBudget.API.Helpers;

public static class PatchHelper
{
    public static void ApplyNonNullProperties<TSource, TTarget>(TSource source, TTarget target)
    {
        var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var tgtType = typeof(TTarget);

        foreach (var sp in srcProps)
        {
            var value = sp.GetValue(source);

            if (value == null)
            {
                continue; // skip non-provided properties
            }

            var tp = tgtType.GetProperty(sp.Name, BindingFlags.Public | BindingFlags.Instance);

            if (tp == null || !tp.CanWrite)
            {
                continue;
            }

            // Considerare conversione tipi se necessario (es. nullable -> non-nullable)
            tp.SetValue(target, value);
        }
    }
}