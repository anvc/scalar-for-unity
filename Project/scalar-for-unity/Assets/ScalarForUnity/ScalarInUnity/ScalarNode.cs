using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Linq;
using System;

namespace ANVC.Scalar
{
    public class ScalarNode
    {
        public string url;
        public string slug;
        public string created;
        public string baseType;
        public string color;
        public string title;
        public string thumbnail;
        public string tableOfContents;
        public string name;
        public string homepage;
        public string urn;
        public string background;
        public string banner;
        public string audio;
        public bool isLive;
        public string paywall;
        public string author;
        public ScalarVersion current;
        public List<ScalarVersion> versions;
        public List<ScalarRelation> incomingRelations;
        public List<ScalarRelation> outgoingRelations;
        public Dictionary<string,ScalarType> scalarTypes;
        public JSONNode data;
        public Dictionary<string, string> properties;

        public ScalarNode(string _url, JSONNode json, List<JSONNode> versionData)
        {
            url = _url;
            data = json;
            slug = ScalarAPI.StripAllExtensions(url).Substring(ScalarAPI.urlPrefix.Length);
            incomingRelations = new List<ScalarRelation>();
            outgoingRelations = new List<ScalarRelation>();
            versions = new List<ScalarVersion>();
            scalarTypes = new Dictionary<string, ScalarType>();
            properties = new Dictionary<string, string>();
            ParseData(json, versionData);
        }

        public void ParseData(JSONNode json, List<JSONNode> versionData)
        {
            ScalarProperty property;
            int n = ScalarAPI.nodePropertyMap.Count;
            for (int i = 0; i < n; i++)
            {
                property = ScalarAPI.nodePropertyMap[i];
                if (json[property.uri] != null)
                {
                    switch (property.name)
                    {
                        case "created":
                            created = json[property.uri][0]["value"];
                            break;
                        case "baseType":
                            baseType = json[property.uri][0]["value"];
                            break;
                        case "color":
                            color = json[property.uri][0]["value"];
                            break;
                        case "title":
                            title = json[property.uri][0]["value"];
                            break;
                        case "thumbnail":
                            thumbnail = json[property.uri][0]["value"];
                            break;
                        case "tableOfContents":
                            tableOfContents = json[property.uri][0]["value"];
                            break;
                        case "name":
                            name = json[property.uri][0]["value"];
                            break;
                        case "homepage":
                            homepage = json[property.uri][0]["value"];
                            break;
                        case "urn":
                            urn = json[property.uri][0]["value"];
                            break;
                        case "background":
                            background = json[property.uri][0]["value"];
                            break;
                        case "banner":
                            banner = json[property.uri][0]["value"];
                            break;
                        case "audio":
                            audio = json[property.uri][0]["value"];
                            break;
                        case "isLive":
                            isLive = json[property.uri][0]["value"].AsInt == 1 ? true : false;
                            break;
                        case "paywall":
                            paywall = json[property.uri][0]["value"];
                            break;
                        case "author":
                            author = json[property.uri][0]["value"];
                            break;
                    }
                }
            }

            foreach (var key in json.Keys)
            {
                properties[key] = json[key][0]["value"];
            }

            if (versionData != null && versionData.Count > 0)
            {
                versions.Clear();
                n = versionData.Count;
                ScalarVersion version;
                for (int i = 0; i < n; i++)
                {
                    version = new ScalarVersion(versionData[i], this);
                    versions.Add(version);
                }
                versions.OrderByDescending(i => i.number);
                current = versions[0];
            }

            switch (baseType)
            {
                case "http://scalar.usc.edu/2012/01/scalar-ns#Composite":
                    scalarTypes["page"] = ScalarAPI.scalarTypes["page"];
                    break;

                case "http://scalar.usc.edu/2012/01/scalar-ns#Media":
                    scalarTypes["media"] = ScalarAPI.scalarTypes["media"];
                    break;

                case "http://scalar.usc.edu/2012/01/scalar-ns#Book":
                    scalarTypes["book"] = ScalarAPI.scalarTypes["book"];
                    break;

                case "http://scalar.usc.edu/2012/01/scalar-ns#Page":
                    scalarTypes["defaultPage"] = ScalarAPI.scalarTypes["defaultPage"];
                    break;

                case "http://xmlns.com/foaf/0.1/Person":
                    scalarTypes["person"] = ScalarAPI.scalarTypes["person"];
                    title = name;
                    break;
            }

            if (slug == "toc")
            {
                scalarTypes["toc"] = ScalarAPI.scalarTypes["toc"];
            }
        }

        public void ParseRelations()
        {
            // TODO: add code for parsing non-OA relations
        }

