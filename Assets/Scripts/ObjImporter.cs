using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

//класс для загрузки формата .obj из текстового файла
public class ObjImporter
{
    //максимально возможное кратное 3 число вершин в Unity
    private const int MaxVertices = 64998;
    private struct meshStruct
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uv;
        public Vector2[] uv1;
        public Vector2[] uv2;
        public int[] triangles;
        public int[] faceVerts;
        public int[] faceUVs;
        public Vector3[] faceData;
        public string name;
        public string fileName;
    }

    //интерфейс класса - принимает имя файла, возвращает массив импортированных мешей
    public List<Mesh> ImportFile(string filePath)
    {
        //выяснение размеров мешей в модели
        List<meshStruct> newMeshes = createMeshStruct(filePath);
        //заполнение полученных на первом шаге массивов
        populateMeshStruct(ref newMeshes);
        List<Mesh> meshes = new List<Mesh>();
        int meshNum = 0;
        foreach (var meshStruct1 in newMeshes)
        {
            int length = Mathf.Min(meshStruct1.faceData.Length, MaxVertices);
            //подготовка вершин, нормалей и текстурных координат к экспорту в формат Unity
            Vector3[] newVerts = new Vector3[length];
            Vector2[] newUVs = new Vector2[length];
            Vector3[] newNormals = new Vector3[length];
            int[] triangles = new int[meshStruct1.faceData.Length];
            int i = 0;
            
            foreach (Vector3 v in meshStruct1.faceData)
            {
                int index = (int) v.x - 1;
                if(meshStruct1.vertices.Length > index)
                    newVerts[i] = meshStruct1.vertices[index];
                if (v.y >= 1)
                    newUVs[i] = meshStruct1.uv[(int)v.y - 1];

                if (v.z >= 1 && meshStruct1.normals.Length > 0)
                    newNormals[i] = meshStruct1.normals[(int)v.z - 1];
                triangles[i] = i;
                i++;
            }
            //заполнение объекта Unity Mesh полученными значениями
            Mesh mesh = new Mesh();

            mesh.vertices = newVerts;
            mesh.uv = newUVs;
            mesh.normals = newNormals;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();  
            meshes.Add(mesh);
            meshNum++;
        }

        return meshes;
    }

    //подсчёт числа вершин, нормалей, uv-координат и треугольников в модели
    private static List<meshStruct> createMeshStruct(string filename)
    {
        List<meshStruct> meshList = new List<meshStruct>();
        List<int> triangles = new List<int>();
        triangles.Add(0);
        List<int> vertices = new List<int>();
        vertices.Add(0);
        List<int> vt = new List<int>();
        vt.Add(0);
        List<int> vn = new List<int>();
        vn.Add(0);
        List<int> face = new List<int>();
        face.Add(0);

        int currentVertexIndex = 0;
        int currentNormalIndex = 0;
        int currentUVIndex = 0;
        int currentTriangleIndex = 0;
        int currentFaceIndex = 0;
        StreamReader stream = File.OpenText(filename);
        string entireText = stream.ReadToEnd();
        stream.Close();
        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();
            char[] splitIdentifier = { ' ' };
            string[] brokenString;
            while (currentText != null)
            {
                if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                    && !currentText.StartsWith("vn "))
                {
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
                else
                {
                    currentText = currentText.Trim();             
                    brokenString = currentText.Split(splitIdentifier, 50);      
                    switch (brokenString[0])
                    {
                        case "v":
                            if (vertices[currentVertexIndex] >= MaxVertices)
                            {
                                currentVertexIndex++; 
                                vertices.Add(0);
                            }
                            vertices[currentVertexIndex]++;
                            break;
                        case "vt":
                            if (vt[currentUVIndex] >= MaxVertices)
                            {
                                currentUVIndex++;
                                vt.Add(0);
                            }
                            vt[currentUVIndex]++;
                            break;
                        case "vn":
                            if (vn[currentNormalIndex] >= MaxVertices)
                            {
                                currentNormalIndex++;
                                vn.Add(0);
                            }
                            vn[currentNormalIndex]++;
                            break;
                        case "f":
                            if (triangles[currentTriangleIndex] + 3 > MaxVertices * 2)
                            {
                                currentTriangleIndex++;
                                triangles.Add(0);
                            }
                            if (face[currentFaceIndex] + brokenString.Length >= MaxVertices)
                            {
                                currentFaceIndex++;
                                face.Add(0);
                            }
                            face[currentFaceIndex] = face[currentFaceIndex] + brokenString.Length - 1;
                            triangles[currentTriangleIndex] = triangles[currentTriangleIndex] + 3 * (brokenString.Length - 2); 
                            break;
                    }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
            }
        }
        for (int i = 0; i < vertices.Count; i++)
        {
            meshStruct mesh = new meshStruct();
            mesh.fileName = filename;
            mesh.triangles = new int[triangles[i]];
            mesh.vertices = new Vector3[vertices[i]];
            if(vt.Count <= i)
                mesh.uv = new Vector2[0];
            else
                mesh.uv = new Vector2[vt[i]];
            if(vn.Count <= i)
                mesh.normals = new Vector3[0];
            else
                mesh.normals = new Vector3[vn[i]];
            mesh.faceData = new Vector3[face[i]];
            meshList.Add(mesh);
        }
        
        return meshList;
    }

    //заполнение полученных на первом шаге массивов
    private static void populateMeshStruct(ref List<meshStruct> meshes)
    {
        StreamReader stream = File.OpenText(meshes[0].fileName);
        string entireText = stream.ReadToEnd();
        stream.Close();
        int currentVertexIndex = 0;
        int currentNormalIndex = 0;
        int currentUVIndex = 0;
        int currentTriangleIndex = 0;
        int currentFaceIndex = 0;
        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();

            char[] splitIdentifier = { ' ' };
            char[] splitIdentifier2 = { '/' };
            string[] brokenString;
            string[] brokenBrokenString;
            int f = 0;
            int f2 = 0;
            int v = 0;
            int vn = 0;
            int vt = 0;
            int vt1 = 0;
            int vt2 = 0;
            while (currentText != null)
            {
                if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                    !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                    !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                    !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                {
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");
                    }
                }
                else
                {
                    currentText = currentText.Trim();
                    brokenString = currentText.Split(splitIdentifier, 50);
                    switch (brokenString[0])
                    {
                        case "g":
                            break;
                        case "usemtl":
                            break;
                        case "usemap":
                            break;
                        case "mtllib":
                            break;
                        case "v":
                            if (v >= meshes[currentVertexIndex].vertices.Length)
                            {
                                currentVertexIndex++;
                                v = 0;
                            }
                            meshes[currentVertexIndex].vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                     System.Convert.ToSingle(brokenString[3]));
                            v++;
                            break;
                        case "vt":
                            if (vt >= meshes[currentUVIndex].uv.Length)
                            {
                                currentUVIndex++;
                                vt = 0;
                                vt1 = 0;
                                vt2 = 0;
                            }
                            meshes[currentUVIndex].uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt++;
                            break;
                        case "vt1":
                            meshes[currentUVIndex].uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt1++;
                            break;
                        case "vt2":
                            meshes[currentUVIndex].uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                            vt2++;
                            break;
                        case "vn":
                            if (vn >= meshes[currentNormalIndex].normals.Length)
                            {
                                currentNormalIndex++;
                                vn = 0;
                            }
                            meshes[currentNormalIndex].normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                    System.Convert.ToSingle(brokenString[3]));
                            vn++;
                            break;
                        case "vc":
                            break;
                        case "f":

                            int j = 1;
                            List<int> intArray = new List<int>();
                            while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                            {
                                Vector3 temp = new Vector3();
                                brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    
                                temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
                                temp.x -= meshes[currentFaceIndex].faceData.Length*currentFaceIndex;
                                if (brokenBrokenString.Length > 1)                                  
                                {
                                    if (brokenBrokenString[1] != "")                                
                                    {
                                        temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
                                    }
                                    temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
                                }
                                j++;
                                if (f2 >= meshes[currentFaceIndex].faceData.Length)
                                {
                                    currentFaceIndex++;
                                    f2 = 0;
                                }
                                meshes[currentFaceIndex].faceData[f2] = temp;
                                intArray.Add(f2);
                                f2++;
                            }
                            j = 1;
                            if (f >= meshes[currentTriangleIndex].triangles.Length)
                            {
                                currentTriangleIndex++;
                                f = 0;
                            }
                            while (j + 2 < brokenString.Length)     
                            {
                                meshes[currentTriangleIndex].triangles[f] = intArray[0];
                                f++;
                                meshes[currentTriangleIndex].triangles[f] = intArray[j];
                                f++;
                                meshes[currentTriangleIndex].triangles[f] = intArray[j + 1];
                                f++;   
                                j++;
                            }
                            break;
                    }
                    currentText = reader.ReadLine();
                    if (currentText != null)
                    {
                        currentText = currentText.Replace("  ", " ");       
                    }
                }
            }
        }
    }
}