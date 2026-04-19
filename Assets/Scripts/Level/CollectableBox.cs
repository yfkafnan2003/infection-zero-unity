using UnityEngine;

public class CollectableBox : MonoBehaviour
{
    private RetrieveBoxManager boxManager;
    private int boxId;
    public AudioSource audioSource;
    public AudioClip collectSound;
    
    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    public void Setup(RetrieveBoxManager manager, int id)
    {
        boxManager = manager;
        boxId = id;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play sound first (before destroying)
            if (audioSource != null && collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            
            // Then collect/destroy
            if (boxManager != null)
            {
                boxManager.CollectBox(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}