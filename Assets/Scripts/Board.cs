using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] int row;
    [SerializeField] int column;
    [SerializeField] float center_distance;
    [SerializeField] GameObject squarePrefab;
    [SerializeField] GameObject player;
    private Tile[,] board;
    float elapsedTime;
    [SerializeField] float game_frame_time = 0.01f;
    private int frame_count = 0;
    [SerializeField] int num_colors;
    public static Color[] colorList = {Color.black, Color.white, Color.blue, Color.red, Color.green, Color.yellow, Color.magenta};
    public const int BLACK_INDEX = 0;
    public const int WHITE_INDEX = 1;
    public int score=0;
    

    void ShiftBlocksDown() {
        mainmenu obj=FindObjectOfType<mainmenu>();
        for(int i = 1; i <=row-2; i++) {
            for(int j = 0; j < column; j++) {
                board[j, i].SetTileType(board[j, i+1].GetTileType());
                board[j, i].SetColor(board[j, i+1].GetColor());
            }
        }
        for(int i=0; i<column; i++) {
            if(board[i,1].GetTileType()==Tile.TileType.Block) {
                FindObjectOfType<GameManager>().EndGame();
                Destroy(FindObjectOfType<Player>());
                obj.PlayGame();
            }
        }
    }

    public void GenerateRow() {
        ShiftBlocksDown();
        for(int i = column-1; i >= 0; i--) {
            int random_color = UnityEngine.Random.Range(2, num_colors+2);

            board[i, row-1].SetColor(random_color);
            board[i, row-1].SetTileType(Tile.TileType.Block);
        }

    }

    // checks which blocks are floating out of the list of possible floating blocks
    void DestroyFloatingBlocks() {
        for(int i = 1; i < row; i++) {
            for(int j = 0; j < column; j++) {
                if(board[j, i].GetTileType() == Tile.TileType.Block) board[j, i].SetIsFloating(true);
                else board[j, i].SetIsFloating(false);
            }
        }

        Queue<Tuple<int, int> > nonFloating = new Queue< Tuple<int, int>>();

        // the base row blocks should be non floating
        for(int j = 0; j < column; j++) {
            if(board[j, row-1].GetIsFloating()) {       
                board[j, row-1].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(row-1, j));
            }
        }

        // the blocks hugging the vertical walls should be non floating
        for(int i = row-2; i >= 1; i--) {
            if(board[0, i].GetIsFloating()) {       
                board[0, i].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(i, 0));
            }
            if(board[column-1, i].GetIsFloating()) { 
                board[column-1, i].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(i, column-1));
            }
        }
        // find all the non floating tiles
        while(nonFloating.Count != 0) {
            Tuple<int, int> tmp = nonFloating.Peek();
            nonFloating.Dequeue();
            int r = tmp.Item1;
            int c = tmp.Item2;

            if(IsValidTile(r+1, c) && board[c, r+1].GetIsFloating()) {
                board[c, r+1].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(r+1, c));
            }
            if(IsValidTile(r-1, c) && board[c, r-1].GetIsFloating()) {
                board[c, r-1].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(r-1, c));
            }
            if(IsValidTile(r, c+1) && board[c+1, r].GetIsFloating()) {
                board[c+1, r].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(r, c+1));
            }
            if(IsValidTile(r, c-1) && board[c-1, r].GetIsFloating()) {
                board[c-1, r].SetIsFloating(false);
                nonFloating.Enqueue(new Tuple<int, int>(r, c-1));
            }
        }        
        
        // finally remove all the floating tiles
        for(int i = row-2; i >= 1; i--) {
            for(int j = 0; j < column; j++) {
                if(board[j, i].GetIsFloating()){
                    board[j, i].DestroyTile();
                } 
            }
        }
    }

    bool IsValidTile(int r, int c) {
        return (0 <= r) && (r <= (row-1)) && (0 <= c) && (c <= (column-1));
    }

    // only valid tiles are given as input
    bool CanDestroy(int r, int c, int col) {
        return board[c, r].GetTileType() == Tile.TileType.Block && (board[c, r].GetColor() == col);
    }

    void DestroyBlocks(int r, int c, int col) {
        int counter=0;
        Queue<Tuple<int, int> > bfs = new Queue<Tuple<int, int>>();
        bfs.Enqueue(new Tuple<int, int>(r, c));
        bool at_least_one = CanDestroy(r+1, c, col) || 
                            CanDestroy(r, c+1, col) ||
                            CanDestroy(r-1, c, col) ||
                            CanDestroy(r, c-1, col);

        if(at_least_one) {
            board[c, r].DestroyTile();
            while(bfs.Count != 0) {
                Tuple<int, int> tmp = bfs.Peek();
                bfs.Dequeue();

                if(IsValidTile(tmp.Item1+1, tmp.Item2)) {
                    if(CanDestroy(tmp.Item1+1, tmp.Item2, col)) {
                        board[tmp.Item2, tmp.Item1+1].DestroyTile();
                        bfs.Enqueue(new Tuple<int, int>(tmp.Item1+1, tmp.Item2));counter++;
                    }
                }

                if(IsValidTile(tmp.Item1, tmp.Item2+1)) {
                    if(CanDestroy(tmp.Item1, tmp.Item2+1, col)) {
                        board[tmp.Item2+1, tmp.Item1].DestroyTile();
                        bfs.Enqueue(new Tuple<int, int>(tmp.Item1, tmp.Item2+1));counter++;
                    }
                }

                if(IsValidTile(tmp.Item1-1, tmp.Item2)) {
                    if(CanDestroy(tmp.Item1-1, tmp.Item2, col)) {
                        board[tmp.Item2, tmp.Item1-1].DestroyTile();
                        bfs.Enqueue(new Tuple<int, int>(tmp.Item1-1, tmp.Item2));counter++;
                    }
                }

                if(IsValidTile(tmp.Item1, tmp.Item2-1)) {
                    if(CanDestroy(tmp.Item1, tmp.Item2-1, col)) {
                        board[tmp.Item2-1, tmp.Item1].DestroyTile();
                        bfs.Enqueue(new Tuple<int, int>(tmp.Item1, tmp.Item2-1));counter++;
                    }
                }
            }
            score+=10*(counter);
            Debug.Log(score);
            DestroyFloatingBlocks();
        }
        
    }

    void UpdateBullets() {
        for(int i = row-1; i >= 1; i--) {
            for(int j = 0; j < column; j++) {
                if(board[j, i].GetTileType() != Tile.TileType.Bullet) continue;

                if(i == row - 1) {
                    board[j, i].SetTileType(Tile.TileType.Block);
                }
                else {
                    switch(board[j, i+1].GetTileType()) {
                        case Tile.TileType.Empty :
                        board[j, i+1].SetTileType(Tile.TileType.Bullet);
                        board[j, i+1].SetColor(board[j, i].GetColor());
                        board[j, i].SetTileType(Tile.TileType.Empty);
                        board[j, i].SetColor(WHITE_INDEX);
                        break;

                        case Tile.TileType.Block : 
                        board[j, i].SetTileType(Tile.TileType.Block);
                        DestroyBlocks(i, j, board[j, i].GetColor());
                        break;

                        default :

                        break;
                    }
                }
            }
        }
    }

    public void CreateBullet(int r, int c, int bullet_color) {
        if(board[c, r].GetTileType() == Tile.TileType.Empty) {
            board[c, r].SetTileType(Tile.TileType.Bullet);
            board[c, r].SetColor(bullet_color);
        }
    }

    void GenerateBoard(int r, int c) {
        board = new Tile[c, r];

        for(int i = 0; i < c; i++) {
            for(int j = 0; j < r; j++) {
                board[i, j] = Instantiate(squarePrefab, new Vector3(i*center_distance, j*center_distance, 0), Quaternion.identity).GetComponent<Tile>();
                board[i, j].SetIsFloating(false);
            }
        }

        this.UpdatePlayer(0, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard(row, column);
        GenerateRow();
        
    }

    public void UpdatePlayer(int old_player_location, int player_location) {
        board[old_player_location, 0].GetComponent<Tile>().SetIsPlayer(false);
        board[player_location, 0].GetComponent<Tile>().SetIsPlayer(true);

        player.transform.position = new Vector3(player_location * center_distance, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > game_frame_time) {
            elapsedTime = 0;
            frame_count++;
            if((frame_count % 5) == 0) {
                UpdateBullets();
            }
            /*if((frame_count % 150) == 0) {
                GenerateRow();
            }
            */
        }
    }

    public int GetRow() {
        return row;
    }

    public int GetColumn() {
        return column;
    }

    public int GetNumColors() {
        return num_colors;
    }
}
