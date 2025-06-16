using System.Collections.Generic;
using RepoXR.Managers;
using UnityEngine;

namespace RepoXR.Data;

public class AnimationCurveData : ScriptableObject
{
    private static Dictionary<AnimationCurveData, float> curveTimers = [];
    
    public AnimationCurve curve;

    public float Evaluate(float value)
    {
        return curve.Evaluate(value);
    }
    
    /// <summary>
    /// Evaluates the curve based on a timer, optionally with a modified speed
    /// </summary>
    public float EvaluateTimed(float speed = 1, float length = 1)
    {
        if (!curveTimers.TryGetValue(this, out var currentTime))
        {
            curveTimers[this] = 0;
            currentTime = 0;
        }

        currentTime += Time.deltaTime * speed;
        currentTime = Mathf.Repeat(currentTime, length);

        curveTimers[this] = currentTime;

        return Evaluate(currentTime);
    }

    public void Pulse(HapticManager.Hand hand, float amplitude = 0.1f, float speed = 1, int priority = 1)
    {
        HapticManager.PulseCurve(hand, this, amplitude, speed, priority);
    }
}