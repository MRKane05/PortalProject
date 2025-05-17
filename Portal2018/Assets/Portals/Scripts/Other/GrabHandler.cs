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
        private LayerMask _layer;

        private Camera _camera;
        private ConfigurableJoint _joint;
        private Portal _currentObjectPortal;
        private static GrabHandler _instance;
        #endregion

        float holdDistance = 2f;
        public LayerMask traceLayer;

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

        void Awake() {
            _camera = Camera.main;
            heldObject = null;
            holdDistance = Vector3.Distance(Camera.main.transform.position, _staticAnchor.transform.position);
            /*
            _staticAnchorClone = new GameObject("staticAnchorClone").transform;
            _staticAnchorClone.transform.eulerAngles = _staticAnchor.transform.eulerAngles;
            _staticAnchorClone.transform.position = _staticAnchor.transform.position;
            _staticAnchorClone.transform.SetParent(_staticAnchor.transform.parent);*/

            //_joint = _floatingAnchor.gameObject.GetComponent<ConfigurableJoint>();
            /*
            if (_joint == null) {
                Debug.LogError("Anchor object needs a ConfigurableJoint");
            }*/
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
                Debug.LogError("Cannot grab object without a Rigidbody");
                return;
            }

            rigidbody.useGravity = false;
            rigidbody.drag = 10f;
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

            rigidbody.useGravity = true;
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

        public float carryForce = 1f;


        Vector3 cloneholdPosition = Vector3.zero;
        public void CarryObject()
        {

            if (heldObject)
            {
                //See if we can get a point reflected in a portal
                RaycastHit hit;
                Ray newRay = new Ray(Camera.main.transform.position, (_staticAnchor.transform.position - Camera.main.transform.position).normalized);
                Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + newRay.direction * holdDistance * 1.5f, Color.red, 0.5f);

                if (Physics.Raycast(newRay, out hit, holdDistance * 1.5f, traceLayer))
                {
                    //we've got a hit so project the position of this object through the portal
                    Portal hitPortal = hit.collider.gameObject.GetComponent<Portal>();
                    if (hitPortal)
                    {
                        cloneholdPosition = hitPortal.TeleportPoint(_staticAnchor.transform.position);
                        _staticAnchorClone.transform.position = cloneholdPosition;
                    }
                } else
                {
                    cloneholdPosition = Vector3.one * 10000f;   // gameObject.transform.position;  //We need some way to nullify this...
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
                        Debug.Log("Static anchor");
                    } else
                    {
                        Debug.Log("Clone position");
                        forceDirection = cloneholdPosition - heldObject.transform.position;
                    }
                    heldObject.GetComponent<Rigidbody>().AddForce(forceDirection * carryForce);
                }
            }
        }

        public void Grab() {
            if (!CarryingObject()) {
                RaycastHit hit;
                if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _pickupRange, _layer, QueryTriggerInteraction.UseGlobal)) {
                    GameObject obj = hit.collider.gameObject;
                    //Debug.Log(obj);
                    //PortalClone portalClone = obj.GetComponent<PortalClone>();
                    //if (portalClone) {
                    //    // This is a clone, we should grab the real object instead
                    //    GrabObject(portalClone.target.gameObject);
                    //} else {
                        // Just pickin it up
                    GrabObject(obj);
                    //}
                }
            } else {
                Debug.LogError("Grab() called while already holding an object");
            }
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
            if (Input.GetKeyDown(KeyCode.E)) {
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
