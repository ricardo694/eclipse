using System.Collections;
using UnityEngine;

namespace EclipseraGlitch
{

    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class IntermittentWall : MonoBehaviour
    {
        [Header("Estado inicial")]
        [SerializeField] private bool startVisible = true;

        [Header("Advertencia (telegraph)")]
        [Tooltip("Si está activo, el muro parpadea brevemente antes de cambiar de estado, para avisar al jugador.")]
        [SerializeField] private bool useWarningBlink = true;
        [SerializeField] private float warningDuration = 0.3f;
        [SerializeField] private float blinkInterval = 0.06f;

        [Header("Visual")]
        [Tooltip("Alpha cuando el muro está 'apagado'. 0 = invisible total. Un valor bajo (ej 0.1) deja un rastro tenue como pista visual.")]
        [Range(0f, 1f)]
        [SerializeField] private float hiddenAlpha = 0f;

        private SpriteRenderer _sprite;
        private Collider2D _collider;
        private bool _isVisible;
        private Color _baseColor;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _baseColor = _sprite.color;
            _isVisible = startVisible;
        }

        private void Start()
        {
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
            if (useWarningBlink)
                StartCoroutine(BlinkThenToggle());
            else
                Toggle();
        }

        private IEnumerator BlinkThenToggle()
        {
            float t = 0f;
            bool blinkBright = true;

            while (t < warningDuration)
            {
                blinkBright = !blinkBright;
                SetAlpha(blinkBright ? 1f : 0.3f);
                yield return new WaitForSeconds(blinkInterval);
                t += blinkInterval;
            }

            Toggle();
        }

        private void Toggle()
        {
            _isVisible = !_isVisible;
            ApplyState();
        }

        private void ApplyState()
        {
            SetAlpha(_isVisible ? 1f : hiddenAlpha);
            _collider.enabled = _isVisible;
        }

        private void SetAlpha(float a)
        {
            Color c = _baseColor;
            c.a = a;
            _sprite.color = c;
        }
    }
}