#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class StreetEmpireWorldBuilder
{
    const string Root = "Assets/StreetEmpireComplete";
    const string ScenePath = "Assets/Scenes/StreetEmpirePrototype_Generated.unity";
    const string MaterialsPath = Root + "/Materials";
    static Material asphalt, concrete, glass, brick, blue, black, white, green, skin, policeBlue, yellow, roadWhite, dark, red, shirtGreen, shirtRed;

    [MenuItem("Street Empire/Build First-Person Open City")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("Street Empire — Open City", "Создать новую сцену: вид от первого лица, большой город, здания с фасадами и люди с головой, туловищем, руками и ногами? Сцены меню не изменяются.", "Создать", "Отмена")) return;
        EnsureFolders();
        asphalt=Mat("Asphalt",new Color(.105f,.12f,.135f)); concrete=Mat("Concrete",new Color(.48f,.49f,.48f)); glass=Mat("Blue Glass",new Color(.055f,.19f,.26f)); brick=Mat("Warm Brick",new Color(.40f,.29f,.25f)); blue=Mat("Player Jacket",new Color(.06f,.22f,.75f)); black=Mat("Rubber",new Color(.025f,.028f,.032f)); white=Mat("Police White",new Color(.9f,.91f,.9f)); green=Mat("Grass",new Color(.16f,.29f,.14f)); skin=Mat("Skin",new Color(.72f,.46f,.31f)); policeBlue=Mat("Police Navy",new Color(.025f,.07f,.22f)); yellow=Mat("Road Yellow",new Color(.95f,.7f,.08f)); roadWhite=Mat("Road White",new Color(.82f,.83f,.8f)); dark=Mat("Dark Trim",new Color(.08f,.085f,.09f)); red=Mat("Shirt Red",new Color(.65f,.09f,.08f)); shirtGreen=Mat("Shirt Green",new Color(.08f,.38f,.22f)); shirtRed=Mat("Shirt Burgundy",new Color(.36f,.045f,.12f));
        Random.InitState(7319);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); scene.name="StreetEmpire_FirstPersonCity";
        BuildCity();
        var player=BuildPlayer(new Vector3(0,.15f,-12f));
        var cameraObj=new GameObject("First Person Camera"); cameraObj.tag="MainCamera"; cameraObj.transform.SetParent(player.transform); cameraObj.transform.localPosition=new Vector3(0,1.62f,0); cameraObj.transform.localRotation=Quaternion.identity; var camera=cameraObj.AddComponent<Camera>(); camera.nearClipPlane=.03f; camera.fieldOfView=82f; cameraObj.AddComponent<AudioListener>(); var cam=cameraObj.AddComponent<StreetEmpireCamera>(); cam.target=player.transform; cam.eyeOffset=new Vector3(0,1.62f,0); player.GetComponent<StreetEmpirePlayer>().cameraPivot=cameraObj.transform;
        BuildFirstPersonArms(cameraObj.transform);
        var gm=new GameObject("Game Manager"); var manager=gm.AddComponent<StreetEmpireGameManager>(); manager.player=player.transform;
        BuildCar(new Vector3(5,.15f,-10),"Starter Sedan",blue); BuildCar(new Vector3(-7,.15f,15),"City Sedan",white); BuildCar(new Vector3(28,.15f,30),"Sports Coupe",red);
        BuildPolice(new Vector3(34,.15f,18),player.transform); BuildPolice(new Vector3(-45,.15f,-42),player.transform);
        BuildCitizens(70); BuildLighting();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene,ScenePath); AddSceneToBuildSettings(ScenePath); Selection.activeGameObject=player;
        EditorUtility.DisplayDialog("Street Empire", "Готово. Открыта новая сцена от первого лица. Открой Assets/Scenes/StreetEmpirePrototype_Generated.unity и нажми Play. WASD — движение, мышь — обзор, Shift — бег, Space — прыжок, E — машина.","Окей");
    }

    static void EnsureFolders(){ if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes"); if(!AssetDatabase.IsValidFolder(Root+"/Materials")) AssetDatabase.CreateFolder(Root,"Materials"); }
    static Material Mat(string name, Color color) { string path = MaterialsPath + "/" + name.Replace(" ", "_") + ".mat"; var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (m) { m.color = color; EditorUtility.SetDirty(m); return m; } m = new Material(Shader.Find("Standard")); m.name = name; m.color = color; AssetDatabase.CreateAsset(m, path); return m; }
    static GameObject Prim(PrimitiveType type,string name,Vector3 pos,Vector3 scale,Material mat,Transform parent=null,bool collider=true){ var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.position=pos;o.transform.localScale=scale;if(parent)o.transform.SetParent(parent,true);var r=o.GetComponent<Renderer>();if(r&&mat)r.sharedMaterial=mat;if(!collider){var c=o.GetComponent<Collider>();if(c)Object.DestroyImmediate(c);}return o; }
    static GameObject Box(string name,Vector3 pos,Vector3 scale,Material mat,bool collider=true){return Prim(PrimitiveType.Cube,name,pos,scale,mat,null,collider);}

    static void BuildCity()
    {
        Box("Ground — city limits",new Vector3(0,-.5f,0),new Vector3(330,1,330),green);
        // 11 x 11 avenues: approx 300 x 300 metres of traversable city.
        for(int i=-5;i<=5;i++){
            float p=i*28f; Box("Avenue N-S",new Vector3(p,.015f,0),new Vector3(9,.13f,320),asphalt); Box("Avenue E-W",new Vector3(0,.02f,p),new Vector3(320,.13f,9),asphalt);
            for(int z=-154;z<=154;z+=12){Box("Dashed lane marking",new Vector3(p,.09f,z),new Vector3(.13f,.025f,5f),yellow,false);Box("Crosswalk stripe",new Vector3(p+5.1f,.095f,z),new Vector3(.65f,.025f,2.2f),roadWhite,false);}
            for(int x=-154;x<=154;x+=12){Box("Dashed lane marking",new Vector3(x,.095f,p),new Vector3(5f,.025f,.13f),yellow,false);}
        }
        // City blocks filled with a varied skyline, sidewalks, storefronts and windows.
        for(int ix=-5;ix<5;ix++) for(int iz=-5;iz<5;iz++){
            float cx=ix*28f+14f, cz=iz*28f+14f;
            if(Mathf.Abs(cx)<23 && Mathf.Abs(cz)<23){ BuildPlaza(cx,cz); continue; }
            Box("Sidewalk block",new Vector3(cx,.11f,cz),new Vector3(17.5f,.22f,17.5f),concrete,false);
            int count=Random.Range(1,4);
            for(int b=0;b<count;b++){
                float w=Random.Range(5.5f,8.8f), d=Random.Range(5.5f,8.8f), h=Random.Range(9f,48f);
                float ox=count==1?0f:(b-(count-1)*.5f)*5.4f; float oz=Random.Range(-3.3f,3.3f);
                Vector3 center=new Vector3(cx+ox,h/2f+.22f,cz+oz); Material facade=(b%4==0)?brick:((b%4==1)?concrete:((b%4==2)?dark:white));
                var building=Box("Building — facade",center,new Vector3(w,h,d),facade);
                // storefront base and facade bands
                Box("Storefront",new Vector3(center.x,.9f,center.z-d/2f-.04f),new Vector3(w*.78f,1.5f,.09f),glass,false);
                Box("Facade ledge",new Vector3(center.x,2.0f,center.z-d/2f-.08f),new Vector3(w+.15f,.16f,.16f),white,false);
                int floors=Mathf.FloorToInt((h-3f)/3.1f);
                for(int floor=0;floor<floors;floor++) for(int col=-1;col<=1;col++){
                    float wx=center.x+col*w*.28f, wy=3f+floor*3.05f;
                    Box("Window frame",new Vector3(wx,wy,center.z-d/2f-.055f),new Vector3(1.35f,1.55f,.10f),dark,false);
                    Box("Glass window",new Vector3(wx,wy,center.z-d/2f-.115f),new Vector3(1.12f,1.3f,.035f),glass,false);
                    Box("Window divider",new Vector3(wx,wy,center.z-d/2f-.14f),new Vector3(.045f,1.25f,.025f),white,false);
                }
                // roof machinery makes the skyline less boxy
                if(h>20) Box("Roof equipment",new Vector3(center.x,h+.55f,center.z),new Vector3(w*.32f,1.1f,d*.28f),dark,false);
            }
            // Street lamps and trees at block corners
            if((ix+iz)%2==0){ BuildTree(new Vector3(cx-8f,.15f,cz+8f)); BuildLamp(new Vector3(cx+8f,.1f,cz-8f)); }
        }
        // Decorative skyline landmark
        Box("Downtown tower",new Vector3(0,29f,0),new Vector3(11,58,11),dark);
        for(int y=5;y<55;y+=4) for(int x=-1;x<=1;x++) Box("Tower windows",new Vector3(x*2.8f,y, -5.56f),new Vector3(1.7f,1.6f,.04f),glass,false);
    }
    static void BuildPlaza(float x,float z){ Box("Central plaza",new Vector3(x,.22f,z),new Vector3(17,.25f,17),concrete,false); for(int i=-1;i<=1;i++){Prim(PrimitiveType.Cylinder,"Planter",new Vector3(x+i*5,.55f,z),new Vector3(1.5f,.45f,1.5f),dark,null,false); BuildTree(new Vector3(x+i*5,.8f,z));} }
    static void BuildTree(Vector3 p){Prim(PrimitiveType.Cylinder,"Tree trunk",p+Vector3.up*.9f,new Vector3(.3f,1.8f,.3f),brick,null,false);Prim(PrimitiveType.Sphere,"Tree canopy",p+Vector3.up*2.5f,new Vector3(2.1f,2.3f,2.1f),green,null,false);}
    static void BuildLamp(Vector3 p){Prim(PrimitiveType.Cylinder,"Street lamp pole",p+Vector3.up*2f,new Vector3(.09f,4f,.09f),dark,null,false);Prim(PrimitiveType.Sphere,"Street lamp",p+Vector3.up*4.1f,new Vector3(.35f,.25f,.35f),yellow,null,false);}

    static GameObject BuildPlayer(Vector3 pos){
        var root=new GameObject("Player — First Person");root.transform.position=pos;var cc=root.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.34f;cc.center=new Vector3(0,.9f,0);root.AddComponent<StreetEmpirePlayer>();
        // A body silhouette exists in-world; head is hidden from the first-person view to prevent camera clipping.
        var body=Prim(PrimitiveType.Capsule,"Player torso",pos+Vector3.up*1.05f,new Vector3(.48f,.52f,.30f),blue,root.transform,false);body.transform.localPosition=new Vector3(0,1.05f,0);
        var head=Prim(PrimitiveType.Sphere,"Player head",pos+Vector3.up*1.68f,new Vector3(.42f,.46f,.42f),skin,root.transform,false);head.transform.localPosition=new Vector3(0,1.68f,0);head.GetComponent<Renderer>().enabled=false;
        AddLimb(root.transform,"Player left leg",new Vector3(-.16f,.82f,0),new Vector3(-.18f,.18f,0),.12f,blue); AddLimb(root.transform,"Player right leg",new Vector3(.16f,.82f,0),new Vector3(.18f,.18f,0),.12f,blue);
        AddLimb(root.transform,"Player left arm",new Vector3(-.28f,1.28f,0),new Vector3(-.42f,.72f,.02f),.09f,blue); AddLimb(root.transform,"Player right arm",new Vector3(.28f,1.28f,0),new Vector3(.42f,.72f,.02f),.09f,blue);
        return root;
    }
    static void BuildFirstPersonArms(Transform camera){
        var armMat=blue;
        var left=Prim(PrimitiveType.Capsule,"FPS left forearm",Vector3.zero,new Vector3(.12f,.34f,.12f),armMat,camera,false);left.transform.localPosition=new Vector3(-.32f,-.36f,.48f);left.transform.localRotation=Quaternion.Euler(-28f,0f,-16f);
        var right=Prim(PrimitiveType.Capsule,"FPS right forearm",Vector3.zero,new Vector3(.12f,.34f,.12f),armMat,camera,false);right.transform.localPosition=new Vector3(.32f,-.36f,.48f);right.transform.localRotation=Quaternion.Euler(-28f,0f,16f);
        Prim(PrimitiveType.Sphere,"FPS left hand",Vector3.zero,new Vector3(.17f,.15f,.15f),skin,camera,false).transform.localPosition=new Vector3(-.27f,-.48f,.72f);
        Prim(PrimitiveType.Sphere,"FPS right hand",Vector3.zero,new Vector3(.17f,.15f,.15f),skin,camera,false).transform.localPosition=new Vector3(.27f,-.48f,.72f);
    }
    static void AddLimb(Transform parent,string name,Vector3 a,Vector3 b,float radius,Material mat){Vector3 delta=b-a;var limb=Prim(PrimitiveType.Capsule,name,parent.position+(a+b)*.5f,new Vector3(radius,delta.magnitude*.5f+radius,radius),mat,parent,false);limb.transform.localPosition=(a+b)*.5f;limb.transform.localRotation=Quaternion.FromToRotation(Vector3.up,delta.normalized);}

    static void BuildHumanoid(Vector3 p,int style,string name){
        var root=new GameObject(name);root.transform.position=p;Material shirt=style%5==0?red:style%5==1?shirtGreen:style%5==2?shirtRed:style%5==3?blue:dark;Material pants=style%3==0?dark:policeBlue;Material tone=skin;
        Prim(PrimitiveType.Capsule,"Torso",p+Vector3.up*1.08f,new Vector3(.43f,.48f,.28f),shirt,root.transform,false).transform.localPosition=new Vector3(0,1.08f,0);
        Prim(PrimitiveType.Sphere,"Head",p+Vector3.up*1.78f,new Vector3(.34f,.38f,.34f),tone,root.transform,false).transform.localPosition=new Vector3(0,1.78f,0);
        // neck, arms, legs and shoes give the NPC a recognisable human silhouette
        Prim(PrimitiveType.Cylinder,"Neck",p+Vector3.up*1.48f,new Vector3(.11f,.14f,.11f),tone,root.transform,false).transform.localPosition=new Vector3(0,1.48f,0);
        AddLimb(root.transform,"Left upper arm",new Vector3(-.25f,1.32f,0),new Vector3(-.34f,.82f,.02f),.085f,shirt); AddLimb(root.transform,"Right upper arm",new Vector3(.25f,1.32f,0),new Vector3(.34f,.82f,.02f),.085f,shirt);
        Prim(PrimitiveType.Sphere,"Left hand",p+new Vector3(-.34f,.78f,.02f),new Vector3(.13f,.13f,.13f),tone,root.transform,false).transform.localPosition=new Vector3(-.34f,.78f,.02f); Prim(PrimitiveType.Sphere,"Right hand",p+new Vector3(.34f,.78f,.02f),new Vector3(.13f,.13f,.13f),tone,root.transform,false).transform.localPosition=new Vector3(.34f,.78f,.02f);
        AddLimb(root.transform,"Left trouser leg",new Vector3(-.13f,.76f,0),new Vector3(-.15f,.18f,.015f),.105f,pants); AddLimb(root.transform,"Right trouser leg",new Vector3(.13f,.76f,0),new Vector3(.15f,.18f,.015f),.105f,pants);
        Prim(PrimitiveType.Cube,"Left shoe",p+new Vector3(-.15f,.08f,.08f),new Vector3(.19f,.12f,.32f),black,root.transform,false).transform.localPosition=new Vector3(-.15f,.08f,.08f); Prim(PrimitiveType.Cube,"Right shoe",p+new Vector3(.15f,.08f,.08f),new Vector3(.19f,.12f,.32f),black,root.transform,false).transform.localPosition=new Vector3(.15f,.08f,.08f);
        root.AddComponent<StreetEmpireCitizen>();
    }
    static void BuildCitizens(int count){for(int i=0;i<count;i++){Vector3 p; if(Random.value>.5f){float road=Random.Range(-5,6)*28f+((Random.value>.5f)?5.8f:-5.8f);p=new Vector3(road,.15f,Random.Range(-145f,145f));}else{float road=Random.Range(-5,6)*28f+((Random.value>.5f)?5.8f:-5.8f);p=new Vector3(Random.Range(-145f,145f),.15f,road);}BuildHumanoid(p,i,"Citizen — pedestrian");}}

    static void BuildCar(Vector3 pos,string name,Material bodyMat){var car=new GameObject(name);car.transform.position=pos;Box("Car body",pos+new Vector3(0,.55f,0),new Vector3(2f,.65f,4f),bodyMat,false).transform.SetParent(car.transform,true);Box("Car cabin",pos+new Vector3(0,1.12f,-.15f),new Vector3(1.45f,.65f,1.8f),glass,false).transform.SetParent(car.transform,true);for(int side=-1;side<=1;side+=2)for(int front=-1;front<=1;front+=2)Prim(PrimitiveType.Cylinder,"Wheel",pos+new Vector3(side*1.02f,.28f,front*1.25f),new Vector3(.34f,.18f,.34f),black,car.transform,false).transform.localRotation=Quaternion.Euler(0,0,90);var col=car.AddComponent<BoxCollider>();col.center=new Vector3(0,.55f,0);col.size=new Vector3(2.05f,1.25f,4.1f);car.AddComponent<Rigidbody>();car.AddComponent<StreetEmpireCar>();}
    static void BuildPolice(Vector3 pos,Transform player){var car=new GameObject("Police Patrol");car.transform.position=pos;Box("Patrol car body",pos+new Vector3(0,.5f,0),new Vector3(2,.7f,4),white,false).transform.SetParent(car.transform,true);Box("Patrol car roof",pos+new Vector3(0,1.05f,0),new Vector3(1.4f,.5f,1.6f),policeBlue,false).transform.SetParent(car.transform,true);Prim(PrimitiveType.Cube,"Police lightbar",pos+new Vector3(0,1.38f,0),new Vector3(1.1f,.15f,.28f),red,car.transform,false);var rb=car.AddComponent<Rigidbody>();rb.isKinematic=true;var col=car.AddComponent<BoxCollider>();col.center=new Vector3(0,.55f,0);col.size=new Vector3(2,1.2f,4);var ai=car.AddComponent<StreetEmpirePolice>();ai.target=player;BuildHumanoid(pos+new Vector3(2.4f,0,1f),99,"Police officer");}
    static void BuildLighting(){var sun=new GameObject("Sun Light");var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;sun.transform.rotation=Quaternion.Euler(48,-30,0);RenderSettings.ambientLight=new Color(.48f,.5f,.54f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.57f,.63f,.68f);RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogDensity=.0017f;}
    static void AddSceneToBuildSettings(string path){var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);bool exists=false;foreach(var s in list)if(s.path==path)exists=true;if(!exists){list.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=list.ToArray();}}
}
#endif
