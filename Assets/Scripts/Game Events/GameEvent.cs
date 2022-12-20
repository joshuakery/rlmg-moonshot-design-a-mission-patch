using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;


[CreateAssetMenu(menuName = "Event/GameEvent"), System.Serializable]
public class GameEvent : ScriptableObject
{
	private List<GameEventListener> listeners = 
		new List<GameEventListener>();

    public void Raise()
    {
        //RLMGLogger.Instance.Log("GameEvent raised: " + this.name, MESSAGETYPE.INFO);
        for (int i = listeners.Count -1; i >= 0; i--) {
            listeners[i].OnEventRaised();
        }

    }

    public void RegisterListener(GameEventListener listener)
    { listeners.Add(listener); }

    public void UnregisterListener(GameEventListener listener)
    { listeners.Remove(listener); }
}
