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
const string ballPath = "Assets/Bowling/Prefabs/BowlingBall.prefab";
var ball = PrefabUtility.LoadPrefabContents(ballPath);
foreach (var a in ball.GetComponents<AudioSource>()) UnityEngine.Object.DestroyImmediate(a);
foreach (var a in ball.GetComponents<BallAudio>()) UnityEngine.Object.DestroyImmediate(a);
var roll = ball.AddComponent<AudioSource>();
roll.clip = rollClip; roll.loop = true; roll.playOnAwake = true; roll.volume = 0f;
Spatial(roll, 1.5f);
var hit = ball.AddComponent<AudioSource>();
hit.playOnAwake = false;
Spatial(hit, 4f);                       // Strike soll auch am Anlauf gut hörbar sein
var ba = ball.AddComponent<BallAudio>();
ba.rollSource = roll; ba.hitSource = hit; ba.strikeClip = strikeClip;
PrefabUtility.SaveAsPrefabAsset(ball, ballPath);
PrefabUtility.UnloadPrefabContents(ball);
sb.AppendLine("BowlingBall: 2 AudioSources + BallAudio");

// ---------- Pin ----------
const string pinPath = "Assets/Bowling/Prefabs/CollidableBowlingPin.prefab";
var pin = PrefabUtility.LoadPrefabContents(pinPath);
foreach (var a in pin.GetComponents<PinAudio>()) UnityEngine.Object.DestroyImmediate(a);
foreach (var a in pin.GetComponents<AudioSource>()) UnityEngine.Object.DestroyImmediate(a);
var ps = pin.AddComponent<AudioSource>();
ps.playOnAwake = false;
Spatial(ps, 3f);
var pa = pin.AddComponent<PinAudio>();
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
