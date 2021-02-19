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

        protected void Awake()
        {
            if (spline != null)
            {
                SetPosition(time);
            }
        }

#if UNITY_EDITOR

        protected void Update()
        {
            SetPosition(time);
        }

#endif

        public void SetSpline(Spline spline)
        {
            this.spline = spline;
        }

        public void SetPosition(float time)
        {
            this.time = time;

            var curve = spline.GetSample(time);
            transform.localPosition = curve.location;
            transform.up = curve.up;

        }
    }
}
