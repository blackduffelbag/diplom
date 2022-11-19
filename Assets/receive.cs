using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class receive : MonoBehaviour
{

    TCPConnect conn;
   
    void Start()
    {
        conn = new TCPConnect("192.168.1.107", 5005);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(0f, 20f, 0f);
            conn.CloseClient();
        }

        if (conn.RecivedMessages.Count > 0)
        {

            string output = conn.RecivedMessages[0].Remove(conn.RecivedMessages[0].Length - 1);

            int n = int.Parse(output[0].ToString());
            if (n==0)
                mess.Messages0.Add(output);
            if (n==1)
                mess.Messages1.Add(output);
            if (n==2)
                mess.Messages2.Add(output);
            if (n==3)
            {
                mess.Messages3.Add(output);
                //Debug.Log(output);
            }
            if (n==4)
            {
                mess.Messages4.Add(output);
                //Debug.Log(output);
            }
            if (n==5)
                mess.Messages5.Add(output);

            if (Input.GetKey(KeyCode.C))
            {
                //transform.Rotate(20f, 20f, 20f);
                conn.CloseClient();
            }

            conn.RecivedMessages.RemoveAt(0);
        }

    }
}
