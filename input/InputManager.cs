using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace input
{
    public class InputManager : Singleton<InputManager>
    {
        protected InputManager() { }

        // Perhaps make it so you can bind only to a specific ActionState, so you don't check for it in every listener
        private Dictionary<Action<ActionInfo>, string> m_ActionEvents = new Dictionary<Action<ActionInfo>, string>();

        // Stores key configuration
        private Dictionary<ActionBinding, string> m_ActionBindings = new Dictionary<ActionBinding, string>();

        // Stores mouse positions at beginning of key press
        private Dictionary<ActionBinding, Vector3> m_ActionInitialPositions = new Dictionary<ActionBinding, Vector3>();

        private Dictionary<Action<ActionInfo>, Func<ActionInfo, bool>> m_MousePositionEvents = new Dictionary<Action<ActionInfo>, Func<ActionInfo, bool>>();

        private void Update()
        {
            foreach(var actionBinding in m_ActionBindings)
            {
                CheckActionInitiated(actionBinding);
                CheckActionExecuting(actionBinding);
                CheckActionFinished(actionBinding);
            }
            foreach(var mousePositionEvent in m_MousePositionEvents)
            {
                if(mousePositionEvent.Value.Invoke(new ActionInfo { InitialMousePosition = Input.mousePosition}))
                {
                    mousePositionEvent.Key.Invoke(new ActionInfo { InitialMousePosition = Input.mousePosition });
                }
            }
        }

        private void CheckActionFinished(KeyValuePair<ActionBinding, string> actionBinding)
        {
            if (Input.GetKeyUp(actionBinding.Key.Key) &&
                    (!actionBinding.Key.Modifier.HasValue || Input.GetKey(actionBinding.Key.Modifier.Value)))
            {
                foreach (var listener in m_ActionEvents.Where(t => t.Value == actionBinding.Value))
                {
                    listener.Key.Invoke(new ActionInfo
                    {
                        InitialMousePosition = m_ActionInitialPositions[actionBinding.Key],
                        LastMousePosition = Input.mousePosition,
                        State = ActionState.Finished
                    });
                }
                m_ActionInitialPositions.Remove(actionBinding.Key);
            }
        }

        private void CheckActionExecuting(KeyValuePair<ActionBinding, string> actionBinding)
        {
            if (actionBinding.Key.ShouldTick && Input.GetKey(actionBinding.Key.Key) &&
                    (!actionBinding.Key.Modifier.HasValue || Input.GetKey(actionBinding.Key.Modifier.Value)))
            {
                foreach (var listener in m_ActionEvents.Where(t => t.Value == actionBinding.Value))
                {
                    listener.Key.Invoke(new ActionInfo
                    {
                        InitialMousePosition = m_ActionInitialPositions[actionBinding.Key],
                        LastMousePosition = Input.mousePosition,
                        State = ActionState.Executing
                    });
                }
            }
        }

        private void CheckActionInitiated(KeyValuePair<ActionBinding, string> actionBinding)
        {
            if (Input.GetKeyDown(actionBinding.Key.Key) &&
                    (!actionBinding.Key.Modifier.HasValue || Input.GetKey(actionBinding.Key.Modifier.Value)))
            {
                m_ActionInitialPositions.Add(actionBinding.Key, Input.mousePosition);
                foreach (var listener in m_ActionEvents.Where(t => t.Value == actionBinding.Value))
                {
                    listener.Key.Invoke(new ActionInfo
                    {
                        InitialMousePosition = Input.mousePosition,
                        LastMousePosition = Input.mousePosition,
                        State = ActionState.Initiated
                    });
                }
            }
        }

        public void AddAction(string _actionName, ActionBinding _binding)
        {
            if (!m_ActionBindings.ContainsKey(_binding))
                m_ActionBindings.Add(_binding, _actionName);
        }

        public void RemoveAction(ActionBinding _binding)
        {
            if(m_ActionBindings.ContainsKey(_binding))
            {
                m_ActionBindings.Remove(_binding);
            }
        }

        public void AddActionListener(string _actionName, Action<ActionInfo> _listener)
        {
            if(!m_ActionEvents.ContainsKey(_listener))
            {
                m_ActionEvents.Add(_listener, _actionName);
            }
        }

        public void RemoveActionListener(Action<ActionInfo> _listener)
        {
            if(m_ActionEvents.ContainsKey(_listener))
            {
                m_ActionEvents.Remove(_listener);
            }
        }

        public void AddMousePositionListener(Func<ActionInfo, bool> _condition, Action<ActionInfo> _listener)
        {
            if(!m_MousePositionEvents.ContainsKey(_listener))
            {
                m_MousePositionEvents.Add(_listener, _condition);
            }
        }

        public void RemoveMousePositionListener(Action<ActionInfo> _listener)
        {
            if (m_MousePositionEvents.ContainsKey(_listener))
            {
                m_MousePositionEvents.Remove(_listener);
            }
        }

        public void Start()
        {
            AddAction("SimpleSelect", new ActionBinding { Key = KeyCode.Mouse0, ShouldTick = false });
            AddAction("DragSelect", new ActionBinding { Key = KeyCode.Mouse0, ShouldTick = true });
            AddAction("RightClick", new ActionBinding { Key = KeyCode.Mouse1, ShouldTick = false });
            AddAction("CameraForward", new ActionBinding { Key = KeyCode.UpArrow, ShouldTick = true });
            AddAction("CameraBackward", new ActionBinding { Key = KeyCode.DownArrow, ShouldTick = true });
            AddAction("CameraLeft", new ActionBinding { Key = KeyCode.LeftArrow, ShouldTick = true });
            AddAction("CameraRight", new ActionBinding { Key = KeyCode.RightArrow, ShouldTick = true });
            AddAction("Return", new ActionBinding { Key = KeyCode.Return, ShouldTick = false });
            AddAction("AddModifier", new ActionBinding { Key = KeyCode.LeftShift, ShouldTick = false });
            AddAction("CompoundModifier", new ActionBinding { Key = KeyCode.LeftControl, ShouldTick = false });
        }
    }
}
