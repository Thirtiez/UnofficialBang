using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    [ExecuteInEditMode]
    public class SplinePoint : MonoBehaviour
    {
        [SerializeField]
        private Spline spline;

        [SerializeField]
        [Range(0, 1)]
        private float time;

        [SerializeField]
        private bool applyRotation = false;

        [SerializeField]
        private bool applyUp = false;

        protected void Awake()
        {
            SetTransform(time);
        }

#if UNITY_EDITOR

        protected void Update()
        {
            SetTransform(time);
        }

#endif

        public void Configure(Spline spline)
        {
            this.spline = spline;
        }

        public void SetTransform(float time)
        {
            if (spline != null)
            {
                this.time = time;

                var curve = spline.GetSample(time);
                transform.localPosition = curve.location;

                if (applyRotation)
                {
                    transform.localRotation = curve.Rotation;
                }

                if (applyUp)
                {
                    transform.up = curve.up;
                }
            }
        }
    }
}
