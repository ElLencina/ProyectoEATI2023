using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VRStep
{

	public delegate void StepHandler();

	public class StepDetector : MonoBehaviour
	{

		[Tooltip("Recommended to leave at default. This is a smooth factor to remove noise, higher values incease latency but lower noise, lower values decrease latency but increase noise.")]
		public int filterNumSamplesToAverage = 4;

		private int filterNumSamples = 0; //number of samples that have been summed so far
		private float filterTotalX = 0;

		private int dynamicThresholdNumSamplesToUpdate = 50; //size of sample window to update the dynamic threshold.
		private int dynamicThresholdNumSamples = 0;
		private float curMin = Single.PositiveInfinity; //minimum value of the last dynamicThresholdNumSamples samples
		private float curMax = Single.NegativeInfinity; //maximum value of the last dynamicThresholdNumSamples samples
		private float dynamicThreshold = 0; //the changing threshold to detect a step

		[Tooltip("This is essentially a sensitivity setting.  Lower values is MORE sensitive, higher values is LESS sensitive.  .35 is the recommended setting")]
		public float stepMinThreshold = .35f; //minimum threshold for a sample to be passed into the step detection stuff
		private float stepSampleOld = 0; //the last valid sample
		private float stepSampleNew = 0; //the potentially new valid sample, might = old sample if the input sample is below the minimum threshold

		private float timeSinceLastDetectedStep = 10;
		//private float stepIntervalMax = 1.5f;
		private const float stepIntervalMin = .2f;

		public StepHandler OnStepDetected;

		public static bool isOculusGo = false;

		[Tooltip("In the editor, hitting this key on your keyboard will simulate a step and fire the OnStepDetected registered functions.  Use this to test your game in the editor with the WIP controllers")]
		public KeyCode SimulateStepsWithKey = KeyCode.UpArrow;

		public static StepDetector instance;

		void Awake()
		{
			instance = this;

		}

		void Start()
		{
			//enable the gyroscope, should be enabled anyways, but not guaranteed
			bool gyroBool = SystemInfo.supportsGyroscope;
			if (gyroBool)
			{
				Input.gyro.enabled = true;
				Input.compass.enabled = true;
			}

			//if (UnityEngine.VR.VRDevice.model == "Oculus Go") {
			//	isOculusGo = true;
			//}

		}

		void Update()
		{
			//update step counter time
			timeSinceLastDetectedStep += Time.deltaTime;

			//detect upwards velocity
			float upVector = GetUpwardsAcceleration();

			//update the dynamic threshold every dynamicThresholdNumSamplesToUpdate steps
			if (dynamicThresholdNumSamples < dynamicThresholdNumSamplesToUpdate)
			{

				if (upVector > curMax) curMax = upVector;
				if (upVector < curMin) curMin = upVector;

				dynamicThresholdNumSamples++;
			}
			else
			{
				dynamicThreshold = (curMax + curMin) / 2;
				curMin = Single.PositiveInfinity;
				curMax = Single.NegativeInfinity;
				dynamicThresholdNumSamples = 0;
			}

			
			//sample once per update, sum filterNumSamplesToAverage samples together and average them to smooth out noise, then pass to step detection main part
			filterTotalX += upVector;
			filterNumSamples++;


			if(filterNumSamples >= filterNumSamplesToAverage)
			{
				float totalX = filterTotalX / filterNumSamplesToAverage;

				StepDetection(totalX);

				//reset filter counter and values.
				filterNumSamples = 0;
				filterTotalX = 0;
			}



			if (Input.GetKeyDown(SimulateStepsWithKey))
			{
				if(OnStepDetected != null)
					OnStepDetected();
			}
		}

		private void StepDetection(float stepSampleResult)
		{
			stepSampleOld = stepSampleNew; //the old threshold value is always updated with whatever was in "new" before

			//Check to see if the difference in acceleration is at least some threshold, if not, stepSampleNew remains unchanged.
			if (Math.Abs(stepSampleResult - stepSampleNew) > stepMinThreshold)
			{
				stepSampleNew = stepSampleResult;

				//we detect a step if we have a negative slope when acceleration crosses below the dynamic threshold
				if (stepSampleNew < dynamicThreshold && stepSampleNew < stepSampleOld)
				{
					//check to see how long ago our last detected step was so that we avoid unrealistic detections
					if (timeSinceLastDetectedStep > stepIntervalMin)
					{
						//Step detected!
						timeSinceLastDetectedStep = 0;
						if(OnStepDetected != null)
							OnStepDetected();
					}
				}
			}
		}

		private float GetUpwardsAcceleration(Vector3 rawAccel)
		{
			Vector3 up = -Input.gyro.gravity;
			return Vector3.Dot(rawAccel, up);
		}

		public static float GetUpwardsAcceleration() {
#if OCULUS_VR
			if (isOculusGo) {
				return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.Head, OVRPlugin.Step.Render).FromFlippedZVector3f().y / 16f;
			}
#endif

			return Vector3.Dot(Input.gyro.userAcceleration, -Input.gyro.gravity);
			
		}

		public static void AddStepAction(StepHandler h)
		{
			if(instance != null)
				instance.OnStepDetected += h;
		}


		public static void RemoveStepAction(StepHandler h)
		{
			if(instance != null)
				instance.OnStepDetected -= h;
		}
	}
}
