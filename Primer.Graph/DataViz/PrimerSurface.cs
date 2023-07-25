using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [RequireComponent(typeof(GraphDomain))]
    public class PrimerSurface : MonoBehaviour, IDisposable
    {
        private IGrid renderedGrid = new DiscreteGrid(0);
        private IGrid incomingGrid = null;

        private MeshFilter meshFilterCache;
        private MeshFilter meshFilter => transform.GetOrAddComponent(ref meshFilterCache);

        private MeshRenderer meshRendererCache;
        private MeshRenderer meshRenderer => transform.GetOrAddComponent(ref meshRendererCache);

        private GraphDomain domainCache;
        private GraphDomain domain => transform.GetOrAddComponent(ref domainCache);

        #region public bool invertTriangles;
        [SerializeField, HideInInspector]
        private bool _invertTriangles;

        [ShowInInspector]
        public bool invertTriangles {
            get => _invertTriangles;
            set {
                _invertTriangles = value;
                Render();
            }
        }
        #endregion

        [ShowInInspector]
        public Vector2Int resolution {
            get => renderedGrid.resolution;
            set => Render(renderedGrid.ChangeResolution(value));
        }

        public void SetData(float[,] data)
        {
            var cols = data.GetLength(0) - 1;
            var rows = data.GetLength(1) - 1;
            incomingGrid = new DiscreteGrid(cols, rows);

            // create x,y,z points from data
            for (var x = 0; x <= cols; x++) {
                for (var z = 0; z <= rows; z++) {
                    incomingGrid.points[x, z] = new Vector3(x, data[x, z], z);
                }
            }
        }

        public void SetData(IEnumerable<IEnumerable<Vector3>> data)
        {
            var array = data.Select(x => x.ToArray()).ToArray();

            if (array.Select(x => x.Length).Distinct().Count() != 1)
                throw new Exception("All rows must have the same length");

            var points = new Vector3[array.Length, array[0].Length];

            for (var x = 0; x < array.Length; x++) {
                for (var y = 0; y < array[0].Length; y++) {
                    points[x, y] = array[x][y];
                }
            }

            incomingGrid = new DiscreteGrid(points);
        }

        public void SetData(Vector3[,] data)
        {
            incomingGrid = new DiscreteGrid(data);
        }

        public void SetFunction(Func<float, float, float> function, Vector2Int? resolution = null, Vector2? start = null, Vector2? end = null)
        {
            var current = GetFunctionLineParams();

            incomingGrid = new FunctionGrid(function) {
                resolution = resolution ?? current.resolution,
                start = start ?? current.start,
                end = end ?? current.end,
            };
        }

        public void SetFunction(Func<Vector2, Vector3> function, int? resolution = null)
        {
            throw new NotImplementedException();
        }

        public Tween Transition()
        {
            if (incomingGrid is null)
                return Tween.noop;

            var targetGrid = incomingGrid;
            var (from, to) = IGrid.SameResolution(renderedGrid, targetGrid);

            incomingGrid = null;

            return new Tween(
                t => Render(IGrid.Lerp(from, to, t))
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of IGrid.Lerp() which is always a DiscreteGrid.
                onComplete: () => Render(targetGrid)
            );
        }

        public Tween GrowFromStart()
        {
            var targetGrid = incomingGrid ?? renderedGrid;
            var resolution = targetGrid.resolution;

            return new Tween(
                t => {
                    var cutLength = new Vector2(resolution.x * t, resolution.y * t);
                    Render(targetGrid.SmoothCut(cutLength, fromOrigin: false));
                }
            ).Observe(
                // After the transition is complete, we ensure we store the line we got
                // instead of the result of SmoothCut() which is always a DiscreteGrid.
                onComplete: () => Render(targetGrid)
            );
        }

        public Tween ShrinkToEnd()
        {
            var targetGrid = renderedGrid;
            var res = targetGrid.resolution;

            return new Tween(
                t => {
                    var cutLength = new Vector2(res.x * (1 - t), res.y * (1 - t));
                    Render(targetGrid.SmoothCut(cutLength, fromOrigin: true));
                }
            );
        }

        public void Dispose()
        {
            new Container(transform).Dispose();
        }

        private FunctionGrid GetFunctionLineParams()
        {
            if (incomingGrid is FunctionGrid incoming)
                return incoming;

            if (renderedGrid is FunctionGrid rendered)
                return rendered;

            return FunctionGrid.Default();
        }

        private void Render() => Render(renderedGrid);
        private void Render(IGrid grid)
        {
            if (grid is null)
                return;

            meshFilter.mesh = new Mesh {
                vertices = DefinePoints(grid),
                triangles = DefineTriangles(grid.resolution + Vector2Int.one),
            };

            renderedGrid = grid;
        }

        private static Vector3[] DefinePoints(IGrid grid)
        {
            var points2D = grid.points;
            var colCount = points2D.GetLength(0);
            var rowCount = points2D.GetLength(1);
            var points = new Vector3[rowCount * colCount];

            for (var y = 0; y < rowCount; y++) {
                for (var x = 0; x < colCount; x++) {
                    var p = points2D[x, y];
                    points[y * colCount + x] = new Vector3(p.x, p.y, -p.z);
                }
            }

            return points;
        }

        private int[] DefineTriangles(Vector2Int size) {
            var triangles = new List<int>();

            // setting each square's triangles
            for (var x = 0; x < size.x - 1; x++) {
                for (var y = 0; y < size.y - 1; y++) {
                    var topLeft = y * size.x + x;
                    var topRight = y * size.x + x + 1;
                    var bottomLeft = (y + 1) * size.x + x;
                    var bottomRight = (y + 1) * size.x + x + 1;

                    if (invertTriangles) {
                        triangles.AddTriangle(topRight, topLeft, bottomLeft);
                        triangles.AddTriangle(topRight, bottomRight, bottomLeft);
                    }
                    else {
                        triangles.AddTriangle(topLeft, topRight, bottomRight);
                        triangles.AddTriangle(topLeft, bottomLeft, bottomRight);
                    }
                }
            }

            return triangles.ToArray();
        }
    }
}
