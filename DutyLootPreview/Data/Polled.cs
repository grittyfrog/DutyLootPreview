using System;
using System.Collections.Generic;

namespace DutyLootPreview.Data;

public static class Polled {
    public static Polled<T> Of<T>(Func<T> read) => new(read);
}

public sealed class Polled<T>(Func<T> read) {
    private T last = default!;
    private bool hasValue;

    public (T value, bool changed) Poll() {
        var next = read();
        var changed = !hasValue || !EqualityComparer<T>.Default.Equals(last, next);
        last = next;
        hasValue = true;
        return (next, changed);
    }
}
