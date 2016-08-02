using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]

public class ParticleLight : MonoBehaviour {

    public Light lightPrefab;

    ParticleSystem _system;
    ParticleSystem.Particle[] _particles;
    Light[] _lights;


    void Start()
    {
        _system = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_system.maxParticles];

        _lights = new Light[_system.maxParticles];
        for (int i = 0; i < _lights.Length; i++)
        {
            _lights[i] = (Light)Instantiate(lightPrefab);
            _lights[i].transform.parent = transform;
        }
    }

    void Update()
    {

        int count = _system.GetParticles(_particles);

        for (int i = 0; i < _lights.Length; i++)
        {
            _lights[i].intensity = 0;

        }

        for (int i = 0; i < count; i++)
        {
            _lights[i].gameObject.SetActive(true);
            _lights[i].intensity = .55f;
            _lights[i].transform.position = _particles[i].position;
            //_lights[i].color = _particles[i].GetCurrentColor(_system);
        }

        for (int i = count; i < _particles.Length; i++)
        {
            _lights[i].gameObject.SetActive(false);
        }
    }
}
