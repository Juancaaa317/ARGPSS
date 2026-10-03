using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private Text vidaText;
    [SerializeField] private float invulnerabilidad = 1f;

    private int vidaActual;
    private float invulnerableHasta;

    public bool Muerto { get; private set; }

    void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarUI();
    }

    public void RecibirDano(int cantidad)
    {
        if (Muerto || Time.time < invulnerableHasta) return;

        vidaActual = Mathf.Max(0, vidaActual - cantidad);
        invulnerableHasta = Time.time + invulnerabilidad;

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif

        if (vidaActual <= 0)
        {
            Muerto = true;
            if (vidaText != null) vidaText.text = "¡Has perdido!";
            return;
        }
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (vidaText != null) vidaText.text = "Vida: " + vidaActual;
    }

    public void Reiniciar()
    {
        Muerto = false;
        vidaActual = vidaMaxima;
        invulnerableHasta = 0f;
        ActualizarUI();
    }
}