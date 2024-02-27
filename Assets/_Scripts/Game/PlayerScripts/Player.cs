using _Scripts.Game;
using _Scripts.Movement;
using UnityEngine;

namespace _Scripts.PlayerScripts
{
    public class Player : MonoBehaviour
    {
        private FishMovementControl m_movementControl = null;

        public FishMovementControl MovementControl
        {
            get { return m_movementControl; }
            set { m_movementControl = value; }
        }

        private bool m_playerCanMove = true;
        public bool PlayerCanMove => m_playerCanMove;

        void Start()
        {
            App.Instance.Player = this;
            
            m_movementControl = GetComponent<FishMovementControl>();
            m_movementControl.SetInitialRotation();
        }

        void Update()
        {
            UpdateMovementControl();
        }

        private void UpdateMovementControl()
        {
            if (!PlayerCanMove)
            {
                m_movementControl.MoveTowardsVelocity(Vector3.zero);
                return;
            }

            Vector3 desiredVel = new Vector3(App.Instance.GameInput.tiltX, App.Instance.GameInput.tiltY,
                0f).normalized;

            m_movementControl.MoveTowardsVelocity(desiredVel);
        }
    }
}