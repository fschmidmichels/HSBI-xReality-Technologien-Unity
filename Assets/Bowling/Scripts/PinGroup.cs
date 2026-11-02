using UnityEngine;

public class PinGroup : MonoBehaviour
{
    private Pin[] pins;
    private Vector3[] positions;
    private Quaternion[] rotations;

    protected void Awake()
    {
        pins = GetComponentsInChildren<Pin>();
        SavePositions();
    }

    public void SavePositions()
    {
        positions = new Vector3[pins.Length];
        rotations = new Quaternion[pins.Length];
        for (int index = 0; index < pins.Length; index++)
        {
            positions[index] = pins[index].transform.position;
            rotations[index] = pins[index].transform.rotation;
        }
    }

    public void ResetPositions()
    {
        for (int index = 0; index < pins.Length; index++)
        {
            // Zuerst aktivieren, sonst wirken die Rigidbody-Werte bei ausgeblendeten Pins nicht.
            pins[index].gameObject.SetActive(true);
            pins[index].CancelToppleCheck();
            Rigidbody pinRigidbody = pins[index].GetComponent<Rigidbody>();
            pinRigidbody.linearVelocity = Vector3.zero;
            pinRigidbody.angularVelocity = Vector3.zero;
            // Rigidbody und Transform setzen, damit die Interpolation die Pins nicht zurückzieht.
            pinRigidbody.position = positions[index];
            pinRigidbody.rotation = rotations[index];
            pins[index].transform.SetPositionAndRotation(positions[index], rotations[index]);
        }
    }
}