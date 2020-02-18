using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ANVC.Scalar;
using SimpleJSON;

public class AccessScalarBook : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ScalarAPI.LoadNode("index", HandleSuccess, HandleError, 1, false, "path"));
    }

    public void HandleSuccess(JSONNode json)
    {
        Debug.Log("Received Scalar data");
    }

    public void HandleError(string error)
    {
        Debug.Log(error);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
