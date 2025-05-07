using System.Threading.Tasks;
using UnityEngine;

public static class CustomTweens
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
}