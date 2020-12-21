using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

namespace ANVC.Scalar
{
    public class ScalarAPI : MonoBehaviour
    {
        public static string urlPrefix;
        public static List<ScalarNode> nodes;
        public static Dictionary<string, ScalarNode> nodesByURL;
        public static Dictionary<string, ScalarType> scalarTypes;
        public static Dictionary<string, RelationType> relationTypes;
        public static Dictionary<string, ScalarRelation> relationsById;
        public static List<ScalarProperty> nodePropertyMap;
        public static List<ScalarProperty> versionPropertyMap;
        public static string untitledNodeString = "(No title)";

        public string bookUrlPrefix;

        void Awake()
        {
            nodes = new List<ScalarNode>();
            nodesByURL = new Dictionary<string, ScalarNode>();
            relationsById = new Dictionary<string, ScalarRelation>();

            if (bookUrlPrefix[bookUrlPrefix.Length - 1] != '/')
            {
                bookUrlPrefix += "/";
            }
            urlPrefix = bookUrlPrefix;

            ParseScalarTypes();
            ParseRelationTypes();
            ParseNodePropertyMap();
            ParseVersionPropertyMap();
        }

        public static ScalarNode GetNode(string uriSegment)
        {
            string url = urlPrefix + uriSegment;
            ScalarNode node = null;
            if (nodesByURL.ContainsKey(url))
            {
                node = nodesByURL[url];
            }
            return node;
        }

        public static void RemoveNodes()
        {
            nodes.Clear();
            nodesByURL.Clear();
            relationsById.Clear();
        }

        public static IEnumerator LoadNode(string uriSegment, HandleLoadNodeSuccess successCallback = null, HandleNodeLoadError errorCallback = null, int depth = 0, bool references = false, string relation = null, int start = -1, int results = -1, bool provenance = false, bool allVersions = false)
        {
            string queryString = "format=json";
            queryString += "&rec=" + depth;
            queryString += "&ref=" + (references ? 1 : 0);
            if (relation != null) queryString += "&res=" + relation;
            if (start != -1) queryString += "&start=" + start;
            if (results != -1) queryString += "&results=" + results;
            queryString += "&prov=" + (provenance ? 1 : 0);
            queryString += "&versions" + (allVersions ? 1 : 0);
            UnityWebRequest request = UnityWebRequest.Get(urlPrefix + "rdf/node/" + uriSegment + "?" + queryString);
            Debug.Log("Load node: " + request.url);
            yield return request.SendWebRequest();
            if (!request.isNetworkError && !request.isHttpError)
            {
                Debug.Log(request.downloadHandler.text);
                JSONNode data = JSON.Parse(request.downloadHandler.text);
                ParseNodes(data);
                ParseRelations(data);
                if (successCallback != null)
                {
                    successCallback(data);
                }
            }
            else
            {
                if (errorCallback != null)
                {
                    errorCallback(request.error);
                }
            }
        }

