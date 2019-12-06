﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

namespace ANVC.Scalar
{
    public class ScalarRelation
    {
        public string id;
        public ScalarNode body;
        public ScalarNode target;
        public RelationType type;
        public string subType;
        public RelationProperties properties;
        public float index;
        public string startString;
        public string endString;
        public string separator;

        public ScalarRelation(JSONNode json, ScalarNode bodyNode = null, ScalarNode targetNode = null, RelationType relationType = null)
        {
            if (json != null)
            {
                JSONNode bodyProp = json["http://www.openannotation.org/ns/hasBody"];
                JSONNode targetProp = json["http://www.openannotation.org/ns/hasTarget"];
                properties = new RelationProperties();
                if (bodyProp != null && targetProp != null)
                {
                    id = ScalarAPI.StripAllExtensions(bodyProp[0]["value"]) + ScalarAPI.StripAllExtensions(targetProp[0]["value"]);
                    body = ScalarAPI.nodesByURL[ScalarAPI.StripVersion(bodyProp[0]["value"])];
                    target = ScalarAPI.nodesByURL[ScalarAPI.StripVersion(ScalarAPI.StripAllExtensions(targetProp[0]["value"]))];

                    // parse the relation type and populate extents (if any)
                    Dictionary<string, string> anchorVars = ScalarAPI.GetAnchorVars(targetProp[0]["value"]);

                    List<string> temp;
                    if (anchorVars == null)
                    {
                        type = ScalarAPI.relationTypes["tag"];
                    }
                    else
                    {
                        foreach (KeyValuePair<string,string> kv in anchorVars)
                        {
                            switch (kv.Key)
                            {
                                case "t":
                                    // we use this construction here and below so that if the 'title' var
                                    // is specified first it won't get overwritten
                                    if (type == null) type = ScalarAPI.relationTypes["annotation"];

                                    temp = new List<string>(anchorVars[kv.Key].Substring(4).Split(','));
                                    properties.start = float.Parse(temp[0]);
                                    properties.end = float.Parse(temp[1]);
                                    startString = ScalarAPI.DecimalSecondsToHMMSS(properties.start);
                                    endString = ScalarAPI.DecimalSecondsToHMMSS(properties.end);
                                    separator = " - ";
                                    subType = "temporal";
                                    index = properties.start;
                                    id += kv.Key + index;
                                    break;

                                case "line":
                                    if (type == null) type = ScalarAPI.relationTypes["annotation"];
                                    temp = new List<string>(anchorVars[kv.Key].Split(','));
                                    properties.start = float.Parse(temp[0]);
                                    properties.end = float.Parse(temp[1]);
                                    startString = "Line " + properties.start;
                                    if (properties.start == properties.end)
                                    {
                                        separator = "";
                                        endString = "";
                                    }
                                    else
                                    {
                                        separator = " - ";
                                        endString = properties.end.ToString();
                                    }
                                    subType = "textual";
                                    index = properties.start;
                                    id += kv.Key + index;
                                    break;

                                case "xywh":
                                    if (type == null) type = ScalarAPI.relationTypes["annotation"];
                                    temp = new List<string>(anchorVars[kv.Key].Split(','));
                                    properties.x = temp[0];
                                    properties.y = temp[1];
                                    properties.width = temp[2];
                                    properties.height = temp[3];
                                    startString = "x:" + Mathf.Round(float.Parse(properties.x));
                                    if (properties.x.Contains("%"))
                                    {
                                        startString += "%";
                                    }
                                    startString += " y:" + Mathf.Round(float.Parse(properties.y));
                                    if (properties.y.Contains("%"))
                                    {
                                        startString += "%";
                                    }
                                    if (int.Parse(properties.width) == 0 && int.Parse(properties.height) == 0)
                                    {
                                        endString = "";
                                    }
                                    else
                                    {
                                        endString = "w:" + Mathf.Round(float.Parse(properties.width));
                                        if (properties.width.Contains("%"))
                                        {
                                            endString += "%";
                                        }
                                        endString += " h:" + Mathf.Round(float.Parse(properties.height));
                                        if (properties.height.Contains("%"))
                                        {
                                            endString += "%";
                                        }
                                    }
                                    separator = " ";
                                    subType = "spatial";
                                    index = float.Parse(properties.x) * float.Parse(properties.y);
                                    id += kv.Key + index;
                                    break;

                                case "index":
                                    if (type == null) type = ScalarAPI.relationTypes["path"];
                                    properties.index = float.Parse(anchorVars[kv.Key]);
                                    startString = "Page " + properties.index;
                                    endString = "";
                                    separator = "";
                                    index = properties.index;
                                    id += kv.Key + index;
                                    break;

                                case "datetime":
                                    if (type == null) type = ScalarAPI.relationTypes["comment"];
                                    properties.datetime = anchorVars[kv.Key];
                                    DateTime date = new DateTime(); // this should really be based on properties.datetime
                                    startString = properties.datetime;
                                    endString = "";
                                    separator = "";
                                    index = date.Ticks;
                                    id += kv.Key + index;
                                    break;

                                case "type":
                                    switch (anchorVars[kv.Key])
                                    {

                                        case "commentary":
                                            type = ScalarAPI.relationTypes["commentary"];
                                            break;

                                        case "review":
                                            type = ScalarAPI.relationTypes["review"];
                                            break;

                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                body = bodyNode;
                target = targetNode;
                id = body.url + target.url;
                if (type != null)
                {
                    type = relationType;
                }
                else
                {
                    type = ScalarAPI.relationTypes["unknown"];
                }
            }

            if (body != null && target != null)
            {
                body.AddRelation(this);
                target.AddRelation(this);
            }
        }
    }
}