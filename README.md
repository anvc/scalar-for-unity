# scalar-for-unity
A bridge between Scalar and Unity. Not for public release at this time.
## Getting Started
### Import the package
Import the package into your Unity project. Inside are a couple of demo scenes: **_ScalarInUnity** loads JSON data from the Scalar User’s Guide, and **_UnityInScalar** is a project which can be built and annotated as a media element within a Scalar book.
### Specify the URL of the book to work with
Add the `ScalarAPI` component to your scene, and fill in the Book URL Prefix for your book (`https://scalar.usc.edu/works/guide2` is the URL to the Scalar User’s Guide, for example).
### Load data
This package turns the JSON returned by Scalar's API into native C# objects which can be accessed in Unity. This process is triggered by the `LoadNode` method, which targets specific content in a Scalar book, since loading an entire book is a heavy and typically unnecessary call.

Here’s some code from the sample scene, that loads the book’s home page and its path contents into memory, and then outputs their resulting C# objects to the console:

```csharp
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
```
## ScalarAPI
### Public Static Methods
**LoadNode**(uriSegment, [successCallback], [errorCallback], [depth], [references], [relation], [start], [results], [provenance], [allVersions])

Calls the Scalar API data about the specified Scalar node in the current book, converting the results into ScalarNode objects stored in the ScalarAPI object.

**GetNode**(uriSegment)

Returns the ScalarNode in the current book identified by the specified URI or URI segment (also known as the “slug”), or null if no matching ScalarNode can be found.


## ScalarNode
### Public Methods
**GetRelatedNodes**(type, direction, [includeNonPages], [sort])

Returns an array of ScalarNode objects related to the node by the specified criteria.

**GetDisplayTitle**()

Returns the title of the node, formatted for display.

**GetSortTitle**()

Returns the title of the node, formatted for alphabetical sorting (i.e. with initial non-alphabetic components like the word “the” removed). Normally used for internal processing of arrays of nodes.

## ScalarVersion
## ScalarRelation
