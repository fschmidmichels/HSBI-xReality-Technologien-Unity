var sb = new System.Text.StringBuilder();
var prev = Physics.simulationMode;
Physics.simulationMode = SimulationMode.Script;
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/BowlingVR.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
var pin = GameObject.Find("BowlingLane/Pins/Pin_1");
var rb = pin.GetComponent<Rigidbody>();
var r0 = rb.rotation;
sb.AppendLine("COM local=" + rb.centerOfMass.ToString("F3") + " world=" + rb.worldCenterOfMass.ToString("F3") + " inertia=" + rb.inertiaTensor.ToString("F5"));
var box = pin.GetComponent<BoxCollider>(); var cap = pin.GetComponent<CapsuleCollider>();
sb.AppendLine("box bounds min.y=" + box.bounds.min.y.ToString("F4") + " size=" + box.bounds.size.ToString("F4") + " cap min.y=" + cap.bounds.min.y.ToString("F4"));
for (int i = 1; i <= 150; i++) {
    Physics.Simulate(Time.fixedDeltaTime);
    if (i == 1 || i == 5 || i == 10 || i == 25 || i == 50 || i == 100 || i == 150)
        sb.AppendLine("t=" + (i * Time.fixedDeltaTime).ToString("F2") + " pos=" + rb.position.ToString("F4") + " angle=" + Quaternion.Angle(r0, rb.rotation).ToString("F2") + " v=" + rb.linearVelocity.ToString("F3") + " w=" + rb.angularVelocity.ToString("F3"));
}
Physics.simulationMode = prev;
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/BowlingVR.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
return sb.ToString();
