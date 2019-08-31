using UnityEngine;
using Valve.VR;

namespace Nokobot.Assets.Crossbow
{
    public class CrossbowController : MonoBehaviour
    {
        public GameObject arrowPrefab;
        public Transform arrowLocation;


        private SoundManager sm;

        public float launchPower = 100000.0f;

        public float fireRate = 0.1f;
        private float untilNextFire = 0.0f;

        public SteamVR_Action_Boolean shootAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerAction");

        private AudioSource arrowShootSource;

        private int arrowShots = 0;
        bool shootHintEnabled = true;

        public delegate void ArrowShots();
        public static event ArrowShots OnArrowShots;

        void Start()
        {
            if (arrowLocation == null)
                arrowLocation = transform;

            sm = GameObject.Find("GameManager").GetComponent<SoundManager>();

            arrowShootSource = gameObject.AddComponent<AudioSource>();

        }

        void Update()
        {
            untilNextFire += Time.deltaTime;
            if (shootAction.GetStateDown(SteamVR_Input_Sources.RightHand) 
                && (untilNextFire >= fireRate))
            {
                untilNextFire = 0.0f;
                GameObject arrow = Instantiate(arrowPrefab, arrowLocation.position, arrowLocation.rotation);
                arrow.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * launchPower);
                arrowShootSource.PlayOneShot(sm.arrowShoot, sm.volume[sm.arrowShoot]);
                arrowShots++;
                if (arrowShots >= 5 && shootHintEnabled)
                {
                    shootHintEnabled = false;
                    OnArrowShots?.Invoke();
                }
            }
        }
    }
}
