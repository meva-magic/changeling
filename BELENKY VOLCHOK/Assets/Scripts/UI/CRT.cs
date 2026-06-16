using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ForceCRTInBuild : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(EnableCRTAfterFrame());
    }
    
    private System.Collections.IEnumerator EnableCRTAfterFrame()
    {
        yield return null;
        
        var camera = GetComponent<Camera>();
        var urpCamera = camera.GetUniversalAdditionalCameraData();
        
        var renderer = urpCamera.scriptableRenderer;
        var featureType = typeof(ScriptableRendererFeature);
        
        var featuresField = typeof(ScriptableRenderer).GetField("m_RendererFeatures", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (featuresField != null)
        {
            var features = featuresField.GetValue(renderer) as System.Collections.IList;
            if (features != null)
            {
                foreach (var feature in features)
                {
                    if (feature != null && feature.GetType().Name.Contains("FullScreenPass"))
                    {
                        var setActiveMethod = feature.GetType().GetMethod("SetActive");
                        if (setActiveMethod != null)
                        {
                            setActiveMethod.Invoke(feature, new object[] { true });
                            Debug.Log("CRT Feature enabled in build!");
                        }
                        break;
                    }
                }
            }
        }
    }
}