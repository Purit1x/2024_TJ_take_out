using System.ComponentModel.DataAnnotations;

namespace takeout_tj.DTO
{
    public class StationDBDto
    {
        public int StationId { get; set; }

        public string StationName { get; set; }

        public string StationAddress { get; set; }
    }
}
