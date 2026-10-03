using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("Rondas (editables)")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int totalRondas = 5;
    [SerializeField] private int monstruosPrimeraRonda = 1;
    [SerializeField] private int monstruosExtraPorRonda = 1;
    [SerializeField] private float pausaEntreRondas = 3f;

    [Header("Aparición")]
    [SerializeField] private float radioMin = 2.5f;
    [SerializeField] private float radioMax = 4f;
    [SerializeField] private float alturaBajoCamara = 1.2f;

    [Header("UI (opcional)")]
    [SerializeField] private Text rondaText;     // "Ronda 2/5 - Monstruos: 2"
    [SerializeField] private Text anuncioText;   // mensajes grandes

    [Header("Pruebas")]
    [SerializeField] private bool iniciarAlComenzar = false;

    private Transform cam;
    private PlayerHealth jugador;
    private bool iniciado;
    private int rondaActual;

    [SerializeField] private GameObject botonReintentar;

    void Start()
    {
        Camera c = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        cam = c != null ? c.transform : null;
        jugador = FindAnyObjectByType<PlayerHealth>();

        if (rondaText != null) rondaText.text = "";
        if (anuncioText != null) anuncioText.text = "";

        if (iniciarAlComenzar) IniciarRondas();

        if (botonReintentar != null) botonReintentar.SetActive(false);
    }

    public void IniciarRondas()
    {
        if (iniciado) return;
        iniciado = true;
        StartCoroutine(BucleRondas());
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (iniciado && rondaText != null && rondaActual > 0)
            rondaText.text = $"Ronda {rondaActual}/{totalRondas} - Monstruos: {Monster.Activos.Count}";
    }

    private IEnumerator BucleRondas()
    {
        for (int ronda = 1; ronda <= totalRondas; ronda++)
        {
            rondaActual = ronda;
            yield return Anunciar($"RONDA {ronda}", 2f);

            int cantidad = monstruosPrimeraRonda + (ronda - 1) * monstruosExtraPorRonda;
            SpawnearMonstruos(cantidad, ronda);

            // Esperar a que mueran todos o a que pierdas
            yield return null;
            while (Monster.Activos.Count > 0 && !(jugador != null && jugador.Muerto))
                yield return null;

            if (jugador != null && jugador.Muerto)
            {
                LimpiarMonstruos();
                SetAnuncio("¡Has perdido!");
                if (botonReintentar != null) botonReintentar.SetActive(true);
                yield break;
            }

            if (ronda < totalRondas)
                yield return Anunciar("¡Ronda superada!", pausaEntreRondas);
        }

        SetAnuncio("¡VICTORIA!");
    }

    private void SpawnearMonstruos(int cantidad, int ronda)
    {
        if (monsterPrefab == null || cam == null) return;

        // Ronda 1: el primer monstruo aparece frente a ti. Después, ángulo aleatorio.
        float anguloBase = cam.eulerAngles.y + (ronda == 1 ? 0f : Random.Range(0f, 360f));
        float paso = 360f / cantidad;

        for (int i = 0; i < cantidad; i++)
        {
            float angulo = anguloBase + i * paso + (cantidad > 1 ? Random.Range(-20f, 20f) : 0f);
            float radio = Random.Range(radioMin, radioMax);

            Vector3 offset = Quaternion.Euler(0f, angulo, 0f) * Vector3.forward * radio;
            Vector3 pos = new Vector3(cam.position.x + offset.x,
                                      cam.position.y - alturaBajoCamara,
                                      cam.position.z + offset.z);

            Instantiate(monsterPrefab, pos, Quaternion.identity);
        }
    }

    private void LimpiarMonstruos()
    {
        var copia = new System.Collections.Generic.List<Monster>(Monster.Activos);
        foreach (var m in copia)
            if (m != null) Destroy(m.gameObject);
    }

    private IEnumerator Anunciar(string texto, float segundos)
    {
        SetAnuncio(texto);
        yield return new WaitForSeconds(segundos);
        SetAnuncio("");
    }

    private void SetAnuncio(string texto)
    {
        if (anuncioText != null) anuncioText.text = texto;
    }

    public void Reintentar()
    {
        if (botonReintentar != null) botonReintentar.SetActive(false);

        StopAllCoroutines();
        LimpiarMonstruos();

        // Borrar balas que sigan en el aire
        foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        if (jugador != null) jugador.Reiniciar();

        SetAnuncio("");
        rondaActual = 0;
        iniciado = false;
        IniciarRondas();
    }
}