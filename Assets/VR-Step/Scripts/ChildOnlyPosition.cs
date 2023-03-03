using UnityEngine;
using System.Collections;

namespace VRStep
{

	public class ChildOnlyPosition : MonoBehaviour
	{

		public Transform locParent;

		private Vector3 offset;

		// Use this for initialization
		void Start()
		{
			offset = transform.position - locParent.position;
		}

		// Update is called once per frame
		void Update()
		{
			if (locParent != null)
			{
				transform.position = locParent.position + offset;
			}
		}
	}
}