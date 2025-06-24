using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    protected GridMovementCharacter player;

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<GridMovementCharacter>();
        }
        else
        {
            Debug.LogError("Player object not found. Ensure the player has the 'Player' tag.");
        }
    }

    public abstract void PerformAI();
}