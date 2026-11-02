var sb = new System.Text.StringBuilder();
var scenePath = "Assets/Scenes/BowlingVR.unity";
var prevMode = Physics.simulationMode;
Physics.simulationMode = SimulationMode.Script;

void Reload() { UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single); }
GameObject Find(string path) => GameObject.Find(path);

string Run(string label, bool bumpers, Vector3 start, Vector3 velocity, float seconds) {
    Reload();
    Find("BowlingLane/Bumpers").SetActive(bumpers);
    var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    ball.transform.localScale = Vector3.one * 0.218f;
    ball.transform.position = start;
    var rb = ball.AddComponent<Rigidbody>();
    rb.mass = 7f;
    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    rb.linearVelocity = velocity;
    var pins = Find("BowlingLane/Pins").GetComponentsInChildren<Rigidbody>(true); var startRot = System.Array.ConvertAll(pins, x => x.rotation);
    var gl = Find("BowlingLane/Zones/GutterZone_Left").GetComponent<BoxCollider>().bounds;
    var gr = Find("BowlingLane/Zones/GutterZone_Right").GetComponent<BoxCollider>().bounds;
    var pz = Find("BowlingLane/Zones/PitZone").GetComponent<BoxCollider>().bounds;
    bool wasInGutter = false; float minY = 99f; float maxAbsXonLane = 0f;
    float dt = Time.fixedDeltaTime; int steps = Mathf.RoundToInt(seconds / dt);
    for (int i = 0; i < steps; i++) {
        Physics.Simulate(dt);
        var p = ball.transform.position;
        if (gl.Contains(p) || gr.Contains(p)) wasInGutter = true;
        minY = Mathf.Min(minY, p.y);
    }
    int down = 0;
    for (int k = 0; k < pins.Length; k++) if (!pins[k].gameObject.activeSelf || Quaternion.Angle(startRot[k], pins[k].rotation) > 45f) down++;
    var e = ball.transform.position;
    string zone = gl.Contains(e) ? "GutterZone_Left" : gr.Contains(e) ? "GutterZone_Right" : pz.Contains(e) ? "PitZone" : "-";
    return label + ": Ende=" + e.ToString("F2") + " v=" + rb.linearVelocity.magnitude.ToString("F2") + " m/s, Zone=" + zone + ", war in Rinne=" + wasInGutter + ", Pins um=" + down + "/10";
}

// 0) Pins stehen ohne Kugel stabil?
Reload();
var pinsIdle = Find("BowlingLane/Pins").GetComponentsInChildren<Rigidbody>(); var idleStart = System.Array.ConvertAll(pinsIdle, x => x.rotation);
for (int i = 0; i < 150; i++) Physics.Simulate(Time.fixedDeltaTime);
int idleDown = 0; float drift = 0f;
for (int k = 0; k < pinsIdle.Length; k++) { if (Quaternion.Angle(idleStart[k], pinsIdle[k].rotation) > 5f) idleDown++; drift = Mathf.Max(drift, pinsIdle[k].linearVelocity.magnitude); }
sb.AppendLine("Ruhetest 3 s: gekippte Pins=" + idleDown + ", max. Geschwindigkeit=" + drift.ToString("F3"));

float y = 0.109f + 0.002f;
sb.AppendLine(Run("Gerade, 8 m/s, Bumper an", true,  new Vector3(0.05f, y, 0.5f), new Vector3(0f, 0f, 8f), 4f));
sb.AppendLine(Run("Schräg nach rechts, Bumper an", true,  new Vector3(0f, y, 0.5f), new Vector3(0.35f, 0f, 7f), 4.5f));
sb.AppendLine(Run("Schräg nach rechts, Bumper aus", false, new Vector3(0f, y, 0.5f), new Vector3(0.35f, 0f, 7f), 4.5f));
sb.AppendLine(Run("Langsam in Rinne, 1 m/s, Bumper aus", false, new Vector3(0.64f, 0.07f, 0.5f), new Vector3(0f, 0f, 1f), 30f));

Physics.simulationMode = prevMode;
Reload();
return sb.ToString();
