using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightManager : MonoBehaviour
{
    private Transform highlightedPoneTransform;
    private Transform selection;
    private RaycastHit raycastHit;

    private GameManager gameManager;
   
    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }
    void Update()
    {
        if (gameManager.currentState == GameManager.GameState.PlayerTurn)
        {
            CheckForClick();
        }

    }


    private void FixedUpdate()
    {
        if (gameManager.currentState == GameManager.GameState.PlayerTurn)
                    
        {
            HighlightRay();
        }

    }
    private void CheckForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            if (highlightedPoneTransform)
            {
                if (selection != null)  // üstünde ve tıklıysa, tekrar tıklanırsa kapat
                {
                    selection.gameObject.GetComponent<Outline>().enabled = false;
                }
                //üstündeyse,tıklayınca bu sefer highlight selection oldu
                selection = raycastHit.transform;
                selection.gameObject.GetComponent<Outline>().enabled = true;
                highlightedPoneTransform = null;
            }
            else
            {
                //üstünde degilse ama önceden tıklanmışsa
                if (selection)
                {
                    selection.gameObject.GetComponent<Outline>().enabled = false;
                    selection = null;
                }
            }
        }
    }
 

    private void HighlightRay()
    {
        if (highlightedPoneTransform != null) // original material i geri getiriyor
        {
            highlightedPoneTransform.gameObject.GetComponent<Outline>().enabled = false;
            highlightedPoneTransform = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out raycastHit, 100f) /*&& hit.collider.gameObject == this.gameObject*/
            && !EventSystem.current.IsPointerOverGameObject())  // Ray objeye geldiyse ve önünde ui yoksa
        {
            highlightedPoneTransform = raycastHit.transform;
           
            if (highlightedPoneTransform.CompareTag("Pone") && highlightedPoneTransform != selection) //üstüne gelinen obje pone tagli ve seçilmemişse
            {
                if (highlightedPoneTransform.gameObject.GetComponent<Outline>() != null) //üstünde Outline component'ı varsa 
                {
                    highlightedPoneTransform.gameObject.GetComponent<Outline>().enabled = true; // aktif et
                }
                else  //üstünde Outline component'ı yoksa ve diger durumlarda
                {
                    Outline outline = highlightedPoneTransform.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    highlightedPoneTransform.gameObject.GetComponent<Outline>().OutlineColor = Color.yellow;
                    highlightedPoneTransform.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
                }

            }
            else // üstüne gelinen obje seçili değil ve tagli değilse
            {
                highlightedPoneTransform = null;
            }

        }
    }

    private void HighlightEnemyPone()
    {

    }
}
