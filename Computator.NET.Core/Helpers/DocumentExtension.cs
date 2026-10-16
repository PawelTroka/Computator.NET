using System.Collections.Generic;

namespace Computator.NET.Core.Helpers
{
    public static class DocumentExtension
    {
        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            TKey fromKey, TKey toKey)
        {
            if (EqualityComparer<TKey>.Default.Equals(fromKey, toKey)) return;
            var value = dic[fromKey];
            // Add first: a colliding target must not replace either document.
            dic.Add(toKey, value);
            dic.Remove(fromKey);
        }
    }
}
