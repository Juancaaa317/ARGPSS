using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ARShooter : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private int danoPorDisparo = 10;

    private void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    private void Update()
    {
        if (TryGetTap(out Vector2 posicion))
            Disparar(posicion);
    }

    private bool TryGetTap(out Vector2 posicion)
    {
        posicion = default;

#if ENABLE_INPUT_SYSTEM
        var pantalla = Touchscreen.current;
        if (pantalla != null)
        {
            if (pantalla.primaryTouch.press.wasPressedThisFrame)
            {
                posicion = pantalla.primaryTouch.position.ReadValue();
                return true;
            }
            return false; // Input System activo: no consultar el legacy
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        posicion = Input.GetTouch(0).position;
        return true;
    }
#endif
        return false;
    }

    private void Disparar(Vector2 posicionPantalla)
    {
        Debug.Log("Toque detectado en " + posicionPantalla);

        if (arCamera == null)
        {
            Debug.LogWarning("ARShooter: no hay cámara asignada");
            return;
        }

        Ray rayo = arCamera.ScreenPointToRay(posicionPantalla);

        if (Physics.Raycast(rayo, out RaycastHit golpe, 100f))
        {
            Debug.Log("El rayo golpeó: " + golpe.collider.name);

            Monster monstruo = golpe.collider.GetComponentInParent<Monster>();
            if (monstruo != null)
                monstruo.RecibirDano(danoPorDisparo);
            else
                Debug.LogWarning("Ese objeto no tiene el script Monster");
        }
        else
        {
            Debug.Log("El rayo no golpeó nada");
        }
    }
}