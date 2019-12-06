using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ANVC.Scalar
{
    public class ScalarCamera : MonoBehaviour
    {

        public Transform target;

        private Camera _camera;

        // Use this for initialization
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTransform(string transformStr)
        {
            // data is comma-delimited
            string[] temp = transformStr.Split(',');

            // first three values are x, y, and z position
            Vector3 position = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
            transform.position = position;
            Quaternion rotation;

            // second three values are heading, tilt, and field of view
            if (temp.Length == 6)
            {
                rotation = Quaternion.Euler(float.Parse(temp[4]), float.Parse(temp[3]), 0);
                transform.rotation = rotation;
                _camera.fieldOfView = float.Parse(temp[5]);
            }

            // otherwise look at the target (if present)
            else if (target != null)
            {
                transform.LookAt(target);
            }
        }
    }
}
