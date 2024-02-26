using _Scripts.Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Movement
{
    public class MovementControl : MonoBehaviour
    {
        protected Rigidbody m_rigidbody = null;

        // constants
        protected const float m_defaultMovementSpeed = 5.0f;
        
        // editor vars
        public float m_rotBlendSpeed = 500.0f;
        // public bool m_rotateLimitedAtLowSpeed = true;

        protected float m_movementSpeed = m_defaultMovementSpeed;
        protected float m_movementSpeedScaleModifier = 1.0f;
        protected Vector3 m_setMovementVelocity = Vector3.zero;
        protected Vector3 m_setAngularVelocity = Vector3.zero;

        public virtual void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        public virtual void Start()
        {
        }

        public virtual void Update()
        {
            
            
            canApplyUpdate = true;
        }
        
        protected bool canApplyUpdate=true;
        void FixedUpdate()
        {
            if(canApplyUpdate)
            {
                ApplyUpdate();
                canApplyUpdate=false;
            }
        }

        public virtual void SetInitialRotation()
        {
            m_setAngularVelocity = Vector3.zero;
        }

        public virtual void ApplyUpdate()
        {
            if (m_rigidbody == null || m_rigidbody.isKinematic) return;

            m_rigidbody.velocity = m_setMovementVelocity;
            m_rigidbody.angularVelocity = m_setAngularVelocity;
        }

        public virtual void MoveTowardsPoint(Vector3 targetPos)
        {
            // float speed = m_movementSpeed * m_movementSpeedScaleModifier;
            Vector3 desiredVel = targetPos - transform.position;
            
            MoveTowardsVelocity(desiredVel);
        }

        public virtual void MoveTowardsVelocity(Vector3 desiredVelocity)
        {
            m_setMovementVelocity = desiredVelocity;

            UpdateRotation();
        }

        public void UpdateRotation()
        {
            Vector3 dir = m_setMovementVelocity;

            RotateToPointInDirection(dir);
        }

        public virtual void RotateToPointInDirection(Vector3 dir)
        {
            if(dir.sqrMagnitude < 0.01f)
            {
                m_setAngularVelocity = Vector3.zero;
                return;
            }
            
            // now get desired rot angles, in degrees.
            float desiredPitch = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;	// standard rot angle, pointing in the direction of desired velocity
            float desiredTwist = (Mathf.Abs(desiredPitch)>90.0f) ? 180.0f : 0.0f;
            
            // blend
            Quaternion qPitch = Quaternion.Euler(0.0f, 0.0f, desiredPitch);
            Quaternion qTwist = Quaternion.Euler(desiredTwist, 0.0f, 0.0f);
            Quaternion qDesired = qPitch * qTwist;							// this is the desired orientation based on shark velocity
            
            // stop rotating at low movement speeds.  Below 1m/s do not change rot at all, and between 1-2m/s, scale down blend amount
            float rotBlendAmount = m_rotBlendSpeed * Mathf.Deg2Rad;
            // float linearSpeed = m_setMovementVelocity.magnitude;
            //
            // if(m_rotateLimitedAtLowSpeed)
            // {
            //     if(linearSpeed < 1.0f)
            //         rotBlendAmount = 0.0f;
            //     else if(linearSpeed < 2.0f)
            //         rotBlendAmount *= (linearSpeed-1.0f);
            // }
            
            Quaternion qDelta = qDesired * Quaternion.Inverse(transform.rotation);
            Vector3 deltaAxis;
            float deltaAng;
            qDelta.ToAngleAxis(out deltaAng, out deltaAxis);
            float signedDeltaAng = Util.FixAnglePlusMinusDegrees(deltaAng);
            deltaAng = Mathf.Abs(signedDeltaAng);
		
            float angDampingRange = 20.0f;
            if(deltaAng < angDampingRange)
                rotBlendAmount *= (deltaAng/angDampingRange);
            
            if(deltaAng == 0.0f)
                m_setAngularVelocity = Vector3.zero;
            else
                m_setAngularVelocity = deltaAxis * ((signedDeltaAng < 0.0f) ? -rotBlendAmount : rotBlendAmount);
        }
    }
}