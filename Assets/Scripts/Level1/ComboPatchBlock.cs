using System.Collections;
using UnityEngine;

namespace EclipseraGlitch
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class ComboPatchBlock : MonoBehaviour
    {
        [Header("Colores")]
        [SerializeField] private Color corruptedColor = new Color(1f, 0.2f, 0.6f); // rosa "error"
        [SerializeField] private Color patchedColor = new Color(0.2f, 1f, 1f);     // cian "estable"

        [Header("Tiempo")]
        [Tooltip("Cuánto tiempo se mantiene parchado/pasable antes de volver a corromperse")]
        [SerializeField] private float patchDuration = 2.5f;

        [Header("Aviso antes de re-corromperse")]
        [SerializeField] private bool useWarningBlink = true;
        [SerializeField] private float warningDuration = 0.5f;
        [SerializeField] private float blinkInterval = 0.08f;

        private SpriteRenderer _sprite;
        private Collider2D _collider;
        private Coroutine _revertRoutine;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            SetCorrupted();
        }

 
        public void Patch()
        {
            if (_revertRoutine != null) StopCoroutine(_revertRoutine);

            SetPatched();
            _revertRoutine = StartCoroutine(RevertAfterDelay());
        }

        private IEnumerator RevertAfterDelay()
        {
            float wait = patchDuration - (useWarningBlink ? warningDuration : 0f);
            if (wait > 0f) yield return new WaitForSeconds(wait);

            if (useWarningBlink)
                yield return StartCoroutine(BlinkWarning());

            SetCorrupted();
            _revertRoutine = null;
        }

        private IEnumerator BlinkWarning()
        {
            float t = 0f;
            bool toggle = true;
            while (t < warningDuration)
            {
                toggle = !toggle;
                _sprite.color = toggle ? patchedColor : corruptedColor;
                yield return new WaitForSeconds(blinkInterval);
                t += blinkInterval;
            }
        }

        private void SetPatched()
        {
            _sprite.color = patchedColor;
            _collider.enabled = false; // pasable
        }

        private void SetCorrupted()
        {
            _sprite.color = corruptedColor;
            _collider.enabled = true; // bloquea
        }
    }
}