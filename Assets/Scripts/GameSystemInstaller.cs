using UnityEngine;

public class GameSystemsInstaller : MonoBehaviour
{
    public GameObject gameSystemsPrefab;

    void Awake()
    {
        if (DataController.Instance == null)
        {
            var go = Instantiate(gameSystemsPrefab);
            go.name = "GameSystems";
        }
    }
}