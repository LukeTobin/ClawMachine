using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingManager : MonoBehaviour
{
    [Header("XR References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private XRReferenceImageLibrary referenceImageLibrary;

    [Header("Claw Machines")]
    [SerializeField] private GameObject[] clawMachines;

    private static Dictionary<Guid, GameObject> trackingList = new Dictionary<Guid, GameObject>();

    private void OnEnable()
    {
        for (int i = 0; i < clawMachines.Length; i++)
        {
            trackingList.Add(referenceImageLibrary[i].guid, clawMachines[i]);
        }
        
        trackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj)
    {
        foreach (ARTrackedImage image in obj.added)
        {
            if (trackingList.ContainsKey(image.referenceImage.guid))
            {
                // Rotate claw machine on Y axis toward the camera
                Vector3 cameraToImage = Camera.main.transform.position - image.transform.position;
                Quaternion initialRotation = trackingList[image.referenceImage.guid].transform.rotation;
                Quaternion lookRotation = Quaternion.LookRotation(cameraToImage, Vector3.up);
                
                float angle = Quaternion.Angle(Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), initialRotation);
                
                Quaternion finalRotation = Quaternion.Euler(initialRotation.eulerAngles.x, angle, initialRotation.eulerAngles.z);
                
                Instantiate(trackingList[image.referenceImage.guid], image.transform.position, finalRotation);
            }
        }
    }
}