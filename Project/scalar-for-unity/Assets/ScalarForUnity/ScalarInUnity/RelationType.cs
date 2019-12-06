using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace ANVC.Scalar
{
    public class RelationType
    {
        public string id;
        public string body;
        public string bodyPlural;
        public string target;
        public string targetPlural;
        public string incoming;
        public string outgoing;

        public RelationType(JSONNode data)
        {
            id = data["id"];
            body = data["body"];
            bodyPlural = data["bodyPlural"];
            target = data["target"];
            targetPlural = data["targetPlural"];
            incoming = data["incoming"];
            outgoing = data["outgoing"];
        }
    }
}