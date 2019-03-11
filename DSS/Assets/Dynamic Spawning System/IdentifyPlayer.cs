using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

namespace DDS
{
    /// <summary>
    /// The Options which we can get the Player Object with
    /// </summary>
    [System.Serializable]
    public enum IdentifyPlayer
    {
        byTag, byName, byField
    }

    /// <summary>
    /// Stores the data which we use for the Identification of the Player Object
    /// </summary>
    [System.Serializable]
    public struct Identification
    {
        public string tag;
        public int Tag;
        public string Name;
        public GameObject Object;
    }
}
