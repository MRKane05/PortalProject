using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portals {
    public class GrabHandler : MonoBehaviour {
        #region Private Members
        // Anchor point that IS NOT affected by portals
        [SerializeField]
        private Transform _staticAnchor;
        public Transform _staticAnchorClone;

        // Anchor point that IS affected by portals
        //[SerializeField]
        //private Rigidbody _floatingAnchor;
        [SerializeField]
        private float _pickupRange = 2.0f;
        [SerializeField]
        private LayerMask _carryLayer;
        public LayerMask portalGrabLayer;

        private Camera _camera;
        private ConfigurableJoint _joint;
        private Portal _currentObjectPortal;
        private static GrabHandler _instance;
        #endregion

        float holdDistance = 2f;
        public LayerMask traceLayer;
        RigidbodyConstraints grabbedObjectInitialConstraints;
        float maxSqrCarryDistance = 1.5f;  //We'll drop what we're carrying if the distance from the intended point is more than this
        #region Public Members
        public static GrabHandler instance {
            get {
                if (!_instance)
                    _instance = GameObject.FindObjectOfType<GrabHandler>();
                return _instance;
            }
        }

        public delegate void GrabEvent(GrabHandler grabHandler, GameObject obj);
        public event GrabEvent onObjectGrabbed;
        public event GrabEvent onObjectReleased;

        public GameObject heldObject;
        #endregion

        void Awake()
        {
            _camera = Camera.main;
            heldObject = null;
            holdDistance = Vector3.Distance(Camera.main.transform.position, _staticAnchor.transform.position);
        }

        void OnEnable() {
            //Portal.onPortalTeleportGlobal += HandleObjectCarriedThroughPortal;
        }

        void OnDisable() {
            //Portal.onPortalTeleportGlobal -= HandleObjectCarriedThroughPortal;
        }

        void HandleObjectCarriedThroughPortal(Portal portal, GameObject obj) {
            if (obj == this.gameObject) {
                // Player exited portal
                if (_currentObjectPortal || heldObject == null) {
                    // Current object already on other side, reset
                    _currentObjectPortal = null;
                } else {
                    // Current object on previous side, use new positioning
                    _currentObjectPortal = portal.ExitPortal;
                }
            } else if (obj == heldObject) {
                // Current object exited portal
                if (_currentObjectPortal) {
                    // Player already on this side, reset
                    _currentObjectPortal = null;
                } else {
                    // Player on opposite side
                    _currentObjectPortal = portal;
                }

                // Multiply joint values by portal scale
                float scaleMultiplier = portal.PortalScaleAverage;

                SoftJointLimit softJointLimit = _joint.linearLimit;
                softJointLimit.limit *= scaleMultiplier;
                _joint.linearLimit = softJointLimit;

                float cubeScaleMultiplier = scaleMultiplier * scaleMultiplier * scaleMultiplier;
                JointDrive linearDrive = _joint.xDrive;
                linearDrive.maximumForce *= cubeScaleMultiplier;
                linearDrive.positionDamper *= cubeScaleMultiplier;
                linearDrive.positionSpring *= cubeScaleMultiplier;
                _joint.xDrive = linearDrive;
                _joint.yDrive = linearDrive;
                _joint.zDrive = linearDrive;

                // Disable joint for one frame so it doesn't freak out
                _joint.gameObject.SetActive(false);
            }
        }

        void GrabObject(GameObject obj) {
            Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                Debug.Log("Cannot grab object without a Rigidbody");
                return;
            }

            if (obj.tag != "Interactable")
            {
                Debug.Log("Cannot grab object that's not tagged as being interactable");
                return;
            }

            if (obj.name.Contains("(Clone)")) {   //We don't want to grab the clone, but instead grab the original
                string objectName = obj.name.Replace("(Clone)", "");
                obj = GameObject.Find(objectName);
                rigidbody = obj.GetComponent<Rigidbody>();
                //Debug.LogError("Opted to get original instead of Clone");
            }

            rigidbody.useGravity = false;
            rigidbody.drag = 20f;
            grabbedObjectInitialConstraints = rigidbody.constraints;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            //_joint.connectedBody = rigidbody;
            heldObject = obj;

            if (onObjectGrabbed != null) {
                onObjectGrabbed(this, obj);
            }
        }

        void ReleaseObject() {

            GameObject obj = heldObject;
            Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                Debug.LogError("Cannot release object without a Rigidbody");
                return;
            }

            rigidbody.drag = 0f;
            rigidbody.useGravity = true;
            rigidbody.constraints = grabbedObjectInitialConstraints;

            //_joint.connectedBody = null;
            _currentObjectPortal = null;
            heldObject = null;

            if (onObjectReleased != null) {
                onObjectReleased(this, obj);
            }
        }

        public bool CarryingObject() {
            return heldObject != null;
        }

        public float carryForce = 4f;


        Vector3 cloneholdPosition = Vector3.zero;
        int iPortalCarry = 0; //0: Ordinary carry, 1: carry in portal, 2: transitioned from carrying in portal this tick
        public void CarryObject()
        {
            if (heldObject)
            {

                bool bObjectObstructed = bCarriedObjectObstructed();

                if (bObjectObstructed)
                {
                    ReleaseObject();
                    return;
                }
                //See if we can get a point reflected in a portal
                RaycastHit hit;
                Ray newRay = new Ray(Camera.main.transform.position, (_staticAnchor.transform.position - Camera.main.transform.position).normalized);

                bool bCanHoldInPortal = false;
                if (Physics.Raycast(newRay, out hit, holdDistance * 1.5f, traceLayer))  //Detect if we're looking into a portal
                {
                    //we've got a hit so project the position of this object through the portal
                    Portal hitPortal = hit.collider.gameObject.GetComponent<Portal>();
                    if (hitPortal)
                    {
                        cloneholdPosition = hitPortal.TeleportPoint(_staticAnchor.transform.position);
                        _staticAnchorClone.transform.position = cloneholdPosition;
                        bCanHoldInPortal = true;
                    } else
                    {
                        cloneholdPosition = Vector3.one * 10000f;
                    }
                } else
                {
                    //If this happens after the above there's a possibility we should drop our item...
                    //cloneholdPosition = Vector3.one * 10000f;   // gameObject.transform.position;  //We need some way to nullify this...
                    _staticAnchorClone.transform.position = _staticAnchor.transform.position;

                }

                
                if (Vector3.Distance(heldObject.transform.position, _staticAnchor.transform.position) < 0.1f || Vector3.Distance(heldObject.transform.position, cloneholdPosition) < 0.1f)
                {
                    //We've no need to move this
                } else
                {
                    //See if we should move to our clone position as it's closest for the hold
                    Vector3 forceDirection = Vector3.zero;
                    if (Vector3.SqrMagnitude(_staticAnchor.transform.position - heldObject.transform.position) < Vector3.SqrMagnitude(cloneholdPosition - heldObject.transform.position)) {
                        forceDirection = _staticAnchor.transform.position - heldObject.transform.position;
                        heldObject.GetComponent<Rigidbody>().AddForce(forceDirection * carryForce);
                    } else
                    {
                        if (bCanHoldInPortal)
                        {
                            forceDirection = cloneholdPosition - heldObject.transform.position;
                            heldObject.GetComponent<Rigidbody>().AddForce(forceDirection * carryForce);
                        }
                        else
                        {
                            ReleaseObject();
                        }
                    }                    
                }
            }
        }

        private const float PortalThroughOffset = 0.01f;
        public void Grab() {
            if (!CarryingObject()) {
                RaycastHit hit;
                if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _pickupRange, _carryLayer, QueryTriggerInteraction.UseGlobal)) {
                    GameObject obj = hit.collider.gameObject;
                    if (hit.collider.gameObject.GetComponent<Portal>())
                    {
                        //See if we're grabbing something through the portal
                        RaycastHit portalHit;
                        Portal portal = hit.collider.GetComponent<Portal>();
                        if (!portal)
                        {
                            string msg = string.Format("{0} is on Portal layer, but is not a portal", hit.collider.gameObject);
                            throw new System.Exception(msg);
                        }

                        Matrix4x4 portalMatrix = portal.PortalMatrix;
                        Vector3 newDirection = portalMatrix.MultiplyVector(_camera.transform.forward);
                        // Offset by Epsilon so we can't hit the exit portal on our way out
                        Vector3 newOrigin = portalMatrix.MultiplyPoint3x4(hit.point) + newDirection * PortalThroughOffset;
                        float newDistance = _pickupRange - hit.distance - PortalThroughOffset;


                        if (Physics.Raycast(newOrigin, newDirection, out portalHit, newDistance, portalGrabLayer, QueryTriggerInteraction.UseGlobal))
                        {
                            if (portalHit.collider.gameObject)
                            {
                                GrabObject(portalHit.collider.gameObject);
                            }
                        }
                    }
                    else //Try to do a "normal" grab
                    {
                        GrabObject(obj);
                    }
                }
            } else {
                Debug.LogError("Grab() called while already holding an object");
            }
        }

        bool bCarriedObjectObstructed()
        {
            RaycastHit hit;
            Vector3 carryDir = heldObject.transform.position - Camera.main.transform.position;
  
            float carryDistance = carryDir.magnitude;

            if (Physics.Raycast(_camera.transform.position, carryDir.normalized, out hit, carryDistance, _carryLayer, QueryTriggerInteraction.UseGlobal))
            {
                GameObject obj = hit.collider.gameObject;
                if (hit.collider.gameObject.GetComponent<Portal>())
                {
                    //See if we're grabbing something through the portal
                    RaycastHit portalHit;
                    Portal portal = hit.collider.GetComponent<Portal>();
                    if (!portal)
                    {
                        string msg = string.Format("{0} is on Portal layer, but is not a portal", hit.collider.gameObject);
                        throw new System.Exception(msg);
                    }

                    Matrix4x4 portalMatrix = portal.PortalMatrix;
                    Vector3 newDirection = portalMatrix.MultiplyVector(_camera.transform.forward);
                    // Offset by Epsilon so we can't hit the exit portal on our way out
                    Vector3 newOrigin = portalMatrix.MultiplyPoint3x4(hit.point) + newDirection * PortalThroughOffset;
                    float newDistance = _pickupRange - hit.distance - PortalThroughOffset;


                    if (Physics.Raycast(newOrigin, newDirection, out portalHit, newDistance, portalGrabLayer, QueryTriggerInteraction.UseGlobal))
                    {
                        if (portalHit.collider.gameObject)
                        {
                            if (portalHit.collider.gameObject == heldObject)
                            {
                                return false;
                            } else
                            {
                                return true;     //We're intersectin with a wall or something
                            }
                        }
                    }
                } else
                {
                    if (hit.collider.gameObject == heldObject)
                    {
                        return false; //We're not obstructed
                    }
                    else
                    {
                        return true;     //We're intersectin with a wall or something
                    }
                }
            } else
            {
                //we really should be hitting the object we're carrying, but lets assume we're doing a positive based obstruction instead
            }
            return false; //default return
        }
    


        public void Release() {
            if (CarryingObject()) {
                ReleaseObject();
            } else {
                Debug.LogError("Release() called without an object held");
            }
        }

        void OnPortalTeleport(Portal portal) {
            _pickupRange *= portal.PortalScaleAverage;
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetButton("Circle")) {
                if (CarryingObject()) {
                    Release();
                } else {
                    Grab();
                }
            }
        }

        void FixedUpdate() {
            //We need to see if we should put a point on the other side of a portal
            CarryObject();


            if (_currentObjectPortal) {
                // Current object on other side of portal, let's warp our position
                //_currentObjectPortal.TeleportTransform(_staticAnchorClone.transform, _staticAnchor.transform);
            } else {
                //_staticAnchorClone.transform.position = _staticAnchor.transform.position;
                //_staticAnchorClone.transform.rotation = _staticAnchor.transform.rotation;
            }
            //_joint.gameObject.SetActive(true);
        }
    }
}
