using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AnimasyonKurulumEditor : EditorWindow
{
    private struct Kayit
    {
        public MonoBehaviour script;
        public SerializedObject so;
        public SerializedProperty sp;
        public string goName;
        public string tipAdi;
    }

    private static readonly Type[] HedefTipler =
    {
        typeof(MusteriHareket),
        typeof(UrunTasiyici),
        typeof(KahveTasiyicisiNPC),
        typeof(KasiyerHareket),
        typeof(SodaTasiyici),
        typeof(DepocuCalisan),
    };

    private List<Kayit> kayitlar = new List<Kayit>();
    private Vector2 scroll;

    [MenuItem("Araçlar/Animasyon Kurulumu")]
    static void Ac() => GetWindow<AnimasyonKurulumEditor>("Animasyon Kurulumu").Tara();

    void OnEnable() => Tara();

    void Tara()
    {
        kayitlar.Clear();
        foreach (Type tip in HedefTipler)
        {
            // modelTransform alanı bu tipte tanımlı mı kontrol et
            FieldInfo field = tip.GetField("modelTransform", BindingFlags.Public | BindingFlags.Instance);
            if (field == null) continue;

            UnityEngine.Object[] bulunanlar = FindObjectsOfType(tip);
            foreach (UnityEngine.Object obj in bulunanlar)
            {
                MonoBehaviour mb = obj as MonoBehaviour;
                if (mb == null) continue;
                SerializedObject so = new SerializedObject(mb);
                SerializedProperty sp = so.FindProperty("modelTransform");
                if (sp == null) continue;
                kayitlar.Add(new Kayit
                {
                    script  = mb,
                    so      = so,
                    sp      = sp,
                    goName  = mb.gameObject.name,
                    tipAdi  = tip.Name,
                });
            }
        }
    }

    // Görsel mesh'i taşıyan child transform'u bul
    static Transform BulModelTransform(GameObject go)
    {
        // Önce SkinnedMeshRenderer'lı child'a bak
        SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null && smr.transform != go.transform)
            return smr.transform;

        // Sonra direkt child'larda MeshRenderer ara
        foreach (Transform child in go.transform)
        {
            if (child.GetComponent<MeshRenderer>() != null)
                return child;
        }

        // Bulamazsa kendi transform'unu döndür
        return go.transform;
    }

    void OnGUI()
    {
        EditorGUILayout.Space(6);

        // Üst butonlar
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sahneyi Tara", GUILayout.Height(30)))
            Tara();
        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("Tümünü Otomatik Ata", GUILayout.Height(30)))
            TumunuAta();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField($"Bulunan karakter: {kayitlar.Count}", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (kayitlar.Count == 0)
        {
            EditorGUILayout.HelpBox("Sahnede ilgili script bulunamadı. Önce sahneyi aç, sonra 'Sahneyi Tara' butonuna bas.", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (Kayit k in kayitlar)
        {
            if (k.script == null) continue;

            k.so.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            // Başlık satırı
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(k.goName, EditorStyles.boldLabel, GUILayout.ExpandWidth(true)))
                Selection.activeGameObject = k.script.gameObject;
            EditorGUILayout.LabelField(k.tipAdi, EditorStyles.miniLabel, GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();

            // Model Transform alanı
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(k.sp, new GUIContent("Model Transform"));
            if (EditorGUI.EndChangeCheck())
            {
                k.so.ApplyModifiedProperties();
                EditorUtility.SetDirty(k.script);
            }

            // Otomatik bul butonu
            if (GUILayout.Button("Otomatik Bul", GUILayout.Width(100), GUILayout.Height(18)))
            {
                Transform bulunan = BulModelTransform(k.script.gameObject);
                k.sp.objectReferenceValue = bulunan;
                k.so.ApplyModifiedProperties();
                EditorUtility.SetDirty(k.script);
            }
            EditorGUILayout.EndHorizontal();

            // Uyarı: atanmamış
            if (k.sp.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Model Transform atanmamış!", MessageType.Warning);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();
    }

    void TumunuAta()
    {
        int sayac = 0;
        foreach (Kayit k in kayitlar)
        {
            if (k.script == null) continue;
            k.so.Update();
            k.sp.objectReferenceValue = BulModelTransform(k.script.gameObject);
            k.so.ApplyModifiedProperties();
            EditorUtility.SetDirty(k.script);
            sayac++;
        }
        Debug.Log($"[AnimasyonKurulum] {sayac} karakter için modelTransform atandı.");
    }
}
