using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position + cam.transform.forward * 0.5f, cam.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Debug.Log("Луч уперся в объект: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log("На объекте '" + hit.collider.name + "' нет скрипта взаимодействия (IInteractable)!");
            }
        }
    }
}

public interface IInteractable
{
    void Interact();
}