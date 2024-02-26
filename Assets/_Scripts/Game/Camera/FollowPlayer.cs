using _Scripts.PlayerScripts;
using UnityEngine;

namespace _Scripts.Game.Camera
{
    public class FollowPlayer : MonoBehaviour
    {
        private Player m_player;
        private Vector3 m_playerPosition => m_player.transform.position;

        private Vector3 m_desiredPosition;
        private float m_distanceToPlayer;

        public Vector3 offset = new Vector3(0, 0, -10f);

        private void Awake()
        {
            m_player = FindObjectOfType<Player>();
        }

        private void Update()
        {
            m_desiredPosition = new Vector3(m_playerPosition.x, m_playerPosition.y, 0f) + offset;
            m_distanceToPlayer = (m_playerPosition - m_desiredPosition).magnitude;
            transform.position = Vector3.Lerp(transform.position, m_desiredPosition,
                Time.deltaTime * (m_distanceToPlayer >= 2f ? 11f : 4f));
        }
    }
}