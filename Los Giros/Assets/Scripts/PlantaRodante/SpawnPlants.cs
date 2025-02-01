using UnityEngine;

public class SpawnPlants : MonoBehaviour
{
    [SerializeField] private float waitTime;
    [SerializeField] private GameObject prefabPlant;
    [SerializeField] private Transform world;
    [SerializeField] private bool moveRight;
    
    private void Start()
    {
        InvokeRepeating(nameof(SpawnPlant), 0, waitTime);
    }

    private void SpawnPlant()
    {
        GameObject go = Instantiate(prefabPlant, world);
        go.transform.localPosition = transform.localPosition;
        go.GetComponent<Plant>().moveRight = moveRight;
    }
}
