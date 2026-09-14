using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// All visual objects are authored in MainGame. Runtime only updates/reuses six rows.
public class UI_KillFeed : MonoBehaviour
{
    [Serializable]
    private class Row
    {
        public RectTransform root;
        public TMP_Text attacker;
        public Image weapon;
        public TMP_Text victim;
    }

    private struct Entry
    {
        public string attacker, victim;
        public int cause;
        public bool suicide;
        public double expiresAt;
    }

    [Header("Scene references")]
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private RectTransform container;
    [SerializeField] private Row[] rows;
    [SerializeField] private Sprite bulletIcon;
    [SerializeField] private Sprite grenadeIcon;
    [Header("FIFO")]
    [SerializeField, Min(0.1f)] private float messageSeconds = 6;
    [SerializeField, Range(1, 6)] private int maximumLines = 6;
    [Header("Safe area (Canvas units)")]
    [SerializeField, Min(0)] private float sideMargin = 24;
    [Tooltip("Reserve space below the round banner, including its expanded state.")]
    [SerializeField, Min(0)] private float topMargin = 190;
    [SerializeField, Min(0)] private float bottomMargin = 24;
    [SerializeField, Min(1)] private float panelWidth = 560;
    [SerializeField, Min(1)] private float rowHeight = 42;
    [SerializeField, Min(0)] private float rowSpacing = 6;

    private readonly List<Entry> entries = new List<Entry>(6);
    private Rect lastSafeArea;
    private Vector2 lastScreenSize;
    private int capacity;

    private void Awake() => RefreshRows();
    private void OnEnable() => LifeComponent.PlayerKilled += OnPlayerKilled;
    private void OnDisable()
    {
        LifeComponent.PlayerKilled -= OnPlayerKilled;
        entries.Clear();
        RefreshRows();
    }

    private void Update()
    {
        UpdateSafeArea();
        bool changed = false;
        for (int i = entries.Count - 1; i >= 0; i--)
            if (Time.unscaledTimeAsDouble >= entries[i].expiresAt)
            { entries.RemoveAt(i); changed = true; }
        if (changed) RefreshRows();
    }

    private void OnPlayerKilled(int attacker, int victim, string attackerName, string victimName, int cause)
    {
        UpdateSafeArea();
        if (capacity == 0) return;
        // Drop the oldest immediately, then push the new entry at the bottom.
        while (entries.Count >= capacity) entries.RemoveAt(0);
        entries.Add(new Entry {
            attacker = attacker > 0 ? CleanName(attackerName) : "Entorno",
            victim = CleanName(victimName), cause = cause,
            suicide = attacker == victim,
            expiresAt = Time.unscaledTimeAsDouble + messageSeconds
        });
        RefreshRows();
    }

    private static string CleanName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Jugador";
        // Keep arbitrary nicknames on a single line; TMP rich text is disabled as well.
        string clean = value.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
        return clean.Length > 24 ? clean.Substring(0, 24) : clean;
    }

    private void UpdateSafeArea()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Rect area = Screen.safeArea;
        if (screenSize == lastScreenSize && area == lastSafeArea) return;
        lastScreenSize = screenSize;
        lastSafeArea = area;
        if (screenSize.x <= 0 || screenSize.y <= 0) return;
        safeArea.anchorMin = area.min / screenSize;
        safeArea.anchorMax = area.max / screenSize;
        safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        float availableHeight = Mathf.Max(0, safeArea.rect.height - topMargin - bottomMargin);
        capacity = Mathf.Clamp(Mathf.FloorToInt((availableHeight + rowSpacing) / (rowHeight + rowSpacing)),
            0, Mathf.Min(maximumLines, rows.Length));
        container.anchoredPosition = new Vector2(-sideMargin, -topMargin);
        container.sizeDelta = new Vector2(Mathf.Max(0, Mathf.Min(panelWidth,
            safeArea.rect.width - sideMargin * 2)), capacity * (rowHeight + rowSpacing));
        while (entries.Count > capacity) entries.RemoveAt(0);
        RefreshRows();
    }

    private void RefreshRows()
    {
        if (rows == null) return;
        for (int i = 0; i < rows.Length; i++)
        {
            Row row = rows[i];
            row.root.gameObject.SetActive(i < entries.Count);
            if (i >= entries.Count) continue;
            Entry entry = entries[i];
            row.root.anchoredPosition = new Vector2(0, -i * (rowHeight + rowSpacing));
            row.root.sizeDelta = new Vector2(0, rowHeight);
            row.attacker.text = entry.attacker;
            row.victim.text = entry.victim;
            row.attacker.color = entry.suicide ? new Color32(255, 190, 110, 255) : new Color32(115, 210, 255, 255);
            row.weapon.sprite = entry.cause == 1 ? grenadeIcon : bulletIcon;
        }
    }
}
