// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
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
        private HashSet<uint> trackedHands = new HashSet<uint>();
        private Dictionary<uint, GameObject> trackingObject = new Dictionary<uint, GameObject>();

        void Awake()
        {
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
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
                if (trackingObject.ContainsKey(args.state.source.id))
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
        }

        void OnDestroy()
        {            
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        }
    }
}