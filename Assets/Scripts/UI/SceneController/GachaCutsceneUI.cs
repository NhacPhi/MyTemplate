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
        Time.timeScale = 1f;
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
        if (director != null && !_isCompleted)
        {
            if (director.state == PlayState.Playing && director.playableGraph.IsValid())
            {
                director.playableGraph.GetRootPlayable(0).SetSpeed(timelineSpeed);
            }

            // Kiểm tra khi Timeline vừa chạm mốc cuối -> Chuyển sang ResultScene LẬP TỨC 0ms!
            if (director.duration > 0 && director.time >= (director.duration - 0.05f))
            {
                CompleteCutscene();
            }
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
                uiManager.CloseWindowScene(ScreenIds.GachaCutsceneScene);
            }

            // Go to GachaResultScene
            uiManager.OpenWindowScene(ScreenIds.GachaResultScene);
        }
        else
        {
            Debug.LogWarning("[GachaCutsceneUI] UIManager is not injected or available. Screen transitions skipped.");
        }
    }

    private static readonly int WormholeColourId = Shader.PropertyToID("_Wormhole_colour");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock _propBlock;
    private MaterialPropertyBlock PropBlock
    {
        get
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            return _propBlock;
        }
    }

    private void ApplyColor(Color color)
    {
        // 0. Apply to Wormhole bằng MaterialPropertyBlock (Không tạo Instance Material, không gây lag/rác RAM)
        if (wormholeRenderer != null)
        {
            wormholeRenderer.GetPropertyBlock(PropBlock);
            PropBlock.SetColor(WormholeColourId, color);
            PropBlock.SetColor(ColorId, color);
            if (!string.IsNullOrEmpty(colorPropertyName))
            {
                PropBlock.SetColor(Shader.PropertyToID(colorPropertyName), color);
            }
            wormholeRenderer.SetPropertyBlock(PropBlock);
        }

        // 1. Apply to Renderers bằng MaterialPropertyBlock
        foreach (var r in targetRenderers)
        {
            if (r != null)
            {
                r.GetPropertyBlock(PropBlock);
                PropBlock.SetColor(ColorId, color);
                PropBlock.SetColor(BaseColorId, color);
                PropBlock.SetColor(EmissionColorId, color);
                r.SetPropertyBlock(PropBlock);
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
