using UnityEngine;
using static UnityEngine.Mathf;

public static class WaveFunctions {
    public static Vector3 SinWave(float u, float v, float t) {
        float w = Sin(PI * (u + v + t));
        return new Vector3(u, w, v);
    }

    public static Vector3 MultiWave(float u, float v, float t) {
        float w = (Sin(PI * (u + t)) + Sin(2 * PI * (v + t))) / 2f;
        return new Vector3(u, w, v);
    }
    
    public static Vector3 Ripple(float u, float v, float t) {
        float d = Sqrt(u * u + v * v);
        float w = Sin(PI * (4 * d - t)) / (10 * d + 1);
        return new Vector3(u, w, v);
    }

    public static Vector3 Sphere(float u, float v, float t) {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 point = new Vector3(s * Sin(PI * u), r * Sin(PI * 0.5f * v), s * Cos(PI * u));
        return point;
    }

    public static Vector3 Torus(float u, float v, float t) {
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 point = new Vector3(s * Sin(PI * u), r2 * Sin(PI * v), s * Cos(PI * u));
        return point;
    }
}
