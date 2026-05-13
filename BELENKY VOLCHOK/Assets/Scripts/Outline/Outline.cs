using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    public Color OutlineColor = Color.white;
    [Range(0.01f, 1f)]
    public float OutlineWidth = 0.1f;

    private List<GameObject> outlineObjects = new List<GameObject>();
    private Material outlineMaterial;
    private bool isSetup = false;

    void Awake()
    {
        Setup();
    }

    void Setup()
    {
        if (isSetup) return;
        isSetup = true;
        
        CreateOutlineMaterial();
        CreateOutlineObjects();
        
        foreach (GameObject obj in outlineObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void CreateOutlineMaterial()
    {
        Shader shader = Shader.Find("Custom/SimpleOutline");
        if (shader == null) return;
        outlineMaterial = new Material(shader);
    }

    void CreateOutlineObjects()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Outline")
            {
                Destroy(child.gameObject);
            }
        }
        
        outlineObjects.Clear();
        
        MeshFilter[] allMeshFilters = GetComponentsInChildren<MeshFilter>(true);
        
        foreach (MeshFilter mf in allMeshFilters)
        {
            if (mf.gameObject.name == "Outline") continue;
            if (mf.sharedMesh == null) continue;

            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(mf.transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;
            
            MeshFilter outlineMF = outlineObj.AddComponent<MeshFilter>();
            outlineMF.sharedMesh = mf.sharedMesh;
            
            MeshRenderer outlineMR = outlineObj.AddComponent<MeshRenderer>();
            
            Material mat = new Material(outlineMaterial);
            mat.renderQueue = 3000;
            outlineMR.material = mat;
            outlineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineMR.receiveShadows = false;
            
            outlineObjects.Add(outlineObj);
        }
    }

    void Update()
    {
        foreach (GameObject obj in outlineObjects)
        {
            if (obj == null) continue;
            
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr != null && mr.material != null)
            {
                mr.material.SetColor("_OutlineColor", OutlineColor);
                
                // Calculate scale-adjusted width
                Transform parent = obj.transform.parent;
                if (parent != null)
                {
                    // Get the largest scale axis to normalize
                    Vector3 lossyScale = parent.lossyScale;
                    float maxScale = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
                    
                    // Divide width by scale so it looks consistent
                    float adjustedWidth = OutlineWidth / maxScale;
                    mr.material.SetFloat("_OutlineWidth", adjustedWidth);
                }
                else
                {
                    mr.material.SetFloat("_OutlineWidth", OutlineWidth);
                }
            }
        }
    }

    void OnEnable()
    {
        foreach (GameObject obj in outlineObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    void OnDisable()
    {
        foreach (GameObject obj in outlineObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void OnDestroy()
    {
        foreach (GameObject obj in outlineObjects)
        {
            if (obj != null) Destroy(obj);
        }
        outlineObjects.Clear();
        
        if (outlineMaterial != null) Destroy(outlineMaterial);
    }
}