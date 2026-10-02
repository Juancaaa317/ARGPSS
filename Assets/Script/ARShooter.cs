using UnityEngine;

public class ARShooter : MonoBehaviour
{
    [SerializeField] private Camera arCamera; // Tu cámara principal de AR
    [SerializeField] private int danoPorDisparo = 10;

    void Update()
    {
        // Detectamos si el jugador tocó la pantalla
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);

            // Solo disparamos en el momento exacto en que el dedo toca la pantalla
            if (toque.phase == TouchPhase.Began)
            {
                Disparar(toque.position);
            }
        }
    }

    private void Disparar(Vector2 posicionPantalla)
    {
        // Creamos un rayo que va desde donde tocaste en la pantalla hacia el mundo 3D
        Ray rayo = arCamera.ScreenPointToRay(posicionPantalla);
        RaycastHit golpe;

        // Lanzamos el rayo (hasta 100 metros de distancia)
        if (Physics.Raycast(rayo, out golpe, 100f))
        {
            // Verificamos si el objeto que golpeamos tiene el script "Monster"
            Monster monstruo = golpe.collider.GetComponent<Monster>();

            if (monstruo != null)
            {
                // Si es el monstruo, le hacemos daño
                monstruo.RecibirDano(danoPorDisparo);
            }
        }
    }
}