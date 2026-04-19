    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;


    public class MinimapRadar : MonoBehaviour
    {
        [Header("Radar Settings")]
        public Transform playerTransform;
        public float radarRadius = 30f;
        public float updateRate = 0.1f;
        
        [Header("UI References")]
        public RectTransform radarPanel;
        public GameObject playerDotPrefab;
        public GameObject enemyDotPrefab;
        public float dotSize = 10f;
        
        [Header("Colors")]
        public Color playerColor = Color.green;
        public Color enemyColor = Color.red;
        
        [Header("Rotation")]
        public bool rotateRadar = true;
        
        private GameObject playerDot;
        private List<GameObject> enemyDots = new List<GameObject>();
        private List<Transform> activeEnemies = new List<Transform>();
        
        void Start()
        {
            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }
            
            // Create player dot
            if (playerDotPrefab != null && radarPanel != null)
            {
                playerDot = Instantiate(playerDotPrefab, radarPanel);
                playerDot.GetComponent<Image>().color = playerColor;
                RectTransform rect = playerDot.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(dotSize, dotSize);
                rect.anchoredPosition = Vector2.zero;
            }
            
            // Start updating
            InvokeRepeating("UpdateRadar", 0f, updateRate);
        }
        
        void Update()
        {
            // Continuously rotate the radar panel to follow player
            if (rotateRadar && playerTransform != null && radarPanel != null)
            {
                float playerAngle = playerTransform.eulerAngles.y;
                radarPanel.rotation = Quaternion.Euler(0, 0, playerAngle);
            }
        }
        
        void UpdateRadar()
        {
            if (playerTransform == null || radarPanel == null) return;
            
            // Find all enemies with tag "Enemy"
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // Clear dead enemies from list
            activeEnemies.Clear();
            foreach (GameObject enemy in enemies)
            {
                ZombieHealth health = enemy.GetComponent<ZombieHealth>();
                if (health != null && !health.IsDead())
                {
                    activeEnemies.Add(enemy.transform);
                }
            }
            
            // Remove dots for dead enemies
            for (int i = enemyDots.Count - 1; i >= 0; i--)
            {
                if (enemyDots[i] == null)
                {
                    enemyDots.RemoveAt(i);
                }
            }
            
            // Create or update enemy dots
            foreach (Transform enemy in activeEnemies)
            {
                bool found = false;
                
                // Check if dot already exists for this enemy
                foreach (GameObject dot in enemyDots)
                {
                    if (dot != null && dot.GetComponent<RadarDot>() != null && 
                        dot.GetComponent<RadarDot>().target == enemy)
                    {
                        found = true;
                        UpdateDotPosition(dot, enemy.position);
                        break;
                    }
                }
                
                // Create new dot if needed
                if (!found && Vector3.Distance(playerTransform.position, enemy.position) <= radarRadius)
                {
                    CreateEnemyDot(enemy);
                }
            }
            
            // Remove dots for enemies out of range
            for (int i = enemyDots.Count - 1; i >= 0; i--)
            {
                if (enemyDots[i] != null)
                {
                    RadarDot dot = enemyDots[i].GetComponent<RadarDot>();
                    if (dot != null && dot.target != null)
                    {
                        float distance = Vector3.Distance(playerTransform.position, dot.target.position);
                        if (distance > radarRadius)
                        {
                            Destroy(enemyDots[i]);
                            enemyDots.RemoveAt(i);
                        }
                    }
                    else
                    {
                        Destroy(enemyDots[i]);
                        enemyDots.RemoveAt(i);
                    }
                }
                else
                {
                    enemyDots.RemoveAt(i);
                }
            }
        }
        
        void CreateEnemyDot(Transform enemy)
        {
            if (enemyDotPrefab == null || radarPanel == null) return;
            
            GameObject dot = Instantiate(enemyDotPrefab, radarPanel);
            dot.GetComponent<Image>().color = enemyColor;
            
            RadarDot radarDot = dot.AddComponent<RadarDot>();
            radarDot.target = enemy;
            
            RectTransform rect = dot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(dotSize, dotSize);
            
            UpdateDotPosition(dot, enemy.position);
            enemyDots.Add(dot);
            
            // Register the radar dot with the zombie
            ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();
            if (zombieHealth != null)
            {
                zombieHealth.SetRadarDot(dot);
            }
        }
        
        void UpdateDotPosition(GameObject dot, Vector3 worldPosition)
        {
            if (playerTransform == null || radarPanel == null) return;
            
            // Get direction from player to enemy
            Vector3 direction = worldPosition - playerTransform.position;
            
            // Calculate distance
            float distance = direction.magnitude;
            if (distance > radarRadius) return;
            
            // Calculate relative position in world space (XZ plane)
            float relativeX = direction.x;
            float relativeZ = direction.z;
            
            // Normalize to radar radius
            float normalizedX = relativeX / radarRadius;
            float normalizedZ = relativeZ / radarRadius;
            
            // Get radar panel size
            float radarSize = radarPanel.rect.width;
            

            Vector2 uiPosition = new Vector2(normalizedX * radarSize / 2, normalizedZ * radarSize / 2);
            
            // Set dot position
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchoredPosition = uiPosition;
            
            // Optional: Rotate enemy dot to face away from center
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            dotRect.rotation = Quaternion.Euler(0, 0, -angle);
        }
        public void UpdateCustomDotPosition(GameObject dot, Vector3 worldPosition)
        {
            if (playerTransform == null || radarPanel == null) return;
            
            // Get direction from player to target
            Vector3 direction = worldPosition - playerTransform.position;
            
            // Calculate distance
            float distance = direction.magnitude;
            
            // Calculate relative position in world space (XZ plane)
            float relativeX = direction.x;
            float relativeZ = direction.z;
            
            // Normalize to radar radius
            float normalizedX = relativeX / radarRadius;
            float normalizedZ = relativeZ / radarRadius;
            
            // Calculate distance on radar (0 to 1)
            float radarDistance = Mathf.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ);
            
            // If target is outside radar range, clamp to edge
            if (distance > radarRadius)
            {
                // Clamp to the edge of the radar circle
                normalizedX /= radarDistance;
                normalizedZ /= radarDistance;
                radarDistance = 1f;
            }
            
            // Get radar panel size
            float radarSize = radarPanel.rect.width;
            
            Vector2 uiPosition = new Vector2(normalizedX * radarSize / 2, normalizedZ * radarSize / 2);
            
            // Set dot position
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchoredPosition = uiPosition;
        }
    }

    public class RadarDot : MonoBehaviour
    {
        public Transform target;
        public bool alwaysShow = false; // For boxes that show even outside range
    }