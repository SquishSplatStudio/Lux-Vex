/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject LightPrefab; // todo: make this a list or use object pool.
    public GameObject DarkPrefab;
    public Billboard billboard;
    public float buildTime;

    public bool IsBuilding { get; set; }

    public void SpawnLightCreature()
    {
        StartCoroutine(UpdateBillboard(buildTime));
    }

    public void SpawnLightCreatureNow()
    {
        Instantiate(LightPrefab, transform.position, Quaternion.identity);
    }

    public IEnumerator UpdateBillboard(float waitInSeconds)
    {
        IsBuilding = true;
        float count = 0;
        StartCoroutine(billboard.UpdateProgressSlider());
        while (count < waitInSeconds)
        {
            count += Time.deltaTime;
            billboard.Progress = (count / waitInSeconds);
            yield return 0;
        }
        Instantiate(LightPrefab, transform.position, Quaternion.identity);
        billboard.ProgressDone = true;
        IsBuilding = false;
    }

    public void SpawnDarkCreature()
    {
        Instantiate(DarkPrefab, transform.position, transform.localRotation);
    }
}
