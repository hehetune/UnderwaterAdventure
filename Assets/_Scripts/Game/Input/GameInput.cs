using UnityEngine;

namespace _Scripts.Game
{
    public class GameInput : MonoBehaviour
    {
        public enum ControlMethod
        {
            Mouse = 0,
            KeyBoard = 1,
        }

        public ControlMethod CurrentControlMethod = ControlMethod.Mouse;
        public float tiltX = 0;
        public float tiltY = 0;

        void Update()
        {
            if (CurrentControlMethod == ControlMethod.Mouse)
            {
                Vector2 mousePos = Input.mousePosition;
                tiltX = mousePos.x;
                tiltY = mousePos.y;
            }
            else if (CurrentControlMethod == ControlMethod.KeyBoard)
            {
                tiltX = Input.GetAxisRaw("Horizontal");
                tiltY = Input.GetAxisRaw("Vertical");
            }
        }
    }
}