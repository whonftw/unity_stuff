using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using input;
using UnityEngine;

public class SelectionManager : Singleton<SelectionManager>
{
    private List<GameObject> m_SelectedObjects = new List<GameObject>();

    // Shift Modifier
    private bool m_IsAdditionModifier = false;

    // Ctrl Modifier
    private bool m_IsCompoundModifier = false;

    // Selection not allowed, n/a
    private bool m_IsBlocked = false;
	
    private EventManager m_EventManager;
    private InputManager m_InputManager;

    private void Awake()
    {
        m_EventManager = EventManager.Instance;
        m_InputManager = InputManager.Instance;
    }

    private void Start()
    {
        m_EventManager.AddListener<BoxSelectionEvent>(OnBoxSelection);
        m_EventManager.AddListener<DeathEvent>(OnUnitKilled);
        // Shift state
        m_InputManager.AddActionListener("AddModifier", t => m_IsAdditionModifier = t.State == input.ActionState.Finished ? false : true);
        // CTRL state
        m_InputManager.AddActionListener("CompoundModifier", t => m_IsCompoundModifier = t.State == input.ActionState.Finished ? false : true);

        m_InputManager.AddActionListener("SimpleSelect", OnSimpleSelection);
    }

    private void OnSimpleSelection(ActionInfo _info)
    {
        if (m_IsBlocked || _info.State == ActionState.Finished)
            return;
        if (!m_IsAdditionModifier)
            ClearSelection();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(_info.LastMousePosition), out RaycastHit rayHit, Mathf.Infinity, GameSettings.Instance.m_UnitsLayer))
        {
            if (!m_IsCompoundModifier)
            {
                if (!m_SelectedObjects.Contains(rayHit.transform.gameObject))
                {
                    Select(rayHit.transform.gameObject);
                }
                else
                {
                    Deselect(rayHit.transform.gameObject);
                }
            }
            else
            {
                foreach (var unit in UnitsManager.Instance.Where(t => t.tag == rayHit.transform.tag))
                {
                    if (!m_SelectedObjects.Contains(unit))
                    {
                        Select(unit);
                    }
                }
            }
        }
    }

    private void OnUnitKilled(DeathEvent _event)
    {
        Deselect(_event.GameObject);
    }

    public void OnBoxSelection(BoxSelectionEvent _event)
    {
        if (m_IsBlocked)
            return;
        if (!m_IsAdditionModifier)
            ClearSelection();
        var rect = gui.Utils.GetScreenRect(_event.StartPosition, _event.EndPosition);
        if(!m_IsCompoundModifier)
        {
            foreach(var unit in UnitsManager.Instance.Where(t => IsWithinSelectionBounds(t, _event.StartPosition, _event.EndPosition)))
            {
                Select(unit);
            }
        }
        else
        {
            foreach (var tag in UnitsManager.Instance.Where(t => IsWithinSelectionBounds(t, _event.StartPosition, _event.EndPosition)).Select(t => t.tag).Distinct())
            {
                foreach (var unit in UnitsManager.Instance.Where(t => t.tag == tag))
                {
                    Select(unit);
                }
            }
        }
    }

    public void ClearSelection()
    {
        while (m_SelectedObjects.Count > 0)
            Deselect(m_SelectedObjects.Last());
        m_SelectedObjects.Clear();
    }

    private void Select(GameObject _gameObject)
    {
        if (!m_SelectedObjects.Contains(_gameObject))
        {
            m_SelectedObjects.Add(_gameObject);
            var selectableObject = _gameObject.GetComponent<SelectableUnitComponent>();
            if (selectableObject.m_SelectionCircle == null)
            {
                selectableObject.m_SelectionCircle = Instantiate(GameSettings.Instance.m_UnitSelectionPrefab);
                selectableObject.m_SelectionCircle.transform.SetParent(selectableObject.transform, false);
                selectableObject.m_SelectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
            }
        }
    }


    private void Deselect(GameObject _gameObject)
    {
        if (m_SelectedObjects.Contains(_gameObject))
        {
            var selectableObject = _gameObject.GetComponent<SelectableUnitComponent>();
            if (selectableObject.m_SelectionCircle != null)
            {
                Destroy(selectableObject.m_SelectionCircle.gameObject);
                selectableObject.m_SelectionCircle = null;
            }
            m_SelectedObjects.Remove(_gameObject);
        }
    }

    public static bool IsWithinSelectionBounds(GameObject _gameObject, Vector3 _start, Vector3 _end)
    {
        var viewportBounds =
            gui.Utils.GetViewportBounds(Camera.main, _start, _end);

        return viewportBounds.Contains(
            Camera.main.WorldToViewportPoint(_gameObject.transform.position));
    }
}