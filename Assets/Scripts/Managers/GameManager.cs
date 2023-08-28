using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameState currentState;

    [Header("Population Data")]
    public int totalGreenPonePopulation ;
    public int totalOrangePonePopulation ;
    public int totalPurplePonePopulation ;

  


    [Header("RayCastClick")]
    public LayerMask poneLayers; // Assign the pone layers in the Inspector
    public int chosenPoneLayer;
    private Pone selectedPlayerPone = null;


    [Header("UI")]
    public TextMeshProUGUI choosePoneText;
    public TextMeshProUGUI endGameButtonText;
    public Button endGameButton;


    private void Start()
    {
        ResetGame();
   
    }

    private void Update()
    {
       
        RaycastClick();
        CheckForGameOver();

    }
    private void RaycastClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, poneLayers))
            {
                
                Pone clickedPone = hit.collider.gameObject.GetComponent<Pone>();
                switch (currentState)
                {
                    case GameState.ChoosingPone:
                        if (chosenPoneLayer == 0)
                        {
                            chosenPoneLayer = hit.collider.gameObject.layer;
                            choosePoneText.text = "You are " + LayerMask.LayerToName(chosenPoneLayer);
                            StartCoroutine(ClearChosenPoneTextAfterDelay(2f));
                        }
                        break;

                    case GameState.PlayerTurn:
                        if (IsPlayerControlledPone(clickedPone.gameObject.layer))
                        {
                            // Store this Pone as the selected player Pone for attack
                            selectedPlayerPone = clickedPone;
                        }
                        else if (selectedPlayerPone != null)
                        {
                            // If the clicked Pone is an enemy and a neighbor of the selected player Pone, attack
                            List<Pone> enemyNeighbors = selectedPlayerPone.GetEnemies();

                            if (enemyNeighbors.Contains(clickedPone))
                            {
                                AttackEnemy(selectedPlayerPone, clickedPone);
                            }
                        }
                        break;
                    

                }
            }
        }
    }
    private void AttackEnemy(Pone attacker, Pone defender)
    {
        if (attacker.currentPopulation > defender.currentPopulation)
        {
            // Capture the defender Pone
            defender.currentPopulation = attacker.currentPopulation - defender.currentPopulation ; // Capture remaining population
            defender.ChangeType(attacker.PoneType, attacker.GetComponent<MeshRenderer>().material, attacker.gameObject.name, attacker.gameObject.layer); // Change type and material
            attacker.currentPopulation = 1; // 1 should stay at the attacking pone
        }
        else if (attacker.currentPopulation == defender.currentPopulation)
        {
            attacker.currentPopulation = defender.currentPopulation = 1;
        }
        else
        {
            // Just reduce the populations if defender is stronger
            defender.currentPopulation = defender.currentPopulation - (attacker.currentPopulation - 1); // Subtract the attacking force, leaving 1 behind
            attacker.currentPopulation = 1; // 1 should stay at the attacking pone
        }

        // Update the UI for both pones
        attacker.UpdatePopulationText();
        defender.UpdatePopulationText();


         // all pone listed their neighbour enemies again
          Pone[] allPones = FindObjectsOfType<Pone>();

          foreach (Pone pone in allPones)
          {
              pone.enemies.Clear();
              pone.Raycasting360();


          }

        // CheckForGameOver buraya yaz

    }
    public void ResetGame()
    {
       
        currentState = GameState.ChoosingPone;
        choosePoneText.text = "Choose your pone by clicking on it!";
 
        DistributePopulation();

       
        
    }
    public void NextTurnButton()
    {
        EndPlayerTurn();
        StartCoroutine(AllEnemyTurnsCoroutine());
    }

    #region End Game
    public void ReloadScene()
    {
        // Get the current Scene name
        string sceneName = SceneManager.GetActiveScene().name;

        // Load it
        SceneManager.LoadScene(sceneName);
        endGameButton.gameObject.SetActive(false);
    }

    void CheckForGameOver() // bunu en son hit içinde çagır
    {
        Pone[] allPones = FindObjectsOfType<Pone>(); // bu sıkıntı update içinde
        // if (allPones.Length == 0) return; // No pones on the board, so just return

        PoneScriptableObjects firstPoneType = allPones[0].PoneType;
        //Debug.Log("First pone type: " + firstPoneType);
        foreach (Pone pone in allPones) //// bu sıkıntı update içinde
        {
            if (pone.PoneType != firstPoneType)
            {
                return; // Found a different type, so the game isn't over
            }
        }
        ;
       
        //Debug.Log("Is player's pone: " + IsPlayerControlledPone(firstPoneType.poneLayer));


        if (IsPlayerControlledPone(firstPoneType.poneLayer))
        {
            // You win
            GameOver("You Win, Play Again");
        }
        else
        {

            // You lose
            GameOver("Play Again");
        }
    }

    void GameOver(string endGameMessage)
    {
        // Your game over logic here
         Debug.Log("Game Over: " + endGameMessage);
        // Activate the endGameButton and set its text
        endGameButton.gameObject.SetActive(true);
        endGameButtonText.text = endGameMessage;
    }

    #endregion
    #region Distribution Pone counts to pones with Enemies
    private Dictionary<string, int> CountAllPonePiecesByType()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        Pone[] allPones = FindObjectsOfType<Pone>();
        foreach (Pone pone in allPones)
        {
            string typeName = pone.PoneType.poneName;
            if (!counts.ContainsKey(typeName))
            {
                counts[typeName] = 0;
            }
            counts[typeName] += 1; 
        }
        return counts;
    }
    private void DistributePoneCountsToPonesWithEnemies()
    {
        // Count all pone pieces by type
        Dictionary<string, int> ponePieceCounts = CountAllPonePiecesByType();
      
        // Get all pones in the scene
        Pone[] allPones = FindObjectsOfType<Pone>();

        // Distribute the counts to pones with enemies
        foreach (string poneType in ponePieceCounts.Keys)
        {
            List<Pone> ponesWithEnemies = allPones.Where(p => p.PoneType.poneName == poneType && p.GetEnemies().Count > 0).ToList();
            int numberOfPonesWithEnemies = ponesWithEnemies.Count;

            if (numberOfPonesWithEnemies == 0) continue;

            int totalTypeCount = ponePieceCounts[poneType];
            int remainingToDistribute = totalTypeCount;

            while (remainingToDistribute > 0)
            {
                for (int i = 0; i < numberOfPonesWithEnemies && remainingToDistribute > 0; i++)
                {
                    ponesWithEnemies[i].currentPopulation++;
                    ponesWithEnemies[i].UpdatePopulationText();
                    remainingToDistribute--;
                }
            }
        }

    }



    #endregion
    #region Enemy Turn
    public IEnumerator AllEnemyTurnsCoroutine()
    {
    // Get all pones
    Pone[] allPones = FindObjectsOfType<Pone>();

    // Filter out the player-controlled pones and sort the remaining pones by population
    List<Pone> sortedPones = allPones.Where(p => !IsPlayerControlledPone(p.gameObject.layer)).ToList();
    sortedPones = SortPonesByPopulation(sortedPones);

    // Loop through each sorted pone type
    foreach (var poneTypeGroup in sortedPones.GroupBy(p => p.PoneType))
    {
        // Display the current pone type
        choosePoneText.text = "Now attacking: " + poneTypeGroup.Key.poneName;
        yield return new WaitForSeconds(2f);  // Wait for a moment to show the text

        foreach (Pone pone in poneTypeGroup)
        {
            // Get enemies for this pone
            List<Pone> enemyNeighbors = pone.GetEnemies();

            for (int i = 0; i < enemyNeighbors.Count; i++)
            {
                // Perform the attack
                AttackEnemy(pone, enemyNeighbors[i]);

                // Refresh the list of enemies for this pone
                enemyNeighbors = pone.GetEnemies();

                yield return new WaitForSeconds(0.5f);  // Wait for a moment to show the attack
            }
        }
    }

    // Show "Your turn again" text
    choosePoneText.text = "Your turn again";
    yield return new WaitForSeconds(2f); // Wait for 2 seconds
    choosePoneText.text = ""; // Clear the text

    // End the enemy turn and switch back to player's turn
    EndEnemyTurn();
    }


    private List<Pone> SortPonesByPopulation(List<Pone> pones)
    {
        return pones.OrderBy(p => p.currentPopulation).ToList();
    }



    #endregion
    #region Pone Count and Population Distribution
    private void DistributePopulation()
    {
        // Count pone instances
        Pone[] allPones = FindObjectsOfType<Pone>();
        int greenPoneCount = 0;
        int orangePoneCount = 0;
        int purplePoneCount = 0;
        
        foreach (Pone pone in allPones)
        {
           // Debug.Log("Pone GameObject: " + pone.gameObject.name + ", Type: " + pone.PoneType.poneName);
            switch (pone.PoneType.poneName)
            {
                case "Green Pone":
                    greenPoneCount++;
                    break;
                case "Orange Pone":
                    orangePoneCount++;
                    break;
                case "Purple Pone":
                    purplePoneCount++;
                    break;
                default:
                    Debug.LogError("Unknown Pone type: " + pone.PoneType.poneName);
                    break;
            }
        }
       
        // Distribute population
        DistributePopulationToType(allPones, "Green Pone", greenPoneCount, totalGreenPonePopulation);
        DistributePopulationToType(allPones, "Orange Pone", orangePoneCount, totalOrangePonePopulation);
        DistributePopulationToType(allPones, "Purple Pone", purplePoneCount, totalPurplePonePopulation);
        
    }
    private void DistributePopulationToType(Pone[] allPones, string poneTypeName, int poneCount, int totalPopulation)
    {
        if (poneCount == 0) return; // Avoid division by zero

        List<Pone> relevantPones = new List<Pone>();

        // Filter out pones that match the given type name and populate relevantPones list.
        foreach (Pone pone in allPones)
        {
            if (pone.PoneType.poneName == poneTypeName)
            {
                relevantPones.Add(pone);
            }
        }

        // Initialize all pones with at least 1 population.
        foreach (Pone pone in relevantPones)
        {
            pone.currentPopulation = 1;
        }

        // Distribute the remaining population.
        int remainingPopulation = totalPopulation - poneCount;  // We already assigned 1 to each pone, so subtract poneCount.
     


        while (remainingPopulation > 0)
        {
            // Randomly pick one pone and increase its population by 1.
            int randomIndex = UnityEngine.Random.Range(0, relevantPones.Count);
         
            relevantPones[randomIndex].currentPopulation++;
            remainingPopulation--;
        }

        // Update population text.
        foreach (Pone pone in relevantPones)
        {
            pone.UpdatePopulationText(); // Assumes you have a method to update the text/UI in your Pone class.
        }
    }

    #endregion
    #region Choosing Pone
    private IEnumerator ClearChosenPoneTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        choosePoneText.text = "";
        EndChoosingPoneTurn();
    }
    public bool IsPlayerControlledPone(int poneLayer)
    {
        return chosenPoneLayer == poneLayer;
    }
    #endregion
    #region GameState
    
    public enum GameState { ChoosingPone, PlayerTurn, EnemyTurn, EndTurn }
    

    // ...
    public void EndChoosingPoneTurn()
    {
        currentState = GameState.PlayerTurn;
    }
    public void EndPlayerTurn()
    {
        currentState = GameState.EnemyTurn;
    }

    public void EndEnemyTurn()
    {
        currentState = GameState.PlayerTurn;
        DistributePoneCountsToPonesWithEnemies();
    }

    public void GameOverState()
    {
        currentState = GameState.EndTurn;

    }

  
    #endregion

}
