# scalar-for-unity
A bridge between Scalar and Unity.
## In Memoriam
The original impetus for the scalar-for-unity package came from the late [Angel David Nieves](https://cssh.northeastern.edu/faculty/angel-david-nieves/), a brilliant scholar in the digital humanities and a longtime supporter of Scalar, whose spatial humanities project [Apartheid Heritages](https://apartheidheritagesproject.org/) made it possible.
## Getting Started
For an overview of working with Unity in Scalar, including important hosting requirements, see [Working with Unity Scenes](https://scalar.usc.edu/works/guide2/working-with-unity-scenes) in the Scalar User's Guide.
### Import the package
Import the package into your Unity project. Inside are a couple of demo scenes: **_ScalarInUnity** loads JSON data from the Scalar User’s Guide, and **_UnityInScalar** is a project which can be built and annotated as a media element within a Scalar book.
## Accessing the ScalarAPI from Unity
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
## Setting up a Unity scene to be annotated in Scalar
### Working with the ScalarCamera
Add the `ScalarAPI` and `ScalarCamera` prefabs to your scene. If you're planning to make calls from the scene to a specific book, fill in the Book URL prefix on the ScalarAPI component with the URL of your book (`https://scalar.usc.edu/works/guide2` is the URL to the Scalar User’s Guide, for example).

When built as a WebGL project using the included Scalar template, and imported into Scalar as a media item, the `ScalarCamera` component in your scene will receive camera position and orientation data when annotations are selected in Scalar. Scalar's annotation editor will also be able to query the scene for the current position and orientation of the camera.

The ScalarCamera prefab also includes a FreeCam component that allows WASD navigation in the 3D scene, and mouselook on right-click and drag.

### ScalarCamera events
You can register your Unity components to receive several events from ScalarCamera that can enhance the integration.

The **Annotation Selected Externally** event is called whenever the user selects an annotation in Scalar. Components registered to this event will receive JSON with the transform parameters of the selected annotation.

The **Annotations Updated Externally** event is called when an annotation is edited in Scalar, to give the Unity scene a chance to update accordingly. No data is passed to components registered to this event.

The **Message Received** event is called when an annotation with `dcterms:abstract` metadata is selected in Scalar. The string contained in `dcterms:abstract` will be parsed as JSON and then sent to components registered to this event.

### Building your project
The package contains WebGL templates that must be used when building your project -- these include the JavaScript functions that allow Scalar and Unity to communicate. 

You can choose the template you want to use in Project Settings > Player > Resolution and Presentation. Use **ScalarTemplate** for Unity versions prior to 2020, and **ScalarTemplate2020** for Unity versions 2020 and above. 

NOTE: If the server where your project is hosted does not support gzip, be sure to set Project Settings > Player > Publishing Settings > Compression Format to Disabled.

## ScalarAPI
### Public Static Methods
**LoadNode**(uriSegment, [successCallback], [errorCallback], [depth], [references], [relation], [start], [results], [provenance], [allVersions])

Calls the Scalar API data about the specified Scalar node in the current book, converting the results into ScalarNode objects stored in the ScalarAPI object.

**GetNode**(uriSegment)

Returns the ScalarNode in the current book identified by the specified URI or URI segment (also known as the “slug”), or null if no matching ScalarNode can be found.

**RemoveNodes**()

Removes all nodes from this instance of the API.

## ScalarNode
### Public Methods
**GetRelatedNodes**(type, direction, [includeNonPages], [sort])

Returns a List of ScalarNode objects related to the node by the specified criteria. For the type parameter, pass in a Scalar type like `path`, `tag`, `annotation`, `comment`, or `referee` (for references to media from a page). For the direction parameter, pass in either `incoming` or `outgoing` depending on the directionality of the relationship (relationships are typically outgoing from the named item; for example, a path has outgoing relationships to the pages and media it contains). 

To get all of the annotations on a piece of media, you would call `mediaNode.GetRelatedNodes('annotation', 'incoming')`.

**GetRelations**(type, direction, [includeNonPages], [sort])

Returns a List of ScalarRelation objects related to the node by the specified criteria. See `GetRelatedNodes` above for details on the usage of the type and direction parameters.

**GetDisplayTitle**()

Returns the title of the node, formatted for display.

**GetSortTitle**()

Returns the title of the node, formatted for alphabetical sorting (i.e. with initial non-alphabetic components like the word “the” removed). Normally used for internal processing of arrays of nodes.

**GetAbsoluteThumbnailURL**()

Returns the absolute URL to the node's thumbnail (if one exists).

## ScalarVersion
Represents a single version of a Scalar node.

## ScalarRelation
Represents (in most cases) a relationship between two Scalar nodes. Use the ScalarRelations `properties` property to access the annotation's position and/or dimensions, based on its type.
