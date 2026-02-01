using UnityEngine;

public class AutoDestroyParticles : MonoBehaviour
{
    [SerializeField] private float extraLifetime = 0.25f;

    private ParticleSystem[] systems;
    private float killTime;

    private void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);

        float max = 0f;
        foreach (var ps in systems)
        {
            var main = ps.main;
            float life = main.duration + main.startLifetime.constantMax;
            if (life > max) max = life;
        }

        killTime = Time.time + max + extraLifetime;
    }

    public void Play()
    {
        foreach (var ps in systems)
            ps.Play(true);
    }

    private void Update()
    {
        if (Time.time >= killTime)
            Destroy(gameObject);
    }
}
