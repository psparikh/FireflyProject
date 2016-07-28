using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightBeam : MonoBehaviour
{
    public Valve.VR.EVRButtonId buttonId = Valve.VR.EVRButtonId.k_EButton_Axis0;

    public GameObject laserPrefab;
    public GameObject reticlePrefab;
    public Transform player;

    private Reticle reticle;
    private Laser laser;

    public float range = 20f;

    public Color enabledColor = Color.white;
    public Color disabledColor = Color.red;

    private SteamVR_TrackedObject controller;

    private RaycastHit target;
    private bool canBeam;

    // Use this for initialization
    void Start()
    {

        GameObject laserObj = (GameObject)Instantiate(laserPrefab);
        GameObject reticleObj = (GameObject)Instantiate(reticlePrefab);

        laserObj.transform.SetParent(player);
        reticleObj.transform.SetParent(player);

        reticle = reticleObj.GetComponent<Reticle>();
        laser = laserObj.GetComponent<Laser>();

        controller = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {

        SteamVR_Controller.Device device = SteamVR_Controller.Input((int)controller.index);

        if (device.GetPress(buttonId))
        {

            canBeam = false;

            laser.gameObject.SetActive(true);
            reticle.gameObject.SetActive(true);

            List<Vector3> waypoints = new List<Vector3>();

            if (CanBeam(ref waypoints, ref target))
            {
                canBeam = true;
                reticle.transform.up = target.normal;
            }

            if (waypoints.Count > 0)
            {
                reticle.transform.position = waypoints[waypoints.Count - 1];
            }

            laser.SetWaypoints(waypoints.ToArray());
            Color color = canBeam ? enabledColor : disabledColor;
            laser.SetColor(color);
            reticle.SetColor(color);
        }
        else
        {
            laser.gameObject.SetActive(false);
            reticle.gameObject.SetActive(false);
        }

        if (device.GetPressUp(buttonId) && canBeam)
        {
            StartCoroutine(Beam());
        }
    }

    /// <summary>
    /// Can Beam
    /// </summary>
    /// <param name="waypoints"></param>
    /// <param name="final"></param>
    /// <returns></returns>
    private bool CanBeam(ref List<Vector3> waypoints, ref RaycastHit final) {

          Ray ray = new Ray(transform.position, transform.forward);
        waypoints.Add(transform.position);

          return CanBeamHelper(ref waypoints, ref final, range, ray, false);
    }

    /// <summary>
    /// Can Beam Helper
    /// </summary>
    /// <param name="waypoints"></param>
    /// <param name="final"></param>
    /// <param name="rangeRemaining"></param>
    /// <param name="ray"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    private bool CanBeamHelper(ref List<Vector3> waypoints, ref RaycastHit final, float rangeRemaining, Ray ray, bool success)
    {

        if (rangeRemaining <= 0 || success)
        {
            return success;
        }

        RaycastHit hit;

        Vector3 waypoint = ray.origin + ray.direction * rangeRemaining;

        if (Physics.Raycast(ray, out hit, rangeRemaining, 1, QueryTriggerInteraction.Ignore))
        {

            final = hit;
            waypoint = final.point;

            rangeRemaining -= (ray.origin - hit.point).magnitude;

            if (hit.collider.CompareTag("lamp"))
            {
                rangeRemaining = 0;
                success = true;
            }

            else
            {
                rangeRemaining = 0;
            }
        }
        else
        {
            rangeRemaining = 0;
        }

        waypoints.Add(waypoint);

        return CanBeamHelper(ref waypoints, ref final, rangeRemaining, ray, success);
    }

    /// <summary>
    /// Beam
    /// </summary>
    /// <returns></returns>
    private IEnumerator Beam()
    {
        target.transform.GetComponent<LampController>().Activate( true );
        yield return null;
    }


}