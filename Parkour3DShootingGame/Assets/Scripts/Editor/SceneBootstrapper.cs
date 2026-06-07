using Parkour3DShooting.Animation;
using Parkour3DShooting.Boss;
using Parkour3DShooting.Camera;
using Parkour3DShooting.Managers;
using Parkour3DShooting.Player;
using Parkour3DShooting.Stage;
using Parkour3DShooting.UI;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Parkour3DShooting.Editor
{
    /// <summary>
    /// MVPシーン、ステージ、プレイヤー、ボス、UI、カメラをエディタ上で自動生成する補助クラスです。
    /// </summary>
    public static class SceneBootstrapper
    {
        /// <summary>自動生成するメインシーンの保存先です。</summary>
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        /// <summary>自動生成する弾プレハブの保存先です。</summary>
        private const string ProjectilePrefabPath = "Assets/Prefabs/Projectiles/Projectile.prefab";
        /// <summary>Cinemachineとフォールバックカメラで使う追従オフセットです。</summary>
        private static readonly Vector3 FollowCameraOffset = new Vector3(0f, 1f, -7f);
        /// <summary>フォールバックカメラがプレイヤーを見る高さです。</summary>
        private const float FollowCameraLookHeight = 1.4f;
        /// <summary>カメラが追う安定ターゲットの名前です。</summary>
        private const string CameraTargetName = "PlayerCameraTarget";

        /// <summary>
        /// MVP用のメインシーンを空シーンから構築し、ビルド対象にも登録します。
        /// </summary>
        [MenuItem("Parkour3DShooting/Build MVP Scene")]
        public static void BuildMvpScene()
        {
            EnsureProjectFolders();

            Projectile projectilePrefab = CreateProjectilePrefab();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainScene";

            RenderSettings.skybox = null;
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.42f);

            GameObject stageRoot = new GameObject("StageRoot");
            GameObject buildingRoot = new GameObject("BuildingRoot");
            GameObject obstacleRoot = new GameObject("ObstacleRoot");
            GameObject grazeRoot = new GameObject("GrazeRoot");
            buildingRoot.transform.SetParent(stageRoot.transform);
            obstacleRoot.transform.SetParent(stageRoot.transform);
            grazeRoot.transform.SetParent(stageRoot.transform);

            PlayerController player = CreatePlayer(projectilePrefab);
            BossController boss = CreateBoss(player.transform, projectilePrefab);
            CreateStage(buildingRoot.transform, obstacleRoot.transform, grazeRoot.transform);
            CreateLighting();

            ResultView resultView;
            HpView hpView;
            ScoreView scoreView;
            CreateUi(player, out resultView, out hpView, out scoreView);

            ScoreManager scoreManager = new GameObject("ScoreManager").AddComponent<ScoreManager>();
            GameObject gameManagerObject = new GameObject("GameManager");
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();
            gameManager.Configure(player, boss, resultView, scoreManager);
            scoreView.Configure(scoreManager, scoreView.GetComponentInChildren<Text>());

            new GameObject("AudioManager");
            CreateCamera(player.transform);

            EditorSceneManager.SaveScene(scene, MainScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"MVP scene generated: {MainScenePath}");
        }

        /// <summary>
        /// 生成に必要なAssets配下のフォルダーを作成します。
        /// </summary>
        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "Scripts");
            EnsureFolder("Assets/Scripts", "Core");
            EnsureFolder("Assets/Scripts", "Managers");
            EnsureFolder("Assets/Scripts", "Player");
            EnsureFolder("Assets/Scripts", "Boss");
            EnsureFolder("Assets/Scripts", "Stage");
            EnsureFolder("Assets/Scripts", "UI");
            EnsureFolder("Assets/Scripts", "Events");
            EnsureFolder("Assets/Scripts", "Animation");
            EnsureFolder("Assets/Scripts", "Camera");
            EnsureFolder("Assets/Scripts", "Data");
            EnsureFolder("Assets/Scripts", "Utility");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Projectiles");
            EnsureFolder("Assets", "Materials");
        }

        /// <summary>
        /// 指定フォルダーが存在しなければ作成します。
        /// </summary>
        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        /// <summary>
        /// 弾プレハブを取得または新規生成します。
        /// </summary>
        private static Projectile CreateProjectilePrefab()
        {
            Projectile existing = AssetDatabase.LoadAssetAtPath<Projectile>(ProjectilePrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Projectile";
            projectileObject.transform.localScale = Vector3.one * 0.35f;
            projectileObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial("ProjectileMat", new Color(0.2f, 0.85f, 1f));
            SphereCollider collider = projectileObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            Rigidbody rigidbody = projectileObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            Projectile projectile = projectileObject.AddComponent<Projectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectileObject, ProjectilePrefabPath);
            Object.DestroyImmediate(projectileObject);
            return prefab.GetComponent<Projectile>();
        }

        /// <summary>
        /// プレイヤー本体、CharacterController、射撃位置、PlayerControllerを生成します。
        /// </summary>
        private static PlayerController CreatePlayer(Projectile projectilePrefab)
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player";
            playerObject.transform.position = new Vector3(0f, 6.15f, -2f);
            playerObject.transform.localScale = new Vector3(1f, 1.2f, 1f);
            playerObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial("PlayerMat", new Color(0.15f, 0.75f, 1f));
            Object.DestroyImmediate(playerObject.GetComponent<CapsuleCollider>());

            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.height = 2.3f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1.1f, 0f);

            Transform shotOrigin = new GameObject("ShotOrigin").transform;
            shotOrigin.SetParent(playerObject.transform);
            shotOrigin.localPosition = new Vector3(0f, 1.3f, 0.8f);

            PlayerController player = playerObject.AddComponent<PlayerController>();
            player.Configure(projectilePrefab, shotOrigin);
            return player;
        }

        /// <summary>
        /// ボス本体、射撃位置、BossControllerを生成します。
        /// </summary>
        private static BossController CreateBoss(Transform player, Projectile projectilePrefab)
        {
            GameObject bossObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bossObject.name = "Boss";
            bossObject.transform.position = new Vector3(0f, 5f, 38f);
            bossObject.transform.localScale = new Vector3(4f, 2f, 4f);
            bossObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial("BossMat", new Color(0.92f, 0.28f, 0.24f));
            SphereCollider collider = bossObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            Rigidbody rigidbody = bossObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            Transform shotOrigin = new GameObject("ShotOrigin").transform;
            shotOrigin.SetParent(bossObject.transform);
            shotOrigin.localPosition = new Vector3(0f, 0f, -0.8f);

            BossController boss = bossObject.AddComponent<BossController>();
            boss.Configure(player, projectilePrefab, shotOrigin);
            return boss;
        }

        /// <summary>
        /// ビル群、屋上ルート、橋、障害物、壁グレイズを生成します。
        /// </summary>
        private static void CreateStage(Transform buildingRoot, Transform obstacleRoot, Transform grazeRoot)
        {
            GameObject chasmFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chasmFloor.name = "DeepChasmVisualFloor";
            chasmFloor.transform.position = new Vector3(0f, -20f, 72f);
            chasmFloor.transform.localScale = new Vector3(34f, 1f, 190f);
            chasmFloor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("ChasmMat", new Color(0.06f, 0.065f, 0.075f));
            Object.DestroyImmediate(chasmFloor.GetComponent<BoxCollider>());
            chasmFloor.transform.SetParent(buildingRoot);

            Vector3[] roofPositions =
            {
                new Vector3(0f, 3f, 0f),
                new Vector3(-3.8f, 4.2f, 16f),
                new Vector3(4.4f, 5.3f, 33f),
                new Vector3(0.5f, 6.6f, 51f),
                new Vector3(-4.8f, 7.8f, 70f),
                new Vector3(4.2f, 8.9f, 91f),
                new Vector3(0f, 10.2f, 114f),
                new Vector3(-3.5f, 11.4f, 139f)
            };

            Vector3[] roofScales =
            {
                new Vector3(10f, 6f, 14f),
                new Vector3(8f, 8.4f, 11f),
                new Vector3(9f, 10.6f, 13f),
                new Vector3(7f, 13.2f, 12f),
                new Vector3(8.5f, 15.6f, 14f),
                new Vector3(7.5f, 17.8f, 13f),
                new Vector3(10f, 20.4f, 16f),
                new Vector3(8f, 22.8f, 14f)
            };

            for (int i = 0; i < roofPositions.Length; i++)
            {
                GameObject roof = CreateBuilding(buildingRoot, roofPositions[i], roofScales[i]);
                roof.name = $"RoofRouteBlock_{i:00}";
            }

            for (int i = 0; i < roofPositions.Length - 1; i++)
            {
                Vector3 from = roofPositions[i];
                Vector3 to = roofPositions[i + 1];
                Vector3 mid = (from + to) * 0.5f;
                bool makeBridge = i == 1 || i == 4;
                if (!makeBridge)
                {
                    continue;
                }

                GameObject bridge = CreateBuilding(
                    buildingRoot,
                    new Vector3(mid.x, Mathf.Min(from.y + roofScales[i].y * 0.5f, to.y + roofScales[i + 1].y * 0.5f) - 0.25f, mid.z),
                    new Vector3(2.6f, 0.5f, Vector3.Distance(from, to) * 0.75f));
                bridge.name = $"NarrowSkyBridge_{i:00}";
                bridge.transform.rotation = Quaternion.LookRotation((to - from).normalized, Vector3.up);
            }

            for (int i = 0; i < 18; i++)
            {
                float z = i * 9f + 4f;
                float leftHeight = 14f + (i % 5) * 3f;
                float rightHeight = 16f + ((i + 2) % 5) * 3f;
                CreateBuilding(buildingRoot, new Vector3(-15f - (i % 2) * 2f, leftHeight * 0.5f, z), new Vector3(8f, leftHeight, 10f)).name = $"LeftCanyonBuilding_{i:00}";
                CreateBuilding(buildingRoot, new Vector3(15f + (i % 3), rightHeight * 0.5f, z + 3f), new Vector3(8f, rightHeight, 10f)).name = $"RightCanyonBuilding_{i:00}";
            }

            for (int i = 0; i < 9; i++)
            {
                Vector3 roof = roofPositions[Mathf.Min(i, roofPositions.Length - 1)];
                Vector3 scale = roofScales[Mathf.Min(i, roofScales.Length - 1)];
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = $"RooftopObstacle_{i:00}";
                obstacle.transform.position = new Vector3(roof.x + (i % 2 == 0 ? -1.8f : 1.8f), roof.y + scale.y * 0.5f + 0.8f, roof.z + 2f);
                obstacle.transform.localScale = new Vector3(1.8f, 1.6f, 1.4f);
                obstacle.GetComponent<Renderer>().sharedMaterial = CreateMaterial("ObstacleMat", new Color(0.95f, 0.7f, 0.16f));
                obstacle.GetComponent<BoxCollider>().isTrigger = true;
                obstacle.AddComponent<Obstacle>();
                obstacle.transform.SetParent(obstacleRoot);
            }

            CreateWallGrazeTriggers(grazeRoot, roofPositions, roofScales);
        }

        /// <summary>
        /// 建物の側面に壁走り用グレイズトリガーを配置します。
        /// </summary>
        private static void CreateWallGrazeTriggers(Transform grazeRoot, Vector3[] roofPositions, Vector3[] roofScales)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 roof = roofPositions[Mathf.Min(i + 1, roofPositions.Length - 1)];
                Vector3 scale = roofScales[Mathf.Min(i + 1, roofScales.Length - 1)];
                GameObject wallGraze = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallGraze.name = $"WallGraze_{i:00}";
                float side = i % 2 == 0 ? -1f : 1f;
                float visualThickness = 0.3f;
                float detectionThickness = 3.6f;
                wallGraze.transform.position = new Vector3(roof.x + side * (scale.x * 0.5f + 0.15f), roof.y + scale.y * 0.5f - 1.6f, roof.z);
                wallGraze.transform.localScale = new Vector3(visualThickness, 3.2f, scale.z * 0.7f);
                wallGraze.GetComponent<Renderer>().sharedMaterial = CreateMaterial("GrazeMat", new Color(0.35f, 1f, 0.45f));
                BoxCollider wallCollider = wallGraze.GetComponent<BoxCollider>();
                wallCollider.isTrigger = true;
                wallCollider.size = new Vector3(detectionThickness / visualThickness, 1f, 1f);
                wallCollider.center = new Vector3(side * ((detectionThickness * 0.5f - 0.05f) - 0.15f) / visualThickness, 0f, 0f);
                GrazeTrigger grazeTrigger = wallGraze.AddComponent<GrazeTrigger>();
                grazeTrigger.ConfigureWallRunDetection(-side * 0.5f, detectionThickness);
                wallGraze.transform.SetParent(grazeRoot);
            }
        }

        /// <summary>
        /// 既存シーンのGrazeRootだけを壁グレイズ配置で更新します。
        /// </summary>
        [MenuItem("Parkour3DShooting/Update Graze Walls Only")]
        public static void UpdateGrazeWallsOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameObject grazeRootObject = GameObject.Find("GrazeRoot");
            if (grazeRootObject == null)
            {
                Debug.LogError("GrazeRoot was not found.");
                return;
            }

            Transform grazeRoot = grazeRootObject.transform;
            for (int i = grazeRoot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(grazeRoot.GetChild(i).gameObject);
            }

            Vector3[] roofPositions = new Vector3[8];
            Vector3[] roofScales = new Vector3[8];
            for (int i = 0; i < roofPositions.Length; i++)
            {
                GameObject roof = GameObject.Find($"RoofRouteBlock_{i:00}");
                if (roof == null)
                {
                    Debug.LogError($"RoofRouteBlock_{i:00} was not found.");
                    return;
                }

                roofPositions[i] = roof.transform.position;
                roofScales[i] = roof.transform.localScale;
            }

            CreateWallGrazeTriggers(grazeRoot, roofPositions, roofScales);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Graze triggers updated to wall-only placement.");
        }

        /// <summary>
        /// 建物用の直方体を生成して指定親へ配置します。
        /// </summary>
        private static GameObject CreateBuilding(Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "PrimitiveBuilding";
            building.transform.position = position;
            building.transform.localScale = scale;
            building.GetComponent<Renderer>().sharedMaterial = CreateMaterial("BuildingMat", new Color(0.33f, 0.31f, 0.37f));
            building.transform.SetParent(parent);
            return building;
        }

        /// <summary>
        /// シーン用の平行光源を生成します。
        /// </summary>
        private static void CreateLighting()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        /// <summary>
        /// メインカメラ、Cinemachineカメラ、フォールバック追従リグを生成します。
        /// </summary>
        private static void CreateCamera(Transform player)
        {
            GameObject cameraRoot = new GameObject("CameraRoot");
            Transform cameraTarget = CreateStableCameraTarget(cameraRoot.transform, player);

            GameObject mainCameraObject = new GameObject("Main Camera");
            mainCameraObject.tag = "MainCamera";
            mainCameraObject.transform.SetParent(cameraRoot.transform);
            UnityEngine.Camera camera = mainCameraObject.AddComponent<UnityEngine.Camera>();
            camera.fieldOfView = 68f;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            GameObject followCameraObject = new GameObject("Cinemachine Follow Camera");
            followCameraObject.transform.SetParent(cameraRoot.transform);
            followCameraObject.transform.position = player.position + FollowCameraOffset;
            CinemachineCamera virtualCamera = followCameraObject.AddComponent<CinemachineCamera>();
            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
            CinemachineFollow follow = followCameraObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = FollowCameraOffset;
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            CinemachineHardLookAt hardLookAt = followCameraObject.AddComponent<CinemachineHardLookAt>();
            hardLookAt.LookAtOffset = new Vector3(0f, 0f, 90f);

            CameraFollowRig fallbackRig = followCameraObject.AddComponent<CameraFollowRig>();
            fallbackRig.Configure(cameraTarget, FollowCameraOffset, FollowCameraLookHeight);
        }

        /// <summary>
        /// プレイヤーの回転に影響されないカメラ追従ターゲットを生成します。
        /// </summary>
        private static Transform CreateStableCameraTarget(Transform parent, Transform player)
        {
            GameObject targetObject = new GameObject(CameraTargetName);
            targetObject.transform.SetParent(parent);
            StableCameraTarget stableTarget = targetObject.AddComponent<StableCameraTarget>();
            stableTarget.Configure(player, Vector3.zero);
            return targetObject.transform;
        }

        /// <summary>
        /// 既存シーンのCinemachine参照を、安定カメラターゲットへ更新します。
        /// </summary>
        [MenuItem("Parkour3DShooting/Update Camera Stable Target")]
        public static void UpdateCameraStableTarget()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameObject playerObject = GameObject.Find("Player");
            GameObject followCameraObject = GameObject.Find("Cinemachine Follow Camera");
            GameObject cameraRootObject = GameObject.Find("CameraRoot");
            if (playerObject == null || followCameraObject == null || cameraRootObject == null)
            {
                Debug.LogError("Player, Cinemachine Follow Camera, or CameraRoot was not found.");
                return;
            }

            GameObject targetObject = GameObject.Find(CameraTargetName);
            if (targetObject == null)
            {
                targetObject = new GameObject(CameraTargetName);
                targetObject.transform.SetParent(cameraRootObject.transform);
            }

            StableCameraTarget stableTarget = targetObject.GetComponent<StableCameraTarget>();
            if (stableTarget == null)
            {
                stableTarget = targetObject.AddComponent<StableCameraTarget>();
            }

            stableTarget.Configure(playerObject.transform, Vector3.zero);
            CinemachineCamera virtualCamera = followCameraObject.GetComponent<CinemachineCamera>();
            if (virtualCamera != null)
            {
                virtualCamera.Follow = targetObject.transform;
                virtualCamera.LookAt = targetObject.transform;
            }

            CinemachineFollow follow = followCameraObject.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            }

            CameraFollowRig fallbackRig = followCameraObject.GetComponent<CameraFollowRig>();
            if (fallbackRig != null)
            {
                fallbackRig.Configure(targetObject.transform, FollowCameraOffset, FollowCameraLookHeight);
            }

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Camera target updated to stable player target.");
        }

        /// <summary>
        /// HP、スコア、リザルトのUI一式を生成します。
        /// </summary>
        private static void CreateUi(PlayerController player, out ResultView resultView, out HpView hpView, out ScoreView scoreView)
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject uiRoot = new GameObject("UIRoot");
            uiRoot.transform.SetParent(canvasObject.transform, false);

            Text hpText = CreateText("HpText", uiRoot.transform, new Vector2(24f, -24f), TextAnchor.UpperLeft, "HP 0/0", 28);
            hpView = hpText.gameObject.AddComponent<HpView>();
            hpView.Configure(player, hpText);

            Text scoreText = CreateText("ScoreText", uiRoot.transform, new Vector2(-24f, -24f), TextAnchor.UpperRight, "SCORE 000000", 28);
            scoreView = scoreText.gameObject.AddComponent<ScoreView>();

            GameObject resultObject = new GameObject("Result UI");
            resultObject.transform.SetParent(uiRoot.transform, false);
            RectTransform resultRect = resultObject.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = Vector2.zero;
            resultRect.sizeDelta = new Vector2(520f, 220f);
            CanvasGroup canvasGroup = resultObject.AddComponent<CanvasGroup>();
            Image panel = resultObject.AddComponent<Image>();
            panel.color = new Color(0f, 0f, 0f, 0.72f);

            Text titleText = CreateText("ResultTitle", resultObject.transform, new Vector2(0f, -42f), TextAnchor.MiddleCenter, "STAGE CLEAR", 42);
            Text resultScoreText = CreateText("ResultScore", resultObject.transform, new Vector2(0f, -108f), TextAnchor.MiddleCenter, "SCORE 000000", 30);
            ResultOpenAnimation animation = resultObject.AddComponent<ResultOpenAnimation>();
            animation.Configure(canvasGroup, resultRect);
            resultView = resultObject.AddComponent<ResultView>();
            resultView.Configure(canvasGroup, titleText, resultScoreText, animation);
            resultView.HideImmediate();
        }

        /// <summary>
        /// Unity標準UI Textを指定位置と整列で生成します。
        /// </summary>
        private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, TextAnchor alignment, string text, int fontSize)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = AlignmentToAnchor(alignment);
            rectTransform.anchorMax = AlignmentToAnchor(alignment);
            rectTransform.pivot = AlignmentToAnchor(alignment);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(460f, 64f);

            Text uiText = textObject.AddComponent<Text>();
            uiText.text = text;
            uiText.font = GetBuiltinFont();
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.color = Color.white;
            return uiText;
        }

        /// <summary>
        /// TextAnchorに対応するRectTransformのアンカー位置を返します。
        /// </summary>
        private static Vector2 AlignmentToAnchor(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(0f, 1f);
                case TextAnchor.UpperRight:
                    return new Vector2(1f, 1f);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Unity環境で利用可能な組み込みフォントを取得します。
        /// </summary>
        private static Font GetBuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        /// <summary>
        /// 指定名のマテリアルを取得または作成します。
        /// </summary>
        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = name,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
