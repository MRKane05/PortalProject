using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script handles really basic movements that might happen in the world. The likes of point A to point B such as extending pistons
public class SimpleMoveObject : MonoBehaviour {
    int pos = -1;
    public List<GameObject> MoveToPositions;
    public float MoveSpeed = 2f;
    public bool bDoingMove = false;
    //Of course we'll also need audio
    public AudioClip StartMoveSound, MoveSound, EndMoveSound;
    public AudioSource ourAudio;

    void FixedUpdate()
    {
        if (bDoingMove)
        {
            if ((Vector3.SqrMagnitude(MoveToPositions[pos].transform.position - gameObject.transform.position) < 0.01f))
            {
                bDoingMove = false;
            }
            else
            {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, MoveToPositions[pos].transform.position, MoveSpeed * Time.fixedDeltaTime);
            }
        }
    }

    public void MoveNext()
    {
        pos++;
        if (pos < MoveToPositions.Count)    //Only move if we've somewhere to move to
        {
            bDoingMove = true;
        }
    }
}
