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
        // Request the home page for the book, plus its path relationships
        StartCoroutine(ScalarAPI.LoadNode("index", HandleSuccess, HandleError, 1, false, "path"));
    }

    public void HandleSuccess(JSONNode json)
    {
        Debug.Log("Received Scalar data");

        // Get the home page for the book
        ScalarNode indexPage = ScalarAPI.GetNode("index");

        // Get the path children of the book's home page
        Debug.Log(indexPage);
        Debug.Log(indexPage.GetRelatedNodes("path", "outgoing"));
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
