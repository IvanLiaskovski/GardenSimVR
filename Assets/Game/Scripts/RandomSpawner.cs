using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public int gridSize = 20;
    public float cellSize = 2f;
    public int amountToSpawn = 20;

    private bool[,] occupied;

    void Start()
    {
        occupied = new bool[gridSize, gridSize];
        Spawn();
    }

    void Spawn()
    {
        int spawned = 0;

        while (spawned < amountToSpawn)
        {
            int x = Random.Range(0, gridSize);
            int z = Random.Range(0, gridSize);

            if (occupied[x, z]) continue;

            occupied[x, z] = true;

            Vector3 pos = new Vector3(
                x * cellSize,
                0,
                z * cellSize
            );

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Instantiate(prefab, pos, Quaternion.identity);

            spawned++;
        }
    }
}