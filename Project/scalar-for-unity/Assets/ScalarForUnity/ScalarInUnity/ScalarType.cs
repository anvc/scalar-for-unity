using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace ANVC.Scalar
{
    public class ScalarType
    {
        public string id;
        public string singular;
        public string plural;

        public ScalarType(JSONNode data)
        {
            id = data["id"];
            singular = data["singular"];
            plural = data["plural"];
        }
    }
}