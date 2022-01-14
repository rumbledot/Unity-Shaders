using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem[] Particles;
    [SerializeField] float LifeTime = 0.1f;
    private float ParticleCount;

    private void Awake()
    {
        this.Particles = this.gameObject.GetComponents<ParticleSystem>();

        this.ParticleCount = this.Particles.Length;

        for (int i = 0; i < this.Particles.Length; i++)
        {
            Particles[i].Play();
        }
    }
}
