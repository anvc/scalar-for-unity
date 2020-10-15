using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace ANVC.Scalar
{
    public class ScalarCamera : MonoBehaviour
    {

        private Camera _camera;

        // Use this for initialization
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTransform(string point_3d)
        {
            JSONNode node = JSON.Parse(point_3d);
            Vector3 targetPosition = new Vector3(node["targetX"], node["targetY"], node["targetZ"]);
            Vector3 cameraPosition = new Vector3(node["cameraX"], node["cameraY"], node["cameraZ"]);
            transform.position = cameraPosition;
            transform.LookAt(targetPosition);
            transform.RotateAround(cameraPosition, targetPosition - cameraPosition, node["roll"]);
            _camera.fieldOfView = node["fieldOfView"];
        }
    }
}
