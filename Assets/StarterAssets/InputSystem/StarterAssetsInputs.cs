using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool attack;
        public bool grab;
        public bool Inventory;
        public Vector2 lookInput { get { return look; } }

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }
        public void OnInventory(InputValue value)
        {
            bool isPressed = value.isPressed;
            if(InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SetInventoryOpen(isPressed);
            }
            Debug.Log($"Inventory Input Received: {isPressed}");
        }
        // NEW: Input callback for grabbing.
        public void OnGrab(InputValue value)
        {
            GrabInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        } 

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
            Debug.Log($"Attack Input Received: {newAttackState}");
        }
        public void InventoryInput(bool newInventoryState)
        {
            Inventory = newInventoryState;
            Debug.Log($"Inventory Input Received: {newInventoryState}");
        }

        // NEW: Set the grab input state.
        public void GrabInput(bool newGrabState)
        {
            grab = newGrabState;
            Debug.Log($"Grab Input Received: {newGrabState}");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
