using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Grid<TGridObject> {

    public const int HEAT_MAP_MAX_VALUE = 100;
    public const int HEAT_MAP_MIN_VALUE = 0;

    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridArray;
    private Vector3 originPosition;
    private Vector3 modelCentreOffset;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject){
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;


        gridArray = new TGridObject[width, height];
        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                gridArray[x, y] =  createGridObject(this, x, y);
            }
        }
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y){
        return new Vector3(x, 0 ,y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y){
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).z/ cellSize);
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y, Vector3 modelCentreOffset){
        x = Mathf.FloorToInt(((worldPosition - originPosition).x + modelCentreOffset.x) / cellSize);
        y = Mathf.FloorToInt(((worldPosition - originPosition).z + modelCentreOffset.z)/ cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value){
        if (x >= 0 && y >= 0 && x < width && y < height){
            gridArray[x, y] = value;
            if (OnGridValueChanged != null)  {
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridValueChanged != null)  {
            OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value){
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }


    public TGridObject GetGridObject(int x, int y){
        if (x >= 0 && y >= 0 && x < width && y < height){
            return gridArray[x,y];
        } else {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 wordPosition){
        int x, y;
        GetXY(wordPosition, out x, out y);
        return GetGridObject(x, y);
    }
}
