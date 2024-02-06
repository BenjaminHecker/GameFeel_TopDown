using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Header("References")]
    [SerializeField] public PlayerMovement player;
    [SerializeField] private Obstacle obstaclePrefab;

    [Header("Sweeper")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform headLine;
    [SerializeField] private Transform tailLine;
    [SerializeField] private TrailRenderer headTrail;
    [SerializeField] private TrailRenderer tailTrail;
    [SerializeField] private float radius;

    public static Vector3 Origin { get { return instance.origin.position; } }
    public static float Radius { get { return instance.radius; } }
    public static float headAngle = 0f;
    public static float tailAngle = 0f;

    private static float sweeperTime;

    [Space]
    [SerializeField] private float tailDelay;
    [SerializeField] private float turnRate;

    [Space]
    [SerializeField] private float minSwitchDelay;
    [SerializeField] private float maxSwitchDelay;

    public static bool clockwise;

    [Header("Obstacles")]
    [SerializeField] private float initialSpawnDelay;
    [SerializeField] private float spawnTick;
    [SerializeField] private AnimationCurve spawnCurve;
    [SerializeField] private AnimationCurve obstacleScaleCurve;

    private List<Obstacle> obstacles = new List<Obstacle>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CollisionManager.AddCollider(origin);

        StartCoroutine(RunSweeper());
    }

    private IEnumerator RunSweeper()
    {
        sweeperTime = 0f;
        float switchTime = 0f;
        float spawnTime = 0f;

        float switchDelay = Random.Range(minSwitchDelay, maxSwitchDelay);

        headAngle = 0f;
        tailAngle = 0f;
        clockwise = true;

        while (true)
        {
            if (switchTime > switchDelay)
            {
                switchTime = 0f;
                switchDelay = Random.Range(minSwitchDelay, maxSwitchDelay);
                clockwise = !clockwise;

                headTrail.emitting = clockwise;
                tailTrail.emitting = !clockwise;
            }

            float turnAngle;
            if (clockwise)
                turnAngle = -turnRate * Time.deltaTime;
            else
                turnAngle = turnRate * Time.deltaTime;

            headAngle += turnAngle;
            headLine.Rotate(origin.forward, turnAngle);

            if (sweeperTime >= tailDelay)
            {
                tailAngle += turnAngle;
                tailLine.Rotate(origin.forward, turnAngle);
            }

            if (headAngle < -180f)
                headAngle = 180f;
            else if (headAngle > 180f)
                headAngle = -180f;

            if (tailAngle < -180f)
                tailAngle = 180f;
            else if (tailAngle > 180f)
                tailAngle = -180f;

            if (spawnTime >= spawnTick)
            {
                SpawnObstacle();

                spawnTime = 0f;
            }

            yield return new WaitForEndOfFrame();
            sweeperTime += Time.deltaTime;
            switchTime += Time.deltaTime;

            if (sweeperTime >= initialSpawnDelay)
                spawnTime += Time.deltaTime;
        }
    }

    private void SpawnObstacle()
    {
        Vector3 lineStart, lineEnd;

        if (clockwise)
        {
            lineStart = headLine.position;
            lineEnd = headLine.position + headLine.up * radius;
        }
        else
        {
            lineStart = tailLine.position;
            lineEnd = tailLine.position + tailLine.up * radius;
        }

        Vector3 randomSpawnPos = Vector3.Lerp(lineStart, lineEnd, spawnCurve.Evaluate(Random.value));

        Obstacle newObstacle = Instantiate(obstaclePrefab);
        newObstacle.transform.position = randomSpawnPos;

        StartCoroutine(HandleObstacleLife(newObstacle));
    }

    private IEnumerator HandleObstacleLife(Obstacle obstacle)
    {
        obstacles.Add(obstacle);
        CollisionManager.AddCollider(obstacle.transform);

        float obstacleLifeTimer = 0f;

        while (obstacleLifeTimer < tailDelay)
        {
            obstacle.UpdateScale(obstacleLifeTimer / tailDelay);

            yield return new WaitForEndOfFrame();
            obstacleLifeTimer += Time.deltaTime;
        }

        obstacle.UpdateScale(1f);

        obstacles.Remove(obstacle);
        CollisionManager.RemoveCollider(obstacle.transform);
        Destroy(obstacle.gameObject);
    }

    public static void GameOver()
    {
        if (sweeperTime < instance.tailDelay)
            return;

        instance.StopCoroutine(instance.RunSweeper());
        SceneManager.LoadScene(0);
    }
}
