using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Transform sprite;
    [SerializeField] private AnimationCurve scaleCurve;

    public void UpdateScale(float ratio)
    {
        float curvedRatio;

        if (ratio <= 0.5f)
            curvedRatio = scaleCurve.Evaluate(ratio * 2f);
        else
            curvedRatio = scaleCurve.Evaluate((1f - ratio) * 2f);

        sprite.localScale = Vector3.one * curvedRatio;
    }
}
