using UnityEngine;

namespace EclipseraGlitch
{
    /// <summary>
    /// Plataforma que avanza constantemente hacia un punto, y en cada GlitchPulse
    /// "rebobina" de golpe a su posición inicial, dejando un rastro fantasma.
    /// Si el jugador está parado encima en el momento del rebobinado, se mueve con ella.
    /// Castiga quedarse quieto, premia el momentum (dash) para escapar a tiempo.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class StutterBlock : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Hacia dónde avanza la plataforma desde su posición inicial (unidades de mundo)")]
        [SerializeField] private Vector2 moveOffset = new Vector2(3f, 0f);
        [SerializeField] private float moveSpeed = 1.5f;

        [Header("Rebobinado (ghost trail)")]
        [SerializeField] private Color trailColorA = new Color(1f, 0f, 1f); // magenta
        [SerializeField] private Color trailColorB = new Color(0f, 1f, 1f); // cian
        [SerializeField] private int trailCopies = 3;

        private Vector3 _startPos;
        private Vector3 _targetPos;
        private Rigidbody2D _rb;
        private SpriteRenderer _sprite;

        
        private Transform _passengerTransform;
        private Rigidbody2D _passengerRb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic; 
            _sprite = GetComponent<SpriteRenderer>();

            _startPos = transform.position;
            _targetPos = _startPos + (Vector3)moveOffset;
        }

        private void Start()
        {
            if (GlitchPulse.Instance != null)
                GlitchPulse.Instance.OnPulse += Rewind;
        }

        private void OnDestroy()
        {
            if (GlitchPulse.Instance != null)
                GlitchPulse.Instance.OnPulse -= Rewind;
        }

        private void FixedUpdate()
        {
            Vector2 current = _rb.position;
            Vector2 next = Vector2.MoveTowards(current, _targetPos, moveSpeed * Time.fixedDeltaTime);
            Vector2 delta = next - current;

            _rb.MovePosition(next);

          
            CarryPassenger(delta);
        }

        private void Rewind()
        {
            Vector3 before = transform.position;

            SpawnTrail(before);
            _rb.position = _startPos; // teleport instantáneo, sin interpolación

            Vector2 delta = (Vector2)(_startPos - before);
            CarryPassenger(delta);
        }

        private void CarryPassenger(Vector2 delta)
        {
            if (delta == Vector2.zero) return;

            if (_passengerRb != null)
                _passengerRb.position += delta;
            else if (_passengerTransform != null)
                _passengerTransform.position += (Vector3)delta;
        }

        private void SpawnTrail(Vector3 fromPosition)
        {
            for (int i = 0; i < trailCopies; i++)
            {
                Vector3 pos = Vector3.Lerp(fromPosition, _startPos, (float)i / trailCopies);
                Color c = (i % 2 == 0) ? trailColorA : trailColorB;
                c.a = 0.6f;
                GhostTrail.Spawn(_sprite.sprite, pos, transform.localScale, c);
            }
        }

  
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            _passengerTransform = other.transform;
            _passengerRb = other.attachedRigidbody;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.transform == _passengerTransform)
            {
                _passengerTransform = null;
                _passengerRb = null;
            }
        }
    }
}