using System;
using UnityEngine;

namespace EclipseraGlitch
{
    public class GlitchPulse : MonoBehaviour
    {
        public static GlitchPulse Instance { get; private set; }

        [Header("Configuración del pulso")]
        [Tooltip("Segundos entre cada pulso. Más bajo = más rápido/difícil.")]
        [SerializeField] private float pulseInterval = 1.5f;

        [Tooltip("Si está activo, el pulso corre automáticamente al iniciar la escena.")]
        [SerializeField] private bool autoStart = true;
        public event Action OnPulse;


        public bool PulseStateA { get; private set; } = true;

        private float _timer;
        private bool _running;

        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
             if (autoStart) StartPulse();

        }

        private void Update()
        {
            if (!_running) return;

            _timer += Time.deltaTime;
            if (_timer >= pulseInterval)
            {
                _timer = 0f;
                Pulse();
            }
        }

        public void StartPulse()
        {
            _running = true;
            _timer = 0f;
        }

        public void StopPulse()
        {
            _running = false;
        }

        public void SetInterval(float newInterval)
        {
            pulseInterval = Mathf.Max(0.05f, newInterval);
        }


        public void ForcePulse()
        {
            _timer = 0f;
            Pulse();
        }

        private void Pulse()
        {
            PulseStateA = !PulseStateA;
            OnPulse?.Invoke();
        }
    }
}
