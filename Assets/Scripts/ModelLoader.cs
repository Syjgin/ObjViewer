using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ModelLoader : MonoBehaviour {

    private const string ModelPath = "c:\\Users\\syjgin.MOBIRATE\\M9.obj";
	void Start () {

        Material diffuse = Resources.Load("Default-Material") as Material;
        ObjImporter importer = new ObjImporter();
        Mesh importedMesh = importer.ImportFile(ModelPath);
        importedMesh.RecalculateNormals();
        var importedObj = new GameObject("mesh", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
        importedObj.GetComponent<MeshFilter>().mesh = importedMesh;
        importedObj.GetComponent<MeshRenderer>().materials[0] = diffuse;
	}
	

}
