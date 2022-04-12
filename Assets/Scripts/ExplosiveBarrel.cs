using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Enemy" || other.tag == "Boat"){

            Destroy(other.gameObject);
            //TODO ADD EXPLOSION
            Destroy(gameObject);
        }
    }
}
