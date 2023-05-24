using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*IMPORTANTEEEE _matrix[5 *h+5*/

public class Matrix<T> : IEnumerable<T>
{
    T[] _matrix;

    public Matrix(int width, int height)
    {
        Capacity = width * height;
        Width = width;
        Height = height;

        _matrix = new T[Width * Height];
    }

    public Matrix(T[,] copyFrom)
    {
        Width = copyFrom.GetLength(0);
        Height = copyFrom.GetLength(1);
        Capacity = Width * Height;

        _matrix = copyFrom.Cast<T>().ToArray();
    }


    public Matrix<T> Clone() {
        Matrix<T> aux = new Matrix<T>(Width, Height);

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                aux[i, j] = _matrix[i + j * Height];
            }
        }

        return aux;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item) {
        for (int i = x0; i < x1; i++)
        {
            for (int j = y0; j < y1; j++)
            {
                _matrix[i + j * Height] = item;
            }
        }
    }

    //Todos los parametros son INCLUYENTES
    public List<T> GetRange(int x0, int y0, int x1, int y1) {
        List<T> newList = new List<T>();

        for (int i = x0; i < x1; i++)
        {
            for (int j = y0; j < y1; j++)
            {
                newList.Add(_matrix[i + j * Height]);
            }
        }

        return newList;
	}


    public T this[int x, int y] {
        get
        {
            //IMPLEMENTAR
            if (x > Width || y > Height)
                return default;
            else
                return _matrix[x+ y * Height];

        }
        set
        {
            //IMPLEMENTAR
            if (x > Width || y > Height)
                return;
            else
                _matrix[x + y * Height] = value;
        }
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Capacity { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        int x = 0;
        int y = 0;

        while (x < Width)
        {
            y = 0;

            while (y < Height)
            {
                yield return _matrix[x + y*Height];
                y++;
            }

            x++;
        }
    }

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
