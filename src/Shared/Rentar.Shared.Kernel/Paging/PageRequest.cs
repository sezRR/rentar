namespace Rentar.Shared.Kernel.Paging;

/// <summary>
/// Which part of a list to answer: the first <see cref="Size"/> items whose key comes after <see cref="After"/>, or
/// the first items of the list without one. Lists are ordered by an increasing key, so continuing after the last key
/// of a page neither repeats nor skips an item, even when items are added or removed in between.
/// </summary>
public sealed record PageRequest(int? After = null, int Size = PageRequest.DefaultSize)
{
    public const int DefaultSize = 25;
    public const int MaxSize = 100;
}
