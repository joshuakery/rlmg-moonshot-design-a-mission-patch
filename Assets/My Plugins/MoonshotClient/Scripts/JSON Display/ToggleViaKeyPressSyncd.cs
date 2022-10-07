using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleViaKeyPressSyncd : MonoBehaviour
{
	public KeyCode key;

	public GameObject targetObj;
	public MonoBehaviour targetComponent;

	void Update()
	{
		if (Input.GetKeyDown(key))
		{
			if (targetObj != null)
				targetObj.SetActive(!targetObj.activeSelf);

			if (targetComponent != null)
				targetComponent.enabled = targetObj.activeSelf; //sync'd so that component is enabled when obj is active
		}
	}
}
