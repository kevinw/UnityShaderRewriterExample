using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class ShaderEditor : MonoBehaviour {
	public Color color;    // the color to inject into the shader
	public Shader shader;  // the shader to rewrite

	const double SHADER_WRITE_FREQ = 0.5; // how often to actually update unity's asset database

	Color lastColor;
	double lastUpdateTime = -1;
	bool needsUpdate;

	// The regex to rewrite shader code with.
	Regex regex = new Regex(@"(\/\*hardCodedColor\*\/)(.*)(;)");

	void Update () {
		if (color == lastColor)
			return;

		lastColor = color;
		
		var path = AssetDatabase.GetAssetPath(shader.GetInstanceID());
		var code = File.ReadAllText(path);
		var newCode = regex.Replace(code, (m) => m.Groups[1] + hlslColor(color) + m.Groups[3]);
		File.WriteAllText(path, newCode);
		needsUpdate = true;
	}

	void OnEnable()
	{
		EditorApplication.update += OnEditorUpdate;
	}

	void OnDisable()
	{
		EditorApplication.update -= OnEditorUpdate;
	}

	void OnEditorUpdate()
	{
		if (!needsUpdate)
			return;

		var now = EditorApplication.timeSinceStartup;
		if (now - lastUpdateTime < SHADER_WRITE_FREQ)
			return;

		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(shader.GetInstanceID()));
		lastUpdateTime = now;
		needsUpdate = false;
	}

	static string hlslColor(Color color)
	{
		return "fixed4(" +
			color.r.ToString() + ", " +
			color.g.ToString() + ", " +
			color.b.ToString() + ", " +
			color.a.ToString() + ")";
	}

}
