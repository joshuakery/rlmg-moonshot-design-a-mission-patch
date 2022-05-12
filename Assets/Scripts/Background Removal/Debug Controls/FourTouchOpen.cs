using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ArtScan.FourTouch
{
    public class FourTouchOpen : MonoBehaviour
    {
        public List<int> clicks;
        public List<int> solution;
        public void OnClick(int n)
        {
            if (solution[clicks.Count] == n)
            {
                clicks.Add(n);

                if (clicks.Count == solution.Count)
                {
                    if (clicks.SequenceEqual(solution))
                    {
                        gameObject.SetActive(!gameObject.activeSelf);
                    }
                    clicks.Clear();
                }
            }
            else
            {
                clicks.Clear();
            }
        }

        private void Start()
        {
            clicks = new List<int>();
        }

    }
}

