using GameUtils;
using UnityEngine;

public class GeneralActions : MonoBehaviour
{
    // Actions
    public void Dash(){
        int remainingMovement = UGame.GetActiveCreatureStats().GetRemainingMovement();
        int moveSpeed = UGame.GetActiveCreatureStats().GetMovementSpeed();
        UGame.GetActiveCreatureStats().SetReaminingMovement(remainingMovement + moveSpeed);
    }

    // Bonus Actions

    // Reactions
}
