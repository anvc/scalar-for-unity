using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace ANVC.Scalar
{
    public class ScalarVersion
    {
        public string url;
        public JSONNode data;
        public int number;
        public string created;
        public string description;
        public string isReplacedBy;
        public string isVersionOf;
        public string title;
        public string content;
        public string defaultView;
        public string baseType;
        public string sourceFile;
        public string source;
        public string thumbnail;
        public string sourceLocation;
        public string urn;
        public int sortNumber;
        public int continueTo;
        public int continueToContentId;
        public string editorialState;
        public string editorialQueries;
        public int usageRights;
        public bool isLive;
        public string paywall;
        public string fullname;
        public string author;
        public string references;
        public string isReferencedBy;
        public Dictionary<string, string> properties;
        public Dictionary<string, List<string>> auxProperties;

        public ScalarVersion(JSONNode data, ScalarNode node)
        {
            properties = new Dictionary<string, string>();
            auxProperties = new Dictionary<string, List<string>>();
            ParseData(data, node);
        }

        private void ParseData(JSONNode json, ScalarNode node)
        {
            data = json;
            url = data["url"];

            ScalarProperty property;
            int n = ScalarAPI.versionPropertyMap.Count;
            for (int i = 0; i < n; i++)
            {
                property = ScalarAPI.versionPropertyMap[i];
                if (json[property.uri] != null)
                {
                    switch (property.name)
                    {
                        case "number":
                            number = json[property.uri][0]["value"].AsInt;
                            break;
                        case "created":
                            created = json[property.uri][0]["value"];
                            break;
                        case "isReplacedBy":
                            isReplacedBy = json[property.uri][0]["value"];
                            break;
                        case "isVersionOf":
                            isVersionOf = json[property.uri][0]["value"];
                            break;
                        case "title":
                            title = json[property.uri][0]["value"];
                            break;
                        case "content":
                            content = json[property.uri][0]["value"];
                            break;
                        case "defaultView":
                            defaultView = json[property.uri][0]["value"];
                            break;
                        case "baseType":
                            baseType = json[property.uri][0]["value"];
                            break;
                        case "sourceFile":
                            sourceFile = json[property.uri][0]["value"];
                            break;
                        case "source":
                            source = json[property.uri][0]["value"];
                            break;
                        case "thumbnail":
                            thumbnail = json[property.uri][0]["value"];
                            break;
                        case "sourceLocation":
                            sourceLocation = json[property.uri][0]["value"];
                            break;
                        case "urn":
                            urn = json[property.uri][0]["value"];
                            break;
                        case "sortNumber":
                            sortNumber = json[property.uri][0]["value"].AsInt;
                            break;
                        case "continueTo":
                            continueTo = json[property.uri][0]["value"].AsInt;
                            break;
                        case "continueToContentId":
                            continueToContentId = json[property.uri][0]["value"].AsInt;
                            break;
                        case "editorialState":
                            editorialState = json[property.uri][0]["value"];
                            break;
                        case "editorialQueries":
                            editorialQueries = json[property.uri][0]["value"];
                            break;
                        case "usageRights":
                            usageRights = json[property.uri][0]["value"].AsInt;
                            break;
                        case "isLive":
                            isLive = json[property.uri][0]["value"].AsInt == 1 ? true : false;
                            break;
                        case "paywall":
                            paywall = json[property.uri][0]["value"];
                            break;
                        case "fullname":
                            fullname = json[property.uri][0]["value"];
                            break;
                        case "author":
                            author = json[property.uri][0]["value"];
                            break;
                        case "references":
                            references = json[property.uri][0]["value"];
                            break;
                        case "isReferencedBy":
                            isReferencedBy = json[property.uri][0]["value"];
                            break;
                    }
                }
            }

            foreach (var key in json.Keys)
            {
                if (IsVersionProperty(key)) continue;
                int o = json[key].Count;
                if (!auxProperties.ContainsKey(key))
                {
                    auxProperties[key] = new List<string>();
                }
                for (int j = 0; j < o; j++)
                {
                    auxProperties[key].Add(json[key]);
                }
            }

            foreach (var key in json.Keys)
            {
                properties[key] = json[key][0]["value"];
            }
        }

        public void ParseRelations()
        {
            // TODO: add code for parsing non-OA relations
        }

        public override string ToString()
        {
            return "Version " + number + " of “" + title + "”";
        }

        private bool IsVersionProperty(string property)
        {
            int n = ScalarAPI.versionPropertyMap.Count;
            for (int i = 0; i < n; i++)
            {
                if (ScalarAPI.versionPropertyMap[i].uri == property)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
