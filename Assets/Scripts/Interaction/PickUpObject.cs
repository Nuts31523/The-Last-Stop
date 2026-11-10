using UnityEngine;

public class PickUpObject : MonoBehaviour, IInteractable
{
    private bool isPickedUp = false;
    private Rigidbody rb;
    private Transform playerCamera;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        col = GetComponent<Collider>();
    }

    public void Interact()
    {
        if (!isPickedUp)
        {
            // Поднимаем куб
            playerCamera = Camera.main.transform;
            transform.SetParent(playerCamera);

            // Позиция перед камерой
            transform.localPosition = new Vector3(0.5f, -0.3f, 1.5f);
            transform.localRotation = Quaternion.identity;

            rb.isKinematic = true;
            rb.useGravity = false;

            // Делаем коллайдер триггером, чтобы он не толкал игрока, 
            // но луч по-прежнему мог попадать по нему для броска
            if (col != null) col.isTrigger = true;

            isPickedUp = true;
            Debug.Log("Куб поднят в руки!");
        }
        else
        {
            // Бросаем куб обратно
            transform.SetParent(null);

            rb.isKinematic = false;
            rb.useGravity = true;

            // Возвращаем обычный физический коллайдер
            if (col != null) col.isTrigger = false;

            // Придаем импульс вперед при броске
            rb.AddForce(playerCamera.forward * 5f, ForceMode.Impulse);

            isPickedUp = false;
            Debug.Log("Куб брошен!");
        }
    }
}