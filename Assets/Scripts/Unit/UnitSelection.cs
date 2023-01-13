using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 draggingPosition;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            draggingPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void SelectUnitInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            draggingPosition,
            Input.mousePosition
            );

        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");
        bool inBounds;

        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(Camera.main.WorldToViewportPoint(unit.transform.position));

            if (inBounds)
            {
                unit.GetComponent<UnitManager>().Select();
            }
            else
            {
                unit.GetComponent<UnitManager>().DeSelect();
            }
        }

    }


    void OnGUI()
    {
        if (isDragging)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(draggingPosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }
}
