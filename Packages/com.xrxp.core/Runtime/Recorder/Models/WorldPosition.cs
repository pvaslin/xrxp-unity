using UnityEngine;

namespace XRXP.Recorder.Models
{
    public class WorldPosition : Jsonable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public WorldPosition(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
