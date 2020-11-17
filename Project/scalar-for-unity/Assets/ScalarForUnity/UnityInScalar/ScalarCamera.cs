using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;

namespace ANVC.Scalar
{
    public class ScalarCamera : MonoBehaviour
    {
        public float transitionDuration = 1.5f;
        public AnnotationSelectedExternallyEvent annotationSelectedExternallyEvent;

        private Camera _camera;

        // Use this for initialization
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTransform(string point_3d)
        {
            SetTransformNoEvent(point_3d);
            annotationSelectedExternallyEvent.Invoke(JSON.Parse(point_3d));
        }

        public void SetTransformNoEvent(string point_3d)
        {
            JSONNode node = JSON.Parse(point_3d);
            Vector3 targetPosition = new Vector3(node["targetX"], node["targetY"], node["targetZ"]);
            Vector3 cameraPosition = new Vector3(node["cameraX"], node["cameraY"], node["cameraZ"]);
            LeanTween.cancel(transform.gameObject);
            LeanTween.move(transform.gameObject, cameraPosition, transitionDuration).setEaseInOutCubic();
            Vector3 upwards = new Vector3(Mathf.Sin(node["roll"] * Mathf.Deg2Rad), Mathf.Cos(node["roll"] * Mathf.Deg2Rad), 0);
            Quaternion rotation = Quaternion.LookRotation(targetPosition - cameraPosition, upwards);
            LeanTween.rotate(transform.gameObject, rotation.eulerAngles, transitionDuration).setEaseInOutCubic();
            LeanTween.value(transform.gameObject, updateFieldOfView, _camera.fieldOfView, node["fieldOfView"], transitionDuration);
            void updateFieldOfView(float val, float ratio)
            {
                _camera.fieldOfView = val;
            }
        }
    }
}

[System.Serializable]
public class AnnotationSelectedExternallyEvent : UnityEvent<JSONNode> { }