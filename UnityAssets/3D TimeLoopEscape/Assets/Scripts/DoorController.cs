using UnityEngine;

public class DoorController : MonoBehaviour
{
       private bool isOpen = false;

       public void ToggleDoor()
    {
                Transform hinge = transform.parent;

        if (!isOpen)
        {
            hinge.Rotate(0, 90f, 0); 
            isOpen = true;
        }
        else
        {
            hinge.Rotate(0, -90f, 0); 
            isOpen = false;
        }
    }
}