using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			// Options for which axes to use
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}

        public enum JoystickID
        {
            // ID of which side and use Joystick is
            LeftMovement,
            RightShootingDirection
        }

        public int MovementRange = 100;
        public JoystickID ID; // Joystick ID of which side and function
		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

        string opposingHAxisName = "Mouse X"; // For use of left stick when right stick isn't in use
        string opposingVAxisName = "Mouse Y"; // For use of left stick when right stick isn't in use

        public Joystick RightJoyStickScriptRef;

        // Change Method of getting this later
        public GameObject Player;
        CompleteProject.PlayerMovement PlayerMoveScriptRef;

        bool RightStickActive = false;

        Vector3 m_StartPos;
		bool m_UseX; // Toggle for using the x axis
		bool m_UseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

        CrossPlatformInputManager.VirtualAxis m_OpposingHVirtualAxis;
        CrossPlatformInputManager.VirtualAxis m_OpposingVVirtualAxis;

        void OnEnable()
		{
			CreateVirtualAxes();
		}

        void Start()
        {
            m_StartPos = transform.position;

            // Get Player Movement script off Player gameobject
            PlayerMoveScriptRef = Player.GetComponent<CompleteProject.PlayerMovement>();

            if (PlayerMoveScriptRef == null)
                Debug.Log("PlayerMoveScript error: Null");

            // Setting opposing joystick virtual axis references
            if (ID == JoystickID.LeftMovement)
            {
                m_OpposingHVirtualAxis = RightJoyStickScriptRef.m_HorizontalVirtualAxis;
                m_OpposingVVirtualAxis = RightJoyStickScriptRef.m_VerticalVirtualAxis;
            }
        }

		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = m_StartPos - value;
			delta.y = -delta.y;
			delta /= MovementRange;
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Update(-delta.x);

                // If this script is Left Stick AND RightStick is inactive -> Allow change on opposing axis
                if (ID == JoystickID.LeftMovement && RightJoyStickScriptRef.IsRightAxisActive() == false)
                {
                    Debug.Log("Allow direction change");
                    // Direction code change here
                    m_OpposingHVirtualAxis.Update(-delta.x); // Oppsite joystick input axis update
                }
			}

			if (m_UseY)
			{
				m_VerticalVirtualAxis.Update(delta.y);

                // If this script is Left Stick AND RightStick is inactive -> Allow change on opposing axis
                if (ID == JoystickID.LeftMovement && RightJoyStickScriptRef.IsRightAxisActive() == false)
                {
                    Debug.Log("Allow direction change");
                    // Direction code change here
                    m_OpposingVVirtualAxis.Update(delta.y); // Oppsite joystick input axis update
                }
            }
		}

		void CreateVirtualAxes()
		{
			// set axes to use
			m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (m_UseX)
			{
				m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
			}
		}


		public void OnDrag(PointerEventData data)
		{
			Vector3 newPos = Vector3.zero;

			if (m_UseX)
			{
				int delta = (int)(data.position.x - m_StartPos.x);
				//delta = Mathf.Clamp(delta, - MovementRange, MovementRange);
				newPos.x = delta;

                // If Right Stick -> Change boolean to active
                if (ID == JoystickID.RightShootingDirection && RightStickActive == false)
                {
                    RightStickActive = true;
                    Debug.Log(RightStickActive);
                }
            }

			if (m_UseY)
			{
				int delta = (int)(data.position.y - m_StartPos.y);
				//delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
				newPos.y = delta;

                // If Right Stick -> Change boolean to active
                if (ID == JoystickID.RightShootingDirection && RightStickActive == false)
                {
                    RightStickActive = true;
                    Debug.Log(RightStickActive);
                }
            }
            // Clamp the joystick to a circular magnitude rather than the square, now takes all directions into consideration to clamp from centre (what delta Mathf lines where originally doing above). 
            // new position = Clamp between ( position vector , Max Movement Range ) + starting position
			transform.position = Vector3.ClampMagnitude( new Vector3(newPos.x, newPos.y, newPos.z), MovementRange) + m_StartPos;
			UpdateVirtualAxes(transform.position);
		}

        
		public void OnPointerUp(PointerEventData data)
		{
			transform.position = m_StartPos;
			UpdateVirtualAxes(m_StartPos);

            // When player lets go of the joystick, if it is on the right shooting direction joystick reset the active boolean
            if (ID == JoystickID.RightShootingDirection)
            {
                RightStickActive = false;
                Debug.Log(RightStickActive);
            }

		}


		public void OnPointerDown(PointerEventData data) { }

		void OnDisable()
		{
			// remove the joysticks from the cross platform input
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Remove();
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis.Remove();
			}
		}
    
        // Returns boolean to opposing Joystick script that called it (Left will call function to check Right Joystick if active)
        public bool IsRightAxisActive ()
        {
            return RightStickActive;
        }

    }
}