using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HW_Transform : MonoBehaviour
{
    [SerializeField] 
    enum Axis
    {
        X,
        Y,
        Z
    };

    void Start()
    {
        Vector4 point = new Vector4(2, 1, 3, 1);
        Matrix4x4 scale = ScaleMat(1.43f,1.43f, 1.43f);
        Vector4 r1 = scale * point;
        Debug.Log(r1);
        
        Matrix4x4 translate = TranslationMat(4,3,1);
        Vector4 r2 = translate * r1;
        Debug.Log(r2);
        
        Vector4 pointB = new Vector4(3,2,4,1);
        Matrix4x4 translate2 = TranslationMat(-6.86f, -4.43f, -5.29f);
        Vector4 r3 = translate2 * pointB;
        Debug.Log(r3);

        Matrix4x4 rotation = RotationMat(Axis.X, 45);
        Vector4 r4 = rotation * r3;
        Debug.Log(r4);
        
        
        Vector4 pointC = new Vector4(6.86f, 4.43f, 5.29f, 1);
        Matrix4x4 translate3 = TranslationMat(3.66f, 0.63f, 2.63f);
        Vector4 r5 = translate3 * pointC;
        Debug.Log(r5);
    }
    
    //rad of 45 results in 0.7853982
    //cos of .7853982 is 0.7071068
    //sin of .7853982 is 0.7071068
    //-sin of .7853982 is -0.7071068

    //.70 * 2.43 = 1.71
    //.70 * 1.29 = 0.91
    //-.70 * 2.43 = -1.71
    //.70 * 1.29 = 0.91
    
    //-1.71 - 0.91 = -2.62
    //1.71 - 0.91 = 0.80


    public static Matrix4x4 TranslationMat(float tx, float ty, float tz)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[0, 3] = tx;
        matrix[1, 3] = ty;
        matrix[2, 3] = tz;
        return matrix;
    }
    
    public static Matrix4x4 ScaleMat(float sx, float sy, float sz)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[0, 0] = sx;
        matrix[1, 1] = sy;
        matrix[2, 2] = sz;
        return matrix;
    }
    
    private static Matrix4x4 RotationMat(Axis axis, float angle)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        float rad = angle * Mathf.Deg2Rad;
        switch (axis)
        {
            case Axis.X:
                matrix[1, 1] = Mathf.Cos(rad);
                matrix[1, 2] = -Mathf.Sin(rad);
                matrix[2, 1] = Mathf.Sin(rad);
                matrix[2, 2] = Mathf.Cos(rad);
                break;
            case Axis.Y:    
                matrix[0, 0] = Mathf.Cos(rad);
                matrix[0, 2] = Mathf.Sin(rad);
                matrix[2, 0] = -Mathf.Sin(rad);
                matrix[2, 2] = Mathf.Cos(rad);
                break;
            case Axis.Z:
                matrix[0, 0] = Mathf.Cos(rad);
                matrix[0, 1] = -Mathf.Sin(rad);
                matrix[1, 0] = Mathf.Sin(rad);
                matrix[1, 1] = Mathf.Cos(rad);
                break;
        }
        return matrix;
    }
}
