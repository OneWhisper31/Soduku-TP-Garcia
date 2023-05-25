using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class Sudoku : MonoBehaviour {
	public Cell prefabCell;
	public Canvas canvas;
	public Text feedback;
	public float stepDuration = 0.05f;
	[Range(1, 82)]public int difficulty = 40;

	Matrix<Cell> _board;
	Matrix<int> _createdMatrix;
    List<int> posibles = new List<int>();
	int _smallSide;
	int _bigSide;
    string memory = "";
    string canSolve = "";
    bool canPlayMusic = false;
    List<int> nums = new List<int>();



    float r = 1.0594f;
    float frequency = 440;
    float gain = 0.5f;
    float increment;
    float phase;
    float samplingF = 48000;

    bool hasSolved;
    List<Matrix<int>> solutionList = new List<Matrix<int>>();


    void Start()
    {
        long mem = System.GC.GetTotalMemory(true);
        feedback.text = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        memory = feedback.text;
        _smallSide = 3;
        _bigSide = _smallSide * _smallSide;
        frequency = frequency * Mathf.Pow(r, 2);
        CreateEmptyBoard();
        ClearBoard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SolvedSudoku();
        else if (Input.GetKeyDown(KeyCode.C))
            CreateSudoku();
        else if (Input.GetKeyDown(KeyCode.A))
            StartCoroutine(ShowSequence(solutionList));
    }


    void ClearBoard() {
		_createdMatrix = new Matrix<int>(_bigSide, _bigSide);
		foreach(var cell in _board) {
			cell.number = 0;
			cell.locked = cell.invalid = false;
		}
	}

	void CreateEmptyBoard() {//intantiate board
		float spacing = 68f;
		float startX = -spacing * 4f;
		float startY = spacing * 4f;

		_board = new Matrix<Cell>(_bigSide, _bigSide);
		for(int x = 0; x<_board.Width; x++) {
			for(int y = 0; y<_board.Height; y++) {
                var cell = _board[x, y] = Instantiate(prefabCell);
				cell.transform.SetParent(canvas.transform, false);
				cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
			}
		}
	}




    int watchdog = 0;
    bool RecuSolve(Matrix<int> matrixParent, int x, int y, int protectMaxDepth, List<Matrix<int>> solution)
    {
        if (protectMaxDepth <= 0) return false;

        protectMaxDepth--;

        var newX = x;
        var newY = y;

        if (x >= _board.Width)
        {
            newX = x = 0;
            newY = y + 1;
            y = newY;
            Debug.Log("y+");
        }

        if (newY >= _board.Height) return true;

        if (_board[x, y].locked)
        {
            newX = x + 1;
            Debug.Log("locked");
            return RecuSolve(matrixParent, newX, newY, protectMaxDepth, solution);
        }

        for (int i = 1; i < 10; i++)
        {
            if (CanPlaceValue(matrixParent, i, x, y))
            {
                matrixParent[x, y] = i;
                Debug.Log($"value {i} in: {x} & {y}");

                Matrix<int> cloned = matrixParent.Clone();

                solution.Add(cloned);

                newX = x + 1;

                if (RecuSolve(matrixParent, newX, newY, protectMaxDepth, solution)) return true;
                else
                {
                    Debug.Log($"clear {i} in: {x} y {y}");
                    matrixParent[x, y] = Cell.EMPTY;
                }
            }
        }

        return false;
    }



    void OnAudioFilterRead(float[] array, int channels)
    {
        if(canPlayMusic)
        {
            increment = frequency * Mathf.PI / samplingF;
            for (int i = 0; i < array.Length; i++)
            {
                phase = phase + increment;
                array[i] = (float)(gain * Mathf.Sin((float)phase));
            }
        }
        
    }
    void changeFreq(int num)
    {
        frequency = 440 + num * 80;
    }

    IEnumerator ShowSequence(List<Matrix<int>> seq)
    {
        if (!hasSolved)//if hasn't solved search answer
        {
            var solution = new List<Matrix<int>>();
            watchdog = 200;
            var result = RecuSolve(_createdMatrix, 0, 0, watchdog, solution);

            if (!result) yield break;
            seq = solution.Select(x => x).ToList();

            Output(result);//output text
        }

        int counterSeq = 0;
        int totalSteps = seq.Count;
        while (seq.Count != 0)
        {//go step by step
            TranslateAllValues(seq[0]);
            seq.RemoveAt(0);
            counterSeq++;
            feedback.text = "Steps: " + counterSeq + "/" + totalSteps + " - " + memory + " - " + canSolve;
            yield return new WaitForSeconds(stepDuration);
        }

        //output
        feedback.text = "Steps: " + counterSeq + "/" + counterSeq + " - " + memory + " - " + canSolve;
    }
    void SolvedSudoku()
    {
        StopAllCoroutines();        
        bool result = SolveSudoku();//resolves sudoku
        TranslateAllValues(_createdMatrix);//traslate
        Output(result);//output text
    }

    void CreateSudoku()
    {
        StopAllCoroutines();
        canPlayMusic = false;
        ClearBoard();

        //generates random line and lock the numbers to resolve
        _createdMatrix = GenerateValidLine(_createdMatrix, 0, Random.Range(0, _createdMatrix.Height));
        LockCellsWithNumbers(_createdMatrix);

        //resolve sudoku
        bool result = SolveSudoku();
        _createdMatrix = result? _createdMatrix
        : new Matrix<int>(Tests.validBoards[Tests.validBoards.Length - 1]);//if invalid, use prefab boards

        //unlocks all the numbers
        foreach (Cell item in _board)
        {
            item.locked = false;
        }
        
        LockRandomCells();//randomize locks base on difficulty
        ClearUnlocked(_createdMatrix);//clear unlocked cells
        TranslateAllValues(_createdMatrix);//translate

        
        Output(result);//output text

    }
    bool SolveSudoku()
    {

        var solution = new List<Matrix<int>>();
        watchdog = 200;

        //resolves sudoku
        bool result = RecuSolve(_createdMatrix, 0, 0, watchdog, solution);
        solutionList = solution.Select(x => x).ToList();

        return result;
    }

    void Output(bool result)
    {
        //output
        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";
        feedback.text = "Steps: " + solutionList.Count + "/" + solutionList.Count + " - " + memory + " - " + canSolve;
    }

    Matrix<int> GenerateValidLine(Matrix<int> mtx, int x, int y)
	{
		int[]aux = new int[mtx.Width];
		for (int i = 0; i < mtx.Width; i++) 
		{
			aux [i] = i + 1;
		}
		int numAux = 0;
		for (int j = 0; j < aux.Length; j++) 
		{
			int r = 1 + Random.Range(j,aux.Length);
			numAux = aux [r-1];
			aux [r-1] = aux [j];
			aux [j] = numAux;
		}
		for (int k = 0; k < aux.Length; k++) 
		{
			mtx [k, y] = aux [k];
		}
        return mtx;
	}

	void ClearUnlocked(Matrix<int> mtx)
	{
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					mtx[j,i] = Cell.EMPTY;
			}
		}
	}

	void LockRandomCells()
	{
		List<Vector2> posibles = new List<Vector2> ();
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					posibles.Add (new Vector2(j,i));
			}
		}
		for (int k = 0; k < 82-difficulty; k++) {
			int r = Random.Range (0, posibles.Count);
			_board [(int)posibles [r].x, (int)posibles [r].y].locked = true;
			posibles.RemoveAt (r);
		}
	}
    void LockCellsWithNumbers(Matrix<int> range)
    {
        for (int i = 0; i < _board.Height; i++)
        {
            for (int j = 0; j < _board.Width; j++)
            {
                if (range[i, j] != Cell.EMPTY) _board[i, j].locked = true;
                else _board[i, j].locked = false;
            }
        }
    }

    void TranslateAllValues(Matrix<int> matrix)
    {
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
            }
        }
    }

    void TranslateSpecific(int value, int x, int y)
    {
        _board[x, y].number = value;
    }

    void TranslateRange(int x0, int y0, int xf, int yf)
    {
        for (int x = x0; x < xf; x++)
        {
            for (int y = y0; y < yf; y++)
            {
                _board[x, y].number = _createdMatrix[x, y];
            }
        }
    }
    void CreateNew()
    {
        _createdMatrix = new Matrix<int>(Tests.validBoards[Tests.validBoards.Length - 1]);

        LockRandomCells();
        ClearUnlocked(_createdMatrix);


        TranslateAllValues(_createdMatrix);
    }

    bool CanPlaceValue(Matrix<int> mtx, int value, int x, int y)
    {
        List<int> fila = new List<int>();
        List<int> columna = new List<int>();
        List<int> area = new List<int>();
        List<int> total = new List<int>();

        Vector2 cuadrante = Vector2.zero;

        for (int i = 0; i < mtx.Height; i++)
        {
            for (int j = 0; j < mtx.Width; j++)
            {
                if (i != y && j == x) columna.Add(mtx[j, i]);
                else if(i == y && j != x) fila.Add(mtx[j,i]);
            }
        }



        cuadrante.x = (int)(x / 3);

        if (x < 3)
            cuadrante.x = 0;     
        else if (x < 6)
            cuadrante.x = 3;
        else
            cuadrante.x = 6;

        if (y < 3)
            cuadrante.y = 0;
        else if (y < 6)
            cuadrante.y = 3;
        else
            cuadrante.y = 6;
         
        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + 3, (int)cuadrante.y + 3);
        total.AddRange(fila);
        total.AddRange(columna);
        total.AddRange(area);
        total = FilterZeros(total);

        if (total.Contains(value))
            return false;
        else
            return true;
    }


    List<int> FilterZeros(List<int> list)
    {
        List<int> aux = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != 0) aux.Add(list[i]);
        }
        return aux;
    }
}
