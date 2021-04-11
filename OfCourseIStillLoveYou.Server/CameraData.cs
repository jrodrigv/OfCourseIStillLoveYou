namespace OfCourseIStillLoveYou.Server
{
    public class CameraData
    {
        public string CameraId { get; set; }
        public string CameraName { get; set; }

        public string Speed { get; set; }

        public string Altitude { get; set; }

        public byte[] Texture { get; set; }

        public override string ToString()
        {
            return CameraName + CameraId;
        }

        public override bool Equals(object obj)
        {
            return ((CameraData)obj).CameraId.Equals(CameraId);
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}
