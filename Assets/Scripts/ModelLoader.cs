using UnityEngine;

public class ModelLoader : MonoBehaviour {

    [SerializeField] private Material _diffuse;

    private const float MouseThreshold = 0.2f;
    private GameObject _importedObj;
	void Start () {

        if(System.Environment.GetCommandLineArgs().Length > 1)
        {
            string modelPath = "C:\\Users\\syjgin.MOBIRATE\\M9.obj";
            //string modelPath = System.Environment.GetCommandLineArgs()[1];
            ObjImporter importer = new ObjImporter();
            Mesh importedMesh = importer.ImportFile(modelPath);
            importedMesh.RecalculateNormals();
            _importedObj = new GameObject("mesh", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            _importedObj.GetComponent<MeshFilter>().mesh = importedMesh;
            _importedObj.GetComponent<MeshRenderer>().materials[0] = _diffuse;   
        }
	}
	
     void Update()
     {
         if(Input.GetMouseButton(0) && _importedObj != null)
         {
             float mousex = Input.GetAxis("Mouse X");
             float mousey = Input.GetAxis("Mouse Y");

             if (mousex > MouseThreshold)
             {
                 Debug.Log("moved right");
             }
             else if (mousex < -MouseThreshold)
             {
                 Debug.Log("moved left");
             }

             if (mousey > MouseThreshold)
             {
                 Debug.Log("moved up");
             }
             else if (mousey < -MouseThreshold)
             {
                 Debug.Log("moved down");
             }   
         }
     }
}
