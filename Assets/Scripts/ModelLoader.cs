using UnityEngine;

public class ModelLoader : MonoBehaviour {

    [SerializeField] private Material _diffuse;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _sphere1;
    [SerializeField] private GameObject _sphere2;
    [SerializeField] private GuiManager _gui;

    private const float MouseThreshold = 0.2f;
    private const float RotateCoef = 5;
    private const float MaxMagnitude = 0.1f;

    private GameObject _importedObj;
    private MeshFilter _meshFilter;

    private bool _isRightMouseButtonPressed;

    public delegate void SwitchPointsAction();
    public SwitchPointsAction PointsSwitched;

    public delegate void LengthCalculatedAction(float length);
    public LengthCalculatedAction LengthCalculated;

    private Vector3 _firstPointValue = default (Vector3);

	void Start () 
    {
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
            
            _importedObj.transform.parent = this.transform;
            _importedObj.transform.localPosition = new Vector3(0,0,-1);
            _importedObj.transform.Rotate(Vector3.up, 180);
            _meshFilter = _importedObj.GetComponent<MeshFilter>();
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
                 transform.Rotate(Vector3.up, -mousex * RotateCoef);
             }
             else if (mousex < -MouseThreshold)
             {
                 transform.Rotate(Vector3.up, -mousex * RotateCoef);
             }

             if (mousey > MouseThreshold)
             {
                 transform.Rotate(Vector3.right, mousey * RotateCoef);
             }
             else if (mousey < -MouseThreshold)
             {
                 transform.Rotate(Vector3.right, mousey * RotateCoef);
             } 
         }
         if (Input.GetMouseButton(1) && _importedObj != null &&!_isRightMouseButtonPressed)
         {
             _isRightMouseButtonPressed = true;
             Ray ray = _camera.ScreenPointToRay (new Vector3(Input.mousePosition.x,Input.mousePosition.y,0));
             float minDist = float.MaxValue;
             Vector3[] vertices = _meshFilter.mesh.vertices;
             Vector3 closestVertex = default(Vector3);
             foreach (var vertex in vertices)
             {
                 Vector3 realVertex = new Vector3(vertex.x*-1, vertex.y, vertex.z*-1) + _importedObj.transform.localPosition;
                 float currentDist = DistanceToLine(ray, realVertex);
                 if (currentDist < MaxMagnitude)
                 {
                     if (currentDist < minDist)
                     {
                         minDist = currentDist;
                         closestVertex = realVertex;
                     }   
                 }
             }
             if (closestVertex != default (Vector3))
             {
                 if (_gui.IsFirstPointSelected())
                 {
                     _sphere2.gameObject.SetActive(false);
                     _sphere1.transform.position = closestVertex;
                     _sphere1.gameObject.SetActive(true);
                     _firstPointValue = closestVertex;
                     if (PointsSwitched != null)
                         PointsSwitched();
                 }
                 else
                 {
                     _sphere2.transform.position = closestVertex;
                     _sphere2.gameObject.SetActive(true);
                     float length = (_firstPointValue - closestVertex).magnitude;
                     if (LengthCalculated != null)
                         LengthCalculated(length);
                 }
             }
         }
         if (!Input.GetMouseButton(1))
             _isRightMouseButtonPressed = false;
     }

     private static float DistanceToLine(Ray ray, Vector3 point)
     {
         return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
     }

}
