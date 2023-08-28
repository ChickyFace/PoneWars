using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pone : MonoBehaviour
{

    public GameManager gameManager;


    [Header("Population Data")]
    //public int population;
    public int currentPopulation;


    [Header("Population Text")]
    public TextMeshPro currentPopulationText;

    [Header("ScriptableObject")]
    public PoneScriptableObjects PoneType;

    [Header("Raycast360")]
    public float rayLength;
    public LayerMask layerMask;

    [Header("Enemy Data")]
    public List<Pone> enemies = new List<Pone>(); // Add this line to store enemies



    private void Start()
    {
        
        UpdatePopulationText();
        Raycasting360();
       

    }
    void Update()
    {

       /* if (gameManager.currentState == GameManager.GameState.PlayerTurn &&
                    gameManager.IsPlayerControlledPone(Mathf.FloorToInt(Mathf.Log(PoneType.poneLayer.value, 2))))
        {

            //Raycasting360();
            // Respond to player input to control this pone
            // For example:
            // HandleMovement();
            // HandleActions();
        }*/
    }
    
    public void Raycasting360()
    {
        Vector3 origin = transform.position;

        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, rayLength, layerMask))
            {
                Pone hitPone = hit.collider.gameObject.GetComponent<Pone>();
                if (hitPone.PoneType.poneLayer != PoneType.poneLayer)
                {
                    Pone enemyPone = hitPone;
                    //Debug.Log("Enemy detected: " + enemyPone.PoneType.poneName);
                    enemies.Add(enemyPone); // Add the hit Pone to this pone's enemy list


                }
            }
            Debug.DrawRay(origin, direction * rayLength, Color.red, 0.5f);
        }
    }
    public List<Pone> GetEnemies()
    {
        return enemies;
    }
    public void UpdatePopulationText()
    {
        currentPopulationText.text = currentPopulation.ToString();
    }
    public void ChangeType(PoneScriptableObjects newPoneType, Material newMaterial, string newName, int newLayer)
    {
    
        this.PoneType = newPoneType;

    
        this.GetComponent<MeshRenderer>().material = newMaterial;

     
        this.gameObject.name = newName;

      
        this.gameObject.layer = newLayer;

    }






}
