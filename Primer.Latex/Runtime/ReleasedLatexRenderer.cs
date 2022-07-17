using System.Collections.Generic;
using UnityEngine;

namespace LatexRenderer
{
    // Using a container like this prevents the ReleasedLatexRenderer from being created by the
    // editor user (ie: it won't appear in any menus or searches).
    public static class ReleasedLatexRendererContainer
    {
        public class ReleasedLatexRenderer : MonoBehaviour
        {
            [SerializeField] [HideInInspector] private List<string> _headers = new();

            [SerializeField] [HideInInspector] private string _latex = "";

            public IReadOnlyList<string> Headers => _headers;

            public string Latex => _latex;

            internal void SetLatex(string latex, List<string> headers)
            {
                _latex = latex;
                _headers = headers;
            }
        }
    }
}