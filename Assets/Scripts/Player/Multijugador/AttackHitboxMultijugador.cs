using UnityEngine;

public class AttackHitboxMultijugador : MonoBehaviour
{
    public int dano = 1;
    private PlayerControllerMultijugador _dueño;
    private bool _activo = false;

    void Awake()
    {
        _dueño = GetComponentInParent<PlayerControllerMultijugador>();
        // Empieza desactivado
        GetComponent<Collider2D>().enabled = false;
    }

    public void ActivarHitbox()
    {
        _activo = true;
        GetComponent<Collider2D>().enabled = true;
    }

    public void DesactivarHitbox()
    {
        _activo = false;
        GetComponent<Collider2D>().enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"Trigger detectado: {other.gameObject.name}");
    if (!_activo) { Debug.Log("Hitbox no activo"); return; }
    if (_dueño == null) { Debug.Log("Dueño null"); return; }

    var objetivo = other.GetComponent<PlayerControllerMultijugador>();
    if (objetivo == null) { Debug.Log("No es PlayerControllerMultijugador"); return; }
    if (objetivo == _dueño) { Debug.Log("Es el mismo jugador"); return; }
    if (objetivo.muerto) { Debug.Log("Ya está muerto"); return; }

    Debug.Log($"Enviando daño a {objetivo.PlayerIndex}");
    objetivo.RPC_RecibirDano(transform.position, dano);
    DesactivarHitbox();
}
}