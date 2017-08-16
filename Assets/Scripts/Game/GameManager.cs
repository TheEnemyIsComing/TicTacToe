using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Canvas GameOverMess;
    private Text winnerText, winsText, losesText, drawsText;
    private int LastFirstTurn = 0, LastWinner = 0, currentTurn = 0, wins = 0, loses = 0, draws = 0;
    private TileScript[,] tiles = new TileScript[3, 3];
    private bool XTurn = true, playerX = true, gameOver = false, playersTurn = true;

    private void Awake()
    {
        GameOverMess = GameObject.Find("GameOverMessage").GetComponent<Canvas>();
        GameOverMess.enabled = false;
        winnerText = GameOverMess.transform.Find("TextWinner").GetComponent<Text>();
        winsText = GameObject.Find("TextWinsValue").GetComponent<Text>();
        losesText = GameObject.Find("TextLosesValue").GetComponent<Text>();
        drawsText = GameObject.Find("TextDrawsValue").GetComponent<Text>();
    }

    private void Start()
    {
        GenerateField();
        playersTurn = CalcFirstTurn();
    }

    private void Update()
    {
        if (!gameOver)
        {
            if (playersTurn)
            {
                PlayerTurn();
            }
            else
                AITurn();
        }
    }

    private void PlayerTurn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.rigidbody != null && hit.rigidbody.GetComponent<TileScript>() != null && hit.rigidbody.GetComponent<TileScript>().State == TileState.Free)
                {
                    SpawnFigure(hit.rigidbody.GetComponent<TileScript>());
                    currentTurn++;
                    if (CheckWin() != 0)
                    {
                        GameOver(CheckWin());
                    }
                    playersTurn = false;
                }
            }
        }
    }

    private void AITurn()
    {
        if (GameSettings.difficulty == 1)
            AIEasy();
        else if (GameSettings.difficulty == 2)
            AIMedium();
        else
            AIImpossible();
        currentTurn++;
        if (CheckWin() != 0)
        {
            GameOver(CheckWin());
        }
        if (!gameOver)
        playersTurn = true;
    }

    private void AIEasy()
    {
        List<TileScript> freeTiles = new List<TileScript>();
        foreach (TileScript t in tiles)
            if (t.State == TileState.Free)
                freeTiles.Add(t);
        SpawnFigure(freeTiles[Random.Range(0, freeTiles.Count)]);
    }

    private void AIMedium()
    {
        if (currentTurn < 2)
        {
            Vector2 figureCoords;
            do
                figureCoords = AIMediumFirstTurn();
            while (tiles[(int)figureCoords.x, (int)figureCoords.y].State != TileState.Free);
            SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
        }
        else
        {
            if (!CheckAICanWinLoose())
            {
                AIEasy();
            }
        }
    }

    private void AIImpossible()
    {
        //Spawn in corner on first AI turn for X
        if (currentTurn == 0)
        {
            Vector2 figureCoords = AIImpossibleFirstTurn();
            SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
        }
        else if (currentTurn == 1)
        {
            Vector2 figureCoords = new Vector2();
            if (tiles[1, 1].State == TileState.Free)
                figureCoords = new Vector2(1, 1);
            else
            {
                if (AICheckLineOrColumn(true, 0)[0] == 0)
                    figureCoords.x = 0;
                else
                    figureCoords.x = 2;
                if (AICheckLineOrColumn(false, 0)[0] == 0)
                    figureCoords.y = 0;
                else
                    figureCoords.y = 2;
            }
            SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
        }
        //Can't loose
        else if (!CheckAICanWinLoose())
            //Spawn in opposing corner in AI second turn for X
            if (currentTurn == 2)
            {

                Vector2 figureCoords = new Vector2();
                if (AICheckLineOrColumn(true, 0)[0] == 0)
                    figureCoords.x = 0;
                else
                    figureCoords.x = 2;
                if (AICheckLineOrColumn(false, 0)[0] == 0)
                    figureCoords.y = 0;
                else
                    figureCoords.y = 2;
                if (tiles[(int)figureCoords.x, (int)figureCoords.y].State == TileState.Free)
                    SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
                else
                    if (tiles[1, 1].State == TileState.Free)
                    SpawnFigure(tiles[1, 1]);
                else
                {
                    do
                        figureCoords = AIMediumFirstTurn();
                    while (tiles[(int)figureCoords.x, (int)figureCoords.y].State != TileState.Free);
                    SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
                }
            }
            else if (currentTurn == 3)
            {
                Vector2 figureCoords = new Vector2();
                //if no X in corners
                if (AICheckOpposingCorners(TileState.X))
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand < .25f)
                        figureCoords = new Vector2(0, 1);
                    else if (rand < .5f)
                        figureCoords = new Vector2(1, 0);
                    else if (rand < .75f)
                        figureCoords = new Vector2(2, 1);
                    else
                        figureCoords = new Vector2(1, 2);
                }
                else if (tiles[0, 1].State == TileState.X || tiles[1, 0].State == TileState.X)
                    figureCoords = new Vector2(0, 0);
                else if (tiles[1, 2].State == TileState.X || tiles[2, 1].State == TileState.X)
                    figureCoords = new Vector2(2, 2);
                else
                {
                    do
                        figureCoords = AIImpossibleFirstTurn();
                    while (tiles[(int)figureCoords.x, (int)figureCoords.y].State != TileState.Free);
                }
                SpawnFigure(tiles[(int)figureCoords.x, (int)figureCoords.y]);
            }
            else
                AIEasy();
    }

    private Vector2 AIMediumFirstTurn()
    {
        if (Random.Range(0f, 1f) < .2f)
            return (new Vector2(1, 1));
        else
        {
            Vector2 res = AIImpossibleFirstTurn();
            return (res);
        }
    }

    private Vector2 AIImpossibleFirstTurn()
    {
        int x, y;
        if (Random.Range(0f, 1f) > .5f)
            x = 2;
        else
            x = 0;
        if (Random.Range(0f, 1f) > .5f)
            y = 2;
        else
            y = 0;
        return (new Vector2(x, y));
    }

    private bool CheckAICanWinLoose()
    {
        if (playerX)
        {
            if (AICheckTwoFigures(1))
                return (true);
            if (AICheckTwoFigures(0))
                return (true);
        }
        else
        {
            if (AICheckTwoFigures(0))
                return (true);
            if (AICheckTwoFigures(1))
                return (true);
        }
        return (false);
    }

    private bool AICheckOpposingCorners(TileState figure)
    {
        if (tiles[0, 0].State == figure && tiles[2, 2].State == figure)
            return (true);
        if (tiles[0, 2].State == figure && tiles[2, 0].State == figure)
            return (true);
        return (false);
    }

    private int[] AICheckLineOrColumn(bool line, int coord)
    {
        int[] res = new int[] { 0, 0, 0 };
        if (line)
            for (int i = 0; i < 3; i++)
                if (tiles[coord, i].State == TileState.X)
                    res[0]++;
                else if (tiles[coord, i].State == TileState.O)
                    res[1]++;
                else
                    res[2]++;
        else
            for (int i = 0; i < 3; i++)
                if (tiles[i, coord].State == TileState.X)
                    res[0]++;
                else if (tiles[i, coord].State == TileState.O)
                    res[1]++;
                else
                    res[2]++;
        return (res);
    }

    private bool AICheckTwoFigures(int figure)
    {
        if (figure < 0 || figure > 1)
            return (false);
        //Check lines
        for (int i = 0; i < 3; i++)
        {
            int[] line = AICheckLineOrColumn(true, i);
            if (line[figure] == 2 && line[2] == 1)
                for (int j = 0; j < 3; j++)
                    if (tiles[i, j].State == TileState.Free)
                    {
                        SpawnFigure(tiles[i, j]);
                        return (true);
                    }
        }
        //Check columns
        for (int i = 0; i < 3; i++)
        {
            int[] column = AICheckLineOrColumn(false, i);
            if (column[figure] == 2 && column[2] == 1)
                for (int j = 0; j < 3; j++)
                    if (tiles[j, i].State == TileState.Free)
                    {
                        SpawnFigure(tiles[j, i]);
                        return (true);
                    }
        }
        //Check diagonals
        int[] diag = new int[] { 0, 0, 0 };
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i, i].State == TileState.X)
                diag[0]++;
            else if (tiles[i, i].State == TileState.O)
                diag[1]++;
            else
                diag[2]++;
        }
        if (diag[figure] == 2 && diag[2] == 1)
            for (int i = 0; i < 3; i++)
                if (tiles[i, i].State == TileState.Free)
                {
                    SpawnFigure(tiles[i, i]);
                    return (true);
                }
        diag = new int[] { 0, 0, 0 };
        for (int i = 0; i < 3; i++)
        {
            if (tiles[2-i, i].State == TileState.X)
                diag[0]++;
            else if (tiles[2-i, i].State == TileState.O)
                diag[1]++;
            else
                diag[2]++;
        }
        if (diag[figure] == 2 && diag[2] == 1)
            for (int i = 0; i < 3; i++)
                if (tiles[2 - i, i].State == TileState.Free)
                {
                    SpawnFigure(tiles[2 - i, i]);
                    return (true);
                }
        return (false);
    }

    private void SpawnFigure(TileScript t)
    {
        if (XTurn)
            t.State = TileState.X;
        else
            t.State = TileState.O;
        XTurn = !XTurn;
    }

    private void GenerateField()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                GameObject t = Instantiate(Resources.Load("Prefabs/Tile")) as GameObject;
                tiles[i, j] = t.GetComponent<TileScript>();
                tiles[i, j].transform.position = new Vector3(i * 4 - 4, 0, j * 4 - 4);
                tiles[i, j].GetComponent<TileScript>().coords = new Vector2(i, j);
            }
    }

    private void ClearField()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                tiles[i, j].State = TileState.Free;
    }

    private bool CalcFirstTurn()
    {
        int player;
        if (LastWinner == 0)
        {
            if (LastFirstTurn == 0)
                player = Random.Range(1, 3);
            else
            {
                if (LastFirstTurn == 1)
                    player = 2;
                else
                    player = 1;
            }
        }
        else
        {
            player = LastWinner;
        }
        LastFirstTurn = player;
        if (player == 1)
        {
            playerX = true;
            return (true);
        }
        else
        {
            playerX = false;
            return (false);
        }
    }

    private int CheckWin()
    {
        //check lines
        for (int i = 0; i < 3; i++)
        {
            int[] line = new int[] { 0, 0 };
            for (int j = 0; j < 3; j++)
            {
                if (tiles[i, j].State == TileState.X)
                    line[0]++;
                else if (tiles[i, j].State == TileState.O)
                    line[1]++;
                else
                    break;
            }
            for (int l = 0; l < line.Length; l++)
            {
                if (line[l] == 3)
                {
                    for (int j = 0; j < 3; j++)
                        tiles[i, j].SetWinColor();
                    return (l + 1);
                }
            }
        }
        //check columns
        for (int i = 0; i < 3; i++)
        {
            int[] line = new int[] { 0, 0 };
            for (int j = 0; j < 3; j++)
            {
                if (tiles[j, i].State == TileState.X)
                    line[0]++;
                else if (tiles[j, i].State == TileState.O)
                    line[1]++;
                else
                    break;
            }
            for (int l = 0; l < line.Length; l++)
            {
                if (line[l] == 3)
                {
                    for (int j = 0; j < 3; j++)
                        tiles[j, i].SetWinColor();
                    return (l + 1);
                }
            }
        }
        //check diagonals
        int[] diag = new int[] { 0, 0 };
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i, i].State == TileState.X)
                diag[0]++;
            else if (tiles[i, i].State == TileState.O)
                diag[1]++;
            else
                break;
        }
        for (int i = 0; i < diag.Length; i++)
        {
            if (diag[i] == 3)
            {
                for (int j = 0; j < 3; j++)
                    tiles[j, j].SetWinColor();
                return (i + 1);
            }
        }
        diag = new int[] { 0, 0 };
        for (int i = 0; i < 3; i++)
        {
            if (tiles[2 - i, i].State == TileState.X)
                diag[0]++;
            else if (tiles[2 - i, i].State == TileState.O)
                diag[1]++;
            else
                break;
        }
        for (int i = 0; i < diag.Length; i++)
        {
            if (diag[i] == 3)
            {
                for (int j = 0; j < 3; j++)
                    tiles[2 - j, j].SetWinColor();
                return (i + 1);
            }
        }
        //Check draw
        int winner = 3;
        foreach (TileScript t in tiles)
            if (t.State == TileState.Free)
                return (0);
        return (winner);
    }

    private void GameOver(int winner)
    {
        gameOver = true;
        currentTurn = 0;
        GameOverMess.enabled = true;
        if (winner == 3)
        {
            winnerText.text = "Draw";
            draws++;
            drawsText.text = draws.ToString();
            LastWinner = 0;
        }
        else if (playersTurn && winner != 3)
        {
            winnerText.text = "You won";
            wins++;
            winsText.text = wins.ToString();
            LastWinner = 1;
        }
        else
        {
            winnerText.text = "You lost";
            loses++;
            losesText.text = loses.ToString();
            LastWinner = 2;
        }
    }

    public void PlayAgainButton()
    {
        XTurn = true;
        ClearField();
        playersTurn = CalcFirstTurn();
        gameOver = false;
        GameOverMess.enabled = false;
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("menu");
    }
}