using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject controllerSelection;
    private float rayRange = 10000.0f;

    private Vector3 controllerSelectionOffset = new Vector3(-0.05f, 0, 0);

    public SteamVR_Action_Boolean selectAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerAction");

    public GameObject oneController;
    public GameObject twoControllers;

    public GameObject easyDiff;
    public GameObject mediumDiff;
    public GameObject hardDiff;

    public GameObject diffCheck;

    private Vector3 difficultySelectionOffset;

    void Start()
    {
        GameObject.Find("/Player/SteamVRObjects/RightHand/ButtonHints").SetActive(false);
        SetupLineRenderer();
        if(Options.twoControllers)
        {
            controllerSelection.transform.position = twoControllers.transform.position + controllerSelectionOffset;
            EnableLeftHand();
        }
        else
        {
            controllerSelection.transform.position = oneController.transform.position + controllerSelectionOffset;
            DisableLeftHand();
        }
        difficultySelectionOffset = diffCheck.transform.position;
    }

    void SetupLineRenderer()
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.forward * rayRange);
        lineRenderer.startColor = new Color(128, 255, 255, 50);
        lineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray pointingRay = new Ray(transform.position, transform.forward);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.forward * rayRange);

        lineRenderer.startColor = new Color(128, 255, 255, 50);

        if (Physics.Raycast(pointingRay, out hit) && lineRenderer.enabled)
        {
            if (hit.collider.transform.CompareTag("DesertLevel"))
            {
                lineRenderer.startColor = new Color(255, 255, 0, 50);
                if (selectAction.GetStateUp(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("LEVEL 1 OPENED");
                    Destroy(transform.parent.parent.gameObject);
                    SceneManager.LoadScene("Desert");
                }
            }
            else if (hit.collider.transform.CompareTag("ForestLevel"))
            {
                lineRenderer.startColor = new Color(0, 255, 0, 50);
                if (selectAction.GetStateUp(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("LEVEL 2 OPENED");
                    Destroy(transform.parent.parent.gameObject);
                    SceneManager.LoadScene("Forest");
                }
            }
            else if (hit.collider.transform.CompareTag("OneController"))
            {
                if (selectAction.GetStateUp(SteamVR_Input_Sources.RightHand))
                {
                    controllerSelection.transform.position = hit.transform.position + controllerSelectionOffset;
                    Options.twoControllers = false;
                    DisableLeftHand();                                            
                }                                                                    
            }                                                                        
            else if (hit.collider.transform.CompareTag("TwoControllers"))            
            {                                                                        
                if (selectAction.GetStateUp(SteamVR_Input_Sources.RightHand))          
                {                                                                    
                    controllerSelection.transform.position = hit.transform.position + controllerSelectionOffset;
                    Options.twoControllers = true;
                    EnableLeftHand();
                }
            }
            else if (hit.collider.transform.CompareTag("EasyDiff") || 
                     hit.collider.transform.CompareTag("MediumDiff") ||
                     hit.collider.transform.CompareTag("HardDiff"))
            {
                if (selectAction.GetStateUp(SteamVR_Input_Sources.RightHand))
                {   
                    diffCheck.transform.position = new Vector3(difficultySelectionOffset.x,
                                                               hit.transform.position.y,
                                                               difficultySelectionOffset.z);
                    if (hit.collider.transform.tag == "EasyDiff")
                        Options.difficulty = 0;
                    if (hit.collider.transform.tag == "MediumDiff")
                        Options.difficulty = 1;
                    if (hit.collider.transform.tag == "HardDiff")
                        Options.difficulty = 2;
                }
            }
        }
    }

    void DisableLeftHand()
    {
        GameObject.Find("/Player/SteamVRObjects/LeftHand").SetActive(false);
    }

    void EnableLeftHand()
    {
        GameObject.Find("/Player/SteamVRObjects/LeftHand").SetActive(true);
    }
}
