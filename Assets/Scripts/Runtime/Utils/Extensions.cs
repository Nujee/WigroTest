using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Wigro.Runtime
{
    public static class Extensions
    {
        public static async Task LinearMoveTo(this Transform transform, Vector3 endPoint, float duration)
        {
            var startPoint = transform.position;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPoint, endPoint, t);
                elapsed += Time.deltaTime;

                await Task.Yield();
            }

            transform.position = endPoint;
        }

        public static TKey GetKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TValue value)
        {
            foreach (var pair in dictionary)
            {
                if (EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                {
                    return pair.Key;
                }
            }
            return default;
        }
    }
}