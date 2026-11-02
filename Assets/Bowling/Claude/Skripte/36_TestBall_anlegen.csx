// Legt in BowlingVR die Testkugel aus Block 3 an (deaktiviert, wie in Woche_3.md beschrieben).
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
foreach (var r in scene.GetRootGameObjects()) if (r.name == "TestBall") UnityEngine.Object.DestroyImmediate(r);
var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
ball.name = "TestBall";
ball.transform.position = new Vector3(0f, 0.11f, 0.5f);
ball.transform.localScale = Vector3.one * 0.218f;
ball.layer = LayerMask.NameToLayer("BowlingBall");   // kollidiert wie die echte Kugel nicht mit dem Spieler
ball.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Bowling/Materials/Ball.mat");
ball.GetComponent<SphereCollider>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Bowling/PhysicsMaterials/Ball.physicMaterial");
var rb = ball.AddComponent<Rigidbody>();
rb.mass = 7f;
rb.interpolation = RigidbodyInterpolation.Interpolate;
rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
ball.AddComponent<TestImpulse>();
ball.SetActive(false);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "TestBall angelegt (deaktiviert), Layer " + LayerMask.LayerToName(ball.layer);
