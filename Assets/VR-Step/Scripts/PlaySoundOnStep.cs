using UnityEngine;
using System.Collections;

namespace VRStep
{

	public class PlaySoundOnStep : MonoBehaviour
	{

		public AudioSource sound;

		// Use this for initialization
		void Start()
		{
			StepDetector.AddStepAction(OnStep);
		}

		void OnStep()
		{
			sound.Play();
		}
	}
}
