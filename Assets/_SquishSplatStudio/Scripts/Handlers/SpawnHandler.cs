/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using SquishSplatStudio;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{

    #region ---[ singleton code base ]---

    static SpawnHandler() { }

    private SpawnHandler() { }

    public static SpawnHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    [SerializeField] GameObject _spawnerHandler;

    private Spawner _spawner;

    public void Initialize(GameObject spawner)
    {
        _spawnerHandler = spawner;
        _spawner = spawner.GetComponentInChildren<Spawner>();
    }

    public bool BuildLightWorker()
    {
        if (_spawner.IsBuilding) return false;
        _spawner.SpawnLightCreature();
        return true;
    }

    public bool SpawnLightWorker()
    {
        return _spawnerHandler.GetComponent<PlayerStructure>().SpawnLightWorker();
    }
}
