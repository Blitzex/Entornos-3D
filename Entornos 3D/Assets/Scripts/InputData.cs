using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct InputData
{
    //movimiento
    public float Horizontal;
    public float Vertical;

    //mouse rotacion
    public float verticalMouse;
    public float horizontalMouse;

    //mov extras
    public bool dash;
    public bool jump;

    public void getInput()
    {
        //movement
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");

        //mouse rotation
        verticalMouse = Input.GetAxis("Mouse Y");
        horizontalMouse = Input.GetAxis("Mouse X");

        //Extra movemnts
        dash = Input.GetButton("Dash");
        jump = Input.GetButtonDown("Jump");
    }
}
