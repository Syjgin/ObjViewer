using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//обрабатывает события графического интерфейса
public class GuiManager : MonoBehaviour
{
    [SerializeField] private Toggle _firstPoint;
    [SerializeField] private Toggle _secondPoint;
    [SerializeField] private Text _lengthValue;
    [SerializeField] private ModelLoader _loader;
    [SerializeField] private Text _loadingText;

    public bool IsFirstPointSelected()
    {
        return _firstPoint.isOn;
    }

	void Awake ()
	{
        //если нажата галочка первой вершины, снимаем отметку со второй, и наоборот
	    _firstPoint.onValueChanged.AddListener(firstPointValueChanged);
        _secondPoint.onValueChanged.AddListener(secondPointValueChanged);

        //обработка событий загрузчика модели
        _loader.PointsSwitched += onPointsSwitched;
        _loader.LengthCalculated += onLengthCalculated;
        _loader.LoadingFinished += onLoadingFinished;
	}

    private void onLoadingFinished()
    {
        //когда модель загрузилась, убираем сообщение о загрузке
        _loadingText.gameObject.SetActive(false);
    }

    private void onLengthCalculated(float length)
    {
        //при окончании рассчёта расстояния выводим его значение
        _lengthValue.text = String.Format("{0:0.00}", length) +"m";
        _secondPoint.isOn = false;
    }

    private void onPointsSwitched()
    {
        _lengthValue.text = "";
        _secondPoint.isOn = true;
    }

    void OnDestroy()
    {
        //при выходе из программы отписываемся от событий
        _firstPoint.onValueChanged.RemoveListener(firstPointValueChanged);
        _secondPoint.onValueChanged.RemoveListener(secondPointValueChanged);

        _loader.PointsSwitched -= onPointsSwitched;
        _loader.LengthCalculated -= onLengthCalculated;
        _loader.LoadingFinished -= onLoadingFinished;
    }

    private void firstPointValueChanged(bool val)
    {
        _secondPoint.isOn = !val;
    }

    private void secondPointValueChanged(bool val)
    {
        _firstPoint.isOn = !val;
    }
}