        public void AddRelation(ScalarRelation relation)
        {
            int n;
            bool foundExisting = false;
            if (relation.body == this)
            {
                if (!outgoingRelations.Contains(relation))
                {
                    n = outgoingRelations.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (outgoingRelations[i].id == relation.id)
                        {
                            outgoingRelations[i] = relation;
                            foundExisting = true;
                        }
                    }
                    if (!foundExisting)
                    {
                        outgoingRelations.Add(relation);
                    }
                    if (relation.type.id != "referee" && relation.type.id != "author" && relation.type.id != "commentator" && relation.type.id != "reviewer" && scalarTypes.ContainsKey(relation.type.id))
                    {
                        scalarTypes[relation.type.id] = ScalarAPI.scalarTypes[relation.type.id];
                    }
                }
            } else if (relation.target == this)
            {
                if (!incomingRelations.Contains(relation))
                {
                    n = incomingRelations.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (incomingRelations[i].id == relation.id)
                        {
                            incomingRelations[i] = relation;
                            foundExisting = true;
                        }
                    }
                    if (!foundExisting)
                    {
                        incomingRelations.Add(relation);
                    }
                    if (relation.type.id != "referee" && relation.type.id != "author" && relation.type.id != "commentator" && relation.type.id != "reviewer" && scalarTypes.ContainsKey(relation.type.id))
                    {
                        scalarTypes[relation.type.id] = ScalarAPI.scalarTypes[relation.type.id];
                    }
                }
            }
        }

        public List<ScalarNode> GetRelatedNodes(string type, string direction, bool includeNonPages = false, string sort = "index")
        {
            int n;
            ScalarRelation relation;
            List<ScalarRelation> relations = new List<ScalarRelation>();
            List<ScalarNode> results = new List<ScalarNode>();
            if (direction == "incoming" || direction == "both")
            {
                n = incomingRelations.Count;
                for (int i = 0; i < n; i++)
                {
                    relation = incomingRelations[i];
                    if (relation.type.id == type || type == null)
                    {
                        if (includeNonPages || (!includeNonPages && !System.Object.ReferenceEquals(relation.body.current, null) && !System.Object.ReferenceEquals(relation.target.current, null))) {
                            relations.Add(relation);
                        }
                    }
                }
            }
            if (direction == "outgoing" || direction == "both")
            {
                n = outgoingRelations.Count;
                for (int i = 0; i < n; i++)
                {
                    relation = outgoingRelations[i];
                    if (relation.type.id == type || type == null)
                    {
                        if (includeNonPages || (!includeNonPages && !System.Object.ReferenceEquals(relation.body.current, null) && !System.Object.ReferenceEquals(relation.target.current, null))) {
                            relations.Add(relation);
                        }
                    }
                }
            }
            switch (sort)
            {
                case "index":
                    relations.OrderBy(i => i.index);
                    break;
                case "reverseindex":
                    relations.OrderByDescending(i => i.index);
                    break;
            }
            n = relations.Count;
            for (int i = 0; i < n; i++)
            {
                relation = relations[i];
                if (relation.body != this)
                {
                    results.Add(relation.body);
                }
                if (relation.target != this)
                {
                    results.Add(relation.target);
                }
            }
            if (sort == "alphabetical")
            {
                TitleComparer comparer = new TitleComparer();
                results.Sort(comparer);
            }
            return results;
        }

        public List<ScalarRelation> GetRelations(string type, string direction, bool includeNonPages = false, string sort = "index")
        {
            int n;
            ScalarRelation relation;
            List<ScalarRelation> results = new List<ScalarRelation>();
            if (direction == "incoming" || direction == "both")
            {
                n = incomingRelations.Count;
                for (int i = 0; i < n; i++)
                {
                    relation = incomingRelations[i];
                    if (relation.type.id == type || type == null)
                    {
                        if (includeNonPages || (!includeNonPages && !System.Object.ReferenceEquals(relation.body.current, null) && !System.Object.ReferenceEquals(relation.target.current, null)))
                        {
                            results.Add(relation);
                        }
                    }
                }
            }
            if (direction == "outgoing" || direction == "both")
            {
                n = outgoingRelations.Count;
                for (int i = 0; i < n; i++)
                {
                    relation = outgoingRelations[i];
                    if (relation.type.id == type || type == null)
                    {
                        if (includeNonPages || (!includeNonPages && !System.Object.ReferenceEquals(relation.body.current, null) && !System.Object.ReferenceEquals(relation.target.current, null)))
                        {
                            results.Add(relation);
                        }
                    }
                }
            }
            switch (sort)
            {
                case "index":
                    results.OrderBy(i => i.index);
                    break;
                case "reverseindex":
                    results.OrderByDescending(i => i.index);
                    break;
            }
            return results;
        }

        public string GetDisplayTitle()
        {
            string displayTitle = ScalarAPI.untitledNodeString;
            if (!System.Object.ReferenceEquals(current, null))
            {
                if (current.title != null)
                {
                    displayTitle = current.title;
                }
            } else if (title != null)
            {
                displayTitle = title;
            }
            return displayTitle;
        }

        public string GetSortTitle()
        {
            string sortTitle = GetDisplayTitle();
            if (sortTitle != ScalarAPI.untitledNodeString)
            {
                sortTitle = ScalarAPI.StripInitialNonAlphabetics(sortTitle);
            }
            return sortTitle;
        }

        public string GetAbsoluteThumbnailURL()
        {
            if (thumbnail != null)
            {
                string thumbnailUrl = thumbnail;
                if (thumbnailUrl.Contains("://"))
                {
                    thumbnailUrl = ScalarAPI.urlPrefix + thumbnailUrl;
                }
                return thumbnailUrl;
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            if (System.Object.ReferenceEquals(current, null))
            {
                return "Node w/o a version";
            }
            else
            {
                return "Node “" + current.title + "” (" + incomingRelations.Count + " incoming, " + outgoingRelations.Count + " outgoing)";
            }
        }
    }

    public class TitleComparer : IComparer<ScalarNode>
    {
        public int Compare(ScalarNode a, ScalarNode b)
        {
            string nameA = a.GetSortTitle().ToLower();
            string nameB = b.GetSortTitle().ToLower();
            return string.Compare(nameA, nameB);
        }
    }
}
