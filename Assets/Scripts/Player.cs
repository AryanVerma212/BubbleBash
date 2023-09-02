using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class Player : MonoBehaviour
{
    [SerializeField] Board board;
    private int player_location;
    private SpriteRenderer spriteRenderer;
    private int bullet_color;
    void Start()
    {   
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        bullet_color = UnityEngine.Random.Range(2, board.GetNumColors() + 2);   // ignore the white and black colours specially reserved
        spriteRenderer.color = Board.colorList[bullet_color];
    }
    void Update()
    {
        Board obj=FindObjectOfType<Board>();
        mainmenu obj2=FindObjectOfType<mainmenu>();
        if(Input.GetKeyDown(KeyCode.RightArrow)) {
            int old_player_location = player_location;
            player_location = Math.Min(player_location + 1, board.GetColumn() - 1);
            board.UpdatePlayer(old_player_location, player_location);
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            int old_player_location = player_location;
            player_location = Math.Max(player_location - 1, 0);
            board.UpdatePlayer(old_player_location, player_location);
        }
        else if(Input.GetKeyDown(KeyCode.Space)) {
            obj.GenerateRow();
            board.CreateBullet(1, player_location, bullet_color);
            bullet_color = UnityEngine.Random.Range(2, board.GetNumColors() + 2);   // ignore the white and black colours specially reserved
            spriteRenderer.color = Board.colorList[bullet_color];
        }

    }
    
}
