using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuiManager : MonoBehaviour
{
    [SerializeField] private Toggle _firstPoint;
    [SerializeField] private Toggle _secondPoint;
    [SerializeField] private Text _lengthValue;
    [SerializeField] private ModelLoader _loader;

    public bool IsFirstPointSelected()
    {
        return _firstPoint.isOn;
    }

	void Start ()
	{
	    _firstPoint.onValueChanged.AddListener(firstPointValueChanged);
        _secondPoint.onValueChanged.AddListener(secondPointValueChanged);

        _loader.PointsSwitched += onPointsSwitched;
        _loader.LengthCalculated += LengthCalculated;
	}

    private void LengthCalculated(float length)
    {
        _lengthValue.text = String.Format("{0:0.00}", length) +"m";
        _secondPoint.isOn = false;
    }

    private void onPointsSwitched()
    {
        _secondPoint.isOn = true;
    }

    void OnDestroy()
    {
        _firstPoint.onValueChanged.RemoveListener(firstPointValueChanged);
        _secondPoint.onValueChanged.RemoveListener(secondPointValueChanged);

        _loader.PointsSwitched -= onPointsSwitched;
        _loader.LengthCalculated -= LengthCalculated;
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
