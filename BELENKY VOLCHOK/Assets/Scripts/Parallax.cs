using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        [Range(0f, 1f)] public float depth = 0.5f;  // 0 = дальний, 1 = ближний
        [HideInInspector] public Vector3 startPosition;
    }
    
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private Transform player;
    [SerializeField] private float globalStrength = 0.15f;  // Общий множитель для всех слоёв
    [SerializeField] private float maxParallaxSpeed = 0.3f;  // Максимальная скорость параллакса
    
    private Vector3 playerStartPosition;
    
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        playerStartPosition = player.position;
        
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
                layer.startPosition = layer.layerTransform.position;
        }
    }
    
    void Update()
    {
        Vector3 playerDelta = player.position - playerStartPosition;
        
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                // depth=0 (дальний) → медленный, depth=1 (ближний) → быстрый
                float speed = Mathf.Lerp(0.02f, maxParallaxSpeed, layer.depth) * globalStrength;
                
                layer.layerTransform.position = layer.startPosition + new Vector3(
                    -playerDelta.x * speed,
                    -playerDelta.y * speed * 0.5f,  // вертикаль ещё слабее
                    0
                );
            }
        }
    }
}