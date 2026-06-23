using UnityEngine;

namespace EclipseraGlitch
{

    public class GhostTrail : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.3f;

        private SpriteRenderer _sr;
        private Color _startColor;
        private float _timer;


        public static void Spawn(Sprite sprite, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = new GameObject("GhostTrail");
            go.transform.position = position;
            go.transform.localScale = scale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;

            GhostTrail ghost = go.AddComponent<GhostTrail>();
            ghost.Init(sr, color);
        }

        private void Init(SpriteRenderer sr, Color startColor)
        {
            _sr = sr;
            _startColor = startColor;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float t = _timer / fadeDuration;

            Color c = _startColor;
            c.a = Mathf.Lerp(_startColor.a, 0f, t);
            _sr.color = c;

            if (t >= 1f) Destroy(gameObject);
        }
    }
}