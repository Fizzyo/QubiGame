using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundingManager : MonoBehaviour
{
    public bool isGrounded = false;
    public PlayerBehaviour player;

    private void Start()
    {
        if(!player) player = this.GetComponentInParent<PlayerBehaviour>();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
        player.Land();
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
