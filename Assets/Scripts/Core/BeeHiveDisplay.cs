using UnityEngine;
using BeeSwarm.Core;

/// <summary>
/// Визуализация улья (2D) — показывает состояние прямо на сцене
/// </summary>
public class BeeHiveDisplay : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private HiveManager hiveManager;
    [SerializeField] private Seasons.SeasonCycle seasonCycle;

    [Header("Визуал")]
    [SerializeField] private SpriteRenderer hiveSprite;
    [SerializeField] private SpriteRenderer honeyIndicator;
    [SerializeField] private SpriteRenderer seasonOverlay;

    [Header("Анимация")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.1f;

    private Vector3 baseScale;
    private float pulseTimer;

    void Start()
    {
        if (hiveManager == null) hiveManager = HiveManager.Instance;
        if (seasonCycle == null) seasonCycle = FindObjectOfType<Seasons.SeasonCycle>();

        baseScale = transform.localScale;

        seasonCycle?.OnSeasonChanged.AddListener(OnSeasonChanged);
    }

    void Update()
    {
        if (hiveManager == null) return;

        // Пульсация
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
        transform.localScale = baseScale * pulse;

        // Индикатор мёда
        if (honeyIndicator != null)
        {
            float fill = hiveManager.HoneyAmount / 1000f;
            honeyIndicator.transform.localScale = new Vector3(Mathf.Clamp01(fill), 1f, 1f);
        }

        // Сезонный цвет
        if (hiveSprite != null && seasonCycle != null)
        {
            hiveSprite.color = seasonCycle.CurrentSeason switch
            {
                Seasons.Season.Spring => new Color(0.7f, 1f, 0.7f),
                Seasons.Season.Summer => Color.white,
                Seasons.Season.Autumn => new Color(1f, 0.8f, 0.5f),
                Seasons.Season.Winter => new Color(0.7f, 0.8f, 1f),
                _ => Color.white
            };
        }
    }

    private void OnSeasonChanged(Seasons.Season newSeason)
    {
        Debug.Log($"🏠 Улей: сезон сменился на {newSeason}");
    }

    void OnDrawGizmosSelected()
    {
        if (hiveManager == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
