using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationRate;
    [SerializeField] private float drag;
    [SerializeField] private float counterAccelerationFactor;

    private Vector2 velocity;

    [Header("Sprite")]
    [SerializeField] private Transform sprite;
    [SerializeField] private float minWidth;
    [SerializeField] private float maxWidth;
    [SerializeField] private AnimationCurve scaleCurve;

    [Header("Collision Handling")]
    [SerializeField] private float movementIncrement;

    void Update()
    {
        UpdateVelocity();
        UpdateSprite();
        HandleCollisions();
    }

    private void UpdateVelocity()
    {
        Vector2 inputDir;
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.y = Input.GetAxisRaw("Vertical");
        inputDir.Normalize();

        float angleDeltaRatio = Vector3.Angle(velocity, inputDir) / 180f;
        float counterAccelerationModifier = angleDeltaRatio * counterAccelerationFactor;

        Vector2 acceleration = inputDir * accelerationRate * (1 + counterAccelerationModifier) * Time.deltaTime;

        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        if (inputDir == Vector2.zero)
        {
            if (velocity.magnitude > drag * Time.deltaTime)
                velocity -= velocity.normalized * drag * Time.deltaTime;
            else
                velocity = Vector2.zero;
        }
    }

    private void UpdateSprite()
    {
        sprite.up = velocity;

        float curvedRatio = scaleCurve.Evaluate(Mathf.InverseLerp(0f, maxSpeed, velocity.magnitude));

        Vector3 scale = sprite.localScale;
        scale.x = Mathf.Lerp(minWidth, maxWidth, 1 - curvedRatio);
        sprite.localScale = scale;
    }

    private void HandleCollisions()
    {
        Vector3 velocityThisFrame = velocity;
        Vector3 microVelocity;
        Vector3 positionNextFrame = transform.position;

        bool velXPositive = velocityThisFrame.x > 0;
        bool velYPositive = velocityThisFrame.y > 0;

        while (velocityThisFrame != Vector3.zero)
        {
            if (Mathf.Abs(velocityThisFrame.x) > Mathf.Abs(velocityThisFrame.y))
            {
                velocityThisFrame.x -= MovementIncrementSigned(velXPositive);
                microVelocity = new Vector3(MovementIncrementSigned(velXPositive), 0, 0);

                if (CollisionManager.CheckCollision(positionNextFrame + microVelocity, transform.localScale))
                    velocityThisFrame.x = 0;
                else
                    positionNextFrame += microVelocity;

                if (velocityThisFrame.x < movementIncrement && velocityThisFrame.x > -movementIncrement)
                    velocityThisFrame.x = 0;

            }
            else
            {
                velocityThisFrame.y -= MovementIncrementSigned(velYPositive);
                microVelocity = new Vector3(0, MovementIncrementSigned(velYPositive), 0);

                if (CollisionManager.CheckCollision(positionNextFrame + microVelocity, transform.localScale))
                    velocityThisFrame.y = 0;
                else
                    positionNextFrame += microVelocity;

                if (velocityThisFrame.y < movementIncrement && velocityThisFrame.y > -movementIncrement)
                    velocityThisFrame.y = 0;
            }
        }

        transform.position = positionNextFrame;
    }

    private float MovementIncrementSigned(bool positiveDirection)
    {
        if (positiveDirection)
            return movementIncrement;
        else
            return -movementIncrement;
    }
}