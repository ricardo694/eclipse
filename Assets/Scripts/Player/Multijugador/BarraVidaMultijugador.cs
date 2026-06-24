using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class BarraVidaMultijugador : MonoBehaviour
{
    [Header("Configuración")]
    public Image rellenoBarraVida;
    public int   playerIndex = 0;  // 0 = P1, 1 = P2

    [Header("Colores opcionales")]
    public Color colorAlto  = Color.green;
    public Color colorMedio = Color.yellow;
    public Color colorBajo  = Color.red;

    private PlayerControllerMultijugador _player;
    private float            _vidaMaxima;

    void Update()
    {
        // Buscar el jugador si aún no lo encontramos
        if (_player == null)
        {
            FindPlayer();
            return;
        }

        // Calcular porcentaje de vida
        float pct = (float)_player.vida / _vidaMaxima;
        rellenoBarraVida.fillAmount = Mathf.Clamp01(pct);

        // Cambiar color según vida restante
        if      (pct > 0.5f) rellenoBarraVida.color = colorAlto;
        else if (pct > 0.25f) rellenoBarraVida.color = colorMedio;
        else                  rellenoBarraVida.color = colorBajo;
    }

    private void FindPlayer()
{
    foreach (var p in FindObjectsByType<PlayerControllerMultijugador>())
    {
        // Verificar que Fusion ya inicializó el objeto antes de leer PlayerIndex
        if (p.Object == null || !p.Object.IsValid) continue;
        
        if (p.PlayerIndex == playerIndex)
        {
            _player     = p;
            _vidaMaxima = p.vidaMaxima;
            break;
        }
    }
}

    // Llamar esto desde el HUD si necesitas resetear entre rondas
    public void ResetBarra()
    {
        _player = null;
    }
}