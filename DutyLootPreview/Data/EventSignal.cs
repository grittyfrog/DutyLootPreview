using System;

namespace DutyLootPreview.Data;

public sealed class EventSignal : IDisposable {
    private uint generation;
    private Action? unsubscribe;

    public static EventSignal From<TArgs>(
        Action<Action<TArgs>> subscribe,
        Action<Action<TArgs>> unsubscribe,
        Func<TArgs, bool> predicate) {
        var signal = new EventSignal();
        Action<TArgs> handler = args => { if (predicate(args)) signal.generation++; };
        subscribe(handler);
        signal.unsubscribe = () => unsubscribe(handler);
        return signal;
    }

    public Watcher Watch() => new(this);

    public void Dispose() => unsubscribe?.Invoke();

    public sealed class Watcher {
        private readonly EventSignal signal;
        private uint seen;
        private bool started;

        internal Watcher(EventSignal signal) => this.signal = signal;

        public bool Fired() {
            if (started && seen == signal.generation) return false;
            seen = signal.generation;
            started = true;
            return true;
        }
    }
}
