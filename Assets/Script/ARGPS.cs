using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ARGPS : MonoBehaviour
{
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private GameObject prefab; // arrastra aquí el PREFAB (asset)

    [SerializeField] private Text distanceText;
    [SerializeField] private Text messageText;

    [SerializeField] private double targetLatitude;
    [SerializeField] private double targetLongitude;

    [SerializeField] private float discoveryDistance = 15f;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float heightBelowCamera = 1.2f;

    [SerializeField] private RoundManager roundManager;

    private bool relicDiscovered = false;
    private GameObject relicInstance;

    public double latitude;
    public double longitude;
    public double altitude;

    private void Awake()
    {
        if (sessionOrigin == null)
            sessionOrigin = FindAnyObjectByType<XROrigin>();

        if (messageText != null)
            messageText.text = "Busca la reliquia...";
    }

    private IEnumerator Start()
    {
        yield return AskPermission("android.permission.ACCESS_FINE_LOCATION");
        yield return AskPermission("android.permission.CAMERA");
        yield return new WaitForSeconds(0.5f);
        yield return UpdateGPS();
    }

    private IEnumerator AskPermission(string permission)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Permission.HasUserAuthorizedPermission(permission))
            yield break;

        bool answered = false;
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += _ => answered = true;
        callbacks.PermissionDenied += _ => answered = true;
        callbacks.PermissionDeniedAndDontAskAgain += _ => answered = true;

        Permission.RequestUserPermission(permission, callbacks);

        float timeout = 30f;
        while (!answered && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
#else
        yield break;
#endif
    }

    private IEnumerator UpdateGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            if (messageText != null)
                messageText.text = "Activa la ubicación del dispositivo";
            yield break;
        }

        Input.location.Start(5f, 0.5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            if (messageText != null)
                messageText.text = maxWait <= 0
                    ? "Tiempo agotado iniciando GPS"
                    : "No se pudo obtener la ubicación";
            yield break;
        }

        while (!relicDiscovered)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                var data = Input.location.lastData;
                latitude = data.latitude;
                longitude = data.longitude;
                altitude = data.altitude;

                double distance = CalculateDistance(latitude, longitude, targetLatitude, targetLongitude);

                if (distanceText != null)
                    distanceText.text = $"Distancia: {distance:F1} m (precisión ±{data.horizontalAccuracy:F0} m)";

                if (distance <= discoveryDistance)
                    DiscoverRelic((float)distance);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000.0;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private void DiscoverRelic(float distance)
    {
        relicDiscovered = true;

        if (messageText != null)
            messageText.text = "¡RELIQUIA ENCONTRADA!";

        Input.location.Stop();

        if (roundManager != null)
        {
            roundManager.IniciarRondas();
            return;
        }

        if (prefab == null || sessionOrigin == null || sessionOrigin.Camera == null)
            return;

        Transform cam = sessionOrigin.Camera.transform;

        Vector3 forward = cam.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 pos = cam.position + forward * spawnDistance;
        pos.y = cam.position.y - heightBelowCamera;

        // La reliquia mira hacia el jugador
        relicInstance = Instantiate(prefab, pos, Quaternion.LookRotation(-forward));

        Debug.Log($"Reliquia encontrada a {distance:F1} m. Spawn en {pos}");
    }

    private void OnDisable()
    {
        if (Input.location.status != LocationServiceStatus.Stopped)
            Input.location.Stop();
    }
}