using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//A handler for the test chamber sign which will have to be set manually for the start of each level
public class TestChamberSign : MonoBehaviour {
    public Color OffColor;
    public Color FlickerColor;
    public int ChamberNumber = 01;
    public bool BoxHit = false;
    public bool BoxDrop = false;
    public bool Droids = false;
    public bool Drowning = false;
    public bool Fling = false;
    public bool NoDrinkWater = false;
    public bool PelletHit = false;
    public bool PelletPuzzle = false;
    public bool PortalJump = false;

    //It's going to be messy...
    [Header("Sign Items")]
    public GameObject LevelCompleteTally;
    //All of our different panel items
    public GameObject Icon_BoxHit;
    public GameObject Icon_BoxDrop;
    public GameObject Icon_Droids;
    public GameObject Icon_Drowning;
    public GameObject Icon_Fling;
    public GameObject Icon_NoDrinkWater;
    public GameObject Icon_PelletHit;
    public GameObject Icon_PelletPuzzle;
    public GameObject Icon_PortalJump;
    Color TextStartColor;
    public TextMeshPro SignTitle;
    public TextMeshPro LevelNumber;

    public List<Material> DynamicMaterials = new List<Material>();
    List<Renderer> ItemRenderers = new List<Renderer>();

    bool bTriggerPanelActive = false;
    bool bPanelActiveComplete = false;
    float TriggerPanelActiveTime = 0;
    Range FlickerDuration = new Range(0.05f, 0.125f);
    float FlickerStartTime = 0f;
    float TotalFlickerTime = 1f;
    float NextFlickerTime = 0;

    public void Start()
    {
        TotalFlickerTime = Random.Range(1f, 2f);    //Add a random to how long this sign will flicker for
        AssignIconType(Icon_BoxHit, BoxHit);
        AssignIconType(Icon_BoxDrop, BoxDrop);
        AssignIconType(Icon_Droids, Droids);
        AssignIconType(Icon_Drowning, Drowning);
        AssignIconType(Icon_Fling, Fling);
        AssignIconType(Icon_NoDrinkWater, NoDrinkWater);
        AssignIconType(Icon_PelletHit, PelletHit);
        AssignIconType(Icon_PelletPuzzle, PelletPuzzle);
        AssignIconType(Icon_PortalJump, PortalJump);
        SetupTally(LevelCompleteTally);

        //Now we need to get all other materials
        Renderer[] ChildRenderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < ChildRenderers.Length; i++)
        {
            if (!ItemRenderers.Contains(ChildRenderers[i])) //Need to make a clone material and add this for control
            {
                ItemRenderers.Add(ChildRenderers[i]);
                Material MatClone = new Material(ChildRenderers[i].material);
                DynamicMaterials.Add(MatClone);
                ChildRenderers[i].material = MatClone;
            }
        }

        TextStartColor = SignTitle.faceColor;
        SignTitle.text = ChamberNumber.ToString("00");
        LevelNumber.text = ChamberNumber.ToString("00") + "/19";

        SetPanelOff();
    }

    void SetPanelOff()
    {
        Color ColorOff = new Color(OffColor.r, OffColor.g, OffColor.b, 0f);
        for (int i=0; i<DynamicMaterials.Count; i++)
        {
            DynamicMaterials[i].SetColor("_Blend", ColorOff);
        }
        SignTitle.faceColor = ColorOff;
        LevelNumber.faceColor = ColorOff;
    }

    void SetupTally(GameObject tallyItem)
    {
        Renderer TallyRender = tallyItem.GetComponent<Renderer>();
        if (TallyRender)
        {
            ItemRenderers.Add(TallyRender);
            Material TallyClone = new Material(TallyRender.material);
            TallyClone.SetFloat("_UVCut", (float)ChamberNumber / 19.0f);
            DynamicMaterials.Add(TallyClone);
            TallyRender.material = TallyClone;
        }
    }

    void AssignIconType(GameObject thisIcon, bool bThisState)
    {
        Renderer IconRenderer = thisIcon.GetComponent<Renderer>();
        if (IconRenderer) {
            ItemRenderers.Add(IconRenderer);
            Material CloneMat = new Material(thisIcon.GetComponent<Renderer>().material);
            CloneMat.SetFloat("_TextureBlend", bThisState ? 1f : 0f);
            DynamicMaterials.Add(CloneMat);
            IconRenderer.material = CloneMat;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            if (!bTriggerPanelActive)
            {
                SetTriggerPanelActive();
            }
        }
    }

    public void SetTriggerPanelActive()
    {
        bTriggerPanelActive = true;
        TriggerPanelActiveTime = Time.time;
        FlickerStartTime = Time.time;
        NextFlickerTime = Time.time + Random.Range(0.125f, 0.25f);
        bFlickerOn = false; //So that when we come out of this we're lit down
        SetSignColor(0f);   //Show power on before we do flickering
    }

    public void SetSignColor(float GreyAlpha)
    {
        Color BlendCol = new Color(FlickerColor.r, FlickerColor.g, FlickerColor.b, GreyAlpha);
        for (int i=0; i<DynamicMaterials.Count; i++)
        {
            DynamicMaterials[i].SetColor("_Blend", BlendCol);
        }
        SignTitle.faceColor = BlendCol;
        LevelNumber.faceColor = BlendCol;
    }

    bool bFlickerOn = true;

    void Update()
    {
        if (bTriggerPanelActive && !bPanelActiveComplete)
        {
            if (Time.time > NextFlickerTime)
            {
                if (Time.time > FlickerStartTime + TotalFlickerTime)
                {
                    SetSignColor(1f);
                    bPanelActiveComplete = true;
                } else
                {
                    NextFlickerTime = Time.time + FlickerDuration.GetRandom();
                    bFlickerOn = !bFlickerOn;
                    if (bFlickerOn)
                    {
                        NextFlickerTime = Time.time + FlickerDuration.GetRandom() * Random.Range(2f, 5f);
                    }
                    SetSignColor(bFlickerOn ? 1f : 0.25f);
                }
            }
        }
    }
}
