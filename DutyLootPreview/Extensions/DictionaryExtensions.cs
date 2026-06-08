using System;
using System.Collections.Generic;

namespace DutyLootPreview.Extensions;

public static class DictionaryExtensions {
    extension<TKey, TValue>(IDictionary<TKey, TValue> self) {

        public TValue GetOrAdd(TKey key, Func<TValue> valueProvider) { return self.GetOrAdd(key, (dict, key) => valueProvider()); }
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueProvider) { return self.GetOrAdd(key, (dict, key) => valueProvider(key)); }
        
        public TValue GetOrAdd(TKey key, Func<IDictionary<TKey, TValue>, TKey, TValue> valueProvider) {
            if (self.TryGetValue(key, out var foundValue)) {
                return foundValue;
            }

            self[key] = valueProvider(self, key);
            return self[key];
        }
    }
}