        private static List<ScalarNode> ParseNodes(JSONNode data)
        {
            bool isNode;
            ScalarNode node;
            string versionUrl = "";
            List<ScalarNode> resultNodes = new List<ScalarNode>();
            List<JSONNode> versionData = new List<JSONNode>();
            foreach (var key in data.Keys)
            {
                isNode = true;
                if (data[key]["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"] != null)
                {
                    if (data[key]["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"][0]["value"] == "http://scalar.usc.edu/2012/01/scalar-ns#Version")
                    {
                        isNode = false;
                    }
                } else
                {
                    try
                    {
                        int version = int.Parse(ScalarAPI.GetVersionExtension(key));
                        if (version == 0)
                        {
                            isNode = false;
                        }
                    } catch
                    {
                        isNode = false;
                    }
                }
                if (data[key]["http://www.openannotation.org/ns/hasBody"] != null || data[key]["http://purl.org/dc/terms/isVersionOf"] != null)
                {
                    isNode = false;
                }
                if (isNode)
                {
                    versionData.Clear();
                    // gather all versions of the node contained in the current data
                    if (data[key]["http://purl.org/dc/terms/hasVersion"] != null)
                    {
                        int n = data[key]["http://purl.org/dc/terms/hasVersion"].Count;
                        for (int i = 0; i < n; i++)
                        {
                            versionUrl = data[key]["http://purl.org/dc/terms/hasVersion"][i]["value"];
                            versionData.Add(data[versionUrl]);
                        }
                    }
                    // If there is no version, create an empty one .. this ensures that node.current exists ..
                    if (versionData.Count == 0 && data[key]["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"] == null && data[key]["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"][0]["value"] != "http://scalar.usc.edu/2012/01/scalar-ns#Book" && data[key]["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"][0]["value"] != "http://xmlns.com/foaf/0.1/Person")
                    {
                        versionUrl = key + ".0";
                        versionData.Add(data[versionUrl] != null ? data[versionUrl] : JSON.Parse("{}"));
                    }
                    
                    if (nodesByURL.ContainsKey(key))
                    {
                        // if this node has already been created, then update it
                        node = nodesByURL[key];
                        node.ParseData(data[key], versionData);
                    }
                    else
                    {
                        // otherwise, create the node and store it
                        node = new ScalarNode(key, data[key], versionData);
                        AddNode(node);
                    }
                    resultNodes.Add(node);
                }
            }
            return resultNodes;
        }

        private static void ParseRelations(JSONNode data)
        {
            ScalarRelation relation;
            foreach (var key in data.Keys)
            {
                if (data[key]["http://www.openannotation.org/ns/hasBody"] != null)
                {
                    relation = new ScalarRelation(data[key]);
                    relationsById[relation.id] = relation;
                }
            }
            int n = nodes.Count;
            ScalarNode node;
            for (int i = 0; i < n; i++)
            {
                node = nodes[i];
                node.ParseRelations();
                if (!System.Object.ReferenceEquals(node.current, null))
                {
                    node.current.ParseRelations();
                }
            }
        }

        private static void AddNode(ScalarNode node)
        {
            if (!nodesByURL.ContainsKey(node.url))
            {
                nodes.Add(node);
                nodesByURL[node.url] = node;
            }
        }

        public static string GetVersionExtension(string uri)
        {
            string[] array = uri.Split('?');
            uri = array[0];
            string basename = GetBasename(uri);
            array = basename.Split('.');
            if (array.Length == 1) return "";
            string[] arrayB = array[array.Length - 1].Split('#');
            string ext = arrayB[0];
            try
            {
                int version = int.Parse(ext);
                return ext;
            } catch
            {
                return "";
            }
        }

        public static string GetBasename(string path, string suffix = null)
        {
            string pattern = @"/^.*[\/\\]/g";
            string b = Regex.Replace(path, pattern, "", RegexOptions.None);
            if (suffix != null && b.Substring(b.Length - suffix.Length) == suffix)
            {
                b = b.Substring(0, b.Length - suffix.Length);
            }
            return b;
        }

        public static string StripAllExtensions(string uri)
        {
            List<string> orig;
            List<string> array;

            array = new List<string>(uri.Split('#'));
            uri = array[0];

            array = new List<string>(uri.Split('?'));
            uri = array[0];

            orig = array = new List<string>(uri.Split('/'));
            string segment = array[array.Count - 1];

            array = new List<string>(segment.Split('.'));
            if (array.Count == 1) return uri;

            try
            {
                int version = int.Parse(array[array.Count - 1]);
            } catch
            {
                return uri;
            }

            array.RemoveAt(array.Count - 1);
            orig.RemoveAt(orig.Count - 1);
            if (orig.Count == 0) return String.Join(".", array.ToArray());
            return String.Join("/", orig.ToArray()) + "/" + String.Join(".", array.ToArray());
        }

        public static string GetAnchorSegment(string uri)
        {
            List<string> temp = new List<string>(uri.Split('#'));
            if (temp.Count > 1)
            {
                return temp[temp.Count - 1];
            }
            return "";
        }

