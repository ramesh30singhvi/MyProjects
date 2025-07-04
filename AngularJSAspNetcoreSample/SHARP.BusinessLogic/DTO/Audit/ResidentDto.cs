using System;
namespace SHARP.BusinessLogic.DTO.Audit
{
	public class ResidentDto: IEquatable<ResidentDto>
	{
		public string ResidentName { get; set; }
		public string Room { get; set; }

		public ResidentDto(string ResidentName, string Room)
		{
			this.ResidentName = ResidentName;
			this.Room = Room;
		}

		public bool Equals(ResidentDto other)
		{
			if (other == null) return false;
			return ResidentName == other.ResidentName;
        }

        public override int GetHashCode()
        {
            return ResidentName.GetHashCode();
        }
    }
}

