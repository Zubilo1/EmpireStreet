#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Положи этот файл в Assets/Editor/
public static class StreetEmpireBuilder
{
    static Material matRoad, matGrass, matBuilding, matShop, matPlayer, matCitizen, matPolice, matCar, matWhite;

    [MenuItem("Street Empire/Build Prototype Scene")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        matRoad = Mat("Road", new Color(.15f,.16f,.18f));
        matGrass = Mat("Grass", new Color(.18f,.34f,.19f));
        matBuilding = Mat("Building", new Color(.48f,.49f,.53f));
        matShop = Mat("Shop", new Color(.2f,.48f,.72f));
        matPlayer = Mat("Player", new Color(.15f,.85f,.92f));
        matCitizen = Mat("Citizen", new Color(.9f,.65f,.25f));
        matPolice = Mat("Police", new Color(.1f,.2f,.85f));
        matCar = Mat("Car", new Color(.8f,.15f,.17f));
        matWhite = Mat("White", Color.white);

        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("PrototypeUI").AddComponent<PrototypeUI>();

        // ВАЖНО: у земли должен быть коллайдер, иначе игрок провалится
        MakeCube("Ground", new Vector3(0,-.2f,0), new Vector3(70,.3f,70), matGrass, true);
        MakeCube("Main Road", new Vector3(0,0,0), new Vector3(12,.12f,70), matRoad, false);
        MakeCube("Cross Road", new Vector3(0,.01f,0), new Vector3(70,.12f,10), matRoad, false);
        for (int z=-30; z<=30; z+=8) MakeCube("Road Marking", new Vector3(0,.09f,z), new Vector3(.18f,.03f,3), matWhite, false);

        // Кварталы (теперь с коллайдерами)
        for (int x=-25; x<=25; x+=10)
        for (int z=-25; z<=25; z+=10)
        {
            if (Mathf.Abs(x)<8 || Mathf.Abs(z)<7) continue;
            float height = Random.Range(3.5f, 9f);
            MakeCube("Building", new Vector3(x,height/2,z), new Vector3(6,height,6), matBuilding, true);
        }

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(2,1,2);
        player.GetComponent<Renderer>().sharedMaterial = matPlayer;
        Object.DestroyImmediate(player.GetComponent<Collider>()); // CharacterController заменяет CapsuleCollider
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController3D>();

        MakeInteractable("Мини-маркет", new Vector3(9,1,2), new Vector3(4,2,4), matShop, InteractionType.Shop, 0);
        MakeInteractable("Квартира на продажу", new Vector3(-10,1,8), new Vector3(4,2,4), matBuilding, InteractionType.Apartment, 800);
        MakeInteractable("Бизнес на продажу", new Vector3(12,1,-10), new Vector3(4,2,4), matShop, InteractionType.Business, 1500);
        MakeInteractable("Подработка", new Vector3(-9,1,-8), new Vector3(3,2,3), matCitizen, InteractionType.Job, 0);

        for (int i=0;i<6;i++)
        {
            Vector3 p = new Vector3(Random.Range(-5,5),.9f,Random.Range(-5,5));
            if (Vector3.Distance(p, player.transform.position)<3) p += new Vector3(5,0,5);
            // Капсула высотой 2 * scale.y 0.9 = 1.8, центр на y=0.9 — стоит на земле
            MakeInteractable("Прохожий", p, new Vector3(.8f,.9f,.8f), matCitizen, InteractionType.Citizen, 0, PrimitiveType.Capsule);
        }

        var car = MakeInteractable("Автомобиль", new Vector3(5,.6f,-5), new Vector3(2,1,3.5f), matCar, InteractionType.Vehicle, 0);
        car.AddComponent<VehicleController>();

        var police = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        police.name = "Police";
        police.transform.position = new Vector3(-8,1,18);
        police.GetComponent<Renderer>().sharedMaterial = matPolice;
        var ai = police.AddComponent<PoliceAI>();
        ai.Target = player.transform;

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0,18,-15);
        cameraObject.transform.rotation = Quaternion.Euler(48,0,0);
        var follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        player.GetComponent<PlayerController3D>().cameraTransform = cameraObject.transform;

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(50,-30,0);

        EditorSceneManager.SaveScene(scene, "Assets/StreetEmpirePrototype.unity");
        Selection.activeGameObject = player;
        Debug.Log("Street Empire prototype scene created. Press Play.");
    }

    // Материалы сохраняются как ассеты, чтобы не пропадали (розовые) после перезагрузки сцены
    static Material Mat(string name, Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        string path = "Assets/Materials/SE_" + name + ".mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static GameObject MakeCube(string name, Vector3 pos, Vector3 scale, Material mat, bool collider=true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject MakeInteractable(string name, Vector3 pos, Vector3 scale, Material mat, InteractionType type, int price, PrimitiveType primitive=PrimitiveType.Cube)
    {
        var go=GameObject.CreatePrimitive(primitive);
        go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        var interact=go.AddComponent<Interactable>();
        interact.type=type; interact.displayName=name; interact.price=price;
        return go;
    }
}
#endif
