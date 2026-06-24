using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{
    private AttackHitboxMultijugador _hitbox;
    private PlayerControllerMultijugador _controller;

    void Awake()
    {
        _hitbox = GetComponentInChildren<AttackHitboxMultijugador>();
        _controller = GetComponent<PlayerControllerMultijugador>();
    }

    public void ActivarHitbox()
    {
        _hitbox?.ActivarHitbox();
    }

    public void DesactivarHitbox()
    {
        _hitbox?.DesactivarHitbox();
    }

    public void IniciarCombo()
    {
        _controller?.IniciarCombo();
    }

    public void DesactivaAtaque()
    {
        _controller?.DesactivaAtaque();
    }

    public void DesactivarDano()
    {
        _controller?.DesactivarDano();
    }
}