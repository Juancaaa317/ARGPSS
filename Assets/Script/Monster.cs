using UnityEngine;
using System.Collections.Generic;
public class Monster : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 30;

    [Header("Teletransporte")]
    [SerializeField] private float radioMin = 2.5f;
    [SerializeField] private float radioMax = 4f;
    [SerializeField] private float anguloMinimoCambio = 90f;   // cuánto debe cambiar de lado
    [SerializeField] private float pausaTrasTeleport = 1.5f;   // tiempo sin disparar tras moverse

    [Header("Ataque")]
    [SerializeField] private GameObject proyectilPrefab;       // opcional
    [SerializeField] private float intervaloDisparo = 2.5f;
    [SerializeField] private float velocidadProyectil = 2.5f;
    [SerializeField] private int danoProyectil = 10;
    [SerializeField] private float alturaDisparo = 0.4f;       // altura sobre la base del monstruo

    private int vidaActual;
    private Renderer rend;
    private Color colorOriginal;
    private Transform cam;
    private PlayerHealth jugador;
    private float alturaBase;
    private float proximoDisparo;

    public static readonly List<Monster> Activos = new List<Monster>();

    void OnEnable() { Activos.Add(this); }
    void OnDisable() { Activos.Remove(this); }

    void Start()
    {
        vidaActual = vidaMaxima;
        alturaBase = transform.position.y;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null) colorOriginal = rend.material.color;

        Camera c = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        cam = c != null ? c.transform : null;

        jugador = FindAnyObjectByType<PlayerHealth>();
        proximoDisparo = Time.time + intervaloDisparo + Random.Range(0f, 1.5f);
    }

    void Update()
    {
        if (cam == null) return;

        // Mirar siempre al jugador (solo horizontal)
        Vector3 dir = cam.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        if (jugador != null && jugador.Muerto) return;

        if (Time.time >= proximoDisparo)
        {
            Disparar();
            proximoDisparo = Time.time + intervaloDisparo;
        }
    }

    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;
        Debug.Log("Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
            return;
        }

        if (rend != null)
        {
            rend.material.color = Color.red;
            CancelInvoke(nameof(RestaurarColor));
            Invoke(nameof(RestaurarColor), 0.3f);
        }

        Teletransportar();
    }

    private void Teletransportar()
    {
        if (cam == null) return;

        // Ángulo actual del monstruo respecto a ti
        Vector3 rel = transform.position - cam.position;
        float anguloActual = Mathf.Atan2(rel.x, rel.z) * Mathf.Rad2Deg;

        // Nuevo ángulo: al menos "anguloMinimoCambio" lejos del actual
        float salto = Random.Range(anguloMinimoCambio, 360f - anguloMinimoCambio);
        float nuevoAngulo = anguloActual + salto;
        float radio = Random.Range(radioMin, radioMax);

        Vector3 offset = Quaternion.Euler(0f, nuevoAngulo, 0f) * Vector3.forward * radio;
        transform.position = new Vector3(cam.position.x + offset.x, alturaBase, cam.position.z + offset.z);

        // Tras moverse, le das tiempo al jugador de reubicarse
        proximoDisparo = Time.time + pausaTrasTeleport;
    }

    private void Disparar()
    {
        Vector3 origen = transform.position + Vector3.up * alturaDisparo;
        Vector3 direccion = (cam.position - origen).normalized;

        GameObject bala;
        if (proyectilPrefab != null)
        {
            bala = Instantiate(proyectilPrefab, origen, Quaternion.identity);
        }
        else
        {
            // Respaldo: esfera simple si no asignaste prefab
            bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(bala.GetComponent<Collider>());
            bala.transform.position = origen;
            bala.transform.localScale = Vector3.one * 0.15f;
            bala.GetComponent<Renderer>().material.color = Color.magenta;
        }

        Projectile p = bala.GetComponent<Projectile>();
        if (p == null) p = bala.AddComponent<Projectile>();
        p.Iniciar(direccion, velocidadProyectil, danoProyectil, cam, jugador);
    }

    private void RestaurarColor()
    {
        if (rend != null) rend.material.color = colorOriginal;
    }

    private void Morir()
    {
        Debug.Log("¡Monstruo derrotado!");
        Destroy(gameObject);
    }
}