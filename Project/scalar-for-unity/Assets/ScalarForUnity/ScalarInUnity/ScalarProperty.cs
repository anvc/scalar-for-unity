using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace ANVC.Scalar
{
    public class ScalarProperty
    {
        public string name;
        public string uri;
        public string type;

        public ScalarProperty(JSONNode data)
        {
            name = data["property"];
            uri = data["uri"];
            type = data["type"];
        }
    }
}
