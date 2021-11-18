using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI
{
    private static readonly Font _font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

    private readonly MVRScript _plugin;

    public UI(MVRScript plugin)
    {
        _plugin = plugin;
    }

    public void AddHeader(string title, int level, bool rightSide = false)
    {
        var headerUI = _plugin.CreateSpacer(rightSide);
        headerUI.height = 40f;

        var text = headerUI.gameObject.AddComponent<Text>();
        text.text = title;
        text.font = _font;
        switch (level)
        {
            case 1:
                text.fontSize = 30;
                text.fontStyle = FontStyle.Bold;
                text.color = new Color(0.95f, 0.9f, 0.92f);
                break;
            case 2:
                text.fontSize = 28;
                text.fontStyle = FontStyle.Bold;
                text.color = new Color(0.85f, 0.8f, 0.82f);
                break;
        }
    }

    public void AddFloat(JSONStorableFloat jsf, bool rightSide = false)
    {
        _plugin.CreateSlider(jsf, rightSide);
    }

    public void AddBool(JSONStorableBool jsf, bool rightSide = false)
    {
        _plugin.CreateToggle(jsf, rightSide);
    }

    public void AddStringChooser(JSONStorableStringChooser jssc, bool rightSide = false)
    {
        _plugin.CreatePopup(jssc, rightSide);
    }

    public void AddAction(string label, bool rightSide, UnityAction action)
    {
        _plugin.CreateButton(label, rightSide).button.onClick.AddListener(action);
    }
}
