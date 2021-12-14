namespace ImageRecognition.Frontend.Models
{
    public class Coordinate
    {
        /// <summary>
        ///     Degree
        /// </summary>
        public decimal D { get; set; }

        /// <summary>
        ///     Minute
        /// </summary>
        public decimal M { get; set; }

        /// <summary>
        ///     Second
        /// </summary>
        public decimal S { get; set; }

        /// <summary>
        ///     Direction
        /// </summary>
        public string Direction { get; set; }
    }
}