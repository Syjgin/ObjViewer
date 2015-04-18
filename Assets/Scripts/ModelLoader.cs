using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//класс для загрузки модели в сцену и её отображения
public class ModelLoader : MonoBehaviour {

    [SerializeField] private Material _diffuse;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _sphere1;
    [SerializeField] private GameObject _sphere2;
    [SerializeField] private GuiManager _gui;

    //чувствительность мыши
    private const float MouseThreshold = 0.2f;
    private const float RotateCoef = 5;

    //загруженные из файла части модели
    private List<GameObject> _importedObjects;
    private List<MeshCollider> _meshColliders;

    private bool _isRightMouseButtonPressed;

    //события, на которые реагирует GUI: смена вершин, завершение вычисления расстояния, завершение загрузки модели из файла
    public delegate void SwitchPointsAction();
    public SwitchPointsAction PointsSwitched;

    public delegate void LengthCalculatedAction(float length);
    public LengthCalculatedAction LengthCalculated;

    public delegate void LoadingPhaseAction(string message);
    public LoadingPhaseAction SetStatus;

    private Vector3 _firstPointValue = default (Vector3);

    private bool _isModelInitialized;

	void Start ()
	{
	    _isModelInitialized = false;
	    StartCoroutine(LoadModelCoroutine());
	}

    IEnumerator LoadModelCoroutine()
    {
        yield return new WaitForSeconds(2);
        if (System.Environment.GetCommandLineArgs().Length > 1)
        {
            //string modelPath = "C:\\Explorer\\teapot.obj";
            //имя модели берётся из командной строки
            string modelPath = System.Environment.GetCommandLineArgs()[1];
            //загружаем модель из файла
            ObjImporter importer = new ObjImporter();
            List<Mesh> importedMeshes = importer.ImportFile(modelPath);
            //если модель не проходит ограничение в 65000 вершин на меш, разбиваем на несколько мешей
            _importedObjects = new List<GameObject>();
            _meshColliders = new List<MeshCollider>();
            foreach (var importedMesh in importedMeshes)
            {
                GameObject importedObj = new GameObject("mesh", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
                importedObj.GetComponent<MeshFilter>().mesh = importedMesh;
                importedObj.GetComponent<MeshRenderer>().materials[0] = _diffuse;

                importedObj.GetComponent<MeshCollider>().sharedMesh = importedMesh;

                importedObj.transform.parent = this.transform;
                importedObj.transform.localPosition = new Vector3(0, 0, -1);
                importedObj.transform.Rotate(Vector3.up, 180);
                MeshCollider col = importedObj.GetComponent<MeshCollider>();
                _importedObjects.Add(importedObj);
                //сохраняем коллайдер каждого меша для обработки клика мышью
                _meshColliders.Add(col);
            }
            //по окончанию загрузки убираем статусное сообщение
            if (SetStatus != null)
                SetStatus("");
            _isModelInitialized = true;
        }
        else
        {
            if (SetStatus != null)
                SetStatus("не задан входной файл");
        }
    }

     void Update()
     {
         if (_isModelInitialized)
         {
             if (Input.GetMouseButton(0))
             {
                 //при зажатой левой клавише мыши вращаем модель
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
             if (Input.GetMouseButton(1) && !_isRightMouseButtonPressed)
             {
                 //при нажатии правой кнопки мыши пытаемся найти точку пересечения курсора с моделью
                 _isRightMouseButtonPressed = true;
                 Ray ray = _camera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                 Vector3 closestVertex = default(Vector3);
                 foreach (var col in _meshColliders)
                 {
                     RaycastHit hit;
                     if (col.Raycast(ray, out hit, 1000))
                     {
                         //точка пересечения найдена
                         closestVertex = hit.point;
                         break;
                     }
                 }
                 if (closestVertex != default(Vector3))
                 {
                     //если до этого была выбрана первая точка, переходим в режим выбора второй
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
                         //если была выбрана вторая точка, считаем расстояние и отправляем событие в GUI
                         _sphere2.transform.position = closestVertex;
                         _sphere2.gameObject.SetActive(true);
                         float length = (_firstPointValue - closestVertex).magnitude;
                         if (LengthCalculated != null)
                             LengthCalculated(length);
                     }
                 }
             }
             //предотвращаем многократную обработку нажатия
             if (!Input.GetMouseButton(1))
                 _isRightMouseButtonPressed = false;
         }
     }
}
