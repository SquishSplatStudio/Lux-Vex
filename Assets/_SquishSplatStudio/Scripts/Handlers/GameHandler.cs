/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using SquishSplatStudio;
using UnityEngine;
using System.Collections;

public sealed class GameHandler : MonoBehaviour
{
    #region ---[ singleton code base ]---

    static GameHandler() { }

    private GameHandler() { }

    public static GameHandler Instance { get; private set; }

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

    [SerializeField] public int levelTime;
    [SerializeField] public GameObject lightWorkerPrefab;

    [Header("Resources")]
    [SerializeField] int Crystals;
    [SerializeField] int MaxCrystals;
    [SerializeField] int Souls;
    [SerializeField] int MaxSouls;
    [SerializeField] int Workers;
    [SerializeField] int MaxWorkers;
    [SerializeField] int Capacity;
    [SerializeField] int MaxCapacity;
    [SerializeField] int Light;
    [SerializeField] int MaxLight;
    [SerializeField] int Leylines;
    [SerializeField] int Mines;
    [SerializeField] public GameObject miniSun;
    [SerializeField] public MeshRenderer sunRenderer;
    [SerializeField] public float sunTimer;
    [SerializeField] Light sceneSunLight;
    [SerializeField] public float startingSunBrightness;
    [SerializeField] public float timeLeftPercent;

    [Header("Game Variables")]
    [SerializeField] public int LightWorkerDamage = 5;
    [SerializeField] public int LightToShadeDamage = 5;
    [SerializeField] public int ShadeDamage = 6;
    [SerializeField] public int ShadeToLightDamage = 5;

    public int TickRate;
    public int TickCost;

    void Start()
    {
        
        ResourceType.Crystal.IncreaseMaxBy(MaxCrystals);
        ResourceType.Crystal.IncreaseBy(Crystals);

        ResourceType.Soul.IncreaseMaxBy(MaxSouls);
        ResourceType.Soul.IncreaseBy(Souls);

        ResourceType.Worker.IncreaseMaxBy(MaxWorkers);
        ResourceType.Worker.IncreaseBy(Workers);

        ResourceType.Capacity.IncreaseMaxBy(MaxCapacity);
        ResourceType.Capacity.IncreaseBy(Capacity);

        ResourceType.Light.IncreaseMaxBy(MaxLight);
        ResourceType.Light.IncreaseBy(Light);

        SetTickCost(TickCost);
        SetTickRate(TickRate);

        StartCoroutine(EnableInputs());

        sunTimer = levelTime * 60f;

        InvokeRepeating("UpdateSceneLightingAndAudio", 0f, 1f);

    }
    bool p100 = false;
    bool p66 = false;
    bool p33 = false;
    void UpdateSceneLightingAndAudio()
    {
        timeLeftPercent = (sunTimer / (levelTime * 60f));
        sceneSunLight.intensity = timeLeftPercent * startingSunBrightness;
        if (timeLeftPercent <= 1 && !p100)
        {
            p100 = true;
            AudioController.Instance.PlaySolarActivity(0);
        }
        else if (timeLeftPercent <= .66 && !p66)
        {
            p66 = true;
            AudioController.Instance.PlaySolarActivity(1);
        } else if(timeLeftPercent <= .33 && !p33)
        {
            p33 = true;
            AudioController.Instance.PlaySolarActivity(2);
        } 
    }

    /// <summary>
    /// This will display the Mouse and Mini Sun
    /// </summary>
    public void DisplayOverlayElements()
    {
        // Display Mini Sun
        miniSun.SetActive(true);
        // Set the Mini Sun Animation Playback speed
        miniSun.GetComponent<Animator>().speed = 1f / (float)levelTime;

        // Display Mouse Cursor
        MouseCursor.Instance.DisplayMouseCursors(true);
    }

    IEnumerator EnableInputs()
    {
        yield return new WaitForSeconds(4f);
        GameUI.Instance.ShowCanvas();
        InputHandler.Instance.EnableInput();
    }

    internal void SetTickRate(int tickRate) => TickRate = tickRate;

    internal void SetTickCost(int tickCost) => TickCost = tickCost;

    void Update()
    {
        //if (Time.frameCount % 60 != 0) return;
        if (sunTimer > 0)
            sunTimer -= Time.deltaTime;
        //    if (sunTimer <= 0)
        //        DoGameDeath();
    }
}
