using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;
using System.Runtime.InteropServices;

namespace ANVC.Scalar
{
    public class ScalarCamera : MonoBehaviour
    {
        public float transitionDuration = 1.5f;
        public AnnotationSelectedExternallyEvent annotationSelectedExternallyEvent;
        public UnityEvent annotationsUpdatedExternallyEvent;
        public MessageReceivedEvent messageReceivedEvent;

        private Camera _camera;
        private Vector3 _targetPosition;

        [DllImport("__Internal")]
        private static extern void ReturnPosition3D(string position3D);

        // Use this for initialization
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void HandleAnnotationsUpdated()
        {
            annotationsUpdatedExternallyEvent.Invoke();
        }

        public void GetTransform()
        {
            JSONObject data = new JSONObject();
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
            {
                _targetPosition = transform.position + (transform.forward * hit.distance);
            }
            else
            {
                _targetPosition = transform.position + (transform.forward * 5);
            }
            data["targetX"] = _targetPosition.x;
            data["targetY"] = _targetPosition.y;
            data["targetZ"] = _targetPosition.z;
            data["cameraX"] = transform.position.x;
            data["cameraY"] = transform.position.y;
            data["cameraZ"] = transform.position.z;
            data["roll"] = 360 - transform.rotation.eulerAngles.z;
            data["fieldOfView"] = _camera.fieldOfView;
            ReturnPosition3D(data.ToString());
        }

        public void SetTransform(string data)
        {
            JSONNode json = JSON.Parse(data);
            SetTransformNoEvent(json);
            annotationSelectedExternallyEvent.Invoke(json);
        }

        public void SetTransformNoEvent(JSONNode node)
        {
            _targetPosition = new Vector3(node["targetX"], node["targetY"], node["targetZ"]);
            Vector3 cameraPosition = new Vector3(node["cameraX"], node["cameraY"], node["cameraZ"]);
            LeanTween.cancel(transform.gameObject);
            LeanTween.move(transform.gameObject, cameraPosition, transitionDuration).setEaseInOutCubic();
            Vector3 upwards = new Vector3(Mathf.Sin(node["roll"] * Mathf.Deg2Rad), Mathf.Cos(node["roll"] * Mathf.Deg2Rad), 0);
            Quaternion rotation = Quaternion.LookRotation(_targetPosition - cameraPosition, upwards);
            LeanTween.rotate(transform.gameObject, rotation.eulerAngles, transitionDuration).setEaseInOutCubic();
            LeanTween.value(transform.gameObject, updateFieldOfView, _camera.fieldOfView, node["fieldOfView"], transitionDuration);
            void updateFieldOfView(float val, float ratio)
            {
                _camera.fieldOfView = val;
            }
        }

        public void HandleMessage(string data)
        {
            JSONNode json = JSON.Parse(data);
            messageReceivedEvent.Invoke(json);
        }
    }
}

[System.Serializable]
public class AnnotationSelectedExternallyEvent : UnityEvent<JSONNode> { }

[System.Serializable]
public class MessageReceivedEvent : UnityEvent<JSONNode> { }