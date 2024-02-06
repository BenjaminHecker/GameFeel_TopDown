using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private static CollisionManager instance;

    [Header("Collisions")]
    [SerializeField] private List<Transform> colliders = new List<Transform>();

    private void Awake()
    {
        instance = this;
    }

    public static void AddCollider(Transform collider)
    {
        instance.colliders.Add(collider);
    }

    public static void RemoveCollider(Transform collider)
    {
        instance.colliders.Remove(collider);
    }

    public static bool CheckCollision(Vector3 position, Vector3 scale)
    {
        if ((position - GameManager.Origin).magnitude > GameManager.Radius)
        {
            GameManager.GameOver();
            return false;
        }

        float playerAngle = Vector2.SignedAngle(Vector2.up, position);

        if (GameManager.headAngle <= GameManager.tailAngle)
        {
            if (playerAngle < GameManager.headAngle || playerAngle > GameManager.tailAngle)
            {
                GameManager.GameOver();
                return false;
            }
        }
        else
        {
            if (playerAngle < GameManager.headAngle && playerAngle > GameManager.tailAngle)
            {
                GameManager.GameOver();
                return false;
            }
        }


        foreach (Transform currentWall in instance.colliders)
        {
            float xDist = Mathf.Abs(position.x - currentWall.position.x);
            float yDist = Mathf.Abs(position.y - currentWall.position.y);
            float xMaxDist = scale.x / 2 + currentWall.localScale.x / 2;
            float yMaxDist = scale.y / 2 + currentWall.localScale.y / 2;

            if (xDist < xMaxDist && yDist < yMaxDist)
                return true;
        }

        return false;
    }
}
