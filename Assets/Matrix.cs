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
        Width = width;
        Height = height;

        _matrix = new T[Width * Height];
    }

    public Matrix(T[,] copyFrom)
    {
        Width = copyFrom.GetLength(0);
        Height = copyFrom.GetLength(1);
        _matrix = copyFrom.Cast<T>().ToArray();
    }


    public Matrix<T> Clone() {
        Matrix<T> aux = new Matrix<T>(Width, Height);
        aux._matrix = _matrix;
        return aux;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item) {
        for (int i = x0; i < x1; i++)
        {
            for (int j = y0; j < y1; j++)
            {
                _matrix[i* Height + j] = item;
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
                newList.Add(_matrix[i * Height + j]);
            }
        }

        return newList;
	}


    public T this[int x, int y] {
		get
        {
            return _matrix[x*Height+y];
		}
		set {
            _matrix[x * Height + y] = value;
		}
	}

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Capacity { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _matrix)
        {
            yield return item;
        }
    }

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
