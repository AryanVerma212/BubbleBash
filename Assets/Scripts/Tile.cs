using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject destructionPrefab;
    public enum TileType {
        Empty,
        Player,
        Block,
        Bullet
    }
    private TileType tile_type = TileType.Empty;
    private int color_index = 1;
    private bool is_floating = false;

    public int GetColor() {
        return this.color_index;
    }

    public void SetColor(int color_index) {
        this.color_index = color_index;
        spriteRenderer.color = Board.colorList[color_index];
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DestroyTile() {
        GameObject tmp = Instantiate(destructionPrefab, this.transform.position, Quaternion.identity);
        tmp.GetComponent<SpriteRenderer>().color = Board.colorList[this.color_index];

        this.SetTileType(Tile.TileType.Empty);
        this.SetColor(Board.WHITE_INDEX);
        this.is_floating = false;

    }

    public TileType GetTileType() {
        return tile_type;
    }
    public void SetTileType(TileType tile_type) {
        this.tile_type = tile_type;
    }

    public void SetIsPlayer(bool is_player) {
        if(is_player) {
            this.tile_type = TileType.Player;
            this.SetColor(Board.BLACK_INDEX);
        }
        else {
            this.tile_type = TileType.Empty;
            this.SetColor(Board.WHITE_INDEX);
        }
    }

    public bool GetIsFloating() {
        return this.is_floating;
    }
    public void SetIsFloating(bool is_floating) {
        this.is_floating = is_floating;
    }
}
