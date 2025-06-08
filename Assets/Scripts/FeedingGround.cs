using UnityEngine;

public class FeedingGround : MonoBehaviour
{
    public float beforeAte = 0;

    public readonly float reSpawnTime = 5f;
    private float healthAmount = 50f;
    public float HealthAmount { get { return healthAmount; } }


    void Start()
    {
        Debug.Log(gameObject.tag);
    }

    void Update()
    {
    }

    public bool Ate()
    {
        if (gameObject.activeSelf == false) return false;
        beforeAte = Time.time;
        gameObject.SetActive(false);
        Invoke(nameof(Reactivate), reSpawnTime);
        return true;
    }

    private void Reactivate()
    {
        gameObject.SetActive(true);
        Debug.Log("FeedingGroundが再度アクティブになりました");
    }
}
