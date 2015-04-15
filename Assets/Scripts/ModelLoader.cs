using UnityEngine;

public class ModelLoader : MonoBehaviour {

    [SerializeField] private Material _diffuse;
    [SerializeField] private Camera _camera;

    private const float MouseThreshold = 0.2f;
    private GameObject _importedObj;
    private MeshCollider _collider;
	void Start () {

        if(System.Environment.GetCommandLineArgs().Length > 1)
        {
            //string modelPath = "C:\\Explorer\\teapot.obj";
            string modelPath = System.Environment.GetCommandLineArgs()[1];
            ObjImporter importer = new ObjImporter();
            Mesh importedMesh = importer.ImportFile(modelPath);
            importedMesh.RecalculateNormals();
            _importedObj = new GameObject("mesh", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            _importedObj.GetComponent<MeshFilter>().mesh = importedMesh;
            _importedObj.GetComponent<MeshRenderer>().materials[0] = _diffuse;
            _collider = _importedObj.GetComponent<MeshCollider>();
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
                 _importedObj.transform.Rotate(Vector3.up, -mousex*10, Space.World);
             }
             else if (mousex < -MouseThreshold)
             {
                 _importedObj.transform.Rotate(Vector3.up, -mousex * 10, Space.World);
             }

             if (mousey > MouseThreshold)
             {
                 _importedObj.transform.Rotate(Vector3.right, mousey * 10, Space.World);
             }
             else if (mousey < -MouseThreshold)
             {
                 _importedObj.transform.Rotate(Vector3.right, mousey * 10, Space.World);
             }   
         }
         if (Input.GetMouseButton(1) && _importedObj != null)
         {
             Ray ray = _camera.ScreenPointToRay (new Vector3(Input.mousePosition.x,Input.mousePosition.y,0));
             RaycastHit hitInfo;
             if (_collider.Raycast(ray, out hitInfo, 100))
             {
                 Debug.Log(hitInfo.point.x);
                 Debug.Log(hitInfo.point.y);
             }
         }
     }
}
