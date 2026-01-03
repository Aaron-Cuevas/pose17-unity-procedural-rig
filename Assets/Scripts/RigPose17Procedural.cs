using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RigPose17Procedural : MonoBehaviour
{
    [Header("Archivo (StreamingAssets)")]
    public string nombreArchivo = "pose_test.csv";

    [Header("Reproducción")]
    public float fps = 30f;
    public bool reproducirAlIniciar = true;

    [Header("Unidades y ejes")]
    public float escala = 1f;                  // si viene en milímetros: 0.001
    public Vector3 signoEjes = Vector3.one;    // espejado: (-1,1,1) o inversión: (1,1,-1)

    [Header("Geometría")]
    public float radioArticulacion = 0.03f;
    public float grosorHueso = 0.02f;

    // Conexiones mínimas (COCO 17)
    static readonly (int a, int b)[] Huesos = new (int, int)[]
    {
        (5,7),(7,9),        // brazo izq
        (6,8),(8,10),       // brazo der
        (11,13),(13,15),    // pierna izq
        (12,14),(14,16),    // pierna der
        (5,6),(11,12),      // hombros y caderas
        (5,11),(6,12)       // tronco
    };

    List<Vector3[]> frames;
    Transform[] joints = new Transform[17];
    Transform[] boneObjs;

    int frameIndex = 0;
    bool playing = true;
    float acumulador = 0f;

    void Start()
    {
        playing = reproducirAlIniciar;

        string ruta = Path.Combine(Application.streamingAssetsPath, nombreArchivo);
        frames = PoseCsvLoader.LoadFrames(ruta);

        CrearArticulaciones();
        CrearHuesos();

        AplicarFrame(0);
    }

    void Update()
    {
        if (frames == null || frames.Count == 0) return;

        // Controles: espacio (play/pause), flechas (paso a paso)
        if (WasPressedSpace()) playing = !playing;

        if (WasPressedLeft())
        {
            playing = false;
            Paso(-1);
        }
        if (WasPressedRight())
        {
            playing = false;
            Paso(+1);
        }

        if (!playing) return;

        acumulador += Time.deltaTime;
        float dt = 1f / Mathf.Max(1e-6f, fps);

        while (acumulador >= dt)
        {
            acumulador -= dt;
            Paso(+1);
        }
    }

    void Paso(int delta)
    {
        frameIndex = (frameIndex + delta) % frames.Count;
        if (frameIndex < 0) frameIndex += frames.Count;
        AplicarFrame(frameIndex);
    }

    void AplicarFrame(int idxFrame)
    {
        var f = frames[idxFrame];

        // Articulaciones
        for (int i = 0; i < 17; i++)
        {
            Vector3 v = Vector3.Scale(f[i] * escala, signoEjes);
            joints[i].localPosition = v;
        }

        // Huesos (cilindros). El cilindro en Unity apunta por defecto en Y.
        for (int i = 0; i < Huesos.Length; i++)
        {
            int a = Huesos[i].a;
            int b = Huesos[i].b;

            Vector3 pa = joints[a].position;
            Vector3 pb = joints[b].position;

            Vector3 dir = pb - pa;
            float len = dir.magnitude;

            if (len < 1e-6f)
            {
                boneObjs[i].gameObject.SetActive(false);
                continue;
            }
            boneObjs[i].gameObject.SetActive(true);

            Vector3 mid = (pa + pb) * 0.5f;
            boneObjs[i].position = mid;
            boneObjs[i].rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);

            // Altura del cilindro por defecto = 2, así que escalaY = len/2
            boneObjs[i].localScale = new Vector3(grosorHueso, len * 0.5f, grosorHueso);
        }
    }

    void CrearArticulaciones()
    {
        for (int i = 0; i < 17; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"J{i:00}";
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * (radioArticulacion * 2f);

            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            joints[i] = go.transform;
        }
    }

    void CrearHuesos()
    {
        boneObjs = new Transform[Huesos.Length];
        for (int i = 0; i < Huesos.Length; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"Hueso{i:00}_{Huesos[i].a}-{Huesos[i].b}";
            go.transform.SetParent(transform, false);

            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            boneObjs[i] = go.transform;
        }
    }

    // --- Entrada compatible (Input System / Input Manager) ---

    bool WasPressedSpace()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }

    bool WasPressedLeft()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.leftArrowKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.LeftArrow);
#else
        return false;
#endif
    }

    bool WasPressedRight()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.rightArrowKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.RightArrow);
#else
        return false;
#endif
    }
}
