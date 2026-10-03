using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direccion;
    private float velocidad;
    private int dano;
    private Transform objetivo;
    private PlayerHealth jugador;

    private const float RadioImpacto = 0.35f;
    private const float TiempoVida = 8f;

    public void Iniciar(Vector3 dir, float vel, int danoBala, Transform cam, PlayerHealth pj)
    {
        direccion = dir;
        velocidad = vel;
        dano = danoBala;
        objetivo = cam;
        jugador = pj;
        Destroy(gameObject, TiempoVida);
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;

        if (objetivo != null &&
            Vector3.Distance(transform.position, objetivo.position) <= RadioImpacto)
        {
            if (jugador != null) jugador.RecibirDano(dano);
            Destroy(gameObject);
        }
    }
}