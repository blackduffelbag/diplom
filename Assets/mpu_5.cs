using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mpu_5 : MonoBehaviour
{
    Quaternion originRotation;
    Quaternion next;
    float angleX = 0, angleY = 0, angleZ = 0;
    // Start is called before the first frame update
    void Start()
    {
        originRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, 0f, 20f);
        }

        if (mess.Messages5.Count > 0)
        {

            //string output = conn.RecivedMessages[0].Remove(conn.RecivedMessages[0].Length - 1);
            string[] data = mess.Messages5[0].Split(',');


            if (int.Parse(data[0]) == 5)
            {
                //UnityEngine.Debug.Log("N: " + data[0] + " X: " + data[1] + " Y: " + data[2] + " Z: " + data[3]);
                angleX = float.Parse(data[1]);
                angleY = float.Parse(data[2]);
                angleZ = float.Parse(data[3]);
                Quaternion rot_x = Quaternion.AngleAxis(angleX, new Vector3(1, 0, 0));
                Quaternion rot_y = Quaternion.AngleAxis(angleY, new Vector3(0, 1, 0));
                Quaternion rot_z = Quaternion.AngleAxis(angleZ, new Vector3(0, 0, 1));
                transform.rotation = originRotation * rot_x;
                next = originRotation * rot_x;
                transform.rotation = next * rot_y;
                next = next * rot_y;
                transform.rotation = next * rot_z;
                //transform.rotation = Quaternion.Euler(0, 0, 0);

            }

            mess.Messages5.RemoveAt(0);
        }


    }
}
