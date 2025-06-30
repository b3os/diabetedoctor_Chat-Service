namespace ChatService.Contract.Helpers;

public static class EnumHelper
{
    /// <summary>
    /// Map tự động giữa 2 enum cùng tên value.
    /// </summary>
    public static TTarget ToEnum<TSource, TTarget>(this TSource source)
        where TSource : Enum
        where TTarget : Enum
    {
        var sourceName = source.ToString();

        if (Enum.TryParse(typeof(TTarget), sourceName, out var result))
            return (TTarget)result!;

        throw new ArgumentException($"Không thể map enum {typeof(TSource).Name}.{sourceName} sang {typeof(TTarget).Name}");
    }

    /// <summary>
    /// Map tự động giữa 2 enum nullable cùng tên value.
    /// Trả về null nếu source là null.
    /// </summary>
    public static TTarget? ToEnumNullable<TSource, TTarget>(this TSource? source)
        where TSource : struct, Enum
        where TTarget : struct, Enum
    {
        if (source == null) return null;

        var sourceName = source.Value.ToString();

        if (Enum.TryParse<TTarget>(sourceName, out var result))
            return result;

        throw new ArgumentException($"Không thể map enum {typeof(TSource).Name}.{sourceName} sang {typeof(TTarget).Name}");
    }
}