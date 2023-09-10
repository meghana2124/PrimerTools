using System.Collections.Generic;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Scene
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        private Camera cameraCache;
        public Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;


        public float distance = 10;
        public Vector3 swivelOrigin;
        public Vector3 swivel;
        public bool faceSwivel = true;
        public Color backgroundColor = new(41 / 255f, 45 / 255f, 47 / 255f);


        public float ActualDistance => (transform.position - swivelOrigin).magnitude;


        private void Update() => Validate();
        private void OnValidate() => Validate();
        private void OnDrawGizmos() => Gizmos.DrawSphere(swivelOrigin, 0.1f);

        private void Awake()
        {
            if (cam != null && backgroundColor != cam.backgroundColor) {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
            }
        }

        // TODO: using properties with getter / setter we can get rid of the old* variables
        private Vector3? oldSwivelOrigin;
        private Vector3? oldSwivel;
        private bool? lastFaceSwivel;
        private void Validate()
        {
            var isDistanceWrong = Mathf.Abs(ActualDistance - distance) > 0.1;
            var isPositionOutdated = lastFaceSwivel != faceSwivel || oldSwivelOrigin != swivelOrigin || isDistanceWrong;

            if (isPositionOutdated || oldSwivel != swivel) {
                UpdateSwivel();
                lastFaceSwivel = faceSwivel;
                oldSwivelOrigin = swivelOrigin;
                oldSwivel = swivel;
            }
        }

        private void UpdateSwivel()
        {
            var direction = faceSwivel ? Vector3.back : Vector3.forward;
            transform.position = Quaternion.Euler(swivel) * direction * distance + swivelOrigin;

            transform.LookAt(swivelOrigin);

            // handle rotation in the axis the camera is pointing at as LookAt can't do this
            if (swivel.z != 0) {
                transform.Rotate(0, 0, swivel.z);
            }
        }

        public Tween FocusOn(Component target, Vector3 offset, float? distance = null, Vector3? swivel = null)
        {
            return Travel(distance, target.transform.position + offset, swivel);
        }

        public Tween Travel(float? distance = null, Vector3? swivelOrigin = null, Vector3? swivel = null)
        {
            var tween = new List<Tween>();
            var linear = LinearEasing.instance;

            if (distance.HasValue)
            {
                tween.Add(Tween.Value(
                        v => this.distance = v,
                        () => this.distance,
                        () => distance.Value
                    ) with
                    {
                        easing = linear
                    });
            }

            if (swivelOrigin.HasValue) {
                {
                    tween.Add(Tween.Value(
                            v => this.swivelOrigin = v,
                            () => this.swivelOrigin,
                            () => swivelOrigin.Value
                        ) with
                        {
                            easing = linear
                        });
                }
            }

            if (swivel.HasValue) {
                {
                    tween.Add(Tween.Value(
                            v => this.swivel = v,
                            () => this.swivel,
                            () => swivel.Value
                        ) with
                        {
                            easing = linear
                        });
                }
            }

            // or use tween.RunInBatch() to merge all tweens into one with unified easing
            return tween.RunInParallel() with { easing = IEasing.defaultMethod };
        }

        [PropertySpace]
        [Button(ButtonSizes.Large)]
        private void CopyCode()
        {
            GUIUtility.systemCopyBuffer = $@"
.Travel(
    distance: {distance}f,
    swivelOrigin: {swivelOrigin.ToCode()},
    swivel: {swivel.ToCode()}
)
            ".Trim();
        }
    }
}

