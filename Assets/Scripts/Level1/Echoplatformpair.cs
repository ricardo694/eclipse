using UnityEngine;

namespace EclipseraGlitch
{
    public class EchoPlatformPair : MonoBehaviour
    {
        [Header("Las dos plataformas del par")]
        [Tooltip("Arrastra el GameObject de la plataforma A (debe tener SpriteRenderer + Collider2D)")]
        [SerializeField] private GameObject platformA;

        [Tooltip("Arrastra el GameObject de la plataforma B (debe tener SpriteRenderer + Collider2D)")]
        [SerializeField] private GameObject platformB;

        [Header("Visual")]
        [Tooltip("Alpha de la plataforma fantasma (la que NO tiene colisión en ese momento)")]
        [Range(0f, 1f)]
        [SerializeField] private float ghostAlpha = 0.25f;

        [Tooltip("Color base de la plataforma cuando está sólida")]
        [SerializeField] private Color solidColor = Color.white;

        [Tooltip("Si está activo, A empieza sólida. Si no, empieza B.")]
        [SerializeField] private bool startWithAActive = true;

        private SpriteRenderer _spriteA, _spriteB;
        private Collider2D _colA, _colB;
        private bool _aIsActive;

        private void Awake()
        {
            _spriteA = platformA.GetComponent<SpriteRenderer>();
            _spriteB = platformB.GetComponent<SpriteRenderer>();
            _colA = platformA.GetComponent<Collider2D>();
            _colB = platformB.GetComponent<Collider2D>();
        }

        private void Start()
        {
            _aIsActive = startWithAActive;
            ApplyState();


            if (GlitchPulse.Instance != null)
                GlitchPulse.Instance.OnPulse += HandlePulse;
        }

        private void OnDestroy()
        {
            if (GlitchPulse.Instance != null)
                GlitchPulse.Instance.OnPulse -= HandlePulse;
        }

        private void HandlePulse()
        {
            _aIsActive = !_aIsActive;
            ApplyState();
        }

        private void ApplyState()
        {
            SetPlatformState(_spriteA, _colA, _aIsActive);
            SetPlatformState(_spriteB, _colB, !_aIsActive);
        }

        private void SetPlatformState(SpriteRenderer sprite, Collider2D col, bool isSolid)
        {
            if (isSolid)
            {
                sprite.color = solidColor;
                col.enabled = true;
            }
            else
            {
                Color ghost = solidColor;
                ghost.a = ghostAlpha;
                sprite.color = ghost;
                col.enabled = false; 
            }
        }
    }
}
