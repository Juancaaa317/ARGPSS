using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    [SerializeField] private RectTransform flecha;   // la Image de la flecha
    [SerializeField] private Camera arCamera;

    void Start()
    {
        if (arCamera == null) arCamera = Camera.main;
    }

    void Update()
    {
        if (flecha == null || arCamera == null) return;

        var monstruos = Monster.Activos;
        if (monstruos.Count == 0)
        {
            flecha.gameObject.SetActive(false);
            return;
        }

        Monster masCercano = null;
        float mejorDist = float.MaxValue;
        bool algunoVisible = false;

        foreach (var m in monstruos)
        {
            if (m == null) continue;

            Vector3 vp = arCamera.WorldToViewportPoint(m.transform.position);
            if (vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f)
                algunoVisible = true;

            float d = (m.transform.position - arCamera.transform.position).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; masCercano = m; }
        }

        if (algunoVisible || masCercano == null)
        {
            flecha.gameObject.SetActive(false);
            return;
        }

        flecha.gameObject.SetActive(true);

        // Dirección de la cámara en el plano horizontal
        Vector3 fwd = arCamera.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f)   // celular apuntando casi al piso/techo
        {
            fwd = arCamera.transform.up;
            fwd.y = 0f;
        }

        Vector3 haciaMonstruo = masCercano.transform.position - arCamera.transform.position;
        haciaMonstruo.y = 0f;

        float angulo = Vector3.SignedAngle(fwd, haciaMonstruo, Vector3.up);
        flecha.localEulerAngles = new Vector3(0f, 0f, -angulo);
    }
}