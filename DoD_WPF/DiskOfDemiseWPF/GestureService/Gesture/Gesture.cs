using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using DiskOfDemiseWPF.Gesture.Parts;
using DiskOfDemiseWPF.EventArguments;

namespace DiskOfDemiseWPF.Gesture
{
    class Gesture
    {
        /// <summary>
        /// The parts that compose one gesture
        /// </summary>
        private GestureSegment[] gestureParts;

        /// <summary>
        /// Current gesture part being matched to skeleton
        /// </summary>
        private int currentGesturePart = 0;

        /// <summary>
        /// Number of frames allowed to pass during a pause
        /// </summary>
        private int pauseFrameCount = 10;

        /// <summary>
        /// The current frame
        /// </summary>
        private int currentFrame = 0;

        /// <summary>
        /// true when paused
        /// </summary>
        private bool paused = false;

        /// <summary>
        /// The type of this gesture instance
        /// </summary>
        ///
        private string gestureType;

        /// <summary>
        /// Constructor for Gesture class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="gestureParts"></param>
        public Gesture(string gestureType, GestureSegment[] gestureParts)
        {
            this.gestureType = gestureType;
            this.gestureParts = gestureParts;
        }

        /// <summary>
        /// dispatched when a gesture is recognized
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Updates the gesture.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateGesture(Skeleton skeleton)
        {
            if (this.paused)
            {
                if (this.currentFrame >= this.pauseFrameCount)
                {
                    this.paused = false;
                }
                this.currentFrame++;
            }
            GesturePartResult result = this.gestureParts[this.currentGesturePart].CheckGesture(skeleton);
            /// if the skeleton data matches the gesture part...
            if (result == GesturePartResult.Succeed)
            {
                if (this.currentGesturePart + 1 < this.gestureParts.Length)
                {
                    this.currentGesturePart++;
                    this.currentFrame = 0;
                    this.pauseFrameCount = 10;
                    this.paused = true;
                }
                else
                {
                    if (this.GestureRecognized != null)
                    {
                        this.GestureRecognized(this, new GestureEventArgs(this.gestureType, skeleton.TrackingId));
                        this.Reset();
                    }
                }
            }
            /// if the gesture part doesn't match, or if the pause times out...
            else if (result == GesturePartResult.Fail || this.currentFrame == 50)
            {
                Reset();
            }
            else
            {
                this.currentFrame++;
                this.pauseFrameCount = 5;
                this.paused = true;
            }
        }
        
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this.currentGesturePart = 0;
            this.currentFrame = 0;
            this.pauseFrameCount = 5;
            this.paused = true;
        }
    }
}
