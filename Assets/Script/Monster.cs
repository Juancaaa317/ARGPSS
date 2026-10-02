using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 30;
    private int vidaActual;

    void Start()
    {
        // Al aparecer, el monstruo tiene la vida al máximo
        vidaActual = vidaMaxima;
    }

    // Esta función la llamaremos desde el script del jugador cuando le dispare
    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;
        Debug.Log("¡Le diste al monstruo! Vida restante: " + vidaActual);

        // Feedback visual temporal (cambia el color a rojo un instante)
        GetComponentInChildren<Renderer>().material.color = Color.red;
        Invoke(nameof(RestaurarColor), 0.2f);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void RestaurarColor()
    {
        GetComponentInChildren<Renderer>().material.color = Color.white;
    }

    private void Morir()
    {
        Debug.Log("¡Monstruo derrotado!");
        // Aquí luego pondremos partículas o animaciones. Por ahora, lo destruimos.
        Destroy(gameObject);
    }
}