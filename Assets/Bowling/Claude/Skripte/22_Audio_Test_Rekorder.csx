// Wirft die Kugel und zeichnet 6 s lang pro Frame die Audio-Zustände auf (Ergebnis in SessionState "AudioProbe").
var ba = GameObject.Find("BowlingBall").GetComponent<BallAudio>();
var rb = ba.GetComponent<Rigidbody>();
var pinSources = GameObject.Find("BowlingLane/Pins").GetComponentsInChildren<AudioSource>(true);
rb.position = new Vector3(0f, 0.115f, 0.5f); ba.transform.position = rb.position;
rb.linearVelocity = new Vector3(0f, 0f, 7f);
float t0 = Time.time, maxVol = 0f, maxPitch = 0f, strikeAt = -1f, firstPinAt = -1f; int maxPinsPlaying = 0;
var pinsHeard = new System.Collections.Generic.HashSet<int>();
UnityEditor.SessionState.SetString("AudioProbe", "läuft");
EditorApplication.CallbackFunction cb = null;
cb = () => {
    if (!EditorApplication.isPlaying || ba == null) { EditorApplication.update -= cb; return; }
    float t = Time.time - t0;
    maxVol = Mathf.Max(maxVol, ba.rollSource.volume); maxPitch = Mathf.Max(maxPitch, ba.rollSource.pitch);
    if (strikeAt < 0 && ba.hitSource.isPlaying) strikeAt = t;
    int n = 0;
    for (int i = 0; i < pinSources.Length; i++) if (pinSources[i] != null && pinSources[i].isPlaying) { n++; pinsHeard.Add(i); if (firstPinAt < 0) firstPinAt = t; }
    maxPinsPlaying = Mathf.Max(maxPinsPlaying, n);
    if (t > 6f) {
        EditorApplication.update -= cb;
        UnityEditor.SessionState.SetString("AudioProbe", "Rollen: max vol=" + maxVol.ToString("F2") + " max pitch=" + maxPitch.ToString("F2")
            + " | Strike ab t=" + strikeAt.ToString("F2") + " s | Pin-Sounds: erster bei t=" + firstPinAt.ToString("F2") + " s, verschiedene Pins=" + pinsHeard.Count + ", max gleichzeitig=" + maxPinsPlaying
            + " | Kugel jetzt z=" + ba.transform.position.z.ToString("F1") + " vol=" + ba.rollSource.volume.ToString("F2"));
    }
};
EditorApplication.update += cb;
return "Wurf gestartet";