        public static Dictionary<string,string> GetAnchorVars(string uri)
        {
            Dictionary<string, string> obj = null;
            List<string> varChunks, propChunks;
            string anchorSeg = ScalarAPI.GetAnchorSegment(uri);
            if (anchorSeg != "")
            {
                obj = new Dictionary<string, string>();
                varChunks = new List<string>(anchorSeg.Split('&'));
                int n = varChunks.Count;
                for (int i = 0; i < n; i++)
                {
                    propChunks = new List<string>(varChunks[i].Split('='));
                    obj[propChunks[0]] = propChunks[1];
                }
            }
            return obj;
        }

        public static string StripVersion(string versionURI)
        {
            List<string> tempA = new List<string>(versionURI.Split('/'));
            if (tempA[tempA.Count - 1] == "")
            {
                tempA.RemoveAt(tempA.Count - 1);
            }
            string segment = tempA[tempA.Count - 1];
            List<string> tempB = new List<string>(segment.Split('.'));
            if (tempB.Count > 1)
            {
                tempB.RemoveAt(tempB.Count - 1);
                segment = String.Join(".", tempB.ToArray());
            }
            tempA[tempA.Count - 1] = segment;
            string uri = String.Join("/", tempA.ToArray());
            return uri;
        }

        public static string StripInitialNonAlphabetics(string str)
        {
            str = Regex.Replace(str, "[^a-zA-Z 0-9]+", "");
            str = Regex.Replace(str, "^the /i", "");
            str = Regex.Replace(str, "^a /i", "");
            str = Regex.Replace(str, "^\"|^\'", "");
            str = char.ToUpper(str[0]) + str.Substring(1);
            return str;
        }

        public static string DecimalSecondsToHMMSS(float seconds, bool showMilliseconds = false)
        {
            float h, m, s, ms;
            string mString, sString;
            string msString = "";

            h = Mathf.Floor(seconds / 3600);
            seconds -= (h * 3600);
            m = Mathf.Floor(seconds / 60);
            seconds -= (m * 60);
            s = Mathf.Floor(seconds);
            seconds -= s;
            ms = Mathf.Round(seconds * 1000);

            if (!showMilliseconds)
            {
                s += Mathf.Round(ms * .001f);
            }

            mString = msString + "";
            if (mString.Length == 1) mString = "0" + mString;
            sString = s + "";
            if (sString.Length == 1) sString = "0" + sString;
            msString = ms + "";
            while (msString.Length < 3) { msString = msString + "0"; }

            if (showMilliseconds)
            {
                return h + ":" + mString + ":" + sString + "." + msString;
            }
            else
            {
                return h + ":" + mString + ":" + sString;
            }
        }

        private void ParseScalarTypes()
        {
            scalarTypes = new Dictionary<string, ScalarType>();
            var file = Resources.Load("scalarTypes") as TextAsset;
            JSONNode data = JSON.Parse(file.text);
            foreach (var key in data.Keys)
            {
                scalarTypes.Add(key, new ScalarType(data[key]));
            }
        }

        private void ParseRelationTypes()
        {
            relationTypes = new Dictionary<string, RelationType>();
            var file = Resources.Load("relationTypes") as TextAsset;
            JSONNode data = JSON.Parse(file.text);
            foreach (var key in data.Keys)
            {
                relationTypes.Add(key, new RelationType(data[key]));
            }
        }

        private void ParseNodePropertyMap()
        {
            nodePropertyMap = new List<ScalarProperty>();
            var file = Resources.Load("nodePropertyMap") as TextAsset;
            JSONArray data = JSON.Parse(file.text).AsArray;
            foreach (JSONNode item in data)
            {
                nodePropertyMap.Add(new ScalarProperty(item));
            }
        }

        private void ParseVersionPropertyMap()
        {
            versionPropertyMap = new List<ScalarProperty>();
            var file = Resources.Load("versionPropertyMap") as TextAsset;
            JSONArray data = JSON.Parse(file.text).AsArray;
            foreach (JSONNode item in data)
            {
                versionPropertyMap.Add(new ScalarProperty(item));
            }
        }
    }

    public delegate void HandleLoadNodeSuccess(JSONNode json);
    public delegate void HandleNodeLoadError(string error);
}
