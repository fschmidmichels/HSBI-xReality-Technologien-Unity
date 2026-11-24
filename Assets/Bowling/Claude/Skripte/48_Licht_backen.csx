// Startet das Backen (Lightmaps, Light Probes, Reflection Probe) asynchron. Fortschritt: Lightmapping.isRunning.
if (Lightmapping.isRunning) return "läuft bereits";
return Lightmapping.BakeAsync() ? "gestartet" : "Start fehlgeschlagen";
