using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portals;

static internal class AnimationCurves {
    public static AnimationCurve Overshoot = new AnimationCurve(new Keyframe[] {
        new Keyframe(0.0f, 0.0f, 0.0f, 0.0f),
        new Keyframe(0.9f, 1.1f, 0.0f, 0.0f),
        new Keyframe(1.0f, 1.0f, 0.0f, 0.0f),
    });
}

public class SpawnPortalOnClick : PortalSpawnerBase {

    [SerializeField] Camera _camera;
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] float _bulletSpawnOffset = 3.0f;
    [SerializeField] float _bulletSpeed = 20.0f;
    [SerializeField] GameObject _splashParticles;


    void Awake() {
        if (!isActiveAndEnabled) {
            return;
        }
    }

    void Start() {
        if (!isActiveAndEnabled) {
            return;
        }

        _leftPortal = SpawnPortal(Vector3.zero, Quaternion.identity, LeftPortalColor);
        _rightPortal = SpawnPortal(Vector3.zero, Quaternion.identity, RightPortalColor);

        _leftPortal.ExitPortal = _rightPortal;
        _rightPortal.ExitPortal = _leftPortal;

        _leftPortal.name = "Left Portal";
        _rightPortal.name = "Right Portal";

        _leftPortal.gameObject.SetActive(false);
        _rightPortal.gameObject.SetActive(false);

    }

    void Update () {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);
#if !UNITY_EDITOR
        leftClick = Input.GetButtonDown("Left Shoulder");
        rightClick = Input.GetButtonDown("Right Shoulder");
#endif
        if (leftClick || rightClick) {
            Polarity polarity = leftClick ? Polarity.Left : Polarity.Right;
            Fire(polarity);
        }
	}

    private enum Polarity {
        Left,
        Right
    }

    private void Fire(Polarity polarity) {
        Color color = polarity == Polarity.Left ? LeftPortalColor : RightPortalColor;
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        RaycastHit hit;
        bool hitWall;

        if (false) { //_shootThroughPortals
            hitWall = PortalPhysics.Raycast(ray, out hit, Mathf.Infinity, _canHit, QueryTriggerInteraction.Collide);
        } else {
            int mask = _canHit | PortalPhysics.PortalLayerMask;
            hitWall = Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Collide);
        }

        if (hitWall) {
            Portal portal = hit.collider.GetComponent<Portal>();
            if (portal) {
                //For this implementation we don't much care about fancy effects
                //WavePortalOverTime(portal, hit.point, _portalWaveAmplitude, _portalWaveDuration);
            } else {
                bool spawnedPortal = TrySpawnPortal(polarity == Polarity.Left, hit, _camera.transform.forward);
                if (!spawnedPortal) {
                    SpawnSplashParticles(hit.point, hit.normal, color);
                }
            }
        }
        
        // Spawn a bullet that will auto-destroy itself after it travels a certain distance
        if (_bulletPrefab)
            SpawnBullet(_bulletPrefab, _camera.transform.position + _camera.transform.forward * _bulletSpawnOffset, _camera.transform.forward, hit.distance, color);
    }    private Vector2 CalculatePortalUVSpacePoint(Portal portal, Vector3 pointWorldSpace) {
        // This calculation is specific to the portal mesh used at the time of writing
        return portal.transform.InverseTransformPoint(pointWorldSpace) + Vector3.one * 0.5f;
    }

    private Coroutine _wavingCoroutine;
    private void WavePortalOverTime(Portal portal, Vector3 originWorldSpace, float amplitude, float duration) {
        if (_wavingCoroutine != null) {
            StopCoroutine(_wavingCoroutine);
        }
        _wavingCoroutine = StartCoroutine(WavePortalOverTimeRoutine(portal, originWorldSpace, amplitude, duration));
    }

    IEnumerator WavePortalOverTimeRoutine(Portal portal, Vector3 originWorldSpace, float amplitude, float duration) {
        Vector2 originUVSpace = CalculatePortalUVSpacePoint(portal, originWorldSpace);
        portal.PortalRenderer.FrontFaceMaterial.SetVector("_WaveOrigin", originUVSpace);
        portal.PortalRenderer.BackFaceMaterial.SetVector("_WaveOrigin", originUVSpace);

        portal.PortalRenderer.FrontFaceMaterial.EnableKeyword("PORTAL_WAVING_ENABLED");
        portal.PortalRenderer.BackFaceMaterial.EnableKeyword("PORTAL_WAVING_ENABLED");

        float elapsed = 0;
        while (elapsed < duration) {
            float ratio = elapsed / duration;
            float amp = (1 - ratio) * amplitude; // Fade to zero over time
            portal.PortalRenderer.FrontFaceMaterial.SetFloat("_WaveAmplitude", amp);
            portal.PortalRenderer.BackFaceMaterial.SetFloat("_WaveAmplitude", amp);
            yield return null;
            elapsed += Time.deltaTime;
        }
        portal.PortalRenderer.FrontFaceMaterial.SetFloat("_WaveAmplitude", 0);
        portal.PortalRenderer.BackFaceMaterial.SetFloat("_WaveAmplitude", 0);

        portal.PortalRenderer.FrontFaceMaterial.DisableKeyword("PORTAL_WAVING_ENABLED");
        portal.PortalRenderer.BackFaceMaterial.DisableKeyword("PORTAL_WAVING_ENABLED");
    }

    void SpawnSplashParticles(Vector3 position, Vector3 direction, Color color) {
        if (!_splashParticles) { return; } //Don't do anything if we don't have a prefab
        GameObject obj = Instantiate(_splashParticles);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        ParticleSystem particles = obj.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;
    }

    GameObject SpawnBullet(GameObject prefab, Vector3 position, Vector3 direction, float distance, Color color) {
        GameObject bullet = Instantiate(prefab);
        bullet.transform.position = position;
        bullet.GetComponent<Rigidbody>().velocity = direction * _bulletSpeed;

        ParticleSystem particles = bullet.transform.Find("Trail").GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;

        float duration = distance / _bulletSpeed;

        StartCoroutine(DestroyBulletAfterTime(bullet, duration));
        return bullet;
    }


    IEnumerator DestroyBulletAfterTime(GameObject bullet, float duration) {
        yield return new WaitForSeconds(duration);
        bullet.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(bullet, 1.0f);
    }
}
