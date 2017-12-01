// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloLensHandTracking
{
    /// <summary>
    /// HandsManager determines if the hand is currently detected or not.
    /// </summary>
    public class HandsTrackingController : MonoBehaviour
    {
        /// <summary>
        /// HandDetected tracks the hand detected state.
        /// Returns true if the list of tracked hands is not empty.
        /// </summary>
        public bool HandDetected
        {
            get { return trackedHands.Count > 0; }
        }

        public GameObject TrackingObject;
        public TextMesh StatusText;
        public Color DefaultColor = Color.green;
        public Color TapColor = Color.blue;
        public Color HoldColor = Color.red;

        private HashSet<uint> trackedHands = new HashSet<uint>();
        private Dictionary<uint, GameObject> trackingObject = new Dictionary<uint, GameObject>();
        private GestureRecognizer gestureRecognizer;
        private uint activeId;

        void Awake()
        {
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;

            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.Hold);
            gestureRecognizer.Tapped += GestureRecognizerTapped;
            gestureRecognizer.HoldStarted += GestureRecognizer_HoldStarted;
            gestureRecognizer.HoldCompleted += GestureRecognizer_HoldCompleted;
            gestureRecognizer.HoldCanceled += GestureRecognizer_HoldCanceled;            
            gestureRecognizer.StartCapturingGestures();
            StatusText.text = "READY";
        }

        void ChangeObjectColor(GameObject obj, Color color)
        {            
            var rend = obj.GetComponentInChildren<Renderer>();
            if (rend)
            {
                rend.material.color = color;
                Debug.LogFormat("Color Change: {0}", color.ToString());
            }
        }


        private void GestureRecognizer_HoldStarted(HoldStartedEventArgs args)
        {
            uint id = args.source.id;            
            StatusText.text = $"HoldStarted - Kind:{args.source.kind.ToString()} - Id:{id}";
            if (trackingObject.ContainsKey(activeId))
            {
                ChangeObjectColor(trackingObject[activeId], HoldColor);
                StatusText.text += "-TRACKED";
            }
        }

        private void GestureRecognizer_HoldCompleted(HoldCompletedEventArgs args)
        {
            uint id = args.source.id;            
            StatusText.text = $"HoldCompleted - Kind:{args.source.kind.ToString()} - Id:{id}";
            if(trackingObject.ContainsKey(activeId))
            {
                ChangeObjectColor(trackingObject[activeId], DefaultColor);
                StatusText.text += "-TRACKED";
            }
        }

        private void GestureRecognizer_HoldCanceled(HoldCanceledEventArgs args)
        {
            uint id = args.source.id;            
            StatusText.text = $"HoldCanceled - Kind:{args.source.kind.ToString()} - Id:{id}";
            if (trackingObject.ContainsKey(activeId))
            {
                ChangeObjectColor(trackingObject[activeId], DefaultColor);
                StatusText.text += "-TRACKED";
            }
        }

        private void GestureRecognizerTapped(TappedEventArgs args)
        {            
            uint id = args.source.id;
            StatusText.text = $"Tapped - Kind:{args.source.kind.ToString()} - Id:{id}";
            if (trackingObject.ContainsKey(activeId))
            {
                ChangeObjectColor(trackingObject[activeId], TapColor);
                StatusText.text += "-TRACKED";
            }            
        }
        

        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
        {
            uint id = args.state.source.id;
            // Check to see that the source is a hand.
            if (args.state.source.kind != InteractionSourceKind.Hand)
            {
                return;
            }            
            trackedHands.Add(id);
            activeId = id;

            var obj = Instantiate(TrackingObject) as GameObject;
            Vector3 pos;

            if (args.state.sourcePose.TryGetPosition(out pos))
            {
                obj.transform.position = pos;
            }

            trackingObject.Add(id, obj);
        }

        private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            uint id = args.state.source.id;
            Vector3 pos;
            Quaternion rot;

            if (args.state.source.kind == InteractionSourceKind.Hand)
            {
                if (trackingObject.ContainsKey(id))
                {
                    if (args.state.sourcePose.TryGetPosition(out pos))
                    {
                        trackingObject[id].transform.position = pos;
                    }

                    if (args.state.sourcePose.TryGetRotation(out rot))
                    {
                        trackingObject[id].transform.rotation = rot;
                    }
                }
            }
        }

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            uint id = args.state.source.id;
            // Check to see that the source is a hand.
            if (args.state.source.kind != InteractionSourceKind.Hand)
            {
                return;
            }

            if (trackedHands.Contains(id))
            {
                trackedHands.Remove(id);
            }

            if (trackingObject.ContainsKey(id))
            {
                var obj = trackingObject[id];
                trackingObject.Remove(id);
                Destroy(obj);
            }
            if (trackedHands.Count > 0)
            {
                activeId = trackedHands.First();
            }
        }

        void OnDestroy()
        {                        
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;

            gestureRecognizer.Tapped -= GestureRecognizerTapped;
            gestureRecognizer.HoldStarted -= GestureRecognizer_HoldStarted;
            gestureRecognizer.HoldCompleted -= GestureRecognizer_HoldCompleted;
            gestureRecognizer.HoldCanceled -= GestureRecognizer_HoldCanceled;
            gestureRecognizer.StopCapturingGestures();
        }
    }
}