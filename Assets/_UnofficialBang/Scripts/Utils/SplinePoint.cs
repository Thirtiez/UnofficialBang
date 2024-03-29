﻿using SplineMesh;
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
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.LookRotation(Vector3.forward, curve.up);
            }
        }
    }
}
