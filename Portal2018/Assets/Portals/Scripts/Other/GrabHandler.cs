using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portals {

    public class LineSegment
    {
        Vector3 LineStart;
        Vector3 LineEnd;

        public LineSegment(Vector3 newStart, Vector3 newEnd)
        {
            LineStart = newStart;
            LineEnd = newEnd;
        }

        /// <summary>
        /// Finds the closest point on the line segment to a given point
        /// </summary>
        /// <param name="point">The point to find the closest position to</param>
        /// <param name="t">Output: The parametric distance along the line (0-1, clamped to segment)</param>
        /// <param name="closestPoint">Output: The actual closest point on the line segment</param>
        /// <returns>The distance from the input point to the closest point on the line</returns>
        public float GetClosestPoint(Vector3 point, out float t, out Vector3 closestPoint)
        {
            Vector3 lineVector = LineEnd - LineStart;
            Vector3 pointVector = point - LineStart;

            float lineLength = lineVector.magnitude;

            // Handle degenerate case where start and end are the same
            if (lineLength < Mathf.Epsilon)
            {
                t = 0f;
                closestPoint = LineStart;
                return Vector3.Distance(point, LineStart);
            }

            // Project point onto the line
            float projectionLength = Vector3.Dot(pointVector, lineVector.normalized);

            // Calculate t (parametric distance along line)
            t = projectionLength / lineLength;

            // Clamp t to the line segment (0-1)
            t = Mathf.Clamp01(t);

            // Calculate the actual closest point
            closestPoint = LineStart + lineVector * t;

            // Return distance from input point to closest point
            return Vector3.Distance(point, closestPoint);
        }

    }

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
        public GameObject playerObject;
        #endregion

        void Awake()
        {
            _camera = Camera.main;
            heldObject = null;
            holdDistance = Vector3.Distance(Camera.main.transform.position, _staticAnchor.transform.position);
            playerObject.GetComponent<Teleportable>().OnTeleport += PlayerTeleported;
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

            heldObject.GetComponent<Teleportable>().OnTeleport += ObjectTeleported;

            if (onObjectGrabbed != null) {
                onObjectGrabbed(this, obj);
            }
        }

        public bool bObjectThroughPortal = false;

        bool bObjectOccludedByPortal()
        {
            //Do a basic raycast from where the player is to the view location

            RaycastHit hit;
            Ray newRay = new Ray(Camera.main.transform.position, (_staticAnchor.transform.position - Camera.main.transform.position).normalized);

            bool bCanHoldInPortal = false;
            if (Physics.Raycast(newRay, out hit, holdDistance, traceLayer))  //Detect if we're looking into a portal
            {
                //we've got a hit so project the position of this object through the portal
                Portal hitPortal = hit.collider.gameObject.GetComponent<Portal>();
                if (hitPortal)
                {
                    bObjectThroughPortal = true;
                }
                else
                {

                    //cloneholdPosition = hit.point;  //We just want to have our loation be...the wall.
                    bObjectThroughPortal = false;
                }

            }
            return false;
        }


        void ObjectTeleported(Teleportable sender, Portal portal)
        {
            bObjectThroughPortal = bObjectOccludedByPortal();
            Debug.Log("Object Through Portal: " + bObjectThroughPortal);
        }

        public Portal playerTransitionPortal = null;
        void PlayerTeleported(Teleportable sender, Portal portal)
        {
            //playerTransitionPortal = portal;    //Which is fine unless we go through it backwards...
            bObjectThroughPortal = bObjectOccludedByPortal();
            Debug.Log("Object Through Portal: " + bObjectThroughPortal);
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

        public Portal currentInterfacePortal = null;

        private const float Epsilon = 0.001f;
        public void CarryObject()
        {
            //New idea: Need to re-evaluate the position of the object (in front of/behiond portal) every time the player or object crosses a portal
            //and that way we can set which target the carried object should be moving towards
            if (heldObject)
            {
                
                /*
                //PROBLEM: Need to address our release code
                bool bObjectObstructed = bCarriedObjectObstructed();

                if (bObjectObstructed)
                {
                    ReleaseObject();
                    return;
                }*/

                //See if we can get a point reflected in a portal
                RaycastHit hit;
                Ray newRay = new Ray(Camera.main.transform.position, (_staticAnchor.transform.position - Camera.main.transform.position).normalized);
                LineSegment beforePortal = new LineSegment(Camera.main.transform.position, _staticAnchor.transform.position);
                LineSegment afterPortal;

                bool bCanHoldInPortal = false;
                if (Physics.Raycast(newRay, out hit, holdDistance*1.25f, traceLayer))  //Detect if we're looking into a portal, and extend our clip so that we can be sure of detecting a transition
                {
                    //beforePortal = new LineSegment(Camera.main.transform.position, hit.point + newRay.direction * 0.25f);
                    //we've got a hit so project the position of this object through the portal
                    Portal hitPortal = hit.collider.gameObject.GetComponent<Portal>();  //This doesn't mean that our object is through the portal...
                    if (hitPortal)
                    {
                        /*
                        Matrix4x4 portalMatrix = hitPortal.PortalMatrix;
                        Vector3 newDirection = portalMatrix.MultiplyVector(newRay.direction);
                        // Offset by Epsilon so we can't hit the exit portal on our way out
                        Vector3 newOrigin = portalMatrix.MultiplyPoint3x4(hit.point) + newDirection * Epsilon;
                        
                        afterPortal = new LineSegment(newOrigin, newOrigin + newDirection * (holdDistance - Vector3.Distance(Camera.main.transform.position, hit.point)));

                        float beforeT = -1f;
                        Vector3 beforeClosest = Vector3.zero;
                        beforePortal.GetClosestPoint(heldObject.transform.position, out beforeT, out beforeClosest);

                        float afterT = -1f;
                        Vector3 afterClosest = Vector3.zero;
                        afterPortal.GetClosestPoint(heldObject.transform.position, out afterT, out afterClosest);
                        */

                        Matrix4x4 portalMatrix = hitPortal.PortalMatrix;
                        Vector3 newDirection = portalMatrix.MultiplyVector(newRay.direction);
                        // Offset by Epsilon so we can't hit the exit portal on our way out
                        Vector3 newOrigin = portalMatrix.MultiplyPoint3x4(hit.point) + newDirection * Epsilon;
                        cloneholdPosition = newOrigin + newDirection * (holdDistance - Vector3.Distance(Camera.main.transform.position, hit.point));

                        //Ok, see which one we're closest to
                        if (Vector3.SqrMagnitude(heldObject.transform.position - cloneholdPosition) < Vector3.SqrMagnitude(heldObject.transform.position - _staticAnchor.transform.position))
                        {
                            DoObjectLocation(cloneholdPosition);
                            Debug.Log("Clone Hold Position");
                        } else
                        {
                            DoObjectLocation(_staticAnchor.transform.position);
                            Debug.Log("Anchor Hold Position");
                        }

                        //So in theory we still need to figure out which side of this portal our cube is on (including situations where the portal is behind us)

                        /*
                        //Debug.Log("Before: " + beforeT + " After: " + afterT);
                        if (!bObjectThroughPortal && false)   //We're not fully transitioned into our portal so lets keep moving along our line
                        {
                            Debug.Log("Moving Through Portal");
                            DoObjectLocation(_staticAnchor.transform.position);
                        }
                        else
                        {
                            //Calculate where we'd be keeping this item on the opposite side of the portal
                            Matrix4x4 portalMatrix = hitPortal.PortalMatrix;
                            Vector3 newDirection = portalMatrix.MultiplyVector(newRay.direction);
                            // Offset by Epsilon so we can't hit the exit portal on our way out
                            Vector3 newOrigin = portalMatrix.MultiplyPoint3x4(hit.point) + newDirection * Epsilon;

                            Debug.Log("Tracking Point beyond Portal");
                            DoObjectLocation(newOrigin + newDirection * (holdDistance - Vector3.Distance(Camera.main.transform.position, hit.point)));
                        }*/

                        /*
                        cloneholdPosition = hitPortal.TeleportPoint(_staticAnchor.transform.position);
                        _staticAnchorClone.transform.position = cloneholdPosition;
                        bCanHoldInPortal = true;
                        */
                    }
                    else
                    {

                        //cloneholdPosition = hit.point;  //We just want to have our loation be...the wall.
                        Debug.Log("No Portal Hit Location");
                        DoObjectLocation(_staticAnchor.transform.position);
                    }

                }
                else
                {
                    //If this happens after the above there's a possibility we should drop our item...
                    //cloneholdPosition = Vector3.one * 10000f;   // gameObject.transform.position;  //We need some way to nullify this...
                    // _staticAnchorClone.transform.position = _staticAnchor.transform.position;
                    Debug.Log("No Object Hit Location");
                    DoObjectLocation(_staticAnchor.transform.position);

                }

            }
        }

        public void DoObjectLocation(Vector3 toThisPoint) { 
                
            if (Vector3.Distance(heldObject.transform.position, toThisPoint) < 0.1f)
            {
                //We've no need to move this
            } else
            {
                //See if we should move to our clone position as it's closest for the hold
                Vector3 forceDirection = Vector3.zero;
                forceDirection = toThisPoint - heldObject.transform.position;
                heldObject.GetComponent<Rigidbody>().AddForce(forceDirection * carryForce);
                      
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
            if (heldObject)
                currentInterfacePortal = heldObject.GetComponent<Teleportable>().VisibleToPlayerThroughPortal();

            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Circle")) {
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
