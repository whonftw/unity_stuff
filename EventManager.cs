using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherit from this to make your custom event arguments
/// </summary>
public interface IGameEvent
{

}


public sealed class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate<T>(T t) where T : IGameEvent;
    private delegate void EventDelegate(IGameEvent ev);

    private Dictionary<System.Type, EventDelegate> m_InternalDelegates = new Dictionary<System.Type, EventDelegate>();
    private Dictionary<System.Delegate, EventDelegate> m_DelegateLookups = new Dictionary<System.Delegate, EventDelegate>();

    public void AddListener<T>(EventDelegate<T> _del) where T: IGameEvent
    {
        m_DelegateLookups.Add(_del, AddDelegate(_del));
    }

    private EventDelegate AddDelegate<T>(EventDelegate<T> _del) where T : IGameEvent
    {
        var type = typeof(T);
        if (m_DelegateLookups.ContainsKey(_del))
            return null;
        EventDelegate genericDelegate = arg => _del((T)arg);
        if(m_InternalDelegates.TryGetValue(type, out EventDelegate eventDelegate))
        {
            m_InternalDelegates[type] = eventDelegate + genericDelegate;
        }
        else
        {
            m_InternalDelegates[type] = genericDelegate;
        }
        return genericDelegate;
    }

    public void RemoveListener<T>(EventDelegate<T> _del) where T : IGameEvent
    {
        var type = typeof(T);
        if(m_DelegateLookups.TryGetValue(_del, out EventDelegate internalDelegate))
        {
            if(m_InternalDelegates.TryGetValue(type, out EventDelegate tempDelegate))
            {
                tempDelegate -= internalDelegate;
                if (tempDelegate != null)
                    m_InternalDelegates[type] = tempDelegate;
                else
                    m_InternalDelegates.Remove(type);
            }
            m_DelegateLookups.Remove(_del);
        }
    }

    public bool HasListener<T>(EventDelegate<T> _del) where T: IGameEvent
    {
        return m_DelegateLookups.ContainsKey(_del);
    }

    public void RemoveAll()
    {
        m_DelegateLookups.Clear();
        m_InternalDelegates.Clear();
    }

    public void TriggerEvent(IGameEvent _gameEvent)
    {
        if(m_InternalDelegates.TryGetValue(_gameEvent.GetType(), out EventDelegate eventDelegate))
        {
            eventDelegate.Invoke(_gameEvent);
        }
        else
        {
            Debug.Log($"Event of type {_gameEvent.GetType()} fired, but no listeners were found !");
        }
    }
}
