using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Billboard : MonoBehaviour
{
    public Camera cam;
    public Canvas can;
    public Sprite lootableSprite;
    public Sprite lootActionSprite;

    public GameObject ProgressSliderPrefab;
    private Slider ProgressSlider;

    private Image lootableIcon;
    private Image lootActionIcon;
    private Vector3 height;

    void Start()
    {
        height = new Vector3(0f, 17f, 0f);
    }

    void LateUpdate()
    {
        if (!ProgressSlider) return;

        ProgressSlider.transform.position = cam.WorldToScreenPoint(transform.position + height);
    }

    public void PositionSlider()
    {
        if (ProgressSlider) return;
        ProgressSlider = Instantiate(ProgressSliderPrefab, new Vector3(transform.position.x, 2f, transform.position.y), Quaternion.identity, can.transform).GetComponent<Slider>();
    }

    public float Progress { get; set; }
    public bool ProgressDone { get; set; }

    public IEnumerator UpdateProgressSlider()
    {
        PositionSlider();
        ProgressSlider.gameObject.SetActive(true);
        if (!ProgressSlider) yield return 0;
        while (!ProgressDone)
        {
            var prog = Progress;
            ProgressSlider.value = Mathf.Clamp01(prog);
            yield return 0;
        }
        ProgressDone = false;
        ProgressSlider.gameObject.SetActive(false);
    }
}
