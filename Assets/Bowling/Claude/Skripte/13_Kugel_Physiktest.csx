var sb = new System.Text.StringBuilder();
var scenePath = "Assets/Scenes/BowlingVR.unity";
var prev = Physics.simulationMode;
Physics.simulationMode = SimulationMode.Script;
void Reload() { UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single); }

// 1) Kugel bleibt 3 s auf der Ablage liegen?
Reload();
var ball = GameObject.Find("BowlingBall");
var start = ball.transform.position;
for (int i = 0; i < 150; i++) Physics.Simulate(Time.fixedDeltaTime);
sb.AppendLine("Ablage: Start=" + start.ToString("F3") + " nach 3 s=" + ball.transform.position.ToString("F3"));

// 2) Prefab-Kugel gerade auf die Bahn werfen
Reload();
ball = GameObject.Find("BowlingBall");
var rb = ball.GetComponent<Rigidbody>();
rb.position = new Vector3(0.05f, 0.111f, 0.5f); ball.transform.position = rb.position;
rb.linearVelocity = new Vector3(0f, 0f, 8f);
var pins = GameObject.Find("BowlingLane/Pins").GetComponentsInChildren<Rigidbody>(true);
var r0 = System.Array.ConvertAll(pins, x => x.rotation);
for (int i = 0; i < 200; i++) Physics.Simulate(Time.fixedDeltaTime);
int down = 0; for (int k = 0; k < pins.Length; k++) if (Quaternion.Angle(r0[k], pins[k].rotation) > 45f) down++;
sb.AppendLine("Wurf 8 m/s: Kugel bei " + ball.transform.position.ToString("F2") + ", Pins um=" + down + "/10");

Physics.simulationMode = prev;
Reload();
return sb.ToString();
