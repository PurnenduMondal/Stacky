using UnityEngine;

public class GradientBackground : MonoBehaviour
{
    [Header("Gradient Colours")]
    public Color topColour = new Color(0.01f, 0.01f, 0.08f);  // dark navy
    public Color bottomColour = new Color(0.40f, 0.70f, 1.00f);  // sky blue

    private Color currentTop;
    private Color currentBottom;
    private Material gradientMat;

    void Awake()
    {
        currentTop = topColour;
        currentBottom = bottomColour;

        // Use a built-in shader that always exists in Unity
        gradientMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        gradientMat.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnPostRender()
    {
        DrawGradient();
    }

    void DrawGradient()
    {
        gradientMat.SetPass(0);

        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);

        // Bottom left
        GL.Color(currentBottom);
        GL.Vertex3(0, 0, -1);

        // Bottom right
        GL.Color(currentBottom);
        GL.Vertex3(1, 0, -1);

        // Top right
        GL.Color(currentTop);
        GL.Vertex3(1, 1, -1);

        // Top left
        GL.Color(currentTop);
        GL.Vertex3(0, 1, -1);

        GL.End();
        GL.PopMatrix();
    }

    public void UpdateGradient(float height)
    {
        float t = Mathf.Clamp01(height / 30f);

        currentTop = Color.Lerp(
            new Color(0.05f, 0.05f, 0.20f),  // dark navy
            new Color(0.01f, 0.01f, 0.05f),  // near black
            t
        );

        currentBottom = Color.Lerp(
            new Color(0.40f, 0.70f, 1.00f),  // sky blue
            new Color(0.10f, 0.10f, 0.40f),  // deep night blue
            t
        );
    }

    void OnDestroy()
    {
        if (gradientMat != null)
            Destroy(gradientMat);
    }
}