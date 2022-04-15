using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class PirateLogic : AgentLogic
{
    #region Static Variables
    private static float _boxPoints = 1.5f;
    private static float _bigBoxPoints = 3.5f;
    private static float _boatPoints = 8f;
    #endregion
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Box"))
        {
            points += _boxPoints;
            objectivesCaptured++;
            Destroy(other.gameObject);
        }
        if(other.gameObject.tag.Equals("BigBox"))
        {
            points += _bigBoxPoints;
            objectivesCaptured++;
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag.Equals("Boat"))
        {
            points += _boatPoints;
            objectivesCaptured++;
            Destroy(other.gameObject);
        }
    }

}
