using UnityEngine;

public class ModelLoader : MonoBehaviour {

    [SerializeField] private Material _diffuse;
    
	void Start () {

        if(System.Environment.GetCommandLineArgs().Length > 1)
        {
            string modelPath = System.Environment.GetCommandLineArgs()[1];//"C:\\Users\\syjgin.MOBIRATE\\M9.obj"
            ObjImporter importer = new ObjImporter();
            Mesh importedMesh = importer.ImportFile(modelPath);
            importedMesh.RecalculateNormals();
            var importedObj = new GameObject("mesh", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            importedObj.GetComponent<MeshFilter>().mesh = importedMesh;
            importedObj.GetComponent<MeshRenderer>().materials[0] = _diffuse;   
        }
	}
	

}
