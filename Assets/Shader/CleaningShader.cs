using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeMonkey.Utils;

public class CleaningShader : MonoBehaviour {
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Texture2D dirtMaskTextureBase;
    [SerializeField] private Texture2D dirtBrush;
    [SerializeField] private Material material;

    [SerializeField] private Texture2D dirtMaskTexture;

    [SerializeField] private float dirtAmountTotal;
    [SerializeField] private float dirtAmount;
    [SerializeField] private Vector2Int lastPaintPixelPosition;
    [SerializeField] private int pixelXOffset;
    [SerializeField] private int pixelYOffset;
    [SerializeField] private int dirtAmountRounded;
    [SerializeField] bool startCleaningTheDirt = false;
    //[SerializeField] CleanTheTable cleanTheTable;
    [SerializeField] private TextMeshProUGUI dirtAmountPrecentage;
    [SerializeField] private Image radialProgressImage; // Reference to your radial progress circle
    [SerializeField] private int dirtAmountCountUp = 0; // New variable to count up
    [SerializeField] private GameObject dirt;
    public static CleaningShader Instance { get; private set; }
    private void Awake() 
    {
        //dirt = GetComponent<GameObject>();
        //dirt.GetComponent<GameObject>();
        dirt = this.gameObject;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want the instance to persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
        //cleanTheTable = CleanTheTable.Instance;
        // Ensure the cleanTheTable reference is correctly set


        dirtMaskTexture = new Texture2D(dirtMaskTextureBase.width, dirtMaskTextureBase.height);
        dirtMaskTexture.SetPixels(dirtMaskTextureBase.GetPixels());
        dirtMaskTexture.Apply();
        material.SetTexture("_DirtMask", dirtMaskTexture);


        dirtAmountTotal = 0f;
        for (int x = 0; x < dirtMaskTextureBase.width; x++) {
            for (int y = 0; y < dirtMaskTextureBase.height; y++) {

                dirtAmountTotal += dirtMaskTextureBase.GetPixel(x, y).g;

            }
        }
        dirtAmount = dirtAmountTotal;
        

        //FunctionPeriodic.Create(() => {
        //    uiText.text = Mathf.RoundToInt(GetDirtAmount() * 100f) + "%";
        //}, .03f);
    }
    public void StartCleaning()
    {
        // Ensure the cleaningShader reference is correctly set
        //if (cleanTheTable == null)
        //{
        //    cleanTheTable = CleanTheTable.Instance;
        //}
        Debug.Log("Cleaning Started");
        startCleaningTheDirt = true;
    }
    public void StopCleaning()
    {
        Debug.Log("Cleaning Stopped");
        startCleaningTheDirt = false;
        
    }
    private void Update() // i should add something that trigger the table oncmore before it ended
    {
        if (startCleaningTheDirt == true)
        {
            if (Input.GetMouseButton(0))
            {

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit, Mathf.Infinity, layerMask))
                {
                    Vector2 textureCoord = raycastHit.textureCoord;

                    int pixelX = (int)(textureCoord.x * dirtMaskTexture.width);
                    int pixelY = (int)(textureCoord.y * dirtMaskTexture.height);

                    Vector2Int paintPixelPosition = new Vector2Int(pixelX, pixelY);

                    int paintPixelDistance = Mathf.Abs(paintPixelPosition.x - lastPaintPixelPosition.x) + Mathf.Abs(paintPixelPosition.y - lastPaintPixelPosition.y);
                    int maxPaintDistance = 7;
                    if (paintPixelDistance < maxPaintDistance)
                    {
                        // Painting too close to last position
                        return;
                    }
                    lastPaintPixelPosition = paintPixelPosition;

                    pixelXOffset = pixelX - (dirtBrush.width / 2);

                    pixelYOffset = pixelY - (dirtBrush.height / 2);

                    for (int x = 0; x < dirtBrush.width; x++)
                    {
                        for (int y = 0; y < dirtBrush.height; y++)
                        {


                            Color pixelDirt = dirtBrush.GetPixel(x, y);
                            Color pixelDirtMask = dirtMaskTexture.GetPixel(pixelXOffset + x, pixelYOffset + y);

                            float removedAmount = pixelDirtMask.g - (pixelDirtMask.g * pixelDirt.g);
                            dirtAmount -= removedAmount;

                            dirtMaskTexture.SetPixel(
                                pixelXOffset + x,
                                pixelYOffset + y,
                                new Color(0, pixelDirtMask.g * pixelDirt.g, 0)
                            );
                        }
                    }
                    dirtMaskTexture.Apply();
                }
            }
            Debug.Log("Dirt Amount: " + Mathf.RoundToInt(GetDirtAmount() * 100f) + "%");

            dirtAmountRounded = Mathf.RoundToInt(GetDirtAmount() * 100f);

            

            int newProgress = 100 - dirtAmountRounded;
            // If progress reaches 94% or more, jump to 100% if it hasn't already
            if (newProgress >= 92 && dirtAmountCountUp < 90)
            {
                dirtAmountCountUp = 100;
                dirtAmountPrecentage.text = dirtAmountCountUp + "%";
                radialProgressImage.fillAmount = dirtAmountCountUp / 100f;
            }
            // Only update if progress has increased by at least 10%
            else if (newProgress >= dirtAmountCountUp + 10)
            {
                dirtAmountCountUp = Mathf.FloorToInt(newProgress / 10f) * 10; // Snap to the nearest 10%
                dirtAmountPrecentage.text = dirtAmountCountUp + "%";
                radialProgressImage.fillAmount = dirtAmountCountUp / 100f;
            }
            if (dirtAmountRounded <= 5)
            {
                dirtAmountCountUp = 100;
				dirtAmountPrecentage.text = dirtAmountCountUp + "%";
				radialProgressImage.fillAmount = dirtAmountCountUp / 100f;
                Debug.Log("Dirt Amount: " + dirtAmountRounded + "%");
                
                startCleaningTheDirt = false;
                dirt.GetComponent<MeshRenderer>().enabled = false;
                //Debug.Log(cleanTheTable);
                //cleanTheTable.StopCleaning();
            }
            // Calculate the corresponding count-up value
            //dirtAmountCountUp = 100 - dirtAmountRounded;>>

            //dirtAmountPrecentage.text = (dirtAmountCountUp - 6) + "%"; old Logic

            // Map the internal dirtAmountRounded to a value from 0 to 100 for the player's view
            //int playerViewValue = Mathf.RoundToInt(((dirtAmountRounded - 5f) / (100f - 6f)) * 100f);
            //dirtAmountPrecentage.text = dirtAmountCountUp + "%"; >>
            //radialProgressImage.fillAmount = GetDirtAmount(); // Update the radial progress

            //radialProgressImage.fillAmount = dirtAmountCountUp / 100f;>>


        }
    }
    public bool isDoneCleaning()
    {
        if (dirtAmountRounded < 6)
        {
            return true;

        }
        else
        {
            return false;
        }
    }
    private float GetDirtAmount() {
        return this.dirtAmount / dirtAmountTotal;
    }

}