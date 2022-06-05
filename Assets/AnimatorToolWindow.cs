using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;


public class AnimatorToolWindow : EditorWindow
{
	[MenuItem("AnimatorTool/Animator Tool Window")]

	static void Init()
	{
		AnimatorToolWindow window = GetWindow<AnimatorToolWindow>();
		window.titleContent = new GUIContent("Animator Tool Window");
		window.Show();

	}

	private Editor previewEditor;



	public Animator currentAnim;
	public GameObject currentAnimModel;
	public AnimationClip currentAnimClip;
	private float currentAnimSpd = 1;

	private bool currentAnimLoop;
	private float currentAnimCurrentLength;
	private float currentAnimMaxLength;

	public List<Animator> animators = new List<Animator>();
	public List<AnimationClip> currentAnimatorAnimations = new List<AnimationClip>();
	Scene currentScene;

	private void OnGUI()
	{
		DisplayAnimatorsOfList(animators);
	}
	private void DisplayAnimatorsOfList(List<Animator> list)
	{
		currentScene = SceneManager.GetActiveScene();
		if (!currentScene.IsValid()) return;

		GameObject[] rootObjects = currentScene.GetRootGameObjects();

		for (int i = 0; i < rootObjects.Length; i++)
		{
			if (rootObjects[i].GetComponent<Animator>())
				animators.Add(rootObjects[i].GetComponent<Animator>());
		}


		GUILayout.Label(name);
		GUILayout.Space(3);
		foreach (Animator animations in list)
		{
			// Define button name
			string buttonName = animations.name;
			bool isSelectedItem = animations.Equals(currentAnim);
			if (isSelectedItem)
			{
				buttonName = buttonName.Insert(0, "-- ");
				buttonName += " --";
				GUI.backgroundColor = Color.black;
				GUI.contentColor = Color.white;
				Selection.activeGameObject = animations.gameObject;
				currentAnimModel = animations.gameObject;
			}
			else
			{
				GUI.backgroundColor = Color.grey;
				GUI.contentColor = Color.white;
			}

			// Draw button
			if (GUILayout.Button(buttonName))
			{
				if (animations == currentAnim)
				{
					SceneView.lastActiveSceneView.FrameSelected();
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = currentAnim;
				}
				currentAnim = animations;
			}

		}


		if (currentAnim != null && currentAnim.runtimeAnimatorController != null)
		{

			foreach (AnimationClip animClip in currentAnim.runtimeAnimatorController.animationClips)
			{
				currentAnimatorAnimations.Add(animClip);
			}

			//Draw extra buttons for animationsClips
			foreach (AnimationClip animClip in currentAnimatorAnimations)
			{
				bool AnimClipIsSelected = animClip.Equals(currentAnimClip);


				using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(256), GUILayout.ExpandWidth(true)))
				{
					if (AnimClipIsSelected)
					{
						GUI.backgroundColor = Color.red;
						GUI.contentColor = Color.white;

					}
					else
					{
						GUI.backgroundColor = Color.blue;
					}


					if (GUILayout.Button(animClip.name))
					{
						//currentAnim.Play(currentAnimClip.name, 0);
						currentAnimClip = animClip;
					}
				}

			}


			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("+"))
				{
					currentAnimSpd++;
				}
				if (GUILayout.Button("-"))
				{
					currentAnimSpd--;
				}


				EditorGUILayout.LabelField("Animation Speed :");
				EditorGUILayout.FloatField(currentAnimSpd);

			}
			using (new GUILayout.HorizontalScope())
			{
				if (currentAnimLoop)
					EditorGUILayout.LabelField("Looping");
				else
					EditorGUILayout.LabelField("Not Looping");

				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.FloatField(currentAnimCurrentLength);
				EditorGUILayout.FloatField(currentAnimMaxLength);
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.TextField("");

			}
			if (currentAnimClip != null)
			{
				float startTime = 0.0f;
				float stopTime = currentAnimClip.length;
				time = EditorGUILayout.Slider(time, startTime, stopTime);
			}
			else if (AnimationMode.InAnimationMode())
				AnimationMode.StopAnimationMode();



		}
		else { currentAnimClip = null; }

		currentAnimatorAnimations.Clear();
		animators.Clear();
		GUILayout.Space(5);
	}

	private float time;
	void Update()
	{
		PreviewClip();
		if (currentAnimClip != null)
		{
			float startTime = 0.0f;
			float stopTime = currentAnimClip.length;


			time += Time.deltaTime * currentAnimSpd;

			if (time >= stopTime)
				time = startTime;


			currentAnimLoop = currentAnimClip.isLooping;
			currentAnimMaxLength = currentAnimClip.length;
			currentAnimCurrentLength = time;
		}
		else if (AnimationMode.InAnimationMode())
			AnimationMode.StopAnimationMode();

	}

	void PreviewClip()
	{
		if (currentAnimClip == null)
			return;
		if (currentAnimModel == null)
			return;
		if (currentAnim == null)
			return;

		AnimationMode.StartAnimationMode();
		if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
		{

			AnimationMode.BeginSampling();
			AnimationMode.SampleAnimationClip(currentAnimModel, currentAnimClip, time);
			AnimationMode.EndSampling();

			SceneView.RepaintAll();
		}

	}
}
