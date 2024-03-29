using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class ShaderRunner : MonoBehaviour
{
    public ComputeShader computeShader;
    
     int numAgents = 10000;
     int width = 2560;
      int height = 1440;
    public RawImage displayImage; 

    private ComputeBuffer agentsBuffer;
    private RenderTexture renderTexture;
    private RenderTexture trailTexture;
    private void Start()
    {
        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetFloat("time", Time.fixedTime);
        computeShader.SetFloat("width", width);
        computeShader.SetFloat("height", height);
        
        RectTransform rt = displayImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height); 

        InitializeRenderTexture();
        InitializeTrailTexture();
        displayImage.texture = renderTexture;

        spawnCenter();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Load the main menu scene
            SceneManager.LoadScene("Mainmenuscene"); 
        }
       CopyTrailToRenderTexture();
        int updateKernelID = computeShader.FindKernel("updateAgent");
        computeShader.SetTexture(updateKernelID, "TrailMap", trailTexture);
        computeShader.SetBuffer(updateKernelID, "AgentsOut", agentsBuffer);
        computeShader.Dispatch(updateKernelID, numAgents / 256, 1, 1);

        DrawAgentsOnTexture();
        CopyTrailToRenderTexture();
        UpdateTrailMap();  
        CopyTrailToRenderTexture();
       Blur(); 
       CopyTrailToRenderTexture();
       
    }
    void Blur() {
    int blurKernelID = computeShader.FindKernel("BlurTrail");
    computeShader.SetTexture(blurKernelID, "TrailMap", trailTexture); 
    computeShader.SetTexture(blurKernelID, "Blur", renderTexture);
    computeShader.Dispatch(blurKernelID, renderTexture.width / 8, renderTexture.height / 8, 1);
}



void UpdateTrailMap() {
    int trailKernelID = computeShader.FindKernel("UpdateTrail");
    computeShader.SetTexture(trailKernelID, "TrailMap", renderTexture);
    computeShader.Dispatch(trailKernelID, width / 8, height / 8 , 1);
    


}
void DrawAgentsOnTexture()
{
    
    int drawKernelID = computeShader.FindKernel("DrawAgent");
    computeShader.SetBuffer(drawKernelID, "AgentsOut", agentsBuffer);
    computeShader.SetTexture(drawKernelID, "Result", renderTexture);
    computeShader.Dispatch(drawKernelID, numAgents / 16, 1, 1);
}
void CopyTrailToRenderTexture()
{
    
    Graphics.Blit(renderTexture, trailTexture);
}


    void InitializeRenderTexture()
{
    renderTexture = new RenderTexture(width, height, 0)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    renderTexture.Create();
}
    void InitializeTrailTexture()
{
    trailTexture = new RenderTexture(width, height, 0)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    trailTexture.Create();
}
    void spawnCenter() {


        agentsBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        int kernelID = computeShader.FindKernel("spawnInCenter");
        computeShader.SetBuffer(kernelID, "AgentsOut", agentsBuffer);

        computeShader.Dispatch(kernelID, numAgents / 16, 1, 1);
        
       
void Noise() {
        
        int width = 2560;
        int height = 1440;
        RenderTexture renderTexture = new RenderTexture(width, height, 24)
        
        {
            enableRandomWrite = true,
            format = RenderTextureFormat.ARGBFloat
        };
        renderTexture.Create();

        int kernelID = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kernelID, "Result", renderTexture);
        computeShader.Dispatch(kernelID, Mathf.CeilToInt((float)width / 8f), Mathf.CeilToInt((float)height / 8f), 1);



        displayImage.texture = renderTexture; // Set the RenderTexture as the RawImage texture
    }

    }
private void OnDestroy()
    {
        // Release the ComputeBuffer when the GameObject is destroyed
        if (agentsBuffer != null)
        {
            agentsBuffer.Release();
        }
    }


   
}
