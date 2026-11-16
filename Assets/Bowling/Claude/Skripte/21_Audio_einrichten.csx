var sb = new System.Text.StringBuilder();
const string snd = "Assets/Bowling/Sounds/";
var rollClip = AssetDatabase.LoadAssetAtPath<AudioClip>(snd + "260778__squirmtheverm__rolling-ball-loop.wav");
var strikeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(snd + "766871__zusisking__sprtind-int_bowlingstrike_zus_owsfx_hard.wav");
var pinClips = new AudioClip[6];
for (int i = 0; i < 6; i++) pinClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(snd + "PinHits/PinHit_0" + (i + 1) + ".wav");

void Spatial(AudioSource s, float minDist) {
    s.spatialBlend = 1f;               // 3D-Sound
    s.rolloffMode = AudioRolloffMode.Logarithmic;
    s.minDistance = minDist;
    s.maxDistance = 40f;
    s.dopplerLevel = 0f;
}

// ---------- Kugel ----------
// Jede AudioSource liegt auf einem eigenen Kindobjekt (RollAudio, HitAudio), weil
// MetaXRAudioSource nur die erste AudioSource eines Objekts steuert (siehe Woche 5).
// Vorhandene Kindobjekte und BallAudio werden wiederverwendet, damit Verweise und
// Szenen-Overrides erhalten bleiben; das Skript ist dadurch wiederholbar.
const string ballPath = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var ball = PrefabUtility.LoadPrefabContents(ballPath);
foreach (var a in ball.GetComponents<AudioSource>()) UnityEngine.Object.DestroyImmediate(a);   // alter Aufbau: AudioSources am Wurzelobjekt

AudioSource ChildSource(string name) {
    var t = ball.transform.Find(name);
    if (t == null) {
        t = new GameObject(name).transform;
        t.SetParent(ball.transform, false);
        t.gameObject.layer = ball.layer;
    }
    var s = t.GetComponent<AudioSource>();
    return s != null ? s : t.gameObject.AddComponent<AudioSource>();
}

var roll = ChildSource("RollAudio");
roll.clip = rollClip; roll.loop = true; roll.playOnAwake = true; roll.volume = 0f;
Spatial(roll, 1.5f);
var hit = ChildSource("HitAudio");
hit.clip = null; hit.loop = false; hit.playOnAwake = false; hit.volume = 1f;
Spatial(hit, 4f);                       // Strike soll auch am Anlauf gut hörbar sein
var ba = ball.GetComponent<BallAudio>();
if (ba == null) ba = ball.AddComponent<BallAudio>();
ba.rollSource = roll; ba.hitSource = hit; ba.strikeClip = strikeClip;
PrefabUtility.SaveAsPrefabAsset(ball, ballPath);
PrefabUtility.UnloadPrefabContents(ball);
sb.AppendLine("BowlingBall: BallAudio + Kindobjekte RollAudio und HitAudio mit je einer AudioSource");

// ---------- Pin ----------
const string pinPath = "Assets/Bowling/Prefabs/CollidableBowlingPin.prefab";
var pin = PrefabUtility.LoadPrefabContents(pinPath);
// Vorhandene Komponenten wiederverwenden (Verweise und Szenen-Overrides bleiben erhalten).
var ps = pin.GetComponent<AudioSource>();
if (ps == null) ps = pin.AddComponent<AudioSource>();
ps.playOnAwake = false;
Spatial(ps, 3f);
var pa = pin.GetComponent<PinAudio>();
if (pa == null) pa = pin.AddComponent<PinAudio>();
pa.hitClips = pinClips;
PrefabUtility.SaveAsPrefabAsset(pin, pinPath);
PrefabUtility.UnloadPrefabContents(pin);
sb.AppendLine("CollidableBowlingPin: AudioSource + PinAudio mit " + pinClips.Length + " Clips");

AssetDatabase.SaveAssets();
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
var sceneBall = GameObject.Find("BowlingBall").GetComponent<BallAudio>();
var scenePins = GameObject.Find("BowlingLane/Pins").GetComponentsInChildren<PinAudio>();
sb.AppendLine("Szene: BallAudio=" + (sceneBall != null) + ", PinAudio an " + scenePins.Length + " Pins, dirty=" + scene.isDirty);
return sb.ToString();
