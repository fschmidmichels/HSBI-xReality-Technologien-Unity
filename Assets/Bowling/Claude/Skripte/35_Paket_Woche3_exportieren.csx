// Exportiert das Paket für Woche 3 (Pin-Prefab, Referenzbahn, Halle) inkl. aller Abhängigkeiten.
string outPath = "A:/sciebo/Lehre/xReality Technologien/Kurs/Woche_3/Assets für Unity/XRT_Bowling_Woche3.unitypackage";
var roots = new[] {
    "Assets/Bowling/Prefabs/CollidableBowlingPin.prefab",
    "Assets/Bowling/Prefabs/BowlingLane.prefab",
    "Assets/Bowling/Prefabs/BowlingHall.prefab",
    "Assets/Bowling/LICENSE_VRTK.md",
    "Assets/Bowling/QUELLEN.md",
};
var deps = new System.Collections.Generic.SortedSet<string>(AssetDatabase.GetDependencies(roots, true));
foreach (var r in roots) deps.Add(r);
// Nur eigene Bowling-Assets, keine Paket- oder Projekteinstellungen
deps.RemoveWhere(p => !p.StartsWith("Assets/Bowling/") || p.StartsWith("Assets/Bowling/Claude/"));
AssetDatabase.ExportPackage(System.Linq.Enumerable.ToArray(deps), outPath, ExportPackageOptions.Default);
var size = new System.IO.FileInfo(outPath).Length / 1024;
return size + " KB, " + deps.Count + " Dateien:\n" + string.Join("\n", deps);
