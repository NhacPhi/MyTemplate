using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UIFramework;
using VContainer;

public class GachaCutsceneUI : WindowController
{
    [Header("Timeline Components")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private float timelineSpeed = 1.25f;
    [SerializeField] private Button btnSkip;

    [Header("Color Settings")]
    [SerializeField] private Color whiteColor = Color.white;
    [SerializeField] private Color purpleColor = new Color(0.6f, 0.2f, 0.8f);
    [SerializeField] private Color goldColor = new Color(1f, 0.8f, 0f);

    [Header("Target Objects to Colorize")]
    [Tooltip("Kéo object Wormhole có chứa Shader Graph vào đây!")]
    [SerializeField] private Renderer wormholeRenderer;
    [Tooltip("Tên Reference của biến màu trong Shader Graph (VD: _BaseColor, _Color, _EmissionColor, Color_xxx)")]
    [SerializeField] private string colorPropertyName = "Wormhole colour";
    
    [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();
    [SerializeField] private List<Light> targetLights = new List<Light>();
    [SerializeField] private List<ParticleSystem> targetParticles = new List<ParticleSystem>();
    [SerializeField] private List<Image> targetImages = new List<Image>();

    [Header("Transition Settings")]
    [SerializeField] private string resultScreenId = "";
    [SerializeField] private bool closeSelfOnFinish = true;

    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onCutsceneFinishedEvent;

    [Inject] private UIManager uiManager;

    public System.Action OnCutsceneFinished;

    private bool _isCompleted = false;

    private void Awake()
    {
        if (btnSkip != null)
        {
            btnSkip.onClick.AddListener(SkipCutscene);
        }
    }

    private void OnEnable()
    {
        _isCompleted = false;
        
        if (btnSkip != null)
        {
            btnSkip.interactable = true;
        }

        if (director != null)
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.stopped += OnTimelineStopped;
        }

        DetermineAndSetColor();
    }

    private void DetermineAndSetColor()
    {
        if (GachaRollState.LastResults == null || GachaRollState.LastResults.Count == 0) return;

        Rare highestRarity = Rare.Common;
        foreach (var res in GachaRollState.LastResults)
        {
            if (res.rarity > highestRarity) 
            {
                highestRarity = res.rarity;
            }
        }

        SetRarityColor(highestRarity);
    }

    private void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }

    private void Update()
    {
        if (director != null && director.state == PlayState.Playing && director.playableGraph.IsValid())
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(timelineSpeed);
        }
    }

    /// <summary>
    /// Set the color of the cutscene objects based on Gacha rarity
    /// </summary>
    /// <param name="rarity">Rare enum</param>
    public void SetRarityColor(Rare rarity)
    {
        Color selectedColor = whiteColor;
        switch (rarity)
        {
            case Rare.Common:
            case Rare.Uncommon:
            case Rare.Rare:
                selectedColor = whiteColor;
                break;
            case Rare.Epic:
                selectedColor = purpleColor;
                break;
            case Rare.Legendary:
                selectedColor = goldColor;
                break;
        }
        ApplyColor(selectedColor);
    }

    /// <summary>
    /// Helper to set rarity color using integer rating (e.g. 3 stars = White, 4 stars = Purple, 5 stars = Gold)
    /// </summary>
    public void SetRarityColor(int stars)
    {
        if (stars >= 5)
        {
            SetRarityColor(Rare.Legendary);
        }
        else if (stars == 4)
        {
            SetRarityColor(Rare.Epic);
        }
        else
        {
            SetRarityColor(Rare.Rare);
        }
    }

    /// <summary>
    /// Skips the cutscene and triggers completion logic
    /// </summary>
    public void SkipCutscene()
    {
        if (_isCompleted) return;

        Debug.Log("[GachaCutsceneUI] Skip cutscene requested.");
        
        if (btnSkip != null)
        {
            btnSkip.interactable = false;
        }

        if (director != null)
        {
            // Set playback speed to fast or directly evaluate at the end
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }
        
        CompleteCutscene();
    }

    private void OnTimelineStopped(PlayableDirector playableDirector)
    {
        if (playableDirector == director)
        {
            CompleteCutscene();
        }
    }

    private void CompleteCutscene()
    {
        if (_isCompleted) return;
        _isCompleted = true;

        Debug.Log("[GachaCutsceneUI] Gacha cutscene completed.");

        // Raise callbacks
        OnCutsceneFinished?.Invoke();
        onCutsceneFinishedEvent?.Invoke();

        // Handle screen transitions
        if (uiManager != null)
        {
            if (closeSelfOnFinish)
            {
                UI_Close();
            }

            // For UI testing: Go to GachaResultScene
            uiManager.OpenWindowScene(ScreenIds.GachaResultScene);
        }
        else
        {
            Debug.LogWarning("[GachaCutsceneUI] UIManager is not injected or available. Screen transitions skipped.");
        }
    }

    private void ApplyColor(Color color)
    {
        // 0. Apply to Wormhole (Mới thêm)
        if (wormholeRenderer != null && wormholeRenderer.material != null)
        {
            Material mat = wormholeRenderer.material;
            mat.color = color; // Gắn _Color mặc định
            
            // Ép màu bằng tên biến lấy từ Inspector
            if (!string.IsNullOrEmpty(colorPropertyName) && mat.HasProperty(colorPropertyName))
            {
                mat.SetColor(colorPropertyName, color);
            }
            
            // Quét dự phòng các tên biến phổ biến khác của Shader Graph
            if (mat.HasProperty("Wormhole colour")) mat.SetColor("Wormhole colour", color);
            if (mat.HasProperty("_Wormhole_colour")) mat.SetColor("_Wormhole_colour", color);
            if (mat.HasProperty("_WormholeColour")) mat.SetColor("_WormholeColour", color);
            if (mat.HasProperty("_Wormholecolour")) mat.SetColor("_Wormholecolour", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color);
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);
            if (mat.HasProperty("Color")) mat.SetColor("Color", color);
            
            DynamicGI.SetEmissive(wormholeRenderer, color);
        }

        // 1. Apply to Renderers
        foreach (var r in targetRenderers)
        {
            if (r != null)
            {
                // Assign new instances or edit materials
                if (r.material != null)
                {
                    r.material.color = color;
                    if (r.material.HasProperty("_EmissionColor"))
                    {
                        r.material.SetColor("_EmissionColor", color);
                        DynamicGI.SetEmissive(r, color);
                    }
                    if (r.material.HasProperty("_BaseColor"))
                    {
                        r.material.SetColor("_BaseColor", color);
                    }
                }
            }
        }

        // 2. Apply to Lights
        foreach (var l in targetLights)
        {
            if (l != null)
            {
                l.color = color;
            }
        }

        // 3. Apply to Particle Systems
        foreach (var ps in targetParticles)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
        }

        // 4. Apply to Images
        foreach (var img in targetImages)
        {
            if (img != null)
            {
                img.color = color;
            }
        }
    }
}
